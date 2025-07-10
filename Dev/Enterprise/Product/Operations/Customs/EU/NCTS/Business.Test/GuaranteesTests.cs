using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuaranteeUnderOrgHeaderCollection))]
	class CusBondDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDoesNotShowEuNctsRows()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var baseDetail1 = Factory.New<CusBondDetail>();
			var baseDetail2 = Factory.New<CusBondDetail>();
			baseDetail1.Parent = org;
			baseDetail2.Parent = org;

			var euCollection = new NctsGuaranteeUnderOrgHeaderCollection(org);
			euCollection.Load();
			Assert(!euCollection.Contains(baseDetail1));
			Assert(!euCollection.Contains(baseDetail2));

			baseDetail2.PW_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
			euCollection = new NctsGuaranteeUnderOrgHeaderCollection(org);
			euCollection.Load();
			Assert(!euCollection.Contains(baseDetail1));
			Assert(euCollection.Contains(baseDetail2));

			baseDetail1.PW_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
			euCollection = new NctsGuaranteeUnderOrgHeaderCollection(org);
			euCollection.Load();
			Assert(euCollection.Contains(baseDetail1));
			Assert(euCollection.Contains(baseDetail2));

			var euDetail = euCollection.AddNew();
			AssertEquals("NCT", euDetail.PW_ApplicationCode);
			Assert(euCollection.Contains(euDetail));
		}

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.FillWithValidTestData();
				}
				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NctsGuaranteeUnderOrgHeaderCollection(Organisation);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(NctsGuaranteeUnderOrgHeaderCollection);
		}
	}

	[TestedType(typeof(OrgHeaderWrapper))]
	class OrgHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgHeaderWrapper.New(Organisation);
		}

		public void TestCaching()
		{
			AssertEquals(OrgHeaderWrapper.New(Organisation), OrgHeaderWrapper.New(Organisation));
		}

		public void TestGuarantees()
		{
			var wrapper = OrgHeaderWrapper.New(Organisation);
			wrapper.BondDetails.Load();
			AssertEquals(0, wrapper.BondDetails.Count);
			var guarantee = Factory.New<NctsGuarantee>();
			wrapper.BondDetails.Load();
			AssertEquals(0, wrapper.BondDetails.Count);
			guarantee.Parent = wrapper.Organisation;
			AssertEquals(0, wrapper.BondDetails.Count);
			wrapper.BondDetails.Load();
			AssertEquals(1, wrapper.BondDetails.Count);
			AssertEquals(guarantee, wrapper.BondDetails[0]);
		}

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.FillWithValidTestData();
				}
				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;
	}
}
