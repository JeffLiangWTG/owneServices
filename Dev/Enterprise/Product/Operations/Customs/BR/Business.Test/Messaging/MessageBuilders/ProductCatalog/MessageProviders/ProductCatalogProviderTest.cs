using System.Linq;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;

namespace Enterprise.Customs.BR.Business.ProductCatalog.Testing
{
	class ProductCatalogProviderTest : TestCaseWithFactory
	{
		public void TestExportProductCatalog()
		{
			var consignee = Factory.New<OrgHeader>();

			consignee.OH_Code = "XXX";
			consignee.OH_FullName = "TEST COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "58.500.398/-04";

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_AuthorityIdentifier = "2";
			catalog.CGC_Description = "Denomination EXP";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			catalog.CGC_Tariff = "40030000";
			catalog.CGC_OH_Owner = consignee.PK;
			catalog.ComplementaryDescription = "New Description XXX";

			var cusReference1 = catalog.LocalPartNumbers.AddNew();
			cusReference1.CGI_Reference = "1111";

			var cusReference2 = catalog.LocalPartNumbers.AddNew();
			cusReference2.CGI_Reference = "";

			var messageSendingObject = new GoodsCatalogMessageSendingObject(catalog);
			messageSendingObject.Action = ActionList.Codes.Activate;
			var provider = new ProductCatalogProvider(messageSendingObject);

			AssertEquals("Sequence", 1, provider.Sequence);
			AssertEquals("Code", (long)2, provider.AuthorityIdentifier);
			AssertEquals("Description", "New Description XXX", provider.Description);
			AssertEquals("Denomination", "Denomination EXP", provider.Denomination);
			AssertEquals("CpfCnpjRaiz", "58500398", provider.ConsigneeRegistrationNumber);
			AssertEquals("Situation", Constants.Situation.Active, provider.Situation);
			AssertEquals("Modality", "EXPORTACAO", provider.Type);
			AssertEquals("MCM", "40030000", provider.Tariff);
			AssertEquals("Version", null, provider.Version);
			AssertEquals("ReferenceData", null, provider.ReferenceData);
			AssertEquals("InternalCodes", 1, provider.LocalPartNumbers.Count());
			AssertEquals("Attributes", 0, provider.Attributes.Count());
			AssertEquals("CompositeAttributes", 0, provider.CompositeAttributes.Count());

			AssertEquals("LocalPartNumbers 0", "1111", provider.LocalPartNumbers.ElementAt(0));
		}

		public void TestImportProductCatalog()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "XXX";
			consignee.OH_FullName = "TEST COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "75.400.331/-15";

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_AuthorityIdentifier = "3";
			catalog.CGC_Description = "Denomination IMP";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog.CGC_Tariff = "51220000";
			catalog.CGC_OH_Owner = consignee.PK;
			catalog.ComplementaryDescription = "New Description YYY";

			var cusReference1 = catalog.LocalPartNumbers.AddNew();
			cusReference1.CGI_Reference = "1111";

			var cusReference2 = catalog.LocalPartNumbers.AddNew();
			cusReference2.CGI_Reference = "2222";

			var messageSendingObject = new GoodsCatalogMessageSendingObject(catalog);
			var provider = new ProductCatalogProvider(messageSendingObject);

			AssertEquals("Sequence", 1, provider.Sequence);
			AssertEquals("Code", (long)3, provider.AuthorityIdentifier);
			AssertEquals("Description", "New Description YYY", provider.Description);
			AssertEquals("Denomination", "Denomination IMP", provider.Denomination);
			AssertEquals("CpfCnpjRaiz", "75400331", provider.ConsigneeRegistrationNumber);
			AssertEquals("Situation", Constants.Situation.Draft, provider.Situation);
			AssertEquals("Modality", "IMPORTACAO", provider.Type);
			AssertEquals("MCM", "51220000", provider.Tariff);
			AssertEquals("Version", null, provider.Version);
			AssertEquals("ReferenceData", null, provider.ReferenceData);
			AssertEquals("InternalCodes", 2, provider.LocalPartNumbers.Count());
			AssertEquals("Attributes", 0, provider.Attributes.Count());
			AssertEquals("CompositeAttributes", 0, provider.CompositeAttributes.Count());

