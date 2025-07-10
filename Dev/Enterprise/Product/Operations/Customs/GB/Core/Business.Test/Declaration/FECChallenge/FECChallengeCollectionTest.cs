using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(FECChallengeCollection))]
	public class FECChallengeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadedCorrectly()
		{
			entryHeader.FECChallenges.Load();
			AssertEquals(8, entryHeader.FECChallenges.Count);

			var entryHeader2 = dec.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine2.PK;
			var fec = Factory.New<FECChallenge>();
			fec.CY_ParentID = entryHeader2.PK;
			fec.CY_ParentTableCode = "CH";
			fec.CY_Code = "JE_FLG";
			fec.CY_Data = "CN";
			fec.CY_Order = 1;
			fec.CY_IsOverridden = false;

			var fec0 = Factory.New<FECChallenge>();
			fec0.CY_ParentID = entryLine2.PK;
			fec0.CY_ParentTableCode = "CL";
			fec0.CY_Code = "JI_SuppUQ";
			fec0.CY_Data = "ML";
			fec0.CY_Order = 1;
			fec0.CY_IsOverridden = false;

			entryHeader.FECChallenges.Load();
			AssertEquals(8, entryHeader.FECChallenges.Count);
		}

		public void TestAllowNewAndAllowRemove()
		{
			var entryheader = Factory.New<CusEntryHeader>();
			AssertEquals(false, entryheader.FECChallenges.AllowNew);
			AssertEquals(false, entryheader.FECChallenges.AllowRemove);
		}

		JobDeclaration dec;
		CusEntryHeader entryHeader;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine invoiceLine1;

		protected override void SetUp()
		{
			CusEntryLine entryLine;
			FECChallenge fec1;
			FECChallenge fec2;
			FECChallenge fec3;
			FECChallenge fec4;
			FECChallenge fec5;
			FECChallenge fec6;
			FECChallenge fec7;
			FECChallenge fec8;

			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine1 = dec.InvoiceLines.AddNew();
			entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			fec1 = Factory.New<FECChallenge>();
			fec1.CY_ParentID = entryHeader.PK;
			fec1.CY_ParentTableCode = "CH";
			fec1.CY_Code = "JE_FLG";
			fec1.CY_Data = "CN";
			fec1.CY_Order = 1;
			fec1.CY_IsOverridden = false;
			fec2 = Factory.New<FECChallenge>();
			fec2.CY_ParentID = entryHeader.PK;
			fec2.CY_ParentTableCode = "CH";
			fec2.CY_Code = "JE_DSP";
			fec2.CY_Data = "CNDSP";
			fec2.CY_Order = 1;
			fec2.CY_IsOverridden = false;
			fec3 = Factory.New<FECChallenge>();
			fec3.CY_ParentID = entryHeader.PK;
			fec3.CY_ParentTableCode = "CH";
			fec3.CY_Code = "JE_DST";
			fec3.CY_Data = "CNDST";
			fec3.CY_Order = 1;
			fec3.CY_IsOverridden = false;
			fec4 = Factory.New<FECChallenge>();
			fec4.CY_ParentID = entryLine.PK;
			fec4.CY_ParentTableCode = "CL";
			fec4.CY_Code = "JI_ORG";
			fec4.CY_Data = "CN";
			fec4.CY_Order = 1;
			fec4.CY_IsOverridden = false;
			fec5 = Factory.New<FECChallenge>();
			fec5.CY_ParentID = entryLine.PK;
			fec5.CY_ParentTableCode = "CL";
			fec5.CY_Code = "JI_NettMass";
			fec5.CY_Data = "100";
			fec5.CY_IsOverridden = false;
			fec6 = Factory.New<FECChallenge>();
			fec6.CY_ParentID = entryLine.PK;
			fec6.CY_ParentTableCode = "CL";
			fec6.CY_Code = "JI_Supp";
			fec6.CY_Data = "100";
			fec6.CY_IsOverridden = false;
			fec7 = Factory.New<FECChallenge>();
			fec7.CY_ParentID = entryLine.PK;
			fec7.CY_ParentTableCode = "CL";
			fec7.CY_Code = "JI_NettMassUQ";
			fec7.CY_Data = "KG";
			fec7.CY_IsOverridden = false;
			fec8 = Factory.New<FECChallenge>();
			fec8.CY_ParentID = entryLine.PK;
			fec8.CY_ParentTableCode = "CL";
			fec8.CY_Code = "JI_SuppUQ";
			fec8.CY_Data = "ML";
			fec8.CY_IsOverridden = false;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_RouteFRequested = true;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			declaration.InvoiceLines.AddNew().JI_CL = entryLine.PK;

			var dsp = entryHeader.FECChallenges.AddNew();
			dsp.CY_ParentTableCode = "CH";
			dsp.CY_ParentID = entryHeader.PK;
			dsp.CY_Code = FECChallengeFields.Codes.JE_DSP;
			dsp.CY_IsOverridden = true;

			return new FECChallengeCollection(Factory, entryHeader);
		}
	}
}
