using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestLookups() => CombineAssertions(() =>
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			AssertType<AdditionalInfoLookups>("not ucc6", cusExitReportItemAdditionalInfo.Lookups);

			AssertType<AdditionalInfoUcc6Lookups>("ucc6", Factory.GetUcc6ExitReportItem().AdditionalInfos.AddNew().Lookups);
		});

		public void TestValidation()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			AssertType<AdditionalInfoValidation>(cusExitReportItemAdditionalInfo.Validation);
		}

		public void TestExitReport()
		{
			(var reportItem, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			AssertSame(reportItem, cusExitReportItemAdditionalInfo.CusExitReportItem);
		}

		public void TestCSI_SubType_Caption()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusExitReportItemAdditionalInfo.CSI_SubTypeInfo);
			AssertEquals("Kind", resourceStringData.Caption);
		}

		public void TestCSI_SubType_ReadOnly()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			AssertEquals(true, cusExitReportItemAdditionalInfo.CSI_SubTypeInfo.ReadOnly);
		}

		public void TestCSI_Code_Caption()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusExitReportItemAdditionalInfo.CSI_CodeInfo);
			AssertEquals("Full Type", resourceStringData.Caption);
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusExitReportItemAdditionalInfo.CSI_ReferenceNumberInfo);
			AssertEquals("Reference", resourceStringData.Caption);
		}

		public void TestSetDefaultValues()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			AssertEquals(EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument, cusExitReportItemAdditionalInfo.CSI_SubType);
		}

		public void TestCSI_ItemNumberCaption()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(cusExitReportItemAdditionalInfo.CSI_ItemNumberInfo, null);
			AssertNotNull("ResourceStringData", resourceStringData);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Sequence Number", resourceStringData.Caption);
				AssertEquals("Short Caption", "Seq. No.", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Seq Number", resourceStringData.MediumCaption);
				AssertEquals("Description", "Additional Document Sequence Number", resourceStringData.FullDescription);
			});
		}

		public void TestCSI_StatusCaption()
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(cusExitReportItemAdditionalInfo.CSI_StatusInfo, null);
			AssertNotNull("ResourceStringData", resourceStringData);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Status", resourceStringData.Caption);
				AssertEquals("Short Caption", "Status", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Status", resourceStringData.MediumCaption);
				AssertEquals("Description", "Additional Document Status", resourceStringData.FullDescription);
			});
		}

		public void TestIsUCC6() => CombineAssertions(() =>
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			AssertEquals("Not UCC6", cusExitReportItemAdditionalInfo.IsUCC6, false);

			AssertEquals("No Parent = not UCC6", Factory.New<AdditionalInfo>().IsUCC6, false);

			var additionalInfo = Factory.GetUcc6ExitReportItem().AdditionalInfos.AddNew();
			AssertEquals("UCC6 taken from parent", additionalInfo.IsUCC6, true);
		});

		public void TestStatusIsMissing() => CombineAssertions(() =>
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			cusExitReportItemAdditionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.Missing;
			AssertEquals("Missing", cusExitReportItemAdditionalInfo.StatusIsMissing, true);

			cusExitReportItemAdditionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.Empty;
			AssertEquals("Empty", cusExitReportItemAdditionalInfo.StatusIsMissing, false);

			cusExitReportItemAdditionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			AssertEquals("DifferencesToDeclared", cusExitReportItemAdditionalInfo.StatusIsMissing, false);
		});

		public void TestStatusIsDifferencesToDeclared() => CombineAssertions(() =>
		{
			(_, _, var cusExitReportItemAdditionalInfo) = GetNewBusinessObject(Factory);
			cusExitReportItemAdditionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.Missing;
			AssertEquals("Missing", cusExitReportItemAdditionalInfo.StatusIsDifferencesToDeclared, false);

			cusExitReportItemAdditionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.Empty;
			AssertEquals("Empty", cusExitReportItemAdditionalInfo.StatusIsDifferencesToDeclared, false);

			cusExitReportItemAdditionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			AssertEquals("DifferencesToDeclared", cusExitReportItemAdditionalInfo.StatusIsDifferencesToDeclared, true);
		});

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetNewBusinessObject(factory).cusExitReportItemAdditionalInfo;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).cusExitReportItemAdditionalInfo;

		public static (CusExitReportItem reportItem, CusExitReport report, AdditionalInfo cusExitReportItemAdditionalInfo) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var reportItem, var report, _) = CusExitReportItemTest.GetNewBusinessObject(factory);
			var cusExitReportItemAdditionalInfo = reportItem.AdditionalInfos.AddNew();
			return (reportItem, report, cusExitReportItemAdditionalInfo);
		}
	}
}
