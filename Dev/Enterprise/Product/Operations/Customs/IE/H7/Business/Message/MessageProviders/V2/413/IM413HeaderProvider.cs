using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM413HeaderProvider : IM413_414_415HeaderProvider, IIM413Header
	{
		public IM413HeaderProvider(MessageSendingObject messageSendingObject) : base(messageSendingObject)
		{
		}

		public IIM413Operation ImportOperation => CachedValueHelper.GetValue(ref importOperationCached, () => new IM413OperationProvider(messageSendingObject));
		CachedValue<IIM413Operation> importOperationCached;
	}
}
