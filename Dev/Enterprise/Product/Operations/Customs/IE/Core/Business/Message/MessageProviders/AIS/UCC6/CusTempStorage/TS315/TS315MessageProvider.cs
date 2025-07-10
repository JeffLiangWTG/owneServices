using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class TS315MessageProvider : TS313And315MessageProvider, ITS315Header
	{
		public TS315MessageProvider(TemporaryStorageMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		public IDeclaration07 Declaration => CachedValueHelper.GetValue(ref declarationCached, () => Declaration07Provider.New(temporaryStorageHeader));
		CachedValue<IDeclaration07> declarationCached;
	}
}
