using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementLinkCollection))]
	public class IncidentManagementLinkCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddShouldUpdateINL_GS_NKResponder()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var collection1 = new IncidentManagementLinkCollection(Factory);
			var collection2 = new IncidentManagementLinkCollection(Factory);
			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_ING_Group = group1.PK;
			link2.INL_ING_Group = group1.PK;
			link3.INL_ING_Group = group2.PK;

			group1.ING_GS_NKGroupOwner = staff1.GS_Code;
			group2.ING_GS_NKGroupOwner = string.Empty;
			link1.INL_GS_NKResponder = string.Empty;
			link2.INL_GS_NKResponder = staff2.GS_Code;
			link3.INL_GS_NKResponder = string.Empty;

			collection1.Add(link1);
			collection1.Add(link2);
			collection2.Add(link3);

			AssertEquals("Should update to match the group's owner", staff1.GS_Code, link1.INL_GS_NKResponder);
			AssertEquals("Should remain unchanged", staff2.GS_Code, link2.INL_GS_NKResponder);
			AssertEquals("Should remain unchanged", string.Empty, link3.INL_GS_NKResponder);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IncidentManagementLinkCollection(Factory);
		}
	}
}
