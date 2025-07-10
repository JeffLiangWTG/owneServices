using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LPCODeclarationModule))]
	class LPCODeclarationModuleTest : ZModuleBasherTest
	{
		public void TestCheckpoints()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(Env.Security.BRLPCODeclaration, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestSupportWorkflow()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				Assert(module.SupportsWorkflow);
				AssertEquals(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode, module.WorkflowType);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Brazil;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.BR.LPCODeclaration;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = factory.NewWithValidTestData<JobDeclaration>();
			result.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			return result;
		}
	}
}
