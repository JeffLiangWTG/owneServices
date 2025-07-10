using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.JI_CC);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddRequestTTCEReferenceFileMenuToGrid();
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => true;

		protected override void AddNewColumnForCustomsInvoiceLinesBoundGrid()
		{
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber, 80);
			if (!(JobDeclaration is JobDeclaration declaration && declaration.IsPersistent))
			{
				CreateNewCheckBoxColumn(JobComInvoiceLine.Schema.JI_RequiresImportLicense, 80, defaultColumn: false);
			}
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_ManufacturerIndicator, 120);
			CreateManufacturerAddressColumns();
			CreateManufacturerColumn();

			var groupCatalog = Res.GetData("BD9204BD-596D-4C94-9C55-7709AE95C6C2", "Catalog");
			if (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.Value)
			{
				CreateNewGuidFindBoxColumn(JobComInvoiceLine.Schema.JI_CGC_Catalog, 90, ModuleIDs.Customs.GoodsCatalog, groupCatalog);
				CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_CatalogAuthorityIdentifier, 180, groupName: groupCatalog);
				CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_CatalogAuthorityVersion, 180, groupName: groupCatalog);
			}

			var groupFMMBenefit = Res.GetData("0f55d2c8-2521-425c-b2d5-76243f615eda", "FMM Benefit");
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.FMMBenefit, 80, groupName: groupFMMBenefit);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.FMMBenefitDescription, 130, groupName: groupFMMBenefit).CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;

			CreateNewDropEditColumn(JobComInvoiceLine.Schema.ICMSTaxRegime, 120);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.ICMSLegalBase, 120);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.JI_ICMSRate, 90);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.JI_ICMSBaseValueReductionPercentage, 120, defaultColumn: false);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_ICMSFormula, 120, defaultColumn: false);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.ICMSFCPRateValue, 90, defaultColumn: false);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.JI_ICMSTotalAmountReductionPercentage, 120, defaultColumn: false);
		}

		void CreateManufacturerColumn()
		{
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_ManufacturerAuthorityIdentifier, 120, defaultColumn: true, groupName: ManufacturerCaption);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_ManufacturerAuthorityVersion, 120, defaultColumn: true, groupName: ManufacturerCaption);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.ManufacturerName, 120, defaultColumn: true);
		}

		protected override IEnumerable<string> GetDefaultColumnsInOrderCore()
		{
			var columns = new List<string>
			{
				JobComInvoiceLine.Schema.JI_LineNo,
				JobComInvoiceLine.Schema.JI_Calc_Invoice,
				JobComInvoiceLine.Schema.JI_PartNo,
			};

			if (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.Value)
			{
				columns.Add(JobComInvoiceLine.Schema.JI_CGC_Catalog);
				columns.Add(JobComInvoiceLine.Schema.JI_CatalogAuthorityIdentifier);
				columns.Add(JobComInvoiceLine.Schema.JI_CatalogAuthorityVersion);
			}

			columns.AddRange(new[]
			{
				JobComInvoiceLine.Schema.JI_Tariff,
				JobComInvoiceLine.Schema.JI_InvoiceQuantity,
				JobComInvoiceLine.Schema.JI_CustomsQuantity,
				JobComInvoiceLine.Schema.JI_LinePrice,
				JobComInvoiceLine.Schema.FullGoodsDescription,
				JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
				JobComInvoiceLine.Schema.JI_Weight,
				JobComInvoiceLine.Schema.JI_NetWeight,
				JobComInvoiceLine.Schema.JI_Volume,
				JobComInvoiceLine.Schema.JI_OrderNumber,
				JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
				JobComInvoiceLine.Schema.UnitPrice,
				JobComInvoiceLine.Schema.JI_SerialNumber,
				JobComInvoiceLine.Schema.JI_ManufacturerIndicator,
				JobComInvoiceLine.Schema.ManufacturerOrgPK,
				JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress,
				JobComInvoiceLine.Schema.JI_CountryOfOrigin,
				JobComInvoiceLine.Schema.JI_ManufacturerAuthorityVersion,
				JobComInvoiceLine.Schema.JI_ManufacturerAuthorityIdentifier,
				JobComInvoiceLine.Schema.ManufacturerName,
				JobComInvoiceLine.Schema.FMMBenefit,
				JobComInvoiceLine.Schema.FMMBenefitDescription,
				JobComInvoiceLine.Schema.JI_CEI,
				JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber,
				JobComInvoiceLine.Schema.ICMSTaxRegime,
				JobComInvoiceLine.Schema.ICMSLegalBase,
				JobComInvoiceLine.Schema.JI_ICMSRate,
				JobComInvoiceLine.Schema.JI_ICMSBaseValueReductionPercentage,
				JobComInvoiceLine.Schema.JI_ICMSFormula,
				JobComInvoiceLine.Schema.ICMSFCPRateValue,
				JobComInvoiceLine.Schema.JI_ICMSTotalAmountReductionPercentage
			});

			return columns;
		}

		#region Request TTCE Reference File Menu

		void AddRequestTTCEReferenceFileMenuToGrid()
		{
			if (JobDeclaration is JobDeclaration declaration && declaration.IsPersistent)
			{
				CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add("-");
				CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("28020433-9697-4346-B25C-F9938110139A", "Request TTCE Reference File"), RequestTTCEReferenceFile));
			}
		}

		void RequestTTCEReferenceFile(object sender, EventArgs args)
		{
			var invoiceLines = CustomsInvoiceLinesBoundGrid.GetSelectedRows().Cast<JobComInvoiceLine>().ToArray();
			var declaration = JobDeclaration as JobDeclaration;
			if (invoiceLines.Length == 0)
			{
				Globals.Message.Show(ResString.GetMultilingualString("71CEA46B-B72E-46AA-B038-D6F906A30E67", "No rows selected."));
			}
			else if (!declaration.BrokerCertificate?.IsValidCertificate ?? true)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("5FC26A24-4617-467B-8CA4-9A44317E7AF4", "Digital Certificate not found, expired, or invalid."));
			}
			else
			{
				var messageCount = new ImportTaxTreatmentsBatchMessageSender(invoiceLines).SendMessagesAndSave();
				if (messageCount > 0)
				{
					Globals.Message.Show(ResString.GetMultilingualString("2D39B6D8-7B6F-4E39-8A86-679C9D4287F0", "{0} TTCE message(s) have been sent.", messageCount));
				}
				else
				{
					Globals.Message.Show(ResString.GetMultilingualString("0CE50D0B-A6B0-42F6-89F0-409E2D1127DC", "None of the invoice lines selected attend the criteria: Tariff and Country of Origin informed."));
				}
			}
		}

		#endregion
	}
}
