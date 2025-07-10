using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE015MessageProvider : IE013AndIE015MessageProvider, IIE015Header
	{
		public IE015MessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public IIE015TransitOperation TransitOperation => CachedValueHelper.GetValue(ref transitOperationCached, () => new IE015TransitOperationProvider(NctsHeader));
		CachedValue<IIE015TransitOperation> transitOperationCached;
	}
}
