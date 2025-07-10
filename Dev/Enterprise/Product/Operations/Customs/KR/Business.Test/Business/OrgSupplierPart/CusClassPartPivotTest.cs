using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusKRClassificationForCusClassPartPivot()
		{
			var pivot = (CusClassPartPivot)GetNewBusinessObject();
			AssertNotNull(pivot.KRClassification);
			pivot.Delete();
			Assert("Should be deleted together", pivot.KRClassification.IsDeleted);
		}

		public void TestGAApprovalCollectionOfCusClassPartPivot()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals(1, pivot.GetType().FindInterfaces(new System.Reflection.TypeFilter((type, criteria) => type.ToString().Equals(criteria.ToString())), "Enterprise.Customs.KR.Business.ILineOrProduct").Length);
			AssertEquals(pivot, pivot.GAApprovalDataCollection.Master);
		}
		public void TestIsExportOfCusClassPartPivot()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert(pivot.IsExport);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert(!pivot.IsExport);
		}

		public void TestLineOrProductIsValidationEnabled()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			Assert(((ILineOrProduct)pivot).IsValidationEnabled);
		}

		public void TestRenewGAApprovalDataCollectionByTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", ZZ.RefCusConditionType.OGA, "Other Government Agency Requirement Details");
			var regulationNumber = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGARegulationNumber, "OGA Regulation Number");
			var documentName = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGADocumentName, "OGA Document Name");
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101211000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "5305003000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff2, "71", "수출허가서", refCusConditionType, regulationNumber, documentName, true, false);

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9305101010", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "34", "수출허가서", refCusConditionType, regulationNumber, documentName, true, false);
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "55", "수출허가증", refCusConditionType, regulationNumber, documentName, true, false);

			var tariff4 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "4203301030", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff4, "71", "수출허가서", refCusConditionType, regulationNumber, documentName, true, false);

			var tariff5 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101291000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff5, "13", "검역증명서", refCusConditionType, regulationNumber, documentName, false, true);
			Factory.Save();

			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "TEST";
			var pivot = orgSupplierPart.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(0, pivot.GAApprovalDataCollection.Count);

			CombineAssertions("If the message type is export and OGA data is present, the collection is populated.", () =>
			{
				pivot.CI_TariffNum = tariff1.ZZ1_TariffCode;
				AssertEquals("This HS has not OGA data.", 0, pivot.GAApprovalDataCollection.Count);

				pivot.CI_TariffNum = tariff2.ZZ1_TariffCode;
				AssertEquals(1, pivot.GAApprovalDataCollection.Count);
				AssertEquals("71", pivot.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", pivot.GAApprovalDataCollection[0].CSI_Description);

				pivot.CI_TariffNum = tariff3.ZZ1_TariffCode;
				AssertEquals(2, pivot.GAApprovalDataCollection.Count);
				AssertEquals("34", pivot.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", pivot.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("55", pivot.GAApprovalDataCollection[1].CSI_Procedure);
				AssertEquals("수출허가증", pivot.GAApprovalDataCollection[1].CSI_Description);

				pivot.CI_TariffNum = tariff4.ZZ1_TariffCode;
				AssertEquals("This HS has not OGA valid data.", 0, pivot.GAApprovalDataCollection.Count);

				pivot.CI_TariffNum = tariff5.ZZ1_TariffCode;
				AssertEquals("This HS has not OGA valid data.", 0, pivot.GAApprovalDataCollection.Count);

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = tariff2.ZZ1_TariffCode;
				AssertEquals("It is not imported because the message type is import.", 0, pivot.GAApprovalDataCollection.Count);

				pivot.CI_TariffNum = tariff5.ZZ1_TariffCode;
				AssertEquals("It is not imported because the message type is import.", 0, pivot.GAApprovalDataCollection.Count);
			});

			CombineAssertions("When the user enters data and the HS changes", () =>
			{
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				SetGAApprovalData(pivot, "71", "수출허가서", "Test value is not deleted.");
				AssertEquals(1, pivot.GAApprovalDataCollection.Count);

				pivot.CI_TariffNum = tariff2.ZZ1_TariffCode;
				AssertEquals(1, pivot.GAApprovalDataCollection.Count);
				AssertEquals("71", pivot.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", pivot.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("If there is a same procedure and description, nothing will work.", "Test value is not deleted.", pivot.GAApprovalDataCollection[0].CSI_AdditionalDescription);

				SetGAApprovalData(pivot, "34", "수출허가서", "Test value is not deleted.");
				AssertEquals(2, pivot.GAApprovalDataCollection.Count);

				pivot.CI_TariffNum = tariff3.ZZ1_TariffCode;
				AssertEquals(2, pivot.GAApprovalDataCollection.Count);
				AssertEquals("34", pivot.GAApprovalDataCollection[0].CSI_Procedure);
				AssertEquals("수출허가서", pivot.GAApprovalDataCollection[0].CSI_Description);
				AssertEquals("Test value is not deleted.", pivot.GAApprovalDataCollection[0].CSI_AdditionalDescription);
				AssertEquals("55", pivot.GAApprovalDataCollection[1].CSI_Procedure);
				AssertEquals("수출허가증", pivot.GAApprovalDataCollection[1].CSI_Description);
				AssertEquals("", pivot.GAApprovalDataCollection[1].CSI_AdditionalDescription);
			});

			void SetGAApprovalData(CusClassPartPivot pivot, string procedure, string description, string additionalDescription)
			{
				var gAApproval = pivot.GAApprovalDataCollection.AddNew();
				gAApproval.CSI_Procedure = procedure;
				gAApproval.CSI_Description = description;
				gAApproval.CSI_AdditionalDescription = additionalDescription;
			}
		}

		public void TestHSExtensionCollection()
		{
			new TestDataSetupHelper(Factory).SetTariffAdditionalCodes();

			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			var pivot = orgSupplierPart.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNull(pivot.UniversalTariff);

			pivot.CI_TariffNum = "0301929090";
			AssertNotNull(pivot.UniversalTariff);
			AssertEquals(1, pivot.HSExtensionCodeCollection.Count);

			var mainAdditionalCode = pivot.HSExtensionCodeCollection[0];
			AssertEquals(HsExtensionCodes.Code.Category, mainAdditionalCode.Category);
			AssertEquals(ZString.Empty, mainAdditionalCode.CY_Code);

			CombineAssertions("0301929090 has four categories(01, 02, 03, 04).", () =>
			{
				AssertHSExtensionCodeCollection("01", 3, 2);
				AssertHSExtensionCodeCollection("02", 3, 2);
				AssertHSExtensionCodeCollection("03", 3, 2);
				AssertHSExtensionCodeCollection("04", 2, 1);
			});

			pivot.CI_TariffNum = "0105949000";
			AssertNotNull(pivot.UniversalTariff);
			AssertEquals(1, pivot.HSExtensionCodeCollection.Count);

			mainAdditionalCode = pivot.HSExtensionCodeCollection[0];
			CombineAssertions("0105949000 has one category(01).", () =>
			{
				AssertHSExtensionCodeCollection("01", 3, 2);
			});

			void AssertHSExtensionCodeCollection(string category, int collectionCount, int subCategoryCount)
			{
				mainAdditionalCode.CY_Code = category;
				AssertEquals(collectionCount, pivot.HSExtensionCodeCollection.Count);

				var hsExtensionCodeCollection = pivot.HSExtensionCodeCollection;
				AssertEquals(1, hsExtensionCodeCollection.Where(x => x.Category == HsExtensionCodes.Code.Category).Count());
				AssertEquals(subCategoryCount, hsExtensionCodeCollection.Where(x => x.Category == HsExtensionCodes.Code.SubCategory).Count());
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			return pivot;
		}
	}
}
