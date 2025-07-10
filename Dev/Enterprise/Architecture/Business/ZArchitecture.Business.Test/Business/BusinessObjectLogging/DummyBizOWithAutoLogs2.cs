using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizOWithAutoLogs2 : DummyEnterpriseBusinessObject
	{
		public DummyBizOWithAutoLogs2(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fAutoLogged = EnterpriseBusinessObject.AutologState.AutoLogged;
			fOnCreateAutoAdminLogCount = 0;
		}

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => fAutoLogged;

		protected override void OnCreateAutoAdminLog()
		{
			fOnCreateAutoAdminLogCount++;
		}

		public int fOnCreateAutoAdminLogCount;
		public EnterpriseBusinessObject.AutologState fAutoLogged;
	}
}
