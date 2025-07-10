using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.Business.BondedWarehousingHelper.Constants;

namespace Enterprise.Customs.DE.Business
{
	public sealed partial class InventorySelectionHeaderForIPRDeclarationCreation : InventorySelectionHeaderForDeclarationCreation
	{
		public InventorySelectionHeaderForIPRDeclarationCreation(CreateDeclarationBizObj createDeclarationBizObj)
			: base(createDeclarationBizObj, true)
		{
		}

		public override bool AutoFillOutDrawQuantities => IsInwardProcessingAVABR;

		protected override FilterBusinessObjectDefaults GetFilterDefaultsCore()
		{
			var filters = base.GetFilterDefaultsCore();

			filters.Add(new FilterBusinessObjectDefault("Pick Area Type", "Property", (ZString)AreaTypes.InwardProcessing, isRemovable: false));

			if (IsInwardProcessingAVABR)
			{
				filters.Add(new FilterBusinessObjectDefault("Customs Deadline", "PropertySearch", ModuleDateFilter.SpecifiedDateRange, false));
				filters.Add(new FilterBusinessObjectDefault("Customs Deadline", "Property1", createDeclarationBizObj.CustomsDeadline, false));
				filters.Add(new FilterBusinessObjectDefault("Customs Deadline", "Property2", createDeclarationBizObj.CustomsDeadline, false));
			}

			return filters;
		}

		protected override void ImportInventoriesCore()
		{
			var randomSelectedLine = SelectedLines.Cast<WhsInventoryWrapper>().First();
			var declaration = CreateAndPopulateDeclaration(randomSelectedLine);
			var header = new ImportIPRInventorySelectionHeader(declaration, createDeclarationBizObj);
			header.SelectedLines.AddRange(SelectedLines);
			header.ImportInventories();
			CreateAndPopulateEntryInstruction(declaration, randomSelectedLine.WarehouseAddress.PK, declaration.InvoiceLines.Cast<JobComInvoiceLine>());
			if (IsInwardProcessingAVABR)
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				PopulateValuationDate(declaration);
				CreateEntries(declaration);
			}
			else
			{
				CreateAndPopulateBillsAndPackages(declaration);
				PopulateInvoiceNumbersAndDates(declaration);
			}
			Factory.Save();
			var publishResult = BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration);
			if (!publishResult.IsEmpty)
			{
				declaration.Delete();
				ImportInventoriesResult += publishResult;
			}
			Factory.Save();
		}

		ZString FormatEntryNumber()
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			return ZString.Format("AV-Abrechnung vom {0}", ZDate.Today.ToString("dd.MM.yyyy"));
#pragma warning restore CW1161 // Res.GetString Analyzer
		}

		void PopulateValuationDate(JobDeclaration declaration)
		{
			foreach (var invoice in declaration.Invoices.Cast<JobComInvoiceHeader>())
			{
				invoice.JZ_ValuationDateOverride = createDeclarationBizObj.CustomsDeadline;
			}
		}

		void CreateEntries(JobDeclaration declaration)
		{
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			if (entryHeader != null)
			{
				entryHeader.EntryNumber = FormatEntryNumber();
			}
		}

		bool IsInwardProcessingAVABR => createDeclarationBizObj.DeclarationType == ImportDeclarationTypeList.Codes.AVABR;
	}
}
