using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class JobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobComInvoiceHeaderIncoTermList_ChangeAsTransportModeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = declaration.Invoices.AddNew();
			var incoTermList = invoice.Lookups.JZ_IncoTerm_List;
			var seaItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeAlongsideShip);
			var roadItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeCarrier);
			var cdsItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.Other);
			AssertEquals("Default List", 8, incoTermList.Count);
			AssertNull("FAS does not exist in the IncoTermList", seaItem);
			AssertNotNull("FCA exists in the IncoTermList", roadItem);
			AssertNotNull("XXX exists in the IncoTermList", cdsItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			incoTermList = invoice.Lookups.JZ_IncoTerm_List;
			seaItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeAlongsideShip);
			roadItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeCarrier);
			cdsItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.Other);
			AssertEquals("IncoTermList for SEA", 12, incoTermList.Count);
			AssertNotNull("FAS exists in the IncoTermList", seaItem);
			AssertNotNull("FCA exists in the IncoTermList", roadItem);
			AssertNotNull("XXX exists in the IncoTermList", cdsItem);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			incoTermList = invoice.Lookups.JZ_IncoTerm_List;
			seaItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeAlongsideShip);
			roadItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.FreeCarrier);
			cdsItem = incoTermList.GetDescriptionFromCode(Constants.IncoTerms.Other);
			AssertEquals("IncoTermList for ALL", 8, incoTermList.Count);
			AssertNull("FAS does not exist in the IncoTermList", seaItem);
			AssertNotNull("FCA exists in the IncoTermList", roadItem);
			AssertNotNull("XXX exists in the IncoTermList", cdsItem);
		}

		public void TestIncoTermList()
		{
			Assert("GB has own implementation of IncoTermList as previously tested", true);
		}
	}
}
