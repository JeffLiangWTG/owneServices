using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSequenceNumberNoDuplicated()
		{
			var messageErrorNotBeDuplicated = "Seq No. (1) should not be duplicated";

			var exitHeader = Factory.New<CusExitHeader>();
			var exitConsignment = exitHeader.CusExitConsignments.AddNew();
			var consignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;

			var packing1 = Factory.New<CusExitConsignmentPackage_SequenceNumberNotReadOnly>();
			var packing2 = Factory.New<CusExitConsignmentPackage_SequenceNumberNotReadOnly>();

			exitHeader.CusExitConsignmentPackages.Add(packing1);
			exitHeader.CusExitConsignmentPackages.Add(packing2);

			var packagePivot1 = Factory.New<CusExitConsignmentPivot>();
			packagePivot1.CNP_CXP_Package = packing1.PK;
			var packagePivot2 = Factory.New<CusExitConsignmentPivot>();
			packagePivot2.CNP_CXP_Package = packing2.PK;

			consignmentItem.CusExitConsignmentPackagePivots.Add(packagePivot1);
			consignmentItem.CusExitConsignmentPackagePivots.Add(packagePivot2);

			CombineAssertions(() =>
			{
				AssertEquals("PRE-CONDITION", false, packing1.CXP_SequenceInfo.ReadOnly);

				packing1.CXP_Sequence = 1;
				AssertNoError("No test sequence duplicated", packing1.CXP_SequenceInfo, messageErrorNotBeDuplicated);

				packing2.CXP_Sequence = 1;
				AssertHasError("The test is for control of sequence duplicated", packing2.CXP_SequenceInfo, messageErrorNotBeDuplicated);
				packing2.Validation.ValidateCXP_Sequence();
				AssertHasError("The test is for control of sequence duplicated after validate again", packing2.CXP_SequenceInfo, messageErrorNotBeDuplicated);
				Factory.Save();
				packing2.Validation.ValidateCXP_Sequence();
				AssertNoError("No test sequence duplicated after save and no changes after clearing notification", packing2.CXP_SequenceInfo, messageErrorNotBeDuplicated);

				packing2.CXP_Sequence = 2;
				AssertNoError("No test sequence duplicated", packing2.CXP_SequenceInfo, messageErrorNotBeDuplicated);
			});
		}

		public void TestCheckCXP_Quantity()
		{
			const string message = "Please enter a 'Pack Qty' within the range 0 to 99999999.";
			CombineAssertions(() =>
			{
				(var exitConsignmentPackage, _, _, _, _) = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);
				exitConsignmentPackage.CXP_Quantity = -1;
				AssertHasError("CXP_Quantity = -1", exitConsignmentPackage.CXP_QuantityInfo, message);

				exitConsignmentPackage.CXP_Quantity = 0;
				AssertNoError("CXP_Quantity = 0", exitConsignmentPackage.CXP_QuantityInfo, message);

				exitConsignmentPackage.CXP_Quantity = 99999999;
				AssertNoError("CXP_Quantity = 99999999", exitConsignmentPackage.CXP_QuantityInfo, message);

				exitConsignmentPackage.CXP_Quantity = 100000000;
				AssertHasError("CXP_Quantity = 100000000", exitConsignmentPackage.CXP_QuantityInfo, message);
			});
		}

		public void TestCheckCXP_PackageType()
		{
			var helperCustomsOffice = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helperCustomsOffice.CreateNewOrGetExistingCusCodeType(UnitedNationsPackageTypes, "Package Units");
			helperCustomsOffice.CreateNewOrGetExistingCusCodeType("Inv", "Invalid Type");
			helperCustomsOffice.CreateCusCodeList(UnitedNationsRecommendations, UnitedNationsPackageTypes, "01", "Test1", new ZDateTime(2011, 12, 20), ZDateTime.Now.AddDays(5));
			Factory.Save();
			(var exitConsignmentPackage, _, _, _, _) = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);

			ValidationTestHelper.AssertInvalidCodeMessageError(exitConsignmentPackage.CXP_PackageTypeInfo, "XX", "01");
		}

		public void TestCheckCXP_Calc_ReportQuantity()
		{
			CombineAssertions(() =>
			{
				(var exitConsignmentPackage, _, _, _, _) = CusExitConsignmentPackageTest.GetNewBusinessObject(Factory);
				using (exitConsignmentPackage.SetupReportItemData(ZGuid.Empty))
				{
					var targetInfo = exitConsignmentPackage.CXP_Calc_ReportQuantityInfo;
					var valueCannotBeNegativeMessage = MandatoryValidation.ValueCannotBeNegativeMessage(targetInfo.HumanReadableName);
					var valueCannotBeZeroMessage = MandatoryValidation.ValueCannotBeZeroMessage(targetInfo.HumanReadableName);

					exitConsignmentPackage.CXP_Calc_ShouldReportItem = true;
					exitConsignmentPackage.CXP_Calc_ReportQuantity = -1;
					AssertHasError("CXP_Calc_ReportQuantity = -1", targetInfo, valueCannotBeNegativeMessage);

					exitConsignmentPackage.CXP_Calc_ShouldReportItem = false;
					exitConsignmentPackage.Validation.ValidateCXP_Calc_ReportQuantity();
					AssertNoError("CXP_Calc_ReportQuantity = -1", targetInfo, valueCannotBeNegativeMessage);

					exitConsignmentPackage.CXP_Calc_ShouldReportItem = true;
					exitConsignmentPackage.CXP_Calc_ReportQuantity = 1;
					AssertNoError("CXP_Calc_ReportQuantity = 1", targetInfo, valueCannotBeNegativeMessage);
					AssertNoMessageError("CXP_Calc_ReportQuantity = 1", targetInfo, valueCannotBeZeroMessage);

					exitConsignmentPackage.CXP_Calc_ReportQuantity = 0;
					AssertHasMessageError("CXP_Calc_ReportQuantity = 0", targetInfo, valueCannotBeZeroMessage);

					exitConsignmentPackage.CXP_Calc_ShouldReportItem = false;
					exitConsignmentPackage.Validation.ValidateCXP_Calc_ReportQuantity();
					AssertNoMessageError("CXP_Calc_ReportQuantity = 1", targetInfo, valueCannotBeZeroMessage);
				}
			});
		}

		public void TestCheckCXP_Sequence_NotNeedToBeGreaterThanZero_WhenSequenceIsReadOnly()
		{
			var consignmentPackage = Factory.New<CusExitConsignmentPackage>();

			AssertEquals("PRE-CONDITION", true, consignmentPackage.CXP_SequenceInfo.ReadOnly);

			CombineAssertions(() =>
			{
				consignmentPackage.CXP_Sequence = -1;
				AssertNoErrors("When CXP_Sequence is negative", consignmentPackage.CXP_SequenceInfo);

				consignmentPackage.CXP_Sequence = 0;
				AssertNoErrors("When CXP_Sequence is zero", consignmentPackage.CXP_SequenceInfo);
			});
		}

		public void TestCheckCXP_Sequence_MustBeGreaterThanZero_WhenSequenceIsNotReadOnly()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var consignmentPackage = Factory.New<CusExitConsignmentPackage_SequenceNumberNotReadOnly>();
			exitHeader.CusExitConsignmentPackages.Add(consignmentPackage);

			AssertEquals("PRE-CONDITION", false, consignmentPackage.CXP_SequenceInfo.ReadOnly);

			CombineAssertions("When CXP_Sequence is negative", () =>
			{
				consignmentPackage.CXP_Sequence = -1;
				AssertHasErrorContaining(consignmentPackage.CXP_SequenceInfo, MandatoryValidation.ValueCannotBeNegative);
				AssertNoErrorContaining(consignmentPackage.CXP_SequenceInfo, MandatoryValidation.ValueCannotBeZero);
			});

			CombineAssertions("When CXP_Sequence is zero", () =>
			{
				consignmentPackage.CXP_Sequence = 0;
				AssertNoErrorContaining(consignmentPackage.CXP_SequenceInfo, MandatoryValidation.ValueCannotBeNegative);
				AssertHasErrorContaining(consignmentPackage.CXP_SequenceInfo, MandatoryValidation.ValueCannotBeZero);
			});
			CombineAssertions("When CXP_Sequence is greater than zero", () =>
			{
				consignmentPackage.CXP_Sequence = 1;
				AssertNoErrorContaining(consignmentPackage.CXP_SequenceInfo, MandatoryValidation.ValueCannotBeNegative);
				AssertNoErrorContaining(consignmentPackage.CXP_SequenceInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckCXP_MarksAndNumbersStatus_ListValidation() => CombineAssertions(() =>
		{
			var itemPackagePivot = Factory.New<CusExitHeader>().CusExitConsignments.AddNew()
				.CusExitConsignmentItems.AddNew()
				.CusExitConsignmentPackagePivots.AddNew();
			var package = itemPackagePivot.Package;
			package.CXP_MarksAndNumbersStatus = "AH3";
			AssertListValidationInvalidCodeMessageError(package.CXP_MarksAndNumbersStatusInfo, false);

			itemPackagePivot = Factory.GetUcc6ExitHeader().CusExitConsignments.AddNew()
				.CusExitConsignmentItems.AddNew()
				.CusExitConsignmentPackagePivots.AddNew();
			package = itemPackagePivot.Package;
			package.CXP_MarksAndNumbersStatus = "AH3";
			AssertListValidationInvalidCodeMessageError(package.CXP_MarksAndNumbersStatusInfo, true);

			package.CXP_MarksAndNumbersStatus = "DIF";
			AssertListValidationInvalidCodeMessageError(package.CXP_MarksAndNumbersStatusInfo, false);
		});

		class CusExitConsignmentPackage_SequenceNumberNotReadOnly : CusExitConsignmentPackage
		{
			public CusExitConsignmentPackage_SequenceNumberNotReadOnly(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool CXP_SequenceReadOnly => false;
		}
	}
}