			AssertEquals("LocalPartNumbers 0", "1111", provider.LocalPartNumbers.ElementAt(0));
			AssertEquals("LocalPartNumbers 1", "2222", provider.LocalPartNumbers.ElementAt(1));
		}

		public void TestAuthorityIdentifier()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_Description = "Description";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog.CGC_Tariff = "02440000";

			var messageSendingObject = new GoodsCatalogMessageSendingObject(catalog);
			var provider = new ProductCatalogProvider(messageSendingObject);

			AssertEquals("Code", null, provider.AuthorityIdentifier);

			catalog.CGC_AuthorityIdentifier = "2";
			AssertEquals("Code", 2L, provider.AuthorityIdentifier);
		}

		public void TestSituation()
		{
			var catalog = Factory.New<CusGoodsCatalog>();

			var messageSendingObject = new GoodsCatalogMessageSendingObject(catalog);
			var dataProvider = new ProductCatalogProvider(messageSendingObject);

			CombineAssertions(() =>
			{
				messageSendingObject.Action = ActionList.Codes.Activate;
				AssertEquals(messageSendingObject.Action, Constants.Situation.Active, dataProvider.Situation);

				messageSendingObject.Action = ActionList.Codes.CreateNewVersion;
				AssertEquals(messageSendingObject.Action, Constants.Situation.Active, dataProvider.Situation);

				messageSendingObject.Action = ActionList.Codes.CreateDraft;
				AssertEquals(messageSendingObject.Action, Constants.Situation.Draft, dataProvider.Situation);

				messageSendingObject.Action = ActionList.Codes.UpdateDraft;
				AssertEquals(messageSendingObject.Action, Constants.Situation.Draft, dataProvider.Situation);

				messageSendingObject.Action = ActionList.Codes.Deactivate;
				AssertEquals(messageSendingObject.Action, Constants.Situation.Deactive, dataProvider.Situation);
			});
		}

		public void TestAttributes()
		{
			var compoundQuestion = Factory.New<RefCusProfileQuestion>();
			compoundQuestion.XQ2_AnswerDataType = AnswerDataTypes.Compound;

			var stringQuestion = Factory.New<RefCusProfileQuestion>();
			stringQuestion.XQ2_AnswerDataType = AnswerDataTypes.String;

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var messageSendingObject = new GoodsCatalogMessageSendingObject(catalog);
			var provider = new ProductCatalogProvider(messageSendingObject);
			AssertEquals("Attributes should be empty", 0, provider.Attributes.Count());

			var att1 = catalog.Attributes.AddNew("ATT_1");
			provider = new ProductCatalogProvider(messageSendingObject);
			AssertEquals("Attributes should be empty", 0, provider.Attributes.Count());

			att1.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			att1.CY_Data = "123";
			provider = new ProductCatalogProvider(messageSendingObject);
			AssertContainsExactElementsInExactOrder("Attributes", new[] { "ATT_1" }, provider.Attributes.Select(x => x.Attribute));

			var att2 = catalog.Attributes.AddNew("ATT_2");
			att2.TariffProfileQuestion = TariffProfileQuestion.New(compoundQuestion);
			att2.CY_Data = "456";

			var att3 = catalog.Attributes.AddNew("ATT_3");
			att3.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			att3.CY_Data = "789";
			provider = new ProductCatalogProvider(messageSendingObject);
			AssertContainsExactElementsInExactOrder("Attributes", new[] { "ATT_1", "ATT_3" }, provider.Attributes.Select(x => x.Attribute));

			stringQuestion.XQ2_StartDate = ZDateTime.Today.AddYears(5);
			att1.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			att3.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);

			provider = new ProductCatalogProvider(messageSendingObject);
			AssertEquals("Attributes should be empty", 0, provider.Attributes.Count());
		}

		public void TestCompoundAttributes()
		{
			var question1 = Factory.New<RefCusProfileQuestion>();
			question1.XQ2_Code = "ATT_1";
			question1.XQ2_AnswerDataType = AnswerDataTypes.Number;

			var question2 = Factory.New<RefCusProfileQuestion>();
			question2.XQ2_Code = "ATT_2";
			question2.XQ2_StartDate = ZDateTime.Today.AddDays(-10);
			question2.XQ2_AnswerDataType = AnswerDataTypes.Compound;

			var question3 = Factory.New<RefCusProfileQuestion>();
			question3.XQ2_Code = "ATT_3";
			question3.XQ2_AnswerDataType = AnswerDataTypes.Number;

			var question4 = Factory.New<RefCusProfileQuestion>();
			question4.XQ2_Code = "ATT_4";
			question4.XQ2_AnswerDataType = AnswerDataTypes.Compound;

			var question21 = Factory.New<RefCusProfileQuestion>();
			question21.XQ2_Code = "ATT_21";
			question21.XQ2_StartDate = ZDateTime.Today.AddDays(-10);
			question21.XQ2_AnswerDataType = AnswerDataTypes.Number;

			var question41 = Factory.New<RefCusProfileQuestion>();
			question41.XQ2_Code = "ATT_41";
			question41.XQ2_AnswerDataType = AnswerDataTypes.Number;

			var question42 = Factory.New<RefCusProfileQuestion>();
			question42.XQ2_Code = "ATT_42";
			question42.XQ2_AnswerDataType = AnswerDataTypes.Number;

			var question43 = Factory.New<RefCusProfileQuestion>();
			question43.XQ2_Code = "ATT_43";
			question43.XQ2_AnswerDataType = AnswerDataTypes.Compound;

			var pathway1 = Factory.New<RefCusProfileQuestionPathway>();
			pathway1.XQP_XQ2_QuestionParent = question2.PK;
			pathway1.XQP_XQ2_QuestionChild = question21.PK;

			var pathway2 = Factory.New<RefCusProfileQuestionPathway>();
			pathway2.XQP_XQ2_QuestionParent = question4.PK;
			pathway2.XQP_XQ2_QuestionChild = question41.PK;

			var pathway3 = Factory.New<RefCusProfileQuestionPathway>();
			pathway3.XQP_XQ2_QuestionParent = question4.PK;
			pathway3.XQP_XQ2_QuestionChild = question42.PK;

			var pathway4 = Factory.New<RefCusProfileQuestionPathway>();
			pathway4.XQP_XQ2_QuestionParent = question4.PK;
			pathway4.XQP_XQ2_QuestionChild = question43.PK;

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var messageSendingObject = new GoodsCatalogMessageSendingObject(catalog);
			var provider = new ProductCatalogProvider(messageSendingObject);
			AssertEquals("CompositeAttributes should be empty", 0, provider.CompositeAttributes.Count());

			var att1 = catalog.Attributes.AddNew("ATT_1");
			att1.TariffProfileQuestion = TariffProfileQuestion.New(question1);
			att1.CY_Data = "123";

			var att2 = catalog.Attributes.AddNew("ATT_2");
			att2.CY_Data = "111";
			att2.TariffProfileQuestion = TariffProfileQuestion.New(question2);

			var att21 = catalog.Attributes.AddNew("ATT_21");
			att21.TariffProfileQuestion = TariffProfileQuestion.New(question21);
			att21.QuestionPathway = pathway1;

			var att3 = catalog.Attributes.AddNew("ATT_3");
			att3.TariffProfileQuestion = TariffProfileQuestion.New(question3);
			att3.CY_Data = "987";

			var att4 = catalog.Attributes.AddNew("ATT_4");
			att4.CY_Data = "222";
			att4.TariffProfileQuestion = TariffProfileQuestion.New(question4);

			var att41 = catalog.Attributes.AddNew("ATT_41");
			att41.TariffProfileQuestion = TariffProfileQuestion.New(question41);
			att41.QuestionPathway = pathway2;

			var att42 = catalog.Attributes.AddNew("ATT_42");
			att42.TariffProfileQuestion = TariffProfileQuestion.New(question42);
			att42.QuestionPathway = pathway3;

			var att43 = catalog.Attributes.AddNew("ATT_43");
			att43.TariffProfileQuestion = TariffProfileQuestion.New(question43);
			att43.QuestionPathway = pathway4;

			provider = new ProductCatalogProvider(messageSendingObject);
			AssertEquals("CompositeAttributes should be empty", 0, provider.CompositeAttributes.Count());

			att21.CY_Data = "789";
			provider = new ProductCatalogProvider(messageSendingObject);
			CombineAssertions(() =>
			{
				AssertEquals("CompositeAttributes", 1, provider.CompositeAttributes.Count());
				AssertCompositeAttribute(provider.CompositeAttributes.ElementAt(0), "ATT_2", new[] { "ATT_21" });
			});

			att41.CY_Data = "321";
			att42.CY_Data = "000";
			att43.CY_Data = "456";
			provider = new ProductCatalogProvider(messageSendingObject);
			CombineAssertions(() =>
			{
				AssertEquals("CompositeAttributes", 2, provider.CompositeAttributes.Count());
				AssertCompositeAttribute(provider.CompositeAttributes.ElementAt(0), "ATT_2", new[] { "ATT_21" });
				AssertCompositeAttribute(provider.CompositeAttributes.ElementAt(1), "ATT_4", new[] { "ATT_41", "ATT_42", "ATT_43" });
			});

			question43.XQ2_StartDate = ZDateTime.Today.AddDays(10);
			att43.TariffProfileQuestion = TariffProfileQuestion.New(question43);
			provider = new ProductCatalogProvider(messageSendingObject);
			CombineAssertions(() =>
			{
				AssertEquals("CompositeAttributes", 2, provider.CompositeAttributes.Count());
				AssertCompositeAttribute(provider.CompositeAttributes.ElementAt(0), "ATT_2", new[] { "ATT_21" });
				AssertCompositeAttribute(provider.CompositeAttributes.ElementAt(1), "ATT_4", new[] { "ATT_41", "ATT_42" });
			});

			void AssertCompositeAttribute(ICompositeAttribute attribute, string expectedAttribute, string[] expectedChildAttribues)
			{
				AssertEquals("Attribute", expectedAttribute, attribute.Attribute);
				AssertContainsExactElementsInExactOrder("Values", expectedChildAttribues, attribute.Values.Select(s => s.Attribute));
			}
		}

		public void TestMultivaluedAttributes()
		{
			var profileQuestion = Factory.New<RefCusProfileQuestion>();
			profileQuestion.XQ2_AnswerDataType = AnswerDataTypes.String;
			profileQuestion.XQ2_AllowMultipleAnswers = true;

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var messageSendingObject = new GoodsCatalogMessageSendingObject(catalog);

			var att1 = catalog.Attributes.AddNew("ATT_1");
			att1.CY_Data = "";
			var provider = new ProductCatalogProvider(messageSendingObject);
			AssertEquals("LinkedAttributes should be empty", 0, provider.MultivaluedAttributes.Count());

			att1.CY_Data = "123";
			provider = new ProductCatalogProvider(messageSendingObject);
			AssertEquals("LinkedAttributes should be empty", 0, provider.MultivaluedAttributes.Count());

			var att2 = catalog.Attributes.AddNew("ATT_2");
			att2.TariffProfileQuestion = TariffProfileQuestion.New(profileQuestion);
			att2.Content = "456" + System.Environment.NewLine + "789";

			var att3 = catalog.Attributes.AddNew("ATT_3");
			att3.TariffProfileQuestion = TariffProfileQuestion.New(profileQuestion);
			att3.Content = "777" + System.Environment.NewLine + "888";
			provider = new ProductCatalogProvider(messageSendingObject);
			CombineAssertions(() =>
			{
				AssertEquals("MultivaluedAttributes count", 2, provider.MultivaluedAttributes.Count());
				AssertContainsExactElementsInExactOrder("MultivaluedAttributes.Attribute", new[] { "ATT_2", "ATT_3" }, provider.MultivaluedAttributes.Select(x => x.Attribute));
				AssertContainsExactElementsInExactOrder("MultivaluedAttributes[0].Values", new[] { "456", "789" }, provider.MultivaluedAttributes.ToArray()[0].Values);
				AssertContainsExactElementsInExactOrder("MultivaluedAttributes[1].Values", new[] { "777", "888" }, provider.MultivaluedAttributes.ToArray()[1].Values);
			});
		}
	}
}
