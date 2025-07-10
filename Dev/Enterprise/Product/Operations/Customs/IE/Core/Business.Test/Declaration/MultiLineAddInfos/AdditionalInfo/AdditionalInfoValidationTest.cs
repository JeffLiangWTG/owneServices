using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	public class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_SubType()
		{
			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = "^_^";
				AssertHasMessageErrorContaining("Invalid code is not allow", additionalInfo.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				additionalInfo.CSI_SubType = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_SubType is mandatory in IE", additionalInfo.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_SubType = "INF";
				AssertNoMessageErrorContaining("CSI_SubType is mandatory in IE", additionalInfo.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is not mandatory when readonly", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_ReferenceNumber = "TestRef";
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is mandatory in IE", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is mandatory in IE", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		[TestDate(2022, 10, 12, 15, 53, 00)]
		public void TestCheckCSI_ReferenceNumber_1D23()
		{
			var mandatoryAndValidErrorMessage = "1D23 Reference is mandatory and must be in the correct format: 'yyyyMMddHHmm'";
			var pastDateErrorMessage = "Scheduled time of departure cannot be in the past at time of registration.";

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			invLine.JI_CL = entryLine.PK;
			var addInfoInstruction = instruction.AdditionalInfos.AddNew();
			var addInfoInvoice = invHeader.AdditionalInfos.AddNew();

			CombineAssertions(() =>
			{
				addInfoInstruction.CSI_SubType = "REF";
				addInfoInstruction.CSI_Code = "1D23";
				addInfoInstruction.CSI_ReferenceNumber = "";
				AssertHasMessageError("Reference cannot be empty", addInfoInstruction.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);

				addInfoInstruction.CSI_ReferenceNumber = "2022-10-13 15:53";
				AssertHasMessageError("Reference must be format valid", addInfoInstruction.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);

				addInfoInstruction.CSI_Code = "N235";
				addInfoInstruction.CSI_ReferenceNumber = "2022-10-13 15:53";
				AssertNoMessageError("Not 1D23 code", addInfoInstruction.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);

				addInfoInstruction.CSI_Code = "1D23";
				addInfoInstruction.CSI_ReferenceNumber = "202210131553";
				AssertNoMessageError("Date in the future", addInfoInstruction.CSI_ReferenceNumberInfo, pastDateErrorMessage);

				addInfoInstruction.CSI_ReferenceNumber = "202210111553";
				AssertHasMessageError("Date must be in the future if MRN not set", addInfoInstruction.CSI_ReferenceNumberInfo, pastDateErrorMessage);

				entryHeader.MovementReferenceNumberSetter("22IEDU4EU157345117");
				addInfoInstruction.CSI_ReferenceNumber = "202210111553";
				AssertNoMessageError("MRN is set", addInfoInstruction.CSI_ReferenceNumberInfo, pastDateErrorMessage);

				declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
				entryHeader.MovementReferenceNumberSetter("");
				addInfoInvoice.CSI_SubType = "REF";
				addInfoInvoice.CSI_Code = "1D23";
				addInfoInvoice.CSI_ReferenceNumber = "202210111553";
				AssertHasMessageError("Date must be in the future if MRN not set", addInfoInvoice.CSI_ReferenceNumberInfo, pastDateErrorMessage);

				addInfoInvoice.CSI_SubType = "INF";
				addInfoInvoice.CSI_ReferenceNumber = "202210111553";
				AssertNoMessageError("CSI_SubType not relevant", addInfoInvoice.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);

				addInfoInvoice.CSI_SubType = "REF";
				entryHeader.MovementReferenceNumberSetter("22IEDU4EU157345117");
				addInfoInvoice.CSI_ReferenceNumber = "202210111553";
				AssertNoMessageError("MRN is set", addInfoInvoice.CSI_ReferenceNumberInfo, pastDateErrorMessage);
			});
		}

		public void TestCheckCSI_ReferenceNumber_BR2049()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			invLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				var addInfoInstruction = instruction.AdditionalInfos.AddNew();
				AssertAdditionalInfo_BR2049(addInfoInstruction);

				var addInfoInvoiceHeader = invHeader.AdditionalInfos.AddNew();
				AssertAdditionalInfo_BR2049(addInfoInvoiceHeader);

				var addInfoInvoiceLine = invLine.AdditionalInfos.AddNew();
				AssertAdditionalInfo_BR2049(addInfoInvoiceLine);
			});
		}

		void AssertAdditionalInfo_BR2049(AdditionalInfo additionalInfo)
		{
			const string mandatoryAndValidErrorMessage = "[BR2049] Additional Document Reference must be 'NAI' when Kind is 'TRA' and Full Type is '00100'.";

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalInfo.CSI_Code = Constants.AdditionalInformationCodes._00100;
			additionalInfo.CSI_ReferenceNumber = "ZZZ";
			AssertHasMessageError("CSI_SubType = TRA, CSI_Code = '00100', CSI_ReferenceNumber = ZZZ", additionalInfo.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalInfo.CSI_Code = Constants.AdditionalInformationCodes._00100;
			additionalInfo.CSI_ReferenceNumber = Constants.AdditionalReferenceNumberCodes.NAI;
			AssertNoMessageError("CSI_SubType = TRA, CSI_Code = '00100', CSI_ReferenceNumber = NAI", additionalInfo.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo.CSI_Code = Constants.AdditionalInformationCodes._00100;
			additionalInfo.CSI_ReferenceNumber = "ZZZ";
			AssertNoMessageError("CSI_SubType = REF, CSI_Code = '00100', CSI_ReferenceNumber = ZZZ", additionalInfo.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalInfo.CSI_Code = Constants.AdditionalReferenceCodes.RoRoShipID;
			additionalInfo.CSI_ReferenceNumber = "ZZZ";
			AssertNoMessageError("CSI_SubType = REF, CSI_Code = '1D94', CSI_ReferenceNumber = ZZZ", additionalInfo.CSI_ReferenceNumberInfo, mandatoryAndValidErrorMessage);
		}

		public void TestCheckCSI_ReferenceNumber_BR20330()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			invLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				var addInfoInstruction = instruction.AdditionalInfos.AddNew();
				AssertAdditionalInfo_BR20330(addInfoInstruction);

				var addInfoInvoiceHeader = invHeader.AdditionalInfos.AddNew();
				AssertAdditionalInfo_BR20330(addInfoInvoiceHeader);

				var addInfoInvoiceLine = invLine.AdditionalInfos.AddNew();
				AssertAdditionalInfo_BR20330(addInfoInvoiceLine);
			});
		}

		void AssertAdditionalInfo_BR20330(AdditionalInfo additionalInfo)
		{
			var errorMessage = "[BR20330] For Additional References, Reference must be 'NAI' when Full Type starts with '6'.";

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo.CSI_Code = "6VTA";
			additionalInfo.CSI_ReferenceNumber = "ZZZ";
			AssertHasMessageError("CSI_SubType = REF, CSI_Code = '6VTA', CSI_ReferenceNumber = ZZZ", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo.CSI_Code = "6VTA";
			additionalInfo.CSI_ReferenceNumber = Constants.AdditionalReferenceNumberCodes.NAI;
			AssertNoMessageError("CSI_SubType = REF, CSI_Code = '6VTA', CSI_ReferenceNumber = NAI", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalInfo.CSI_Code = "6VTA";
			additionalInfo.CSI_ReferenceNumber = "ZZZ";
			AssertNoMessageError("CSI_SubType = TRA, CSI_Code = '6VTA', CSI_ReferenceNumber = ZZZ", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo.CSI_Code = "5VTA";
			additionalInfo.CSI_ReferenceNumber = "ZZZ";
			AssertNoMessageError("CSI_SubType = REF, CSI_Code = '5VTA', CSI_ReferenceNumber = ZZZ", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);
		}

		public void TestCSI_Description()
		{
			var errorMessage = "You have not entered an Additional Document Description.";
			CombineAssertions(() =>
			{
				additionalInfo.CSI_Code = ZString.Empty;
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo.CSI_Description = ZString.Empty;
				AssertNoMessageErrorContaining("CSI_Description is not mandatory when readonly", additionalInfo.CSI_DescriptionInfo, errorMessage);

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Description = "TestDesc.";
				AssertNoMessageErrorContaining("CSI_Description is mandatory in IE", additionalInfo.CSI_DescriptionInfo, errorMessage);

				additionalInfo.CSI_Description = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_Description is mandatory in IE", additionalInfo.CSI_DescriptionInfo, errorMessage);

				additionalInfo.CSI_Description = "Some text";
				AssertNoMessageErrorContaining("CSI_Description is mandatory in IE", additionalInfo.CSI_DescriptionInfo, errorMessage);
			});
		}

		public void TestStatusIsNotEnabled()
		{
			CombineAssertions(() =>
			{
				additionalInfo.CSI_Status = ZString.Empty;
				AssertNoMessageErrors("CSI_Status is not enabled in IE, hence no message error, even though not entered.", additionalInfo.CSI_StatusInfo);

				additionalInfo.CSI_Status = "^_^";
				AssertNoMessageErrors("CSI_Status is not enabled in IE, hence no message error, even though not valid.", additionalInfo.CSI_StatusInfo);
			});
		}

		public void TestCSI_Code()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var instruction = declaration.CustomsEntryInstructions.FirstOrDefault();
				var additionalInfo1 = instruction.AdditionalInfos.AddNew();
				CombineAssertions(() =>
				{
					additionalInfo1.CSI_Description = ZString.Empty;
					additionalInfo1.CSI_Code = ZString.Empty;
					AssertHasMessageErrorContaining("CSI_Code is mandatory in IE", additionalInfo1.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

					additionalInfo1.CSI_Description = "Something";
					additionalInfo1.Validation.ValidateCSI_Code();
					AssertNoMessageErrorContaining("CSI_Code is mandatory in IE", additionalInfo1.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

					additionalInfo1.CSI_Code = "^_^";
					AssertHasMessageError("Should be valid code if entered.", additionalInfo1.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
				});
			}
		}

		public void TestCheckCSI_CodeBR2317()
		{
			var errorMessage = "[BR2317] If Requested Procedure is '71', and Transport Mode is 'SEA', declaring a '1D94' Additional Reference will result in rejection.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var instruction = declaration.CustomsEntryInstructions.FirstOrDefault();
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "71123456";

			CombineAssertions(() =>
			{
				var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoShipID;
				AssertHasMessageErrorContaining("IMP | SEA | REF + 1D94 | 71", additionalInfo1.CSI_CodeInfo, errorMessage);

				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber;
				AssertNoMessageErrorContaining("IMP | SEA | REF + 1D95 | 71", additionalInfo1.CSI_CodeInfo, errorMessage);

				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoShipID;
				AssertNoMessageErrorContaining("IMP | SEA | TRA + 1D94 | 71", additionalInfo1.CSI_CodeInfo, errorMessage);

				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo1.CSI_Code = AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				AssertNoMessageErrorContaining("IMP | SEA | REF + 1D23 | 71", additionalInfo1.CSI_CodeInfo, errorMessage);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoShipID;
				AssertNoMessageErrorContaining("IMP | AIR | REF + 1D94 | 71", additionalInfo1.CSI_CodeInfo, errorMessage);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber;
				AssertNoMessageErrorContaining("IMP | AIR | REF + 1D95 | 71", additionalInfo1.CSI_CodeInfo, errorMessage);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoShipID;
				AssertNoMessageErrorContaining("IMP | SEA | REF + 1D94 | 78", additionalInfo1.CSI_CodeInfo, errorMessage);

				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber;
				AssertNoMessageErrorContaining("IMP | SEA | REF + 1D95 | 78", additionalInfo1.CSI_CodeInfo, errorMessage);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._71;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				additionalInfo1.CSI_Code = AdditionalReferenceCodes.RoRoShipID;
				AssertNoMessageErrorContaining("EXP | SEA | REF + 1D94 | 71", additionalInfo1.CSI_CodeInfo, errorMessage);
			});
		}

		public void TestCSI_Code_BR1113()
		{
			SetUpAdditioanlInformationCodes();
			var errorMessage = "[BR1113] If Requested Procedure is not '76' nor '77', and Transport Mode is 'AIR', then 'N704', 'N705', or 'N714' Transport Document is not allowed.";
			var declaration = additionalInfo.Declaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var instruction = declaration.CustomsEntryInstructions.FirstOrDefault();
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions("IMP, AIR and TRA", () =>
			{
				invoiceLine.JI_Procedure = "70";
				var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "N704";
				AssertHasMessageErrorContaining("CSI_Code is N704", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N705";
				AssertHasMessageErrorContaining("CSI_Code is N705", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N714";
				AssertHasMessageErrorContaining("CSI_Code is N714", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N700";
				AssertNoMessageErrorContaining("CSI_Code is N700", additionalInfo.CSI_CodeInfo, errorMessage);
			});

			CombineAssertions("IMP, AIR and INF", () =>
			{
				invoiceLine.JI_Procedure = "70";
				var additionalInfo = instruction.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = "N704";
				AssertNoMessageErrorContaining("CSI_Code is N704", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N705";
				AssertNoMessageErrorContaining("CSI_Code is N705", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N714";
				AssertNoMessageErrorContaining("CSI_Code is N714", additionalInfo.CSI_CodeInfo, errorMessage);
			});

			CombineAssertions("IMP, AIR and TRA and procedure is 76", () =>
			{
				invoiceLine.JI_Procedure = "76";
				var additionalInfo = instruction.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "N704";
				AssertNoMessageErrorContaining("CSI_Code is N704", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N705";
				AssertNoMessageErrorContaining("CSI_Code is N705", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N714";
				AssertNoMessageErrorContaining("CSI_Code is N714", additionalInfo.CSI_CodeInfo, errorMessage);
			});

			CombineAssertions("IMP, AIR and TRA and procedure is 77", () =>
			{
				invoiceLine.JI_Procedure = "77";
				var additionalInfo = instruction.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "N704";
				AssertNoMessageErrorContaining("CSI_Code is N704", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N705";
				AssertNoMessageErrorContaining("CSI_Code is N705", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N714";
				AssertNoMessageErrorContaining("CSI_Code is N714", additionalInfo.CSI_CodeInfo, errorMessage);
			});

			CombineAssertions("EXP, AIR and TRA and procedure is 70", () =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				invoiceLine.JI_Procedure = "70";
				var additionalInfo = instruction.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "N704";
				AssertNoMessageErrorContaining("CSI_Code is N704", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N705";
				AssertNoMessageErrorContaining("CSI_Code is N705", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N714";
				AssertNoMessageErrorContaining("CSI_Code is N714", additionalInfo.CSI_CodeInfo, errorMessage);
			});

			CombineAssertions("IMP, SEA and TRA and procedure is 70", () =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				invoiceLine.JI_Procedure = "70";
				var additionalInfo = instruction.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "N704";
				AssertNoMessageErrorContaining("CSI_Code is N704", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N705";
				AssertNoMessageErrorContaining("CSI_Code is N705", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N714";
				AssertNoMessageErrorContaining("CSI_Code is N714", additionalInfo.CSI_CodeInfo, errorMessage);
			});

			CombineAssertions("IMP, AIR and TRA and procedure is empty", () =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				invoiceLine.JI_Procedure = ZString.Empty;
				var additionalInfo = instruction.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "N704";
				AssertNoMessageErrorContaining("CSI_Code is N704", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N705";
				AssertNoMessageErrorContaining("CSI_Code is N705", additionalInfo.CSI_CodeInfo, errorMessage);

				additionalInfo.CSI_Code = "N714";
				AssertNoMessageErrorContaining("CSI_Code is N714", additionalInfo.CSI_CodeInfo, errorMessage);
			});
		}

		void SetUpAdditioanlInformationCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "N700", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "N703", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "N704", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "N705", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "N714", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "N740", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "N741", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			additionalInfo = declaration.AdditionalInfos.AddNew();
		}
		AdditionalInfo additionalInfo;
	}
}
