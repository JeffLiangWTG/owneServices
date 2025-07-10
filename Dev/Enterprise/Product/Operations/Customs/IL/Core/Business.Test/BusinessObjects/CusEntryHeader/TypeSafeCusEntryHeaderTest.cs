using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	partial class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<CusEntryHeader>("Update Customs.Business.CusEntryHeader to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryHeader)));
		}

		public void TestCusEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			AssertType<CusEntryInstruction>(entryHeader.EntryInstruction);
		}

		public void TestConfirmedCusEntryHeaderChargesCollectionType()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>>(entryHeader.ConfirmedCharges);
		}

		public void TestCusEntryHeaderChargesCollectionType()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<CusEntryHeaderChargesCollection<CusEntryHeaderCharges>>(entryHeader.Charges);
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);
	}
}
