using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CUSWATHeaderProvider))]
	sealed class CUSWATHeaderProviderTest : ImportHeaderProviderAbstractTest<CUSWATHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSWATHeaderProvider(null));
		}

		public void TestCustomsWarehouseDepartureLocalReferenceNumber()
		{
			entryInstruction.PreviousDocumentMaster.CSI_ReferenceNumber2 = "REF123";
			AssertEquals("REF123", Provider.CustomsWarehouseDepartureLocalReferenceNumber);
		}

		public void TestCustomsWarehouseDeparture()
		{
			entryInstruction.PreviousDocumentMaster.AuthorizationNumber = "DESDE5864S9000648";
			AssertEquals("DESDE5864S9000648", Provider.CustomsWarehouseDeparture);
		}

		public void TestCurrentProcedure_DeclarantIsNull()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals(ZString.Empty, Provider.CurrentProcedure);
		}

		public void TestCurrentProcedure()
		{
			var orgHeader = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				var cusAuthorisationHeader = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
				var address = orgHeader.Addresses.AddNew();
				declaration.JE_OA_DeclarantAddress = address.PK;
				AssertEquals("CW1-Authorisation is valid", "NUMBER1", Provider.CurrentProcedure);

				cusAuthorisationHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
				orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER2");
				var newProvider = GetProvider();
				AssertEquals("CW1-Authorisation is out of date, CWP-Authorisation is valid", "NUMBER2", newProvider.CurrentProcedure);
			});
		}

		public void TestDeclarationPlace()
		{
			AssertEquals("Brisbane", Provider.DeclarationPlace);
		}

		public void TestWarehouseOwner()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			declaration.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR123", Constants.CountryCodes.Germany);
			declaration.Declarant.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS456", Constants.CountryCodes.Germany);
			var warehouseOwner = Provider.WarehouseOwner;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "DEEOR123", warehouseOwner.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS456", warehouseOwner.EoriBranchSuffix);
			});
		}

		public void TestWarehouseOwner_NoDeclarant()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_GB = ZGuid.Empty;
			AssertNull(Provider.WarehouseOwner);
		}

		public void TestContactPerson()
		{
			CombineAssertions(() =>
			{
				using (Factory.SetTemporaryCurrentUser("Sachbearbeiter", "Bob Baumeister", "06131474747", "bob.baumeister@samplefreight.de"))
				{
					AssertEquals("title", "Sachbearbeiter", Provider.ContactPerson.Position);
					AssertEquals("name", "Bob Baumeister", Provider.ContactPerson.PersonName);
					AssertEquals("phoneNumber", "06131474747", Provider.ContactPerson.PhoneNumber);
					AssertEquals("eMail", "bob.baumeister@samplefreight.de", Provider.ContactPerson.MailAddress);
				}
			});
		}

		public void TestDepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber()
		{
			entryInstruction.PreviousDocumentMaster.AuthorizationNumber = "DESDE5864S9000648";
			AssertEquals("DE005864", Provider.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber);
		}

		public void TestDepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber_AuthorizationNumberEmpty()
		{
			entryInstruction.PreviousDocumentMaster.AuthorizationNumber = string.Empty;
			AssertNull(Provider.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber);
		}

		public void TestDepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber_AuthorizationNumberNull()
		{
			entryInstruction.PreviousDocumentMaster.AuthorizationNumber = null;
			AssertNull(Provider.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber);
		}

		public void TestDocuments()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			var doc1 = invoice.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N380";
			doc1.CSI_ReferenceNumber = "REF1";
			doc1.CSI_DateOfIssue = ZDate.Today;
			var doc2 = invoice.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N999";
			doc2.CSI_ReferenceNumber = "REF2";
			doc2.CSI_DateOfIssue = ZDate.Today;
			var doc3 = invoice.SupportingDocuments.AddNew();
			doc3.CSI_Code = ZString.Empty;
			doc3.CSI_ReferenceNumber = "should not show";
			doc3.CSI_DateOfIssue = ZDate.Today;
			AssertEquals(2, Provider.Documents.Count);
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

		protected override IEnumerable<Expression<Func<CUSWATHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.CurrentProcedure;
			yield return x => x.WarehouseOwner;
			yield return x => x.ContactPerson;
			yield return x => x.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber;
			yield return x => x.Documents;
			yield return x => x.Lines;
		}

		protected override CUSWATHeaderProvider GetProvider() => new CUSWATHeaderProvider(entryHeader);

		protected override void SetUp()
		{
			base.SetUp();
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
		}
	}
}
