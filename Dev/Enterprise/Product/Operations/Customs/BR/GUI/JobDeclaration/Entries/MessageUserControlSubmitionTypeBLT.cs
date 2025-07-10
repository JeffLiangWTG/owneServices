using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class MessageUserControlSubmitionTypeBLT : ImportMessageUserControl
	{
		public MessageUserControlSubmitionTypeBLT()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
			LoadEntryLineAdditionalDataUserControl();
			InitAdditionalMenuItem();
		}

		protected JobDeclaration Declaration => JobDeclaration as JobDeclaration;

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			if (Declaration != null)
			{
				using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					EntriesBoundGrid.SetAllAvailability(false);
					EntriesBoundGrid.SetAvailability(true, GetAvailableColumnsForDeclaration(Declaration));
					EntriesBoundGrid.ReOrderColumns(GetDefaultColumnsForDeclaration(Declaration));
				}
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				EntriesBoundGrid.SetAllColumnsVisible(false);
				EntriesBoundGrid.SetColumnVisible(true, GetDefaultColumnsForDeclaration(Declaration));
				EntriesBoundGrid.ReOrderColumns(GetDefaultColumnsForDeclaration(Declaration));
			}
		}

		string[] GetAvailableColumnsForDeclaration(JobDeclaration declaration)
		{
			if (declaration.IsImportSiscomex)
			{
				return availableColumnsForImportSiscomex;
			}
			else if (declaration.IsImport)
			{
				return availableColumnsForImport;
			}

			return availableColumnsForExport;
		}

		string[] GetDefaultColumnsForDeclaration(JobDeclaration declaration)
		{
			if (declaration.IsImportSiscomex)
			{
				return defaultColumnsForImportSiscomex;
			}
			else if (declaration.IsImport)
			{
				return defaultColumnsForImport;
			}

			return defaultColumnsForExport;
		}

		void SetupEntryHeaderColumns()
		{
			using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("d2050e97-5369-4297-8cb1-39e6171b2162", "Entry Submitted Date"),
					ColumnName = CusEntryHeader.Schema.CH_EntrySubmittedDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("b9dc6066-0f5d-496f-9262-20d480b6807e", "Issue Date"),
					ColumnName = CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					DateTimeFormat = ZDateTimePickerFormat.Short,
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("3c5a21c3-c481-453a-ac4e-7e20dd1b79c4", "MRN"),
					ColumnName = CusEntryHeader.Schema.MovementReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("ec950ce5-1579-4c31-824e-fc2db7b02912", "Entry Status"),
					ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_RiskChannel,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.RiskChannelDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					DateTimeFormat = ZDateTimePickerFormat.Short,
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("ab764a47-9dfe-47d2-8acb-3d992a7b1efa", "Message Status"),
					ColumnName = CusEntryHeader.Schema.CH_Status,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("adf490d4-8160-4738-ae30-05d21d85b785", "Message Status Description"),
					ColumnName = CusEntryHeader.Schema.MessageStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("b294dd04-f75e-4284-8279-199da2cf17bb", "Declaration UCR"),
					ColumnName = CusEntryHeader.Schema.DeclarationUCR,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.EntryAccessKey,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_AdministrativeStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.AdministrativeStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_CargoStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CargoStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_AuthorityVersion,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
			});
			}
		}

		readonly string[] defaultColumnsForImport = new string[]
			{
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_AuthorityVersion,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.RiskChannelDescription,
				CusEntryHeader.Schema.CH_Status,
				CusEntryHeader.Schema.MessageStatusDescription
			};

		readonly string[] defaultColumnsForImportSiscomex = new string[]
			{
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.RiskChannelDescription,
				CusEntryHeader.Schema.CH_Status,
				CusEntryHeader.Schema.MessageStatusDescription
			};

		readonly string[] defaultColumnsForExport = new string[]
			{
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.PackagesCount,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				CusEntryHeader.Schema.MovementReferenceNumber,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.RiskChannelDescription,
				CusEntryHeader.Schema.CH_Status,
				CusEntryHeader.Schema.MessageStatusDescription,
				CusEntryHeader.Schema.CH_EntryReleaseDate,
				CusEntryHeader.Schema.DeclarationUCR,
				CusEntryHeader.Schema.EntryAccessKey,
				CusEntryHeader.Schema.CargoStatusDescription,
				CusEntryHeader.Schema.AdministrativeStatusDescription,
			};

		readonly string[] availableColumnsForExport = new string[]
			{
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_CargoStatus,
				CusEntryHeader.Schema.CH_AdministrativeStatus,
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.PackagesCount,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				CusEntryHeader.Schema.MovementReferenceNumber,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.RiskChannelDescription,
				CusEntryHeader.Schema.CH_Status,
				CusEntryHeader.Schema.MessageStatusDescription,
				CusEntryHeader.Schema.CH_EntryReleaseDate,
				CusEntryHeader.Schema.DeclarationUCR,
				CusEntryHeader.Schema.EntryAccessKey,
				CusEntryHeader.Schema.CargoStatusDescription,
				CusEntryHeader.Schema.AdministrativeStatusDescription,
				CusEntryHeader.Schema.CH_RiskChannel,
			};

		readonly string[] availableColumnsForImport = new string[]
			{
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_CargoStatus,
				CusEntryHeader.Schema.CH_AdministrativeStatus,
				CusEntryHeader.Schema.CH_AuthorityVersion,
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.RiskChannelDescription,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.CH_Status,
				CusEntryHeader.Schema.MessageStatusDescription,
				CusEntryHeader.Schema.CH_RiskChannel,
				CusEntryHeader.Schema.EntryAccessKey,
				CusEntryHeader.Schema.CargoStatusDescription,
				CusEntryHeader.Schema.AdministrativeStatusDescription,
				CusEntryHeader.Schema.RiskChannelDescription
			};

		readonly string[] availableColumnsForImportSiscomex = new string[]
			{
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.RiskChannelDescription,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.CH_Status,
				CusEntryHeader.Schema.MessageStatusDescription,
				CusEntryHeader.Schema.CH_RiskChannel,
				CusEntryHeader.Schema.RiskChannelDescription
			};

		protected override string MessagesUserControlBindingPath => "FormalEntryHeaders.Messages";

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			SetEntryLineAdditionalDataUserControlVisibility();
		}

		void SetEntryLineAdditionalDataUserControlVisibility()
		{
			EntryLineAdditionalDataUserControl.Visible = Declaration.IsImport;
			ExtendedInfoGroupBox.Visible = !Declaration.IsImport;
			SiscomexUsageFeeTabPage.TabVisible = Declaration.IsImportOnly;
		}

		void LoadEntryLineAdditionalDataUserControl()
		{
			EntryLineAdditionalDataUserControl.AllowDrop = true;
			EntryLineAdditionalDataUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			EntryLineAdditionalDataUserControl.Name = "EntryLineAdditionalDataUserControl";
			EntryLineAdditionalDataUserControl.TabIndex = 2;
			EntryLineAdditionalDataUserControl.Visible = true;
			EntryLinesTabPage.Controls.Add(EntryLineAdditionalDataUserControl);
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		internal EntryLineAdditionalDataUserControl EntryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl();

		MenuItem[] fExportAdditionalEntriesBoundGridMenuItems;
		MenuItem[] ExportAdditionalEntriesBoundGridMenuItems => fExportAdditionalEntriesBoundGridMenuItems ?? (fExportAdditionalEntriesBoundGridMenuItems = new[]
		{
			new ZMenuItem(ResString.GetMultilingualString("17D236F8-08FB-4DE1-ADE2-07C43FAFC53D", "Reset to Original"), (s, e) => ResetToOriginal_OnClick()),
			new ZMenuItem(ResString.GetMultilingualString("3B7AC4EC-1FC1-4721-B25C-0BC9EF8EF794", "Re-trigger Entry Data Request"), (s, e) => RetriggerEntryDataRequest_OnClick()),
			new ZMenuItem("-")
		});

		MenuItem[] fDuimpAdditionalEntriesBoundGridMenuItems;
		MenuItem[] DuimpAdditionalEntriesBoundGridMenuItems => fDuimpAdditionalEntriesBoundGridMenuItems ?? (fDuimpAdditionalEntriesBoundGridMenuItems = new[]
		{
			new ZMenuItem(ResString.GetMultilingualString("676DE073-4A92-45F3-84C6-639DB1EFFD03", "Consult Entry"), (s, e) => ConsultEntryDataRequest_OnClick()),
			new ZMenuItem("-")
		});

		MenuItem[] fAdditionalEntryLineGridMenuItems;
		MenuItem[] AdditionalEntryLineGridMenuItems => fAdditionalEntryLineGridMenuItems ?? (fAdditionalEntryLineGridMenuItems = new[]
		{
			new ZMenuItem(ResString.GetMultilingualString("5C14F1E0-EB76-4AB4-8D5E-D0A52733BCC9", "Generate Import License"), (s, e) => GenerateImportLicense_OnClick())
		});

		void EntriesBoundGridContextMenu_Popup(object sender, EventArgs e)
		{
			if (EntriesBoundGrid.SelectedElements.Length == 1 && EntriesBoundGrid.SelectedElements[0] is CusEntryHeader entryHeader)
			{
				var messageType = entryHeader.CH_MessageType;

				ExportAdditionalEntriesBoundGridMenuItems.ForEach(x => x.Visible = messageType == MessageTypeList.Codes.CDE);
				DuimpAdditionalEntriesBoundGridMenuItems.ForEach(x => x.Visible = messageType == MessageTypeList.Codes.CDI);
			}
			else
			{
				ExportAdditionalEntriesBoundGridMenuItems.ForEach(x => x.Visible = false);
				DuimpAdditionalEntriesBoundGridMenuItems.ForEach(x => x.Visible = false);
			}
		}

		void EntryLineGridContextMenu_Popup(object sender, EventArgs e)
		{
			var show = ((JobDeclaration)CurrentDataItem).IsImportSiscomex && EntryLineGrid.SelectedElements.Length == 1;

			foreach (var menuItem in AdditionalEntryLineGridMenuItems)
			{
				menuItem.Visible = show;
			}
		}

		void InitAdditionalMenuItem()
		{
			EntriesBoundGrid.ContextMenu.MenuItems.AddRange(ExportAdditionalEntriesBoundGridMenuItems);
			EntriesBoundGrid.ContextMenu.MenuItems.AddRange(DuimpAdditionalEntriesBoundGridMenuItems);
			EntriesBoundGrid.ContextMenu.Popup += EntriesBoundGridContextMenu_Popup;

			EntryLineGrid.ContextMenu.MenuItems.AddRange(AdditionalEntryLineGridMenuItems);
			EntryLineGrid.ContextMenu.Popup += EntryLineGridContextMenu_Popup;
		}

		void ResetToOriginal_OnClick()
		{
			var entryHeader = EntriesBoundGrid.SelectedElements.FirstOrDefault() as CusEntryHeader;
			if (entryHeader != null)
			{
				entryHeader.ResetToOriginal();
				Globals.Message.Show(Res.GetString("BF8024AB-19C0-41DB-8AF8-AB5C81899C67", "Entry {0} has been 'Reset to Original'", entryHeader.CH_BGMReference));
			}
		}

		void GenerateImportLicense_OnClick()
		{
			var entryLine = EntryLineGrid.SelectedElements.FirstOrDefault() as CusEntryLine;
			if (entryLine != null && PreSaveDeclaration())
			{
				var generateLIC = new GenerateImportLicenseObject(entryLine);
				var message = generateLIC.CanGenerateImportLicense();

				if (!message.IsEmpty)
				{
					Globals.Message.ShowError(message);
				}
				else
				{
					ZFormModaliser.ShowDialogAndDispose(new GenerateImportLicenseForm(generateLIC));
				}
			}
		}

		bool PreSaveDeclaration()
		{
			var topLevelBizObj = Declaration.Shipment as BusinessObject ?? Declaration;
			return Customs.GUI.PlugIn.CustomsPlugIn.FormPreSaved(topLevelBizObj, ParentForm as ZForm);
		}

		void RetriggerEntryDataRequest_OnClick()
		{
			var entryHeader = EntriesBoundGrid.SelectedElements.FirstOrDefault() as CusEntryHeader;
			if (entryHeader != null && PreSaveDeclaration())
			{
				var messageSender = new ExportCompleteConsultMessageSender(entryHeader);
				var errorMessage = messageSender.CanSendMessage;
				if (errorMessage.IsNullOrEmpty())
				{
					if (Globals.Message.Show(
						Res.GetString("64855F3A-62E8-4A57-820B-F84139DF4C8E", "A new request will be sent to Customs to retrieve data from the selected Entry. Do you want to proceed?"),
						Res.GetString("2BEB326D-A062-4BB7-B8CB-6E5A0D43AF98", "Re-trigger Entry Data Confirmation"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
					{
						SendMessageAndSave(messageSender);
					}
				}
				else
				{
					Globals.Message.Show(errorMessage);
				}
			}
		}

		void ConsultEntryDataRequest_OnClick()
		{
			var entryHeader = EntriesBoundGrid.SelectedElements.FirstOrDefault() as CusEntryHeader;
			if (entryHeader != null && PreSaveDeclaration())
			{
				var messageSender = new DuimpCompleteConsultMessageSender(entryHeader);
				var errorMessage = messageSender.CanSendMessage;
				if (errorMessage.IsNullOrEmpty())
				{
					SendMessageAndSave(messageSender);
				}
				else
				{
					Globals.Message.Show(errorMessage);
				}
			}
		}

		void SendMessageAndSave(BaseConsultMessageSender messageSender)
		{
			var countOfMessages = messageSender.SendMessageAndSave();
			Globals.Message.Show(Res.GetString("084F95A2-9601-451A-BF65-47866AE25006", "{0} message(s) have been sent.", countOfMessages));
		}
	}
}
