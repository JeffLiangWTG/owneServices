using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE015TransitOperationProvider : IE013AndIE015TransitOperationProvider, IIE015TransitOperation
	{
		public IE015TransitOperationProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}
	}
}
