using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, declaration.JE_MessageType)
		{
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			var invoiceHeader = invoiceLine.InvoiceHeader;
			if (invoiceHeader != null)
			{
				result.Add(invoiceHeader.JZ_IncoTerm);
				result.Add(invoiceHeader.JZ_RX_NKInvoice_Currency);
				result.Add(invoiceHeader.JZ_OH_Supplier);
				result.Add(invoiceHeader.JZ_OH_Buyer);
			}

			return result;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = new MergeKey();
			result.Add(baseInvoiceLine.JI_OP);
			result.Add(baseInvoiceLine.JI_Description);
			if (baseInvoiceLine.IsExtendedCommercialDescriptionEnabled)
			{
				result.Add(baseInvoiceLine.JI_ExtraInfoForClassification);
			}
			result.Add(baseInvoiceLine.JI_Tariff);
			result.Add(baseInvoiceLine.JI_Procedure);
			result.Add(baseInvoiceLine.JI_CustomsUnitQty);
			result.Add(baseInvoiceLine.JI_CountryOfOrigin);
			result.Add(baseInvoiceLine.JI_PreviousEntryNumber);
			result.Add(baseInvoiceLine.JI_PreviousEntryLineNumber);
			result.Add(baseInvoiceLine.JI_PrimaryPreference);
			result.Add(baseInvoiceLine.JI_CustomsSecondUnitQty);
			result.Add(baseInvoiceLine.JI_CustomsThirdUnitQty);
			result.Add(baseInvoiceLine.JI_BondedWhsUnitQty);
			result.Add(baseInvoiceLine.JI_ZZF_NKTaxType);

			ProcessSupportingDocsForLineMergeKey(result, baseInvoiceLine);
			return GetKeyForHeader(baseInvoiceLine) + result;
		}

		void ProcessSupportingDocsForLineMergeKey(MergeKey mergeKey, BaseJobComInvoiceLine invoiceLine)
		{
			var documents = ((JobComInvoiceLine)invoiceLine).SupportingDocuments;
			var keys = documents.Cast<SupportingDocument>().Select(x => $"{x.CSI_Code}_{x.CSI_ReferenceNumber}").OrderBy(x => x).ToArray(); // Merge Key
			mergeKey.Add(new ZString($"{keys.Length}-{string.Join("|", keys)}")); // Merge Key
		}
	}
}
