using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Macros;
using CargoWise.Macros.GUI;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#if WINZOR
using Enterprise.ZArchitecture;
#endif

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class PageView : ZPanel, IPageView
	{
		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public PageView(SizeF size, IPageViewPresenter presenter)
		{
			Argument.NotNull(presenter, nameof(presenter));

			this.presenter = presenter;
			this.presenter.Init(this);

			pageSize = size;
			SetPageSizeInPixels(size, Util.DefaultDpiX, Util.DefaultDpiY);

			Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10);

			BackColor = Color.White;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

			notificationTimer = new Timer();
			notificationTimer.Interval = notificationPopupDelay;
			notificationTimer.Tick += NotificationTimerOnTick;

			urlTimer = new Timer();
			urlTimer.Interval = urlPopupDelay;
			urlTimer.Tick += UrlTimerOnTick;

			toolTip = new ToolTip();
			toolTip.IsBalloon = true;
			toolTip.ShowAlways = true;
			toolTip.Active = false;
			toolTip.SetToolTip(this, "");

			contextMenu = new ContextMenu();

			MouseDown += OnMouseDown;
			MouseUp += OnMouseUp;
			MouseMove += OnMouseMove;
		}

		readonly IPageViewPresenter presenter;
		readonly SizeF pageSize;
		float currentScale = 1f;

		Timer notificationTimer;
		Timer urlTimer;
		ToolTip toolTip;
		ContextMenu contextMenu;

		const int notificationPopupDelay = 1000;
		const int urlPopupDelay = 100;

		Point mouseLocation;
		RectangleF lastShownBoundaries;
		string[] urls;

		public IPageViewPresenter Presenter
		{
			get { return presenter; }
		}

		void SetPageSizeInPixels(SizeF sizeInDisplayUnits, float dpiX, float dpiY)
		{
			ControlDpiScalingHelper.SetWidth(this, Util.ConvertToPixels(dpiX, sizeInDisplayUnits.Width * currentScale), true);
			ControlDpiScalingHelper.SetHeight(this, Util.ConvertToPixels(dpiY, sizeInDisplayUnits.Height * currentScale), true);
		}

		void IPageView.Refresh()
		{
			foreach (var layoutElement in Elements)
			{
#if !WINZOR
				layoutElement.Invalidate();
#else
				if (layoutElement is DynamicContentLayoutElement dynamicContent)
				{
					dynamicContent.ContentControl.Text = dynamicContent.Element.Content;
				}
#endif
			}

			base.Invalidate();
		}

