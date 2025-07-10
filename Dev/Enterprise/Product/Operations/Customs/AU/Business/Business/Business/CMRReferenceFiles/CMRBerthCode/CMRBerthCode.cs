
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(AutoCMRBerthCode.Schema.BC_BerthCode), DescriptionProperty(AutoCMRBerthCode.Schema.BC_BerthCodeName)]
	public class CMRBerthCode : AutoCMRBerthCode
	{
		public CMRBerthCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRBerthCode New(BusinessObjectFactory factory)
		{
			return factory.New<CMRBerthCode>();
		}
	}
}
