using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIPEDHeaderProvider))]
	class SCIPEDHeaderProviderTest : MonthlyClosingDecHeaderProviderAbstractTest<SCIPEDHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCIPEDHeaderProvider(null, string.Empty));
		}

		public void TestLocalClearanceProcedure_EIR()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AAV;
			PrepareCusAuthorizationUsages();
			AssertEquals("AUTHEIR", Provider.LocalClearanceProcedure);
		}

		public void TestLocalClearanceProcedure_SDE()
		{
			declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.VAV;
			PrepareCusAuthorizationUsages();
			AssertEquals("AUTHSDE", Provider.LocalClearanceProcedure);
		}

		public void TestLocalClearanceProcedure_Empty_InvalidDeclarantType()
		{
			PrepareCusAuthorizationUsages();
			declaration.CRD_DeclarantType = MonthlyClosingDeclarationTypeList.Codes.AZ;
			AssertNullOrEmpty(Provider.LocalClearanceProcedure);
		}

		public void TestLocalClearanceProcedure_Empty_NoAuthorizationUsage()
		{
			declaration.CRD_DeclarantType = MonthlyClosingDeclarationTypeList.Codes.AZ;
			AssertNullOrEmpty(Provider.LocalClearanceProcedure);
		}

		public void TestProcedureAuthorization()
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			header.CPH_Number = "AUTHNUMBER";
			declaration.CRD_CPH_ReconClearanceAuthorisation = header.PK;
			AssertEquals("AUTHNUMBER", Provider.ProcedureAuthorization);
		}

		public void TestBodies()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_CH = entryHeader.PK;
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_CH = entryHeader.PK;
			entryLine2.CL_LineNumber = 2;
			var invocie1 = Factory.New<JobComInvoiceHeader>();
			var line1 = invocie1.JobComInvoiceLines.AddNew();
			var invocie2 = Factory.New<JobComInvoiceHeader>();
			var line2 = invocie2.JobComInvoiceLines.AddNew();
			var reconEntry1 = declaration.CusReconEntries.AddNew();
			reconEntry1.CRE_CH_OriginalEntry = entryHeader.PK;
			var reconEntryLine1 = reconEntry1.CusReconEntryLines.AddNew();
			reconEntryLine1.CRL_OriginalEntryLineNumber = 1;
			var reconEntry2 = declaration.CusReconEntries.AddNew();
			reconEntry2.CRE_CH_OriginalEntry = entryHeader.PK;
			var reconEntryLine2 = reconEntry2.CusReconEntryLines.AddNew();
			reconEntryLine2.CRL_OriginalEntryLineNumber = 2;
			AssertEquals(2, Provider.Bodies.Count);
		}

		public void TestBodies_NoLines()
		{
			AssertEquals(0, Provider.Bodies.Count);
		}

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
		}

		protected override SCIPEDHeaderProvider GetProvider() => new SCIPEDHeaderProvider(declaration, string.Empty);

		void PrepareCusAuthorizationUsages()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entry = declaration.CusReconEntries.AddNew();
			entry.CRE_CH_OriginalEntry = entryHeader.PK;
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var usageEIR = entryInstruction.CusAuthorizationUsages.AddNew();
			usageEIR.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			usageEIR.AGC_Number = "AUTHEIR";

			var usageSDE = entryInstruction.CusAuthorizationUsages.AddNew();
			usageSDE.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			usageSDE.AGC_Number = "AUTHSDE";
		}

		new ISCIPEDHeader Provider => base.Provider;
	}
}
