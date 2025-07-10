using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXAESExportOperationWrapper : AESCommonExportOperationMRNWrapper, IComplXAESExportOperation
	{
		public ComplXAESExportOperationWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
			invoiceHeader = entryHeader.RandomHeader;
		}
		readonly JobComInvoiceHeader invoiceHeader;

		public ZDecimal TotalAmount
		{
			get
			{
				if (totalAmount == null)
				{
					totalAmount = new CachedProperty<ZDecimal>(entryHeader.Factory, () =>
					{
						return AESWrappersHelper.GetTotalAmount(invoiceHeader, entryHeader);
					});
				}
				return totalAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalAmount;

		public ZString Currency => AESWrappersHelper.GetCurrency(invoiceHeader);
	}
}
