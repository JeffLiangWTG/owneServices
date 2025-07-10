using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED837HeaderProvider))]
	class ED837HeaderProviderTest : HeaderProviderAbstractTest<ED837HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED837HeaderProvider(emcsDeclaration, null));
		}

		public void TestSequenceNumber()
		{
			var cusEntryNumber = CusEntryNumber.New(emcsDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryLineReference = "1";

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
			AssertEquals("TEST TEXT FOR REASON", HeaderProvider.ComplementaryInformation.Text);
		}

		public void TestComplementaryInformation_LanguageDefault()
		{
			AssertEquals("Default Language", GlbBranch.CurrentBranch.Language.ToLower(), HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestComplementaryInformation_LanguageConsignor()
		{
			explanationOnDelay.Information = "TEST TEXT";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Language = "CN";
			emcsDeclaration.JE_OH_Supplier = supplier.PK;
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("Consignor Language", "cn", HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestComplementaryInformation_LanguageConsignee()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Language = "ES";
			explanationOnDelay.Information = "TEST TEXT FOR REASON";
			emcsDeclaration.JE_OH_Importer = importer.PK;
			emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			AssertEquals("Consignee Language", "es", HeaderProvider.ComplementaryInformation.Language);
		}

		public void TestIsDeclarantTypeConsignor()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				AssertEquals("SubmitterType is '1'", true, HeaderProvider.IsDeclarantTypeConsignor);
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				AssertEquals("SubmitterType isn't '1'", false, HeaderProvider.IsDeclarantTypeConsignor);
			});
		}

		public void TestComplementaryInformationSpecified()
		{
			CombineAssertions(() =>
			{
				explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Other;
				AssertEquals("ExplanationCode is '0'", true, HeaderProvider.ComplementaryInformationSpecified);
				explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Accident;
				AssertEquals("ExplanationCode isn't '0'", false, HeaderProvider.ComplementaryInformationSpecified);
			});
		}

		protected override ED837HeaderProvider GetHeaderProvider() => new ED837HeaderProvider(emcsDeclaration, explanationOnDelay);

		protected new IED837Header HeaderProvider => base.HeaderProvider;

		protected override void SetUp()
		{
			base.SetUp();
			explanationOnDelay = new ExplanationOnDelaySendingAction(emcsDeclaration);
			explanationOnDelay.Information = "TEST TEXT";
		}
		ExplanationOnDelaySendingAction explanationOnDelay;

		protected override IEnumerable<Expression<Func<ED837HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
		}
	}
}
