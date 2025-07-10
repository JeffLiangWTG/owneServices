using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LPCOModule))]
	sealed class LPCOModuleTest : ZModuleBasherTest
	{
		public void TestGetNewController()
		{
			using (var module = new LPCOModule())
			{
				AssertType<LPCOController>($"Should load {typeof(LPCOController).FullName} in BR.", module.GetNewController());
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var module = new LPCOModule())
			{
				Assert("SupportsWorkflow should be True", module.SupportsWorkflow);
				AssertEquals("WorkflowType should be True", WorkflowDescriptors.CusBRLPCOHeaderWorkflowDescriptorCode, module.WorkflowType);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new LPCOModule())
			{
				AssertEquals(Env.Security.BRLPCO, module.SecurityCheckpoint);
			}
		}

		public void TestAllows()
		{
			using (var module = new LPCOModule())
			{
				Assert("AllowNew must be TRUE", module.AllowNew);
				Assert("AllowEdit must be TRUE", module.AllowEdit);
				Assert("AllowView must be TRUE", module.AllowView);
				Assert("AllowUniversalCopy must be FALSE", !module.AllowUniversalCopy);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Brazil;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.BR.LPCO;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = factory.NewWithValidTestData<CusLPCOHeader>();
			result.CPH_Type = PermitTypeList.Codes.LPC;
			return result;
		}
	}
}
