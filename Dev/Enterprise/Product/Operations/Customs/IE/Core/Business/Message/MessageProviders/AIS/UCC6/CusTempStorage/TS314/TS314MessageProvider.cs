using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class TS314MessageProvider : TS313And315MessageProvider, ITS314Header
	{
		public TS314MessageProvider(TemporaryStorageMessageSendingObject sendingObject) : base(sendingObject)
		{
			header = Argument.NotNull(sendingObject.Header, nameof(header));
		}
		protected readonly TemporaryStorageHeader header;

		public IDeclaration14 Declaration => CachedValueHelper.GetValue(ref declarationCached, () => Declaration14Provider.New(header));
		CachedValue<IDeclaration14> declarationCached;
	}
}
