using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DuimpTaxRegime))]
	class DuimpTaxRegimeTest : Customs.Business.Testing.CusSupportingInfoTest<DuimpTaxRegime>
	{
		public void TestProperties_FromZZData()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);
			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;

				var profileP01 = invoiceLine.GetRequiredTTProfiles().First(s => s.LegalCode == "P01");
				var profileP02COF1 = invoiceLine.GetRequiredTTProfiles().First(s => s.LegalCode == "P02" && s.TaxType == "COF");
				var profileP02COF2 = invoiceLine.GetRequiredTTProfiles().Last(s => s.LegalCode == "P02" && s.TaxType == "COF");
				var profileP02PIS = invoiceLine.GetRequiredTTProfiles().First(s => s.LegalCode == "P02" && s.TaxType == "PIS");

				var taxRegime1 = invoiceLine.DuimpTaxRegimes.AddNew(profileP01);
				var taxRegime2 = invoiceLine.DuimpTaxRegimes.AddNew(profileP02COF1);
				invoiceLine.DuimpTaxRegimes.AddNew(profileP02COF2);
				var taxRegime3 = invoiceLine.DuimpTaxRegimes.AddNew(profileP02PIS);
				CombineAssertions(() =>
				{
					AssertEquals("TaxRegime1 Parent", invoiceLine, taxRegime1.Parent);
					AssertEquals("TaxRegime1 CSI_Procedure", "P01", taxRegime1.CSI_Procedure);
					AssertEquals("TaxRegime1 ProcedureDescription", "Awaiting payment", taxRegime1.ProcedureDescription);
					AssertEquals("TaxRegime1 MandatoryDescription", "Normal", taxRegime1.MandatoryDescription);
					AssertEquals("TaxRegime1 CSI_SubType", "DTY", taxRegime1.CSI_SubType);
					AssertEquals("TaxRegime1 RateTypeDescription", "Duty", taxRegime1.RateTypeDescription);
					AssertContainsExactElementsInAnyOrder("TaxRegime1 Profiles PK", new[] { profileP01.PK }, taxRegime1.Profiles.Select(s => s.PK));

					AssertEquals("TaxRegime2 Parent", invoiceLine, taxRegime2.Parent);
					AssertEquals("TaxRegime2 CSI_Procedure", "P02", taxRegime2.CSI_Procedure);
					AssertEquals("TaxRegime2 ProcedureDescription", "Annulled/Revoked", taxRegime2.ProcedureDescription);
					AssertEquals("TaxRegime2 MandatoryDescription", "Optional", taxRegime2.MandatoryDescription);
					AssertEquals("TaxRegime2 CSI_SubType", "COF", taxRegime2.CSI_SubType);
					AssertEquals("TaxRegime2 RateTypeDescription", "COFINS", taxRegime2.RateTypeDescription);
					AssertContainsExactElementsInAnyOrder("TaxRegime2 Profiles PK", new[] { profileP02COF1.PK, profileP02COF2.PK }, taxRegime2.Profiles.Select(s => s.PK));

					AssertEquals("TaxRegime3 Parent", invoiceLine, taxRegime3.Parent);
					AssertEquals("TaxRegime3 CSI_Procedure", "P02", taxRegime3.CSI_Procedure);
					AssertEquals("TaxRegime3 ProcedureDescription", "Annulled/Revoked", taxRegime3.ProcedureDescription);
					AssertEquals("TaxRegime3 MandatoryDescription", "Optional", taxRegime3.MandatoryDescription);
					AssertEquals("TaxRegime3 CSI_SubType", "PIS", taxRegime3.CSI_SubType);
					AssertEquals("TaxRegime3 RateTypeDescription", "PIS", taxRegime3.RateTypeDescription);
					AssertContainsExactElementsInAnyOrder("TaxRegime3 Profiles PK", new[] { profileP02PIS.PK }, taxRegime3.Profiles.Select(s => s.PK));
				});
			}
		}

		public void TestProperties_FromMesasge()
		{
			ReferenceTestDataHelper.CreateRefCusRateType(Factory);

			var date = ZDateTime.Now;

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", date.AddDays(-5), date.AddDays(5));

			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationReference = $"01010101|CN|{date:yyyyMMdd}";
			message.EM_MessageNum = "1";
			message.EM_MessageType = MessageTypeList.Codes.RTT;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json").Replace("\"dataFatoGerador\": \"2023-04-17\"", $"\"dataFatoGerador\": \"{date.ToISO8601ShortDateString()}\"");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ValuationDate = date.Date;
			message.EM_LinkedObject = declaration;
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.DuimpLegalBase = "0006";
			invoiceLine.AddDuimpTaxRegimes();

			var tax1100PIS = invoiceLine.DuimpTaxRegimes.GetFirstElementHaving("1", "1100", "PIS");
			var tax1100COF = invoiceLine.DuimpTaxRegimes.GetFirstElementHaving("1", "1100", "COF");
			var tax0006DTY = invoiceLine.DuimpTaxRegimes.GetFirstElementHaving("1", "0006", "DTY");
			CombineAssertions(() =>
			{
				AssertEquals("TaxRegime 1100 PIS CSI_Procedure", "1100", tax1100PIS.CSI_Procedure);
				AssertEquals("TaxRegime 1100 PIS ProcedureDescription", "PIS-IMPORTAÇÃO e COFINS-IMPORTAÇÃO", tax1100PIS.ProcedureDescription);
				AssertEquals("TaxRegime 1100 PIS MandatoryDescription", "Normal", tax1100PIS.MandatoryDescription);
				AssertEquals("TaxRegime 1100 PIS CSI_SubType", "PIS", tax1100PIS.CSI_SubType);
				AssertEquals("TaxRegime 1100 PIS RateTypeDescription", "PIS", tax1100PIS.RateTypeDescription);

				AssertEquals("TaxRegime 1100 COF CSI_Procedure", "1100", tax1100COF.CSI_Procedure);
				AssertEquals("TaxRegime 1100 COF ProcedureDescription", "PIS-IMPORTAÇÃO e COFINS-IMPORTAÇÃO", tax1100COF.ProcedureDescription);
				AssertEquals("TaxRegime 1100 COF MandatoryDescription", "Normal", tax1100COF.MandatoryDescription);
				AssertEquals("TaxRegime 1100 COF CSI_SubType", "COF", tax1100COF.CSI_SubType);
				AssertEquals("TaxRegime 1100 COF RateTypeDescription", "COFINS", tax1100COF.RateTypeDescription);

				AssertEquals("TaxRegime 0006 DTY CSI_Procedure", "0006", tax0006DTY.CSI_Procedure);
				AssertEquals("TaxRegime 0006 DTY ProcedureDescription", "EX-TARIFÁRIOS TEMPORÁRIOS DE II", tax0006DTY.ProcedureDescription);
				AssertEquals("TaxRegime 0006 DTY MandatoryDescription", "Optional", tax0006DTY.MandatoryDescription);
				AssertEquals("TaxRegime 0006 DTY CSI_SubType", "DTY", tax0006DTY.CSI_SubType);
				AssertEquals("TaxRegime 0006 DTY RateTypeDescription", "Duty", tax0006DTY.RateTypeDescription);
			});
		}

		public void TestSupportsNotes()
		{
			var supporting = Factory.New<DuimpTaxRegime>();
			Assert(!supporting.SupportsNotes);
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<DuimpTaxRegime>();
			AssertEquals(CusSupportingInfoTypeList.Codes.DuimpTaxRegime, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		public void TestCanDelete()
		{
			var profiles = ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory).Select(TariffProfile.New).ToArray();
			var taxRegime = Factory.New<DuimpTaxRegime>();
			taxRegime.AddProfile(profiles.First(f => f.LegalCode == "P01" && f.IsMandatory));

			Assert("Cannot be deleted if IsMandatory", !taxRegime.CanDelete);
			AssertEquals("Should have the reason", "The legal base 'P01' cannot be deleted because is mandatory.", taxRegime.ReasonForNotAbleToDelete);

			taxRegime.Profiles.Clear();
			taxRegime.AddProfile(profiles.First(f => f.LegalCode == "P02" && !f.IsMandatory));
			Assert("Can be deleted if NOT IsMandatory", taxRegime.CanDelete);
		}

		public void TestAddProfile()
		{
			var profiles = ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory).Select(TariffProfile.New).ToArray();
			var taxRegime = Factory.New<DuimpTaxRegime>();

			taxRegime.AddProfile(profiles.First(f => f.LegalCode == "P01" && f.IsMandatory));
			AssertEquals("TaxRegime1 CSI_Procedure", "P01", taxRegime.CSI_Procedure);
			AssertEquals("TaxRegime1 MandatoryDescription", "Normal", taxRegime.MandatoryDescription);
			AssertEquals("TaxRegime1 CSI_SubType", "DTY", taxRegime.CSI_SubType);
			AssertEquals("TaxRegime1 RateTypeDescription", "Duty", taxRegime.RateTypeDescription);
			AssertContainsExactElementsInAnyOrder(new[] { "P01" }, taxRegime.Profiles.Select(s => s.LegalCode));

			taxRegime.AddProfile(profiles.First(f => f.LegalCode == "P01" && f.IsMandatory));
			AssertContainsExactElementsInAnyOrder(new[] { "P01" }, taxRegime.Profiles.Select(s => s.LegalCode));

			taxRegime.AddProfile(profiles.First(f => f.LegalCode == "P02" && !f.IsMandatory));
			AssertEquals("TaxRegime1 CSI_Procedure", "P01", taxRegime.CSI_Procedure);
			AssertEquals("TaxRegime1 MandatoryDescription", "Normal", taxRegime.MandatoryDescription);
			AssertEquals("TaxRegime1 CSI_SubType", "DTY", taxRegime.CSI_SubType);
			AssertEquals("TaxRegime1 RateTypeDescription", "Duty", taxRegime.RateTypeDescription);
			AssertContainsExactElementsInAnyOrder(new[] { "P01", "P02" }, taxRegime.Profiles.Select(s => s.LegalCode));
		}

		protected override IEnumerable<DuimpTaxRegime> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var taxRegime = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().DuimpTaxRegimes.AddNew();
			taxRegime.CSI_Code = "1";
			yield return taxRegime;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			return invoiceLine.DuimpTaxRegimes.AddNew();
		}
	}
}
