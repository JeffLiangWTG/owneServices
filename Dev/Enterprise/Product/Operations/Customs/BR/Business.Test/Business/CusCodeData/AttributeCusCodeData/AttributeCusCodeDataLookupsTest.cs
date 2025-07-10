using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class AttributeCusCodeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPossibleValues_FromZZData()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			Factory.Save();

			var valuesList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("01","FOR USE IN AGRICULTURE"),
				new KeyValuePair<string, string>("99","OTHER USES, EXCEPT AGRICULTURAL OR ANIMAL"),
				new KeyValuePair<string, string>("02","FOR USE IN ANIMAL"),
			};

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_2557", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, values: valuesList);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3061", Universal.Constants.ProfileQuestion.AnswerDataTypes.String);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3887", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3890", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3893", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, values: valuesList);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";

			var attribute = invoiceLine.Attributes.GetFirstElementHaving("ATT_2557");
			var possibleValues = attribute.Lookups.PossibleValues;
			AssertEquals(3, possibleValues.Count);
			AssertContainsExactElementsInExactOrder(new[] { "01", "02", "99" }, possibleValues.GetAllCodes());

			attribute = invoiceLine.Attributes.GetFirstElementHaving("ATT_3061");
			possibleValues = attribute.Lookups.PossibleValues;
			AssertEquals(0, possibleValues.Count);

			attribute = invoiceLine.Attributes.GetFirstElementHaving("ATT_3887");
			possibleValues = attribute.Lookups.PossibleValues;
			AssertEquals(0, possibleValues.Count);

			attribute = invoiceLine.Attributes.GetFirstElementHaving("ATT_3890");
			possibleValues = attribute.Lookups.PossibleValues;
			AssertEquals(2, possibleValues.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "N", "S" }, possibleValues.GetAllCodes());

			attribute = invoiceLine.Attributes.GetFirstElementHaving("ATT_3893");
			possibleValues = attribute.Lookups.PossibleValues;
			AssertEquals(3, possibleValues.Count);
			AssertContainsExactElementsInExactOrder(new[] { "01", "02", "99" }, possibleValues.GetAllCodes());
		}

		public void TestPossibleValues_FromMessage()
		{
			var date = ZDateTime.Now;

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", date.AddDays(-5), date.AddDays(5));
			Factory.Save();

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
			invoiceLine.DuimpLegalBase = "0009";
			invoiceLine.AddDuimpTaxRegimes();
			var att2872 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_2872");
			var att2872PossibleValues = att2872.Lookups.PossibleValues;
			var att13743 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13743");
			var att13743PossibleValues = att13743.Lookups.PossibleValues;

			AssertContainsExactElementsInAnyOrder(new[] { "06012000", "06021000", "06022000", "07032000" }, att2872PossibleValues.GetAllCodes());
			CombineAssertions(() =>
			{
				AssertEquals("06012000", "06012000", att2872PossibleValues.GetDescriptionFromCode("06012000"));
				AssertEquals("06021000", "06021000", att2872PossibleValues.GetDescriptionFromCode("06021000"));
				AssertEquals("06022000", "06022000", att2872PossibleValues.GetDescriptionFromCode("06022000"));
				AssertEquals("07032000", "07032000", att2872PossibleValues.GetDescriptionFromCode("07032000"));
			});

			AssertContainsExactElementsInAnyOrder(new[] { "0001", "0003", "0004", "0005", "0006", "9999", "0002", "0007", "0008", "0009", "0010", "0013" }, att13743PossibleValues.GetAllCodes());
			CombineAssertions(() =>
			{
				AssertEquals("0001", "Hybiscus, em folhas frescas", att13743PossibleValues.GetDescriptionFromCode("0001"));
				AssertEquals("0002", "Senna, exceto em folhas frescas", att13743PossibleValues.GetDescriptionFromCode("0002"));
				AssertEquals("0003", "Giz fosfatado", att13743PossibleValues.GetDescriptionFromCode("0003"));
				AssertEquals("0004", "Tirotricina", att13743PossibleValues.GetDescriptionFromCode("0004"));
				AssertEquals("0005", "Viomicina", att13743PossibleValues.GetDescriptionFromCode("0005"));
				AssertEquals("0006", "Cicloserina", att13743PossibleValues.GetDescriptionFromCode("0006"));
				AssertEquals("0007", "Gabromicina", att13743PossibleValues.GetDescriptionFromCode("0007"));
				AssertEquals("0008", "Abacateiro para propagação - Persea americana", att13743PossibleValues.GetDescriptionFromCode("0008"));
				AssertEquals("0009", "Morangueiro para propagação - Fragaria spp.", att13743PossibleValues.GetDescriptionFromCode("0009"));
				AssertEquals("0010", "Abacaxi para propagação - Ananas comosus", att13743PossibleValues.GetDescriptionFromCode("0010"));
				AssertEquals("0013", "De canela (Extra-quota)", att13743PossibleValues.GetDescriptionFromCode("0013"));
				AssertEquals("9999", "Outras espécies de chá", att13743PossibleValues.GetDescriptionFromCode("9999"));
			});

			invoiceLine.RemoveDuimpTaxRegimes(invoiceLine.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "0009"));

			invoiceLine.DuimpLegalBase = "0009";
			invoiceLine.AddDuimpTaxRegimes();
			AssertSame(att2872PossibleValues, invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_2872").Lookups.PossibleValues);
			AssertSame(att13743PossibleValues, invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13743").Lookups.PossibleValues);
		}

		public void TestPossibleValues_GoodsCatalog()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog.CGC_Tariff = "87654321";

			var attribute = catalog.Attributes.GetFirstElementHaving("ATT_2557");
			AssertContainsExactElementsInAnyOrder(new[] { "1", "2" }, attribute.Lookups.PossibleValues.GetAllCodes());

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				catalog.Attributes.Rebuild();
				attribute = catalog.Attributes.GetFirstElementHaving("ATT1");
				AssertContainsExactElementsInAnyOrder(new[] { "N", "S" }, attribute.Lookups.PossibleValues.GetAllCodes());

				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
				attribute = catalog.Attributes.GetFirstElementHaving("ATT_3891");
				AssertContainsExactElementsInAnyOrder(new[] { "1", "2", "3", "4", "5" }, attribute.Lookups.PossibleValues.GetAllCodes());

				attribute = catalog.Attributes.GetFirstElementHaving("ATT_3061");
				AssertEquals(0, attribute.Lookups.PossibleValues.GetAllCodes().Length);
			}
		}
	}
}
