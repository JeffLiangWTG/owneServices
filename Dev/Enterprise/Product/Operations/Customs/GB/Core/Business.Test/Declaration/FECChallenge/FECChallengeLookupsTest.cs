using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class FECChallengeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusCodeDataTypes()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.InvoiceLines.AddNew();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var fec = Factory.New<FECChallenge>();
			fec.CY_ParentID = entryHeader.PK;
			fec.CY_ParentTableCode = "CH";
			fec.CY_Code = "JE_FLG";
			fec.CY_Data = "CN";
			fec.CY_Order = 1;
			fec.CY_IsOverridden = false;
			Assert(fec.Lookups.CusCodeDataTypes.Count > 0);
			Assert(fec.Lookups.CusCodeDataTypes.Contains(new CodeDescriptionPair("FEC", "Front-end credibility challenge")));
		}

		public void TestNewValueList()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.InvoiceLines.AddNew();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var fec1 = Factory.New<FECChallenge>();
			fec1.CY_ParentID = entryHeader.PK;
			fec1.CY_ParentTableCode = "CH";
			fec1.CY_Code = "JE_FLG";
			fec1.CY_Data = "CN";
			fec1.CY_Order = 1;
			fec1.CY_IsOverridden = false;
			var fec2 = Factory.New<FECChallenge>();
			fec2.CY_ParentID = entryHeader.PK;
			fec2.CY_ParentTableCode = "CH";
			fec2.CY_Code = "JE_DSP";
			fec2.CY_Data = "CNDSP";
			fec2.CY_Order = 1;
			fec2.CY_IsOverridden = false;
			var fec3 = Factory.New<FECChallenge>();
			fec3.CY_ParentID = entryHeader.PK;
			fec3.CY_ParentTableCode = "CH";
			fec3.CY_Code = "JE_DST";
			fec3.CY_Data = "CNDST";
			fec3.CY_Order = 1;
			fec3.CY_IsOverridden = false;
			var fec4 = Factory.New<FECChallenge>();
			fec4.CY_ParentID = entryLine.PK;
			fec4.CY_ParentTableCode = "CL";
			fec4.CY_Code = "JI_ORG";
			fec4.CY_Data = "CN";
			fec4.CY_Order = 1;
			fec4.CY_IsOverridden = false;
			var fec5 = Factory.New<FECChallenge>();
			fec5.CY_ParentID = entryLine.PK;
			fec5.CY_ParentTableCode = "CL";
			fec5.CY_Code = "JI_NettMass";
			fec5.CY_Data = "100";
			fec5.CY_IsOverridden = false;
			var fec6 = Factory.New<FECChallenge>();
			fec6.CY_ParentID = entryLine.PK;
			fec6.CY_ParentTableCode = "CL";
			fec6.CY_Code = "JI_Supp";
			fec6.CY_Data = "100";
			fec6.CY_IsOverridden = false;
			var fec7 = Factory.New<FECChallenge>();
			fec7.CY_ParentID = entryLine.PK;
			fec7.CY_ParentTableCode = "CL";
			fec7.CY_Code = "JI_NettMassUQ";
			fec7.CY_Data = "KG";
			fec7.CY_IsOverridden = false;
			var fec8 = Factory.New<FECChallenge>();
			fec8.CY_ParentID = entryLine.PK;
			fec8.CY_ParentTableCode = "CL";
			fec8.CY_Code = "JI_SuppUQ";
			fec8.CY_Data = "ML";
			fec8.CY_IsOverridden = false;
			AssertEquals(dec.Lookups.TransportCountryList.GetType(), fec1.Lookups.NewValueList.GetType());
			AssertEquals(dec.Lookups.Origins.GetType(), fec2.Lookups.NewValueList.GetType());
			AssertEquals(dec.Lookups.FinalDestinations.GetType(), fec3.Lookups.NewValueList.GetType());
			AssertEquals(invoiceLine.Lookups.CountryList.GetType(), fec4.Lookups.NewValueList.GetType());
			AssertEquals(new CodeDescriptionPairList().GetType(), fec5.Lookups.NewValueList.GetType());
			AssertEquals(new CodeDescriptionPairList().GetType(), fec6.Lookups.NewValueList.GetType());
			AssertEquals(invoiceLine.Lookups.WeightUQList.GetType(), fec7.Lookups.NewValueList.GetType());
			AssertEquals(invoiceLine.Lookups.CustomsUQList.GetType(), fec8.Lookups.NewValueList.GetType());
		}

		public void TestFECChallengeFields()
		{
			var testList = Factory.GetCachedValue<FECChallengeFields>();
			Assert("Should have contained the new items", testList.ContainsCode(FECChallengeFields.Codes.JI_Price));
		}
	}
}
