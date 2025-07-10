using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Registry;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIEDeclarationValidationHelperTest : TestCaseWithFactory
	{
		public void TestHasFallbackProcedureInformationAdditionalInfos()
		{
			var correctSubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var correctCode = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FallbackProcedure;
			var wrongSubType = "Type";
			var wrongCode = "Code";

			var declaration1 = Factory.New<JobDeclaration>();
			var additionalInfo1 = declaration1.AdditionalInfos.AddNew();
			additionalInfo1.CSI_SubType = correctSubType;
			additionalInfo1.CSI_Code = correctCode;
			bool result1 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration1.AdditionalInfos);
			AssertEquals("HasFallbackProcedureInformationAdditionalInfos should be true when both CSI subtype and CSI code match fallback procedure criteria.", true, result1);

			var declaration2 = Factory.New<JobDeclaration>();
			var additionalInfo2 = declaration2.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = correctSubType;
			additionalInfo2.CSI_Code = wrongCode;
			bool result2 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration2.AdditionalInfos);
			AssertEquals("HasFallbackProcedureInformationAdditionalInfos should be false when CSI subtype indicates additional info but CSI code does not match fallback procedure criteria.", false, result2);

			var declaration3 = Factory.New<JobDeclaration>();
			var additionalInfo3 = declaration3.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = wrongSubType;
			additionalInfo3.CSI_Code = correctCode;
			bool result3 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration3.AdditionalInfos);
			AssertEquals("HasFallbackProcedureInformationAdditionalInfos should be false when CSI code matches fallback procedure criteria but CSI subtype does not indicate additional info.", false, result3);

			var declaration4 = Factory.New<JobDeclaration>();
			var additionalInfo4 = declaration4.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = wrongSubType;
			additionalInfo4.CSI_Code = wrongCode;
			bool result4 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureInformationAdditionalInfos(declaration4.AdditionalInfos);
			AssertEquals("HasFallbackProcedureInformationAdditionalInfos should be false when neither CSI subtype nor CSI code match fallback procedure criteria.", false, result4);
		}

		public void TestHasFallbackProcedureReferenceAdditionalInfos_AllCases()
		{
			var fallbackSettings = new FallbackSettings();
			fallbackSettings.InvocationReason = "TestInvocationReason";
			FRCustomsDataRegistry.Instance.DeltaIMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSettings);

			var correctSubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var wrongSubType = "Type";
			var correctCode = UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.FallbackProcedure;
			var wrongCode = "Code";
			var correctReference = "TestInvocationReason";
			var wrongReference = "WrongInvocationReason";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo1 = declaration1.AdditionalInfos.AddNew();
			additionalInfo1.CSI_SubType = correctSubType;
			additionalInfo1.CSI_Code = correctCode;
			additionalInfo1.CSI_ReferenceNumber = correctReference;
			bool result1 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration1.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be true when CSI_SubType, CSI_Code, and CSI_ReferenceNumber all match the fallback procedure reference criteria.", true, result1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo2 = declaration2.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = correctSubType;
			additionalInfo2.CSI_Code = correctCode;
			additionalInfo2.CSI_ReferenceNumber = wrongReference;
			bool result2 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration2.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be false when CSI_SubType and CSI_Code are correct but CSI_ReferenceNumber does not match the fallback settings.", false, result2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo3 = declaration3.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = correctSubType;
			additionalInfo3.CSI_Code = wrongCode;
			additionalInfo3.CSI_ReferenceNumber = correctReference;
			bool result3 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration3.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be false when CSI_SubType and CSI_ReferenceNumber are correct but CSI_Code does not match the fallback procedure reference number.", false, result3);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo4 = declaration4.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = correctSubType;
			additionalInfo4.CSI_Code = wrongCode;
			additionalInfo4.CSI_ReferenceNumber = wrongReference;
			bool result4 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration4.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be false when CSI_SubType is correct but both CSI_Code and CSI_ReferenceNumber are incorrect.", false, result4);

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo5 = declaration5.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = wrongSubType;
			additionalInfo5.CSI_Code = correctCode;
			additionalInfo5.CSI_ReferenceNumber = correctReference;
			bool result5 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration5.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be false when CSI_Code and CSI_ReferenceNumber match but CSI_SubType is incorrect.", false, result5);

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo6 = declaration6.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = wrongSubType;
			additionalInfo6.CSI_Code = correctCode;
			additionalInfo6.CSI_ReferenceNumber = wrongReference;
			bool result6 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration6.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be false when only CSI_Code is correct while both CSI_SubType and CSI_ReferenceNumber are incorrect.", false, result6);

			var declaration7 = Factory.New<JobDeclaration>();
			declaration7.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo7 = declaration7.AdditionalInfos.AddNew();
			additionalInfo7.CSI_SubType = wrongSubType;
			additionalInfo7.CSI_Code = wrongCode;
			additionalInfo7.CSI_ReferenceNumber = correctReference;
			bool result7 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration7.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be false when only CSI_ReferenceNumber is correct while CSI_SubType and CSI_Code are incorrect.", false, result7);

			var declaration8 = Factory.New<JobDeclaration>();
			declaration8.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionalInfo8 = declaration8.AdditionalInfos.AddNew();
			additionalInfo8.CSI_SubType = wrongSubType;
			additionalInfo8.CSI_Code = wrongCode;
			additionalInfo8.CSI_ReferenceNumber = wrongReference;
			bool result8 = DeltaIEDeclarationValidationHelper.HasFallbackProcedureReferenceAdditionalInfos(declaration8.AdditionalInfos);
			AssertEquals("HasFallbackProcedureReferenceAdditionalInfos should be false when none of the conditions match (CSI_SubType, CSI_Code, and CSI_ReferenceNumber are all incorrect).", false, result8);
		}

		public void TestGetPortCodeAdditionalReference()
		{
			var testCase = new MultiFactorTestCase<JobDeclaration>(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.AdditionalInfos.AddNew();

				return declaration;
			});

			var subTypeIsREF = new FieldPreq<JobDeclaration>(declaration => declaration.AdditionalInfos[0].CSI_SubTypeInfo)
				.Values(EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference)
				.NotValues(EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation);

			var codeIs1CPT = new FieldPreq<JobDeclaration>(declaration => declaration.AdditionalInfos[0].CSI_CodeInfo)
				.Values(UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.PortCode)
				.NotValues(UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.FallbackProcedure);

			testCase.SetUpCondition(subTypeIsREF && codeIs1CPT);
			testCase.RunAssertion(declaration => AssertNotNull("GetPortCodeAdditionalReference should get the AdditionalInfo with REF subType and 1CPT code.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos)),
				declaration => AssertNull("Should return null if subType or code do not match the expected values.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos)));
		}
	}
}
