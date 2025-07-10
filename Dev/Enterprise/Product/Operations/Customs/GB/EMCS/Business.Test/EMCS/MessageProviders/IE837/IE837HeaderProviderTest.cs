using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE837HeaderProvider))]
	sealed class IE837HeaderProviderTest : HeaderProviderAbstractTest<IE837HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE837HeaderProvider(emcsDeclaration, null));
		}

		public void TestSequenceNumber()
		{
			var cusEntryNumber = CusEntryNumber.New(emcsDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			cusEntryNumber.CE_EntryLineReference = "99";
			AssertEquals(99, HeaderProvider.SequenceNumber);
		}

		public void TestSequenceNumber_Default()
		{
			AssertEquals(1, HeaderProvider.SequenceNumber);
		}

		public void TestSequenceNumber_Zero()
		{
			var cusEntryNumber = CusEntryNumber.New(emcsDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			cusEntryNumber.CE_EntryLineReference = "0";
			AssertEquals(1, HeaderProvider.SequenceNumber);
		}

		public void TestExplanationCode()
		{
			explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Accident;
			AssertEquals(EMCSExplanationOnDelayCodeList.Codes.Accident, HeaderProvider.ExplanationCode);
		}

		public void TestMessageRole()
		{
			explanationOnDelay.MessageRole = EMCSExplanationOnDelayMessageRoleCodeList.Codes._2;
			AssertEquals(EMCSExplanationOnDelayMessageRoleCodeList.Codes._2, HeaderProvider.MessageRole);
		}

		public void TestInformation()
		{
			explanationOnDelay.Information = "Information";
			AssertEquals("Information", ((IE837HeaderProvider)HeaderProvider).Information);
		}

		public void TestSubmitterType()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = ZString.Empty;
				AssertEquals(string.Empty, HeaderProvider.SubmitterType);
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				AssertEquals(EMCSEntryTypeList.Codes.Consignor, HeaderProvider.SubmitterType);
			});
		}

		public void TestSubmitterIdentification_Consignor()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			emcsDeclaration.JE_OH_Supplier = GetPartyTraderExciseNumberOrg("TEN105").PK;
			AssertEquals("Consignor: Trader Excise Number", "TEN105", HeaderProvider.SubmitterIdentification);
		}

		public void TestSubmitterIdentification_Consignee()
		{
			emcsDeclaration.JE_OH_Importer = GetPartyTraderExciseNumberOrg("TEN105").PK;
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			AssertEquals("Consignee: Trader Excise Number", "TEN105", HeaderProvider.SubmitterIdentification);
		}

		public void TestComplementaryInformation_Text()
		{
			explanationOnDelay.Information = "TEST TEXT FOR REASON";
			AssertEquals("Complementary Information Text", "TEST TEXT FOR REASON", HeaderProvider.ComplementaryInformation.Text);
			AssertEquals("Complementary Information Default Language", "en", HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestComplementaryInformation_Consignor()
		{
			explanationOnDelay.Information = "TEST TEXT FOR REASON";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Language = "CN";
			emcsDeclaration.JE_OH_Supplier = supplier.PK;
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("Complementary Information Text", "TEST TEXT FOR REASON", HeaderProvider.ComplementaryInformation.Text);
			AssertEquals("Complementary Information Text Language", "cn", HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestComplementaryInformation_ConsignorWithDefaultLanguage()
		{
			explanationOnDelay.Information = "TEST TEXT FOR REASON";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Language = "";
			emcsDeclaration.JE_OH_Supplier = supplier.PK;
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("Complementary Information Text", "TEST TEXT FOR REASON", HeaderProvider.ComplementaryInformation.Text);
			AssertEquals("Complementary Information Default Language", "en", HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestComplementaryInformation_Consignee()
		{
			explanationOnDelay.Information = "TEST TEXT FOR REASON";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Language = "ES";
			emcsDeclaration.JE_OH_Importer = importer.PK;
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			AssertEquals("Complementary Information Text", "TEST TEXT FOR REASON", HeaderProvider.ComplementaryInformation.Text);
			AssertEquals("Complementary Information Text Language", "es", HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestComplementaryInformation_ConsigneeWithDefaultLanguage()
		{
			explanationOnDelay.Information = "TEST TEXT FOR REASON";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Language = "";
			emcsDeclaration.JE_OH_Importer = importer.PK;
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			AssertEquals("Complementary Information Text", "TEST TEXT FOR REASON", HeaderProvider.ComplementaryInformation.Text);
			AssertEquals("Complementary Information Default Language", "en", HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestComplementaryInformationSpecified()
		{
			CombineAssertions(() =>
			{
				explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Other;
				AssertEquals("Complementary Information is mandatory", true, HeaderProvider.ComplementaryInformationSpecified);

				explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Accident;
				explanationOnDelay.Information = "";
				AssertEquals("Complementary Information is not mandatory (no information provided)", false, HeaderProvider.ComplementaryInformationSpecified);

				explanationOnDelay.Information = "Information to send";
				AssertEquals("Complementary Information should be sent as we have information text provided", true, HeaderProvider.ComplementaryInformationSpecified);
			});
		}

		public void TestDateAndTimeOfValidationOfExplanationOnDelay()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.EMCSGB_ValidationAttributeAllowed, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
			{
				AssertNull(HeaderProvider.DateAndTimeOfValidationOfExplanationOnDelay);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.EMCSGB_ValidationAttributeAllowed, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertNotNull(HeaderProvider.DateAndTimeOfValidationOfExplanationOnDelay);
				CombineAssertions(() =>
				{
					AssertEquals(0, HeaderProvider.DateAndTimeOfValidationOfExplanationOnDelay.Value.Millisecond);
					AssertEquals(DateTimeKind.Unspecified, HeaderProvider.DateAndTimeOfValidationOfExplanationOnDelay.Value.Kind);
				});
			}
		}

		public void TestIsDeclarantTypeConsignor()
		{
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("Is Consignor: ", true, HeaderProvider.SubmitterType == EMCSEntryTypeList.Codes.Consignor);

			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			AssertEquals("Is Consignor: ", false, HeaderProvider.SubmitterType == EMCSEntryTypeList.Codes.Consignor);
		}

		protected override IE837HeaderProvider GetHeaderProvider() => new IE837HeaderProvider(emcsDeclaration, explanationOnDelay);

		new IIE837Header HeaderProvider => base.HeaderProvider;

		protected override void SetUp()
		{
			base.SetUp();
			explanationOnDelay = new ExplanationOnDelaySendingAction(emcsDeclaration);
			explanationOnDelay.Information = "TEST TEXT";
		}
		ExplanationOnDelaySendingAction explanationOnDelay;

		protected override IEnumerable<Expression<Func<IE837HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
		}
	}
}
