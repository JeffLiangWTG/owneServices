using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class HasPGARequirementsTest : TestCaseWithFactory
	{
		public void TestIHasPGARequirements_JobInvoiceLineTest()
		{
			var invoiceLine = GetInvoiceLine();
			var pGARequirementsInstance = (IHasPGARequirements)invoiceLine;

			// assign values to invoice line
			invoiceLine.JI_OA_ManufacturerAddress = new ZGuid("00000000-0000-0000-0000-000000000001");
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_StateOrRegionOfOrigin = "ZZ";

			AssertEquals(new ZGuid("00000000-0000-0000-0000-000000000001"), pGARequirementsInstance.OA_Manufacturer);
			AssertEquals("US", pGARequirementsInstance.RN_NKCountryOfOrigin);
			AssertEquals("ZZ", pGARequirementsInstance.RW_NKOriginState);

			AssertEquals(pGARequirementsInstance.StateCodeListLookup.ElementsAsString, invoiceLine.Lookups.StateCodesList.ElementsAsString);
			AssertEquals(pGARequirementsInstance.CountryOfOriginsLookup.CompleteFilter, (invoiceLine.Lookups.CountryOfOrigins as IBusinessObjectCollection).CompleteFilter);
			AssertEquals(pGARequirementsInstance.ManufacturersLookup.CompleteFilter, invoiceLine.Declaration.Lookups.SuppliersList.CompleteFilter);

			// update values through the interface
			pGARequirementsInstance.OA_Manufacturer = new ZGuid("00000000-0000-0000-0000-000000000002");
			pGARequirementsInstance.RN_NKCountryOfOrigin = "JP";
			pGARequirementsInstance.RW_NKOriginState = "SG";

			AssertEquals(new ZGuid("00000000-0000-0000-0000-000000000002"), invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("JP", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("SG", invoiceLine.JI_StateOrRegionOfOrigin);
		}

		public void TestIHasPGARequirements_CusClassPartPivotTest()
		{
			var pivot = GetPivot();
			var pGARequirementsInstance = (IHasPGARequirements)pivot;

			pivot.CCA_OA_Manufacturer = new ZGuid("00000000-0000-0000-0000-000000000001");
			pivot.CCA_RN_NKOrigin = "US";
			pivot.CCA_ProvinceOfOrigin = "ZZ";

			AssertEquals(new ZGuid("00000000-0000-0000-0000-000000000001"), pGARequirementsInstance.OA_Manufacturer);
			AssertEquals("US", pGARequirementsInstance.RN_NKCountryOfOrigin);
			AssertEquals("ZZ", pGARequirementsInstance.RW_NKOriginState);

			AssertEquals(pGARequirementsInstance.StateCodeListLookup.ElementsAsString, pivot.CAClassificationLookups.StatesOfOrigin.ElementsAsString);
			AssertEquals(pGARequirementsInstance.CountryOfOriginsLookup.CompleteFilter, pivot.Lookups.CountryOfOrigins.CompleteFilter);
			AssertEquals(pGARequirementsInstance.ManufacturersLookup.CompleteFilter, pivot.Lookups.Manufacturers.CompleteFilter);

			pGARequirementsInstance.OA_Manufacturer = new ZGuid("00000000-0000-0000-0000-000000000002");
			pGARequirementsInstance.RN_NKCountryOfOrigin = "AU";
			pGARequirementsInstance.RW_NKOriginState = "AL";

			AssertEquals(new ZGuid("00000000-0000-0000-0000-000000000002"), pivot.CCA_OA_Manufacturer);
			AssertEquals("AU", pivot.CCA_RN_NKOrigin);
			AssertEquals("AL", pivot.CCA_ProvinceOfOrigin);
		}

		#region Implementation

		JobComInvoiceLine GetInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			return invoiceLine;
		}

		CusClassPartPivot GetPivot()
		{
			return Factory.New<CusClassPartPivot>();
		}

		#endregion
	}
}
