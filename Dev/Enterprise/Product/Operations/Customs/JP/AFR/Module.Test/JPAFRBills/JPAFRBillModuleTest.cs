using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRBillModule))]
	class JPAFRBillModuleTest : ZModuleBasherTest
	{
		public void TestSupportWorkflow()
		{
			Assert(Module.SupportsWorkflow);
		}

		public void TestDenyNewAndDelete()
		{
			AssertEquals(false, Module.AllowNew);
			AssertEquals(false, Module.AllowDelete);
		}

		#region Implementation

		JPAFRBillModule Module
		{
			get { return module ?? (module = (JPAFRBillModule)ZModuleFactory.Instance.Create(GetModuleID())); }
		}
		JPAFRBillModule module;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.JP.AFRBill;
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			var consol = factory.New<ForwardingConsol>();
			var header = factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.JPH_IsShippingLineEntry = ZBool.False;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "ABCD123456789012";
			factory.Save();
		}

		#endregion
	}
}
