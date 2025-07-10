using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_Box18TransportID_RuleC0646()
		{
			var message = "[C0646] Transport ID & Code shouldn't be entered for this transport mode.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.ZG_Box18TransportID = "10";
				AssertNoMessageError("Rule C0646: Import, not UCC6, JE_TransportMode is not equal to MAI or ROA, ZG_Box18TransportID is not empty, there should be no message error.",
					declaration.ZG_Box18TransportIDInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.AddInfoValidation.ValidateZG_Box18TransportID();
				AssertNoMessageError("Rule C0646: Import, not UCC6, JE_TransportMode is equal to MAI, ZG_Box18TransportID is not empty, there should be no message error.",
					declaration.ZG_Box18TransportIDInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				declaration.AddInfoValidation.ValidateZG_Box18TransportID();
				AssertNoMessageError("Rule C0646: Import, not UCC6, JE_TransportMode is equal to ROA, ZG_Box18TransportID is not empty, there should be no message error.",
					declaration.ZG_Box18TransportIDInfo, message);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.ZG_Box18TransportID = "10";
				AssertNoMessageError("Rule C0646: Export, UCC6, JE_TransportMode is equal to MAI, ZG_Box18TransportID is not empty, there should be no message error.",
					declaration.ZG_Box18TransportIDInfo, message);

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.AddInfoValidation.ValidateZG_Box18TransportID();
				AssertNoMessageError("Rule C0646: Import, UCC6, JE_TransportMode is not equal to MAI or ROA, ZG_Box18TransportID is not empty, there should be no message error.",
					declaration.ZG_Box18TransportIDInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.ZG_Box18TransportID = "10";
				AssertHasMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to MAI, ZG_Box18TransportID is not empty, there should be a message error.",
					declaration.ZG_Box18TransportIDInfo, message);

				declaration.ZG_Box18TransportID = ZString.Empty;
				AssertNoMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to MAI, ZG_Box18TransportID is empty, there should be no message error.",
					declaration.ZG_Box18TransportIDInfo, message);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				declaration.ZG_Box18TransportID = "10";
				AssertHasMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to ROA, ZG_Box18TransportID is not empty, there should be a message error.",
					declaration.ZG_Box18TransportIDInfo, message);

				declaration.ZG_Box18TransportID = ZString.Empty;
				AssertNoMessageError("Rule C0646: Import, UCC6, JE_TransportMode is equal to ROA, ZG_Box18TransportID is empty, there should be no message error.",
					declaration.ZG_Box18TransportIDInfo, message);
			}
		}

		public void TestCheckZG_Box18TransportID_RuleC0623()
		{
			var isRuleC0623Enable = true;
			CombineAssertions("In case RuleC0623 is Enable, there is one entry instruction and (i) Sub type in C or F or Procedure = 71 and (ii) arrival transport means is entered, then blue message error has to be shown. In case there are multiple entry instructions, the blue message error should be replaced by an orange warning.", () =>
			{
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "120", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "121", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "D", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "122", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "F", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "123", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "130", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "D", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "F", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "124", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "C", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "D", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "F", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "125", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "C", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "126", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "D", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "127", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "F", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.True, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, ZString.Empty, ZInt.Zero, hasMultipleEntryInstructions: ZBool.True, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "128", ZInt.Zero, hasMultipleEntryInstructions: ZBool.True, "C", ZString.Empty, hasOrangeWarning: ZBool.True, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "129", ZInt.Zero, hasMultipleEntryInstructions: ZBool.True, ZString.Empty, "71", hasOrangeWarning: ZBool.True, hasBlueError: ZBool.False, isRuleC0623Enable);
			});

			isRuleC0623Enable = false;
			CombineAssertions("In case RuleC0623 is Disable, there is one entry instruction and (i) Sub type in C or F or Procedure = 71 and (ii) arrival transport means is entered, then blue message error has to be shown. In case there are multiple entry instructions, the blue message error should be replaced by an orange warning.", () =>
			{
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "120", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "122", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "F", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "124", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, ZString.Empty, "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "125", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "C", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "126", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "D", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "127", ZInt.Zero, hasMultipleEntryInstructions: ZBool.False, "F", "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);

				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "128", ZInt.Zero, hasMultipleEntryInstructions: ZBool.True, "C", ZString.Empty, hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
				AssertArrivalTransportMeansMessage(declaration.ZG_Box18TransportIDInfo, "129", ZInt.Zero, hasMultipleEntryInstructions: ZBool.True, ZString.Empty, "71", hasOrangeWarning: ZBool.False, hasBlueError: ZBool.False, isRuleC0623Enable);
			});
		}

		public void TestCheckZG_Box18TransportNationality()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				declaration.ZG_Box18TransportNationality = "~~";
				AssertHasMessageError("Invalid Code", declaration.ZG_Box18TransportNationalityInfo, ListValidation.InvalidCodeMessageError);
				declaration.ZG_Box18TransportNationality = "CN";
				AssertNoMessageError("Valid Code", declaration.ZG_Box18TransportNationalityInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckZG_AgreedPlaceCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "TEST DESC", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				declaration.ZG_AgreedPlaceCode = "X";
				AssertHasMessageError("Invalid Code", declaration.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);
				declaration.ZG_AgreedPlaceCode = "1";
				AssertNoMessageError("Valid Code", declaration.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckZG_AgreedPlaceCodeToBeSameAsIncoTermPlaceCodeOnInvoice()
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader1 = declaration.Invoices.AddNew();
				invoiceHeader1.ZG_AgreedPlaceCode = "2";
				var invoiceHeader2 = declaration.Invoices.AddNew();
				invoiceHeader2.ZG_AgreedPlaceCode = "2";

				declaration.ZG_AgreedPlaceCode = "1";
				AssertHasMessageError(declaration.ZG_AgreedPlaceCodeInfo, "Incoterm place code values do not match between Declaration and Invoice Header.");

				declaration.ZG_AgreedPlaceCode = "2";
				AssertNoMessageError(declaration.ZG_AgreedPlaceCodeInfo, "Incoterm place code values do not match between Declaration and Invoice Header.");

				invoiceHeader2.ZG_AgreedPlaceCode = "3";
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertHasMessageError(declaration.ZG_AgreedPlaceCodeInfo, "Incoterm place code values do not match between Declaration and Invoice Header.");
			}
		}

		public void TestCheckZG_MethodOfPayment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method Of Payment");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "B", "DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				declaration.ZG_MethodOfPayment = "~";
				AssertHasMessageError("Invalid Code", declaration.ZG_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
				declaration.ZG_MethodOfPayment = "B";
				AssertNoMessageError("Valid Code", declaration.ZG_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckZG_BorderTransportMeans()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.ZG_BorderTransportMeansInfo, "!~", "02");
		}

		public void TestCheckZG_TypeOfSecurity_RuleC0211()
		{
			var message = "[C0211] If security is 2 Itinerary countries are required.";
			using (var context = new DeclarationValidationDeciderTestContext(declaration))
			{
				context.DisableRule(r => r.IsRuleC0211Active);
				declaration.ZG_TypeOfSecurity = ExportSecurityTypeList.Codes.EXS;
				AssertNoMessageError("Security is '2', itinerary countries is empty but Rule C0211 is disabled", declaration.ZG_TypeOfSecurityInfo, message);

				context.EnableRule(r => r.IsRuleC0211Active);
				declaration.ZG_TypeOfSecurity = ExportSecurityTypeList.Codes.NotUsed;
				AssertNoMessageError("Security is '0'", declaration.ZG_TypeOfSecurityInfo, message);

				declaration.ZG_TypeOfSecurity = ExportSecurityTypeList.Codes.EXS;
				AssertHasMessageError("Security is '2' and itinerary countries is empty", declaration.ZG_TypeOfSecurityInfo, message);

				var itineraryCountry = declaration.ItineraryCountries.AddNew();
				declaration.Validation.ValidateZG_TypeOfSecurity();
				AssertNoMessageError("Security is '2' and itinerary countries is filled", declaration.ZG_TypeOfSecurityInfo, message);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		void AssertArrivalTransportMeansMessage(ZPropertyInfo propertyName, ZString transportIDValue, ZInt transportTypeValue, ZBool hasMultipleEntryInstructions, ZString subStyle, ZString procedure, ZBool hasOrangeWarning, ZBool hasBlueError, bool isRuleC0623Enable)
		{
			using (var context = new DeclarationValidationDeciderTestContext(declaration))
			{
				if (isRuleC0623Enable)
				{
					context.EnableRule(r => r.IsRuleC0623Active);
				}
				else
				{
					context.DisableRule(r => r.IsRuleC0623Active);
				}

				var entryInstruction = declaration.CustomsEntryInstructions[0];

				if (declaration.CustomsEntryInstructions.Count == 0)
				{
					entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				}

				if (hasMultipleEntryInstructions && declaration.CustomsEntryInstructions.Count == 1)
				{
					declaration.CustomsEntryInstructions.AddNew();
				}

				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = procedure;

				if (propertyName.Name == "ZG_Box18TransportID")
				{
					declaration.ZG_Box18TransportID = transportIDValue;
				}
				else
				{
					declaration.ZG_Box18TransportType = transportTypeValue;
				}

				var message = "[C0623] Arrival Transport means must not be entered for this declaration type or requested procedure.";

				if (hasOrangeWarning)
				{
					AssertHasWarning(propertyName, message);
				}
				else
				{
					AssertNoWarning(propertyName, message);
				}

				if (hasBlueError)
				{
					AssertHasMessageError(propertyName, message);
				}
				else
				{
					AssertNoMessageError(propertyName, message);
				}
			}
		}

		JobDeclaration declaration;
	}
}
