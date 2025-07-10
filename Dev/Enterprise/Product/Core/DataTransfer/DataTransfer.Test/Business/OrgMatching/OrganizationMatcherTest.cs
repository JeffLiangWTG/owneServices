using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class OrganizationMatcherTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateForeignCodeMapping()
		{
			var mappingOrg = OrgHeader.New(Factory);
			var orgMatch = OrgHeader.New(Factory);
			var matcher = new EDICodeMappingCreater();

			AssertEquals("There are no code mappings defined", 0, Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK)).Length);

			matcher.CreateEDICodeMapping(mappingOrg, "CODE123", orgMatch);
			AssertEquals("There is one code mapping defined", 1, Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK)).Length);
			AssertEquals("Code mapping is CODE123", "CODE123", Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK))[0].OO_ForeignCode);

			matcher.CreateEDICodeMapping(mappingOrg, "CODE123", orgMatch);
			AssertEquals("There is still only one code mapping defined", 1, Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK)).Length);

			matcher.CreateEDICodeMapping(null, "", orgMatch);
			matcher.CreateEDICodeMapping(mappingOrg, "", null);
		}

		[ExpectNoExceptions]
		public void TestCreateForeignMapping_ForeignCodeContainInvalidCharacters()
		{
			var mappingOrg = OrgHeader.New(Factory);
			mappingOrg.FillWithValidTestData();
			var orgMatch = OrgHeader.New(Factory);
			orgMatch.FillWithValidTestData();

			var matcher = new EDICodeMappingCreater();
			AssertEquals("There are no code mappings defined", 0, Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK)).Length);
			matcher.CreateEDICodeMapping(mappingOrg, "HTCSSD01", orgMatch);
			AssertEquals("There is one code mapping defined", 1, Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK)).Length);
			AssertEquals("Code mapping is HTCSSD01", "HTCSSD01", Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK))[0].OO_ForeignCode);
			Factory.Save();
			AssertEquals("Should be 1 PatternMatchOverride in the Database", 1, Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride)));

			var newFactory = new BusinessObjectFactory();
			var differentMappingOrg = newFactory.Load<OrgHeader>(mappingOrg.PK);
			AssertEquals("Code mapping is HTCSS?D01", "HTCSS?D01", newFactory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, differentMappingOrg.PK))[0].OO_ForeignCode);
			matcher.CreateEDICodeMapping(differentMappingOrg, "HTCSSD01", orgMatch);
			newFactory.Save();
			AssertEquals("Should be still be only 1 PatternMatchOverride in the Database", 1, Factory.GetDatabaseCount(typeof(OrgPatternMatchOverride)));
		}
	}
}
