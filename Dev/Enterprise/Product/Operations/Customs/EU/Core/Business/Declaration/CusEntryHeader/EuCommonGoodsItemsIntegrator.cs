using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Common;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EuCommonGoodsItemsIntegrator : CommonGoodsItemsIntegrator
	{
		public EuCommonGoodsItemsIntegrator(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override ICommonGoodsItem ConvertToCommonGoodsItem(Customs.Business.BaseJobComInvoiceLine baseLine)
		{
			var result = base.ConvertToCommonGoodsItem(baseLine);
			if (baseLine is JobComInvoiceLine line)
			{
				result.DispatchCountry = line.ZG_CountryOfSupply;
				result.DestinationCountry = line.ZG_CountryOfDestination;
				result.SupplementaryQuantity = line.JI_CustomsSecondQuantity;
				result.SupplementaryQuantityUnit = line.JI_CustomsSecondUnitQty;

				var entryNumber = CusEntryNumber.Load(line.EntryInstruction.EntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, line.Declaration.CountryCode);
				if (entryNumber != null)
				{
					result.EntryNumber = line.GetEntryNumberFormatter().FormatEntryNumber(
						entryNumber, line.EntryInstruction.CEI_Style);
				}
			}
			return result;
		}

		protected override void CopyFromCommonGoodsItem(ICommonGoodsItem source, Customs.Business.BaseJobComInvoiceLine baseLine)
		{
			base.CopyFromCommonGoodsItem(source, baseLine);

			if (baseLine is JobComInvoiceLine line)
			{
				line.ZG_CountryOfSupply = source.DispatchCountry.SubstringSafe(0, line.ZG_CountryOfSupplyInfo.MaxLength);
				line.ZG_CountryOfDestination = source.DestinationCountry.SubstringSafe(0, line.ZG_CountryOfDestinationInfo.MaxLength);

				if (!source.EntryNumber.Reference.IsEmpty)
				{
					var targetMrnDocument = line.PreviousDocuments.AddNew();
					targetMrnDocument.CSI_Code = source.EntryNumber.Type;
					targetMrnDocument.CSI_SubType = source.EntryNumber.Class;
					targetMrnDocument.CSI_ReferenceNumber = source.EntryNumber.Reference;
				}

				var targetDocJobDocument = line.PreviousDocuments.AddNew();
				targetDocJobDocument.CSI_Code = SupportingDocumentTypes.Other;
				targetDocJobDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
				targetDocJobDocument.CSI_ReferenceNumber = source.JobReference;
			}
		}
	}
}
