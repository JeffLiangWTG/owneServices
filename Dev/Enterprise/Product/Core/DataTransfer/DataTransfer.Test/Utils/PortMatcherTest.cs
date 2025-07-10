using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class PortMatcherTest : TestCaseWithFactory
	{
		public void TestByOrgPatternMatch()
		{
			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			orgOverride.OO_ForeignCode = "mapped_portname";
			orgOverride.OO_LocalGuid = unloco.PK;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			PortMatcher matcher = new PortMatcher(Factory, "mapped_portname", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			AssertEquals(true, matcher.Match());
			AssertEquals("Pattern match override table for RefUNLOCO", "FUBAR", matcher.Result);
			AssertEquals(0, buffer.Events.Length);
		}

		public void TestByUSCode()
		{
			RefLocoMap scheduleDlocoMap = unloco.RefLocoMaps.AddNew();
			scheduleDlocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleDlocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			scheduleDlocoMap.RY_LocalPortCode = "9999";

			RefLocoMap scheduleKlocoMap = unloco.RefLocoMaps.AddNew();
			scheduleKlocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleKlocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			scheduleKlocoMap.RY_LocalPortCode = "99999";

			RefLocoMap nonSchedulelocoMap = unloco.RefLocoMaps.AddNew();
			nonSchedulelocoMap.RY_RN = Core.Constants.CountryGuids.Australia;
			nonSchedulelocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			nonSchedulelocoMap.RY_LocalPortCode = "9998";

			PortMatcher matcher = new PortMatcher(Factory, " 999 ", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertNotEquals("FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, " 99 99 ", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertNotEquals("FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, " 9999  ", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, "9998  ", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertNotEquals("FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, "99999  ", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("FUBAR", matcher.Result);

			RefUNLOCO unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "SPOON";
			unloco2.RL_PortName = "FUBAR";

			RefLocoMap scheduleKlocoMap2 = unloco2.RefLocoMaps.AddNew();
			scheduleKlocoMap2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleKlocoMap2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			scheduleKlocoMap2.RY_LocalPortCode = "99999";

			matcher = new PortMatcher(Factory, "  9999", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, "99999", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("Should match 'FUBAR' or 'SPOON'", true, matcher.Result == "FUBAR" || matcher.Result == "SPOON");

			RefLocoMap scheduleDlocoMap2 = unloco2.RefLocoMaps.AddNew();
			scheduleDlocoMap2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleDlocoMap2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			scheduleDlocoMap2.RY_LocalPortCode = "9998";
			matcher = new PortMatcher(Factory, "  9998", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("SPOON", matcher.Result);
		}

		public void TestAllScheduleDTypePortsAreSelected()
		{
			RefLocoMap scheduleDSeaLocoMap = unloco.RefLocoMaps.AddNew();
			scheduleDSeaLocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleDSeaLocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			scheduleDSeaLocoMap.RY_LocalPortCode = "9991";

			RefLocoMap scheduleDAirLocoMap = unloco.RefLocoMaps.AddNew();
			scheduleDAirLocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleDAirLocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			scheduleDAirLocoMap.RY_LocalPortCode = "9992";

			RefLocoMap scheduleDAllLocoMap = unloco.RefLocoMaps.AddNew();
			scheduleDAllLocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleDAllLocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			scheduleDAllLocoMap.RY_LocalPortCode = "9993";

			RefLocoMap scheduleDNonSpecifiedLocoMap = unloco.RefLocoMaps.AddNew();
			scheduleDNonSpecifiedLocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleDNonSpecifiedLocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
			scheduleDNonSpecifiedLocoMap.RY_LocalPortCode = "9994";

			PortMatcher matcher = new PortMatcher(Factory, "9991", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("Schedule D Usage Type SEA", "FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, "9992", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("Schedule D Usage Type AIR", "FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, "9993", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("Schedule D Usage Type ALL", "FUBAR", matcher.Result);

			matcher = new PortMatcher(Factory, "9994", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			matcher.Match();
			AssertEquals("Schedule D Usage Type SCD", "FUBAR", matcher.Result);
		}

		public void TestByPortName()
		{
			AssertEquals("FUBAR", new PortMatcher(Factory, "something", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals(0, buffer.Events.Length);
		}

		public void TestByUNLOCO()
		{
			AssertEquals("FUBAR", new PortMatcher(Factory, "FUBAR", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals(0, buffer.Events.Length);
		}

		public void TestByUNLOCOWhereNameSameAsAnotherCode()
		{
			AssertEquals("DECLO", new PortMatcher(Factory, "DECLO", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals(0, buffer.Events.Length);
		}

		public void TestElse()
		{
			AssertEquals("IHAVE", new PortMatcher(Factory, "I have a spoon in my ear!", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals(1, buffer.Events.Length);
			AssertEquals(true, buffer.HasWarnings);
		}

		public void TestEmpty()
		{
			PortMatcher matcher = new PortMatcher(Factory, ZString.Empty, buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			AssertEquals(true, matcher.Match());
			AssertEquals(ZString.Empty, matcher.Result);
			AssertEquals(0, buffer.Events.Length);
			AssertEquals(false, buffer.HasErrors);
			AssertEquals(false, buffer.HasWarnings);
		}

		public void TestByCountryCode()
		{
			RefUNLOCO testUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_PortName, "XY"));
			if (testUnloco == null)
			{
				RefUNLOCO unloco2 = Factory.New<RefUNLOCO>();
				unloco2.RL_Code = "TSTXY";
				unloco2.RL_PortName = "XY";
				testUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_PortName, "XY"));
			}
			else
			{
				testUnloco.RL_Code = "TSTXY";
			}
			AssertEquals(testUnloco.RL_Code, "TSTXY");

			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "XY"));
			if (country == null)
			{
				country = Factory.New<RefCountry>();
				country.RN_Code = "XY";
			}

			AssertNotEquals("TSTXY", new PortMatcher(Factory, "XY", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals("XY", new PortMatcher(Factory, "XY", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals(0, buffer.Events.Length);
		}

		public void TestOrder()
		{
			AssertEquals("SPOON", new PortMatcher(Factory, "SPOON", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);

			unloco.RL_Code = "FUBAR";
			unloco.RL_PortName = "SPOON";

			RefUNLOCO unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "SPOON";
			unloco2.RL_PortName = "FUBAR";

			AssertEquals("FUBAR", new PortMatcher(Factory, "FUBAR", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals("SPOON", new PortMatcher(Factory, "SPOON", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);

			RefUNLOCO unloco3 = Factory.New<RefUNLOCO>();
			unloco3.RL_Code = "BLEAH";
			unloco3.RL_PortName = "stuff is good";

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			orgOverride.OO_ForeignCode = "SPOON";
			orgOverride.OO_LocalGuid = unloco3.PK;
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			PortMatcher matcher = new PortMatcher(Factory, "SPOON", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			AssertEquals(true, matcher.Match());
			AssertEquals("Pattern match override table for RefUNLOCO", "BLEAH", matcher.Result);

			RefLocoMap scheduleDlocoMap = unloco.RefLocoMaps.AddNew();
			scheduleDlocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleDlocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			scheduleDlocoMap.RY_LocalPortCode = "9999";

			RefLocoMap scheduleKlocoMap = unloco.RefLocoMaps.AddNew();
			scheduleKlocoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			scheduleKlocoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			scheduleKlocoMap.RY_LocalPortCode = "99999";

			unloco2.RL_Code = "9999";
			unloco3.RL_Code = "99999";
			AssertEquals("FUBAR", new PortMatcher(Factory, "9999", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
			AssertEquals("FUBAR", new PortMatcher(Factory, "99999", buffer, GlbCompany.CurrentCompany.GC_OH_OrgProxy).Result);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_PortName = "something";
			unloco.RL_Code = "FUBAR";

			buffer = new NotificationBuffer(null);
		}

		RefUNLOCO unloco;
		NotificationBuffer buffer;

		#endregion
	}
}
