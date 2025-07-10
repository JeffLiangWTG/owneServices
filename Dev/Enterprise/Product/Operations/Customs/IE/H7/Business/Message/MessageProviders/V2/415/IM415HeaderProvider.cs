using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM415HeaderProvider : IM413_414_415HeaderProvider, IIM415Header
	{
		public IM415HeaderProvider(MessageSendingObject messageSendingObject) : base(messageSendingObject)
		{
		}

		public IOperation ImportOperation => CachedValueHelper.GetValue(ref importOperationCached, () => new IM413_414_415OperationProvider(messageSendingObject, true));
		CachedValue<IOperation> importOperationCached;
	}
}