#if WINZOR
		public void AddControls(ILayoutElement[] layoutElements)
		{
			foreach (var item in layoutElements)
			{
				if (item is TextLayoutElement textElem)
				{
					var element = textElem.Element;
					var control = new ZLabel();
					control.Bounds = System.Drawing.Rectangle.Round(textElem.Boundaries);
					control.Text = element.Content;
					control.Font = new System.Drawing.Font(element.Font.Name, element.Font.Size, element.Font.Style);
					control.AutoSize = false;
					control.TextAlign = ConvertAlignment(element.HAlignment, element.VAlignment);
					this.Controls.Add(control);
				}
				else if (item is DynamicContentLayoutElement dynamicContent)
				{
					var element = dynamicContent.Element;
					var control = new ZLabel();
					control.AutoSize = false;
					control.Bounds = System.Drawing.Rectangle.Round(dynamicContent.Boundaries);
					control.Text = element.Content;
					control.Font = new System.Drawing.Font(element.Font.Name, element.Font.Size, element.Font.Style);
					control.TextAlign = ConvertAlignment(element.HAlignment, element.VAlignment);
					control.Click += (object sender, EventArgs e) =>
					{
						this.Presenter.BeginEdit(dynamicContent, EditTrigger.User);
					};
					dynamicContent.ContentControl = control;
					this.Controls.Add(control);
					var editable = element.HasOverriddenData || element.EditableData.Any();
					if (editable)
					{
						var editableIndicator = new EditableIndicator(control);
						this.Controls.Add(editableIndicator);
					}
				}
				else if (item is DrawingLayoutElement drawingElement)
				{
					var element = drawingElement.Element;
					var control = new PictureBox();
					control.Bounds = System.Drawing.Rectangle.Round(drawingElement.Boundaries);
					control.Image = element.Image;
					this.Controls.Add(control);
				}
				else if (item is LineLayoutElement lineElement)
				{
					var boundaries = lineElement.Boundaries;
					var size = new SizeF(Math.Max(boundaries.Size.Width, 1), Math.Max(boundaries.Size.Height, 1));
					var control = new Control()
					{
						BackColor = lineElement.Element.Pen.Color,
						Size = Size.Round(size),
						Location = Point.Round(boundaries.Location)
					};
					control.AllowOverlap(this);
					this.Controls.Add(control);
				}
			}
		}

		ContentAlignment ConvertAlignment(Alignment hAlignment, Alignment vAlignment)
		{
			if (hAlignment == Alignment.Left)
			{
				if (vAlignment == Alignment.Top)
				{
					return ContentAlignment.TopLeft;
				}
				else if (vAlignment == Alignment.Center)
				{
					return ContentAlignment.MiddleLeft;
				}
				else
				{
					return ContentAlignment.BottomLeft;
				}
			}
			else if (hAlignment == Alignment.Right)
			{
				if (vAlignment == Alignment.Top)
				{
					return ContentAlignment.TopRight;
				}
				else if (vAlignment == Alignment.Center)
				{
					return ContentAlignment.MiddleRight;
				}
				else
				{
					return ContentAlignment.BottomRight;
				}
			}
			else
			{
				if (vAlignment == Alignment.Top)
				{
					return ContentAlignment.TopCenter;
				}
				else if (vAlignment == Alignment.Center)
				{
					return ContentAlignment.MiddleCenter;
				}
				else
				{
					return ContentAlignment.BottomCenter;
				}
			}
		}
