using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_IsMainPack_InvalidForBulkAndBreakBulk()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "999";
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "VG";
			package1.CW_HouseBill = bill.CU_BillUniqueCode;
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 10;
			package2.CW_PackType = "NE";
			package2.CW_HouseBill = bill.CU_BillUniqueCode;
			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 20;
			package3.CW_PackType = "CT";
			package3.CW_HouseBill = bill.CU_BillUniqueCode;

			var message = "When Invoice Line is bulk or break bulk, then Is Main Pack does not need to be indicated and has no effect.";

			var lineLinkPachage1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			lineLinkPachage1.IsLinked = true;
			invoiceLine.ZG_IsMainPack = true;
			AssertHasWarning("Bulk", invoiceLine.ZG_IsMainPackInfo, message);

			lineLinkPachage1.IsLinked = false;
			var lineLinkPachage3 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[2];
			lineLinkPachage3.IsLinked = true;
			invoiceLine.ZG_IsMainPack = true;
			AssertNoWarning("Packed", invoiceLine.ZG_IsMainPackInfo, message);

			lineLinkPachage3.IsLinked = false;
			var lineLinkPachage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
			lineLinkPachage2.IsLinked = true;
			invoiceLine.ZG_IsMainPack = true;
			AssertHasWarning("Break Bulk", invoiceLine.ZG_IsMainPackInfo, message);
		}

		public void TestCheckZG_IsMainPack_OneForCommonPackLines()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine4.JI_LineNo = 4;
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine4.JI_CEI = instruction.PK;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "999";
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "VG";
			package1.CW_HouseBill = bill.CU_BillUniqueCode;
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 10;
			package2.CW_PackType = "CT";
			package2.CW_HouseBill = bill.CU_BillUniqueCode;
			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 20;
			package3.CW_PackType = "CT";
			package3.CW_HouseBill = bill.CU_BillUniqueCode;

			invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine3.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invoiceLine3.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;
			invoiceLine4.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invoiceLine4.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;

			invoiceLine1.ZG_IsMainPack = true;
			invoiceLine2.ZG_IsMainPack = true;
			invoiceLine3.ZG_IsMainPack = true;
			invoiceLine4.ZG_IsMainPack = true;

			var message1 = "Where Invoice Lines are Packed AND the Linked Packages are Common then one and only one of the Invoice Lines must be indicated as Is Main Pack. Only one of invoice 1 line 4, invoice 1 line 3 should be indicated as Is Main Pack.";
			var message2 = "Where Invoice Lines are Packed AND the Linked Packages are Common then one and only one of the Invoice Lines must be indicated as Is Main Pack. One of invoice 1 line 4, invoice 1 line 3 should be indicated as Is Main Pack.";
			AssertNoMessageErrors("Not Packed", invoiceLine1.ZG_IsMainPackInfo);
			AssertNoMessageErrors("Not Packed", invoiceLine2.ZG_IsMainPackInfo);
			AssertNoMessageErrors("Only one when was checked", invoiceLine3.ZG_IsMainPackInfo);
			AssertHasMessageError("Two main packs", invoiceLine4.ZG_IsMainPackInfo, message1);

			invoiceLine3.ZG_IsMainPack = false;
			invoiceLine4.ZG_IsMainPack = false;
			AssertHasMessageError("No main pack", invoiceLine4.ZG_IsMainPackInfo, message2);

			invoiceLine4.ZG_IsMainPack = true;
			AssertNoMessageErrors("One main packs", invoiceLine4.ZG_IsMainPackInfo);
		}

		public void TestCheckZG_CountryOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.ZG_CountryOfDestination = "AU";
			AssertNoMessageErrorContaining(invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.ZG_CountryOfDestination = "@1";
			AssertHasMessageErrorContaining(invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZG_CountryOfDestination_EffectiveValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_GoodsDestination = "AU";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
			AssertNoMessageErrorContaining(invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_GoodsDestination = "@1";
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
			AssertHasMessageErrorContaining(invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZG_CountryOfDestination_IsInEU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica, parent: eun);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.JE_EntryStyle = "CO";
			CombineAssertions("Declaration Type = CO", () =>
			{
				var errorMessageCO = "Destination country should be an EU country, when declaration type is 'CO'.";
				new List<string> { "", "IE", "FR", "DE", "IT" }.ForEach(v =>
				{
					invoiceLine.ZG_CountryOfDestination = v;
					AssertNoMessageErrorContaining(v + " should be valid", invoiceLine.ZG_CountryOfDestinationInfo, errorMessageCO);
				});
				new List<string> { "US", "AU", "ZA" }.ForEach(v =>
				{
					invoiceLine.ZG_CountryOfDestination = v;
					AssertHasMessageErrorContaining(v + " should be invalid", invoiceLine.ZG_CountryOfDestinationInfo, errorMessageCO);
				});
			});

			declaration.JE_EntryStyle = "EX";
			var errorMessageEX = "Destination country should NOT be an EU country, when declaration type is 'EX'.";
			CombineAssertions("Declaration Type = EX", () =>
			{
				new List<string> { "IE", "FR", "DE", "IT" }.ForEach(v =>
				{
					invoiceLine.ZG_CountryOfDestination = v;
					AssertHasMessageErrorContaining(v + " should be invalid", invoiceLine.ZG_CountryOfDestinationInfo, errorMessageEX);
				});
				new List<string> { "", "US", "AU", "ZA" }.ForEach(v =>
				{
					invoiceLine.ZG_CountryOfDestination = v;
					AssertNoMessageErrorContaining(v + " should be valid", invoiceLine.ZG_CountryOfDestinationInfo, errorMessageEX);
				});
			});
		}

		public void TestZG_CountryOfDestination_B1871_B1872()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");

			var code_value = UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL140;

			helper.CreateNewOrGetExistingCusCodeType(code_value, "CL140");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "CN", "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "IE", "IE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "QQ", "QQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AD", "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "SM", "SM", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "DE", "DE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "IT", "IT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			code_value = UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL063;
			helper.CreateNewOrGetExistingCusCodeType(code_value, "CL063");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "XX", "XX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "YY", "YY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "AD", "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var messageError1872 = "If one Invoice Line has Country of Destination within Other Regime Country Codes List (CL140) or 'AD' , 'SM' ,'DE' , 'IT' Excluding  'QQ' , 'QR' , 'QV', then all lines should have a Country of Destination within the same list.";
			var messageError1871 = "If one Invoice Line has Country of Destination within Common Transit Community Country Codes List (CL063) excluding 'AD', 'SM', then all lines should have a Country of Destination within the same list.";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Enterprise.Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				AssertEquals("Functionality is enabled", true, declaration.IsTransitionPeriodAES30);

				invoiceLine.ZG_CountryOfDestination = "AU";
				AssertNoMessageError("Should not have 1872 message error ", invoiceLine.ZG_CountryOfDestinationInfo, messageError1872);
				AssertNoMessageError("Should not have 1871 message error", invoiceLine.ZG_CountryOfDestinationInfo, messageError1871);

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.ZG_CountryOfDestination = "QQ";
				AssertHasMessageError("Should have 1872 message error", invoiceLine2.ZG_CountryOfDestinationInfo, messageError1872);
				AssertNoMessageError("Should not have 1871 message error", invoiceLine2.ZG_CountryOfDestinationInfo, messageError1871);

				invoiceLine2.ZG_CountryOfDestination = "XX";
				AssertNoMessageError("Should not have 1871 message error while ZG_CountryOfDestination not in the list", invoiceLine2.ZG_CountryOfDestinationInfo, messageError1871);

				var invoiceLine3 = invoice.InvoiceLines.AddNew();
				invoiceLine3.ZG_CountryOfDestination = "ZZ";
				AssertHasMessageError("Should have 1871 message error", invoiceLine3.ZG_CountryOfDestinationInfo, messageError1871);
				AssertHasMessageError("Should have 1872 message error", invoiceLine3.ZG_CountryOfDestinationInfo, messageError1872);
			}
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "CN", "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			base.SetUp();
		}
	}
}
