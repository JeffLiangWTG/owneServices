using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class PackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCW_PackType()
		{
			AssertNoMessageErrorContaining(package.CW_PackTypeInfo, "You have not entered");
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.CW_PackTypeInfo);
		}

		public void TestCheckCW_MarksAndNos_MaxLength_Import()
		{
			const string messageForImport = "The maximum length for Marks is 70 characters.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				package.CW_MarksAndNos = ZString.Empty.PadLeft(71, 'A');
				AssertHasMessageError("Import and 71 characters", package.CW_MarksAndNosInfo, messageForImport);
				package.CW_MarksAndNos = ZString.Empty.PadLeft(70, 'A');
				AssertNoMessageError("Import and 70 characters", package.CW_MarksAndNosInfo, messageForImport);
			});
		}

		public void TestCheckCW_MarksAndNos_MaxLength_Export()
		{
			var transitionPeriodActive = false;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, transitionPeriodActive))
			{
				const string messageForExport = "The maximum length for Marks is 512 characters.";
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					package.CW_MarksAndNos = ZString.Empty.PadLeft(513, 'A');
					AssertHasMessageError("Export and 513 characters", package.CW_MarksAndNosInfo, messageForExport);
					package.CW_MarksAndNos = ZString.Empty.PadLeft(512, 'A');
					AssertNoMessageError("Export and 512 characters", package.CW_MarksAndNosInfo, messageForExport);
				});
			}
		}

		public void TestCheckCW_MarksAndNos_MaxLength_TransitionPeriod_Export()
		{
			var transitionPeriodActive = true;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, transitionPeriodActive))
			{
				const string messageForExport = "The maximum length for Marks is 42 characters.";
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					package.CW_MarksAndNos = ZString.Empty.PadLeft(43, 'A');
					AssertHasMessageError("Export and 43 characters", package.CW_MarksAndNosInfo, messageForExport);
					package.CW_MarksAndNos = ZString.Empty.PadLeft(42, 'A');
					AssertNoMessageError("Export and 42 characters", package.CW_MarksAndNosInfo, messageForExport);
				});
			}
		}

		public void TestValidatePackageIsAssignedToInvoiceLine_Export()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lineLinkPackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = false;

			package.Validation.ValidateAll();
			AssertNoRowMessageError("Not linked", package, "Package Line is not assigned to an Invoice Line.");
		}

		public void TestValidatePackageIsAssignedToInvoiceLine_Import()
		{
			const string message = "Package Line is not assigned to an Invoice Line.";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lineLinkPackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = false;

			CombineAssertions(() =>
			{
				package.Validation.ValidateAll();
				AssertHasRowMessageError("Not linked", package, message);

				lineLinkPackage.IsLinked = true;
				package.Validation.ValidateAll();
				AssertNoRowMessageError("Linked", package, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			package = (Package)declaration.Packages.AddNew();
		}
		JobDeclaration declaration;
		Package package;
	}
}
