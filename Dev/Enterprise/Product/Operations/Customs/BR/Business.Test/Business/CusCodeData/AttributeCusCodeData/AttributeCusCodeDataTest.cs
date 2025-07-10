using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AttributeCusCodeData))]
	class AttributeCusCodeDataTest : Customs.Business.Testing.CusCodeDataTest<AttributeCusCodeData>
	{
		public void TestSupportsNotes()
		{
			var attribute = Factory.New<AttributeCusCodeData>();
			Assert("SupportsNotes should be false", !attribute.SupportsNotes);
		}

		public void TestAttributesForExp()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			Factory.Save();

			var valuesList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("01","FOR USE IN AGRICULTURE"),
				new KeyValuePair<string, string>("99","OTHER USES, EXCEPT AGRICULTURAL"),
			};

			var attributesList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Caption","Oriatention01"),
				new KeyValuePair<string, string>("Exemplo","Example01"),
				new KeyValuePair<string, string>("Test","Oriatention02"),
			};

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_2557", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTimeValue, values: valuesList, attributes: attributesList);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3061", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTimeValue);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3887", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTimeValue);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3890", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTimeValue);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3891", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTimeValue).ZB1_DecimalPlaces = 5;

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";

			var attList = invoiceLine.Attributes.GetFirstElementHaving("ATT_2557");
			AssertAttribute(attList, "ATT_2557", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, nameof(FieldType.TextDropEdit), "Oriatention01", example: "Example01");

			var attText = invoiceLine.Attributes.GetFirstElementHaving("ATT_3061");
			AssertAttribute(attText, "ATT_3061", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), ZString.Empty);

			var attNumberInteger = invoiceLine.Attributes.GetFirstElementHaving("ATT_3887");
			AssertAttribute(attNumberInteger, "ATT_3887", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), ZString.Empty);

			var attBoolean = invoiceLine.Attributes.GetFirstElementHaving("ATT_3890");
			AssertAttribute(attBoolean, "ATT_3890", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), ZString.Empty);

			var attNumberReal = invoiceLine.Attributes.GetFirstElementHaving("ATT_3891");
			AssertAttribute(attNumberReal, "ATT_3891", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), ZString.Empty, decimalPlaces: 5);
		}

		public void TestAttributesForImpWithRefCusProfile()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "1111";
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				invoiceLine.DuimpLegalBase = "P02";
				invoiceLine.AddDuimpTaxRegimes();
				invoiceLine.DuimpLegalBase = "P04";
				invoiceLine.AddDuimpTaxRegimes();
				invoiceLine.DuimpLegalBase = "P06";
				invoiceLine.AddDuimpTaxRegimes();

				var att1 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT1");
				AssertAttribute(att1, "ATT1", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention01", taxType: "DTY, COF", legalBase: "P01, P02", childAtt: new[] { "ATT1_1" });

				var att1_1 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT1_1");
				AssertAttribute(att1_1, "ATT1_1", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention01", taxType: "DTY, COF", legalBase: "P01, P02", condition: "ATT1|ATT1_1", parentAttCode: "ATT1");

				var att2 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT2");
				AssertAttribute(att2, "ATT2", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention01", taxType: "COF, PIS", legalBase: "P02, P04");

				var att3 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT3");
				AssertAttribute(att3, "ATT3", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, nameof(FieldType.TextDropEdit), "Oriatention02", taxType: "COF", legalBase: "P02");

				var att10 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT10");
				AssertAttribute(att10, "ATT10", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), "Oriatention04", taxType: "PIS", legalBase: "P06", childAtt: new[] { "ATT10_1", "ATT10_2" });

				var att10_1 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT10_1");
				AssertAttribute(att10_1, "ATT10_1", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention04", taxType: "PIS", legalBase: "P06", condition: "ATT10|ATT10_1", parentAttCode: "ATT10");

				var att10_2 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT10_2");
				AssertAttribute(att10_2, "ATT10_2", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention04", taxType: "PIS", legalBase: "P06", condition: "ATT10|ATT10_2", parentAttCode: "ATT10");

				var att11 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT11");
				AssertAttribute(att11, "ATT11", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention04", taxType: "PIS", legalBase: "P06", childAtt: new[] { "ATT11_1" });

				var att11_1 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT11_1");
				AssertAttribute(att11_1, "ATT11_1", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention04", taxType: "PIS", legalBase: "P06",  condition: "ATT11|ATT11_1", parentAttCode: "ATT11", childAtt: new[] { "ATT11_11" });

				var att11_11 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT11_11");
				AssertAttribute(att11_11, "ATT11_11", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention04", taxType: "PIS", legalBase: "P06", condition: "ATT11_1|ATT11_11", parentAttCode: "ATT11_1");

				invoiceLine.JI_Tariff = "87654321";

				AssertEquals("Attributes count should be 8", 8, invoiceLine.Attributes.Count);

				var att4801 = invoiceLine.Attributes.GetFirstElementHaving("ATT_4801");
				AssertAttribute(att4801, "ATT_4801", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), "Oriatention02");

				var att4802 = invoiceLine.Attributes.GetFirstElementHaving("ATT_4802");
				AssertAttribute(att4802, "ATT_4802", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, nameof(FieldType.TextDropEdit), "Oriatention02");

				var att4803 = invoiceLine.Attributes.GetFirstElementHaving("ATT_4803");
				AssertAttribute(att4803, "ATT_4803", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention02", childAtt: new[] { "ATT_48031", "ATT_48032" });

				var att48031 = invoiceLine.Attributes.GetFirstElementHaving("ATT_48031");
				AssertAttribute(att48031, "ATT_48031", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), "Oriatention02", readOnly: true, condition: "ATT_4803|ATT_48031", parentAttCode: "ATT_4803");

				var att48032 = invoiceLine.Attributes.GetFirstElementHaving("ATT_48032");
				AssertAttribute(att48032, "ATT_48032", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), "Oriatention02", readOnly: true, condition: "ATT_4803|ATT_48032", parentAttCode: "ATT_4803");

				var att4804 = invoiceLine.Attributes.GetFirstElementHaving("ATT_4804");
				AssertAttribute(att4804, "ATT_4804", Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound, nameof(FieldType.Text), "Oriatention02", readOnly: true, childAtt: new[] { "ATT_48041", "ATT_48042" });

				var att48041 = invoiceLine.Attributes.GetFirstElementHaving("ATT_48041");
				AssertAttribute(att48041, "ATT_48041", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention02", condition: "ATT_4804|ATT_48041", parentAttCode: "ATT_4804", isParentCompoundAtt: true);

				var att48042 = invoiceLine.Attributes.GetFirstElementHaving("ATT_48042");
				AssertAttribute(att48042, "ATT_48042", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), "Oriatention02", condition: "ATT_4804|ATT_48042", parentAttCode: "ATT_4804", isParentCompoundAtt: true);

				var attWithoutProfile = invoiceLine.Attributes.AddNew();
				AssertEquals("attWithoutProfile CY_DataFieldType", nameof(FieldType.Text), attWithoutProfile.CY_DataFieldType);
			}
		}

		public void TestAttributesForImp_FromMessage()
		{
			var date = ZDateTime.Now;

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Brazil, hsnTariffType.PK, "01010101", date.AddDays(-5), date.AddDays(5));

			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_ApplicationReference = $"01010101|CN|{date:yyyyMMdd}";
			message.EM_MessageNum = "1";
			message.EM_MessageType = MessageTypeList.Codes.RTT;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource("ResponseMessageOTA.json").Replace("\"dataFatoGerador\": \"2023-04-17\"", $"\"dataFatoGerador\": \"{date.ToISO8601ShortDateString()}\"")
				.Replace("ATT_15574", "ATT_13743");

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

			AssertEquals("TaxRegimeAttributes should contain", 3, invoiceLine.TaxRegimeAttributes.Count);
			var att13715 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13715");
			var att13741 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13741");
			var att13743 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13743");
			CombineAssertions(() =>
			{
				AssertEquals("att13715 CY_Code", "ATT_13715", att13715.CY_Code);
				AssertEquals("att13715 ParentAttributeCode", ZString.Empty, att13715.ParentAttributeCode);
				AssertEquals("att13715 ConditionDescription", ZString.Empty, att13715.ConditionDescription);
				AssertEquals("att13715 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13715.CY_DataFieldType);
				AssertEquals("att13715 FillOrientation", ZString.Empty, att13715.FillOrientation);
				AssertEquals("att13715 Example", ZString.Empty, att13715.Example);
				AssertEquals("att13715 Content should NOT be ReadOnly", false, att13715.ContentInfo.ReadOnly);
				AssertEquals("att13715 CY_DataDecimalPlaces should be 0", (ZShort)0, att13715.CY_DataDecimalPlaces);
				AssertEquals("att13715 IsMandatory should be true", true, att13715.IsMandatory);
				AssertEquals("att13715 TaxType should NOT be Empty", "PIS, COF", att13715.TaxType);
				AssertEquals("att13715 LegalBase should NOT be Empty", "1100, 1100", att13715.LegalBase);

				AssertEquals("att13741 CY_Code", "ATT_13741", att13741.CY_Code);
				AssertEquals("att13741 ParentAttributeCode", ZString.Empty, att13741.ParentAttributeCode);
				AssertEquals("att13741 ConditionDescription", ZString.Empty, att13741.ConditionDescription);
				AssertEquals("att13741 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13741.CY_DataFieldType);
				AssertEquals("att13741 FillOrientation", ZString.Empty, att13741.FillOrientation);
				AssertEquals("att13741 Example", ZString.Empty, att13741.Example);
				AssertEquals("att13741 Content should NOT be ReadOnly", false, att13741.ContentInfo.ReadOnly);
				AssertEquals("att13741 CY_DataDecimalPlaces should be 0", (ZShort)0, att13741.CY_DataDecimalPlaces);
				AssertEquals("att13741 IsMandatory should be true", true, att13741.IsMandatory);
				AssertEquals("att13741 TaxType should NOT be Empty", "PIS, COF", att13741.TaxType);
				AssertEquals("att13741 LegalBase should NOT be Empty", "1100, 1100", att13741.LegalBase);

				AssertEquals("att13743 CY_Code", "ATT_13743", att13743.CY_Code);
				AssertEquals("att13743 ParentAttributeCode", ZString.Empty, att13743.ParentAttributeCode);
				AssertEquals("att13743 ConditionDescription", ZString.Empty, att13743.ConditionDescription);
				AssertEquals("att13743 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13743.CY_DataFieldType);
				AssertEquals("att13743 FillOrientation", ZString.Empty, att13743.FillOrientation);
				AssertEquals("att13743 Example", ZString.Empty, att13743.Example);
				AssertEquals("att13743 Content should NOT be ReadOnly", false, att13743.ContentInfo.ReadOnly);
				AssertEquals("att13743 CY_DataDecimalPlaces should be 0", (ZShort)0, att13743.CY_DataDecimalPlaces);
				AssertEquals("att13743 IsMandatory should be true", true, att13743.IsMandatory);
				AssertEquals("att13743 TaxType should NOT be Empty", "DTY", att13743.TaxType);
				AssertEquals("att13743 LegalBase should NOT be Empty", "0006", att13743.LegalBase);
			});

			invoiceLine.DuimpLegalBase = "0007";
			invoiceLine.AddDuimpTaxRegimes();
			invoiceLine.DuimpLegalBase = "0008";
			invoiceLine.AddDuimpTaxRegimes();
			invoiceLine.DuimpLegalBase = "0009";
			invoiceLine.AddDuimpTaxRegimes();

			AssertEquals("TaxRegimeAttributes should contain", 5, invoiceLine.TaxRegimeAttributes.Count);
			att13715 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13715");
			att13741 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13741");
			att13743 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_13743");
			var att2872 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_2872");
			var att2874 = invoiceLine.TaxRegimeAttributes.GetFirstElementHaving("ATT_2874");
			CombineAssertions(() =>
			{
				AssertEquals("att13715 CY_Code", "ATT_13715", att13715.CY_Code);
				AssertEquals("att13715 ParentAttributeCode", ZString.Empty, att13715.ParentAttributeCode);
				AssertEquals("att13715 ConditionDescription", ZString.Empty, att13715.ConditionDescription);
				AssertEquals("att13715 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13715.CY_DataFieldType);
				AssertEquals("att13715 FillOrientation", ZString.Empty, att13715.FillOrientation);
				AssertEquals("att13715 Example", ZString.Empty, att13715.Example);
				AssertEquals("att13715 Content should NOT be ReadOnly", false, att13715.ContentInfo.ReadOnly);
				AssertEquals("att13715 CY_DataDecimalPlaces should be 0", (ZShort)0, att13715.CY_DataDecimalPlaces);
				AssertEquals("att13715 IsMandatory should be true", true, att13715.IsMandatory);
				AssertEquals("att13715 TaxType should NOT be Empty", "PIS, COF, IPI", att13715.TaxType);
				AssertEquals("att13715 LegalBase should NOT be Empty", "1100, 1100, 0008", att13715.LegalBase);

				AssertEquals("att13741 CY_Code", "ATT_13741", att13741.CY_Code);
				AssertEquals("att13741 ParentAttributeCode", ZString.Empty, att13741.ParentAttributeCode);
				AssertEquals("att13741 ConditionDescription", ZString.Empty, att13741.ConditionDescription);
				AssertEquals("att13741 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13741.CY_DataFieldType);
				AssertEquals("att13741 FillOrientation", ZString.Empty, att13741.FillOrientation);
				AssertEquals("att13741 Example", ZString.Empty, att13741.Example);
				AssertEquals("att13741 Content should NOT be ReadOnly", false, att13741.ContentInfo.ReadOnly);
				AssertEquals("att13741 CY_DataDecimalPlaces should be 0", (ZShort)0, att13741.CY_DataDecimalPlaces);
				AssertEquals("att13741 IsMandatory should be true", true, att13741.IsMandatory);
				AssertEquals("att13741 TaxType should NOT be Empty", "PIS, COF", att13741.TaxType);
				AssertEquals("att13741 LegalBase should NOT be Empty", "1100, 1100", att13741.LegalBase);

				AssertEquals("att13743 CY_Code", "ATT_13743", att13743.CY_Code);
				AssertEquals("att13743 ParentAttributeCode", ZString.Empty, att13743.ParentAttributeCode);
				AssertEquals("att13743 ConditionDescription", ZString.Empty, att13743.ConditionDescription);
				AssertEquals("att13743 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13743.CY_DataFieldType);
				AssertEquals("att13743 FillOrientation", ZString.Empty, att13743.FillOrientation);
				AssertEquals("att13743 Example", ZString.Empty, att13743.Example);
				AssertEquals("att13743 Content should NOT be ReadOnly", false, att13743.ContentInfo.ReadOnly);
				AssertEquals("att13743 CY_DataDecimalPlaces should be 0", (ZShort)0, att13743.CY_DataDecimalPlaces);
				AssertEquals("att13743 IsMandatory should be true", true, att13743.IsMandatory);
				AssertEquals("att13743 TaxType should NOT be Empty", "DTY, DTY, DTY", att13743.TaxType);
				AssertEquals("att13743 LegalBase should NOT be Empty", "0006, 0007, 0009", att13743.LegalBase);

				AssertEquals("att2872 CY_Code", "ATT_2872", att2872.CY_Code);
				AssertEquals("att2872 ParentAttributeCode", ZString.Empty, att2872.ParentAttributeCode);
				AssertEquals("att2872 ConditionDescription", ZString.Empty, att2872.ConditionDescription);
				AssertEquals("att2872 CY_DataFieldType", nameof(FieldType.TextDropEdit), att2872.CY_DataFieldType);
				AssertEquals("att2872 FillOrientation", ZString.Empty, att2872.FillOrientation);
				AssertEquals("att2872 Example", ZString.Empty, att2872.Example);
				AssertEquals("att2872 Content should NOT be ReadOnly", false, att2872.ContentInfo.ReadOnly);
				AssertEquals("att2872 CY_DataDecimalPlaces should be 0", (ZShort)0, att2872.CY_DataDecimalPlaces);
				AssertEquals("att2872 IsMandatory should be true", true, att2872.IsMandatory);
				AssertEquals("att2872 TaxType should NOT be Empty", "DTY", att2872.TaxType);
				AssertEquals("att2872 LegalBase should NOT be Empty", "0009", att2872.LegalBase);

				AssertEquals("att2874 CY_Code", "ATT_2874", att2874.CY_Code);
				AssertEquals("att2874 ParentAttributeCode", ZString.Empty, att2874.ParentAttributeCode);
				AssertEquals("att2874 ConditionDescription", ZString.Empty, att2874.ConditionDescription);
				AssertEquals("att2874 CY_DataFieldType", nameof(FieldType.TextDropEdit), att2874.CY_DataFieldType);
				AssertEquals("att2874 FillOrientation", ZString.Empty, att2874.FillOrientation);
				AssertEquals("att2874 Example", ZString.Empty, att2874.Example);
				AssertEquals("att2874 Content should NOT be ReadOnly", false, att2874.ContentInfo.ReadOnly);
				AssertEquals("att2874 CY_DataDecimalPlaces should be 0", (ZShort)0, att2874.CY_DataDecimalPlaces);
				AssertEquals("att2874 IsMandatory should be true", true, att2874.IsMandatory);
				AssertEquals("att2874 TaxType should NOT be Empty", "DTY", att2874.TaxType);
				AssertEquals("att2874 LegalBase should NOT be Empty", "0007", att2874.LegalBase);
			});

			invoiceLine.RemoveDuimpTaxRegimes(invoiceLine.DuimpTaxRegimes.Where(w => w.CSI_Procedure == "0008"));
			CombineAssertions(() =>
			{
				AssertEquals("att13715 CY_Code", "ATT_13715", att13715.CY_Code);
				AssertEquals("att13715 ParentAttributeCode", ZString.Empty, att13715.ParentAttributeCode);
				AssertEquals("att13715 ConditionDescription", ZString.Empty, att13715.ConditionDescription);
				AssertEquals("att13715 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13715.CY_DataFieldType);
				AssertEquals("att13715 FillOrientation", ZString.Empty, att13715.FillOrientation);
				AssertEquals("att13715 Example", ZString.Empty, att13715.Example);
				AssertEquals("att13715 Content should NOT be ReadOnly", false, att13715.ContentInfo.ReadOnly);
				AssertEquals("att13715 CY_DataDecimalPlaces should be 0", (ZShort)0, att13715.CY_DataDecimalPlaces);
				AssertEquals("att13715 IsMandatory should be true", true, att13715.IsMandatory);
				AssertEquals("att13715 TaxType should NOT be Empty", "PIS, COF", att13715.TaxType);
				AssertEquals("att13715 LegalBase should NOT be Empty", "1100, 1100", att13715.LegalBase);

				AssertEquals("att13741 CY_Code", "ATT_13741", att13741.CY_Code);
				AssertEquals("att13741 ParentAttributeCode", ZString.Empty, att13741.ParentAttributeCode);
				AssertEquals("att13741 ConditionDescription", ZString.Empty, att13741.ConditionDescription);
				AssertEquals("att13741 CY_DataFieldType", nameof(FieldType.TextDropEdit), att13741.CY_DataFieldType);
				AssertEquals("att13741 FillOrientation", ZString.Empty, att13741.FillOrientation);
				AssertEquals("att13741 Example", ZString.Empty, att13741.Example);
				AssertEquals("att13741 Content should NOT be ReadOnly", false, att13741.ContentInfo.ReadOnly);
				AssertEquals("att13741 CY_DataDecimalPlaces should be 0", (ZShort)0, att13741.CY_DataDecimalPlaces);
				AssertEquals("att13741 IsMandatory should be true", true, att13741.IsMandatory);
				AssertEquals("att13741 TaxType should NOT be Empty", "PIS, COF", att13741.TaxType);
				AssertEquals("att13741 LegalBase should NOT be Empty", "1100, 1100", att13741.LegalBase);
			});
		}

		public void TestAttributes_RefCusProfile()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);

			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog.CGC_Tariff = "87654321";

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;

				AssertEquals("Attributes count should be 7", 7, catalog.Attributes.Count);

				var attText = catalog.Attributes.GetFirstElementHaving("ATT_3061");
				AssertAttribute(attText, "ATT_3061", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), "Oriatention02");

				var attList = catalog.Attributes.GetFirstElementHaving("ATT_3891");
				AssertAttribute(attList, "ATT_3891", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, nameof(FieldType.TextCodeFindBox), "Oriatention02");

				var att1000 = catalog.Attributes.GetFirstElementHaving("ATT_1000");
				AssertAttribute(att1000, "ATT_1000", Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound, nameof(FieldType.Text), "Oriatention02", readOnly: true, childAtt: new[] { "ATT_10001", "ATT_10002" });

				var att10001 = catalog.Attributes.GetFirstElementHaving("ATT_10001");
				AssertAttribute(att10001, "ATT_10001", Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound, nameof(FieldType.Text), "Oriatention02", readOnly: true, condition: "ATT_1000|ATT_10001", parentAttCode: "ATT_1000", isParentCompoundAtt: true, childAtt: new[] { "ATT_100011", "ATT_100012" });

				var att100011 = catalog.Attributes.GetFirstElementHaving("ATT_100011");
				AssertAttribute(att100011, "ATT_100011", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention02", condition: "ATT_10001|ATT_100011", parentAttCode: "ATT_10001", isParentCompoundAtt: true, decimalPlaces: 2);

				var att100012 = catalog.Attributes.GetFirstElementHaving("ATT_100012");
				AssertAttribute(att100012, "ATT_100012", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention02", condition: "ATT_10001|ATT_100012", parentAttCode: "ATT_10001", isParentCompoundAtt: true);

				var att10002 = catalog.Attributes.GetFirstElementHaving("ATT_10002");
				AssertAttribute(att10002, "ATT_10002", Universal.Constants.ProfileQuestion.AnswerDataTypes.Date, nameof(FieldType.Date), "Oriatention02", condition: "ATT_1000|ATT_10002", parentAttCode: "ATT_1000", isParentCompoundAtt: true);

				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;

				AssertEquals("Attributes count should be 1", 1, catalog.Attributes.Count);

				var att1 = catalog.Attributes.GetFirstElementHaving("ATT1");
				AssertAttribute(att1, "ATT1", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention01");
			}

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;

				AssertEquals("Attributes count should be 4", 4, catalog.Attributes.Count);

				var attNumberInteger = catalog.Attributes.GetFirstElementHaving("ATT_3887");
				AssertAttribute(attNumberInteger, "ATT_3887", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention02");

				var attBoolean = catalog.Attributes.GetFirstElementHaving("ATT_3890");
				AssertAttribute(attBoolean, "ATT_3890", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, nameof(FieldType.TextDropEdit), "Oriatention02", childAtt: new[] { "ATT_38901", "ATT_38902" });

				var att38901 = catalog.Attributes.GetFirstElementHaving("ATT_38901");
				AssertAttribute(att38901, "ATT_38901", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention02", readOnly: true, condition: "[Anwser] = 'true'", parentAttCode: "ATT_3890");

				var att38902 = catalog.Attributes.GetFirstElementHaving("ATT_38902");
				AssertAttribute(att38902, "ATT_38902", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.Text), "Oriatention02", readOnly: true, condition: "[Anwser] = 'false'", parentAttCode: "ATT_3890");

				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;

				AssertEquals("Attributes count should be 3", 3, catalog.Attributes.Count);

				var attList = catalog.Attributes.GetFirstElementHaving("ATT_2557");
				AssertAttribute(attList, "ATT_2557", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.TextMultiLine), "Oriatention02");

				var attNumberReal = catalog.Attributes.GetFirstElementHaving("ATT_3892");
				AssertAttribute(attNumberReal, "ATT_3892", Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, nameof(FieldType.Text), "Oriatention02", decimalPlaces: 5);

				var attStringMultivalues = catalog.Attributes.GetFirstElementHaving("ATT_4807");
				AssertAttribute(attStringMultivalues, "ATT_4807", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, nameof(FieldType.TextMultiLine), "Oriatention02", decimalPlaces: 0);
			}
		}

		public void TestValidation()
		{
			var attribute = Factory.New<AttributeCusCodeData>();
			AssertType<AttributeCusCodeDataValidation>(attribute.Validation);
		}

		public void TestCopyValuesIfEntered()
		{
			var attribute = Factory.New<AttributeCusCodeData>();
			attribute.CY_Order = 0;
			attribute.Content = ZString.Empty;

			var attributeCopy = Factory.New<AttributeCusCodeData>();
			attributeCopy.Content = "TEST1";

			attributeCopy.CopyValuesIfEntered(attribute);
			AssertEquals("Should NOT override Content when source is empty", "TEST1", attributeCopy.Content);
			AssertEquals("Should NOT override CY_Data when source is empty", "TEST1", attributeCopy.CY_Data);

			attribute.Content = "TEST_2";
			attributeCopy.CopyValuesIfEntered(attribute);
			AssertEquals("Should override Content when source is entered", "TEST_2", attributeCopy.Content);
			AssertEquals("Should override CY_Data when source is entered", "TEST_2", attributeCopy.CY_Data);
		}

		public void TestContent()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);
			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
				catalog.CGC_Tariff = "87654321";
				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;

				var attList = catalog.Attributes.GetFirstElementHaving("ATT_3891");
				Assert(attList.Content.IsEmpty);
				Assert(attList.CY_Data.IsEmpty);

				attList.Content = "2";
				AssertEquals("2 - desc2", attList.Content);
				AssertEquals("2", attList.CY_Data);

				attList.Content = "2 - desc2";
				AssertEquals("2 - desc2", attList.Content);
				AssertEquals("2", attList.CY_Data);

				var attText = catalog.Attributes.GetFirstElementHaving("ATT_3061");
				Assert(attText.Content.IsEmpty);
				Assert(attText.CY_Data.IsEmpty);

				attText.Content = "TEST 123";
				AssertEquals("TEST 123", attText.Content);
				AssertEquals("TEST 123", attText.CY_Data);

				var attBoolean = catalog.Attributes.GetFirstElementHaving("ATT_100012");
				Assert(attBoolean.Content.IsEmpty);
				Assert(attBoolean.CY_Data.IsEmpty);

				attBoolean.Content = "N";
				AssertEquals("Não", attBoolean.Content);
				AssertEquals("false", attBoolean.CY_Data);

				attBoolean.Content = "S";
				AssertEquals("Sim", attBoolean.Content);
				AssertEquals("true", attBoolean.CY_Data);

				var attNumberWithDecimals = catalog.Attributes.GetFirstElementHaving("ATT_100011");
				Assert(attNumberWithDecimals.Content.IsEmpty);
				Assert(attNumberWithDecimals.CY_Data.IsEmpty);

				attNumberWithDecimals.Content = "12,53";
				AssertEquals("12,53", attNumberWithDecimals.Content);
				AssertEquals("12.53", attNumberWithDecimals.CY_Data);

				attNumberWithDecimals.Content = "15,4556811547174";
				AssertEquals("15,45", attNumberWithDecimals.Content);
				AssertEquals("15.45", attNumberWithDecimals.CY_Data);

				attNumberWithDecimals.Content = "0,00";
				AssertEquals("0", attNumberWithDecimals.Content);
				AssertEquals("0", attNumberWithDecimals.CY_Data);

				attNumberWithDecimals.Content = ZString.Empty;
				Assert(attNumberWithDecimals.Content.IsEmpty);
				Assert(attNumberWithDecimals.CY_Data.IsEmpty);

				attNumberWithDecimals.Content = "A01,23";
				Assert(attNumberWithDecimals.Content.IsEmpty);
				Assert(attNumberWithDecimals.CY_Data.IsEmpty);

				var attDate = catalog.Attributes.GetFirstElementHaving("ATT_10002");
				Assert(attDate.Content.IsEmpty);
				Assert(attDate.CY_Data.IsEmpty);

				attDate.Content = "D123";
				Assert(attDate.Content.IsEmpty);
				Assert(attDate.CY_Data.IsEmpty);

				attDate.Content = "30-Jun-02";
				AssertEquals("30-JUN-02", attDate.Content);
				AssertEquals("30/06/2002", attDate.CY_Data);
			}
		}

		public void TestContentReadOnly()
		{
			var compoundQuestion = Factory.New<RefCusProfileQuestion>();
			compoundQuestion.XQ2_Code = "ATT_1";
			compoundQuestion.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound;

			var parentQuestion = Factory.New<RefCusProfileQuestion>();
			parentQuestion.XQ2_Code = "ATT_2";
			parentQuestion.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			parentQuestion.XQ2_AllowMultipleAnswers = true;
			parentQuestion.Answers.AddNew().XQ4_Value = "1";
			parentQuestion.Answers.AddNew().XQ4_Value = "2";
			parentQuestion.Answers.AddNew().XQ4_Value = "3";
			parentQuestion.Answers.AddNew().XQ4_Value = "4";
			parentQuestion.Answers.AddNew().XQ4_Value = "5";

			var childQuestion = Factory.New<RefCusProfileQuestion>();
			childQuestion.XQ2_Code = "ATT_3";
			childQuestion.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Number;

			var pathway = Factory.New<RefCusProfileQuestionPathway>();
			pathway.XQP_XQ2_QuestionParent = parentQuestion.PK;
			pathway.XQP_XQ2_QuestionChild = childQuestion.PK;

			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var parentAttribute = goodsCatalog.Attributes.AddNew();

			parentAttribute.CY_Code = compoundQuestion.XQ2_Code;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(compoundQuestion);
			Assert("Content Read Only when AnswerDataType = Compound", parentAttribute.ContentInfo.ReadOnly);

			parentAttribute.CY_Code = parentQuestion.XQ2_Code;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(parentQuestion);
			Assert("Parent Attribute Content NOT Read Only when QuestionPathway is null", !parentAttribute.ContentInfo.ReadOnly);

			var childAttribute = goodsCatalog.Attributes.AddNew();
			childAttribute.CY_Code = childQuestion.XQ2_Code;
			childAttribute.TariffProfileQuestion = TariffProfileQuestion.New(childQuestion);
			Assert("Child Attribute Content NOT Read Only when QuestionPathway is null", !childAttribute.ContentInfo.ReadOnly);

			childAttribute.QuestionPathway = pathway;
			Assert("Child Attribute Content NOT Read Only when XQP_ConditionToProceedFormula is empty", !childAttribute.ContentInfo.ReadOnly);

			AssertContentReadOnly(AnswerDataTypes.List, "([Answer] = 3 | [Answer] = 4 | [Answer] = 5)", new[] { "3", "4", "5", "1,2,3" }, new[] { "6", "1,2", "XX", "" });
			AssertContentReadOnly(AnswerDataTypes.String, "([Answer] = 3 | [Answer] = 4 | [Answer] = 5)", new[] { "3", "4", "5", "1\r\n2\r\n3" }, new[] { "6", "1,2", "XX", "" });
			AssertContentReadOnly(AnswerDataTypes.Boolean, "[Answer] = 1", new[] { "S" }, new[] { "N", "S,N", "1", "" });
			AssertContentReadOnly(AnswerDataTypes.Number, "[Answer] = 3", new[] { "3" }, new[] { "1", "13", "XX", "" });
			AssertContentReadOnly(AnswerDataTypes.Number, "[Answer] != 3", new[] { "1", "13" }, new[] { "3", "XX", "" });
			AssertContentReadOnly(AnswerDataTypes.Number, "[Answer] >= 3", new[] { "3", "4", "12" }, new[] { "0", "2", "XX", "" });
			AssertContentReadOnly(AnswerDataTypes.Number, "([Answer] >= 26 & [Answer] <= 60)", new[] { "26", "44", "60" }, new[] { "0", "25.9", "60.1", "XX", "" });

			void AssertContentReadOnly(string answerDataType, string formula, string[] validAnswers, string[] invalidAnswers)
			{
				CombineAssertions($"AnswerDataType:{answerDataType}, Formula:{formula}", () =>
				{
					parentQuestion.XQ2_AnswerDataType = answerDataType;
					parentQuestion.XQ2_AllowMultipleAnswers = true;
					parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(parentQuestion);

					pathway.XQP_ConditionToProceedFormula = formula;

					foreach (var answer in validAnswers)
					{
						parentAttribute.Content = answer;
						Assert($"Child Attribute Content NOT Read Only when Parent Attribute = {parentAttribute.Content}", !childAttribute.ContentInfo.ReadOnly);
					}
					foreach (var answer in invalidAnswers)
					{
						parentAttribute.Content = answer;
						Assert($"Child Attribute Content Read Only when Parent Attribute = {parentAttribute.Content}", childAttribute.ContentInfo.ReadOnly);
					}
				});
			}
		}

		public void TestIsEffectiveInFuture()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var parentAttribute = goodsCatalog.Attributes.AddNew();

			var compoundQuestion = Factory.New<RefCusProfileQuestion>();
			compoundQuestion.XQ2_Code = "ATT_1";

			parentAttribute.CY_Code = compoundQuestion.XQ2_Code;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(compoundQuestion);

			AssertEquals("IsEffectiveInFuture should be False ", false, parentAttribute.IsEffectiveInFuture);

			compoundQuestion.XQ2_StartDate = ZDateTime.Today;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(compoundQuestion);
			AssertEquals("IsEffectiveInFuture should be False ", false, parentAttribute.IsEffectiveInFuture);

			compoundQuestion.XQ2_StartDate = ZDateTime.Today.AddDays(-10);
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(compoundQuestion);
			AssertEquals("IsEffectiveInFuture should be False ", false, parentAttribute.IsEffectiveInFuture);

			compoundQuestion.XQ2_StartDate = ZDateTime.Today.AddDays(+10);
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(compoundQuestion);
			AssertEquals("IsEffectiveInFuture should be True ", true, parentAttribute.IsEffectiveInFuture);

			compoundQuestion.XQ2_StartDate = ZDateTime.Invalid;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(compoundQuestion);
			AssertEquals("IsEffectiveInFuture should be False ", false, parentAttribute.IsEffectiveInFuture);
		}

		void AssertAttribute(AttributeCusCodeData att, string code, string answerDataType, string fieldType, string fillOrientation, string taxType = "", string legalBase = "", bool readOnly = false, string condition = "",
							string parentAttCode = "", bool isParentCompoundAtt = false, short decimalPlaces = 0, string example = "", IEnumerable<string> childAtt = null)
		{
			CombineAssertions("Assert fields to Attribute " + att.CY_Code, () =>
			{
				AssertEquals("att CY_Code should be", code, att.CY_Code);
				AssertEquals("att Forma Preenchimento should be", answerDataType, att.TariffProfileQuestion.AnswerDataType);
				AssertEquals("att CY_DataFieldType should be", fieldType, att.CY_DataFieldType);
				AssertEquals("att FillOrientation should be", fillOrientation, att.FillOrientation);
				AssertEquals("att Example should be", example, att.Example);
				AssertEquals("att CY_DataDecimalPlaces should be", decimalPlaces, att.CY_DataDecimalPlaces);
				AssertEquals("att TaxType should be", taxType, att.TaxType);
				AssertEquals("att LegalBase should be", legalBase, att.LegalBase);
				AssertEquals("att Content should be ReadOnly", readOnly, att.ContentInfo.ReadOnly);
				AssertEquals("att IsCompoundAttribute  should be", att.TariffProfileQuestion.AnswerDataType == AnswerDataTypes.Compound, att.IsCompoundAttribute);
				AssertEquals("att ConditionDescription should be", condition, att.ConditionDescription);
				AssertEquals("att ParentAttributeCode should be", parentAttCode, att.ParentAttributeCode);
				AssertEquals("att ParentIsCompoundAttribute should be", isParentCompoundAtt, att.ParentIsCompoundAttribute);
				AssertEquals("att StartDate should be", ZDateTime.MinSmallDateTimeValue, att.StartDate);
				AssertEquals("att EndDate should be", ZDateTime.MaxSmallDateTimeValue, att.EndDate);

				if (childAtt != null)
				{
					AssertContainsExactElementsInAnyOrder("att ChildAttributes should be ", childAtt, att.ChildAttributes.Select(s => s.CY_Code));
				}
				else
				{
					Assert("att ChildAttributes should be Empty", !att.ChildAttributes.Any());
				}
			});
		}

		public void TestContentWithMultilines()
		{
			var stringQuestion = Factory.New<RefCusProfileQuestion>();
			stringQuestion.XQ2_Code = "ATT_1";
			stringQuestion.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			stringQuestion.XQ2_AllowMultipleAnswers = true;

			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var parentAttribute = goodsCatalog.Attributes.AddNew();

			parentAttribute.CY_Code = stringQuestion.XQ2_Code;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			parentAttribute.Content = "TEST\r\n123\r\n456";
			AssertAttributeCusCodeData(parentAttribute, "ATT_1", "TEST", (ZShort)0);
			AssertEquals("MultivaluedAttributesLinked.Count", 2, parentAttribute.MultivaluedAttributesLinked.Count);
			AssertAttributeCusCodeData(parentAttribute.MultivaluedAttributesLinked[0], "ATT_1", "123", (ZShort)1);
			AssertAttributeCusCodeData(parentAttribute.MultivaluedAttributesLinked[1], "ATT_1", "456", (ZShort)2);
			AssertEquals("Content", "TEST\r\n123\r\n456", parentAttribute.Content);

			parentAttribute.Content = "TEST1\r\n";
			AssertAttributeCusCodeData(parentAttribute, "ATT_1", "TEST1", (ZShort)0);
			AssertEquals("MultivaluedAttributesLinked.Count", 0, parentAttribute.MultivaluedAttributesLinked.Count);
			AssertEquals("Content", "TEST1", parentAttribute.Content);

			parentAttribute.Content = "TEST2\r\n\r\n123\r\n";
			AssertAttributeCusCodeData(parentAttribute, "ATT_1", "TEST2", (ZShort)0);
			AssertEquals("MultivaluedAttributesLinked.Count", 1, parentAttribute.MultivaluedAttributesLinked.Count);
			AssertAttributeCusCodeData(parentAttribute.MultivaluedAttributesLinked[0], "ATT_1", "123", (ZShort)1);
			AssertEquals("Content", "TEST2\r\n123", parentAttribute.Content);

			parentAttribute.Content = "\r\n";
			AssertAttributeCusCodeData(parentAttribute, "ATT_1", ZString.Empty, (ZShort)0);
			AssertEquals("MultivaluedAttributesLinked.Count", 0, parentAttribute.MultivaluedAttributesLinked.Count);

			void AssertAttributeCusCodeData(AttributeCusCodeData attribute, ZString expectedCode, ZString expectedData, ZShort expectedOrder) => CombineAssertions(() =>
			{
				AssertEquals("CY_Type", CusCodeDataTypeList.Codes.Attribute, attribute.CY_Type);
				AssertEquals("CY_Code", expectedCode, attribute.CY_Code);
				AssertEquals("CY_Data", expectedData, attribute.CY_Data);
				AssertEquals("CY_Order", expectedOrder, attribute.CY_Order);
			});
		}

		public void TestAllowMutipleAnswersForString()
		{
			var stringQuestion = Factory.New<RefCusProfileQuestion>();
			stringQuestion.XQ2_Code = "ATT_1";
			stringQuestion.XQ2_AnswerDataType = AnswerDataTypes.String;
			stringQuestion.XQ2_AllowMultipleAnswers = true;

			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var parentAttribute = goodsCatalog.Attributes.AddNew();
			parentAttribute.CY_Code = stringQuestion.XQ2_Code;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			Assert("AllowMutipleAnswersForString", parentAttribute.AllowMultipleAnswersForString);

			stringQuestion.XQ2_AllowMultipleAnswers = false;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			Assert("AllowMutipleAnswersForString", !parentAttribute.AllowMultipleAnswersForString);

			stringQuestion.XQ2_AllowMultipleAnswers = true;
			stringQuestion.XQ2_AnswerDataType = AnswerDataTypes.List;
			parentAttribute.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion);
			Assert("AllowMutipleAnswersForString", !parentAttribute.AllowMultipleAnswersForString);
		}

		public void TestAllowMutipleAnswersForList()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);
			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
				catalog.CGC_Tariff = "87654321";
				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;

				var attList = catalog.Attributes.GetFirstElementHaving("ATT_3891");
				Assert("AllowMutipleAnswersForList", attList.AllowMultipleAnswersForList);

				var attDate = catalog.Attributes.GetFirstElementHaving("ATT_10002");
				Assert("AllowMutipleAnswersForList", !attDate.AllowMultipleAnswersForList);
			}
		}

		public void TestAnswers()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);
			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				catalog.CGC_Tariff = "87654321";
				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;

				var attList = catalog.Attributes.GetFirstElementHaving("ATT_3891");
				attList.Content = "2,3";
				AssertContainsExactElementsInExactOrder("Answers for LIST", new ZString[] { "2", "3" }, attList.Answers);

				var attDate = catalog.Attributes.GetFirstElementHaving("ATT_10002");
				attDate.Content = "30-Jun-02";
				AssertContainsExactElementsInExactOrder("Answers for DATE", new ZString[] { "30/06/2002" }, attDate.Answers);
			}

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
				var attMultilinesText = catalog.Attributes.GetFirstElementHaving("ATT_4807");
				attMultilinesText.Content = "1\r\n2\r\n3";
				AssertContainsExactElementsInExactOrder("Answers for STRING", new ZString[] { "1", "2", "3" }, attMultilinesText.Answers);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().Attributes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().Attributes.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
