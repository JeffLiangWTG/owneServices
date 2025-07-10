using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonMessageWrapperTest : WrapperHelperTest<AESCommonMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => new AESCommonMessageWrapper(null));

				var entryHeader = Factory.New<CusEntryHeader>();
				AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => new AESCommonMessageWrapper(entryHeader));
			});
		}

		public void TestSender()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Declarant.OA_OH = orgHeader.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Sender when declarant has no NIF/EORI/PAS declared", ZString.Empty, wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Sender with country code when no NIF or EORI declared", "GB333333333", wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Sender", "NIF22222222", wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations Sender", "ESNIF22222222", wrapper.Sender);

				var eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Sender with country code", "FR22222222", wrapper.Sender);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Sender with country code not repeated", "ES22222222", wrapper.Sender);
			});
		}

		public void TestSender_Representative()
		{
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			declaration.JE_OA_Representative = orgAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Sender when representative has no NIF/EORI/PAS declared", ZString.Empty, wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Sender with country code when no NIF or EORI declared", "GB333333333", wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Sender", "NIF22222222", wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations Sender", "ESNIF22222222", wrapper.Sender);

				var eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Sender with country code", "FR22222222", wrapper.Sender);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Sender with country code not repeated", "ES22222222", wrapper.Sender);
			});
		}

		public void TestSender_Exporter()
		{
			var orgHeaderE = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = orgHeaderE.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Sender when exporter has no NIF/EORI/PAS declared", ZString.Empty, wrapper.Sender);

				orgHeaderE.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Sender with country code when no NIF or EORI declared", "GB333333333", wrapper.Sender);

				orgHeaderE.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderE.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Sender", "NIF22222222", wrapper.Sender);

				orgHeaderE.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderE.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations Sender", "ESNIF22222222", wrapper.Sender);

				var eoriCusCode = orgHeaderE.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Sender with country code", "FR22222222", wrapper.Sender);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Sender with country code not repeated", "ES22222222", wrapper.Sender);
			});
		}

		public void TestSender_RepresentativeOrDeclarantOrExporter()
		{
			var orgAddressR = Factory.New<OrgAddress>();
			var orgHeaderR = Factory.NewWithValidTestData<OrgHeader>();
			orgAddressR.OA_OH = orgHeaderR.PK;
			declaration.JE_OA_Representative = orgAddressR.PK;

			var orgHeaderD = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Declarant.OA_OH = orgHeaderD.PK;

			var orgHeaderE = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = orgHeaderE.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Sender when representative, declarant and exporter have no NIF/EORI/PAS declared", ZString.Empty, wrapper.Sender);

				orgHeaderE.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "IT222222222", "IT");
				AssertEquals("Expected Declarant PAS in Sender when only id declared is for the exporter", "IT222222222", wrapper.Sender);

				orgHeaderD.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected Declarant PAS in Sender when only id declared is for the declarant", "GB333333333", wrapper.Sender);

				orgHeaderR.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "FR444444444", "FR");
				AssertEquals("Expected Representative PAS in Sender when an id is delcared in the representative (even if declarant has an id)", "FR444444444", wrapper.Sender);

				declaration.JE_OA_Representative = ZGuid.Empty;
				AssertEquals("Expected Declarant PAS in Sender when representative is not declared", "GB333333333", wrapper.Sender);
			});
		}

		public void TestMessageIdentification()
		{
			entryHeader.CH_BGMReference = "reference";
			AssertEquals("Expected filled MessageIdentification", "reference", wrapper.MessageIdentification);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new AESCommonMessageWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		AESCommonMessageWrapper wrapper;

		protected override AESCommonMessageWrapper GetProvider() => wrapper;
	}
}
