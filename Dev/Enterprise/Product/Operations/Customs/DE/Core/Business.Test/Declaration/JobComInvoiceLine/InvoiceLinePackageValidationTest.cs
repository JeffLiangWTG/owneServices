using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class InvoiceLinePackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPackQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Enterprise.Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Enterprise.Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MasterBill = "M";

			var package = declaration.Bills[0].PackingGroups[0].Packages[0];
			package.CW_PackQty = 1;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage = linePackageCollection[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = false;

			var linePackageCollection2 = (InvoiceLineCusLinkPackageCollection)invoiceLine2.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage2 = linePackageCollection2[0];
			lineLinkPackage2.Package = package;
			lineLinkPackage2.IsLinked = false;

			package.CW_PackType = "VG";
			lineLinkPackage.IsLinked = true;
			lineLinkPackage.PackQty = 2;
			AssertHasMessageError(lineLinkPackage.PackQtyInfo, "Package Type of VG requires Pack Quantity to be 1.");
			lineLinkPackage.PackQty = 0;
			AssertNoMessageError(lineLinkPackage.PackQtyInfo, "You have not entered a value.");
			AssertHasMessageError(lineLinkPackage.PackQtyInfo, "Package Type of VG requires Pack Quantity to be 1.");

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			lineLinkPackage.Validation.ValidatePackQty();
			AssertNoMessageErrors(lineLinkPackage.PackQtyInfo);

			package.CW_PackType = "TT";
			lineLinkPackage.Validation.ValidatePackQty();
			AssertHasMessageError(lineLinkPackage.PackQtyInfo, "You have not entered a value.");
			AssertNoMessageError(lineLinkPackage.PackQtyInfo, "Package Type of VG requires Pack Quantity to be 1.");

			lineLinkPackage2.IsLinked = true;
			lineLinkPackage.Validation.ValidatePackQty();
			AssertHasMessageError(lineLinkPackage.PackQtyInfo, "You have not entered a value.");

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			lineLinkPackage.Validation.ValidatePackQty();
			AssertNoMessageErrors(lineLinkPackage.PackQtyInfo);
			package.CW_PackType = "NE";
			lineLinkPackage.Validation.ValidatePackQty();
			AssertNoMessageErrors(lineLinkPackage.PackQtyInfo);

			lineLinkPackage2.IsLinked = false;
			lineLinkPackage.Validation.ValidatePackQty();
			AssertHasMessageError(lineLinkPackage.PackQtyInfo, "Package Type of NE requires Pack Quantity to be between 1 and 99999999.");

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				lineLinkPackage.Validation.ValidatePackQty();
				AssertHasMessageError(lineLinkPackage.PackQtyInfo, "Package Type of NE requires Pack Quantity to be between 1 and 99999.");
			}

			lineLinkPackage2.IsLinked = true;
			invoiceLine2.JI_IsMainPack = true;
			lineLinkPackage.PackQty = 3;
			AssertHasMessageError(lineLinkPackage.PackQtyInfo, "There is an Invoice Line marked as ‘Is Main Pack’ linked to this package, this requires all other Pack quantities linked to this package to be 0.");
		}

		public void TestCheckPackQtyForExport_TransitionPeriodActive()
		{
			const string message = "Please enter a 'Pack Quantity' within the range 0 to 99999.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "M";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			var package = declaration.Bills[0].PackingGroups[0].Packages[0];

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage = linePackageCollection[0];
			lineLinkPackage.Package = package;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				lineLinkPackage.PackQty = 100000;
				AssertHasError("PackQty > 99999", lineLinkPackage.PackQtyInfo, message);

				lineLinkPackage.PackQty = 99999;
				AssertNoError("PackQty <= 99999", lineLinkPackage.PackQtyInfo, message);
			}
		}

		public void TestCheckPackQtyForExport_TransitionPeriodInactive()
		{
			const string message = "Please enter a 'Pack Quantity' within the range 0 to 99999999.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "M";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			var package = declaration.Bills[0].PackingGroups[0].Packages[0];

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage = linePackageCollection[0];
			lineLinkPackage.Package = package;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				lineLinkPackage.PackQty = 100000000;
				AssertHasError("PackQty > 99999999", lineLinkPackage.PackQtyInfo, message);

				lineLinkPackage.PackQty = 99999999;
				AssertNoError("PackQty <= 99999999", lineLinkPackage.PackQtyInfo, message);
			}
		}

		public void TestCheckIsLinked_Export()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "M";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var packages = declaration.Bills[0].PackingGroups[0].Packages;
			var package1 = packages[0];
			package1.CW_PackType = "CT";
			package1.CW_MarksAndNos = "MARKS AND NUMBERS";
			var package2 = packages.AddNew();
			package2.CW_PackType = "NE";
			package2.CW_MarksAndNos = "MARKS AND NUMBERS";

			var linePackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage1 = linePackageCollection[0];
			lineLinkPackage1.Package = package1;
			lineLinkPackage1.IsLinked = true;

			var lineLinkPackage2 = linePackageCollection.AddNew();
			lineLinkPackage2.Package = package2;
			lineLinkPackage2.IsLinked = true;

			CombineAssertions(() =>
			{
				lineLinkPackage1.Validation.ValidateIsLinked();
				AssertNoMessageError("LineLinkedPackage1", lineLinkPackage1.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertNoMessageError("LineLinkedPackage2", lineLinkPackage2.IsLinkedInfo, SamePackTypeAndMarksMessage);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				lineLinkPackage1.Validation.ValidateIsLinked();
				lineLinkPackage2.Validation.ValidateIsLinked();
				AssertHasMessageError("LineLinkedPackage1 - IMP", lineLinkPackage1.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertHasMessageError("LineLinkedPackage2 - IMP", lineLinkPackage2.IsLinkedInfo, SamePackTypeAndMarksMessage);
			});
		}

		public void TestCheckIsLinked_Import()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "M";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var packages = declaration.Bills[0].PackingGroups[0].Packages;
			var package1 = packages[0];
			package1.CW_PackType = "CT";
			package1.CW_MarksAndNos = "MARKS AND NUMBERS";
			var package2 = packages.AddNew();
			package2.CW_PackType = "NE";
			package2.CW_MarksAndNos = "MARKS AND NUMBERS";
			var notLinkedPackage = packages.AddNew();
			notLinkedPackage.CW_PackType = "CT";
			notLinkedPackage.CW_MarksAndNos = "MARKS AND NUMBERS";
			var linePackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage1 = linePackageCollection[0];
			lineLinkPackage1.Package = package1;
			lineLinkPackage1.IsLinked = true;
			var lineLinkPackage2 = linePackageCollection.AddNew();
			lineLinkPackage2.Package = package2;
			lineLinkPackage2.IsLinked = true;
			var notLinkedLineLinkPackage = linePackageCollection.AddNew();
			notLinkedLineLinkPackage.Package = notLinkedPackage;

			CombineAssertions(() =>
			{
				lineLinkPackage1.Validation.ValidateIsLinked();
				lineLinkPackage2.Validation.ValidateIsLinked();
				notLinkedLineLinkPackage.Validation.ValidateIsLinked();
				AssertHasMessageError("LineLinkPackage1: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'NE', 'MARKS AND NUMBERS'", lineLinkPackage1.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertHasMessageError("LineLinkPackage2: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'NE', 'MARKS AND NUMBERS'", lineLinkPackage2.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertNoMessageError("LineLinkPackage3: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'NE', 'MARKS AND NUMBERS'", notLinkedLineLinkPackage.IsLinkedInfo, SamePackTypeAndMarksMessage);

				package2.CW_PackType = "CT";
				lineLinkPackage1.Validation.ValidateIsLinked();
				lineLinkPackage2.Validation.ValidateIsLinked();
				notLinkedLineLinkPackage.Validation.ValidateIsLinked();
				AssertNoMessageError("LineLinkPackage1: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'MARKS AND NUMBERS'", lineLinkPackage1.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertNoMessageError("LineLinkPackage2: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'MARKS AND NUMBERS'", lineLinkPackage2.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertNoMessageError("LineLinkPackage3: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'MARKS AND NUMBERS'", notLinkedLineLinkPackage.IsLinkedInfo, SamePackTypeAndMarksMessage);

				package2.CW_MarksAndNos = "NOT EQUAL";
				lineLinkPackage1.Validation.ValidateIsLinked();
				lineLinkPackage2.Validation.ValidateIsLinked();
				notLinkedLineLinkPackage.Validation.ValidateIsLinked();
				AssertHasMessageError("LineLinkPackage1: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'NOT EQUAL'", lineLinkPackage1.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertHasMessageError("LineLinkPackage2: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'NOT EQUAL'", lineLinkPackage2.IsLinkedInfo, SamePackTypeAndMarksMessage);
				AssertNoMessageError("LineLinkPackage3: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'NOT EQUAL'", notLinkedLineLinkPackage.IsLinkedInfo, SamePackTypeAndMarksMessage);
			});
		}

		const string SamePackTypeAndMarksMessage = "Multiple Package Lines on one Invoice Line require to have same Package Type and Marks.";
	}
}
