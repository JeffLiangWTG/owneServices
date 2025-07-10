using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business
{
	public struct AEODocumentData
	{
		public CusAuthorisationHeader Authorisation { get; set; }
		public ZBool ShouldRemoveDoc { get; set; }
	}
}
