using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSAddInfoJobDeclarationValidationTest : EUEMCSAddInfoValidationTest
	{
		public void TestCheckZG_DeferredSubmission()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.ZG_DeferredSubmissionInfo, "X", EMCSDeferredSubmissionList.Codes.Yes);
		}

		public void TestCheckZG_GuarantorType_MandatoryAndList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, "Excise Movement Control System (EMCS) Guarantor Type");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, EMCSGuarantorTypeList.Codes.Consignee, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.ZG_GuarantorTypeInfo, "X", EMCSGuarantorTypeList.Codes.Consignee);
		}

		public void TestCheckZG_GuarantorType_UnknownDestinationConsigneeUnknown()
		{
			const string errorMessage = "Guarantor(s) should not contain a 4 when Destination Type is 8.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.JointGuaranteeOfTheConsignorAndOfTheConsignee;
				AssertHasMessageError("Validation requirements met", declaration.ZG_GuarantorTypeInfo, errorMessage);
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationDirectDelivery;
				declaration.AddInfoValidation.ValidateZG_GuarantorType();
				AssertNoMessageError("Message SubType not required", declaration.ZG_GuarantorTypeInfo, errorMessage);
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;
				AssertNoMessageError("Guarantor Type not required", declaration.ZG_GuarantorTypeInfo, errorMessage);
			});
		}

		public void TestCheckZG_TransportArrangement()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.ZG_TransportArrangementInfo, "X", EMCSTransportArrangementList.Codes.Consignee);
		}

		public void TestCheckZG_OriginType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.ZG_OriginTypeInfo, "X", EMCSOriginTypeList.Codes.Import);

			declaration.ZG_OriginType = EMCSOriginTypeList.Codes.Import;
			declaration.AddInfoValidation.ValidateZG_OriginType();
			AssertHasMessageError(declaration.ZG_OriginTypeInfo, "A Customs Office with Purpose 'DIS' is required.");

			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDispatch);
			declaration.AddInfoValidation.ValidateZG_OriginType();
			AssertNoMessageError(declaration.ZG_OriginTypeInfo, "A Customs Office with Purpose 'DIS' is required.");
		}

		public void TestCheckZG_OriginType_YouHaveNotEnteredAnImportSADEntryNumber()
		{
			const string youHaveNotEnteredAnImportSADEntryNumber = "You have not entered an Import SAD Entry Number.";
			CombineAssertions(() =>
			{
				declaration.ZG_OriginType = EMCSOriginTypeList.Codes.Import;
				declaration.AddInfoValidation.ValidateZG_OriginType();
				AssertHasMessageError("No Import SAD Number", declaration.ZG_OriginTypeInfo, youHaveNotEnteredAnImportSADEntryNumber);

				var sad = declaration.ImportSADNumbers.AddNew();
				sad.CSI_Description = "SAD";
				declaration.AddInfoValidation.ValidateZG_OriginType();
				AssertNoMessageError("An Import SAD Number exists", declaration.ZG_OriginTypeInfo, youHaveNotEnteredAnImportSADEntryNumber);
			});
		}

		public void TestCheckZG_DispatchReference()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.ZG_DispatchReferenceInfo);
		}

		public void TestCheckZG_SubmissionType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.ZG_SubmissionTypeInfo, "X", EMCSSubmissionTypeList.Codes.StandardSubmission);
		}

		public void TestCheckZG_CCTMSA()
		{
			var info = declaration.ZG_CCTMSAInfo;
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationDirectDelivery;
			declaration.AddInfoValidation.ValidateZG_CCTMSA();
			AssertNoMessageErrors(info);
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			declaration.AddInfoValidation.ValidateZG_CCTMSA();
			AssertHasMessageError(info, "Member State is required when Destination Type = 5 - Destination - Exempted consignee.");

			declaration.ZG_CCTMSA = Core.Constants.CountryCodes.Monaco;
			AssertHasMessageError(info, "The code you have selected is not in the list.");
			declaration.ZG_CCTMSA = Core.Constants.CountryCodes.IsleOfMan;
			AssertHasMessageError(info, "The code you have selected is not in the list.");
			declaration.ZG_CCTMSA = Core.Constants.CountryCodes.Austria;
			AssertNoMessageErrors(info);
		}

		public void TestCheckOriginTypeCodeWhenSubmissionTypeIsAssigned()
		{
			var message = $"Origin Type must be either 1 or 2 when Submission Type is not 3.";
			declaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.StandardSubmission;
			declaration.ZG_OriginType = EMCSOriginTypeList.Codes.DutyPaid;
			declaration.AddInfoValidation.ValidateZG_OriginType();
			AssertHasMessageError(declaration.ZG_OriginTypeInfo, message);

			declaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.SubmissionForExport;
			declaration.ZG_OriginType = EMCSOriginTypeList.Codes.DutyPaid;
			declaration.AddInfoValidation.ValidateZG_OriginType();
			AssertHasMessageError(declaration.ZG_OriginTypeInfo, message);

			message = $"Origin Type must be 3 when Submission Type is 3.";
			declaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B;
			declaration.ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;
			declaration.AddInfoValidation.ValidateZG_OriginType();
			AssertHasMessageError(declaration.ZG_OriginTypeInfo, message);

			declaration.ZG_OriginType = EMCSOriginTypeList.Codes.Import;
			declaration.AddInfoValidation.ValidateZG_OriginType();
			AssertHasMessageError(declaration.ZG_OriginTypeInfo, message);
		}

		protected AddInfo GetNewAddInfo() => new EMCSAddInfoJobDeclaration(Factory.New<EMCSJobDeclaration>());

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
