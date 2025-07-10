using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class CustomsEntryLineDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryLineDataObjectWriter
	{
		public CustomsEntryLineDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override List<AddInfo> GetEntryLineAddInfoCollection(CusEntryLine entryLineBO)
		{
			var addInfoCollection = base.GetEntryLineAddInfoCollection(entryLineBO);

			if (Business.UniversalReferenceConstants.IsCarmR2)
			{
				addInfoCollection.AddIfMissing(Constants.AddInfoKeys.EntryLine.GoodsShipmentSequence, entryLineBO.CL_GoodsShipmentSequence.ToString());
				addInfoCollection.AddIfMissing(Constants.AddInfoKeys.EntryLine.CommoditySequence, entryLineBO.CL_CommoditySequence.ToString());
			}

			return addInfoCollection;
		}

		protected override List<CusEntryLineFee> GetEntryLineFeeBOs(CusEntryLine entryLineBO)
		{
			if (Business.UniversalReferenceConstants.IsCarmR2)
			{
				return entryLineBO.ConfirmedFees.Cast<CusEntryLineFee>().Concat(entryLineBO.Fees.Cast<CusEntryLineFee>()).ToList();
			}
			return base.GetEntryLineFeeBOs(entryLineBO);
		}
	}
}
