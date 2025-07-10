using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(NEXDOCSQuarantineExDocEstablishmentAndTimeValidation))]
	sealed class NEXDOCSQuarantineExDocEstablishmentAndTimeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEE_EstablishmentIndicator()
		{
			var newAddress = Factory.New<JobDocAddress>();
			newAddress.E2_AddressOverride = true;
			newAddress.E2_CompanyName = "Company";

			var expectedMessage = "Establishment Indicator is required when either Processing Establishment or Establishment ID is entered.";

			process.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-Condition", !process.EE_EstablishmentIndicatorInfo.HasMessageErrors());
			process.EE_E2_Address = ZGuid.Empty;
			process.EE_AuthorisationEstablishmentID = ZString.Empty;
			process.EE_EstablishmentIndicator = ZString.Empty;
			AssertNoMessageError(process.EE_EstablishmentIndicatorInfo, expectedMessage);

			process.EE_E2_Address = ZGuid.Empty;
			process.EE_AuthorisationEstablishmentID = "14";
			process.Validation.ValidateEE_EstablishmentIndicator();
			AssertHasMessageError(process.EE_EstablishmentIndicatorInfo, expectedMessage);
			process.EE_ProcessingType = "HA";
			process.Validation.ValidateEE_EstablishmentIndicator();
			AssertNoMessageError(process.EE_EstablishmentIndicatorInfo, expectedMessage);

			process.EE_ProcessingType = "PC";
			process.EE_E2_Address = newAddress.PK;
			process.EE_AuthorisationEstablishmentID = "";
			process.Validation.ValidateEE_EstablishmentIndicator();
			AssertHasMessageError(process.EE_EstablishmentIndicatorInfo, expectedMessage);
			process.EE_ProcessingType = "HA";
			process.Validation.ValidateEE_EstablishmentIndicator();
			AssertNoMessageError(process.EE_EstablishmentIndicatorInfo, expectedMessage);

			process.EE_ProcessingType = "PC";
			process.EE_E2_Address = newAddress.PK;
			process.EE_AuthorisationEstablishmentID = "14";
			process.EE_EstablishmentIndicator = "IN";
			process.Validation.ValidateEE_EstablishmentIndicator();
			AssertNoMessageError(process.EE_EstablishmentIndicatorInfo, expectedMessage);
		}

		public void TestCheckEE_E2_Address_ProcessingTypes_AQ_CB()
		{
			var propertyInfoToValidate = process.EE_E2_AddressInfo;
			var validateProperty = () => process.Validation.ValidateEE_E2_Address();
			AssertEstablishmentAddressOrIdMustBeEntered(EXDOCProcessTypeCodes.Codes.AquacultureFarm, propertyInfoToValidate, validateProperty);
			AssertEstablishmentAddressOrIdMustBeEntered(EXDOCProcessTypeCodes.Codes.CatcherBoat, propertyInfoToValidate, validateProperty);
		}

		public void TestCheckEE_E2_Address_ProcessingTypes_OtherProcessingTypes()
		{
			process.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.Validation.ValidateEE_E2_Address();
			AssertNoMessageErrors(process.EE_E2_AddressInfo);

			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process.Validation.ValidateEE_E2_Address();
			AssertNoMessageErrors(process.EE_E2_AddressInfo);

			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process.Validation.ValidateEE_E2_Address();
			AssertNoMessageErrors(process.EE_E2_AddressInfo);
		}

		public void TestCheckEE_AuthorisationEstablishmentID_ProcessingTypes_HA_TR()
		{
			process.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			process.Validation.ValidateEE_AuthorisationEstablishmentID();
			AssertNoMessageErrors(process.EE_AuthorisationEstablishmentIDInfo);

			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			process.Validation.ValidateEE_AuthorisationEstablishmentID();
			AssertNoMessageErrors(process.EE_AuthorisationEstablishmentIDInfo);
		}

		public void TestCheckEE_AuthorisationEstablishmentID_ProcessingTypes_AQ_CB()
		{
			var propertyInfoToValidate = process.EE_AuthorisationEstablishmentIDInfo;
			var validateProperty = () => process.Validation.ValidateEE_AuthorisationEstablishmentID();
			AssertEstablishmentAddressOrIdMustBeEntered(EXDOCProcessTypeCodes.Codes.AquacultureFarm, propertyInfoToValidate, validateProperty);
			AssertEstablishmentAddressOrIdMustBeEntered(EXDOCProcessTypeCodes.Codes.CatcherBoat, propertyInfoToValidate, validateProperty);
		}

		void AssertEstablishmentAddressOrIdMustBeEntered(ZString processingType, ZPropertyInfo propertyInfo, Action validateProperty)
		{
			const string message = "Process establishment address or ID must be entered.";
			process.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			process.EE_ProcessingType = processingType;
			process.EE_E2_Address = ZGuid.Empty;
			process.EE_AuthorisationEstablishmentID = ZString.Empty;
			validateProperty();
			AssertHasMessageError("Both EE_E2_Address and EE_AuthorisationEstablishmentID are empty", propertyInfo, message);

			var address = Factory.New<JobDocAddress>();
			address.E2_CompanyName = "Company";
			process.EE_E2_Address = address.PK;
			validateProperty();
			AssertNoMessageError("EE_E2_Address is not empty", propertyInfo, message);

			process.EE_E2_Address = ZGuid.Empty;
			process.EE_AuthorisationEstablishmentID = "14";
			validateProperty();
			AssertNoMessageError("EE_AuthorisationEstablishmentID is not empty", propertyInfo, message);
		}

		public void TestCheckEE_AuthorisationEstablishmentID_OtherProcessingTypes()
		{
			const string message = "Process establishment ID must be entered.";
			process.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Freezing;
			process.Validation.ValidateEE_AuthorisationEstablishmentID();
			AssertHasMessageError(process.EE_AuthorisationEstablishmentIDInfo, message);

			process.EE_AuthorisationEstablishmentID = "14";
			AssertNoMessageError(process.EE_AuthorisationEstablishmentIDInfo, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			eXDOCLine = invoiceLine.QuarantineExDocLine;
			process = eXDOCLine.Processes.AddNew();
		}

		QuarantineExDocEstablishmentAndTime process;
		QuarantineExDocLine eXDOCLine;
	}
}
