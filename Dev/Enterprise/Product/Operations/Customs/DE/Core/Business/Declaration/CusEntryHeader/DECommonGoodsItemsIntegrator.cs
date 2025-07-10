using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Declaration
{
	sealed class DECommonGoodsItemsIntegrator : EuCommonGoodsItemsIntegrator
	{
		public DECommonGoodsItemsIntegrator(EU.Business.Declaration.CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override ICommonGoodsItem ConvertToCommonGoodsItem(Customs.Business.BaseJobComInvoiceLine baseLine)
		{
			var result = base.ConvertToCommonGoodsItem(baseLine);

			var entryNum = result.EntryNumber;
			result.EntryNumber = (entryNum.Class, entryNum.Type, entryNum.Reference,
				baseLine.CusEntryLine?.CL_LineNumber);

			return result;
		}
	}
}
