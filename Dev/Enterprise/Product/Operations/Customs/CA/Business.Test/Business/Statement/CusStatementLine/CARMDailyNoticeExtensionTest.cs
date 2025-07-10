using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CARMDailyNoticeExtension))]
	sealed class CARMDailyNoticeExtensionTest : CusSupportingInfoTest<CARMDailyNoticeExtension>
	{
		public void TestProperties()
		{
			var dailyNoticeExtentions = GetBizObjsForCorrectlyTypeDecideTest(Factory).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("CSI_Description", "Test Description", dailyNoticeExtentions.CSI_Description);
				AssertEquals("CSI_Code", "Test Code", dailyNoticeExtentions.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "Test Reference Number", dailyNoticeExtentions.CSI_ReferenceNumber);
				AssertEquals("CSI_Status", "ABC", dailyNoticeExtentions.CSI_Status);
				AssertEquals("CSI_Procedure", "Port1", dailyNoticeExtentions.CSI_Procedure);
				AssertEquals("CSI_Value", 10m, dailyNoticeExtentions.CSI_Value);
			});
		}

		protected override IEnumerable<CARMDailyNoticeExtension> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var statementHeader = factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			var dailyNoticeExtentions = new CARMDailyNoticeExtensionCollection(statementLine).AddNew();
			dailyNoticeExtentions.CSI_Type = Common.CA.CusSupportingInfoTypeList.Codes.CarmDailyNoticeExtension;
			dailyNoticeExtentions.CSI_Description = "Test Description";
			dailyNoticeExtentions.CSI_Code = "Test Code";
			dailyNoticeExtentions.CSI_ReferenceNumber = "Test Reference Number";
			dailyNoticeExtentions.CSI_Status = "ABC";
			dailyNoticeExtentions.CSI_Procedure = "Port1";
			dailyNoticeExtentions.CSI_Value = 10m;

			yield return dailyNoticeExtentions;
		}

		protected override BusinessObject GetNewBusinessObject() => dailyNoticeExtension;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (CARMDailyNoticeExtension)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = Common.CA.CusSupportingInfoTypeList.Codes.CarmDailyNoticeExtension;
			return businessObj;
		}

		protected override void SetUp()
		{
			statementLine = Factory.New<CusStatementHeader>().StatementLines.AddNew();
			var dailyNoticeExtensionCollection = new CARMDailyNoticeExtensionCollection(statementLine);
			dailyNoticeExtension = dailyNoticeExtensionCollection.AddNew();
		}
		CARMDailyNoticeExtension dailyNoticeExtension;
		CusStatementLine statementLine;
	}
}
