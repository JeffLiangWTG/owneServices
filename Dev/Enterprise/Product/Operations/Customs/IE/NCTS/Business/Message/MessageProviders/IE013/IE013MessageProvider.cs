using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE013MessageProvider : IE013AndIE015MessageProvider, IIE013Header
	{
		public IE013MessageProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public IIE013TransitOperation TransitOperation => CachedValueHelper.GetValue(ref transitOperationCached, () => new IE013TransitOperationProvider(NctsHeader));
		CachedValue<IIE013TransitOperation> transitOperationCached;
	}
}
