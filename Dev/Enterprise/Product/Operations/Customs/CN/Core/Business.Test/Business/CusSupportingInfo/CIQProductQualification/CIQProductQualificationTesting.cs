using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CIQProductQualification))]
	class CIQProductQualificationTesting : CusSupportingInfoTest<CIQProductQualification>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((CIQProductQualification)BusinessObject).SupportsNotes);
		}

		public void TestCodeDescription()
		{
			var tetItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			tetItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var testItem = tetItems.InvoiceLine.CIQProductQualifications.AddNew();
			AssertEquals(ZString.Empty, testItem.DocumentName);
			testItem.CSI_Code = "103";
			AssertEquals("直通放行申请", testItem.DocumentName);
			tetItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testItem1 = tetItems.InvoiceLine.CIQProductQualifications.AddNew();
			AssertEquals(ZString.Empty, testItem1.DocumentName);
			testItem1.CSI_Code = "105";
			AssertEquals("兽医(卫生)证书", testItem1.DocumentName);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(factory, () => { });
			var result = testItems.InvoiceLine.CIQProductQualifications.AddNew();
			result.CSI_Code = "325";
			return result;
		}

		protected override IEnumerable<CIQProductQualification> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = CNCusEntryHeaderHelper.SetupCusEntryHeader(factory, () => { })
				.InvoiceLine.CIQProductQualifications.AddNew();
			result.CSI_Code = "325";
			yield return result;
		}

		public void TestSupportsVIN()
		{
			var testItem = (CIQProductQualification)GetNewBusinessObjectForDeleteTest(Factory);
			Assert(!testItem.SupportsVIN);
			testItem.CSI_Code = "408";
			Assert(testItem.SupportsVIN);
			testItem.CSI_Code = "409";
			Assert(testItem.SupportsVIN);
			testItem.CSI_Code = "603";
			Assert(testItem.SupportsVIN);
		}
	}
}
