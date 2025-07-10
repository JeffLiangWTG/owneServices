using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ISingleLineEntryManager
	{
		void Execute();
		SingleLineEntry SingleLineEntry { get; }
		JobDeclaration Declaration { get; }
		bool ExecutedSuccessfully { get; }
	}

	public class SingleLineEntryManager : ISingleLineEntryManager
	{
		public bool ExecutedSuccessfully { get; protected set; }

		public SingleLineEntryManager(JobDeclaration declaration, ISingleLineEntryProvider singleLineEntryProvider = null)
		{
			var provider = singleLineEntryProvider ?? new SingleLineEntryProvider();

			Declaration = Argument.NotNull(declaration, nameof(declaration));

			SingleLineEntry = provider.GetSingleLineEntry(declaration);

			var shipment = Declaration.Shipment;
			if (shipment != null)
			{
				SingleLineEntry.Currency = shipment.GoodsValueCurrencyPK;
			}
			if (SingleLineEntry.Currency.IsEmpty)
			{
				if (Declaration.SupplierImporterLink is OrgSupplierBuyerLink link)
				{
					SingleLineEntry.Currency = link.DefaultCurrency?.PK ?? ZGuid.Empty;
				}
			}

			using (SingleLineEntry.SuspendSettingHasChanges())
			{
				SingleLineEntry.CPCCode = provider.DefaultCPCCode(Declaration);
				if (shipment != null)
				{
					SingleLineEntry.Price = shipment.JS_GoodsValue;
				}
				// To do - one day capture pieces (for box 6 and box 31) etc
			}
		}

		public JobDeclaration Declaration { get; }
		public SingleLineEntry SingleLineEntry { get; }

		public void Execute()
		{
			Declaration.Invoices.DeleteAll();
			Declaration.InvoiceLines.RemoveAndDeleteAll();

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			ExecuteCore(invoice, invoiceLine);

			IBusiness declaration = Declaration;
			declaration.RunPreSaveValidationFetch(true);
			declaration.RunPreSaveValidation();
			ExecutedSuccessfully = true;
		}

		protected virtual void ExecuteCore(JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			invoice.JZ_InvoiceNumber = SingleLineEntry.InvoiceNumber;
			invoice.JZ_InvoiceAmount = SingleLineEntry.Price;
			RefCurrency currency = invoice.Factory.Load<RefCurrency>(SingleLineEntry.Currency);
			if (currency != null)
			{
				invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			}

			invoiceLine.JI_Tariff = SingleLineEntry.TariffNumber;
			invoiceLine.JI_Procedure = SingleLineEntry.CPCCode;
			invoiceLine.JI_Description = Declaration.JE_GoodsDescription;
			invoiceLine.JI_Weight = Declaration.JE_TotalWeight;
			invoiceLine.JI_WeightUQ = Declaration.JE_TotalWeightUnit;
			invoiceLine.JI_NetWeight = SingleLineEntry.NetWeight;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_LinePrice = SingleLineEntry.Price;

			if (Declaration.Bills.Count == 0)
			{
				var bill = Declaration.Bills.AddNew();
				bill.CU_BillNum = (NoResString)"Bill";
			}
			if (Declaration.Bills[0].PackingGroups.Count == 0)
			{
				Declaration.Bills[0].PackingGroups.AddNew();
			}
			var pack = Declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			pack.CW_PackQty = Declaration.JE_TotalNoOfPacks;

			var refPacks = CusRefPacksHelper.LoadFilteredRefPacks(Declaration.Factory, Declaration.CountryCode, RPTypeList.Codes.CommercialInvoice, Declaration.JE_TotalNoOfPacksPackType, true, 1);

			if (!refPacks.IsNullOrEmpty())
			{
				pack.CW_PackType = refPacks[0].RP_CustomsPack.Left(3);
			}

			var shipment = Declaration.Shipment;
			if (shipment != null)
			{
				pack.CW_MarksAndNos = shipment.JS_MarksAndNumbersShort;
			}
			var piv = invoiceLine.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>().FirstOrDefault(p => p.CHC_CW == pack.PK);
			if (piv == null)
			{
				piv = invoiceLine.PackagesPivot.AddNew();
				piv.CHC_CW = pack.PK;
				piv.CHC_JE = Declaration.PK;
				piv.CHC_JI = invoiceLine.PK;
			}
			piv.CHC_NumberOfPacks = pack.CW_PackQty;

			if (SingleLineEntry.CreateLIC99)
			{
				var addInfos = invoiceLine.AdditionalInfos.AddNew();
				addInfos.CSI_Code = "LIC99";
			}
			if (!SingleLineEntry.AdditionalInfoCode.IsEmpty)
			{
				var addInfos = invoiceLine.AdditionalInfos.AddNew();
				addInfos.CSI_Code = SingleLineEntry.AdditionalInfoCode;
			}
		}
	}
}
