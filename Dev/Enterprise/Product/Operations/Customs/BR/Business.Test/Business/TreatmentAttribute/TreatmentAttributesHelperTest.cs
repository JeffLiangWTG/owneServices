using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class TreatmentAttributesHelperTest : TestCaseWithFactory
	{
		public void TestGetDuimpLegalBaseList_FromMessage()
		{
			var date = new ZDateTime(2023, 04, 17);

			var messageMTT1 = Factory.New<BREDIMessage>();
			messageMTT1.EM_ApplicationReference = "01010101|CN|20230415";
			messageMTT1.EM_MessageNum = "1";
			messageMTT1.EM_MessageType = MessageTypeList.Codes.RTT;
			messageMTT1.EM_MessageSubType = EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment;
			messageMTT1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageMTT1.EM_Status = EDIMessage.Status.Received;
			messageMTT1.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageMTT.json");
			messageMTT1.EM_SystemCreateTimeUtc = date.AddDays(-2);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ValuationDate = date.Date;
			messageMTT1.EM_LinkedObject = declaration;
			Factory.Save();

			AssertEquals(null, declaration.GetDuimpLegalBaseListFromMessage("02020202", "CN", optionalOnly: true));
			AssertEquals(null, declaration.GetDuimpLegalBaseListFromMessage("01010101", "MX", optionalOnly: true));

			var legalBaseList = declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: true);
			AssertContainsExactElementsInExactOrder(new[] { "0006", "0903", "0908" }, legalBaseList.GetAllCodesZString());
			AssertEquals("Code 0006 Description", "EX-TARIFÁRIOS TEMPORÁRIOS DE II", legalBaseList.GetDescriptionFromCode("0006"));
			AssertEquals("Code 0903 Description", "ADMISSÃO TEMPORÁRIA PARA APERFEIÇOAMENTO ATIVO ", legalBaseList.GetDescriptionFromCode("0903"));
			AssertEquals("Code 0908 Description", "ADMISSÃO NO GNL-TEMPORÁRIO ", legalBaseList.GetDescriptionFromCode("0908"));
			AssertSame(legalBaseList, declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: true));

			legalBaseList = declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: false);
			AssertContainsExactElementsInExactOrder(new[] { "0006", "0903", "0908", "1100" }, legalBaseList.GetAllCodesZString());
			AssertEquals("Code 1100 Description", "PIS-IMPORTAÇÃO e COFINS-IMPORTAÇÃO", legalBaseList.GetDescriptionFromCode("1100"));
			AssertSame(legalBaseList, declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: false));

			var messageOTA1 = Factory.New<BREDIMessage>();
			messageOTA1.EM_ApplicationReference = "01010101|CN|20230416";
			messageOTA1.EM_MessageNum = "2";
			messageOTA1.EM_MessageType = MessageTypeList.Codes.RTT;
			messageOTA1.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			messageOTA1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageOTA1.EM_Status = EDIMessage.Status.Received;
			messageOTA1.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json");
			messageOTA1.EM_SystemCreateTimeUtc = date.AddDays(-1);
			messageOTA1.EM_LinkedObject = declaration;
			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			legalBaseList = declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: true);
			AssertContainsExactElementsInExactOrder(new[] { "0006", "0007", "0008", "0009", "0903", "0908" }, legalBaseList.GetAllCodesZString());
			AssertEquals("Code 0007 Description", "SGPC - SISTEMA GLOBAL DE PREFERÊNCIAS COMERCIAIS", legalBaseList.GetDescriptionFromCode("0007"));
			AssertEquals("Code 0008 Description", "IPI Opcional Tributo", legalBaseList.GetDescriptionFromCode("0008"));
			AssertEquals("Code 0009 Description", "AAP.AG N° 2 -  SEMENTES MERCOSUL X BOLIVIA,CHILE, CUBA, EQUADOR E PERU", legalBaseList.GetDescriptionFromCode("0009"));
			AssertEquals("Code 0006 Description", "EX-TARIFÁRIOS TEMPORÁRIOS DE II", legalBaseList.GetDescriptionFromCode("0006"));
			AssertEquals("Code 0903 Description", "ADMISSÃO TEMPORÁRIA PARA APERFEIÇOAMENTO ATIVO ", legalBaseList.GetDescriptionFromCode("0903"));
			AssertEquals("Code 0908 Description", "ADMISSÃO NO GNL-TEMPORÁRIO ", legalBaseList.GetDescriptionFromCode("0908"));

			legalBaseList = declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: false);
			AssertContainsExactElementsInExactOrder(new[] { "0006", "0007", "0008", "0009", "0903", "0908", "1100" }, legalBaseList.GetAllCodesZString());
			AssertEquals("Code 1100 Description", "PIS-IMPORTAÇÃO e COFINS-IMPORTAÇÃO", legalBaseList.GetDescriptionFromCode("1100"));

			var messageMTT2 = Factory.New<BREDIMessage>();
			messageMTT2.EM_ApplicationReference = "01010101|CN|20230416";
			messageMTT2.EM_MessageNum = "2";
			messageMTT2.EM_MessageType = MessageTypeList.Codes.RTT;
			messageMTT2.EM_MessageSubType = EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment;
			messageMTT2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageMTT2.EM_Status = EDIMessage.Status.Received;
			messageMTT2.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageMTT.json");
			messageMTT2.EM_SystemCreateTimeUtc = date;
			messageMTT2.EM_LinkedObject = declaration;
			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			legalBaseList = declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: true);
			AssertContainsExactElementsInExactOrder(new[] { "0006", "0903", "0908" }, legalBaseList.GetAllCodesZString());
			AssertEquals("Code 0006 Description", "EX-TARIFÁRIOS TEMPORÁRIOS DE II", legalBaseList.GetDescriptionFromCode("0006"));
			AssertEquals("Code 0903 Description", "ADMISSÃO TEMPORÁRIA PARA APERFEIÇOAMENTO ATIVO ", legalBaseList.GetDescriptionFromCode("0903"));
			AssertEquals("Code 0908 Description", "ADMISSÃO NO GNL-TEMPORÁRIO ", legalBaseList.GetDescriptionFromCode("0908"));

			legalBaseList = declaration.GetDuimpLegalBaseListFromMessage("01010101", "CN", optionalOnly: false);
			AssertContainsExactElementsInExactOrder(new[] { "0006", "0903", "0908", "1100" }, legalBaseList.GetAllCodesZString());
			AssertEquals("Code 1100 Description", "PIS-IMPORTAÇÃO e COFINS-IMPORTAÇÃO", legalBaseList.GetDescriptionFromCode("1100"));
		}

		public void TestGetTariffProfiles_FromMessage()
		{
			var date = new ZDateTime(2023, 04, 17);

			var messageMTT1 = Factory.New<BREDIMessage>();
			messageMTT1.EM_ApplicationReference = "01010101|CN|20230415";
			messageMTT1.EM_MessageNum = "1";
			messageMTT1.EM_MessageType = MessageTypeList.Codes.RTT;
			messageMTT1.EM_MessageSubType = EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment;
			messageMTT1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageMTT1.EM_Status = EDIMessage.Status.Received;
			messageMTT1.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageMTT.json");
			messageMTT1.EM_SystemCreateTimeUtc = date.AddDays(-2);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ValuationDate = date.Date;
			messageMTT1.EM_LinkedObject = declaration;
			Factory.Save();

			AssertEquals(null, declaration.GetTariffProfilesFromMessage("02020202", "CN"));
			AssertEquals(null, declaration.GetTariffProfilesFromMessage("01010101", "MX"));

			var tariffProfile = declaration.GetTariffProfilesFromMessage("01010101", "CN");
			AssertContainsExactElementsInAnyOrder(new[] { "0006", "0903", "0903", "0908", "0908", "1100", "1100", "1100", "1100", "1100", "1100" }, tariffProfile.Select(s => s.LegalCode));
			AssertSame(tariffProfile, declaration.GetTariffProfilesFromMessage("01010101", "CN"));

			var messageOTA1 = Factory.New<BREDIMessage>();
			messageOTA1.EM_ApplicationReference = "01010101|CN|20230416";
			messageOTA1.EM_MessageNum = "2";
			messageOTA1.EM_MessageType = MessageTypeList.Codes.RTT;
			messageOTA1.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			messageOTA1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageOTA1.EM_Status = EDIMessage.Status.Received;
			messageOTA1.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json");
			messageOTA1.EM_SystemCreateTimeUtc = date.AddDays(-1);
			messageOTA1.EM_LinkedObject = declaration;
			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			tariffProfile = declaration.GetTariffProfilesFromMessage("01010101", "CN");
			AssertContainsExactElementsInAnyOrder(new[] { "0006", "0007", "0007", "0008", "0009", "0009", "1100", "1100", "1100", "1100", "0903", "0903", "0908", "0908" }, tariffProfile.Select(s => s.LegalCode));

			var messageMTT2 = Factory.New<BREDIMessage>();
			messageMTT2.EM_ApplicationReference = "01010101|CN|20230417";
			messageMTT2.EM_MessageNum = "3";
			messageMTT2.EM_MessageType = MessageTypeList.Codes.RTT;
			messageMTT2.EM_MessageSubType = EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment;
			messageMTT2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageMTT2.EM_Status = EDIMessage.Status.Received;
			messageMTT2.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageMTT.json");
			messageMTT2.EM_SystemCreateTimeUtc = date;
			messageMTT2.EM_LinkedObject = declaration;
			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			tariffProfile = declaration.GetTariffProfilesFromMessage("01010101", "CN");
			AssertContainsExactElementsInAnyOrder(new[] { "0006", "0903", "0903", "0908", "0908", "1100", "1100", "1100", "1100", "1100", "1100" }, tariffProfile.Select(s => s.LegalCode));
		}

		public void TestGetTariffProfileQuestions_FromMessage()
		{
			var date = new ZDateTime(2023, 04, 17);

			var messageMTT = Factory.New<BREDIMessage>();
			messageMTT.EM_ApplicationReference = "01010101|CN|20230415";
			messageMTT.EM_MessageNum = "1";
			messageMTT.EM_MessageType = MessageTypeList.Codes.RTT;
			messageMTT.EM_MessageSubType = EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment;
			messageMTT.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageMTT.EM_Status = EDIMessage.Status.Received;
			messageMTT.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageMTT.json");
			messageMTT.EM_SystemCreateTimeUtc = date.AddDays(-2);

			var messageOTA = Factory.New<BREDIMessage>();
			messageOTA.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			messageOTA.EM_ApplicationReference = "01010101|CN|20230417";
			messageOTA.EM_MessageNum = "1";
			messageOTA.EM_MessageType = MessageTypeList.Codes.RTT;
			messageOTA.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			messageOTA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageOTA.EM_Status = EDIMessage.Status.Received;
			messageOTA.EM_SystemCreateTimeUtc = date.AddDays(-1);
			messageOTA.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ValuationDate = date.Date;
			messageMTT.EM_LinkedObject = declaration;
			messageOTA.EM_LinkedObject = declaration;
			Factory.Save();

			AssertEquals(null, declaration.GetTariffProfileQuestionsFromMessage("02020202", "CN"));
			AssertEquals(null, declaration.GetTariffProfileQuestionsFromMessage("01010101", "MX"));

			var tariffProfileQuestion = declaration.GetTariffProfileQuestionsFromMessage("01010101", "CN");
			AssertContainsExactElementsInExactOrder(new[] { "ATT_13715", "ATT_13741", "ATT_13743", "ATT_15574", "ATT_2872", "ATT_2874" }, tariffProfileQuestion.OrderBy(o => o.Code).Select(s => s.Code));
			AssertContainsExactElementsInExactOrder(new[] { "LIST", "LIST", "LIST", "LIST", "LIST", "LIST" }, tariffProfileQuestion.OrderBy(o => o.Code).Select(s => s.AnswerDataType));
			AssertContainsExactElementsInExactOrder(new[] { ZShort.Zero, ZShort.Zero, ZShort.Zero, ZShort.Zero, ZShort.Zero, ZShort.Zero }, tariffProfileQuestion.OrderBy(o => o.Code).Select(s => s.AnswerDecimalPlaces));
			AssertSame(tariffProfileQuestion, declaration.GetTariffProfileQuestionsFromMessage("01010101", "CN"));

			messageMTT.EM_SystemCreateTimeUtc = date;
			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			tariffProfileQuestion = declaration.GetTariffProfileQuestionsFromMessage("01010101", "CN");
			AssertContainsExactElementsInExactOrder(new[] { "ATT_13715", "ATT_13741", "ATT_15574", "ATT_3741", "ATT_3941" }, tariffProfileQuestion.OrderBy(o => o.Code).Select(s => s.Code));
			AssertContainsExactElementsInExactOrder(new[] { "LIST", "LIST", "LIST", "NUMBER", "NUMBER" }, tariffProfileQuestion.OrderBy(o => o.Code).Select(s => s.AnswerDataType));
			AssertContainsExactElementsInExactOrder(new[] { ZShort.Zero, ZShort.Zero, ZShort.Zero, ZShort.Zero, (ZShort)5 }, tariffProfileQuestion.OrderBy(o => o.Code).Select(s => s.AnswerDecimalPlaces));
			AssertSame(tariffProfileQuestion, declaration.GetTariffProfileQuestionsFromMessage("01010101", "CN"));
		}

		public void TestGetTaxType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ImportDuty", Constants.RateTypes.ImportDuty, TreatmentAttributesHelper.GetTaxType("1"));
				AssertEquals("IPI", Constants.RateTypes.IPI, TreatmentAttributesHelper.GetTaxType("2"));
				AssertEquals("Antidumping", Constants.RateTypes.Antidumping, TreatmentAttributesHelper.GetTaxType("3"));
				AssertEquals("PIS", Constants.RateTypes.PIS, TreatmentAttributesHelper.GetTaxType("6"));
				AssertEquals("Cofins", Constants.RateTypes.Cofins, TreatmentAttributesHelper.GetTaxType("7"));
				AssertEquals("Should be Empty", ZString.Empty, TreatmentAttributesHelper.GetTaxType("8"));
				AssertEquals("Should be Empty", ZString.Empty, TreatmentAttributesHelper.GetTaxType(null));
			});
		}

		public void TestLoadMostRecentTreatmentAttributesMessage()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var date = new ZDateTime(2023, 04, 17);

			var message1 = CreateMessage(declaration1, "01010101|CN|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date);
			var message2 = CreateMessage(declaration2, "01010101|CN|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date.AddDays(1));
			var message3 = CreateMessage(declaration1, "01010101|CN|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date.AddDays(-1));
			var message4 = CreateMessage(declaration1, "01010101|CN|20230417", MessageTypeList.Codes.CDD, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date.AddDays(1));
			var message5 = CreateMessage(declaration1, "01010101|CN|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.GoodsCatalog, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date.AddDays(5));
			var message6 = CreateMessage(declaration1, "01010101|CN|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, EDIMessage.Direction.Transmit, EDIMessage.Status.Received, date.AddDays(1));
			var message7 = CreateMessage(declaration2, "01010101|CN|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date.AddDays(2));
			var message8 = CreateMessage(declaration2, "02020202|CN|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date);
			var message9 = CreateMessage(declaration2, "01010101|AU|20230417", MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, EDIMessage.Direction.Receive, EDIMessage.Status.Received, date);
			Factory.Save();

			var declaration3 = Factory.New<JobDeclaration>();
			var anotherFactory = new BusinessObjectFactory();
			declaration1 = anotherFactory.Load<JobDeclaration>(declaration1.PK);
			declaration2 = anotherFactory.Load<JobDeclaration>(declaration2.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Declaration == null => Should be NULL", null, TreatmentAttributesHelper.LoadMostRecentTreatmentAttributesMessage(null, "01010101", "CN", EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes));
				AssertEquals("Declaration NOT in database => Should be NULL", null, declaration3.LoadMostRecentTreatmentAttributesMessage("01010101", "CN", EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes));
				AssertEquals("Tariff Code == Empty => Should be NULL", null, declaration1.LoadMostRecentTreatmentAttributesMessage(ZString.Empty, "CN", EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes));
				AssertEquals("Country == Empty => Should be NULL", null, declaration1.LoadMostRecentTreatmentAttributesMessage("01010101", ZString.Empty, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes));
				AssertEquals("Declaration1, 01010101 and CN => Should be Message1", message1.PK, declaration1.LoadMostRecentTreatmentAttributesMessage("01010101", "CN", EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes).PK);
				AssertEquals("Declaration2, 01010101 and CN => Should be message2", message2.PK, declaration2.LoadMostRecentTreatmentAttributesMessage("01010101", "CN", EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes).PK);
				AssertEquals("Declaration2, 01010101 and CN => Should be Message7", message7.PK, declaration2.LoadMostRecentTreatmentAttributesMessage("01010101", "CN", EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes).PK);
				AssertEquals("Declaration2, 02020202 and CN => Should be Message8", message8.PK, declaration2.LoadMostRecentTreatmentAttributesMessage("02020202", "CN", []).PK);
				AssertEquals("Declaration2, 01010101 and AU => Should be Message9", message9.PK, declaration2.LoadMostRecentTreatmentAttributesMessage("01010101", "AU").PK);

				AssertEquals("Only hit table EDIMessage once for each declaration", 2, anotherFactory.TableSelects.Single(x => x.TableName == EDIMessageSchema.Constants.TableName).Value);
			});

			EDIMessage CreateMessage(JobDeclaration declaration, ZString applicationReference, ZString messageType, ZString messageSubType, ZString receiveTrasmit, ZString status, ZDateTime date)
			{
				var message = Factory.New<BREDIMessage>();
				message.EM_ApplicationReference = applicationReference;
				message.EM_MessageType = messageType;
				message.EM_MessageSubType = messageSubType;
				message.EM_ReceiveTransmit = receiveTrasmit;
				message.EM_Status = status;
				message.EM_LinkedObject = declaration;
				message.EM_SystemCreateTimeUtc = date;
				return message;
			}
		}
	}
}
