using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(PGAProgramRequirementCollection))]
	sealed class PGAProgramRequirementCollectionTests : NonPersistentBusinessObjectCollectionTestCase<PGAProgramRequirementCollection>
	{
		public void TestIsDeclared()
		{
			SetUp();
			var emptyCollection = CreatePGARequirement(PGACodes.Codes.HC);
			var programRequirmentCollection = emptyCollection.ProgramCodeRequirements;
			AssertEquals(12, programRequirmentCollection.Count);
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.API));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.BBC));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CPR));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CTO));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.DSE));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.HDR));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.MDE));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.NHP));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.OCS));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.PES));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.RED));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.VET));
			programRequirmentCollection.Populate();
			AssertEquals(12, programRequirmentCollection.Count);
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.API));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.BBC));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CPR));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CTO));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.DSE));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.HDR));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.MDE));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.NHP));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.OCS));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.PES));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.RED));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.VET));
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			programRequirmentCollection.Populate();
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.API));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.BBC));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CPR));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CTO));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.DSE));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.HDR));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.MDE));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.NHP));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.OCS));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.PES));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.RED));
			AssertEquals(false, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.VET));
			invoiceLine.HCPGAHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_BBCProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_CTOProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_DSEProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_HDRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_MDEProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_NHPProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_OCSProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_PESProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_REDProgramInd = YesNoList.Codes.Yes;
			invoiceLine.HCPGAHeader.CA_VETProgramInd = YesNoList.Codes.Yes;
			programRequirmentCollection.Populate();
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.API));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.BBC));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CPR));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.CTO));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.DSE));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.HDR));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.MDE));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.NHP));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.OCS));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.PES));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.RED));
			AssertEquals(true, programRequirmentCollection.IsDeclared(HCPGADepartmentCodes.Codes.VET));
		}

		public void TestPopulate()
		{
			AssertEquals(12, CreatePGARequirement(PGACodes.Codes.HC).ProgramCodeRequirements.Count);
			AssertEquals(1, CreatePGARequirement(PGACodes.Codes.PHAC).ProgramCodeRequirements.Count);
			AssertEquals(3, CreatePGARequirement(PGACodes.Codes.NRCan).ProgramCodeRequirements.Count);
			AssertEquals(3, CreatePGARequirement(PGACodes.Codes.DFO).ProgramCodeRequirements.Count);
			AssertEquals(1, CreatePGARequirement(PGACodes.Codes.GAC).ProgramCodeRequirements.Count);
			AssertEquals(4, CreatePGARequirement(PGACodes.Codes.ECCC).ProgramCodeRequirements.Count);
			AssertEquals(1, CreatePGARequirement(PGACodes.Codes.CNSC).ProgramCodeRequirements.Count);
			AssertEquals(2, CreatePGARequirement(PGACodes.Codes.TC).ProgramCodeRequirements.Count);
			AssertEquals(1, CreatePGARequirement(PGACodes.Codes.CFIA).ProgramCodeRequirements.Count);
		}

		PGARequirement CreatePGARequirement(ZString agencyCode)
		{
			var pgaProvider = new PGARequirementProvider(invoiceLine);
			return new PGARequirement(Factory, agencyCode, pgaProvider);
		}

		#region Overrides

		protected override PGAProgramRequirementCollection GetCollectionToTest()
		{
			SetUp();
			return invoiceLine.PGARequirements[0].ProgramCodeRequirements;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			throw new InvalidOperationException();
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
