using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.EConversation.GUI
{
	public partial class ConversationMessageUserControl : ZUserControl
	{
		public ConversationMessageUserControl()
		{
			InitializeComponent();
			RegisterHotkeys();
		}

		public ConversationMessageUserControl(IConversationMessage message, bool isClientSystem)
			: this()
		{
			this.Message = message;
			this.isClientSystem = isClientSystem;
			this.bodyTextBox.ContentsResized += (o, e) =>
			{
				this.LastNewRectangle = e.NewRectangle;
#if WINZOR
				if (Parent is DoubleBufferedStackLayoutPanel doubleBufferedStackLayoutPanel && doubleBufferedStackLayoutPanel.LayoutEngine is StackLayoutEngine stackLayoutEngine)
				{
					stackLayoutEngine.forceLayoutChange = true;
					if (doubleBufferedStackLayoutPanel.Parent is EConversationMessageListUserControl eConversationMessageListUserControl)
					{
						eConversationMessageListUserControl.RefreshMessages();
					}
				}
#endif
			};
		}

		public Rectangle LastNewRectangle { get; set; }

		readonly bool isClientSystem;

		internal IConversationMessage Message { get; }

		#region Setup Controls

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Message != null)
			{
				RichTextActionManager.FixCustomisedHyperlinks(bodyTextBox, Message.Body, Prefix.Length);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Message != null)
			{
				SetupTextControls();
				SetupRatingControls();
			}
		}

		public string Prefix
		{
			get
			{
				if (!Message.AdditionalNoteForDisplay.IsEmpty)
				{
					return Message.AdditionalNoteForDisplay + "  ";
				}
				return "";
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void SetupTextControls()
		{
			var backgroundColor = GetBackgroundColor();

			var builder = new RtfStringBuilder(new HyperlinkActionCollection());
			if (!Message.AdditionalNoteForDisplay.IsEmpty)
			{
				builder.SetForgroundColour(Color.DarkRed);
				builder.Append(Prefix);
				builder.SetForgroundColour(Color.Black);
			}
			RichTextActionManager.SetTextFromMarkdown(bodyTextBox, Message.Body, builder);

			bodyTextBox.BackColor = backgroundColor;
			bodyTextContainsNonAsciiChars = Message.Body.ToString().Any(c => c > 255);
			if (bodyTextContainsNonAsciiChars)
			{
				bodyTextBox.Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Regular, GraphicsUnit.Point, 0); // Reflect the font used in RTF
				// Adjust text box location and size to fit unicode font
				bodyTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(bodyTextBox.Location.X, bodyTextBox.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
				ControlDpiScalingHelper.SetHeight(ref bodyTextBox, bodyTextBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(6), false);
			}

			ContextMenu bodyTextBoxContextMenu = new ContextMenu();
			bodyTextBoxContextMenu.Popup += BodyTextBoxContextMenu_Popup;
			ZMenuItem bodyTextBoxCopyMenuItem = new ZMenuItem(Res.GetData("93cb3d80-0aaf-46e3-89d5-343ed0e369dd", "Copy"));
			bodyTextBoxCopyMenuItem.Click += BodyTextBoxCopyMenuItem_Click;
			bodyTextBoxContextMenu.MenuItems.Add(bodyTextBoxCopyMenuItem);
			ZMenuItem bodyTextBoxSelectAllMenuItem = new ZMenuItem(Res.GetData("e46e88a7-8753-454d-9f2e-23d017b05b32", "Select All"));
			bodyTextBoxSelectAllMenuItem.Click += BodyTextBoxSelectAllMenuItem_Click;
			bodyTextBoxContextMenu.MenuItems.Add(bodyTextBoxSelectAllMenuItem);
			bodyTextBox.ContextMenu = bodyTextBoxContextMenu;

			timeLabel.Text = Message.SendLocalDateTime.ToShortTimeString();
			timeLabel.BackColor = backgroundColor;
			int originaTimeLabellWidth = timeLabel.Width;
			Size adjustedTimeLabelSize = TextRenderer.MeasureText(timeLabel.Text, timeLabel.Font, timeLabel.ClientSize, TextFormatFlags.Default);
			ControlDpiScalingHelper.SetWidth(ref timeLabel, adjustedTimeLabelSize.Width, false);

			int locationAdjustment = timeLabel.Width - originaTimeLabellWidth;
			timeLabel.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(timeLabel.Location.X - locationAdjustment), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(timeLabel.Location.Y));
			ratingUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(ratingUserControl.Location.X - locationAdjustment, ratingUserControl.Location.Y, false);

			usernameLabel.ForeColor = GetUsernameColor();

			if (Message.IsSystemMessage)
			{
				usernameLabel.Text = Message.SenderDisplayName;
				usercodeToolTip.SetToolTip(usernameLabel, Message.SenderDisplayName);
			}
			else
			{
				usernameLabel.Text = Message.SenderDisplayName;

				string userCaption;
				if (isClientSystem)
				{
					userCaption = (Message.MessageType == MessageType.Remote)
									? Res.GetString("bf0afaa7-6530-47f4-b36c-af5c75c7c5e1", "WiseTech Global")
									: Res.GetString("06d8e41d-c831-440d-9a3e-8aa52d5a62ae", "Local User");
				}
				else
				{
					userCaption = (Message.MessageType == MessageType.Remote)
									? Res.GetString("3b00f780-d36f-43b8-84b9-c41323ae9885", "Client User")
									: Res.GetString("bf0afaa7-6530-47f4-b36c-af5c75c7c5e1", "WiseTech Global");
				}

				usercodeToolTip.SetToolTip(usernameLabel, string.Format(CultureInfo.InvariantCulture, "{0}: {1} ({2})", userCaption, Message.SenderDisplayName, Message.SenderCode));
			}
		}

		void SetupRatingControls()
		{
			if (ShouldShowRatingControl)
			{
				ratingUserControl.BackColor = GetBackgroundColor();
				ratingUserControl.Message = Message;

				int originalWidth = ratingUserControl.Width;
				ratingUserControl.Activate(ShouldActivateRatingControl);
				int widthAdjustment = ratingUserControl.Width - originalWidth;
				ratingUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(ratingUserControl.Location.X - widthAdjustment, ratingUserControl.Location.Y, false);
			}
			else
			{
				ratingUserControl.Visible = false;
			}
		}

		bool ShouldShowRatingControl
		{
			get { return Message.Rating != 0 || ShouldActivateRatingControl; }
		}

		bool ShouldActivateRatingControl
		{
			get { return (isClientSystem && Message.IsRatingEnabled); }
		}

		bool bodyTextContainsNonAsciiChars;

		#endregion

		#region Control Actions

		internal void DeactivateRatingControlForOutdatedMessage()
		{
			if (!Message.IsRatingEnabled)
			{
				if (Message.Rating != 0)
				{
					int originalWidth = ratingUserControl.Width;
					ratingUserControl.Activate(false);
					int widthAdjustment = ratingUserControl.Width - originalWidth;
					ratingUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ratingUserControl.Location.X - widthAdjustment), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(ratingUserControl.Location.Y));
				}
				else
				{
					ratingUserControl.Visible = false;
				}
			}
		}

		internal void SetUsernameLabelVisible(IConversationMessage lastMessage)
		{
			var isSameAsPreviousMessage = lastMessage != null
				&& Message.SenderCode == lastMessage.SenderCode
				&& Message.MessageType == lastMessage.MessageType
				&& Message.MessageSubType == lastMessage.MessageSubType
				&& Message.SendLocalDateTime.Date == lastMessage.SendLocalDateTime.Date;

			usernameLabel.Visible = !isSameAsPreviousMessage;
		}

		#endregion

		#region Paint Message Panel		

		void MessagePanel_Paint(object sender, PaintEventArgs e)
		{
			#if !WINZOR

			DrawRoundRect(e.Graphics,
							messagePanel.ClientRectangle.Left,
							messagePanel.ClientRectangle.Top,
							messagePanel.ClientRectangle.Width - 1,
							messagePanel.ClientRectangle.Height - 1,
							5);
			#else
			messagePanel.BackColor = GetBackgroundColor();
			messagePanel.htmlBorder.BorderRadius = 5;
			messagePanel.htmlBorder.BorderColor = GetBorderColor();
			messagePanel.htmlBorder.BorderWidth = 0.5f;
			messagePanel.htmlBorder.BorderLineStyle = KBorderHtmlStyle.Solid;
			#endif
		}

		#if !WINZOR

		void DrawRoundRect(Graphics graphics, float x, float y, float width, float height, float radius)
		{
			using (GraphicsPath path = new GraphicsPath())
			using (var pen = new Pen(GetBorderColor()))
			{
				path.AddLine(x + radius, y, x + width - (radius * 2), y);
				path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
				path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
				path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);
				path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
				path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
				path.AddLine(x, y + height - (radius * 2), x, y + radius);
				path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
				path.CloseFigure();

				graphics.FillPath(BrushProvider.FromColor(GetBackgroundColor()), path);
				graphics.DrawPath(pen, path);
			}
		}

		#endif

		#endregion

		#region Colors

		Color GetUsernameColor()
		{
			return GetColor(ColorType.Username);
		}

		Color GetBackgroundColor()
		{
			return GetColor(ColorType.Background);
		}

		Color GetBorderColor()
		{
			return ColorTranslator.FromHtml((NoResString)"#DDDBD1");
		}

		Color GetColor(ColorType colorType)
		{
			string htmlColorCode;
			if (Message.MessageType == MessageType.LocalInternal || Message.MessageSubType == MessageSubType.SystemLog)
			{
				htmlColorCode = ColorMap[colorType].Item1;
			}
			else
			{
				if (isClientSystem)
				{
					htmlColorCode = (Message.MessageType == MessageType.Remote) ? ColorMap[colorType].Item2 : ColorMap[colorType].Item3;
				}
				else
				{
					htmlColorCode = (Message.MessageType == MessageType.Remote) ? ColorMap[colorType].Item3 : ColorMap[colorType].Item2;
				}
			}

			return ColorTranslator.FromHtml(htmlColorCode);
		}

		// <ColorType, <InternalOrSystemColor, CargoWiseColor, ClientColor>>
		Dictionary<ColorType, Tuple<string, string, string>> ColorMap
		{
			get
			{
				if (colorMap == null)
				{
					colorMap = new Dictionary<ColorType, Tuple<string, string, string>>();
					colorMap.Add(ColorType.Username, new Tuple<string, string, string>((NoResString)"#898E8C", (NoResString)"#51BFE2", (NoResString)"#F96B07"));
					colorMap.Add(ColorType.Background, new Tuple<string, string, string>((NoResString)"#F5F5F5", (NoResString)"#E8F7FF", (NoResString)"#FFF9E0"));
				}
				return colorMap;
			}
		}
		Dictionary<ColorType, Tuple<string, string, string>> colorMap;

		enum ColorType
		{
			Username,
			Background
		}

		#endregion

		#region Visible

		#endregion

		#region Resize

		int lastParentWidth = -1;

		#endregion

		public override Size GetPreferredSize(Size proposedSize)
		{
			return CalculateSize(Parent.Width);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "MaxBodyTextBoxHeight / SystemInformation.VerticalScrollBarWidth, no need for scaling")]
		public Size CalculateSize(int parentWidth)
		{
			if (Visible && !string.IsNullOrEmpty(bodyTextBox.Text) && lastParentWidth != parentWidth)
			{
				lastParentWidth = parentWidth;

				var heightWithoutBodyTextBox = Height - bodyTextBox.Height;
				var widthWithoutBodyTextBox = Width - bodyTextBox.Width;
				//that is, how much horizontal space could text fill at maximum without creating a horizontal scrollbar for the entire eConversation control?
				var parentWidthMinusPadding = parentWidth - this.Left - widthWithoutBodyTextBox - ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				var estimatedBodyTextBoxSize = EstimateBodyTextBoxSize(parentWidthMinusPadding);

				if (estimatedBodyTextBoxSize.Height > MaxBodyTextBoxHeight)
				{
					estimatedBodyTextBoxSize.Height = MaxBodyTextBoxHeight;
					estimatedBodyTextBoxSize.Width += SystemInformation.VerticalScrollBarWidth;
					bodyTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
				}
				else
				{
					bodyTextBox.ScrollBars = RichTextBoxScrollBars.None;
				}

				var timeLabelMarginLeft = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				return ControlDpiScalingHelper.NewScaledSize(estimatedBodyTextBoxSize.Width + widthWithoutBodyTextBox + timeLabelMarginLeft, estimatedBodyTextBoxSize.Height + heightWithoutBodyTextBox, false);
			}

			return this.Size;
		}

		Size EstimateBodyTextBoxSize(int proposedWidthScaled)
		{
			var reducedWidthToCounteractWordBreakDifferences = proposedWidthScaled;
			var boundingBox = ControlDpiScalingHelper.NewScaledSize(reducedWidthToCounteractWordBreakDifferences, int.MaxValue / 10, false);
			var bodyText = bodyTextBox.Text;

			#if !WINZOR
			using (var graphics = bodyTextBox.CreateGraphics())
			{
				var result = TextRendererHelper.MeasureText(graphics, bodyText, bodyTextBox.Font, boundingBox, new StringFormat(StringFormatFlags.NoClip)).ToSize();
				return result;
			}
			#else
			return TextRenderer.MeasureText(bodyText, bodyTextBox.Font, boundingBox, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
			#endif
		}

		#region Body Text Box

		int MaxBodyTextBoxHeight => (ParentForm?.Height * 2) ?? ControlDpiScalingHelper.ScaleToCurrentDpiY(2048);

		void BodyTextBoxContextMenu_Popup(object sender, EventArgs e)
		{
			if (!bodyTextBox.Focused)
			{
				bodyTextBox.Focus();
			}
			bodyTextBox.ContextMenu.MenuItems[0].Enabled = (bodyTextBox.SelectedText.Length > 0);
		}

		void BodyTextBoxCopyMenuItem_Click(object sender, EventArgs e)
		{
#if !WINZOR
			var selectedText = bodyTextBox.SelectedText ?? string.Empty;
			if (selectedText.Contains("#" + HyperlinkActionCollection.Key))
			{
				var fixedRtfWithViewkind = ORtfTextUtil.AppendRtfStrings(bodyTextBox.SelectedRtf, string.Empty);
				selectedText = ORtfTextUtil.RtfToText(fixedRtfWithViewkind, false);
				if (string.IsNullOrEmpty(selectedText) || !SafeClipboard.SetText(selectedText))
				{
					bodyTextBox.Copy();
				}
			}
			else
			{
				bodyTextBox.Copy();
			}
#else
			bodyTextBox.Copy();
#endif
		}

		void BodyTextBoxSelectAllMenuItem_Click(object sender, EventArgs e)
		{
			bodyTextBox.SelectAll();
		}

		#endregion

		#region Mouse Wheel Scroll

		internal event MouseEventHandler MessageTextBoxMouseWheel
		{
			add
			{
				bodyTextBox.MouseWheel += value;
			}
			remove
			{
				bodyTextBox.MouseWheel -= value;
			}
		}

		internal event EventHandler ChildrenControlsClick
		{
			add
			{
				messagePanel.Click += value;
				usernamePanel.Click += value;
				usernameLabel.Click += value;
				timeLabel.Click += value;
			}
			remove
			{
				messagePanel.Click -= value;
				usernamePanel.Click -= value;
				usernameLabel.Click -= value;
				timeLabel.Click -= value;
			}
		}

		#endregion

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.Control | Keys.C, ControlC);

			void ControlC()
			{
				BodyTextBoxCopyMenuItem_Click(null, null);
			}
		}
	}
}
