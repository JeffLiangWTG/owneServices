using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("To be used in 2nd part of WI00559691")]
	public class CusAuthorisationHelper
	{
		public CusAuthorisationHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public static CusAuthorisationHeader FindAuthorisationHeader(BusinessObjectFactory factory, ZString code, ZGuid owner)
			=> new CusAuthorisationHelper(factory).FindAuthorisationHeader(code, owner);

		public CusAuthorisationHeader FindAuthorisationHeader(ZString code, ZGuid owner)
		{
			CusAuthorisationHeader header = null;

			var headers = factory.Load<CusAuthorisationHeader>(new ZQuery(CusPermitHeaderSchema.CPH_Type, code)
															 .AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, owner));

			if (headers.Length == 1)
			{
				header = headers[0];
			}

			return header;
		}
	}
}
