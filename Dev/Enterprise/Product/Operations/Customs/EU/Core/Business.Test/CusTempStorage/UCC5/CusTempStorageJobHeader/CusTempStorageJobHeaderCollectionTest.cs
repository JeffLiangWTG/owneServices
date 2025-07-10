using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>))]
	public class CusTempStorageJobHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>>
	{
		public void TestNulLBranch()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, null));
		}

		public void TestDontLoadIfCreatedInOtherCountriesOrCompanies()
		{
			var usBranch1Company1 = CreateNewCompanyAndBranch(Core.Constants.CountryCodes.UnitedStates, "USCHI");
			var usBranch2Company1 = usBranch1Company1.Company.Branches.AddNew();
			usBranch2Company1.FillWithValidTestData();
			var usBranch1Company2 = CreateNewCompanyAndBranch(Core.Constants.CountryCodes.UnitedStates, "USNYC");
			var auBranch = CreateNewCompanyAndBranch(Core.Constants.CountryCodes.Australia, "AUSYD");

			var usTempStorageJobHeader1 = CreateTempStorageJobHeader(usBranch1Company1);
			var usTempStorageJobHeader2 = CreateTempStorageJobHeader(usBranch2Company1);
			CreateTempStorageJobHeader(usBranch1Company2);
			CreateTempStorageJobHeader(auBranch);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tempStorageJobHeaderCollection = new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(newFactory, usBranch1Company1);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { usTempStorageJobHeader1.PK, usTempStorageJobHeader2.PK }, tempStorageJobHeaderCollection.Select(x => x.PK).ToArray());
		}

		GlbBranch CreateNewCompanyAndBranch(ZString countryCode, ZString firstBranchPort)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RL_NKHomePort = firstBranchPort;

			return branch;
		}

		CusTempStorageJobHeader CreateTempStorageJobHeader(GlbBranch branch)
		{
			var tempStorageJobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			tempStorageJobHeader.SJH_GB = branch.PK;
			return tempStorageJobHeader;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var tempStorageJobHeader = CreateTempStorageJobHeader(GlbBranch.CurrentBranch);
			Factory.Save();
			return tempStorageJobHeader;
		}

		protected override CusTempStorageJobHeaderCollection<CusTempStorageJobHeader> GetCollectionToTest() => new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch);
	}
}
