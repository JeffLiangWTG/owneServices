using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportEntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
	{
		public ExportEntryCreationStrategy(EU.Business.Declaration.JobDeclaration declaration) : base(declaration, ZString.Empty)
		{
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var invoice = invoiceLine.InvoiceHeader;
			var mergeKey = base.GetKeyForHeaderCore(invoiceLine);
			mergeKey.Add(invoice.JZ_IncoTerm);
			mergeKey.Add(invoice.JZ_IncoTermPlace);
			mergeKey.Add(invoice.ZG_AgreedPlaceCode);
			mergeKey.Add(invoice.JZ_AdditionalTerms);
			mergeKey.Add(invoice.JZ_RX_NKInvoice_Currency);

			return mergeKey;
		}

		protected override void ProcessInvoiceHeaderAdditionalInfosForHeaderCore(MergeKey mergeKey, BaseJobComInvoiceLine invoiceLine)
		{
			// no need to do unnecessary iteration of invoice header and then do nothing
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var mergeKey = base.GetKeyForLine(baseInvoiceLine);

			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var invoice = invoiceLine.InvoiceHeader;

			mergeKey.Add(invoice.JZ_ValuationCode);
			mergeKey.Add(invoice.JZ_UCR);
			mergeKey.Add(invoiceLine.JI_CustomsUnitQty);
			var overallPackageType = invoiceLine.OverallPackageType;
			mergeKey.Add(overallPackageType == PackageType.Packed || overallPackageType == PackageType.None ? invoiceLine.PackagesPivot.MergeKey : (ZString)invoiceLine.PK.ToStringKey());

			return mergeKey;
		}

		protected override void AddLineLevelOrganisationMergeKeys(MergeKey result, EU.Business.Declaration.JobComInvoiceHeader invoice, EU.Business.Declaration.JobComInvoiceLine invoiceLine)
		{
			result.Add(GetEffectiveValue(invoiceLine.ExporterAddress?.PK ?? ZGuid.Empty, () => invoice.SupplierAddress?.PK ?? ZGuid.Empty, () => Declaration.SupplierDocumentaryAddress.PK));
			result.Add(GetEffectiveValue(invoiceLine.ConsigneeAddress?.PK ?? ZGuid.Empty, () => invoice.BuyerAddress?.PK ?? ZGuid.Empty, () => Declaration.ImporterDocumentaryAddress.PK));
		}

		T GetEffectiveValue<T>(T invoiceLineValue, Func<T> getInvoiceValue, Func<T> getDeclarationValue)
			where T : IZType
		{
			var result = invoiceLineValue;
			if (result.IsEmpty)
			{
				result = getInvoiceValue();
				if (result.IsEmpty)
				{
					result = getDeclarationValue();
				}
			}
			return result;
		}

		protected override bool ShouldProcessNationalCodesForLineMergeKey => true;

		protected override string[] GetCusSupplyChainActorReferenceKeysCore() => new[]
		{
			CusSupplyChainActorReference.Schema.CFR_Code,
			CusSupplyChainActorReference.Schema.CFR_Reference
		};

		protected override string[] GetAdditionalInfoKeysCore() => new[]
		{
			AdditionalInfo.Schema.CSI_SubType,
			AdditionalInfo.Schema.CSI_Code,
			AdditionalInfo.Schema.CSI_ReferenceNumber,
			AdditionalInfo.Schema.CSI_Description
		};

		protected override string[] GetCusAuthorizationUsageKeysCore() => new[]
		{
			CusAuthorizationUsage.Schema.AGC_Code,
			CusAuthorizationUsage.Schema.AGC_Number
		};

		protected override string[] GetPreviousDocumentHeaderKeysCore() => new[]
		{
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_ReferenceNumber
		};

		protected override string[] GetPreviousDocumentKeysCore() => new[]
		{
			PreviousDocument.Schema.CSI_ItemNumber,
			PreviousDocument.Schema.CSI_PackType,
			PreviousDocument.Schema.CSI_PackQty,
			PreviousDocument.Schema.CSI_UnitOfQuantity,
			PreviousDocument.Schema.CSI_Quantity,
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_ReferenceNumber
		};

		protected override string[] GetSupportingDocumentKeysCore() => new[]
		{
			SupportingDocument.Schema.CSI_ItemNumber,
			SupportingDocument.Schema.CSI_Code,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_AdditionalDescription,
			SupportingDocument.Schema.CSI_DateOfExpiry
		};
	}
}
