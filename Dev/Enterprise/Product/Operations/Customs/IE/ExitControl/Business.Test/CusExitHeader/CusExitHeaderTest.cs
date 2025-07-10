using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeader))]
	sealed class CusExitHeaderTest : EU.ExitControl.Business.Testing.CusExitHeaderAbstractTest<CusExitHeader>
	{
		public void TestDefaultDataFromParent()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "COM";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BRN";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_OwnerRef = "UCR001";
			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = orgHeader.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN001";
			entry.CH_BGMReference = "UCR_LRN";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var mrn2 = CusEntryNumber.LoadOrCreate(entry2, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			entry2.CH_BGMReference = "UCR002";

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = declaration;
			exitHeader.DefaultDataFromParent(true);
			CombineAssertions("Include Header Data", () =>
			{
				AssertEquals("CXH_GB_Branch", branch.PK, exitHeader.CXH_GB_Branch);
				AssertEquals("CXH_OH_Exporter", supplier.PK, exitHeader.CXH_OH_Exporter);
				AssertEquals("CXH_OA_Carrier", orgHeader.MainAddress.PK, exitHeader.CXH_OA_Carrier);

				var consignments = exitHeader.CusExitConsignments;
				AssertEquals("entry.mrn has value, entry2.mrn is empty", 1, consignments.Count);
				var consignment = consignments[0];
				AssertEquals("CXC_MovementReference", "MRN001", consignment.CXC_MovementReference);
				AssertEquals("CXC_UniqueConsignmentReference set to JE_OwnerRef", "UCR001", consignment.CXC_UniqueConsignmentReference);
			});

			mrn2.CE_EntryNum = "MRN002";
			declaration.JE_OwnerRef = ZString.Empty;
			exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = declaration;
			exitHeader.DefaultDataFromParent(false);
			CombineAssertions("Not Include Header Data", () =>
			{
				AssertEquals("CXH_GB_Branch", GlbBranch.CurrentBranch.PK, exitHeader.CXH_GB_Branch);
				AssertEquals("CXH_OH_Exporter", ZGuid.Empty, exitHeader.CXH_OH_Exporter);
				AssertEquals("CXH_OA_Carrier", ZGuid.Empty, exitHeader.CXH_OA_Carrier);

				var consignments = exitHeader.CusExitConsignments;
				AssertEquals("Both entry.mrn and entry2.mrn have values", 2, consignments.Count);
				var consignment = consignments[0];
				AssertEquals("CXC_MovementReference", "MRN001", consignment.CXC_MovementReference);
				AssertEquals("JE_OwnerRef is empty, CXC_UniqueConsignmentReference set to lrn", "UCR_LRN", consignment.CXC_UniqueConsignmentReference);
				var consignment2 = consignments[1];
				AssertEquals("consignment2.CXC_MovementReference", "MRN002", consignment2.CXC_MovementReference);
				AssertEquals("JE_OwnerRef is empty, CXC_UniqueConsignmentReference set to lrn", "UCR002", consignment2.CXC_UniqueConsignmentReference);
			});
		}

		public void TestValidation()
		{
			AssertType<CusExitHeaderValidation>(exitHeader.Validation);
		}

		public void TestCusExitConsignments()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentCollection<CusExitConsignment>>(exitHeader.CusExitConsignments);
		}

		public void TestCusExitReports()
		{
			AssertType<ExitControlBase.Business.CusExitReportCollection<CusExitReport>>(exitHeader.CusExitReports);
		}

		public void TestCusExitConsignmentPackages()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPackageCollection<CusExitConsignmentPackage>>(exitHeader.CusExitConsignmentPackages);
		}

		public void TestCusExitContainers()
		{
			AssertType<ExitControlBase.Business.CusExitContainerCollection<CusExitContainer>>(exitHeader.CusExitContainers);
		}
	}
}
