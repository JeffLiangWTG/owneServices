using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_OwnerRefWarnIfEmpty()
		{
			ValidationTestHelper.AssertWarningIfNotEntered(jobDeclaration.JE_OwnerRefInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			jobDeclaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageErrorContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_RL_NKFinalDestination = "XXX";
			AssertNoMessageErrorContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			jobDeclaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertNoMessageErrorContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_IATALoadPort()
		{
			jobDeclaration.JE_TransportMode = jobDeclaration.TransportModeAirCodeForTesting;
			jobDeclaration.JE_IATALoadPort = "XXX";
			AssertNoMessageErrorContaining(jobDeclaration.JE_IATALoadPortInfo, ListValidation.InvalidCodeMessageError);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var declaration1 = Factory.New<EU.Business.Declaration.JobDeclaration>();
				Factory.Save();

				declaration1.JE_TransportMode = declaration1.TransportModeAirCodeForTesting;
				declaration1.JE_IATALoadPort = "XXX";
				AssertHasMessageErrorContaining(declaration1.JE_IATALoadPortInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public override void TestJE_ShipmentIncoTerm()
		{
			Assert("Precondition: No error", !jobDeclaration.JE_ShipmentIncoTermInfo.HasErrors());
			jobDeclaration.JE_ShipmentIncoTerm = "XYZ";
			AssertHasMessageErrors(jobDeclaration.JE_ShipmentIncoTermInfo);
			jobDeclaration.JE_ShipmentIncoTerm = jobDeclaration.Lookups.IncoTermList[0].Code;
			Assert("No error after selecting valid incoterm", !jobDeclaration.JE_ShipmentIncoTermInfo.HasErrors());

			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtFrontier;
			AssertHasMessageError(jobDeclaration.JE_ShipmentIncoTermInfo, ListValidation.InvalidCodeMessageError);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredExShip;
			AssertHasMessageError(jobDeclaration.JE_ShipmentIncoTermInfo, ListValidation.InvalidCodeMessageError);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredExQuay;
			AssertHasMessageError(jobDeclaration.JE_ShipmentIncoTermInfo, ListValidation.InvalidCodeMessageError);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredDutyUnpaid;
			AssertHasMessageError(jobDeclaration.JE_ShipmentIncoTermInfo, ListValidation.InvalidCodeMessageError);
			jobDeclaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertNoMessageErrors(jobDeclaration.JE_ShipmentIncoTermInfo);
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("C0010", "C0010");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "C0010", "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "GB", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			jobDeclaration.JE_RL_NKOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(jobDeclaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_RL_NKOrigin = "123";
			AssertNoMessageErrorContaining(jobDeclaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Desc.");
			var expCusOff = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ExpCusOff", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var otherCusOff = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "othCusOff", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeListAttribute(expCusOff.PK, RefCusCodeListAttributeTypes.Codes.ROLE, MessageTypeList.Codes.Export);

			Factory.Save();

			var info = jobDeclaration.JE_CustomsOfficeInfo;
			var msg_invalid = ListValidation.InvalidCodeMessageError;

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.JE_CustomsOffice = "invld_Code";
			AssertHasMessageError(info, msg_invalid);

			jobDeclaration.JE_CustomsOffice = otherCusOff.ZZD_Code;
			AssertNoMessageError(info, msg_invalid);

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageError(info, msg_invalid);

			jobDeclaration.JE_CustomsOffice = otherCusOff.ZZD_Code;
			AssertHasMessageError(info, msg_invalid);

			jobDeclaration.JE_CustomsOffice = expCusOff.ZZD_Code;
			AssertNoMessageError(info, msg_invalid);
		}

		public void TestValidateJE_PaymentMethod()
		{
			CombineAssertions(() =>
			{
				foreach (var paymentMethod in new[]
				{
					DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14,
					DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority,
					DefermentMethodList.Codes.ConsigneesAccountStandingAuthority,
					DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration
				})
				{
					jobDeclaration.JE_PaymentMethod = paymentMethod;
					AssertEquals($"Payment method {paymentMethod}", false, jobDeclaration.JE_PaymentMethodInfo.Notifications.Any(x => x.Message.Contains("requires a Deferment Approval Number (DAN) for the relevant country/region")));
				}
			});
		}

		public void TestValidatePowerOfAttorney()
		{
			var powerOfAttorneyWarningMessage = AuthorityToActValidator.GetNoPOADocumentForImporterString(new AuthorityToActValidator().CountrySpecificNameForPOA);
			CombineAssertions(() =>
			{
				jobDeclaration.JE_OH_Importer = GlbBranch.CurrentBranch.OrgProxy.PK;
				jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("No warning for Import", false, jobDeclaration.JE_DeclarantTypeInfo.HasWarning(powerOfAttorneyWarningMessage));

				jobDeclaration.JE_OH_Supplier = GlbBranch.CurrentBranch.OrgProxy.PK;
				jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("No warning Export", false, jobDeclaration.JE_DeclarantTypeInfo.HasWarning(powerOfAttorneyWarningMessage));
			});
		}

		public void TestCheckJE_OA_RepresentativeIsNotEmpty()
		{
			var message = "Please enter a Representative for Rep. Type 'DIR'.";
			CombineAssertions(() =>
			{
				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_OA_RepresentativeInfo, message, "RepType 'DIR");

				jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				ValidationTestHelper.AssertFieldIsNotMandatory(jobDeclaration.JE_OA_RepresentativeInfo, message, "RepType 'SEL'");
			});
		}

		public void TestCheckJE_DeclarantType()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = jobDeclaration.PK;
			jobDeclaration.Validation.ValidateJE_DeclarantType();
			AssertNoNotifications(jobDeclaration.JE_DeclarantTypeInfo);
		}

		public void TestCheckJE_TransportMode()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = jobDeclaration.PK;
			jobDeclaration.Validation.ValidateJE_TransportMode();
			AssertNoNotifications(jobDeclaration.JE_TransportModeInfo);
		}

		public void TestCheckJE_ContainerMode()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			jobDeclaration.Validation.ValidateJE_ContainerMode();
			AssertNoNotifications(jobDeclaration.JE_ContainerModeInfo);
		}

		public void TestCheckJE_UCR()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			jobDeclaration.Validation.ValidateJE_UCR();
			AssertNoNotifications(jobDeclaration.JE_UCRInfo);
		}

		public void TestCheckJE_RL_NKOrigin_AVABR()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = jobDeclaration.PK;
			jobDeclaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoNotifications(jobDeclaration.JE_RL_NKOriginInfo);
		}
		public void TestCheckJE_CustomsOffice_AVABR()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = jobDeclaration.PK;
			jobDeclaration.Validation.ValidateJE_CustomsOffice();
			AssertNoNotifications(jobDeclaration.JE_CustomsOfficeInfo);
		}

		public void TestCheckJE_RL_NKFinalDestination_AVABR()
		{
			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = jobDeclaration.PK;
			jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertNoNotifications(jobDeclaration.JE_RL_NKFinalDestinationInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
		}
		JobDeclaration jobDeclaration;
	}
}
