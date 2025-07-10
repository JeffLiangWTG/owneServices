using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Business
{
	[ModuleID(ModuleId.KRCustomsStatement)]
	public class CusStatementHeaderCollection : ActiveBusinessObjectCollection<CusStatementHeader>
	{
		public CusStatementHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusStatementHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
