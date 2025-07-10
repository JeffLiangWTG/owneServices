using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_UnitType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(package.B5_UnitTypeInfo, "X", "AE");
		}

		public void TestCheckB5_UnitCount()
		{
			CombineAssertions(() =>
			{
				package.B5_UnitCount = 999999999;
				AssertNoErrors(package.B5_UnitCountInfo);

				package.B5_UnitType = "VQ";
				package.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("case 1", package.B5_UnitCountInfo, "VQ is not a countable package unit for EMCS.");

				package.B5_UnitCount = 0;
				package.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("case 2", package.B5_UnitCountInfo, "VQ is not a countable package unit for EMCS.");
			});
		}

		public void TestCheckB5_UnitCount_Mandatory()
		{
			CombineAssertions(() =>
			{
				package.B5_UnitType = "AE";
				package.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("B5_UnitCount 0", package.B5_UnitCountInfo, "No of Packages can't be 0.");

				package.B5_UnitCount = 1;
				AssertNoMessageError("B5_UnitCount 1", package.B5_UnitCountInfo, "No of Packages can't be 0.");
			});
		}

		public void TestCheckB5_UnitCount_WithinRange()
		{
			CombineAssertions(() =>
			{
				package.B5_UnitType = "AE";
				package.B5_UnitCount = 999999999999999;
				AssertNoError("B5_UnitCount 999999999999999", package.B5_UnitCountInfo, "Please enter a 'Qty' within the range 1 to 999999999999999.");

				package.B5_UnitCount = 1000000000000000;
				AssertHasError("B5_UnitCount 1000000000000000", package.B5_UnitCountInfo, "Please enter a 'Qty' within the range 1 to 999999999999999.");
			});
		}

		public void TestCheckB5_UnitCount_NoMainPack()
		{
			CombineAssertions(() =>
			{
				var packagePivot = invoiceLine.EMCSPackagePivots[0];
				packagePivot.IsForInvoiceLine = true;
				package.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("No Line Is main Pack", package.B5_UnitCountInfo, "Please specify the Main Pack Line for this Package.");

				invoiceLine.ZG_IsMainPack = true;
				package.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("Is main Pack Line exists", package.B5_UnitCountInfo, "Please specify the Main Pack Line for this Package.");
			});
		}

		public void TestCheckB5_UnitCount_NotLinkedToLine()
		{
			CombineAssertions(() =>
			{
				package.Validation.ValidateB5_UnitCount();
				AssertHasMessageError("Package not linked to any line", package.B5_UnitCountInfo, "Package must be linked to at least one Line.");

				var packagePivot = invoiceLine.EMCSPackagePivots[0];
				packagePivot.IsForInvoiceLine = true;
				package.Validation.ValidateB5_UnitCount();
				AssertNoMessageError("Package linked to line", package.B5_UnitCountInfo, "Package must be linked to at least one Line.");
			});
		}

		public void TestCheckB5_MarksAndNumbers()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_MarksAndNumbersInfo);
		}

		void CreateCountableUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");

			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "AE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);

			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		protected override void SetUp()
		{
			CreateCountableUQ();
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			invoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			package = declaration.EMCSPackages.AddNew();
		}
		EMCSPackage package;
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine invoiceLine;
	}
}
