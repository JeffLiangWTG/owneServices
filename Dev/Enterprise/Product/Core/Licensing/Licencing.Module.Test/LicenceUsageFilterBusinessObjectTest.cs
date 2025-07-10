using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Licencing.Module.Testing
{
	[TestedType(typeof(LicenceUsageFilterBusinessObject))]
	sealed class LicenceUsageFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterByLicenceType()
		{
			LicenceUsageLog nonLog = CreateLog(ModuleLicenceType.NON);
			LicenceUsageLog triLog = CreateLog(ModuleLicenceType.TRI);
			LicenceUsageLog purLog = CreateLog(ModuleLicenceType.PUR);
			LicenceUsageLog renLog = CreateLog(ModuleLicenceType.REN);
			LicenceUsageLog odmLog = CreateLog(ModuleLicenceType.ODM);
			LicenceUsageLog otmLog = CreateLog(ModuleLicenceType.OTM);
			LicenceUsageLog opnLog = CreateLog(ModuleLicenceType.OPN);
			LicenceUsageLog sruLog = CreateLog(ModuleLicenceType.SRU);

			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			AssertEquals(8, Factory.Load<LicenceUsageLog>(query).Length);
			AssertFilterByLicenceType(LicenceTypes.Codes.NON, nonLog);
			AssertFilterByLicenceType(LicenceTypes.Codes.TRI, triLog);
			AssertFilterByLicenceType(LicenceTypes.Codes.PUR, purLog);
			AssertFilterByLicenceType(LicenceTypes.Codes.REN, renLog);
			AssertFilterByLicenceType(LicenceTypes.Codes.ODM, odmLog);
			AssertFilterByLicenceType(LicenceTypes.Codes.OTM, otmLog);
			AssertFilterByLicenceType(LicenceTypes.Codes.OPN, opnLog);
			AssertFilterByLicenceType(LicenceTypes.Codes.SRU, sruLog);
		}

		void AssertFilterByLicenceType(string licenceType, params LicenceUsageLog[] expectedLogs)
		{
			LicenceUsageFilterBusinessObject filterBizO = (LicenceUsageFilterBusinessObject)CachedBusinessObject;
			filterBizO["Licence Type"].IsActive = true;
			((ModuleTextFilter)filterBizO["Licence Type"]).Property = licenceType;
			ZQuery query = new ZQuery();
			query.AddToFilter(filterBizO.Filter);
			query.FetchOnlyFromLocalCache = true;
			LicenceUsageLog[] logsFound = Factory.Load<LicenceUsageLog>(query);
			AssertEquals(expectedLogs.Length, logsFound.Length);
			foreach (LicenceUsageLog expectedLog in expectedLogs)
			{
				AssertCollectionContains(expectedLog, logsFound);
			}
		}

		LicenceUsageLog CreateLog(ModuleLicenceType licenceType)
		{
			LicenceUsageLog result = Factory.New<LicenceUsageLog>();
			result.S7_MouseClicks = (int)licenceType;
			return result;
		}

		public void TestLicenceTypeFilterValidation()
		{
			LicenceUsageFilterBusinessObject filterBizO = (LicenceUsageFilterBusinessObject)CachedBusinessObject;
			filterBizO["Licence Type"].IsActive = true;
			var licenceTypeFilter = ((ModuleTextFilter)filterBizO["Licence Type"]);
			licenceTypeFilter.Property = "blah";
			AssertHasError(licenceTypeFilter.PropertyInfo, "Enter a valid selection.");
		}

		public void TestFilterbyLocalUsageTime()
		{
			LicenceUsageLog log1 = Factory.NewWithValidTestData<LicenceUsageLog>();
			log1.S7_OpenDateTimeUtc = new ZDateTime(2013, 9, 6, 1, 0, 0);
			LicenceUsageLog log2 = Factory.NewWithValidTestData<LicenceUsageLog>();
			log2.S7_OpenDateTimeUtc = new ZDateTime(2013, 2, 21, 1, 30, 0);

			LicenceUsageFilterBusinessObject filterBizO = (LicenceUsageFilterBusinessObject)CachedBusinessObject;

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			using (branch1.SetAsTemporaryContext())
			{
				filterBizO["Usage Time (Local)"].IsActive = true;
				((ModuleDateFilter)filterBizO["Usage Time (Local)"]).PropertySearch = "Time range";
				((ModuleDateFilter)filterBizO["Usage Time (Local)"]).Property1 = new ZDateTime(2013, 9, 6, 10, 0, 0);
				((ModuleDateFilter)filterBizO["Usage Time (Local)"]).Property2 = new ZDateTime(2013, 9, 6, 12, 0, 0);

				ZQuery query = new ZQuery();
				query.AddToFilter(filterBizO.Filter);
				query.FetchOnlyFromLocalCache = true;
				LicenceUsageLog[] logsFound = Factory.Load<LicenceUsageLog>(query);
				AssertEquals(1, logsFound.Length);
			}

			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_RL_NKHomePort = "MKSKP";
			Factory.Save();
			using (branch2.SetAsTemporaryContext())
			{
				filterBizO["Usage Time (Local)"].IsActive = true;
				((ModuleDateFilter)filterBizO["Usage Time (Local)"]).PropertySearch = "Time range";
				((ModuleDateFilter)filterBizO["Usage Time (Local)"]).Property1 = new ZDateTime(2013, 2, 21, 2, 0, 0);
				((ModuleDateFilter)filterBizO["Usage Time (Local)"]).Property2 = new ZDateTime(2013, 2, 21, 10, 0, 0);

				ZQuery query = new ZQuery();
				query.AddToFilter(filterBizO.Filter);
				query.FetchOnlyFromLocalCache = true;
				LicenceUsageLog[] logsFound = Factory.Load<LicenceUsageLog>(query);
				AssertEquals(1, logsFound.Length);
			}
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LicenceUsageFilterBusinessObject();
		}

		#endregion
	}
}
