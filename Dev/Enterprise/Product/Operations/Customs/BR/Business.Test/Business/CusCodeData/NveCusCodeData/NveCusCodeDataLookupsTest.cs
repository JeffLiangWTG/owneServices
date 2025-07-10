using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class NveCusCodeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSpecificationList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			Factory.Save();

			var valuesList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("0001","FOR USE IN AGRICULTURE"),
				new KeyValuePair<string, string>("9999","OTHER USES, EXCEPT AGRICULTURAL"),
			};

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "AA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, values: valuesList).ZB1_IsMandatory = true;
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "AB", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";

			var nve = invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AA");
			var specificationList = nve.Lookups.SpecificationList;
			AssertEquals(2, specificationList.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "0001", "9999" }, specificationList.GetAllCodes());

			nve = invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AB");
			specificationList = nve.Lookups.SpecificationList;
			AssertEquals(0, specificationList.Count);
		}

		public void TestPositionList()
		{
			var nveCusCodeData = Factory.New<NveCusCodeData>();

			var lookups = nveCusCodeData.Lookups;
			AssertEquals(6, lookups.PositionList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "1", "2", "3", "4", "5", "6" }, lookups.PositionList.GetAllCodes());
		}
	}
}
