using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public sealed partial class DynamicLayoutPanel : ZUserControl, IControlHost, IExtendedControl, IDynamicLayoutPanel
	{
		public DynamicLayoutPanel()
		{
			InitializeComponent();
			controlDependencyObserver = new ControlDependencyObserver(this);
		}

		readonly HashSet<IControlBag> controlBags = new HashSet<IControlBag>();
		readonly Dictionary<ControlReference, Control> controlsByReference = new Dictionary<ControlReference, Control>();
		readonly Dictionary<ControlReference, ControlExtension> controlExtensions = new Dictionary<ControlReference, ControlExtension>();
		readonly List<Tuple<ZPropertyInfo, ControlExtension>> captionDependencies = new List<Tuple<ZPropertyInfo, ControlExtension>>();
		readonly List<int> columnSeparatorPositions = new List<int>();
		readonly List<ZPanel> columnSeparators = new List<ZPanel>();
		readonly ControlDependencyObserver controlDependencyObserver;
		readonly List<ILayoutExtension> layoutExtensions = new List<ILayoutExtension>();

		PanelLayout currentLayout = new PanelLayout();
		BusinessObject currentDataItem;

		int contentHeight;

		protected override void Dispose(bool disposing)
		{
			Extensions.Dispose();
			if (disposing)
			{
				UnhookEventHandlers();
				CleanupExtensionHooksAndData();
			}

			base.Dispose(disposing);
		}

		public void UpdateLayout(IPanelLayoutProvider provider)
		{
			UpdateLayoutExtensions(provider);
			UpdateLayout(provider?.Layout);
		}

		internal void UpdateLayout(PanelLayout layout)
		{
			if (layout != currentLayout)
			{
				UnhookEventHandlers();
				currentLayout = layout ?? new PanelLayout();
				HookEventHandlers();
				UpdateLayout();
				InitializeLayoutExtensions();
			}
		}

		bool layoutUpdating;
		bool layoutUpdatePending;
		const int maxIterations = 10;

		public Control Host => this;

		public IControlExtensionCollection Extensions => extensions ?? (extensions = new ControlExtensionCollection(this));
		IControlExtensionCollection extensions;

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This method works with actual (scaled) pixel sizes.")]
		void UpdateLayout()
		{
			layoutUpdatePending = true;

			if (layoutUpdating)
			{
				return;
			}

			layoutUpdating = true;
			try
			{
				// State of business object can change during layout update due to lazy loading causing another layout update.
				// Normally, there should be no more than 2 iterations here.
				// But if business object keeps changing due to some error we will stop after a certain number of iterations.
				for (int updateCounter = 0; layoutUpdatePending && updateCounter < maxIterations; updateCounter++)
				{
					if (updateCounter != 0)
					{
						// todo: test
					}
					layoutUpdatePending = false;
					UpdateLayoutInternal();
				}
#if DEBUG
				if (layoutUpdatePending)
				{
					throw new DeveloperNotificationException(FormattableString.Invariant($"Layout still requires update after {maxIterations} iterations."));
				}
#endif
			}
			finally
			{
				layoutUpdating = false;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This method works with actual (scaled) pixel sizes.")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void UpdateLayoutInternal()
		{
			SuspendLayout();
			try
			{
				foreach (var controlBag in currentLayout.ControlBags)
				{
					if (controlBags.Add(controlBag))
					{
						controlBag.CreateControls(this, controlsByReference);
					}
				}

				foreach (var pair in controlsByReference)
				{
					var controlReference = pair.Key;
					var control = pair.Value;

					if (control.Parent == this && !currentLayout.ShouldInclude(controlReference))
					{
						GetControlExtension(controlReference).RemoveControl(this, control);
					}
				}

				int singleRowHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(24);
				int xMargin = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);

				int nextTabIndex = 1;
				int nextColumnX = Padding.Left;
				contentHeight = 0;
				columnSeparatorPositions.Clear();

				int minScrollWidth = 0;
				int minScrollHeight = 0;

				foreach (var column in currentLayout.Columns)
				{
					int nextY = Padding.Top;
					int columnX = nextColumnX;
					foreach (var row in column.Rows)
					{
						int rowHeight = 0;
						foreach (var part in row.Parts)
						{
							if (part is ControlReference controlReference)
							{
								var control = GetControl(controlReference);
								if (control.Parent != this)
								{
									GetControlExtension(controlReference).AddControl(this, control);
								}

								if (!currentLayout.CollapseEmptyRows || currentLayout.IsVisible(controlReference, currentDataItem))
								{
									rowHeight = Math.Max(rowHeight, control.Height);
								}
							}
						}

						if (rowHeight != 0 && row.AlignToControl != null)
						{
							var alignToControl = GetControl(row.AlignToControl);
							if (alignToControl.Parent == this && alignToControl.Visible)
							{
								switch (row.VerticalAlignment)
								{
									case System.Windows.Forms.VisualStyles.VerticalAlignment.Top:
										nextY = Math.Max(nextY, alignToControl.Top);
										break;
									case System.Windows.Forms.VisualStyles.VerticalAlignment.Bottom:
										nextY = Math.Max(nextY, alignToControl.Bottom - rowHeight);
										break;
									case System.Windows.Forms.VisualStyles.VerticalAlignment.Center:
										nextY = Math.Max(nextY, (alignToControl.Top + alignToControl.Bottom - rowHeight) / 2);
										break;
									default:
										nextY = Math.Max(nextY, alignToControl.Top);
										break;
								}
							}
						}

						int nextX = columnX;
						Control previousControl = null;
						foreach (var part in row.Parts)
						{
							if (part is ControlReference controlReference)
							{
								var control = GetControl(controlReference);

								control.TabIndex = nextTabIndex++;
								control.Top = nextY + (rowHeight - control.Height) / 2;

								if (control is ZCheckBox checkBox && checkBox.CheckAlign == ZContentAlignment.Right)
								{
									control.Left = nextX - control.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(14);
								}
								else
								{
									control.Left = nextX;
								}

								nextX = control.Right + xMargin;

								UpdateCaption(controlReference);
								UpdateControlBehaviour(controlReference);
								RefreshControlNotificationIcons(control);

								previousControl = control;

								minScrollWidth = Math.Max(minScrollWidth, nextX);
								minScrollHeight = Math.Max(minScrollHeight, nextY);
							}
							else if (part is PanelLayoutRuler ruler)
							{
								nextX = columnX + ControlDpiScalingHelper.ScaleToCurrentDpiX(ruler.Position);
							}
							else if (part is PanelLayoutRightRuler rightRuler)
							{
								if (previousControl != null)
								{
									ExtendControlRight(previousControl, columnX, rightRuler.Position);
									nextX = previousControl.Right + xMargin;
								}
							}
							else
							{
								// todo: do not throw exception in release mode
								throw new InvalidOperationException(FormattableString.Invariant($"Unexpected type of layout part: {part.GetType().Name} {{Name = {part.Name}}}"));
							}
						}

						nextColumnX = Math.Max(nextX, nextColumnX);
						if (rowHeight != 0)
						{
							if (rowHeight >= singleRowHeight)
							{
								nextY += rowHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(4);
							}
							else
							{
								nextY += (rowHeight / singleRowHeight + 1) * singleRowHeight;
							}
						}
					}

					contentHeight = Math.Max(contentHeight, nextY);
					columnSeparatorPositions.Add(nextColumnX);
					nextColumnX++;
				}

				if (AutoScroll)
				{
					AutoScrollMinSize = new Size(minScrollWidth, minScrollHeight);
				}

				UpdateColumnSeparators();

				if (currentLayout.TabSequence == PanelLayoutTabSequence.RowWise)
				{
					UpdateTabOrderRowWise();
				}

				CorrectBindToOrgListForAddressControls(DataMember);
			}
			finally
			{
				ResumeLayout();
			}
		}

		void RefreshControlNotificationIcons(Control control)
		{
			if (control is null || control.IsDisposed)
			{
				return;
			}

			if (control.Visible
				&& control is IExtendedControl extendedControl
				&& extendedControl.Extensions is IControlExtensionCollection extensions
				&& extensions.Supports<IValidationExtension>()
				&& extensions.Get<INotificationExtension>() is INotificationExtension notificationExtension)
			{
				notificationExtension.Redraw();
			}

			foreach (Control innerControl in control.Controls)
			{
				RefreshControlNotificationIcons(innerControl);
			}
		}

		void UpdateLayoutExtensions(IPanelLayoutProvider provider)
		{
			CleanupExtensionHooksAndData();
			layoutExtensions.Clear();
			if (provider is IPanelLayoutProviderWithExtensions layoutProviderWithExtensions && layoutProviderWithExtensions.Extensions != null)
			{
				layoutExtensions.AddRange(layoutProviderWithExtensions.Extensions);
			}
		}

		void UpdateTabOrderRowWise()
		{
			int tabIndex = 1;
			foreach (var c in Controls.Cast<Control>().OrderBy(c => c.Top).ThenBy(c => c.Left))
			{
				c.TabIndex = tabIndex++;
			}
		}

		void UpdateControlBehaviour(ControlReference controlReference)
		{
			var control = GetControl(controlReference);
			var controlBehaviours = currentLayout.GetControlBehaviours(controlReference).GetAll();
			foreach (var controlBehaviourContainer in controlBehaviours)
			{
				controlBehaviourContainer.UpdateControlBehaviour(control, currentDataItem);
			}

			if (!currentLayout.HasVisibilityBehaviour(controlReference) && !control.Visible)
			{
				control.Visible = true;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This method works with actual (scaled) pixel sizes.")]
		void ExtendControlRight(Control control, int columnX, int rulerPosition)
		{
			var position = columnX + ControlDpiScalingHelper.ScaleToCurrentDpiX(rulerPosition);
			if (!AutoScroll && !AutoSize && DisplayRectangle.Width > 0)
			{
				position = Math.Min(position, DisplayRectangle.Width);
			}

			int newWidth = position - control.Left;
			if (newWidth < 0)
			{
				ErrorReporter.ReportOnce("4F760FC6-40E4-45B7-98B0-60F96922FE3A" + control.Name, $@"The new width({newWidth}) of {control.Name} can't be negative.
Original width: {control.Width} Parent:{control.Parent.Name} width:{control.Parent.Width}
(columnX:{columnX} rulerPosition:{rulerPosition} control.Left:{control.Left} AutoScroll:{AutoScroll} DisplayRectangle.Width:{DisplayRectangle.Width})
This can be caused when form is not big enough to show the control fully. Either set the {control.Parent.Name}.AutoScroll in control {control.Parent.Parent?.Name} to true or ensure that the minimum form size can show the layout.");
				newWidth = control.Width;
			}

			if (control is ZCodeFindBox codeFindBox && codeFindBox.CodeBox != null && !codeFindBox.ShowDescriptionBox)
			{
				int widthAdjustment = newWidth - codeFindBox.Width;
				codeFindBox.CodeBox.Width += widthAdjustment;
			}

			ControlDpiScalingHelper.SetWidth(control, newWidth, false);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This method works with actual (scaled) pixel sizes.")]
		[SuppressMessage("CargoWiseOne", "CW1042", Justification = "This method works with actual (scaled) pixel sizes.")]
		void UpdateColumnSeparators()
		{
			int separatorCount = Math.Max(columnSeparatorPositions.Count - 1, 0);
			while (columnSeparators.Count > separatorCount)
			{
				int lastSeparatorIndex = columnSeparators.Count - 1;
				columnSeparators[lastSeparatorIndex].Dispose();
				columnSeparators.RemoveAt(lastSeparatorIndex);
			}

			for (int col = 0; col < separatorCount; col++)
			{
				ZPanel separator;
				if (col < columnSeparators.Count)
				{
					separator = columnSeparators[col];
				}
				else
				{
					separator = new ZPanel();
					separator.Top = 0;
					separator.BackColor = Color.Transparent;
					separator.Name = FormattableString.Invariant($"ColumnSeparator{col}");

					Controls.Add(separator);
					columnSeparators.Add(separator);
				}

				separator.Left = columnSeparatorPositions[col];
				separator.Width = 1;
				separator.Height = contentHeight;
			}
		}

		Control GetControl(ControlReference controlReference)
		{
			if (!controlsByReference.TryGetValue(controlReference, out var control))
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Control bag did not provide control '{controlReference}'."));
			}

			return control;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			var dataItem = CurrentDataItem as BusinessObject;
			if (dataItem != currentDataItem)
			{
				UnhookEventHandlers();
				CleanupExtensionHooksAndData();
				currentDataItem = dataItem;
				HookEventHandlers();
				UpdateLayout();
				InitializeLayoutExtensions();
			}
		}

		void CleanupExtensionHooksAndData()
		{
			layoutExtensions.ForEach(l => l.Cleanup());
		}

		void InitializeLayoutExtensions()
		{
			layoutExtensions.ForEach(l => l.Initialize(this));
		}

		void HookEventHandlers()
		{
			if (currentDataItem == null || currentDataItem.IsDeleted)
			{
				return;
			}

			foreach (var controlReference in currentLayout.IncludedControls)
			{
				var controlStatesContainerCollection = currentLayout.GetControlBehaviours(controlReference);
				controlDependencyObserver.ConfigureControlBehaviourObservers(controlReference, controlStatesContainerCollection);

				foreach (var property in currentLayout.GetCaptionDependencies(controlReference, currentDataItem))
				{
					var controlExtension = GetControlExtension(controlReference);
					property.ValueChanged += controlExtension.UpdateCaption;
					captionDependencies.Add(new Tuple<ZPropertyInfo, ControlExtension>(property, controlExtension));
				}
			}
		}

		void UnhookEventHandlers()
		{
			controlDependencyObserver.UnsubscribeObservers();
			foreach (var dependencies in captionDependencies)
			{
				dependencies.Item1.ValueChanged -= dependencies.Item2.UpdateCaption;
			}
			captionDependencies.Clear();
		}

		ControlExtension GetControlExtension(ControlReference controlReference)
		{
			if (!controlExtensions.TryGetValue(controlReference, out var controlExtension))
			{
				controlExtension = new ControlExtension(this, controlReference);
				controlExtensions.Add(controlReference, controlExtension);
			}

			return controlExtension;
		}

		static void UpdateCaption(IResCaptionedControl resCaptionedControl, ResourceStringData captionData)
		{
			resCaptionedControl.CaptionResourceString = captionData;
			(resCaptionedControl as Control)?.UpdateCaption(); // Need to call this as CaptionRenderingSupport.RefreshCaptionLabel() only call run in DesignModeFinder.IsDesigning is true
		}

		void UpdateCaption(ControlReference controlReference)
		{
			var control = GetControl(controlReference);
			var controlExtension = GetControlExtension(controlReference);
			if (currentLayout.TryGetCaptionData(controlReference, currentDataItem, out var captionData))
			{
				var controlCaptionDictionary = GetControlMapping(control, captionData);
				if (!controlExtension.OriginalResourceStringDataSaved)
				{
					controlExtension.SaveOriginalCaption(controlCaptionDictionary.Keys.Select(x => (x, x.CaptionResourceString)));
				}

				controlCaptionDictionary.ForEach(x => UpdateCaption(x.Key, x.Value));
			}
			else if (controlExtension.OriginalResourceStringDataSaved)
			{
				controlExtension.OriginalResourceStringData.ForEach(x => UpdateCaption(x.control, x.originalCaption));
			}
			else if (control is IExtendedControl extendedControl)
			{
				var labelCaptionRenderer = extendedControl.Extensions.Get<ILabelCaptionRenderer>();
				labelCaptionRenderer?.Refresh();
			}
		}

		IDictionary<IResCaptionedControl, ResourceStringData> GetControlMapping(Control control, IDictionary<string, ResourceStringData> captionData)
		{
			var complexControlMapping = new Dictionary<IResCaptionedControl, ResourceStringData>();
			GatherControlMapping(complexControlMapping, control, captionData);
			return complexControlMapping;
		}

		void GatherControlMapping(Dictionary<IResCaptionedControl, ResourceStringData> complexControlMapping, Control control, IDictionary<string, ResourceStringData> controlNameToCaptionMapping)
		{
			var name = control.Name;
			if (control is IResCaptionedControl resCaptionedControl && controlNameToCaptionMapping.TryGetValue(name, out var caption))
			{
				complexControlMapping.Add(resCaptionedControl, caption);
			}
			foreach (Control childControl in control.Controls)
			{
				GatherControlMapping(complexControlMapping, childControl, controlNameToCaptionMapping);
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				foreach (var controlReference in currentLayout.IncludedControls)
				{
					var control = GetControl(controlReference);
					control.Visible = currentLayout.IsVisible(controlReference, currentDataItem);
				}
				UpdateLayout();
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				var bindingManager = BindingContext[dataSource, dataMember];
				if (ListBindingHelper.GetListItemType((bindingManager as CurrencyManager).List) is Type bindType)
				{
					DataSourceType = bindType;
				}

				bindToOrgListForAddressControlsCorrected = false;
				CorrectBindToOrgListForAddressControls(dataMember);
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		bool bindToOrgListForAddressControlsCorrected;

		void CorrectBindToOrgListForAddressControls(string prefix)
		{
			if (!string.IsNullOrEmpty(prefix) && !bindToOrgListForAddressControlsCorrected)
			{
				foreach (var control in currentLayout.IncludedControls.Select(GetControl))
				{
					CorrectBindToOrgListForAddressControls(control, prefix + ".");
				}

				bindToOrgListForAddressControlsCorrected = currentLayout.IncludedControls.Count > 0;
			}
		}

		void CorrectBindToOrgListForAddressControls(Control control, string prefix)
		{
			if (control is IDynamicLayoutAddressControl dynamicLayoutAddressControl)
			{
				dynamicLayoutAddressControl.CorrectBindToOrgList(prefix);
			}
			else
			{
				foreach (Control child in control.Controls)
				{
					CorrectBindToOrgListForAddressControls(child, prefix);
				}
			}
		}

		#region IControlHost

		Control IControlHost.GetControl(ControlReference controlReference) => GetControl(controlReference);

		BusinessObject IControlHost.GetCurrentDataItem() => currentDataItem;

		void IControlHost.UpdateLayout() => UpdateLayout();

		Control IControlHost.GetControlByName(string controlName) => GetControlByName(controlName);

		#endregion

		Control GetControlByName(string controlName)
		{
			Argument.NotNullOrEmpty(controlName, nameof(controlName));
			var controlReference = controlsByReference.Keys.SingleOrDefault(k => k.Name == controlName);
			return controlReference == null ? null : GetControl(controlReference);
		}

		class ControlExtension
		{
			readonly DynamicLayoutPanel layoutUpdater;
			readonly ControlReference controlReference;

			public ControlExtension(DynamicLayoutPanel layoutUpdater, ControlReference controlReference)
			{
				this.layoutUpdater = layoutUpdater;
				this.controlReference = controlReference;
			}

			public bool OriginalResourceStringDataSaved { get; private set; }

			public (IResCaptionedControl control, ResourceStringData originalCaption)[] OriginalResourceStringData { get; private set; }

			public string OriginalBindingMember { get; private set; }

			public void SaveOriginalCaption(IEnumerable<(IResCaptionedControl control, ResourceStringData originalCaption)> captionData)
			{
				OriginalResourceStringDataSaved = true;
				OriginalResourceStringData = captionData.ToArray();
			}

			public void UpdateCaption(object sender, EventArgs e)
			{
				layoutUpdater.UpdateCaption(controlReference);
			}

			public void RemoveControl(DynamicLayoutPanel layoutPanel, Control control)
			{
				string bindingMember = layoutPanel.BindingSource.GetBindingMember(control);
				layoutPanel.Controls.Remove(control);
				if (!string.IsNullOrEmpty(bindingMember))
				{
					layoutPanel.BindingSource.SetBindingMember(control, null);
					OriginalBindingMember = bindingMember;
				}
			}

			public void AddControl(DynamicLayoutPanel layoutPanel, Control control)
			{
				layoutPanel.Controls.Add(control);
				string bindingMember = layoutPanel.BindingSource.GetBindingMember(control);
				if (string.IsNullOrEmpty(bindingMember) && OriginalBindingMember != null)
				{
					layoutPanel.BindingSource.SetBindingMember(control, OriginalBindingMember);
				}
			}
		}

		#region IDynamicLayoutPanel

		void IDynamicLayoutPanel.ApplyBinding()
		{
			if (DataSource is null)
			{
				this.ForceBindingIncludingParents();
			}
		}

		#endregion
	}
}
