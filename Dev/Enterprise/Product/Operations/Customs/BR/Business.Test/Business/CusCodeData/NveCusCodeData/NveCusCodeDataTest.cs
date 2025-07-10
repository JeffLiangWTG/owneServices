using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NveCusCodeData))]
	public class NveCusCodeDataTest : Customs.Business.Testing.CusCodeDataTest<NveCusCodeData>
	{
		public void TestLookups()
		{
			var nve = Factory.New<NveCusCodeData>();
			AssertEquals("Lookups", typeof(NveCusCodeDataLookups), nve.Lookups.GetType());
		}

		public void TestNveValues()
		{
			CreateDataFortest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";

			var attList = invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AA");
			attList.CY_Data = "0001";
			AssertEquals("CY_Code", "AA", attList.CY_Code);
			AssertEquals("Specification", "Ductil com nodularidade até 80%", attList.Specification);
			AssertEquals("Position", PositionList.Descriptions.SubItem, attList.Position);
			AssertEquals("Attribute", "NVE DESCRIPTION", attList.Attribute);

			AssertNull(invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("BB"));
		}

		void CreateDataFortest()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN", "NCM");

			anotherFactory.Save();

			var valuesList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("0001","Ductil com nodularidade até 80%"),
				new KeyValuePair<string, string>("9999","Outros"),
			};

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "56049000", compositeKey: "56.049000", ensureDataGroupingExists: false);
			var nve1 = helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "AA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, values: valuesList);
			nve1.ZB1_Text = "NVE DESCRIPTION";
			nve1.ZB1_IsMandatory = true;

			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "BB", Universal.Constants.ProfileQuestion.AnswerDataTypes.List);

			var nomenclatureGroup1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Brazil, "56", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "56", "NCM", ensureDataGroupingExists: false);
			var nomenclatureGroup2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Brazil, "5604", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "56.04", "NCM", ensureDataGroupingExists: false);
			var nomenclatureGroup3 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Brazil, "56049", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "56.049", "NCM", ensureDataGroupingExists: false);
			var nomenclatureGroup4 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Brazil, "560490", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "56.0490", "NCM", ensureDataGroupingExists: false);
			var nomenclatureGroup5 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Brazil, "5604900", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), "DESC GROUP 1", "56.04900", "NCM", ensureDataGroupingExists: false);

			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup1, Constants.Profile.Types.NVE, "AB", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;
			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup2, Constants.Profile.Types.NVE, "AC", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;
			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup3, Constants.Profile.Types.NVE, "AD", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;
			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup4, Constants.Profile.Types.NVE, "AE", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;
			helper.CreateRefCusTariffBRCharacteristic(nomenclatureGroup5, Constants.Profile.Types.NVE, "AF", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;

			anotherFactory.Save();
		}

		public void TestValidation()
		{
			var nve = Factory.New<NveCusCodeData>();
			AssertType<NveCusCodeDataValidation>(nve.Validation);
		}

		public void TestCopyValuesIfEntered()
		{
			var nve = Factory.New<NveCusCodeData>();
			nve.CY_Order = 0;
			nve.CY_Data = ZString.Empty;

			var nveCopy = Factory.New<NveCusCodeData>();
			nveCopy.CY_Data = "TEST1";

			nveCopy.CopyValuesIfEntered(nve);
			AssertEquals("Should NOT override CY_Data when source is empty", "TEST1", nveCopy.CY_Data);

			nve.CY_Data = "TEST_2";
			nveCopy.CopyValuesIfEntered(nve);
			AssertEquals("Should override CY_Data when source is entered", "TEST_2", nveCopy.CY_Data);
		}

		public void TestPositionReadOnly()
		{
			var nve = Factory.New<NveCusCodeData>();
			Assert("Position should always be ReadOnly", nve.PositionInfo.ReadOnly);
		}

		public void TestReadOnly()
		{
			var nve = GetBizObjsForCorrectlyTypeDecideTest(Factory).First();
			Assert("ReadOnly", !nve.ReadOnly);
			var invoiceLine = nve.Parent as JobComInvoiceLine;
			invoiceLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invoiceLine.JI_ParentID = ZGuid.NewZGuid();
			Assert("ReadOnly when cloned from attached Import License Entry", nve.ReadOnly);
		}

		public void TestSetPosition()
		{
			CreateDataFortest();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";

			AssertEquals("Position", PositionList.Descriptions.SubItem, invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AA").Position);
			AssertEquals("Position", PositionList.Descriptions.Chapter, invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AB").Position);
			AssertEquals("Position", PositionList.Descriptions.Position, invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AC").Position);
			AssertEquals("Position", PositionList.Descriptions.SubPositionLevel1, invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AD").Position);
			AssertEquals("Position", PositionList.Descriptions.SubPositionLevel2, invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AE").Position);
			AssertEquals("Position", PositionList.Descriptions.Item, invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AF").Position);
		}

		protected override IEnumerable<NveCusCodeData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			yield return invoiceLine.NVECusCodeDataCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<NveCusCodeData>();
	}
}
