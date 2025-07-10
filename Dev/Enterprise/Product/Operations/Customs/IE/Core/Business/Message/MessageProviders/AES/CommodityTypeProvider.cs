using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class CommodityTypeProvider : ICommodityType
	{
		public CommodityTypeProvider(EntryLineWrapper entryLineWrapper)
		{
			this.entryLineWrapper = entryLineWrapper;
			entryLine = entryLineWrapper.EntryLine;
			invoiceLine = entryLineWrapper.RandomInvoiceLine;
			declaration = entryLineWrapper.Declaration;
		}
		internal readonly EntryLineWrapper entryLineWrapper;
		internal readonly CusEntryLine entryLine;
		internal readonly JobComInvoiceLine invoiceLine;
		internal readonly JobDeclaration declaration;

		public string GoodsDescription => declaration.IsTransitionPeriodAES30 ? entryLine.GoodsDescription.Left(JobComInvoiceLine.JI_DescriptionMaxLength_TransitionPeriodAES30) : entryLine.GoodsDescription.Left(JobComInvoiceLine.JI_DescriptionMaxLength_AISUCC5);
		public string CUSCode => invoiceLine.ZG_CusNumber;

		public ICommodityCode CommodityCode => commodityCode ?? (commodityCode = new CommodityCodeProvider(entryLineWrapper));
		ICommodityCode commodityCode;

		public IReadOnlyCollection<string> DangerousGoods => dangerousGoods ?? (dangerousGoods = invoiceLine.UNDGs.Cast<UNDGDataItem>().Select(x => (string)x.UNDGSubstance?.DG_UNNO).Where(x => !string.IsNullOrEmpty(x)).ToArray());
		IReadOnlyCollection<string> dangerousGoods;
	}
}
