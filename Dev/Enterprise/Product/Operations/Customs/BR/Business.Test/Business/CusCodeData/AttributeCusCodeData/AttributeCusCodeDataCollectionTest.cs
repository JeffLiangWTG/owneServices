using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AttributeCusCodeDataCollection))]
	public class AttributeCusCodeDataCollectionTest : CusCodeDataCollectionTest<AttributeCusCodeData>
	{
		public void TestRebuild_OnInvoiceLine_FromZZData()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateLPCTTariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);
			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals("Attribute should be 0", 0, InvoiceLine.Attributes.Count);

			InvoiceLine.JI_Tariff = "99999999";
			AssertEquals(2, InvoiceLine.Attributes.Count);
			var attrCusCodeData01 = AssertAttributeCusCodeData(0, "ATT_2558", InvoiceLine);
			var attrCusCodeData02 = AssertAttributeCusCodeData(1, "ATT_2559", InvoiceLine);

			InvoiceLine.JI_Tariff = "56049000";
			AssertEquals(1, InvoiceLine.Attributes.Count);
			var attrCusCodeData13 = AssertAttributeCusCodeData(0, "ATT_2557", InvoiceLine);

			Assert("AttributeCusCodeData ATT_2558 was deleted", attrCusCodeData01.IsDeleted);
			Assert("AttributeCusCodeData ATT_2559 was deleted", attrCusCodeData02.IsDeleted);
			Assert("AttributeCusCodeData ATT_2557 was not deleted", !attrCusCodeData13.IsDeleted);

			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals("Attribute should be 0", 0, InvoiceLine.Attributes.Count);
			Assert("AttributeCusCodeData ATT3 was deleted", attrCusCodeData13.IsDeleted);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = "99999999";
			AssertEquals(2, InvoiceLine.Attributes.Count);
			var attrCusCodeData58 = AssertAttributeCusCodeData(0, "ATT_2558", InvoiceLine);
			var attrCusCodeData59 = AssertAttributeCusCodeData(1, "ATT_2559", InvoiceLine);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			InvoiceLine.JI_Tariff = "00000000";
			AssertEquals("Attribute should be 0", 0, InvoiceLine.Attributes.Count);
			Assert("AttributeCusCodeData ATT_2558 was deleted", attrCusCodeData58.IsDeleted);
			Assert("AttributeCusCodeData ATT_2559 was deleted", attrCusCodeData59.IsDeleted);

			InvoiceLine.JI_Tariff = "99999999";
			AssertEquals(2, InvoiceLine.Attributes.Count);
			AssertAttributeCusCodeData(0, "ATT_4508", InvoiceLine);
			AssertAttributeCusCodeData(1, "ATT_4509", InvoiceLine);

			InvoiceLine.JI_Tariff = "56049000";
			AssertEquals(1, InvoiceLine.Attributes.Count);
			AssertAttributeCusCodeData(0, "ATT_4507", InvoiceLine);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals(0, InvoiceLine.TaxRegimeAttributes.Count);
				InvoiceLine.JI_Tariff = "12345678";
				AssertEquals(0, InvoiceLine.TaxRegimeAttributes.Count);
				InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				AssertEquals(2, InvoiceLine.TaxRegimeAttributes.Count);
				AssertAttributeCusCodeData(0, "ATT1", InvoiceLine, "DTY", "P01", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(1, "ATT1_1", InvoiceLine, "DTY", "P01", isInvoiceLineTaxRegime: true);

				InvoiceLine.DuimpLegalBase = "P02";
				InvoiceLine.AddDuimpTaxRegimes();
				AssertEquals(8, InvoiceLine.TaxRegimeAttributes.Count);
				AssertAttributeCusCodeData(0, "ATT1", InvoiceLine, "DTY, COF", "P01, P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(1, "ATT1_1", InvoiceLine, "DTY, COF", "P01, P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(2, "ATT12", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(3, "ATT12_1", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(4, "ATT13", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(5, "ATT13_1", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(6, "ATT2", InvoiceLine, "COF", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(7, "ATT3", InvoiceLine, "COF", "P02", isInvoiceLineTaxRegime: true);

				InvoiceLine.DuimpLegalBase = "P05";
				InvoiceLine.AddDuimpTaxRegimes();
				AssertEquals(8, InvoiceLine.TaxRegimeAttributes.Count);
				AssertAttributeCusCodeData(0, "ATT1", InvoiceLine, "DTY, COF", "P01, P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(1, "ATT1_1", InvoiceLine, "DTY, COF", "P01, P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(2, "ATT12", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(3, "ATT12_1", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(4, "ATT13", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(5, "ATT13_1", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(6, "ATT2", InvoiceLine, "COF", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(7, "ATT3", InvoiceLine, "COF", "P02", isInvoiceLineTaxRegime: true);

				InvoiceLine.DuimpLegalBase = "P06";
				InvoiceLine.AddDuimpTaxRegimes();
				AssertEquals(14, InvoiceLine.TaxRegimeAttributes.Count);
				AssertAttributeCusCodeData(0, "ATT1", InvoiceLine, "DTY, COF", "P01, P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(1, "ATT1_1", InvoiceLine, "DTY, COF", "P01, P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(2, "ATT12", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(3, "ATT12_1", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(4, "ATT13", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(5, "ATT13_1", InvoiceLine, "PIS", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(6, "ATT2", InvoiceLine, "COF", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(7, "ATT3", InvoiceLine, "COF", "P02", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(8, "ATT10", InvoiceLine, "PIS", "P06", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(9, "ATT10_1", InvoiceLine, "PIS", "P06", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(10, "ATT10_2", InvoiceLine, "PIS", "P06", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(11, "ATT11", InvoiceLine, "PIS", "P06", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(12, "ATT11_1", InvoiceLine, "PIS", "P06", isInvoiceLineTaxRegime: true);
				AssertAttributeCusCodeData(13, "ATT11_11", InvoiceLine, "PIS", "P06", isInvoiceLineTaxRegime: true);

				InvoiceLine.JI_Tariff = "99999999";
				AssertEquals("Attributes should be 0", 0, InvoiceLine.Attributes.Count);

				InvoiceLine.JI_Tariff = "56049000";
				AssertEquals("Attributes should be 0", 0, InvoiceLine.Attributes.Count);

				InvoiceLine.JI_Tariff = "87654321";
				AssertEquals(8, InvoiceLine.Attributes.Count);
				AssertAttributeCusCodeData(0, "ATT_4801", InvoiceLine);
				AssertAttributeCusCodeData(1, "ATT_4802", InvoiceLine);
				AssertAttributeCusCodeData(2, "ATT_4803", InvoiceLine);
				AssertAttributeCusCodeData(3, "ATT_48031", InvoiceLine);
				AssertAttributeCusCodeData(4, "ATT_48032", InvoiceLine);
				AssertAttributeCusCodeData(5, "ATT_4804", InvoiceLine);
				AssertAttributeCusCodeData(6, "ATT_48041", InvoiceLine);
				AssertAttributeCusCodeData(7, "ATT_48042", InvoiceLine);
			}
		}
		public void TestRebuild_OnInvoiceLine_FromMessage()
		{
			var date = ZDateTime.Now;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, tariffType.PK, "01010101", date.AddDays(-5), date.AddDays(5));

			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationReference = $"01010101|CN|{date:yyyyMMdd}";
			message.EM_MessageNum = "1";
			message.EM_MessageType = MessageTypeList.Codes.RTT;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json").Replace("\"dataFatoGerador\": \"2023-04-17\"", $"\"dataFatoGerador\": \"{date.ToISO8601ShortDateString()}\"");
			message.EM_LinkedObject = Declaration;
			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("Attribute should be 0", 0, InvoiceLine.Attributes.Count);

			InvoiceLine.JI_Tariff = "00000000";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals("Attribute should be 0", 0, InvoiceLine.Attributes.Count);

			InvoiceLine.JI_Tariff = "01010101";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("Attribute should be 0", 0, InvoiceLine.Attributes.Count);

			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(2, InvoiceLine.TaxRegimeAttributes.Count);
			AssertAttributeCusCodeData(0, "ATT_13715", InvoiceLine, legalCode: "1100, 1100", taxType: "PIS, COF", isInvoiceLineTaxRegime: true);
			AssertAttributeCusCodeData(1, "ATT_13741", InvoiceLine, legalCode: "1100, 1100", taxType: "PIS, COF", isInvoiceLineTaxRegime: true);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = "01010101";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(0, InvoiceLine.TaxRegimeAttributes.Count);
		}

		public void TestRebuild_OnCusClassPartPivot()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;

			AssertType<CusClassPartPivot>(pivot.Attributes.Master);

			pivot.CI_TariffNum = "00000000";
			AssertEquals("Attribute should be 0", 0, pivot.Attributes.Count);

			pivot.CI_TariffNum = "99999999";
			AssertEquals(2, pivot.Attributes.Count);
			var attributeCusCodeData01 = AssertAttributeCusCodeData(0, "ATT_2558", pivot);
			var attributeCusCodeData02 = AssertAttributeCusCodeData(1, "ATT_2559", pivot);

			pivot.CI_TariffNum = "56049000";
			AssertEquals(1, pivot.Attributes.Count);
			AssertNull(pivot.Attributes.GetFirstElementHaving("ATT_2559"));
			var attributeCusCodeData12 = AssertAttributeCusCodeData(0, "ATT_2557", pivot);

			Assert("AttributeCusCodeData ATT_2558 was deleted", attributeCusCodeData01.IsDeleted);
			Assert("AttributeCusCodeData ATT_2559 was deleted", attributeCusCodeData02.IsDeleted);
			Assert("AttributeCusCodeData ATT_2557 was not deleted", !attributeCusCodeData12.IsDeleted);

			pivot.CI_TariffNum = "00000000";
			AssertEquals("Attribute should be 0", 0, pivot.Attributes.Count);
			Assert("AttributeCusCodeData AA was deleted", attributeCusCodeData12.IsDeleted);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "99999999";
			AssertEquals("Attribute should be 0", 1, pivot.Attributes.Count);
			var attributeCusCodeData60 = AssertAttributeCusCodeData(0, "ATT_2560", pivot);

			pivot.CI_TariffNum = "00000000";
			AssertEquals("Attribute should be 0", 0, pivot.Attributes.Count);
			Assert("AttributeCusCodeData ATT_2560 was deleted", attributeCusCodeData60.IsDeleted);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_TariffNum = "99999999";
			AssertEquals(3, pivot.Attributes.Count);
			attributeCusCodeData01 = AssertAttributeCusCodeData(0, "ATT_2558", pivot);
			attributeCusCodeData02 = AssertAttributeCusCodeData(1, "ATT_2559", pivot);
			attributeCusCodeData60 = AssertAttributeCusCodeData(2, "ATT_2560", pivot);

			pivot.CI_TariffNum = "00000000";
			AssertEquals("Attribute should be 0", 0, pivot.Attributes.Count);
			Assert("AttributeCusCodeData ATT_2558 was deleted", attributeCusCodeData01.IsDeleted);
			Assert("AttributeCusCodeData ATT_2559 was deleted", attributeCusCodeData02.IsDeleted);
			Assert("AttributeCusCodeData ATT_2560 was deleted", attributeCusCodeData60.IsDeleted);
		}

		public void TestRebuild_OnCusGoodsCatalog()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);

			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			cusGoodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			cusGoodsCatalog.CGC_Tariff = "00000000";
			AssertEquals("Attribute should be 0", 0, cusGoodsCatalog.Attributes.Count);

			cusGoodsCatalog.CGC_Tariff = "87654321";
			AssertEquals(3, cusGoodsCatalog.Attributes.Count);
			AssertAttributeCusCodeData(0, "ATT_2557", cusGoodsCatalog);
			AssertAttributeCusCodeData(1, "ATT_3892", cusGoodsCatalog);
			AssertAttributeCusCodeData(2, "ATT_4807", cusGoodsCatalog);

			cusGoodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			AssertEquals(4, cusGoodsCatalog.Attributes.Count);
			AssertAttributeCusCodeData(0, "ATT_3887", cusGoodsCatalog);
			AssertAttributeCusCodeData(1, "ATT_3890", cusGoodsCatalog);
			AssertAttributeCusCodeData(2, "ATT_38901", cusGoodsCatalog);
			AssertAttributeCusCodeData(3, "ATT_38902", cusGoodsCatalog);
		}

		AttributeCusCodeData AssertAttributeCusCodeData(int index, string code, BusinessObject businessObject, string taxType = null, string legalCode = null, bool isInvoiceLineTaxRegime = false)
		{
			AttributeCusCodeData attributeCusCodeData = null;
			if (businessObject is JobComInvoiceLine invoiceLine)
			{
				attributeCusCodeData = isInvoiceLineTaxRegime ? invoiceLine.TaxRegimeAttributes[index] : invoiceLine.Attributes[index];
			}
			else if (businessObject is CusClassPartPivot pivot)
			{
				attributeCusCodeData = pivot.Attributes[index];
			}
			else if (businessObject is CusGoodsCatalog catalog)
			{
				attributeCusCodeData = catalog.Attributes[index];
			}
			CombineAssertions(() =>
			{
				AssertEquals($"AttributeCusCodeData created for {code}", code, attributeCusCodeData.CY_Code);
				AssertEquals("TaxType", taxType ?? string.Empty, attributeCusCodeData.TaxType);
				AssertEquals("LegalBase", legalCode ?? string.Empty, attributeCusCodeData.LegalBase);
			});
			return attributeCusCodeData;
		}

		public void TestAllowNewCore()
		{
			var attribute = GetCusCodeDataCollection();
			Assert(!attribute.AllowNew);
		}

		public void TestAllowRemoveCore()
		{
			var attribute = GetCusCodeDataCollection();
			Assert(!attribute.AllowRemove);
		}

		public void TestAddNewOrUpdateValue()
		{
			var collection = new AttributeCusCodeDataCollection(Factory.New<CusGoodsCatalog>());
			collection.AddNewOrUpdateExistingAttribute("Test", "01");

			AssertEquals("collection should be 1", 1, collection.Count);
			AssertEquals("collection CY_Code should be Test", "Test", collection[0].CY_Code);
			AssertEquals("collection CY_Data should be 01", "01", collection[0].CY_Data);

			collection.AddNewOrUpdateExistingAttribute("Test", "02");
			AssertEquals("collection should be 1", 1, collection.Count);
			AssertEquals("collection CY_Data should be 02", "02", collection[0].CY_Data);

			collection.AddNewOrUpdateExistingAttribute("Test2", "03");
			AssertEquals("collection should be 2", 2, collection.Count);
			AssertEquals("collection CY_Code should be Test", "Test", collection[0].CY_Code);
			AssertEquals("collection CY_Data should be 02", "02", collection[0].CY_Data);
			AssertEquals("collection CY_Code should be Test2", "Test2", collection[1].CY_Code);
			AssertEquals("collection CY_Data should be 03", "03", collection[1].CY_Data);
		}

		public void TestRelationshipFilter()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			var attribute1 = catalog.Attributes.AddNew();
			var attribute2 = catalog.Attributes.AddNew();
			var attribute3 = catalog.Attributes.AddNew();
			attribute3.CY_Order = 1;

			var collection = new AttributeCusCodeDataCollection(catalog);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { attribute1, attribute2 }, collection);
			AssertCollectionNotContains(attribute3, collection);
		}

		public void TestAddFetchHintBeforeLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var profileType = helper.CreateRefCusProfileType(Constants.Profile.Types.NCM, "HSN", Core.Constants.CountryCodes.Brazil);
			var stringQuestion = Factory.New<RefCusProfileQuestion>();
			stringQuestion.XQ2_Code = "ATT_1";
			stringQuestion.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			stringQuestion.XQ2_AllowMultipleAnswers = true;
			stringQuestion.XQ2_Name = "TEST";
			stringQuestion.XQ2_Text = "TEST";
			stringQuestion.XQ2_StartDate = DateTime.Now;
			stringQuestion.XQ2_EndDate = DateTime.Now.AddDays(100);
			stringQuestion.XQ2_XXX_ProfileType = profileType.PK;
			stringQuestion.XQ2_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Brazil;
			Factory.Save();

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var attribute1 = catalog.Attributes.AddNew();
			attribute1.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			attribute1.CY_Code = "ATT_1";
			attribute1.MultivaluedAttributesLinked.UpdateAll(new ZString[] { "a", "b", "c", "d" });
			var attribute2 = catalog.Attributes.AddNew();
			attribute2.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			attribute2.CY_Code = "ATT_2";
			attribute2.MultivaluedAttributesLinked.UpdateAll(new ZString[] { "d", "e", "f" } );
			var attribute3 = catalog.Attributes.AddNew();
			attribute3.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			attribute3.CY_Code = "ATT_3";
			attribute3.MultivaluedAttributesLinked.UpdateAll(new ZString[] { "g", "h" });
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var catalogInNewFactory = newFactory.Load<CusGoodsCatalog>(catalog.PK);
			var collection = new AttributeCusCodeDataCollection(catalogInNewFactory);
			collection.Load();
			collection.Sort(CusCodeData.Schema.CY_Code);
			collection[0].TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			collection[1].TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			collection[2].TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			var multivaluedAttributes1 = collection[0].MultivaluedAttributesLinked;
			var multivaluedAttributes2 = collection[1].MultivaluedAttributesLinked;
			var multivaluedAttributes3 = collection[2].MultivaluedAttributesLinked;
			AssertEquals("collection loaded", 3, collection.Count);
			AssertEquals("multivaluedAttributes1 loaded", 4, multivaluedAttributes1.Count);
			AssertEquals("multivaluedAttributes2 loaded", 3, multivaluedAttributes2.Count);
			AssertEquals("multivaluedAttributes3 loaded", 2, multivaluedAttributes3.Count);

			var tableSelects = newFactory.TableSelects;
			AssertEquals("Should have hit the DB once during attributes load.", 1, tableSelects.Single(t => t.TableName == CusCodeDataSchema.Constants.TableName).Value);
		}

		protected override CusCodeDataCollection<AttributeCusCodeData> GetCusCodeDataCollection()
		{
			return new AttributeCusCodeDataCollection(InvoiceLine, CusCodeDataTypeList.Codes.Attribute);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<AttributeCusCodeData>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				}

				return declaration;
			}
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var invoice = Declaration.Invoices.AddNew();
					invoice.JZ_InvoiceNumber = "1111";
					invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "55555555";
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
	}
}
