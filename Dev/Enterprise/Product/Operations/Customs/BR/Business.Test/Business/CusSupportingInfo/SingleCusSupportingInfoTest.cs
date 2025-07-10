using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SingleCusSupportingInfoForTesting))]
	class SingleCusSupportingInfoTest : Customs.Business.Testing.CusSupportingInfoTest<CusSupportingInfo>
	{
		public void TestOnSaving()
		{
			var supportingInfo = Factory.New<SingleCusSupportingInfoForTesting>();
			Factory.Save();
			AssertEquals("Should not be saved if empty", false, supportingInfo.IsInDatabase);
			supportingInfo.CSI_Code = "1";
			supportingInfo.CSI_Type = "111";
			supportingInfo.CSI_ParentTableCode = "CSI";
			Factory.Save();
			AssertEquals("Should be saved if not empty", true, supportingInfo.IsInDatabase);
			supportingInfo.CSI_Code = ZString.Empty;
			supportingInfo.CSI_Type = ZString.Empty;
			supportingInfo.CSI_ParentTableCode = "CSI";
			Factory.Save();
			AssertEquals("Should be deleted if not empty", true, supportingInfo.IsDeleted);
		}

		public void TestIsSavedByFactory()
		{
			var supportingInfo = (SingleCusSupportingInfoForTesting)GetNewBusinessObject();
			AssertEquals("IsSavedByFactory", true, supportingInfo.IsSavedByFactory);
			AssertEquals("IsEmpty", false, supportingInfo.IsEmpty);

			supportingInfo.CSI_Code = "1";
			AssertEquals("IsSavedByFactory when CSI_Code has value", true, supportingInfo.IsSavedByFactory);
			AssertEquals("IsEmpty when CSI_Code has value", false, supportingInfo.IsEmpty);
			supportingInfo.CSI_Code = "";
			supportingInfo.CSI_Quantity = 1m;
			AssertEquals("IsSavedByFactory when CSI_Quantity has value", true, supportingInfo.IsSavedByFactory);
			AssertEquals("IsEmpty when CSI_Quantity has value", false, supportingInfo.IsEmpty);
			supportingInfo.CSI_Quantity = 0m;
			supportingInfo.CSI_DateOfIssue = ZDate.Today;
			AssertEquals("IsSavedByFactory when CSI_DateOfIssue has value", true, supportingInfo.IsSavedByFactory);
			AssertEquals("IsEmpty when CSI_DateOfIssue has value", false, supportingInfo.IsEmpty);
			supportingInfo.CSI_DateOfIssue = ZDate.Empty;
			supportingInfo.CSI_LineNo = 1;
			AssertEquals("IsSavedByFactory when CSI_LineNo has value", true, supportingInfo.IsSavedByFactory);
			AssertEquals("IsEmpty when CSI_LineNo has value", false, supportingInfo.IsEmpty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var supportingInfo = Factory.New<SingleCusSupportingInfoForTesting>();
			supportingInfo.CSI_Code = "1";
			supportingInfo.CSI_Type = "111";
			supportingInfo.CSI_ParentTableCode = "1";
			return supportingInfo;
		}

		protected override IEnumerable<CusSupportingInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var supportingInfo = factory.New<SingleCusSupportingInfoForTesting>();
			supportingInfo.CSI_Code = "1";
			supportingInfo.CSI_Type = "111";
			supportingInfo.CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			supportingInfo.CSI_DataModel = "BR";
			yield return supportingInfo;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}
	}

	class SingleCusSupportingInfoForTesting : SingleCusSupportingInfo
	{
		public SingleCusSupportingInfoForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IEnumerable<ZPropertyInfo> UsedFieldsInfos => GetUsedFieldsInfos();

		public override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_QuantityInfo;
			yield return CSI_DateOfIssueInfo;
			yield return CSI_LineNoInfo;
		}
	}
}
