using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.GUI
{
	public partial class MessageUserControl : EU.GUI.MessageUserControl
	{
		public MessageUserControl()
			: this(null)
		{
		}

		public MessageUserControl(Customs.Business.BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			SetupEntryGridColumns();
			SetUpEntryGridContextMenu();
			SetupEntryLineColumn();
			SetupEntryFeesTab();
			SetUpCorrelationIDColumn();

			RequiresMergeLabel.AllowOverlap(MainHorizontalSplitContainer);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ForceEntryLineAdditionalDataVisibility();
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);
		
		protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => IsUCC6 ? new UCC6EntryLineAdditionalDataUserControl() : new EntryLineAdditionalDataUserControl();

		void SetUpCorrelationIDColumn()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("FCB3D745-3D15-4185-A0CD-010890F6DF98", "LRN/Correlation ID");
			zTextBoxColumnStyleInfo1.ColumnName = "CorrelationID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		}

		void ForceEntryLineAdditionalDataVisibility()
		{
			EntryLineAdditionalDataUserControl.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			EntryLineAdditionalDataUserControl.Visible = true;
			ExtendedInfoGroupBox.Visible = false;
		}

		void SetupEntryFeesTab()
		{
			var entryFeePanel = new ZPanel();
			entryFeePanel.Dock = DockStyle.Fill;
			EntryFeesTabPage.Controls.Add(entryFeePanel);
			entryFeePanel.Controls.Add(SetupEntryFeesGrid());
			entryFeePanel.Controls.Add(SetupEntryConfirmedChargesGrid());
		}

		ZGrid SetupEntryFeesGrid()
		{
			var entryFeeGrid = new ZGrid();
			entryFeeGrid.GridId = "3b731451-b567-46a8-b708-f5c515f6d5cb";
			entryFeeGrid.Name = "EntryFeesGrid";
			entryFeeGrid.BindTo = "CustomsEntryHeaders.Charges";
			entryFeeGrid.CaptionText = Enterprise.Customs.FR.GUI.Res.GetString("51628E08-72C2-48D6-B1B2-8BE5D02D7F5F", "           Calculated");
			entryFeeGrid.CaptionVisible = true;
			entryFeeGrid.CaptionBackColor = System.Drawing.Color.WhiteSmoke;

			entryFeeGrid.ColumnStyles.AddRange(GetEntryChargesColumnInfos());

			entryFeeGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = CusEntryHeaderChargesSchema.Constants.C1_RateOverrideReasonCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			entryFeeGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			entryFeeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			entryFeeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 220, true);

			return entryFeeGrid;
		}

		ZGrid SetupEntryConfirmedChargesGrid()
		{
			var entryConfirmedChargesGrid = new ZGrid();
			entryConfirmedChargesGrid.GridId = "F4608C6A-FCEE-4414-B027-2155F0305D36";
			entryConfirmedChargesGrid.Name = "ConfirmedFeesGrid";
			entryConfirmedChargesGrid.BindTo = "CustomsEntryHeaders.ConfirmedCharges";
			entryConfirmedChargesGrid.CaptionText = Enterprise.Customs.FR.GUI.Res.GetString("684C20F7-9431-4D3D-87B6-E783E0AA2067", "           Confirmed");
			entryConfirmedChargesGrid.CaptionVisible = true;
			entryConfirmedChargesGrid.CaptionBackColor = System.Drawing.Color.WhiteSmoke;
			entryConfirmedChargesGrid.ReadOnly = true;

			entryConfirmedChargesGrid.ColumnStyles.AddRange(GetEntryChargesColumnInfos());
			entryConfirmedChargesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				ColumnName = CusEntryHeaderCharges.Schema.NationalFeeTypeCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			});
			entryConfirmedChargesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				ColumnName = CusEntryHeaderCharges.Schema.TaxStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
			});

			entryConfirmedChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			entryConfirmedChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			entryConfirmedChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 220, true);

			return entryConfirmedChargesGrid;
		}

		ZGridColumnInfo[] GetEntryChargesColumnInfos()
		{
			return new ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "C1_ChargeType",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "C1_ChargeAmount",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "C1_MethodOfPayment",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				}
			};
		}

		void SetupEntryGridColumns()
		{
			EntriesBoundGrid.ReadOnly = false;
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate).IsReadOnly = true;
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_Status).IsReadOnly = true;
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntrySubmittedDate).IsReadOnly = true;
			EntriesBoundGrid.GetColumnStyle("CusEntryNumber+CE_IssueDate").IsReadOnly = true;
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryStatus).IsReadOnly = true;
			EntriesBoundGrid.GetColumnStyle((NoResString)"Duty").IsReadOnly = true;
			EntriesBoundGrid.GetColumnStyle("VAT").IsReadOnly = true;
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus).IsReadOnly = true;

			EntriesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.SequenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsReadOnly = true
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_TriggeringPointForValidation,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "FRCustomsFallbackNumber",
					CaptionResourceString = Res.GetData("2E14AF30-60EA-44A5-BF07-0C617DBE3B7C", "Fallback Entry Number"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = "DeltaGFallbackStatus",
					CaptionResourceString = Res.GetData("F1CF54CC-39E8-4F7D-988F-FB2C888591B0", "Delta G Fallback Status"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = "DeltaGFallbackIssueDate",
					CaptionResourceString = Res.GetData("39953C5B-E11F-4945-B704-F46C22279B48", "Fallback Issue Date"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = "CH_Calc_GuaranteeAmount",
					CaptionResourceString = Res.GetData("EBA54E17-C033-4DC5-8C6D-75E5F150B778", "Guarantee Amount"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = "CH_ConfirmedGuaranteeAmount",
					CaptionResourceString = Res.GetData("2AD4FCF9-5A0C-4E75-9242-5C7F3DE2229C", "Confirmed Guarantee Amount"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_CalcTotalInvoicedAmountInLocalCurrency,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsReadOnly = true
				},
			});
		}

		void SetUpEntryGridContextMenu()
		{
			creditCodeMenuItem = new ZMenuItem(Res.GetString("42B8F5FA-4331-46CB-BE65-7193AB3E0910", "Credit COD"), CreditCodeMenuItem_Click);
			addAlternateProofOfExitMenuItem = new ZMenuItem(Res.GetString("6EABC0FC-4BE9-42F0-A9C3-86D8F21EADE7", "Add Alternate Proof of Exit"), AddAlternateProofOfExitMenuItem_Click);
			removeAlternateProofOfExitMenuItem = new ZMenuItem(Res.GetString("9B25783D-4C52-43F4-9E21-327A7A45F778", "Remove Alternate Proof of Exit"), RemoveAlternateProofOfExitMenuItem_Click);
			setEntryAsAmendmentMenuItem = new ZMenuItem(Res.GetString("83341C0D-C2A8-4844-9FE3-591A1E6DF3EC", "Set Entry As Amendment"), SetEntryAsAmendmentMenuItem_Click);
			addAlternateProofOfExitMenuItem.Visible = false;
			removeAlternateProofOfExitMenuItem.Visible = false;
			creditCodeMenuItem.Visible = false;
			setEntryAsAmendmentMenuItem.Visible = false;
			EntriesBoundGrid.ContextMenu.Popup -= ContextMenu_Popup;
			EntriesBoundGrid.ContextMenu.Popup += ContextMenu_Popup;
			EntriesBoundGrid.ContextMenu.MenuItems.Add(addAlternateProofOfExitMenuItem);
			EntriesBoundGrid.ContextMenu.MenuItems.Add(removeAlternateProofOfExitMenuItem);
			EntriesBoundGrid.ContextMenu.MenuItems.Add(creditCodeMenuItem);
			EntriesBoundGrid.ContextMenu.MenuItems.Add(setEntryAsAmendmentMenuItem);
		}

		MenuItem addAlternateProofOfExitMenuItem;
		MenuItem removeAlternateProofOfExitMenuItem;
		MenuItem creditCodeMenuItem;
		MenuItem setEntryAsAmendmentMenuItem;

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			addAlternateProofOfExitMenuItem.Visible = IsAddAlternateProofOfExitMenuItemVisible();
			removeAlternateProofOfExitMenuItem.Visible = IsRemoveAlternateProofOfExitMenuItemVisible();
			creditCodeMenuItem.Visible = EntriesBoundGrid.SelectedElements.Length > 0;
			setEntryAsAmendmentMenuItem.Visible = IsSetEntryAsAmendmentMenuItemVisible();
		}

		bool IsAddAlternateProofOfExitMenuItemVisible() => IsExportAndSingleEntrySelected();

		bool IsRemoveAlternateProofOfExitMenuItemVisible()
		{
			var visible = false;
			if (IsExportAndSingleEntrySelected())
			{
				var docTypeForAPE = FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.Value;
				if (!string.IsNullOrEmpty(docTypeForAPE))
				{
					var selectedEntry = EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>().First();
					var docManagerInfo = (selectedEntry as IDocManagerSupport).DocManagerInfo;
					var eDocs = docManagerInfo.AllEDocs;
					visible = eDocs.Cast<IeDoc>().Any(d => !d.IsDeleted && d.DocType == docTypeForAPE);
				}
			}
			return visible;
		}

		bool IsSetEntryAsAmendmentMenuItemVisible() => IsUCC6 && JobDeclaration.IsImport;

		bool IsExportAndSingleEntrySelected()
		{
			var selectedEntries = EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>().ToArray();
			var isSelectedEntryValid = selectedEntries.Length == 1 && !selectedEntries[0].EntryNumber.IsEmpty && !selectedEntries[0].MovementReferenceNumber.IsEmpty;
			var isExport = JobDeclaration?.IsExport ?? false;
			return isSelectedEntryValid && isExport;
		}

		void CreditCodeMenuItem_Click(object sender, EventArgs e)
		{
			var runnerFactory = new BusinessObjectFactory();
			var actionInRunnerFactory = runnerFactory.Load<OperationalAction>(new ZGuid("b2fe4809-c252-4519-b3da-ee7d19e67919"));

			OperationalActionSupporter actionSupporter = null;
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.EntryHeader))
			{
				actionSupporter = ((IOperationalActionSupportable)module).OperationalActionSupporter;
			}

			actionInRunnerFactory.Context = new OperationalActionContext(actionSupporter, (NoResString)"Customs Entries");
			var selectedRecords = new SelectedRecords()
			{
				AutoSelectedAllKeys = false,
				PrimaryKeys = EntriesBoundGrid.SelectedElements.Select(target => target.PK).ToArray() ?? Array.Empty<ZGuid>(),
			};

			var runner = new OperationalActionRunner(actionInRunnerFactory, typeof(CusEntryHeader), selectedRecords);
			OperationalActionRunnerForm.Show(runner);
		}

		void AddAlternateProofOfExitMenuItem_Click(object sender, EventArgs e)
		{
			var entryHeader = EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>().FirstOrDefault();
			if (entryHeader != null && GetDocTypeForAPEAndQueryUserIfNotConfigured(out var docTypeForAPE) && SaveAndContinue())
			{
				var docManagerInfo = (entryHeader as IDocManagerSupport).DocManagerInfo;
				var (contents, filename) = GetFileContentsAndName();
				var eDocs = docManagerInfo.AllEDocs;
				var isSelectedFileAdded = eDocs.Cast<IeDoc>().Any(d => !d.IsDeleted && Path.GetFileName(d.FileName) == filename);
				if (isSelectedFileAdded)
				{
					Globals.Message.ShowWarning(Res.GetString("9D4E722D-7BDF-4B9E-B90F-B6D3BC9049F1", "A document with the selected filename already exists. No action will be performed."));
				}
				else if (contents != null)
				{
					docManagerInfo.AddFileOrDocument(contents, filename, docTypeForAPE);
					docManagerInfo.Save();
					entryHeader.CH_ExitedStatus = ExportControlStatusList.Codes.APE;
					entryHeader.Factory.Save();

					var successMessage = Res.GetString("EBF1B90D-3CF8-4626-83FA-A533E36F5642", "ECS status has been set to APE, and a document has has been added.");
					var eDocsRefreshHint = Res.GetString("22844BDA-05F6-4CD0-94AD-048746CB11EF", "You may need to click the \"Refresh\" button in eDocs tab to see the latest documents.");
					Globals.Message.ShowInformation(successMessage + "\r\n" + eDocsRefreshHint);
				}
			}
		}

		bool SaveAndContinue()
		{
			var canContinue = true;

			if (JobDeclaration.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("7E9CB967-892E-4C42-9B4C-6FDBD5B68087", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("7AD553F6-272B-43E0-9455-0FE6395CDD53", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes);

				canContinue = messageBoxResult == DialogResult.Yes && this.FireSaveButton() == ContinueWithSave.Yes;
			}

			return canContinue && !JobDeclaration.HasChanges;
		}

		protected virtual (byte[], string) GetFileContentsAndName()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.CheckFileExists = true;
				openFileDialog.Multiselect = false;
				var contents = openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.OpenFile().ToByteArray() : null;
				var filename = Path.GetFileName(openFileDialog.UnmappedFileName);
				return (contents, filename);
			}
		}

		void RemoveAlternateProofOfExitMenuItem_Click(object sender, EventArgs e)
		{
			var entryHeader = EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>().FirstOrDefault();
			if (entryHeader != null && GetDocTypeForAPEAndQueryUserIfNotConfigured(out var docTypeForAPE) && SaveAndContinue())
			{
				var previousECSStatusToAPE = entryHeader.PreviousECSStatusToAPE;
				var hasPreviousECSStatusToAPE = previousECSStatusToAPE != ZString.Empty;
				var message = hasPreviousECSStatusToAPE
					? Res.GetString("584F8C72-D676-47FB-B1EC-EBEDD88D1535", "Are you sure you want to remove the Alternate Proof of Exit and revert ECS status to ") + previousECSStatusToAPE + "?"
					: Res.GetString("70A7C942-F648-4230-8EED-5297156224C6", "Are you sure you want to remove the Alternate Proof of Exit?");

				if (entryHeader.Declaration.MessageInitiator.ShowUserConfirmation(message, Res.GetString("fd132110-d2d7-40a8-8273-868e3ca4b5aa", "Remove Alternate Proof of Exit"), Res.GetString("e04f85bb-e44d-4805-af4b-d45b55b64aa4", "To confirm, please type: "), (NoResString)"yes"))
				{
					var docManagerInfo = (entryHeader as IDocManagerSupport).DocManagerInfo;
					var eDocs = docManagerInfo.AllEDocs;
					var docsToRemove = eDocs.Cast<IeDoc>().Where(d => !d.IsDeleted && d.DocType == docTypeForAPE).ToArray();
					var deletedDocNames = ZString.Join("\r\n\t", docsToRemove.Select(d => d.FileName).Cast<ZString>().ToArray());

					for (int i = 0; i < docsToRemove.Length; i++)
					{
						docsToRemove[i].IsDeleted = true;
						eDocs.Remove(docsToRemove[i]);
					}

					docManagerInfo.Save();

					if (hasPreviousECSStatusToAPE)
					{
						entryHeader.CH_ExitedStatus = previousECSStatusToAPE;
						entryHeader.Factory.Save();
					}

					var revertedMessage = hasPreviousECSStatusToAPE ? Res.GetString("91AA3553-41A9-4B85-AD41-82B1486C75FA", "ECS status has been reverted to ") + previousECSStatusToAPE + "." + "\r\n" : string.Empty;
					var deletedMessage = Res.GetString("508260CC-98FA-4ABF-8920-5FA78C035606", "Document(s) have been deleted permanently:") + "\r\n\r\n\t" + deletedDocNames;
					var eDocsRefreshHint = Res.GetString("22844BDA-05F6-4CD0-94AD-048746CB11EF", "You may need to click the \"Refresh\" button in eDocs tab to see the latest documents.");
					Globals.Message.ShowInformation(revertedMessage + deletedMessage + "\r\n\r\n" + eDocsRefreshHint);
				}
			}
		}

		bool GetDocTypeForAPEAndQueryUserIfNotConfigured(out string docTypeForAPE)
		{
			docTypeForAPE = FRCustomsDataRegistry.Instance.DocumentTypeAsAlternateProofOfExit.Value;
			var isConfigured = !string.IsNullOrEmpty(docTypeForAPE);
			if (!isConfigured)
			{
				Globals.Message.ShowError(Res.GetString("9A9A33E0-E845-4858-8032-E4B296ED8565", "No document type is specified as Alternate Proof of Exit, please configure it in registry Customs > France > Exit Control System > Document Type as Alternate Proof of Exit."));
			}
			return isConfigured;
		}

		void SetupEntryLineColumn()
		{
			EntryLineGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("C7B40888-34B7-47B2-A3F7-E911DF3C6CDC", "VAT-able Value"),
					ColumnName = FR.Business.Declaration.CusEntryLine.Schema.CL_ValueForVAT,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				},

				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("1C07C73D-4464-401C-B0B3-A11987CBBA91", "UQ"),
					ColumnName = nameof(CusEntryLine.LocalCurrency),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("1EB20834-A8EF-4B66-A84E-8221C8A6BF3B", "Base VAT-able Value"),
					ColumnName = FR.Business.Declaration.CusEntryLine.Schema.CL_BaseVATableValue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},

				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("BA31FAFE-0466-4BB0-A9DE-1A5841003B46", "UQ"),
					ColumnName = nameof(CusEntryLine.CL_BaseVATableValueUQ),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("126250B8-7E0F-421E-93B1-8F5F2C161D03", "Invoiced Price"),
					ColumnName = FR.Business.Declaration.CusEntryLine.Schema.CL_InvoiceAmount,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},

				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("A7395AC8-4B27-4C6E-B62E-EF1293C81244", "Inv. Price UQ"),
					ColumnName = FR.Business.Declaration.CusEntryLine.Schema.CL_RX_NKInvoiceAmountCurrency,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E6A7B149-B9BB-4B3A-BB24-9A1376966415", "Confirmed Statistical Value"),
					ColumnName = CusEntryLine.Schema.CL_ConfirmedStatisticalValue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},

				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("96229E5C-1996-49F2-A8AC-E7B01617539E", "Confirmed Customs Value"),
					ColumnName = CusEntryLine.Schema.CL_ConfirmedCustomsValue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},

				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("B555AD14-907E-4685-99C1-4DEF09B33F96", "Confirmed VAT-able Value"),
					ColumnName = CusEntryLine.Schema.CL_ConfirmedValueForVAT,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},

				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("F26E25E8-8BFA-492E-A8BD-3CFA1F1335BA", "Guarantee Amount"),
					ColumnName = "CL_Calc_GuaranteeAmount",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true
				}
			});
		}

		void SetEntryAsAmendmentMenuItem_Click(object sender, EventArgs e)
		{
			var entryHeader = EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>().FirstOrDefault();
			if (entryHeader != null)
			{
				if (entryHeader != null && allowedDeltaIEImportCusEntryStatusListForAmendment.Contains(entryHeader.CH_EntryStatus))
				{
					var message = Res.GetString("4322BCA3-B246-49F3-AFBC-8535FADBFEA8", "Are you sure you want to set this Entry as AMENDING?");
					var caption = Res.GetString("C5004C38-CEE8-4E95-88F6-7452D263693E", "Amendment");
					var confirmationPrompt = Res.GetString("D8B88078-4EF4-4060-B1E0-C7B67A6963D4", "If you are absolutely sure you want to set this Entry as AMENDING, please type");
					var confirmationString = Res.GetString("F7F3478C-04DC-4A85-9E7E-FB49DC2429C0", "yes");

					var result = Globals.Message.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, ZMessageBoxIcon.None);
					if (result == ZDialogResult.OK)
					{
						entryHeader.SetEntryAsAmending();
						Globals.Message.Show(Res.GetString("8BDCD0D2-C4E6-428A-9C26-0A220EC6E980", "One Entry was set to Amending"));
					}
				}
				else
				{
					var message = Res.GetString("48D543D5-2DE2-4BDB-B129-69600EDE1386", "Entry status doesn't allow Amendment.");
					Globals.Message.Show(message);
				}
			}
		}

		readonly List<ZString> allowedDeltaIEImportCusEntryStatusListForAmendment = new List<ZString>() { DeltaIEImportCusEntryStatusList.Codes.DeclarationAcceptedMrnAllocated, DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered, DeltaIEImportCusEntryStatusList.Codes.Released };
	}
}
