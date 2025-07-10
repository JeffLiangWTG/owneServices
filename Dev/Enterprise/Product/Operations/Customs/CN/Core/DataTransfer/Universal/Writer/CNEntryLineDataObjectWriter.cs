using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNEntryLineDataObjectWriter : CustomsEntryLineDataObjectWriter
	{
		public CNEntryLineDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override List<AddInfo> GetEntryLineAddInfoCollection(CusEntryLine entryLineBO)
		{
			var list = base.GetEntryLineAddInfoCollection(entryLineBO) ?? new List<AddInfo>();

			if (entryLineBO is Business.CusEntryLine entryLineCN)
			{
				list.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryLine.NameOfGoods,
					Value = entryLineCN.NameOfGoods
				});
				list.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryLine.GoodsSpecModel,
					Value = entryLineCN.GoodsSpecModel
				});
				list.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryLine.UnitPrice,
					Value = entryLineCN.UnitPrice.ToStringTrimZeros(4)
				});
				list.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryLine.TotalPrice,
					Value = entryLineCN.TotalPrice.ToStringTrimZeros(4)
				});
				list.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryLine.CurrencyCode,
					Value = entryLineCN.CurrencyCode
				});
			}

			return list;
		}
	}
}
