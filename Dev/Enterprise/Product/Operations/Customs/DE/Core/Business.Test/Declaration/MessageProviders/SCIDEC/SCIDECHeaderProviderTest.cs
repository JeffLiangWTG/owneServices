using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIDECHeaderProvider))]
	sealed class SCIDECHeaderProviderTest : ImportHeaderProviderAbstractTest<SCIDECHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCIDECHeaderProvider(null));
		}

		public void TestSimplifiedRequestAuthorisationFlag()
		{
			entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			AssertEquals(SimplifiedGrantAuthorizationList.Codes.J, Provider.SimplifiedRequestAuthorisationFlag);
		}

		public void TestConsignor_Null()
		{
			AssertNull(Provider.Consignor);
		}

		public void TestConsignor()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var consignorAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress.PK;
			AssertEquals("Consignor's EORI", "GREOR1", Provider.Consignor.Identification.EoriNumber);
		}

		public void TestInwardProcessingCompletionLimitDate()
		{
			entryInstruction.CEI_CompletionDuration = 3;
			AssertEquals(3, Provider.InwardProcessingCompletionLimitDate);
		}

		public void TestInwardProcessingCriteriaType()
		{
			entryInstruction.CEI_CriteriaType = CriteriaTypeList.Codes._1;
			AssertEquals(CriteriaTypeList.Codes._1, Provider.InwardProcessingCriteriaType);
		}

		public void TestInwardProcessingAdditionalInformation()
		{
			entryInstruction.CEI_InwardProcessingAdditionalInformation = "Some additional info";
			AssertEquals("Some additional info", Provider.InwardProcessingAdditionalInformation);
		}

		public void TestIntendedActivityDetailDescription()
		{
			entryInstruction.CEI_InwardProcessingDescription = "Some processing description";
			AssertEquals("Some processing description", Provider.IntendedActivityDetailDescription);
		}

		public void TestMainAccounting()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.Address1 = "Main accounting address";
			entryInstruction.MainAccountingAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Address", "Main accounting address", Provider.MainAccounting.Address);
		}

		public void TestFirstInwardProcessingPlace_NoInwardProcessingPlaces()
		{
			AssertNull(Provider.FirstInwardProcessingPlace);
		}

		public void TestFirstInwardProcessingPlace()
		{
			AddInwardProcessingPlace("Address of place1");
			AddInwardProcessingPlace("Address of place2");
			AssertEquals("Address of place1", Provider.FirstInwardProcessingPlace.Address);
		}

		public void TestAdditionalInwardProcessingPlace_NoInwardProcessingPlaces()
		{
			AssertEquals(false, Provider.AdditionalInwardProcessingPlace.Any());
		}

		public void TestAdditionalInwardProcessingPlace_OneInwardProcessingPlaces()
		{
			AddInwardProcessingPlace("Address of place1");
			AssertEquals(false, Provider.AdditionalInwardProcessingPlace.Any());
		}

		public void TestAdditionalInwardProcessingPlace()
		{
			AddInwardProcessingPlace("Address of place1");
			AddInwardProcessingPlace("Address of place2");
			AddInwardProcessingPlace("Address of place3");
			AssertContainsExactElementsInAnyOrder(new[] { "Address of place2", "Address of place3" }, Provider.AdditionalInwardProcessingPlace.Select(x => x.Address).ToArray());
		}

		public void TestCompletionCustomsOfficeReferenceNumbers_NoCompletionCustomsOffices()
		{
			AssertEquals(false, Provider.CompletionCustomsOfficeReferenceNumbers.Any());
		}

		public void TestCompletionCustomsOfficeReferenceNumbers()
		{
			entryInstruction.CompletionCustomsOffices.AddNew("code1", "office1");
			entryInstruction.CompletionCustomsOffices.AddNew("code2", "office2");
			AssertContainsExactElementsInAnyOrder(new ZString[] { "office1", "office2" }, Provider.CompletionCustomsOfficeReferenceNumbers.ToArray());
		}

		public void TestForeignTradeStatisticsTransactionType_NoInvoiceHeader()
		{
			AssertNull(Provider.ForeignTradeStatisticsTransactionType);
		}

		public void TestForeignTradeStatisticsTransactionType()
		{
			AddInvoiceWithInvoiceLine().JZ_ValuationCode = YesNoList.Codes.Yes;
			AssertEquals(YesNoList.Codes.Yes, Provider.ForeignTradeStatisticsTransactionType);
		}

		public void TestProcedureAuthorisation()
		{
			entryInstruction.CEI_AuthorisationNumber = "AB123";
			AssertEquals("AB123", Provider.ProcedureAuthorisation);
		}

		public void TestLines()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryHeader.MergedLines.AddNew().PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
			AssertEquals(2, Provider.Lines.Count);
		}

		protected override SCIDECHeaderProvider GetProvider() => new SCIDECHeaderProvider(entryHeader);

		protected override IEnumerable<Expression<Func<SCIDECHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Consignor;
			yield return x => x.MainAccounting;
			yield return x => x.FirstInwardProcessingPlace;
		}

		void AddInwardProcessingPlace(ZString address)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.Address1 = address;
			var place = entryInstruction.InwardProcessingPlaces.AddNew();
			place.E2_OA_Address = orgAddress.PK;
		}

		new ISCIDECHeader Provider => base.Provider;
	}
}
