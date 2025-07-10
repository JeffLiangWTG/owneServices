using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCPEDBodyProvider))]
	sealed class CFCPEDBodyProviderTest : MonthlyClosingDecBodyProviderAbstractTest<CFCPEDBodyProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCPEDBodyProvider(null, isModificationMessage: false));
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
				orgAddress.OA_Address1 = "Address 1";
				entry.CRE_EntryType = "TYP";
				entry.CRE_OA_DeclarantAddress = orgAddress.PK;
				TestHelper.CreateCL010CoutryList(Factory);
				var consigorAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				entry.EntryHeader.Declaration.SupplierDocumentaryAddress.E2_OA_Address = consigorAddress.PK;
				AssertNotNull("Populated", Provider.Consignor);
				AssertEquals("Consignor's EORI", "GREOR1", Provider.Consignor.Identification.EoriNumber);
			});
		}

		public void TestConsignorPK()
		{
			var consigorAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			entry.EntryHeader.Declaration.SupplierDocumentaryAddress.E2_OA_Address = consigorAddress.PK;
			AssertEquals(consigorAddress.PK, Provider.ConsignorPK);
		}

		public void TestConsignorPK_WhenSupplierDocumentaryAddressIsNull_ShouldReturnNull()
		{
			AssertNull(Provider.ConsignorPK);
		}

		public void TestAdditionalDutyReferences()
		{
			var fiscalReference1 = entryInstruction.FiscalReferences.AddNew();
			fiscalReference1.CFR_Code = "RF1";
			fiscalReference1.CFR_Reference = "RN001";
			var fiscalReference2 = entryInstruction.FiscalReferences.AddNew();
			fiscalReference2.CFR_Code = "RF2";
			fiscalReference2.CFR_Reference = "RN002";
			AssertEquals(2, Provider.AdditionalDutyReferences.Count);
		}

		public void TestAdditionalDutyReferences_Empty()
		{
			AssertEquals(0, Provider.AdditionalDutyReferences.Count);
		}

		public void TestLines()
		{
			// rejected with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// rejected no snapshot should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";

			// status blank should be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(2, Provider.Lines.Count);
		}

		public void TestLinesEntryHasSnapshot()
		{
			entry.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// rejected should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";

			// status ERR should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "ERR";

			// status blank should be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(2, Provider.Lines.Count);
		}

		public void TestModification()
		{
			isModificationMessage = true;

			// RC2 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "RC2";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// TX4 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "TX4";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// ERR with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "ERR";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// REJ with snapshot should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// status blank should not be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(3, Provider.Lines.Count);
		}

		public void TestModificationEntryHasSnapshot()
		{
			isModificationMessage = true;
			entry.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// RC2 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "RC2";

			// TX4 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "TX4";

			// ERR with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "ERR";

			// REJ with snapshot should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";

			// status blank should not be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(3, Provider.Lines.Count);
		}

		bool isModificationMessage;

		protected override CFCPEDBodyProvider GetProvider() => new CFCPEDBodyProvider(entry, isModificationMessage);

		new ICFCPEDBody Provider => base.Provider;
	}
}
