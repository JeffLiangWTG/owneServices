using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForMasterChild))]
	sealed class AsycudaBillForMasterChildValidationTest : AsycudaBillValidationAbstractTest
	{
		[TestDate(2018, 2, 6)]
		public void TestNoNotifcationsOnAnyOtherField()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVSEA = helper.CreateNewOrGetExistingCusCodeList("VU", RefCusCodeListTypes.Codes.CustomsOffice, "VSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVSEA.PK, "SEA", "VUVLI");
			var pgWWK = helper.CreateNewOrGetExistingCusCodeList("PG", RefCusCodeListTypes.Codes.CustomsOffice, "WWK", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(pgWWK.PK, "PORT", "PGWWK");
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var childBill = header.MasterBill;
			childBill.ABL_BillNumber = "MB1";
			childBill.ABL_E_ARV = ZDateTime.Now.AddDays(-1);
			childBill.ABL_RL_NKPortOfDischarge = "VUVLI";
			childBill.ABL_RL_NKPortOfLoading = "SGSIN";
			childBill.Validation.ValidateAll();
			AssertEquals("", new ZStringBuilder(childBill.Notifications.Select(x => x.Message).OrderBy(x => x)).ToStringWithNewLineBetweenAppends());
		}

		public void TestCheckABL_BillNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = "MB1";
			AssertNoMessageErrors(masterBill.ABL_BillNumberInfo);
			masterBill.ABL_BillNumber = ZString.Empty;
			var message = ValidationConstants.ManifestNumberIsRequired(header.MasterBillLabel.Caption);
			AssertHasMessageError(masterBill.ABL_BillNumberInfo, message);
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			masterBill.ABL_BillNumber = ZString.Empty;
			AssertNoMessageError(masterBill.ABL_BillNumberInfo, message);
		}

		public void TestCheckABL_GrossWeight()
		{
			var message = "Total Gross weight is less than the gross weight entered under bills.";
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			var bill = header.Bills.AddNew();
			bill.ABL_GrossWeight = 1000m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			bill = header.Bills.AddNew();
			bill.ABL_GrossWeight = 2m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			masterBill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			masterBill.ABL_GrossWeight = 3m;

			CombineAssertions(() =>
			{
				AssertNoMessageError("Total House Bills Gross Weight equal to MasterBill GrossWeight", masterBill.ABL_GrossWeightInfo, message);
				masterBill.ABL_GrossWeight = 2m;
				AssertHasMessageError("Total House Bills Gross Weight more than MasterBill GrossWeight", masterBill.ABL_GrossWeightInfo, message);
				masterBill.ABL_GrossWeightUQ = ZString.Empty;
				masterBill.ABL_GrossWeight = 1m;
				AssertNoMessageError("Master Bill Gross Weight UQ is empty", masterBill.ABL_GrossWeightInfo, message);
			});
		}

		public void TestCheckABL_ManifestQty()
		{
			var message = "Total Packages are less than the packages entered under bills.";
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 1;
			bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
			bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 4;
			bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
			masterBill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
			masterBill.ABL_ManifestQty = 5;

			CombineAssertions(() =>
			{
				AssertNoMessageError("Total House Bills Package equal to MasterBill Package", masterBill.ABL_ManifestQtyInfo, message);
				masterBill.ABL_ManifestQty = 4;
				AssertHasMessageError("Total House Bills Package more than MasterBill Package", masterBill.ABL_ManifestQtyInfo, message);
				masterBill.ABL_ManifestUQ = ZString.Empty;
				masterBill.ABL_ManifestQty = 2;
				AssertNoMessageError("Master Bill Package UQ is empty", masterBill.ABL_ManifestQtyInfo, message);
			});
		}

		public void TestCheckABL_BillIssueDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, universalAlias.RefCusCodeList.ManifestValidationRuleCodes.BillIssueDate, "Lame", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(universalAlias.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, "Desc.", universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule);
			validationRule.Attributes.AddNew(universalAlias.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
				var bill = header.MasterBill;
				bill.ABL_BillIssueDate = ZDate.Empty;
				AssertHasMessageErrorContaining(bill.ABL_BillIssueDateInfo, "Lame");
				bill.ABL_BillIssueDate = ZDate.BrettsBirthday;
				AssertNoMessageErrorContaining(bill.ABL_BillIssueDateInfo, "Lame");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Canada;
				var bill = header.MasterBill;
				bill.ABL_BillIssueDate = ZDate.Empty;
				AssertNoMessageErrorContaining(bill.ABL_BillIssueDateInfo, "Lame");
				bill.ABL_BillIssueDate = ZDate.BrettsBirthday;
				AssertNoMessageErrorContaining(bill.ABL_BillIssueDateInfo, "Lame");
			}
		}

		public void TestABL_E_DEP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "An Estimated Departure Time is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedDepartureTime, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageError += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header.AMA_E_DEP = ZDateTime.BrettsBirthday;
			AssertNoMessageError(header.MasterBill.ABL_E_DEPInfo, messageError);
			header.AMA_E_DEP = ZDateTime.Empty;
			AssertHasMessageError(header.MasterBill.ABL_E_DEPInfo, messageError);
		}

		public void TestPortOfDischargeIsRequiredForTransport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.PapuaNewGuinea, "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, "United States", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var portOfDischargeMessage = "Port of Discharge is required";
			var portOfDischargeRulle = helper.CreateNewOrGetExistingCusCodeList("US", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.PortOfDischarge, portOfDischargeMessage, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(portOfDischargeRulle.PK, Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, Core.Constants.TransportModes.Air);
			portOfDischargeMessage += " for US.";

			var iataPortOfDischargeMessage = "IATA is required for Port Of Discharge";
			var za2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfDischarge, iataPortOfDischargeMessage, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			za2.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			iataPortOfDischargeMessage += " for US.";

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "US!2#";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = ZString.Empty;
			Factory.Save();

			var headerPG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.PapuaNewGuinea, "ASY");
			headerPG.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerPG.AMA_RL_NKPortOfLoading = "PGXXX";
			headerPG.AMA_RL_NKPortOfDischarge = "";
			AssertHasMessageErrorContaining(headerPG.MasterBill.ABL_RL_NKPortOfDischargeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarnings(headerPG.MasterBill.ABL_RL_NKPortOfDischargeInfo);
			headerPG.AMA_RL_NKPortOfDischarge = "PGXXX";
			AssertNoMessageErrorContaining(headerPG.MasterBill.ABL_RL_NKPortOfDischargeInfo, MandatoryValidation.YouHaveNotEntered);

			var headerUS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			headerUS.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerUS.AMA_RL_NKPortOfLoading = "PGXXX";
			headerUS.AMA_RL_NKPortOfDischarge = "";
			AssertHasMessageError(headerUS.MasterBill.ABL_RL_NKPortOfDischargeInfo, portOfDischargeMessage);
			AssertNoMessageError(headerUS.MasterBill.ABL_RL_NKPortOfDischargeInfo, iataPortOfDischargeMessage);
			headerUS.AMA_RL_NKPortOfDischarge = "US!2#";
			AssertNoMessageError(headerUS.MasterBill.ABL_RL_NKPortOfDischargeInfo, portOfDischargeMessage);
			AssertHasMessageError(headerUS.MasterBill.ABL_RL_NKPortOfDischargeInfo, iataPortOfDischargeMessage);
			headerUS.AMA_RL_NKPortOfDischarge = "USLAX";
			AssertNoMessageError(headerUS.MasterBill.ABL_RL_NKPortOfDischargeInfo, portOfDischargeMessage);
			AssertNoMessageError(headerUS.MasterBill.ABL_RL_NKPortOfDischargeInfo, iataPortOfDischargeMessage);
		}

		public void TestPortOfLoadingRequiresIATA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.PapuaNewGuinea, "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var iataPortOfLoadingMessage = "IATA is required for Port Of Loading";
			var za2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfLoading, iataPortOfLoadingMessage, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(universalAlias.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, "Desc.", universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.UnitedStates, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule);
			za2.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			iataPortOfLoadingMessage += " for US.";

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "US!2#";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = ZString.Empty;
			Factory.Save();

			var headerPG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.PapuaNewGuinea, "ASY");
			headerPG.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerPG.AMA_RL_NKPortOfLoading = "";
			headerPG.AMA_RL_NKPortOfDischarge = "PGXXX";
			AssertHasMessageErrorContaining(headerPG.MasterBill.ABL_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarnings(headerPG.MasterBill.ABL_RL_NKPortOfLoadingInfo);
			headerPG.AMA_RL_NKPortOfLoading = "PGXXX";
			AssertNoMessageErrorContaining(headerPG.MasterBill.ABL_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

			var headerUS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			headerUS.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerUS.AMA_RL_NKPortOfLoading = "US!2#";
			AssertHasMessageError(headerUS.MasterBill.ABL_RL_NKPortOfLoadingInfo, iataPortOfLoadingMessage);
			headerUS.AMA_RL_NKPortOfLoading = "USLAX";
			AssertNoMessageError(headerUS.MasterBill.ABL_RL_NKPortOfLoadingInfo, iataPortOfLoadingMessage);
		}

		public void TestChangingPortsAfterCreationEnsuresThatAtLeastOneIsSupportedAndThatForBothToBeSupportedYouMustBeLoggedIntoOne()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVSEA = helper.CreateNewOrGetExistingCusCodeList("VU", RefCusCodeListTypes.Codes.CustomsOffice, "VSEA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVSEA.PK, "SEA", "VUVLI");
			var pgWWK = helper.CreateNewOrGetExistingCusCodeList("PG", RefCusCodeListTypes.Codes.CustomsOffice, "WWK", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(pgWWK.PK, "PORT", "PGWWK");
			Factory.Save();
			CombineAssertions(() =>
			{
				foreach (ZString country in new[] { "VU", "PG", "SG", "GB" })
				{
					Assert($"Precondition: {country} is supported country", country.IsSupportedCountries(Factory));
				}

				foreach (ZString country in new[] { "AU", "NZ" })
				{
					Assert($"Precondition: {country} is not supported country", !country.IsSupportedCountries(Factory));
				}
			});
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "whatever";
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			header.AMA_RL_NKPortOfLoading = "VUVLI";
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			header.AMA_RL_NKPortOfLoading = "PGXXX";
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			header.AMA_RL_NKPortOfLoading = "PGXXX";
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "VUVLI";
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			AssertHasError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			AssertHasError(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			header.AMA_RL_NKPortOfLoading = "PGXXX";
			header.AMA_RL_NKPortOfDischarge = "GBLHR";
			AssertNoError(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);
			AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ValidationConstants.ManifestMustGoThruSupportedCountries);

			header.AMA_RL_NKPortOfDischarge = "";
			AssertHasMessageErrorContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "not entered");
			header.AMA_RL_NKPortOfDischarge = "PGXXX";
			AssertNoWarningContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "not entered");
			header.AMA_RL_NKPortOfLoading = "";
			AssertHasMessageErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "not entered");
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			AssertNoMessageErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "not entered");
		}

		public void TestCheckABL_RL_NKPortOfDischarge_PortCodeMustBe5Characters()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var unloco = Factory.New<RefUNLOCO>();
			unloco.Code = "PG3LL";
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "PGX";
			AssertHasErrorContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ListValidation.InvalidCodeError);

			header.AMA_RL_NKPortOfDischarge = "PG3LL";
			AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckABL_RL_NKPortOfLoading_PortCodeMustBe5Characters()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var unloco = Factory.New<RefUNLOCO>();
			unloco.Code = "PG3LL";
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "PGX";
			AssertHasErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeError);

			header.AMA_RL_NKPortOfLoading = "PG3LL";
			AssertNoErrorContaining(header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckABL_E_ARV()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "ETA is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.EstimatedTimeOfArrivalAtBorder, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(validationRule.PK, Core.Constants.TransportModes.Air);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_E_ARV = ZDateTime.Now;
			AssertNoMessageErrors(header.MasterBill.ABL_E_ARVInfo);
		}

		public void TestCheckABL_CustomsLoadPort()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "ATAIHR");
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3RL-001", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3RL-002", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3RL-003", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3LL-001", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3LL-002", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.Code = "TR3RL";
			var customsPort1 = unloco1.RefLocoMaps.AddNew();
			customsPort1.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort1.RY_RN = header.Country.PK;
			customsPort1.RY_LocalPortCode = "TR3RL-001";
			var customsPort2 = unloco1.RefLocoMaps.AddNew();
			customsPort2.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort2.RY_RN = header.Country.PK;
			customsPort2.RY_LocalPortCode = "TR3RL-002";

			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.Code = Core.Constants.CountryCodes.Turkey + "3LL";
			var customsPort3 = unloco2.RefLocoMaps.AddNew();
			customsPort3.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort3.RY_RN = header.Country.PK;
			customsPort3.RY_LocalPortCode = "TR3LL-001";
			var customsPort4 = unloco2.RefLocoMaps.AddNew();
			customsPort4.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort4.RY_RN = header.Country.PK;
			customsPort4.RY_LocalPortCode = "TR3LL-002";
			Factory.Save();
			AssertCustomsPortValidation(header.AMA_RL_NKPortOfLoadingInfo, header.AMA_CustomsLoadPortInfo);
		}

		public void TestCheckABL_CustomsDischargePort()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "ATAITH"); // make FeatureProvider.SupportsCustomsPorts = true
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea; // make FeatureProvider.SupportsCustomsPorts = true
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3RL-001", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3RL-002", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3RL-003", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3LL-001", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TR3LL-002", new ZDateTime(2019, 01, 01), new ZDateTime(2079, 06, 06));

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.Code = "TR3RL";
			var customsPort1 = unloco1.RefLocoMaps.AddNew();
			customsPort1.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort1.RY_RN = header.Country.PK;
			customsPort1.RY_LocalPortCode = "TR3RL-001";
			var customsPort2 = unloco1.RefLocoMaps.AddNew();
			customsPort2.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort2.RY_RN = header.Country.PK;
			customsPort2.RY_LocalPortCode = "TR3RL-002";

			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.Code = Core.Constants.CountryCodes.Turkey + "3LL";
			var customsPort3 = unloco2.RefLocoMaps.AddNew();
			customsPort3.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort3.RY_RN = header.Country.PK;
			customsPort3.RY_LocalPortCode = "TR3LL-001";
			var customsPort4 = unloco2.RefLocoMaps.AddNew();
			customsPort4.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			customsPort4.RY_RN = header.Country.PK;
			customsPort4.RY_LocalPortCode = "TR3LL-002";
			Factory.Save();
			AssertCustomsPortValidation(header.AMA_RL_NKPortOfDischargeInfo, header.AMA_CustomsDischargePortInfo);
		}

		void AssertCustomsPortValidation(ZPropertyInfo unlocoPortInfo, ZPropertyInfo customsPortInfo)
		{
			var invalidPortErrorMessage = ValidationConstants.InvalidPort(unlocoPortInfo.HumanReadableName);

			unlocoPortInfo.Value = new ZString("TR3RL");
			customsPortInfo.Value = new ZString("AAAAA");
			AssertHasMessageErrorContaining(customsPortInfo, ListValidation.InvalidCodeMessageError);

			customsPortInfo.Value = new ZString("TR3RL-001");
			AssertNoMessageErrorContaining(customsPortInfo, ListValidation.InvalidCodeMessageError);

			unlocoPortInfo.Value = new ZString("TR3LL");
			customsPortInfo.Value = new ZString("");
			AssertHasMessageErrorContaining(customsPortInfo, "You have not entered a value.");

			customsPortInfo.Value = new ZString("AAAAA");
			AssertHasMessageErrorContaining(customsPortInfo, ListValidation.InvalidCodeMessageError);

			customsPortInfo.Value = new ZString("TR3RL-003");
			AssertHasMessageErrorContaining(customsPortInfo, invalidPortErrorMessage);

			customsPortInfo.Value = new ZString("TR3LL-002");
			AssertNoMessageErrorContaining(customsPortInfo, invalidPortErrorMessage);
		}
	}
}
