using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(ForeignOperatorModule))]
	class ForeignOperatorModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestElementType()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				var elementType = module.GetElementType();
				AssertEquals(nameof(CusBRForeignOperator), elementType.Name);
			}
		}
		public void TestAllowEdit()
		{
			using (var module = new ForeignOperatorModule())
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new ForeignOperatorModule())
			{
				AssertEquals(true, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			using (var module = new ForeignOperatorModule())
			{
				AssertEquals(true, module.AllowView);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new ForeignOperatorModule())
			{
				AssertEquals(true, module.AllowDelete);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new ForeignOperatorModule())
			{
				AssertEquals(Env.Security.BRForeignOperator, module.SecurityCheckpoint);
			}
		}

		public void TestCheckpoints()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestWorkflowType()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(WorkflowDescriptors.ForeignOperatorWorkflowDescriptorCode, module.WorkflowType);
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				Assert("SupportsWorkflow should be True", module.SupportsWorkflow);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var foreignOperator = factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_AuthorityIdentifier = "123";
			foreignOperator.BFR_AuthorityVersion = "123";
			return foreignOperator;
		}

		protected sealed override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.BR.ForeignOperator;

		protected override string CountryCode => Core.Constants.CountryCodes.Brazil;
	}
}
