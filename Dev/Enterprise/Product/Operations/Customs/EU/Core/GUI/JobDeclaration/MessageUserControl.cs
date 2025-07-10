using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class MessageUserControl : ImportMessageUserControl
	{
		public MessageUserControl()
			: this(null)
		{
		}

		public MessageUserControl(Customs.Business.BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
			LoadEntryLineAdditionalDataUserControl();

			if (!DesignModeFinder.IsDesigning)
			{
				AddDynamicLayoutUserControl();
			}
		}

		public new JobDeclaration CurrentDataItem => (JobDeclaration)base.CurrentDataItem;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			EntryLineAdditionalDataUserVisibility(CurrentDataItem?.Configuration.UseUniversalFeeCalculation(CurrentDataItem) ?? false);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			InitTabsVisibility();
		}

		void InitTabsVisibility()
		{
			NewEntryDetailsTabPage.TabVisible = DynamicLayoutApplied;
		}

		public JobDeclaration EUDeclaration => (JobDeclaration)JobDeclaration;
		protected bool IsUCC6 => EUDeclaration?.IsUCC6 ?? false;

		#region Dynamic Layout

		void AddDynamicLayoutUserControl()
		{
			if (DynamicLayoutApplied)
			{
				EntryDetailsUserControl.SetEntryLineDetailsLayout(EntryDetailsPanelLayout);
			}
		}

		IPanelLayoutProvider EntryDetailsPanelLayout => entryDetailsPanelLayout ?? (entryDetailsPanelLayout = GetNewEntryDetailsPanelLayout());
		IPanelLayoutProvider entryDetailsPanelLayout;

		protected virtual IPanelLayoutProvider GetNewEntryDetailsPanelLayout() => new CommonEntryDetailsLayouts();

		protected virtual ZBool DynamicLayoutApplied => ZBool.False;

		#endregion

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		protected EntryLineAdditionalDataUserControl EntryLineAdditionalDataUserControl => entryLineAdditionalDataUserControl ?? (entryLineAdditionalDataUserControl = GetEntryLineAdditionalData());
		EntryLineAdditionalDataUserControl entryLineAdditionalDataUserControl;

		protected virtual EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => IsUCC6 ? new UCC6EntryLineAdditionalDataUserControl() : new EntryLineAdditionalDataUserControl();

		void EntryLineAdditionalDataUserVisibility(bool activate)
		{
			EntryLineAdditionalDataUserControl.Visible = activate;
			ExtendedInfoGroupBox.Visible = !activate;
		}

		void SetEntryAsFailedFromTransmission(object sender, EventArgs ev) => SetEntryAsFailedFromTransmissionCore(sender, ev);

		protected virtual void SetEntryAsFailedFromTransmissionCore(object sender, EventArgs ev)
		{
			int entries = 0;

			if (EntriesBoundGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("0cc45784-4a86-4a6a-ad5d-4264eb3a3c75", "Please select a row first"));
			}
			else
			{
				var hasConfirmed = false;
				foreach (CusEntryHeader entryHeader in EntriesBoundGrid.SelectedElements)
				{
					if (hasConfirmed || entryHeader.Declaration.MessageInitiator.ShowUserConfirmation(Res.GetString("522129dd-317b-4ed7-8b7f-b52b0056fd46", "Are you sure you want to set this Entry as Failed from Transmission?"),
					Res.GetString("e1e0c963-46a1-4125-b5b3-0e697540208f", "Failed from Transmission"), Res.GetString("51bdba64-b1ec-41a9-850a-b2926b993fb0", "If you are absolutely sure you want to set this Entry as Failed From Transmission, please type: "),
					Res.GetString("348c6ae3-16ea-4fa0-9457-1c23f1c3882d", "yes")))
					{
						entryHeader.SetAsFailedFromTransmission();
						entries++;
						hasConfirmed = true;
					}
				}
				if (entries == 1)
				{
					Globals.Message.Show(Res.GetString("12746e36-88a7-4ed3-895b-db50a2b63b87", "One Entry was set to Failed from Transmission"));
				}
				else
				{
					Globals.Message.Show(Res.GetString("ee6e9875-5a0d-4b90-97fc-32ac43dc7208", "{0} Entries were set to Failed from Transmission", entries.ToString(Culture.Current)));
				}
			}
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("FEB6FD47-0FE1-4852-8348-5D205D84B48E", "Duty"),
					ColumnName = CusEntryHeader.Schema.Duty,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("617F56FF-7BCF-45EB-A04B-538F78A082FE", "VAT"),
					ColumnName = CusEntryHeader.Schema.VAT,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("34417F19-0F20-48E6-8FBB-008E9E3B2C48", "Issue Date"),
					ColumnName = nameof(CusEntryHeader.CusEntryNumber) + "+" + nameof(CusEntryNumber.CE_IssueDate),
					IsMandatory = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					DateTimeFormat = ZDateTimePickerFormat.Short,
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("E7597855-30F1-4451-AD13-3560DFD86C8B", "Entry Submitted Date"),
					ColumnName = CusEntryHeader.Schema.CH_EntrySubmittedDate,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_Status,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("3E81C39F-AE74-4481-A9E3-D5A20F9880EE", "Message Status Description"),
					ColumnName = CusEntryHeader.Schema.MessageStatusDescription,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.MovementReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("F035B193-3D68-49BA-B04E-1C99CFDFCACB", "Entry Status Description"),
					ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("DC1A995A-C04C-4C0D-AE65-BAFDDA355984", "Declaration UCR"),
					ColumnName = CusEntryHeader.Schema.DeclarationUCR,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("2B25DC1C-3D40-4990-A370-36AE2A57019C", "Entry Type"),
					ColumnName = CusEntryHeader.Schema.EntryTypeFriendlyName,
					IsVisible = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_TotalPaid,
					IsVisible = false,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("A3F58E6B-BDB3-4BD8-B654-C49C8D2E3625", "Exit Status"),
					ColumnName = CusEntryHeader.Schema.CH_ExitedStatus,
					IsVisible = false,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98)
				},
			});

			EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("fafaf74a-ed79-484f-87d2-47d27769d314", "Set Entry as Failed from Transmission"), SetEntryAsFailedFromTransmission);
			EntriesBoundGrid.ReOrderColumns(entriesBoundGridDefaultOrderColumns);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			EntriesBoundGrid?.SetColumnVisible(CurrentDataItem?.IsImport ?? false, CusEntryHeader.Schema.CH_TotalPaid);
			var exitedStatus = EntriesBoundGrid?.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus);
			if (exitedStatus != null)
			{
				EntriesBoundGrid?.SetColumnVisible(CurrentDataItem?.IsExport ?? false, CusEntryHeader.Schema.CH_ExitedStatus);
			}
		}

		void LoadEntryLineAdditionalDataUserControl()
		{
			EntryLineAdditionalDataUserControl.AllowDrop = true;
			EntryLineAdditionalDataUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			EntryLineAdditionalDataUserControl.Name = nameof(Enterprise.Customs.EU.GUI.EntryLineAdditionalDataUserControl);
			EntryLineAdditionalDataUserControl.TabIndex = 2;
			EntryLineAdditionalDataUserControl.Visible = false;
			EntryLinesTabPage.Controls.Add(EntryLineAdditionalDataUserControl);
		}

		readonly string[] entriesBoundGridDefaultOrderColumns = new string[]
		{
			Customs.Business.CusEntryHeader.Schema.EntryNumber,
			Customs.Business.AutoCusEntryHeader.Schema.CH_BGMReference,
			Customs.Business.CusEntryHeader.Schema.PackagesCount,
			Customs.Business.CusEntryHeader.Schema.Duty,
			Customs.Business.CusEntryHeader.Schema.VAT,
			Customs.Business.AutoCusEntryHeader.Schema.CH_EntryStatus,
			$"{nameof(CusEntryHeader.CusEntryNumber)}+{Common.AutoCusEntryNum.Schema.CE_IssueDate}",
			Customs.Business.AutoCusEntryHeader.Schema.CH_EntrySubmittedDate,
			Customs.Business.AutoCusEntryHeader.Schema.CH_Status,
			Customs.Business.CusEntryHeader.Schema.MessageStatusDescription,
			Customs.Business.CusEntryHeader.Schema.MovementReferenceNumber,
			Customs.Business.AutoCusEntryHeader.Schema.CH_MessageType,
			Customs.Business.CusEntryHeader.Schema.CH_MessageTypeDescription,
			Customs.Business.CusEntryHeader.Schema.EntryHeaderStatusDescription,
			Customs.Business.CusEntryHeader.Schema.DeclarationUCR,
			Customs.Business.AutoCusEntryHeader.Schema.CH_EntryReleaseDate,
			EU.Business.Declaration.CusEntryHeader.Schema.EntryTypeFriendlyName,
			Customs.Business.AutoCusEntryHeader.Schema.CH_TotalPaid,
		};
	}
}
