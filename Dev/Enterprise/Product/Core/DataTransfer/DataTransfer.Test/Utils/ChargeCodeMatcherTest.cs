using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class ChargeCodeMatcherTest : TestCaseWithFactory
	{
		public void TestMatch()
		{
			SetupMappings();
			Factory.Save();

			AssertMatch("Direct Reference", FRT, "FRT");
			AssertMatch("Pattern Override", FRT, "Freight");
			AssertMatch("Broken Override", null, "Fread");
			AssertMatch("Random Crap", null, "Blat");
		}

		#region Implementation

		void AssertMatch(string message, AccChargeCode expectedChargeCode, string code)
		{
			NotificationBuffer buffer = new NotificationBuffer();
			ChargeCodeMatcher matcher = new ChargeCodeMatcher(Factory, MappingOrg.PK, code, buffer);

			if (expectedChargeCode == null)
			{
				AssertEquals(message + ": should not match", false, matcher.Match());
				AssertEquals(message + ": should have an error", true, buffer.HasErrors);
			}
			else
			{
				AssertEquals(message + ": should match", expectedChargeCode.PK, matcher.Result);
				AssertEquals(message + ": should not have an error", false, buffer.HasErrors);
			}
		}

		AccChargeCode FRT
		{
			get
			{
				if (frt == null)
				{
					ZQuery filter = new ZQuery();
					filter.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
					filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

					frt = Factory.LoadTop1<AccChargeCode>(filter);
				}
				return frt;
			}
		}
		AccChargeCode frt;

		OrgHeader MappingOrg
		{
			get
			{
				if (mappingOrg == null)
				{
					SetupMappings();
				}
				return mappingOrg;
			}
		}
		OrgHeader mappingOrg;

		void SetupMappings()
		{
			mappingOrg = Factory.New<OrgHeader>();
			mappingOrg.OH_Code = "MAPPORG";
			mappingOrg.OH_FullName = "Mapping Org";

			var matchOverride1 = mappingOrg.CreatePatternMatchOverrideForTest();
			matchOverride1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			matchOverride1.OO_ForeignCode = "Freight";
			matchOverride1.OO_LocalCode = "FRT";

			var matchOverride2 = mappingOrg.CreatePatternMatchOverrideForTest();
			matchOverride2.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			matchOverride2.OO_ForeignCode = "Frank";
			matchOverride2.OO_LocalCode = "XXX";
		}

		#endregion
	}
}
