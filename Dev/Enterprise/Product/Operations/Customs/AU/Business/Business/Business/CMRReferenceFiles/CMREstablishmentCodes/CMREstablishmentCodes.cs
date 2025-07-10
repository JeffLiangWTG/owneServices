
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CMREstablishmentCodes.Schema.EC_EstablishmentCode)]
	[DescriptionProperty(CMREstablishmentCodes.Schema.EC_EstablishmentName)]
	public class CMREstablishmentCodes : AutoCMREstablishmentCodes
	{
		public CMREstablishmentCodes(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMREstablishmentCodes New(BusinessObjectFactory factory)
		{
			return factory.New<CMREstablishmentCodes>();
		}
	}
}
