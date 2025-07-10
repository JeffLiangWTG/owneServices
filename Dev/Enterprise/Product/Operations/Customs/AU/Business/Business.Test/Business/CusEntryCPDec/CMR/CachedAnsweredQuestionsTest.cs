using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CachedAnsweredQuestionsTest : TestCaseWithFactory
	{
		public void TestCachingAnswersAndPermit()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000011";
			invoiceLine2.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			AssertEquals(1, invoiceLine2.CusEntryLine.Questions.Count);

			invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode = "Y";
			invoiceLine1.CusEntryLine.Questions[0].ON_Permit = "1213";

			invoiceLine2.CusEntryLine.Questions[0].ON_AnswerCode = "N";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Y", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
			AssertEquals("1213", invoiceLine1.CusEntryLine.Questions[0].ON_Permit);

			AssertEquals("N", invoiceLine2.CusEntryLine.Questions[0].ON_AnswerCode);
		}

		public void TestCachingAnswersAndPermitCaseInsensitive()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000011";
			invoiceLine2.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			AssertEquals(1, invoiceLine2.CusEntryLine.Questions.Count);

			invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode = "Y";
			invoiceLine1.CusEntryLine.Questions[0].ON_Permit = "121a";

			invoiceLine2.CusEntryLine.Questions[0].ON_AnswerCode = "Y";
			invoiceLine2.CusEntryLine.Questions[0].ON_Permit = "121A";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals("Y", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
			AssertEquals("121A", invoiceLine1.CusEntryLine.Questions[0].ON_Permit.ToUpper());
		}

		public void TestPermitMaxLength()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);

			invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode = "Y";
			invoiceLine1.CusEntryLine.Questions[0].ON_Permit = "LongPermitNumberIsBiggerThan16Chars";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Y", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
			AssertEquals("LONGPERMITNUMBERISBIGGERTHAN16CHARS", invoiceLine1.CusEntryLine.Questions[0].ON_Permit.ToUpper());
		}

		public void TestCachingAnswersFromNONToTariff()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000011";
			invoiceLine2.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			AssertEquals(1, invoiceLine2.CusEntryLine.Questions.Count);

			invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode = "Y";
			invoiceLine2.CusEntryLine.Questions[0].ON_AnswerCode = "Y";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals("Y", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
		}

		public void TestWhenABrandNewInvoiceLineIsEntered()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode = "Y";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000011";
			invoiceLine2.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);

			AssertEquals("Answer is not assumed from previous answers as previous answer for a new invoice line is not known", "", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
		}
	}
}
