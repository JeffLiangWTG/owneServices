using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCMandatoryTreatmentAttributesResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCMandatoryTreatmentAttributesResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "RTT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "MTT" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCMandatoryTreatmentAttributesResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "Mandatory Treatment Attributes Response Message", processor.MessageFriendlyName);
		}

		public void TestProcessResponseMessageWithOptionalTreatmentAttributes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			var responseMessageRTT = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageMTT);
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "160")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Tariff = "01010101";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_Tariff = "01010101";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var responseMessageMTT = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessageMTT.EM_MessageText = responseMessageRTT;
			Factory.Save();

			ExecuteMessageProcessor(responseMessageMTT);
			AssertEquals("Messages count", 3, declaration.Messages.Count);
			CombineAssertions("Check EDIMessage", () =>
			{
				AssertEquals("EM_LinkUniqueID", declaration.PK, responseMessageMTT.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", declaration.TableName, responseMessageMTT.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessageMTT.EM_Status);
				AssertEquals("EM_ApplicationReference", "01010101|CN|20230417", responseMessageMTT.EM_ApplicationReference);

				AssertDuimpTaxRegimesAdded(invoiceLine1, [Constants.RateTypes.PIS, Constants.RateTypes.Cofins]);
				AssertDuimpTaxRegimesAdded(invoiceLine2, [Constants.RateTypes.PIS, Constants.RateTypes.Cofins]);
			});

			var responseMessageOTA = declaration.Messages.Cast<BREDIMessage>().FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes);

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", declaration.PK, responseMessageOTA.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", declaration.TableName, responseMessageOTA.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, responseMessageOTA.EM_Status);
				AssertEquals("EM_MessageText", BRMessageTestHelper.GetEmbeddedResource(GeneratedMessageOTA), responseMessageOTA.EM_MessageText.TrimStart());
				AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.BRCustoms, responseMessageOTA.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, responseMessageOTA.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.RTT, responseMessageOTA.EM_MessageType);
				AssertEquals("IsInDatabase", false, responseMessageOTA.IsInDatabase);
			});

			responseMessageMTT = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessageMTT.EM_MessageText = responseMessageRTT.Replace(Constants.LegalBasisType.Optional, Constants.LegalBasisType.Normal);
			Factory.Save();

			ExecuteMessageProcessor(responseMessageMTT);
			AssertEquals("Messages count", 3, declaration.Messages.Count);
			CombineAssertions("Check EDIMessage", () =>
			{
				AssertEquals("EM_LinkUniqueID", declaration.PK, responseMessageMTT.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", declaration.TableName, responseMessageMTT.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessageMTT.EM_Status);
				AssertEquals("EM_ApplicationReference", "01010101|CN|20230417", responseMessageMTT.EM_ApplicationReference);

				AssertDuimpTaxRegimesAdded(invoiceLine1, [Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty]);
				AssertDuimpTaxRegimesAdded(invoiceLine2, [Constants.RateTypes.PIS, Constants.RateTypes.Cofins, Constants.RateTypes.ImportDuty]);
			});
		}

		public void TestProcessResponseMessageWithoutOptionalTreatmentAttributes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "160")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var responseMessageMTT = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessageMTT.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageMTTWithoutOTA);
			Factory.Save();

			ExecuteMessageProcessor(responseMessageMTT);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", declaration.PK, responseMessageMTT.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", declaration.TableName, responseMessageMTT.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessageMTT.EM_Status);
				AssertEquals("EM_ApplicationReference", "01010101|CN|20230417", responseMessageMTT.EM_ApplicationReference);

				AssertDuimpTaxRegimesAdded(invoiceLine, [Constants.RateTypes.PIS, Constants.RateTypes.Cofins]);

				AssertEquals("Messages count", 2, declaration.Messages.Count);
				var responseMessageOTA = declaration.Messages.Cast<BREDIMessage>().FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes);
				AssertNull(responseMessageOTA);
			});
		}

		public void TestProcessResponseMessage_HasCusEntryHeader()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010102", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "160")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);

			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice1 = declaration1.Invoices.AddNew();

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice2 = declaration2.Invoices.AddNew();

			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Description = "ENTRY01";
			var header1 = declaration1.ActiveEntryHeaders.AddNew();
			header1.CH_CEI_Instruction = instruction1.PK;
			header1.CH_AuthorityVersion = "1";

			var instruction2 = declaration1.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Description = "ENTRY02";
			var header2 = declaration1.ActiveEntryHeaders.AddNew();
			header2.CH_CEI_Instruction = instruction2.PK;
			header2.CH_AuthorityVersion = "0";

			var instruction3 = declaration1.CustomsEntryInstructions.AddNew();
			instruction3.CEI_Description = "ENTRY03";
			var header3 = declaration1.ActiveEntryHeaders.AddNew();
			header3.CH_CEI_Instruction = instruction3.PK;

			var instruction4 = declaration1.CustomsEntryInstructions.AddNew();
			instruction4.CEI_Description = "ENTRY04";

			JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice, ZGuid instructionPK, ZShort lineNo, string tariffCode = "01010101", string countryOfOrigin = Core.Constants.CountryCodes.China)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_LineNo = lineNo;
				invoiceLine.JI_Tariff = tariffCode;
				invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
				invoiceLine.JI_CEI = instructionPK;
				return invoiceLine;
			}

			var invoiceLine1 = CreateInvoiceLine(invoice1, instruction1.PK, lineNo: 1);
			var invoiceLine2 = CreateInvoiceLine(invoice1, instruction2.PK, lineNo: 2);
			var invoiceLine3 = CreateInvoiceLine(invoice1, instruction3.PK, lineNo: 3);
			var invoiceLine4 = CreateInvoiceLine(invoice1, instruction4.PK, lineNo: 4);
			var invoiceLine5 = CreateInvoiceLine(invoice1, ZGuid.Empty, lineNo: 5);
			var invoiceLine6 = CreateInvoiceLine(invoice1, instruction1.PK, lineNo: 6, tariffCode: "01010102");
			var invoiceLine7 = CreateInvoiceLine(invoice1, instruction1.PK, lineNo: 7, countryOfOrigin: Core.Constants.CountryCodes.Mexico);
			var invoiceLine8 = CreateInvoiceLine(invoice2, ZGuid.Empty, lineNo: 8);

			Factory.Save();

			var newFactory = NewFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration1.PK);
			var responseMessageMTT = BRCResponseMessageProcessorTest.CreateResponseMessage(loadedDeclaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessageMTT.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageMTT);
			newFactory.Save();

			ExecuteMessageProcessor(responseMessageMTT);

			CombineAssertions(() =>
			{
				AssertEquals("Only one db hint to table CusSupportingInfo", 1, newFactory.TableSelects.Single(c => c.TableName == CusSupportingInfoSchema.Constants.TableName).Value);

				foreach (var invoiceLine in new[] { invoiceLine6, invoiceLine7, invoiceLine8 })
				{
					var loadedInvoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
					AssertEquals($"No DuimpTaxRegime added on {invoiceLine.JI_LineNo}", 0, loadedInvoiceLine.DuimpTaxRegimes.Count);
					AssertEquals($"No TaxRegimeAttributes added on {invoiceLine.JI_LineNo}", 0, loadedInvoiceLine.TaxRegimeAttributes.Count);
				}
				foreach (var invoiceLine in new[] { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, invoiceLine5 })
				{
					var loadedInvoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
					AssertDuimpTaxRegimesAdded(loadedInvoiceLine, [Constants.RateTypes.PIS, Constants.RateTypes.Cofins]);
					AssertDuimpTaxRegimeAttributesAdded(loadedInvoiceLine);
				}
			});
		}

		void AssertDuimpTaxRegimesAdded(JobComInvoiceLine invoiceLine, string[] expectedRateTypes)
		{
			AssertEquals($"Count of DuimpTaxRegimes on {invoiceLine}", expectedRateTypes.Length, invoiceLine.DuimpTaxRegimes.Count);
			AssertDuimpTaxRegime(Constants.RateTypes.PIS, "1", "1100");
			AssertDuimpTaxRegime(Constants.RateTypes.Cofins, "1", "1100");
			AssertDuimpTaxRegime(Constants.RateTypes.ImportDuty, "1", "0006");

			void AssertDuimpTaxRegime(string rateType, string expectedRegime, string expectedLegalCode)
			{
				var taxRegime = invoiceLine.DuimpTaxRegimes.Where(x => x.CSI_SubType == rateType).SingleOrDefault();
				if (expectedRateTypes.Contains(rateType))
				{
					AssertNotNull($"DuimpTaxRegime {rateType} should be added", taxRegime);
					AssertEquals("CSI_Code", expectedRegime, taxRegime.CSI_Code);
					AssertEquals("CSI_Procedure", expectedLegalCode, taxRegime.CSI_Procedure);
					AssertEquals("IsMandatory", true, taxRegime.IsMandatory);
				}
				else
				{
					AssertNull($"DuimpTaxRegime {rateType} should NOT be added", taxRegime);
				}
			}
		}

		void AssertDuimpTaxRegimeAttributesAdded(JobComInvoiceLine invoiceLine, bool containsAtt15574 = false)
		{
			var expectedAttributes = new List<string> { "ATT_13741", "ATT_13715", "ATT_3741", "ATT_3941" };
			if (containsAtt15574)
			{
				expectedAttributes.Add("ATT_15574");
			}
			invoiceLine.TaxRegimeAttributes.Rebuild();
			AssertContainsExactElementsInAnyOrder($"InvoiceLine Number {invoiceLine.JI_LineNo} -> TaxRegimeAttributes", expectedAttributes, invoiceLine.TaxRegimeAttributes.Select(x => x.CY_Code).ToArray());

			var att13741 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13741");
			var att13741PossibleValues = att13741.Lookups.PossibleValues;
			AssertContainsExactElementsInAnyOrder($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_13741 -> PossibleValues", new[] { "0002", "XXXX" }, att13741PossibleValues.GetAllCodes());
			AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_13741 -> 0002 -> Description", "Importador NÃO é pessoa jurídica fabricante de máquinas e veículos relacionados no Anexo II do Art. 1º da Lei 10.485/2002. (§9º, Art. 8º. Lei 10865/2004)\n", att13741PossibleValues.GetDescriptionFromCode("0002"));
			AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_13741 -> XXXX -> Description", "Não se enquadra em outra opção", att13741PossibleValues.GetDescriptionFromCode("XXXX"));

			var att13715 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13715");
			var att13715PossibleValues = att13715.Lookups.PossibleValues;
			AssertContainsExactElementsInAnyOrder($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_13715 -> PossibleValues", new[] { "0001", "XXXX" }, att13715PossibleValues.GetAllCodes());
			AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_13715 -> 0001 -> Description", "Se enquadra na descrição do Anexo II da Lei 10.485/2002", att13715PossibleValues.GetDescriptionFromCode("0001"));
			AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_13715 -> XXXX -> Description", "Não se enquadra em outra opção", att13715PossibleValues.GetDescriptionFromCode("XXXX"));

			if (containsAtt15574)
			{
				var att15574 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_15574");
				var att15574PossibleValues = att15574.Lookups.PossibleValues;
				AssertContainsExactElementsInAnyOrder($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> PossibleValues", new[] { "0007", "0062", "0063", "0064", "0065", "0085", "0086", "0093", "0094", "0095" }, att15574PossibleValues.GetAllCodes());
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0007 -> Description", "Rotores turbo-fan para bombeamento de ar através de sucção central e descarga em fluxo radial, disposto em pás aerodinâmicas com torção tridimensional, conformado através de injeção de precisão das partes (rotor-turbo + anel flange) e unidos através do processo de solda a laser, executada em atmosfera classificada com controle de partículas em suspensão, controle de humidade e controle de temperatura, para uso em unidades evaporadoras (indoor unit) de sistemas de ar condicionado com expansão direta de alta eficiência.", att15574PossibleValues.GetDescriptionFromCode("0007"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0062 -> Description", "Rotores completos dotados de eixo principal e quatro impelidores, para compressão do ar atmosférico do compressor centrífugo cuja vazão nominal é de 10Nm³/h, pressão de entrada 0,972barA e pressão de saída 7barA.", att15574PossibleValues.GetDescriptionFromCode("0062"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0063 -> Description", "Difusores de 1° estágio para redução da velocidade do ar atmosférico do compressor centrífugo cuja vazão nominal é de 10nm³/h, pressão de entrada 0,972barA e pressão de saída 7barA.", att15574PossibleValues.GetDescriptionFromCode("0063"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0064 -> Description", "Difusores do 1º estádio para redução da velocidade do ar atmosférico do compressor centrifugo cuja vazão nominal é de 350.000Nm3/h, pressão de entrada 0,972bara e pressão de saída 7bara.", att15574PossibleValues.GetDescriptionFromCode("0064"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0065 -> Description", "Rotores completos dotados de um eixo principal e 4 impelidores, para compressão do ar atmosférico do compressor centrifugo cuja vazão nominal é de 350.000Nm3/h, pressão de entrada 0,972bara e pressão de saída 7bara.", att15574PossibleValues.GetDescriptionFromCode("0065"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0085 -> Description", "Cruzetas para acionamento de êmbolos, com no mínimo 2,84m de comprimento e 0,90m de largura, para uso em hiper compressores com máxima pressão de operação de 269MPa, usadas na produção de polietileno de baixa densidade, com estrutura em aço fundido, pino cilíndrico, tirantes, porcas e sapatas deslizantes.", att15574PossibleValues.GetDescriptionFromCode("0085"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0086 -> Description", "Conjuntos rotativos utilizados para compor as carcaças de compressores de hidrogênio de primeiro ou segundo estágio, com a função de compressão do gás de pureza de no mínimo de 99%, dotados de um rotor macho com diâmetro entre 163 e 204mm, com 4 lóbulos; e de um rotor fêmea com diâmetro entre 163 e 204mm, com 6 lóbulos; fabricados em aço forjado de baixa liga Cr-Mo, com tratamento térmico, para uso exclusivo em compressores do tipo parafuso, acionados por motor elétrico com potência nominal de 1.410kW.", att15574PossibleValues.GetDescriptionFromCode("0086"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0093 -> Description", "Carcaças para compressor de grande porte de fluxo axial para altos fornos, fabricadas em aço forjado, contendo sistema com 614 palhetas em aço inox JIS SUS403 aerodinâmicas móveis de ângulo variável, dispostas em 12 estágios, que permitam controle da vazão de ar de até 6.400Nm³/h, à pressão de 4,2kgf/cm², com taxa de compressão de 5,2.", att15574PossibleValues.GetDescriptionFromCode("0093"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0094 -> Description", "Rotores para compressor de grande porte de fluxo axial para altos fornos, fabricado em Aço NiCrMo forjado, composto por eixo, acoplamentos e 638 palhetas em aço inox JIS SUS403 aerodinâmicas, dispostas em 12 estágios, que gira a 3.757rpm, permitindo fornecimento de ar soprado a uma vazão de até 6.400Nm³/h, à pressão de 4,2kgf/cm² com taxa de compressão de 5,2.", att15574PossibleValues.GetDescriptionFromCode("0094"));
				AssertEquals($"InvoiceLine Number {invoiceLine.JI_LineNo} ATT_15574 -> 0095 -> Description", "Rotores dotados de eixo principal e 5 impelidores, para compressão de propeno, para uso exclusivo em compressor centrífugo com vazão nominal de 59.118nm³/h, pressão de sucção nominal de 1,599bara e pressão de descarga nominal de 17,535bara.", att15574PossibleValues.GetDescriptionFromCode("0095"));
			}
		}

		public void TestProcessResponseMessage_HasNoCountryMapped()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "150")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "01010101";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessage.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageMTT);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", declaration.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Code '160' has not Country mapped.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_NoMatchTariff()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "160")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010102";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessage.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageMTT);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", declaration.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Invoice Line with Tariff '01010101' and Origin Country 'CN' not found.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_NoMatchCountry()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "160")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Argentina;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessage.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageMTT);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", declaration.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Invoice Line with Tariff '01010101' and Origin Country 'CN' not found.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessage.EM_MessageText = @"";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", declaration.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
		}

		const string GeneratedMessageOTA = "GeneratedMessageOTA.json";

		const string ResponseMessageMTT = "ResponseMessageMTT.json";

		const string ResponseMessageMTTWithoutOTA = "ResponseMessageMTTWithoutOTA.json";
	}
}
