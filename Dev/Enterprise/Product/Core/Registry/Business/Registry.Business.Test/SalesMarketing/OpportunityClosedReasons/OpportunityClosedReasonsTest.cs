using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityClosedReasons))]
	sealed class OpportunityClosedReasonsTest : RegistryBusinessObjectTest
	{
		protected override int ExpectedDefaultMaxCodeLength => OrgOpportunitySchema.P8_LostReason.MaxLength;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new OpportunityClosedReasons();
			result.Code = "A01";
			result.EnglishDescription = "A01 - Full Description";
			result.Bool = true;

			result.StatusRules.AddNew().Code = "WON";
			result.StatusRules.AddNew().Code = "LOS";

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("A01", clone.Code);
			AssertEquals("A01 - Full Description", clone.Description);
			AssertEquals(true, ((OpportunityClosedReasons)clone).Bool);

			var rules = ((OpportunityClosedReasons)clone).StatusRules as ICodeDescriptionPairList;
			AssertEquals(2, rules.Count);
			AssertEquals(true, rules.ContainsCode("WON"));
			AssertEquals(true, rules.ContainsCode("LOS"));
		}

		public void TestRunPreSaveValidation()
		{
			var collection = new OpportunityClosedReasonsCollection();
			var r1 = collection.AddNew();
			r1.Code = "R01";
			r1.EnglishDescription = "R01 - Full Description";
			r1.StatusRules.AddNew().Code = "WON";

			var r11 = collection.AddNew();
			r11.Code = "R11";
			r11.EnglishDescription = "R11 - Full Description";

			collection.RunPreSaveValidation();
			AssertNoErrors(r1);
			AssertNoErrors(r11);

			var r2 = collection.AddNew();
			r2.Code = "R01";
			r2.EnglishDescription = "R01 - Full Description";
			var c1 = r2.StatusRules.AddNew();
			c1.Code = "WON";
			var c2 = r2.StatusRules.AddNew();
			c2.Code = "WON";
			var c3 = r2.StatusRules.AddNew();
			c3.Code = "@@@";

			var r3 = collection.AddNew();
			r3.Code = "";
			r3.EnglishDescription = "R03 - Full Description";

			collection.RunPreSaveValidation();
			AssertHasError(r2.CodeInfo, "The Code has been duplicated and must be unique.");
			AssertHasError(c2.CodeInfo, "The Code has been duplicated and must be unique.");
			AssertHasError(c3.CodeInfo, "Enter a valid selection.");
			AssertHasError(r3.CodeInfo, "Please enter a Code.");
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals(256, BizObj.DescriptionInfo.MaxLength);
		}

		public void TestStatusRules()
		{
			var opportunities1 = new OpportunityStatusCollection();
			opportunities1.Add("TS1", (NoResString)"Test1", true, true, true, "");
			var opportunities2 = new OpportunityStatusCollection();
			opportunities2.Add("TS2", (NoResString)"Test2", true, true, true, "");
			var opportunities3 = new OpportunityStatusCollection();
			opportunities3.Add("TS3", (NoResString)"Test3", true, true, true, "");

			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.OpportunityStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, opportunities1))
			using (OrganisationsDataRegistry.Instance.OpportunityStatus.SetTemporaryValue(Guid.Parse(company.PK.ToString()), Guid.Empty, Guid.Empty, opportunities2))
			using (OrganisationsDataRegistry.Instance.OpportunityStatus.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, opportunities3))
			{
				var result1 = new OpportunityClosedReasons(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
				var result2 = new OpportunityClosedReasons(new FallbackLevel(Guid.Parse(company.PK.ToString()), Guid.Empty, Guid.Empty), Factory);
				var result3 = new OpportunityClosedReasons(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

				var rules1 = result1.StatusRules.Codes;
				var rules2 = result2.StatusRules.Codes;
				var rules3 = result3.StatusRules.Codes;

				Assert(rules1.ContainsCode("TS1"));
				AssertEquals(1, rules1.Count);
				Assert(rules2.ContainsCode("TS2"));
				AssertEquals(1, rules2.Count);
				Assert(rules3.ContainsCode("TS3"));
				AssertEquals(1, rules3.Count);
			}
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OpportunityClosedReasons();
		}
	}
}
