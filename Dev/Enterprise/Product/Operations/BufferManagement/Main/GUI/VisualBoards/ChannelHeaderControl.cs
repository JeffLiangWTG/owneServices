using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ChannelHeaderControl : ZUserControl
	{
		public ChannelHeaderControl()
		{
			InitializeComponent();
		}

		public ChannelHeaderControl(CellContent cell, BMBoardSectionViewModel viewModel)
		{
			InitializeComponent();

			channel = cell.Channel ?? cell.SecondaryChannel;
			Argument.NotNull(channel, nameof(channel));

			this.cell = cell;
			this.viewModel = viewModel;
			this.applicableOrientation = viewModel.Orientation;

			if (cell.Channel == null && cell.SecondaryChannel != null)
			{
				applicableOrientation = applicableOrientation == BMBoardSectionOrientation.Horizontal ? BMBoardSectionOrientation.Vertical : BMBoardSectionOrientation.Horizontal;
			}

			SetupControls();

			if (!viewModel.IsPreview)
			{
				ContextMenuStrip = new LazyContextMenuStrip(AddMenuItems, false);
				components.Add(ContextMenuStrip);

				refreshHeadings = (s, e) => RefreshHeadingAsync(viewModel.ComponentGrid.CardAllocationMap, false);
				Channel.Reloaded += refreshHeadings;
			}

			Name = FormattableString.Invariant($"{GetType().Name}: [{viewModel.SectionName}], [{GetDesiredName(channel, channelNameLabel)}]"); // This is the proper format for this
		}

		public IVisualBoardChannel Channel
		{
			get { return channel; }
		}
		readonly IVisualBoardChannel channel;

		readonly CellContent cell;
		readonly BMBoardSectionViewModel viewModel;
		readonly BMBoardSectionOrientation applicableOrientation;
		readonly EventHandler refreshHeadings;

		#region SetupControls

		KTableLayoutPanel table;
		DirectionalLabel channelNameLabel;
		ChannelStatusLabel statusLabel;
		ChannelStatusPictureBox statusPictureBox;
		KPictureBox channelPictureBox, ccrPictureBox;
		TableLayoutStyle statusImageTableLayoutStyle;

		void SetupControls()
		{
			table = new KTableLayoutPanel
			{
				ColumnCount = applicableOrientation == BMBoardSectionOrientation.Horizontal ? 2 : 4,
				RowCount = applicableOrientation == BMBoardSectionOrientation.Horizontal ? 4 : 2,

				Dock = DockStyle.Fill,
				Padding = Padding.Empty,
				Margin = Padding.Empty,

				BackColor = Color.Transparent,
			};

			channelNameLabel = GetChannelNameLabel();
			table.Controls.Add(channelNameLabel, applicableOrientation == BMBoardSectionOrientation.Horizontal ? 0 : 2, applicableOrientation == BMBoardSectionOrientation.Horizontal ? 1 : 0);
			channelNameLabel.MouseClick += ChannelHeaderControl_MouseClick;

			statusPictureBox = GetChannelStatusPictureBox();
			table.Controls.Add(statusPictureBox, applicableOrientation == BMBoardSectionOrientation.Horizontal ? 0 : 1, applicableOrientation == BMBoardSectionOrientation.Horizontal ? 2 : 0);

			statusLabel = GetStatusLabel();
			table.Controls.Add(statusLabel, applicableOrientation == BMBoardSectionOrientation.Horizontal ? 1 : 2, 1);

			channelPictureBox = GetChannelPictureBox();
			table.Controls.Add(channelPictureBox, applicableOrientation == BMBoardSectionOrientation.Horizontal ? 0 : 3, 0);
			channelPictureBox.MouseClick += ChannelHeaderControl_MouseClick;

			ccrPictureBox = GetCcrPictureBox();
			table.Controls.Add(ccrPictureBox, 0, applicableOrientation == BMBoardSectionOrientation.Horizontal ? 3 : 0);

			channelPictureBox.AllowOverlap(ccrPictureBox);
			statusLabel.AllowOverlap(channelPictureBox);
			statusPictureBox.AllowOverlap(statusLabel);
			channelNameLabel.AllowOverlap(statusPictureBox);

			if (applicableOrientation == BMBoardSectionOrientation.Horizontal)
			{
				table.SetColumnSpan(channelPictureBox, 2);
				table.SetColumnSpan(statusPictureBox, 2);
				table.SetColumnSpan(ccrPictureBox, 2);

				statusImageTableLayoutStyle = new RowStyle(SizeType.Absolute, 0);

				table.RowStyles.Add(new RowStyle(SizeType.Absolute, channelPictureBox.Image != null ? ChannelPictureSizePixels : 0));
				table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
				table.RowStyles.Add(statusImageTableLayoutStyle);
				table.RowStyles.Add(new RowStyle(SizeType.Absolute, ccrPictureBox.Image != null ? CcrPictureSizePixels : 0));

				table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
				table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
			}
			else
			{
				table.SetRowSpan(channelPictureBox, 2);
				table.SetRowSpan(statusPictureBox, 2);
				table.SetRowSpan(ccrPictureBox, 2);

				statusImageTableLayoutStyle = new ColumnStyle(SizeType.Absolute, 0);

				table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ccrPictureBox.Image != null ? CcrPictureSizePixels : 0));
				table.ColumnStyles.Add(statusImageTableLayoutStyle);
				table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
				table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, channelPictureBox.Image != null ? ChannelPictureSizePixels : 0));

				table.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
				table.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
			}

			FadePanel.Controls.Add(table);
		}

		DirectionalLabel GetChannelNameLabel()
		{
			var label = new DirectionalLabel(isVertical: applicableOrientation == BMBoardSectionOrientation.Horizontal);
			label.Dock = DockStyle.Fill;
			label.Font = ChannelNameLabelFont;
			label.Margin = Padding.Empty;
			label.Text = GetDesiredName(channel, channelNameLabel);
			label.TextAlign = ContentAlignment.MiddleCenter;

			return label;
		}

		ChannelStatusLabel GetStatusLabel()
		{
			var label = new ChannelStatusLabel(isVertical: applicableOrientation == BMBoardSectionOrientation.Horizontal);
			label.Dock = DockStyle.Fill;
			label.Margin = Padding.Empty;
			label.TextAlign = ContentAlignment.MiddleCenter;

			if (viewModel.IsPreview)
			{
				label.Text = Channel.Status;
			}

			return label;
		}

		const int ChannelPictureSizePixels = 40;
		const int StatusPictureSizePixels = ChannelPictureSizePixels;
		const int CcrPictureSizePixels = 25;

		KPictureBox GetChannelPictureBox()
		{
			return GetPictureBox(Channel.DisplayImage, ControlDpiScalingHelper.NewScaledSize(ChannelPictureSizePixels, ChannelPictureSizePixels));
		}

		ChannelStatusPictureBox GetChannelStatusPictureBox()
		{
			return new ChannelStatusPictureBox()
			{
				Dock = DockStyle.Fill,
				Margin = Padding.Empty,
				Size = ControlDpiScalingHelper.NewScaledSize(StatusPictureSizePixels, StatusPictureSizePixels),
				SizeMode = PictureBoxSizeMode.CenterImage,
			};
		}

		KPictureBox GetCcrPictureBox()
		{
			return GetPictureBox(null, ControlDpiScalingHelper.NewScaledSize(CcrPictureSizePixels, CcrPictureSizePixels)); // No image is calculated on the first access. It should be set later when UpdateThumbnails is called.
		}

		KPictureBox GetPictureBox(Image image, [DpiState(DpiState.ScaledVariant)] Size size)
		{
			if (image == null || image.IsDisposed())
			{
				image = null;
			}
			else
			{
				image = GetImageAtMaxSize(image, size);
			}

			return new OptimisticPictureBox(shouldDisposeImageOnControlDispose: false)
			{
				Name = "CCRPictureBox",
				Dock = DockStyle.Fill,
				Image = image,
				Margin = Padding.Empty,
				Size = size,
				SizeMode = PictureBoxSizeMode.CenterImage,
			};
		}

		protected Image GetImageAtMaxSize(Image image, Size maxSize)
		{
			var result = image;

			if (image != null)
			{
				lock (ImageLocker)
				{
					if (image.Width > maxSize.Width || image.Height > maxSize.Height)
					{
						result = new Bitmap(image, maxSize);
					}
				}
			}

			return result;
		}

		static readonly object ImageLocker = new object();

		static Bitmap GetCCRBitmap(CCRCandidacy ccrStatus, bool isPersistentlyOverloaded)
		{
			switch (ccrStatus)
			{
				case CCRCandidacy.Candidate:
					return Properties.Resources.hourglass_questionmark_icon;
				case CCRCandidacy.Designate:
					{
						var ccr_hourglass = isPersistentlyOverloaded ? Properties.Resources.hourglass_icon : Properties.Resources.hourglass_icon_green;
						return ccr_hourglass;
					}
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unhandles status [{0}]", ccrStatus));
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (viewModel != null && !viewModel.IsPreview)
			{
				UpdateChannelNameText();
			}
		}

		static int ChannelNameLabelMaxLength(DirectionalLabel channelNameLabel)
		{
			if (channelNameLabel == null)
			{
				return int.MaxValue;
			}

			int channelLabelLength = channelNameLabel.IsVertical ? channelNameLabel.Height : channelNameLabel.Width;
			// channelLabelLength == 1 means that the channelNameLabel is "", which means it isn't set in the current instance.
			if (channelLabelLength == 1)
			{
				return int.MaxValue;
			}
			else
			{
				return channelLabelLength;
			}
		}

		static Font ChannelNameLabelFont => new Font("Arial", 12f, FontStyle.Bold);

		#endregion

		#region Channel form

		public void ShowChannelForm()
		{
			if (Channel != null)
			{
				var pair = GetChannelControllerID(Channel);
				var channelControllerID = pair.Item1;
				var channelEntity = pair.Item2;

				if (channelControllerID != null && channelEntity != null)
				{
					MainThreadRunner.RunOnMainThread(() =>
					{
						var factory = new BusinessObjectFactory { NameForDebugging = nameof(ShowChannelForm) };
						var loadedChannelEntity = factory.Load(channelEntity.GetType(), channelEntity.PK);

						if (loadedChannelEntity != null)
						{
							try
							{
								ZControllerFactory.Create(channelControllerID).ShowEditForm(loadedChannelEntity);
							}
							catch (ModuleGuiNotSupportedException ex)
							{
								Globals.Message.ShowError(ex.Message);
							}
						}
					});
				}
			}
		}

		Tuple<ControllerID, BusinessObject> GetChannelControllerID(IVisualBoardChannel visualBoardChannel)
		{
			var factory = viewModel.FactoryProvider.GetNewEditFactory((NoResString)"Show Channel Form"); // Debug only name

			switch (visualBoardChannel.EntityType)
			{
				case ChannelTypeList.Codes.Resource:
					return Tuple.Create<ControllerID, BusinessObject>(ControllerIDs.GlbStaff, factory.Load<GlbStaff>(channel.EntityPK));

				case ChannelTypeList.Codes.Group:
					return Tuple.Create<ControllerID, BusinessObject>(ControllerIDs.GlbGroup, factory.Load<GlbGroup>(channel.EntityPK));

				case ChannelTypeList.Codes.Capability:
					return Tuple.Create<ControllerID, BusinessObject>(ControllerIDs.GlbCapability, factory.Load<GlbCapability>(channel.EntityPK));

				case ChannelTypeList.Codes.Tag:
					var bizo = factory.Load<TagMagnitude>(channel.EntityPK);
					if (bizo == null || bizo is WorkQueue)
					{
						return Tuple.Create<ControllerID, BusinessObject>(ControllerIDs.WorkQueues, bizo);
					}
					else
					{
						//return Tuple.Create<ControllerID, BusinessObject>(ControllerIDs.BMTagDefinition, bizo.Definition);
						return Tuple.Create<ControllerID, BusinessObject>(ControllerIDs.BMTagMagnitude, bizo);
					}

				default:
					return Tuple.Create<ControllerID, BusinessObject>(null, null);
			}
		}

		#endregion

		#region ChannelExpanded

		public void OnChannelExpansionToggled(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isExpanded = !isExpanded;
				if (Channel.BackgroundColor == Color.Transparent)
				{
					if (isExpanded)
					{
						originalBackgroundColor = BackColor;
						BackColor = Color.White;
					}
					else
					{
						BackColor = originalBackgroundColor;
					}
				}
			}
		}

		Color originalBackgroundColor;

		bool isExpanded;

		#endregion

		#region Event Handlers

		void ChannelHeaderControl_MouseClick(object sender, MouseEventArgs e)
		{
			OnChannelExpansionToggled(e);
		}

		public void RefreshHeading(object sender, HeadingsRefreshedEventArgs e)
		{
			this.BeginInvokeSafe(() =>
			{
				try
				{
					this.SuspendDrawing();
					SuspendLayout();

					RoadRunnerDetails roadRunner;
					e.ViewModels.RoadRunnerDetails.TryGetValue(Channel.ChannelEntityCode, out roadRunner);

					RefreshHeadingCore(e.ViewModels.ChannelViewModels[Channel], roadRunner);
				}
				finally
				{
					this.ResumeDrawing();
					ResumeLayout();

					RefreshCompleted?.Invoke(this, e);
				}
			});
		}

		internal event EventHandler<HeadingsRefreshedEventArgs> RefreshCompleted;

		#endregion

		#region Refresh Heading

		void RefreshHeadingAsync(CardAllocationMap allocationMap, bool clearCache = true)
		{
			void RefreshHeading()
			{
				if (clearCache)
				{
					Channel.ClearChannelCache();
				}

				var factory = viewModel.FactoryProvider.GetNewBackgroundThreadLoaderFactory("RefreshHeadingAsync");
				var set = ChannelHeadingViewModelBuilder.MakeViewModelSet(new[] { Channel }, factory, viewModel, allocationMap);
				var map = set.ChannelViewModels[Channel];
				set.RoadRunnerDetails.TryGetValue(Channel.ChannelEntityCode, out RoadRunnerDetails roadRunner);

				this.BeginInvokeSafe(() => RefreshHeadingCore(map, roadRunner));
			}

			if (BMSRegistry.Instance.BoardOnSecondaryServer.Value)
			{
				RefreshHeading();
			}
			else
			{
				AsyncStrategy.Default.DoAsync(RefreshHeading);
			}
		}

		void RefreshHeadingCore(ChannelHeadingViewModel map, RoadRunnerDetails roadRunnerDetails)
		{
			UpdateThumbnails(map.Thumbnails, roadRunnerDetails);
			UpdateBackground(map.Background);
			UpdateStatus(map.Status);
			UpdateChannelNameText();
		}

		#endregion

		#region Background Fade

		void UpdateBackground(ChannelHeadingViewModel.BackgroundViewModel background)
		{
			if (background.IsBackgroundFade)
			{
				UpdateHeaderFade(background.GetFadeStartColor(cell.BackColor ?? BackColor), background.FadePercent);
			}
			else
			{
				BackColor = background.BackColor;
				ForeColor = background.ForeColor;
			}
		}

		void UpdateHeaderFade(Color fadeStartColor, float fadePercent)
		{
			FadePanel.FadeEndColor = cell.BackColor ?? BackColor;
			FadePanel.GradientAngle = applicableOrientation == BMBoardSectionOrientation.Horizontal ? 270f : 0f;
			FadePanel.GradientSizePercent = (float)ChannelHeadingViewModelBuilder.GradientSizePercent;
			FadePanel.FadeStartColor = fadeStartColor;
			FadePanel.SetFadePercentProgressively(fadePercent);
		}

		#endregion

		#region UpdateStatus

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void UpdateStatus(ChannelHeadingViewModel.StatusViewModel status)
		{
			statusLabel.Text = status.StatusText;
			statusLabel.IsClickable = status.StatusText != status.ToolTipStatusText;
			statusLabel.Cursor = statusLabel.IsClickable ? Cursors.Hand : Cursors.Default;
			if (Channel.EntityType == ChannelTypeList.Codes.Resource && Channel.EntityPK.IsValid)
			{
				ToolTipService.SetToolTip(statusLabel, status.ToolTipStatusText);
			}

			if (statusLabel.IsClickable)
			{
				AddStatusLabelClickHandler(status);
			}
		}

		void AddStatusLabelClickHandler(ChannelHeadingViewModel.StatusViewModel status)
		{
			if (statusLabelClickHandler != null)
			{
				statusLabel.Click -= statusLabelClickHandler;
			}

			statusLabelClickHandler = (s, e) =>
			{
				var componentControl = (Parent as HeaderControlProvider.HeaderWrapperControl).ComponentControl;
				var filter = FilterApplicator.GetCurrentFilter(viewModel);
				FilterApplicator.ToggleComponentViewFilter(status.RiskComponentPK, filter, viewModel, componentControl, componentControl.SectionConfiguration);
			};

			statusLabel.Click += statusLabelClickHandler;
		}

		#endregion

		#region UpdateThumbnails

		void UpdateThumbnails(ChannelHeadingViewModel.ThumbnailViewModel thumbnails, RoadRunnerDetails roadRunnerDetails)
		{
			if (statusPictureBox != null)
			{
				ToolTipService.ClearTooltip(statusPictureBox);
			}
			if (ccrPictureBox != null)
			{
				ToolTipService.ClearTooltip(ccrPictureBox);
			}

			UpdateStatusBitmap(thumbnails, roadRunnerDetails);
			UpdateCCRBitmap(thumbnails);
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void UpdateStatusBitmap(ChannelHeadingViewModel.ThumbnailViewModel thumbnails, RoadRunnerDetails roadRunnerDetails)
		{
			var statusImage = thumbnails.StatusImage;

			if (statusPictureBox.Image != null)
			{
				statusPictureBox.Image.Dispose();
			}

			var statusBitmap = statusImage.Image;
			if (statusBitmap != null && !statusBitmap.IsDisposed() && roadRunnerDetails != null)
			{
				statusPictureBox.Image = GetImageAtMaxSize(statusBitmap, ControlDpiScalingHelper.NewScaledSize(StatusPictureSizePixels, StatusPictureSizePixels));
				statusPictureBox.IsClickable = roadRunnerDetails.Status == RoadRunnerStatus.Alert || roadRunnerDetails.Status == RoadRunnerStatus.WorkingInOtherComponent;
				statusPictureBox.Cursor = statusPictureBox.IsClickable ? Cursors.Hand : Cursors.Default;
				UpdateStatusClickHandler(statusPictureBox.IsClickable, roadRunnerDetails);

				var rowStyle = statusImageTableLayoutStyle as RowStyle;
				if (rowStyle != null)
				{
					ControlDpiScalingHelper.SetHeight(ref rowStyle, StatusPictureSizePixels, true);
				}
				else
				{
					var colStyle = statusImageTableLayoutStyle as ColumnStyle;
					if (colStyle != null)
					{
						ControlDpiScalingHelper.SetWidth(ref colStyle, StatusPictureSizePixels, true);
					}
				}

				if (!string.IsNullOrEmpty(statusImage.ImageTooltip))
				{
					ToolTipService.SetToolTip(statusPictureBox, statusImage.ImageTooltip);
				}
			}
		}

		void UpdateStatusClickHandler(bool allowClickHandler, RoadRunnerDetails roadRunnerDetails)
		{
			if (statusImageClickHandler != null)
			{
				statusPictureBox.Click -= statusImageClickHandler;
			}

			if (allowClickHandler)
			{
				statusImageClickHandler = (s, e) =>
				{
					var task = roadRunnerDetails.RelevantProcessTaskPK;
					if (task != ZGuid.Empty)
					{
						MainThreadRunner.RunOnMainThread(() =>
						{
							var factory = new BusinessObjectFactory { NameForDebugging = "Alert Task Factory" };
							var loadedTask = factory.Load<ProcessTask>(task);
							if (loadedTask != null)
							{
								WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedTask);
							}
							else
							{
								Globals.Message.Show(Res.GetString("935d305b-0a0e-4d8d-b227-c633a599c149", "Task {0}, has been deleted.", roadRunnerDetails.RelevantTaskDescription));
							}
						});
					}
				};

				statusPictureBox.Click += statusImageClickHandler;
			}
			else
			{
				statusImageClickHandler = null;
			}
		}
		EventHandler statusImageClickHandler;
		EventHandler statusLabelClickHandler;

		void UpdateCCRBitmap(ChannelHeadingViewModel.ThumbnailViewModel thumbnails)
		{
			if (channel.EntityType == ChannelTypeList.Codes.Resource && channel.EntityPK.IsValid)
			{
				var ccrStatus = thumbnails.CCRStatus;

				if (ccrStatus != CCRCandidacy.None)
				{
					SetCCRImage(ccrStatus, thumbnails.IsPersistentlyOverloaded);
					SetCCRTooltip(thumbnails);
				}
				else if (ccrPictureBox.Image != null)
				{
					var image = ccrPictureBox.Image;
					ccrPictureBox.Image = null;
					image.Dispose();
				}
			}
		}

		void SetCCRImage(CCRCandidacy ccrType, bool isPersistentlyOverloaded)
		{
			var image = GetCCRBitmap(ccrType, isPersistentlyOverloaded);

			if (ccrPictureBox.Image != null)
			{
				ccrPictureBox.Image.Dispose();
			}
			else
			{
				if (applicableOrientation == BMBoardSectionOrientation.Horizontal)
				{
					ControlDpiScalingHelper.SetHeight(table.RowStyles[table.RowCount - 1], CcrPictureSizePixels, true);
				}
				else
				{
					ControlDpiScalingHelper.SetWidth(table.ColumnStyles[0], CcrPictureSizePixels, true);
				}
			}

			var size = ControlDpiScalingHelper.NewScaledSize(CcrPictureSizePixels, CcrPictureSizePixels);
			ccrPictureBox.Image = new Bitmap(image, size);
			ccrPictureBox.Size = size;
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void SetCCRTooltip(ChannelHeadingViewModel.ThumbnailViewModel thumbnails)
		{
			var tooltipMessage = string.Empty;

			switch (thumbnails.CCRStatus)
			{
				case CCRCandidacy.None:
					break;

				case CCRCandidacy.Candidate:
					{
						tooltipMessage = (thumbnails.TimeConsideredCCR.IsEmpty
							? Res.GetString("7c4db048-6e46-46f1-b4ed-a379c59d1114", "This resource was detected as a capacity constrained candidate {0}.", thumbnails.TimeConsideredCCR)
					: Res.GetString("c28a90be-cd21-4d6d-95c1-f3a4debcdf07", "This resource was detected as a capacity constrained candidate."))
					+ "\r\n"
					+ Res.GetString("a14b9903-098d-4553-8c24-da715176e78d", "Right-click to mark as constrained.");
					}
					break;

				case CCRCandidacy.Designate:
					{
						tooltipMessage = thumbnails.IsPersistentlyOverloaded
							? Res.GetString("fb8730b7-ba96-4e72-bee9-c8a2387cdbcd", "This resource is designated as a Capacity Constrained Resource. They have been detected to be persistently overloaded.")
							: Res.GetString("9ade3d90-c980-4234-88e8-719dd8360786", "This resource is designated as a Capacity Constrained Resource.");
					}
					break;

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognised CCR type [{0}]", thumbnails.CCRStatus));
			}

			if (!string.IsNullOrEmpty(tooltipMessage))
			{
				ToolTipService.SetToolTip(ccrPictureBox, tooltipMessage);
			}
		}

		#endregion

		#region UpdateChannelNameText

		void UpdateChannelNameText()
		{
			var names = Channel?.Names;
			if (names != null)
			{
				channelNameLabel.Text = GetDesiredName(channel, channelNameLabel);
			}
		}

		bool IsInvalidChannelHeader(string name, Func<string, bool> isNameTooLong)
		{
			return string.IsNullOrEmpty(name) || isNameTooLong(name);
		}

		static string GetDesiredName(IVisualBoardChannel channel, DirectionalLabel channelNameLabel)
		{
			if (channel.Names != null)
			{
				var font = channelNameLabel?.Font ?? ChannelNameLabelFont;
				var channelHeaderLength = ChannelNameLabelMaxLength(channelNameLabel);
				return channel.Names.GetDisplayableName(channel.EntityType, c => IsSuitableName(c, font, channelHeaderLength));
			}
			else
			{
				return string.Empty;
			}
		}

		static bool IsSuitableName(ChannelNamePair name, Font font, int channelHeaderLength)
		{
			var nameIsNotEmpty = !name.Name.IsEmpty;

			if (font == null)
			{
				return nameIsNotEmpty;
			}

			return (TextRenderer.MeasureText(name.Name, font).Width <= channelHeaderLength && nameIsNotEmpty)
				|| name.DisplayNameType == DisplayNameType.ChannelCode; // If we get to the point we're evaluating the Channel Code, then Full and FriendlyName are too long, so Code MUST be displayed.
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && (components != null))
				{
					components.Dispose();
					ContextMenuStrip?.Dispose();
					channel.Reloaded -= refreshHeadings;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		protected override bool ReportDisposeCalledToEventLog => true;

		#endregion

		#region Context menu

		void AddMenuItems(KContextMenuStrip menuStrip, Control control)
		{
			var openChannelFormToolStripMenuItem = new ZToolStripMenuItem(Res.GetData("277a5cef-4a27-4418-add8-ba5711dd7bf6", "Open Channel Form"), OpenChannelFormToolStripMenuItem_Click);
			menuStrip.Items.Add(openChannelFormToolStripMenuItem);
			openChannelFormToolStripMenuItem.Name = "OpenChannelFormToolStripMenuItem";
			openChannelFormToolStripMenuItem.Size = ControlDpiScalingHelper.NewScaledSize(181, 22, true);

			if (!viewModel.IsPreview && channel != null && channel.EntityType == ChannelTypeList.Codes.Resource && channel.EntityPK.IsValid)
			{
				var factory = viewModel.FactoryProvider.GetNewEditFactory("ChannelHeaderControlMenu");
				var resource = factory.Load<GlbStaff>(channel.EntityPK);
				var section = factory.Load<BMBoardSection>(viewModel.SectionPK);

				if (viewModel.IsBuffer && resource != null && section != null)
				{
					var menuItem = new CapacityConstrainedResourceMenuItem(resource, section.Component, viewModel);
					menuStrip.Items.Add(menuItem);

					menuItem.Click += (s, e) => RefreshHeadingAsync(viewModel.ComponentGrid.CardAllocationMap);

					var availabilityMenuItem = new AvailabilityMenuItem(resource, section.Component);
					menuStrip.Items.Add(availabilityMenuItem);
				}
			}
		}

		void OpenChannelFormToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowChannelForm();
		}

		#endregion

		#region For Test
#if DEBUG
		public string ChannelStatus
		{
			get { return statusLabel.Text; }
		}

		public Label ChannelStatusLabel
		{
			get { return statusLabel; }
		}

		public PictureBox StatusPictureBox
		{
			get { return statusPictureBox; }
		}

		public EventHandler StatusImageClickHandler
		{
			get { return statusImageClickHandler; }
		}

		public EventHandler StatusLabelClickHandler
		{
			get { return statusLabelClickHandler; }
		}

		public Label ChannelNameLabel
		{
			get { return channelNameLabel; }
		}

		public PictureBox ThumbnailPhotoPictureBox
		{
			get { return channelPictureBox; }
		}

		public PictureBox CcrPictureBox
		{
			get { return ccrPictureBox; }
		}

		public BMBoardSectionViewModel ViewModel_ForTest
		{
			get { return viewModel; }
		}

#endif
		#endregion
	}
}
