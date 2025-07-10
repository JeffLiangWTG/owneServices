using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_TransportMeans_RuleC0646()
		{
			var message = "[C0646] Transport ID & Code shouldn't be entered for this transport mode.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_TransportMeans = "10";
				AssertNoMessageError("Rule C0646: Import, not UCC6, JE_TransportMode is not equal to MAI or ROA, JE_TransportMeans is not empty, there should be no message error.",
					declaration.JE_TransportMeansInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.Validation.ValidateJE_TransportMeans();
				AssertNoMessageError("Rule C0646: Import, not UCC6, JE_TransportMode is equal to MAI, JE_TransportMeans is not empty, there should be no message error.",
					declaration.JE_TransportMeansInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				declaration.Validation.ValidateJE_TransportMeans();
				AssertNoMessageError("Rule C0646: Import, not UCC6, JE_TransportMode is equal to ROA, JE_TransportMeans is not empty, there should be no message error.",
					declaration.JE_TransportMeansInfo, message);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.JE_TransportMeans = "10";
				AssertNoMessageError("Rule C0646: Export, UCC6, JE_TransportMode is equal to MAI, JE_TransportMeans is not empty, there should be no message error.",
					declaration.JE_TransportMeansInfo, message);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.Validation.ValidateJE_TransportMeans();
				AssertNoMessageError("Rule C0646: Import, UCC6, JE_TransportMode is not equal to MAI or ROA, JE_TransportMeans is not empty, there should be no message error.",
					declaration.JE_TransportMeansInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.JE_TransportMeans = "10";
				AssertHasMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to MAI, JE_TransportMeans is not empty, there should be a message error.",
					declaration.JE_TransportMeansInfo, message);

				declaration.JE_TransportMeans = ZString.Empty;
				AssertNoMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to MAI, JE_TransportMeans is empty, there should be no message error.",
					declaration.JE_TransportMeansInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				declaration.JE_TransportMeans = "10";
				AssertHasMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to ROA, JE_TransportMeans is not empty, there should be a message error.",
					declaration.JE_TransportMeansInfo, message);

				declaration.JE_TransportMeans = ZString.Empty;
				AssertNoMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to ROA, JE_TransportMeans is empty, there should be no message error.",
					declaration.JE_TransportMeansInfo, message);
			}
		}

		public void TestCheckJE_TransportMeans_RuleC0623()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			CombineAssertions("In case Rule C0623 is active, there is one entry instruction and (i) Sub type in C or F or Procedure = 71 and (ii) arrival transport means is entered, then blue message error has to be shown. In case there are multiple entry instructions, the blue message error should be replaced by an orange warning.", () =>
			{
				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "C", "XX", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True);
				AssertArrivalTransportMeansMessage(declaration, "2", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "D", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, "3", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "F", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True);

				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.False, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, "4", isEntryTypeImport: ZBool.True, isUCC6: ZBool.False, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);

				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.False, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "C", "XX", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);

				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "D", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "F", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);

				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True);

				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "C", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "D", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "F", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);

				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "C", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True);
				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "D", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True);
				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.False, "F", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True);

				AssertArrivalTransportMeansMessage(declaration, ZString.Empty, isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.True, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.True, "C", ZString.Empty, hasOrangeWarning: ZBool.True, hasBlueError: ZBool.False);
				AssertArrivalTransportMeansMessage(declaration, "1", isEntryTypeImport: ZBool.True, isUCC6: ZBool.True, hasMultipleEntryInstructions: ZBool.True, ZString.Empty, "71", hasOrangeWarning: ZBool.True, hasBlueError: ZBool.False);
			});
		}

		public void TestCheckJE_ShipmentIncoTerm()
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader1 = declaration.Invoices.AddNew();
				invoiceHeader1.JZ_IncoTerm = "2";
				var invoiceHeader2 = declaration.Invoices.AddNew();
				invoiceHeader2.JZ_IncoTerm = "2";

				declaration.JE_ShipmentIncoTerm = "1";
				AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "Incoterm values do not match between Declaration and Invoice Header.");

				declaration.JE_ShipmentIncoTerm = "2";
				AssertNoMessageError(declaration.JE_ShipmentIncoTermInfo, "Incoterm values do not match between Declaration and Invoice Header.");

				invoiceHeader2.JZ_IncoTerm = "3";
				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "Incoterm values do not match between Declaration and Invoice Header.");
			}
		}

		public void TestCheckJE_ShipmentIncoTerm_ForTransport()
		{
			CombineAssertions(() =>
			{
				AssertIncoTermValidation("FAS", true);
				AssertIncoTermValidation("FOB", true);
				AssertIncoTermValidation("CFR", true);
				AssertIncoTermValidation("CIF", true);

				AssertIncoTermValidation("EXW", false);
				AssertIncoTermValidation("CIP", false);
			});
		}

		void AssertIncoTermValidation(ZString incoterm, ZBool shouldHaveWarningIfNotWaterTransportMode)
		{
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "SEA", false);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "AIR", shouldHaveWarningIfNotWaterTransportMode);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "IWT", false);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, ZString.Empty, shouldHaveWarningIfNotWaterTransportMode);
		}

		void AssertIncoTermValidationForTransportAndIncoTerm(ZString incoterm, ZString transportMode, ZBool shouldHaveWarningIfNotWaterTransportMode)
		{
			var warningMessage = "is only valid for sea and inland waterway transport. Please check against the transport mode.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ShipmentIncoTerm = incoterm;

			declaration.JE_TransportMode = transportMode;
			if (shouldHaveWarningIfNotWaterTransportMode)
			{
				AssertHasWarningContaining("There is a warning when transport is " + transportMode + " and incoterm is " + incoterm, declaration.JE_ShipmentIncoTermInfo, warningMessage);
			}
			else
			{
				AssertNoWarningContaining("No warning when transport is " + transportMode + " and incoterm is " + incoterm, declaration.JE_ShipmentIncoTermInfo, warningMessage);
			}
		}

		public void TestCheckJE_IATALoadPortExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var iataType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "EU IATA");
			var iataPort = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "KD1", "KD1 DESC", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1));
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNoMessageErrors(dec.JE_IATALoadPortInfo);
			dec.JE_IATALoadPort = "XXX";
			AssertHasMessageErrorContaining(dec.JE_IATALoadPortInfo, ListValidation.InvalidCodeMessageError);
			dec.JE_IATALoadPort = "KD1";
			AssertNoMessageErrors(dec.JE_IATALoadPortInfo);
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_IATALoadPort = "XXX";
			AssertNoMessageErrors(dec.JE_IATALoadPortInfo);

			var invoice = dec.Invoices.AddNew();
			var fakeDec = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			fakeDec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			fakeDec.JE_MessageType = MessageTypeList.Codes.Export;
			fakeDec.JE_IATALoadPort = "XXX";
			AssertNoMessageErrors(fakeDec.JE_IATALoadPortInfo);
		}
		public void TestCheckJE_IATALoadPortImportCO()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, "FR", new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			var airportFR = Factory.New<RefUNLOCO>();
			airportFR.RL_RN_NKCountryCode = "FR";
			airportFR.RL_Code = "FRCD1";
			airportFR.RL_HasAirport = true;
			airportFR.RL_IATA = "CD1";
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			dec.JE_IATALoadPort = "CD1";
			CombineAssertions(() =>
			{
				AssertNoMessageErrors("Information is not available at JE_IATALoadPortInfo:" + dec.JE_IATALoadPortInfo, dec.JE_IATALoadPortInfo);
				dec.JE_IATALoadPort = "JFK";
				AssertHasMessageErrors("Information is available at JE_IATALoadPortInfo:" + dec.JE_IATALoadPortInfo, dec.JE_IATALoadPortInfo);
			});
		}

		public void TestCheckJE_RL_NKPortOfFirstArrival()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
				var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
				helper.AddCountry(tradeGroup, "GB", new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

				Factory.Save();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
				dec.JE_MessageType = MessageTypeList.Codes.Import;
				AssertNoMessageErrors(dec.JE_RL_NKPortOfFirstArrivalInfo);
				dec.JE_RL_NKPortOfFirstArrival = "AUBNE";
				AssertHasMessageErrorContaining(dec.JE_RL_NKPortOfFirstArrivalInfo, "This port code is invalid. Please check against the transport mode and shipment type.");
				dec.JE_RL_NKPortOfFirstArrival = "GBLHR";
				AssertNoMessageErrors(dec.JE_RL_NKPortOfFirstArrivalInfo);
				dec.JE_MessageType = MessageTypeList.Codes.Export;
				dec.JE_RL_NKPortOfFirstArrival = "AUBNE";
				AssertNoMessageErrors(dec.JE_RL_NKPortOfFirstArrivalInfo);
			}
		}

		public virtual void TestCheckJE_DeclarantType()
		{
			var declaration = Factory.New<JobDeclaration>();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_DeclarantTypeInfo, "~", GetRepresentationTypeSelfToTest);

			declaration.JE_DeclarantType = GetRepresentationTypeSelfToTest;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public virtual void TestCheckJE_OA_DeclarantAddress()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.FillWithValidTestData();
			var declarantAddress = Factory.New<OrgAddress>();
			declarantAddress.OA_Address1 = "AD1";
			declarantAddress.OA_OH = declarant.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = GetRepresentationTypeSelfToTest;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJE_DateOfFirstArrivalDoesNotValidateWhenNotImportJob()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = new ZDateTime(2006, 1, 1);
			AssertNoNotifications(declaration.JE_DateOfFirstArrivalInfo);

			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 2, 1);
			AssertHasNotifications(declaration.JE_DateOfFirstArrivalInfo);
		}

		public void TestCheckJE_MessageSubType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "A!A";
			AssertNoNotifications(declaration.JE_MessageSubTypeInfo);
			declaration.JE_MessageSubType = ZString.Empty;
			AssertNoNotifications(declaration.JE_MessageSubTypeInfo);
		}

		public void TestCheckJE_EntryStyle()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStyle = "_!";
			AssertHasMessageError(declaration.JE_EntryStyleInfo, "The code you have selected is not in the list.");
			declaration.JE_EntryStyle = "";
			AssertNoErrors(declaration.JE_EntryStyleInfo);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			AssertNoNotifications(declaration.JE_EntryStyleInfo);

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			AssertHasMessageError(declaration.JE_EntryStyleInfo, "The code you have selected is not in the list.");

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNoErrors(declaration.JE_EntryStyleInfo);
		}

		public void TestValidateJE_PaymentMethod()
		{
			var declaration = SetUpDeclarationForTestValidateJE_PaymentMethod();

			AssertParty_TestValidateJE_PaymentMethod(declaration);

			var bcd = new List<string>()
			{
				DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority, // B
				DefermentMethodList.Codes.ConsigneesAccountStandingAuthority, // C
				DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration, // D
			};

			foreach (var letter in bcd)
			{
				AssertPartyForDefermentMethod_TestValidateJE_PaymentMethod(declaration, letter);
			}
		}

		protected virtual JobDeclaration SetUpDeclarationForTestValidateJE_PaymentMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;

				var declarant = Factory.New<OrgHeader>();
				declarant.FillWithValidTestData(); //Make sure this doesnt already have any unwanted info.
				declaration.JE_OA_DeclarantAddress = declarant.PK;
			}

			return declaration;
		}

		protected virtual void AssertParty_TestValidateJE_PaymentMethod(JobDeclaration declaration)
		{
			const string message = "The Declarant requires a Deferment Approval Number (DAN) for the relevant country/region, (Edit Organization > Details > Details > Registration Numbers / Codes)";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var ge = RefCountry.LoadFromCountryCode(Factory, Enterprise.Core.Constants.CountryCodes.Germany);

				declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14; // A
				AssertHasMessageError(declaration.JE_PaymentMethodInfo, message);

				declaration.Declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "", ge);
				declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14; // A
				AssertHasMessageError(declaration.JE_PaymentMethodInfo, message);
				declaration.Declarant.CustomsCodes.DeleteAll();

				declaration.Declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "GE actual value", ge);
				declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14; // A
				AssertNoMessageError(declaration.JE_PaymentMethodInfo, message);
			}
		}

		protected virtual void AssertPartyForDefermentMethod_TestValidateJE_PaymentMethod(JobDeclaration declaration, string letter)
		{
			const string message = "The Importer/Consignee requires a Deferment Approval Number (DAN) for the relevant country/region, (Edit Organization > Details > Details > Registration Numbers / Codes)";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var ge = RefCountry.LoadFromCountryCode(Factory, Enterprise.Core.Constants.CountryCodes.Germany);

				declaration.JE_PaymentMethod = letter;
				AssertHasMessageError(letter, declaration.JE_PaymentMethodInfo, message);

				declaration.Importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "", ge);
				declaration.JE_PaymentMethod = letter;
				AssertHasMessageError(letter, declaration.JE_PaymentMethodInfo, message);
				declaration.Importer.CustomsCodes.RemoveAll();

				declaration.Importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "GE actual value", ge);
				declaration.JE_PaymentMethod = letter;
				AssertNoMessageError(letter, declaration.JE_PaymentMethodInfo, message);
				declaration.Importer.CustomsCodes.RemoveAll();
			}
		}

		public void TestCheckJE_UCR_MessageErrorIfNotEntered()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_UCR = ZString.Empty;
			AssertNoMessageErrorContaining("If a declaration is not saved in the database, an empty UCR will not cause a validation error.", dec.JE_UCRInfo, MandatoryValidation.YouHaveNotEntered);

			Factory.Save();
			dec.JE_UCR = ZString.Empty;
			AssertHasMessageErrorContaining(dec.JE_UCRInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_UCR = "123";
			AssertNoMessageErrorContaining(dec.JE_UCRInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_UCR_Duplicate()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_UCR = "123";
			Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_UCR = "321";
			AssertNoMessageErrorContaining(dec2.JE_UCRInfo, "already in use");

			dec2.JE_UCR = "123";
			AssertHasMessageErrorContaining("An existing dec with the same reference should cause a validation error on the current dec", dec2.JE_UCRInfo, "already in use");
		}

		public void TestCheckJE_UCR_DuplicateWithADeclarationInOtherCountry()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_UCR = "33";
			Factory.Save();

			var decInSameCountry = Factory.New<JobDeclaration>();
			decInSameCountry.JE_UCR = "33";
			AssertHasMessageErrorContaining("An existing dec with the same country of origin and same reference should cause a validation error on the current dec", decInSameCountry.JE_UCRInfo, "This Unique Consignment Reference is already in use");

			var testObjectCreator = new TestObjectCreator(Factory);
			var auBranch = testObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var decInDifferentCountry = Factory.New<JobDeclaration>();
				decInDifferentCountry.JE_UCR = "33";
				AssertNoMessageErrorContaining("An existing dec with a different country of origin and same reference should not cause a validation error on the current dec", decInDifferentCountry.JE_UCRInfo, "This Unique Consignment Reference is already in use");
			}
		}

		public void TestCheckJE_UCR_DuplicateWithADeclarationCreatedOver10YearsAgo()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_UCR = "44";
			Factory.Save();

			var decCreatedOver10YearsAgo = Factory.New<JobDeclaration>();
			decCreatedOver10YearsAgo.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-11);
			decCreatedOver10YearsAgo.JE_UCR = "44";
			Factory.Save();
			dec.Validation.ValidateJE_UCR();
			AssertNoMessageErrorContaining("An existing dec created over 10 years ago with the same reference should not cause a validation error on the current dec", dec.JE_UCRInfo, "This Unique Consignment Reference is already in use");

			var decCreatedWithin10Years = Factory.New<JobDeclaration>();
			decCreatedWithin10Years.JE_UCR = "44";
			decCreatedWithin10Years.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-5);
			Factory.Save();
			dec.Validation.ValidateJE_UCR();
			AssertHasMessageErrorContaining("An existing dec created within the last 10 years with the same reference should cause a validation error on the current dec", dec.JE_UCRInfo, "This Unique Consignment Reference is already in use");
		}

		public void TestCheckJE_UCR_Duplicate_CaseOutsideEU()
		{
			// This should be able to load decs of all types, not just EU, without puking:
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var decAU = Factory.New<Customs.Business.BaseJobDeclaration>();
				decAU.JE_UCR = "BLAH";
				Factory.Save();
				var decAU1 = Factory.New<JobDeclaration>();
				decAU1.JE_RL_NKOrigin = "AUBNE";
				decAU1.JE_UCR = "BLAH";
				AssertHasMessageErrorContaining(decAU1.JE_UCRInfo, "This Unique Consignment Reference is already in use on another declaration in Australia");
			}
		}

		public void TestCheckJE_UCR_DuplicateWithASupplementaryDeclaration()
		{
			var masterDeclaration = Factory.New<JobDeclaration>();
			masterDeclaration.JE_UCR = "123";

			var supplementaryDeclaration = masterDeclaration.GetNewRelatedDeclaration(masterDeclaration.Factory, EUCommonConstants.DeclarationRelationshipType.SupplementaryDeclaration);
			supplementaryDeclaration.JE_UCR = "123";
			AssertNoMessageErrors("Sharing the same UCR between a declaration and its supplementary declaration should not cause a validation error on the supplementary declaration.", supplementaryDeclaration.JE_UCRInfo);

			masterDeclaration.Validation.ValidateJE_UCR();
			AssertNoMessageErrors("Sharing the same UCR between a declaration and its supplementary declaration should not cause a validation error on the master declaration.", masterDeclaration.JE_UCRInfo);
		}

		public void TestCheckJE_RN_NKTransportNationality()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_RN_NKTransportNationality = "~~";
			AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.China;
			AssertNoMessageErrors(declaration.JE_RN_NKTransportNationalityInfo);
			declaration.JE_RN_NKTransportNationality = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJE_RN_NKTrailer1Nationality_ListValidation()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RN_NKTrailer1NationalityInfo, "ZZ", Core.Constants.CountryCodes.Germany);
		}

		public void TestJE_RN_NKTrailer2Nationality_ListValidation()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RN_NKTrailer2NationalityInfo, "ZZ", Core.Constants.CountryCodes.Germany);
		}

		public void TestCheckJE_RN_NKTransportNationalityInland()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_RN_NKTransportNationalityInland = "~~";
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Invalid country code", declaration.JE_RN_NKTransportNationalityInlandInfo, ListValidation.InvalidCodeMessageError.ToString());
				declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.China;
				AssertNoMessageErrors("Valid country", declaration.JE_RN_NKTransportNationalityInlandInfo);
				declaration.JE_RN_NKTransportNationalityInland = ZString.Empty;
				AssertNoMessageErrorContaining("Empty country", declaration.JE_RN_NKTransportNationalityInlandInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public virtual void TestCheckJE_CustomsOffice()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			dec.JE_CustomsOffice = "123";
			AssertNoMessageErrorContaining(dec.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			dec.JE_CustomsOffice = ZString.Empty;
			AssertHasMessageErrorContaining(dec.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			dec.JE_CustomsOffice = "123";
			AssertNoMessageErrorContaining(dec.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec1 = Factory.New<JobDeclaration>();
				dec1.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				dec1.JE_CustomsOffice = ZString.Empty;
				AssertNoMessageErrorContaining(dec1.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				dec1.JE_CustomsOffice = "123";
				AssertNoMessageErrorContaining(dec1.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckCustomsOffices()
		{
			var cauMessage = "The declaration requires an office of type Competent Authority Country of Dep. with purpose CAU.";
			var depMessage = "The declaration requires an office of type Office of Departure with purpose DEP.";

			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement("CAU", true, true),
				new CustomsOfficeRequirement("DEP", false, true)
			});

			declaration.RunPreSaveValidation();
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, cauMessage);
			AssertNoMessageError(declaration.JE_CustomsOfficeInfo, depMessage);

			var office = declaration.CustomsOffices.AddNew("CAU");
			declaration.RunPreSaveValidation();
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, cauMessage);
			AssertNoMessageError(declaration.JE_CustomsOfficeInfo, depMessage);

			office.CY_Data = "GB0001";
			declaration.RunPreSaveValidation();
			AssertNoMessageError(declaration.JE_CustomsOfficeInfo, cauMessage);
			AssertNoMessageError(declaration.JE_CustomsOfficeInfo, depMessage);

			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
			{
				new CustomsOfficeRequirement("CAU", true, true),
				new CustomsOfficeRequirement("DEP", true, true)
			});

			declaration.RunPreSaveValidation();
			AssertNoMessageError(declaration.JE_CustomsOfficeInfo, cauMessage);
			AssertHasMessageError(declaration.JE_CustomsOfficeInfo, depMessage);
		}

		public void TestCheckJE_GoodsOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType("IM15", "List validation for origin country/territory for entry style IM");
			helper.CreateNewOrGetExistingCusCodeType("CO15", "List validation for origin country/territory for entry style CO");

			helper.CreateCusCodeList("ES", "IM15", "XA", "Test A", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", "IM15", "XC", "Test C", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
				declaration.JE_GoodsOrigin = ZString.Empty;
				AssertNoMessageErrorContaining(declaration.JE_GoodsOriginInfo, ListValidation.InvalidCodeMessageError);
				dec.JE_GoodsOrigin = "XA";
				AssertNoMessageErrors(dec.JE_GoodsOriginInfo);
				dec.JE_GoodsOrigin = "ES";
				AssertHasMessageErrorContaining(dec.JE_GoodsOriginInfo, ListValidation.InvalidCodeMessageError);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
				dec.JE_GoodsOrigin = "ES";
				AssertEquals(0, dec.Lookups.GoodsOrigin.Count);
				AssertNoMessageErrorContaining(dec.JE_GoodsOriginInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckJE_GoodsDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType("IM17", "List validation for destination country/territory for entry style IM");
			helper.CreateNewOrGetExistingCusCodeType("CO17", "List validation for destination country/territory for entry style CO");

			helper.CreateCusCodeList("ES", "IM17", "XA", "Test A", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("ES", "IM17", "XC", "Test C", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
				dec.JE_GoodsDestination = "XA";
				AssertNoMessageErrors(dec.JE_GoodsDestinationInfo);
				dec.JE_GoodsDestination = "ES";
				AssertHasMessageErrorContaining(dec.JE_GoodsDestinationInfo, ListValidation.InvalidCodeMessageError);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
				dec.JE_GoodsDestination = "ES";
				AssertEquals(0, dec.Lookups.GoodsOrigin.Count);
				AssertNoMessageErrorContaining(dec.JE_GoodsDestinationInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckJE_TransportModeInland()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_TransportModeInland = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_TransportModeInlandInfo);

			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.OwnPropulsion;
			AssertNoMessageErrors(declaration.JE_TransportModeInlandInfo);

			declaration.JE_TransportModeInland = "***";
			AssertHasMessageErrorContaining(declaration.JE_TransportModeInlandInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_TransportModeInland_C0841()
		{
			var expectedMessage = "[C0841] Only for declarations with Sub Style D, E or F the Departure Mode of Transport is optional, in the initial 515 message ELSE required.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Prelodged;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = EntryStyleListExportUCC.Codes.ExportNormal;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;

			CombineAssertions(() =>
			{
				using (var context = new DeclarationValidationDeciderTestContext(declaration))
				{
					context.DisableRule(r => r.IsRuleC0841Active);
					declaration.JE_CustomsOffice = "NL000001";
					var exitOffice = declaration.CustomsOffices.FirstOrDefault();
					exitOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
					exitOffice.CY_Data = "NL000002";
					declaration.Validation.ValidateJE_TransportModeInland();
					AssertNoMessageError("Rule C0841 is not active, no message", declaration.JE_TransportModeInlandInfo, expectedMessage);

					context.EnableRule(r => r.IsRuleC0841Active);
					declaration.Validation.ValidateJE_TransportModeInland();					
					AssertHasMessageError("Rule C0841 is active and all conditions are true", declaration.JE_TransportModeInlandInfo, expectedMessage);
					entryHeader.CH_EntryStatus = "";
					declaration.Validation.ValidateJE_TransportModeInland();
					AssertNoMessageError("RuleC0841 is active, Customs Status is not 'PRE'", declaration.JE_TransportModeInlandInfo, expectedMessage);
					entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Prelodged;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
					declaration.Validation.ValidateJE_TransportModeInland();
					AssertNoMessageError("Rule C0841 is active, SubStyle is D, no message", declaration.JE_TransportModeInlandInfo, expectedMessage);
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					declaration.JE_CustomsOffice = "NL000002";
					declaration.Validation.ValidateJE_TransportModeInland();
					AssertNoMessageError("Rule C0841 is active, customsOffice is equal to exitOffice", declaration.JE_TransportModeInlandInfo, expectedMessage);
					declaration.JE_CustomsOffice = "NL000001";
					declaration.JE_TransportModeInland = "ROA";
					declaration.Validation.ValidateJE_TransportModeInland();
					AssertNoMessageError("Rule C0841 is active, Inland MOT is filled", declaration.JE_TransportModeInlandInfo, expectedMessage);
					declaration.JE_TransportModeInland = string.Empty;
					declaration.JE_CustomsOffice = string.Empty;
					exitOffice.CY_Data = string.Empty;
					declaration.Validation.ValidateJE_TransportModeInland();
					AssertNoMessageError("Rule C0841 is active, customs offices are empty", declaration.JE_TransportModeInlandInfo, expectedMessage);
				}
			});
		}

		public void TestCheckRuleC0843_JE_TransportModeInland()
		{
			var errorMessage = "[C0843] If declaration office is not equal to exit office AND declaration Sub Style is not equal to D, E or F then the Inland Mode of Transport is required.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (var context = new DeclarationValidationDeciderTestContext(declaration))
			{
				context.DisableRule(r => r.IsRuleC0843Active);

				declaration.JE_CustomsOffice = "NL000123";
				declaration.CustomsOffices[0].CY_Code = "EXT";
				declaration.CustomsOffices[0].CY_Data = "NL000234";
				entryInstruction.CEI_SubStyle = "V";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("Rule C0843 is Deactive", declaration.JE_TransportModeInlandInfo, errorMessage);

				context.EnableRule(r => r.IsRuleC0843Active);

				entryInstruction.CEI_SubStyle = "D";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("Rule C0843 is Active and TransportModeInland is empty but no SubStyle is other than D, E or F.", declaration.JE_TransportModeInlandInfo, errorMessage);

				entryInstruction.CEI_SubStyle = "V";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertHasMessageErrorContaining("Rule C0843 is Active, TransportModeInland is empty, SubStyle is V, and declaration office is not equal to exit office", declaration.JE_TransportModeInlandInfo, errorMessage);

				declaration.CustomsOffices[0].CY_Data = "NL000123";
				declaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageErrorContaining("Rule C0843 is Active, TransportModeInland is empty, SubStyle is V, but declaration office is equal to exit office", declaration.JE_TransportModeInlandInfo, errorMessage);

				declaration.CustomsOffices[0].CY_Data = "NL000234";
				declaration.JE_TransportModeInland = "ABC";
				AssertNoMessageErrorContaining("Rule C0843 is Active, TransportModeInland is not empty", declaration.JE_TransportModeInlandInfo, errorMessage);
			}
		}

		public virtual void TestCheckJE_LocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				using (EUCustomsDataRegistry.Instance.ExportAuthorizedLocationCheck.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					declaration.JE_LocationOfGoods = ZString.Empty;
					AssertNoWarning("Registry 'Authorized Location Check' = false - Empty location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
					AssertNoMessageError("Registry 'Authorized Location Check' = false - Empty location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());

					declaration.JE_LocationOfGoods = "***";
					AssertHasWarning("Registry 'Authorized Location Check' = false - Invalid location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
					AssertNoMessageError("Registry 'Authorized Location Check' = false - Invalid location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
				}

				using (EUCustomsDataRegistry.Instance.ExportAuthorizedLocationCheck.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					declaration.JE_LocationOfGoods = ZString.Empty;
					AssertNoWarning("Registry 'Authorized Location Check' = true - Empty location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
					AssertNoMessageError("Registry 'Authorized Location Check' = true - Empty location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());

					declaration.JE_LocationOfGoods = "***";
					AssertNoWarning("Registry 'Authorized Location Check' = true - Invalid location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
					AssertHasMessageError("Registry 'Authorized Location Check' = true - Invalid location", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
				}

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_LocationOfGoods = "***";
				AssertNoWarning("Import declaration, no validation", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
				AssertNoMessageError("Import declaration, no validation", declaration.JE_LocationOfGoodsInfo, JobDeclarationValidation.GoodsLocationNotAuthorizedLocationMessage.ToString());
			});
		}

		public void TestCheckJE_TotalWeight()
		{
			using (CustomsDataRegistry.Instance.SeverityLevelOfTotalWeightValidation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertNoNotifications(declaration.JE_TotalWeightInfo);

				var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line.JI_Weight = 0;
				line.JI_WeightUQ = "KG";

				var line2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line2.JI_Weight = 0;
				line2.JI_WeightUQ = "KG";

				declaration.JE_TotalWeightUnit = "KG";
				declaration.JE_TotalWeight = 101;
				AssertNoWarnings("Total Gross weight on declaration equals to the sum of Gross Weight of individual lines.", declaration.JE_TotalWeightInfo);

				line.JI_Weight = 50;
				line2.JI_Weight = 51;
				Factory.Save();
				AssertNoWarnings("Total Gross weight on declaration equals to the sum of Gross Weight of individual lines.", declaration.JE_TotalWeightInfo);

				declaration.JE_TotalWeight = 102m;
				AssertHasWarning("Total Gross weight on declaration is greater than the sum of Gross Weight of individual lines.", declaration.JE_TotalWeightInfo, declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

				declaration.JE_TotalWeight = 50m;
				AssertHasWarning("Total Gross weight on declaration is lesser than the sum of Gross Weight of individual lines.", declaration.JE_TotalWeightInfo, declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

				line2.JI_Weight = 51;
				line2.JI_WeightUQ = "G";
				declaration.JE_TotalWeight = 101;
				Factory.Save();
				AssertHasWarning("Total Gross weight on declaration is lesser than the sum of Gross Weight of individual lines, with grams unit.", declaration.JE_TotalWeightInfo, declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

				line2.JI_Weight = 51000;
				line2.JI_WeightUQ = "G";
				declaration.JE_TotalWeight = 101;
				Factory.Save();
				AssertNoWarnings("Total Gross weight on declaration is equals than the sum of Gross Weight of individual lines, with grams unit.", declaration.JE_TotalWeightInfo);
			}
		}

		public void TestCheckRuleC0002_Destination()
		{
			var messageForDeclaration = "[C0002] Value can't be entered in both Declaration and Invoice lines.";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceheader1 = declaration.Invoices.AddNew();
			var invoiceheader2 = declaration.Invoices.AddNew();

			var invoiceLine1_1 = invoiceheader1.InvoiceLines.AddNew();
			invoiceheader1.InvoiceLines.AddNew();

			var invoiceLine2_1 = invoiceheader2.InvoiceLines.AddNew();
			invoiceheader2.InvoiceLines.AddNew();

			using (var context = new DeclarationValidationDeciderTestContext(declaration))
			{
				context.DisableRule(r => r.IsRuleC0002Active);

				invoiceLine1_1.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				AssertNoMessageErrorContaining("Rule C0002 is Deactive : declaration and invoiceLine1_1 has value for destination", declaration.JE_GoodsDestinationInfo, messageForDeclaration);

				declaration.JE_GoodsDestination = ZString.Empty;
				invoiceLine1_1.ZG_CountryOfDestination = ZString.Empty;

				context.EnableRule(r => r.IsRuleC0002Active);
				invoiceLine1_1.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				AssertHasMessageErrorContaining("Rule C0002 is Active : declaration and invoiceLine1_1 has value for destination", declaration.JE_GoodsDestinationInfo, messageForDeclaration);

				context.DisableRule(r => r.IsRuleC0002Active);
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				AssertNoMessageErrorContaining("Rule C0002 is Disable : declaration and invoiceLine1_1 has value for destination", declaration.JE_GoodsDestinationInfo, messageForDeclaration);

				context.EnableRule(r => r.IsRuleC0002Active);

				declaration.JE_GoodsDestination = ZString.Empty;
				AssertNoMessageErrorContaining("Rule C0002 is Active :declaration has no value and one invoiceLine has value for destination", declaration.JE_GoodsDestinationInfo, messageForDeclaration);

				invoiceLine1_1.ZG_CountryOfDestination = ZString.Empty;
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				AssertNoMessageErrorContaining("Rule C0002 is Active : declaration has value and none of its invoiceLines have value for destination", declaration.JE_GoodsDestinationInfo, messageForDeclaration);

				declaration.JE_GoodsDestination = ZString.Empty;
				AssertNoMessageErrorContaining("Rule C0002 is Active : declaration has no value and none of its invoiceLines have value for destination", declaration.JE_GoodsDestinationInfo, messageForDeclaration);
			}
		}

		public void TestValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					var jobDeclarationValidation = new JobDeclarationValidation(declaration);
					var declarationValidationDecider = jobDeclarationValidation.ValidationDecider;
					AssertType<UCC6ImportDeclarationValidationDecider>("IMP UCC6 declaration", declarationValidationDecider);
					AssertSame("Cached ", declarationValidationDecider, jobDeclarationValidation.ValidationDecider);
					declaration.JE_MessageType = "EXP";
					AssertType<UCC6ExportDeclarationValidationDecider>("EXP UCC6 declaration", new JobDeclarationValidation(declaration).ValidationDecider);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = "IMP";
					AssertNull("IMP declaration not UCC6", new JobDeclarationValidation(declaration).ValidationDecider);
					declaration.JE_MessageType = "EXP";
					AssertNull("EXP declaration not UCC6", new JobDeclarationValidation(declaration).ValidationDecider);
				}
			});
		}

		protected virtual string GetRepresentationTypeSelfToTest => RepresentationTypeList.Codes._1Self;

		protected virtual string GetRepresentationTypeDirectToTest => RepresentationTypeList.Codes._2Direct;

		void AssertArrivalTransportMeansMessage(JobDeclaration declaration, ZString transportMeans, bool isEntryTypeImport, bool isUCC6, ZBool hasMultipleEntryInstructions, ZString subStyle, ZString procedure, bool hasOrangeWarning, bool hasBlueError)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6))
			{
				var entryInstruction = declaration.CustomsEntryInstructions[0];

				if (declaration.CustomsEntryInstructions.Count == 0)
				{
					entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				}

				if (hasMultipleEntryInstructions && declaration.CustomsEntryInstructions.Count == 1)
				{
					declaration.CustomsEntryInstructions.AddNew();
				}

				if (isEntryTypeImport)
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				}
				else
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				}

				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = procedure;
				declaration.JE_TransportMeans = transportMeans;

				var message = "[C0623] Arrival Transport means must not be entered for this declaration type or requested procedure.";

				if (hasOrangeWarning)
				{
					AssertHasWarning(declaration.JE_TransportMeansInfo, message);
				}
				else
				{
					AssertNoWarning(declaration.JE_TransportMeansInfo, message);
				}

				if (hasBlueError)
				{
					AssertHasMessageError(declaration.JE_TransportMeansInfo, message);
				}
				else
				{
					AssertNoMessageError(declaration.JE_TransportMeansInfo, message);
				}
			}
		}
	}
}
