using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSJobDeclarationValidationTest
		: EU.EMCS.Business.Testing.EMCSJobDeclarationValidationTest
	{
		public void TestShouldValidateDestinationTypeMustBe1WhenGuarantorIs0()
		{
			var validation = new EMCSJobDeclarationValidationForTest(declaration);
			AssertEquals(false, validation.ShouldValidateDestinationTypeMustBe1WhenGuarantorIs0);
		}

		public new void TestCheckJE_MessageSubType()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var traderExciseNumber = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "DE123", Core.Constants.CountryCodes.Greece);
			const string message = "This Destination Type is not valid if consignee has a German Trader Excise Number";
			var traderExciseDestinationTypes = new[] { EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee, EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee, EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationDirectDelivery };

			CombineAssertions(() =>
			{
				declaration.JE_MessageSubType = EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationDirectDelivery;
				AssertNoMessageError("No Importer Setup", declaration.JE_MessageSubTypeInfo, message);

				declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
				foreach (var destinationType in traderExciseDestinationTypes)
				{
					traderExciseNumber.OK_CustomsRegNo = "DE123";
					declaration.JE_MessageSubType = destinationType;
					AssertHasMessageError($"{destinationType} Trader Excise starts with DE", declaration.JE_MessageSubTypeInfo, message);
					traderExciseNumber.OK_CustomsRegNo = "IT123";
					declaration.Validation.ValidateJE_MessageSubType();
					AssertNoMessageError($"{destinationType} Trader Excise starts with IT", declaration.JE_MessageSubTypeInfo, message);
				}
			});
		}

		[TestDate(2020, 07, 01, 20, 20, 20)]
		public void TestCheckJE_DateAtOriginIfDeclarationIsConsolidatedDocument()
		{
			const string messageError = "The Dispatch Time must be the last day of the last month.";
			CombineAssertions(() =>
			{
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertNoMessageError("No ConsolidatedDocument", declaration.JE_DateAtOriginInfo, messageError);
				declaration.SetConsolidatedDocument();
				declaration.Validation.ValidateJE_DateAtOrigin();
				AssertHasMessageError("Is ConsolidatedDocument and empty DispatchTime", declaration.JE_DateAtOriginInfo, messageError);
				var testDate = ZDateTime.Now;
				testDate = testDate.AddDays(-1);
				declaration.JE_DateAtOrigin = testDate;
				AssertNoMessageError("Is ConsolidatedDocument and valid DispatchTime", declaration.JE_DateAtOriginInfo, messageError);
				testDate = testDate.AddDays(-1);
				declaration.JE_DateAtOrigin = testDate;
				AssertHasMessageError("Is ConsolidatedDocument and invalid DispatchTime", declaration.JE_DateAtOriginInfo, messageError);
			});
		}

		public void TestCheckJE_TransportModeIfDeclarationIsConsolidatedDocument()
		{
			const string warningMessage = "If different Transport Modes have been used choose code 'OTH'.";
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertNoWarning("No ConsolidatedDocument", declaration.JE_TransportModeInfo, warningMessage);
				declaration.SetConsolidatedDocument();
				AssertNoWarning("Is ConsolidatedDocument and TransportMode not empty", declaration.JE_TransportModeInfo, warningMessage);
				declaration.JE_TransportMode = ZString.Empty;
				AssertHasWarning("Is ConsolidatedDocument and empty TransportMode", declaration.JE_TransportModeInfo, warningMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
	class EMCSJobDeclarationValidationForTest : EMCSJobDeclarationValidation
	{
		public EMCSJobDeclarationValidationForTest(EMCSJobDeclaration parent) : base(parent)
		{
		}

		public new ZBool ShouldValidateDestinationTypeMustBe1WhenGuarantorIs0 => base.ShouldValidateDestinationTypeMustBe1WhenGuarantorIs0;
	}
}
