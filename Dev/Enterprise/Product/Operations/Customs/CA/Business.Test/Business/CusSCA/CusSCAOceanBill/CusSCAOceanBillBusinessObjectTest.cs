using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBill))]
	sealed class CusSCAOceanBillBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<CusSCAOceanBill>();
		}
		#endregion

		public void TestOriginalCCNMaxLength()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.OriginalCCN = "12345678901234567890123456";
			AssertEquals("Original CCN truncated and no exception", "1234567890123456789012345", oceanBill.OriginalCCN);
			oceanBill.OriginalCCN = "1234";
			AssertEquals("1234", oceanBill.OriginalCCN);
		}

		public void TestAllowCrossCompany()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_GB = branch.PK;
			Assert(!oceanBill.CB_GBInfo.HasError(string.Format("Please select a branch of {0}.", GlbCompany.CurrentCompany.GC_Name)));
		}

		public void TestApplicationCodes()
		{
			List<ZString> applicationCodes = new List<ZString>(CusSCAOceanBill.ApplicationCodes);
			Assert("Application codes, sea", applicationCodes.Contains(Enterprise.Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea));
			Assert("Application codes, air", applicationCodes.Contains(Enterprise.Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir));
			Assert("Application codes, rail", applicationCodes.Contains(Enterprise.Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail));
			Assert("Application codes, road", applicationCodes.Contains(Enterprise.Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad));
		}

		public void TestIsAirEtc()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			RunOneIsTest(oceanBill, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir, true, false, false, false);
			RunOneIsTest(oceanBill, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, false, true, false, false);
			RunOneIsTest(oceanBill, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad, false, false, true, false);
			RunOneIsTest(oceanBill, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail, false, false, false, true);
		}
		void RunOneIsTest(CusSCAOceanBill oceanBill, string appCode, bool expectedIsAir, bool expectedIsSea, bool expectedIsRoad, bool expectedIsRail)
		{
			oceanBill.CB_ApplicationCode = appCode;
			AssertEquals("IsAir with " + appCode, expectedIsAir, oceanBill.IsAir);
			AssertEquals("IsSea with " + appCode, expectedIsSea, oceanBill.IsSea);
			AssertEquals("IsRoad with " + appCode, expectedIsRoad, oceanBill.IsRoad);
			AssertEquals("IsRail with " + appCode, expectedIsRail, oceanBill.IsRail);
		}

		public void TestFindContainerByNumber()
		{
			CusSCATestHelper helper = new CusSCATestHelper();
			ForwardingContainer container1 = helper.Container1;
			ForwardingContainer container2 = helper.Container2;
			ForwardingContainer container3 = helper.Container3;
			helper.OceanBill.EnableAndSynchronise(true);
			AssertEquals("FindContainerByNumber1", CusSCATestHelper.Container1Num, helper.OceanBill.FindContainerByNumber(CusSCATestHelper.Container1Num).CN_ContainerNumber);
			AssertEquals("FindContainerByNumber2", CusSCATestHelper.Container2Num, helper.OceanBill.FindContainerByNumber(CusSCATestHelper.Container2Num).CN_ContainerNumber);
			AssertEquals("FindContainerByNumber3", CusSCATestHelper.Container3Num, helper.OceanBill.FindContainerByNumber(CusSCATestHelper.Container3Num).CN_ContainerNumber);
		}

		public void TestGetNewCusSCAOceanBillProcessTaskCollection()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>", typeof(ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>), ((IWorkflowProvider)oceanBill).WorkflowItems);
		}

		[TestedType(typeof(CusSCAOceanBill))]
		class CusSCAOceanBillWorkflowProviderTest : WorkflowProviderTest<CusSCAOceanBill, ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>>
		{
			#region Implementation

			protected override ZString ExpectedWorkflowType
			{
				get { return WorkflowDescriptors.CusSCAOceanBillDescriptorCode; }
			}

			protected override CusSCAOceanBill GetNewBusinessObject(BusinessObjectFactory factory)
			{
				return CusSCAOceanBill;
			}

			CusSCAOceanBill CusSCAOceanBill
			{
				get { return cusSCAOceanBill ?? (cusSCAOceanBill = Factory.New<CusSCAOceanBill>()); }
			}
			CusSCAOceanBill cusSCAOceanBill;

			protected override IWorkflowProvider ReloadWorkflowProvider(BusinessObjectFactory factory, CusSCAOceanBill workFlowProvider)
			{
				var reloadedCusSCAOceanBill = factory.Load<CusSCAOceanBill>(workFlowProvider.PK);
				return reloadedCusSCAOceanBill;
			}
			#endregion
		}
	}
}
