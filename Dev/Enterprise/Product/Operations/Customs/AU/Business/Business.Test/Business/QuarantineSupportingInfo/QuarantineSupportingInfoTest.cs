using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineSupportingInfo))]
	sealed class QuarantineSupportingInfoTest : CusSupportingInfoTest<QuarantineSupportingInfo>
	{
		public void TestDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("NDECT", "NDECT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NDECT", "Test", "Test1234", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var supportingInfo = invoiceHeader.QuarantineExDocHeader.SupportingInfos.AddNew();
			supportingInfo.CSI_Description = "Test";
			AssertEquals("Test1234", supportingInfo.Description);
		}

		protected override IEnumerable<QuarantineSupportingInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			yield return invoiceHeader.QuarantineExDocHeader.SupportingInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<QuarantineSupportingInfo>();
	}
}
