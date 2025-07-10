using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class FTALineWrapper : NonPersistentBusinessObject
	{
		public FTALineWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public IImportFTALine FirstLine { get; set; }
		public IImportFTALine SecondLine { get; set; }
		public IImportFTALine ThirdLine { get; set; }
	}
}