#endif

		void IPageView.Scale(float scale)
		{
			currentScale = scale;

			SetPageSizeInPixels(pageSize, Util.DefaultDpiX, Util.DefaultDpiY);
			base.Invalidate();
		}

		#region Elements

		public IEnumerable<ILayoutElement> Elements
		{
			get { return elements ?? Enumerable.Empty<ILayoutElement>(); }
			set
			{
				if (elements != null)
				{
					foreach (var element in elements)
					{
						element.PageView = null;
					}
				}

				elements = value;

				if (elements != null)
				{
					foreach (var element in elements)
					{
						element.PageView = this;
					}
				}
			}
		}

		IEnumerable<ILayoutElement> elements;

		#endregion

		#region Mouse Click Handling

		void OnMouseDown(object sender, MouseEventArgs args)
		{
			mouseDownLocation = args.Location;

			if (args.Button != MouseButtons.Left)
			{
				return;
			}

			if (ModifierKeys.HasFlag(Keys.Control))
			{
				var focusedDynamicLayoutElement = FindDynamicContentAt(args.Location);
				ShowMacroEvaluator(focusedDynamicLayoutElement);
				return;
			}

#if !WINZOR

			const int F2KeyCode = 0x71;
			if (CargoWise.Windows.UI.MouseHook.Win32.IsKeyDown(F2KeyCode))
			{
				var focusedTextLayoutElement = FindTextElementsAtLocation(args.Location)
					.FirstOrDefault();

				ShowTranslationFeedbackForm(focusedTextLayoutElement);
				return;
			}

			void ShowTranslationFeedbackForm(ITextLayoutElement layoutElement)
			{
				if (!presenter.IsDocumentTranslatable)
				{
					var message = Res.GetString("8a246a44-f6b4-4dc6-9851-4f585eab99ef", "This document doesn't have translation enabled.");
					Globals.Message.Show(message);
					return;
				}

				var unquotedMacro = GetUnquotedMacro(layoutElement);

				if (string.IsNullOrWhiteSpace(unquotedMacro))
				{
					return;
				}

				if (ModifierKeys.HasFlag(Keys.Alt))
				{
					var caption = unquotedMacro.GetResStringCaption();
					var key = caption.GetResStringKey();

					var message = string.Format((NoResString)"Translation info:\r\nMacro (or caption): {0}\r\nResString.Key: {1}\r\nResString.Caption: {2}", unquotedMacro, key, caption);
					Globals.Message.Show(message);
					return;
				}

				TranslationFeedbackView.OpenFeedbackForm(this, unquotedMacro);
			}

			string GetUnquotedMacro(ITextLayoutElement layoutElement)
			{
				switch (layoutElement)
				{
					case DynamicContentLayoutElement dynamicContentLayoutElement:
						var macro = dynamicContentLayoutElement
							.Element
							?.MacroExpression
							?.Text;

						return !string.IsNullOrWhiteSpace(macro)
							? macro.Trim('\"')
							: null;

					default:
						return layoutElement?.Text?.Content;
				}
			}

#endif

			if (urls?.Length > 0)
			{
				OpenUrls(urls);
				return;
			}

			var content = FindDynamicContentAt(args.Location);
			presenter.BeginEdit(content, EditTrigger.User);
		}

		void OpenUrls(string[] urlsToRun)
		{
			foreach (var url in urlsToRun ?? Array.Empty<string>())
			{
				try
				{
					WebUrlLauncher.Launch(url);
				}
				catch (Exception exc) when (!exc.IsCriticalException())
				{
					// eat
				}
			}
		}

		Point mouseDownLocation;

		void OnMouseUp(object sender, MouseEventArgs args)
		{
			if (args.Button == MouseButtons.Right)
			{
				presenter.NotifyShowingContextMenu();
				ShowContextMenu(args.Location);
			}
		}

		void ShowContextMenu(Point location)
		{
			contextMenu.MenuItems.Clear();

			var showContexMenu = false;

			foreach (var handler in ContextMenuItemsHandlers)
			{
				var menuItem = handler();

				if (menuItem != null)
				{
					contextMenu.MenuItems.Add(menuItem);
					showContexMenu = true;
				}
			}

			if (showContexMenu)
			{
				contextMenu.Show(this, location);
			}
		}

		IEnumerable<Func<MenuItem>> ContextMenuItemsHandlers
		{
			get
			{
				yield return CreateShowMacroEvaluatorMenuItem;
				yield return CreateResetCellMenuItem;
			}
		}

		MenuItem CreateShowMacroEvaluatorMenuItem()
		{
			var layoutElement = FindDynamicContentAt(mouseDownLocation);

			if (layoutElement == null)
			{
				return null;
			}

			EventHandler menuClickHandler = (s, e) => ShowMacroEvaluator(layoutElement);

			return new ZMenuItem(Res.GetString("768a9523-5522-4226-b544-3bf61436adfb", "Show Macro Evaluator"), menuClickHandler);
		}

		MenuItem CreateResetCellMenuItem()
		{
			var element = FindTextElementsAtLocation(mouseDownLocation)
				.OfType<DynamicContentLayoutElement>()
				.FirstOrDefault(elem => elem.Element.HasOverriddenData);

			if (element == null)
			{
				return null;
			}

			EventHandler menuClickHandler = (s, e) => presenter.CancelOverride(element);

			return new ZMenuItem(Res.GetString("b1b62a29-cdd9-4c32-839e-8988a527c809", "Reset"), menuClickHandler);
		}

		#endregion

		#region Painting

		bool isDiagnosticsEnabled;

		protected override void OnPaint(PaintEventArgs e)
		{
			var canvas = new Canvas(e.Graphics, currentScale);

			foreach (var layout in Elements.Where(el => el.IsVisible).OrderBy(el => el.Element.ZOrder))
			{
				layout.Paint(canvas, isDiagnosticsEnabled);
			}
		}

		#endregion

		#region Editing

		void IPageView.ShowEditor(IEditorView editorView)
		{
			if (editorView == null)
			{
				return;
			}

			Invalidate(editorView.Content);

			var editorControl = (Control)editorView.Control;

			if (editorControl != null)
			{
				Controls.Add(editorControl);

#if !WINZOR

				HelpBarManager.AttachToParent(editorControl);

#endif

				var editorForm = editorControl.FindForm();

				if (editorForm != null)
				{
					editorForm.Focus();
				}

				editorControl.Select();
			}
		}

		#endregion

		#region Tooltip

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void NotificationTimerOnTick(object sender, EventArgs eventArgs)
		{
			var elementsAtLocation = FindTextElementsAtLocation(mouseLocation);

			var elementWithNotifications = elementsAtLocation
				.OfType<DynamicContentLayoutElement>()
				.FirstOrDefault(element => element.Element.HasNotifications());

			if (elementWithNotifications != null)
			{
				var notification = elementWithNotifications
					.Element
					.Notifications
					.OrderBy(n => n.Type)
					.First();

				toolTip.Active = true;

				toolTip.SetToolTip(this, notification.Message);
				toolTip.Show(notification.Message, this, mouseLocation);

				lastShownBoundaries = elementWithNotifications.Boundaries;
			}

			notificationTimer.Stop();
		}

		void UrlTimerOnTick(object sender, EventArgs eventArgs)
		{
			var elementsAtLocation = FindTextElementsAtLocation(mouseLocation);

			var elementWithUrl = elementsAtLocation
				.Select(t => new
				{
					Urls = t.Text.GetUrls(),
					Boundaries = t.Boundaries
				})
				.FirstOrDefault(t => t.Urls.Length > 0);

			if (elementWithUrl != null)
			{
				Cursor = Cursors.Hand;
				urls = elementWithUrl.Urls;
				lastShownBoundaries = elementWithUrl.Boundaries;
			}

			urlTimer.Stop();
		}

		void OnMouseMove(object sender, MouseEventArgs mouseEventArgs)
		{
			if (lastShownBoundaries.Contains(mouseEventArgs.Location))
			{
				return;
			}

			toolTip.Active = false;
			lastShownBoundaries = RectangleF.Empty;

			Cursor = Cursors.Default;
			urls = Array.Empty<string>();

			notificationTimer.Start();
			urlTimer.Start();
			mouseLocation = mouseEventArgs.Location;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				notificationTimer.Dispose();
				urlTimer.Dispose();
				toolTip.Dispose();
				contextMenu.Dispose();

				presenter.Dispose();

				notificationTimer = null;
				urlTimer = null;
				toolTip = null;
				contextMenu = null;
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Implementaion

		public void Invalidate(ILayoutElement element)
		{
			if (element != null)
			{
				element.Invalidate();

				var area = System.Drawing.Rectangle.Round(element.Boundaries);
				area.Inflate(ControlDpiScalingHelper.NewScaledSize(2, 2));

				Invalidate(area);
			}
		}

		void ShowMacroEvaluator(DynamicContentLayoutElement layoutElement)
		{
			if (layoutElement == null)
			{
				return;
			}

			var checkpoint = Env.Security.AllowToolsAccess;

			if (checkpoint.IsAllowed)
			{
				void VisualCuesSwitch(bool enable)
				{
					if (isDiagnosticsEnabled != enable)
					{
						isDiagnosticsEnabled = enable;
						Refresh();
					}
				}

				var diagnosticInfo = new LayoutElementDiagnostics(layoutElement, VisualCuesSwitch);

				var scope = new MacroScope(layoutElement.Element.Scope);
				scope.SetVariable((NoResString)"diagnostics", diagnosticInfo);

				var form = new MacroEvaluationForm(
					layoutElement.Element.MacroExpression?.Context,
					layoutElement.Element.MacroExpression?.Text,
					scope,
					false);

				ZFormModaliser.ShowDialogAndDispose(form);
			}
			else
			{
				checkpoint.ShowError();
			}
		}

		DynamicContentLayoutElement FindDynamicContentAt(Point location)
		{
			return Elements
				.Where(elem => elem.Element.ElementType == ElementType.DynamicContent)
				.Cast<DynamicContentLayoutElement>()
				.FirstOrDefault(elem => elem.Boundaries.Contains(location));
		}

		IEnumerable<ITextLayoutElement> FindTextElementsAtLocation(Point location)
		{
			return Elements
				.OfType<ITextLayoutElement>()
				.Where(element => element.Boundaries.Contains(location));
		}

		#endregion
	}
}
