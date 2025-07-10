using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Module.Testing
{
	public class DeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestDeferredStatus()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddRange(addInfoLookups.DeferredSubmissionList);
			expectedList.AddPair(DeclarationFilterLookups.DeferredCodes.All, "Show all records");
			AssertContainsExactElementsInAnyOrder(expectedList, filterLookups.DeferredStatus);
		}

		public void TestDeclarantTypes()
		{
			AssertContainsExactElementsInAnyOrder(declarationLookups.DeclarantTypeList, filterLookups.DeclarantTypes);
		}

		public void TestDestinationTypes()
		{
			AssertContainsExactElementsInAnyOrder(declarationLookups.MessageSubTypeList, filterLookups.DestinationTypes);
		}

		public void TestGuarantorTypes()
		{
			AssertContainsExactElementsInAnyOrder(addInfoLookups.GuarantorTypeList, filterLookups.GuarantorTypes);
		}

		public void TestOriginTypes()
		{
			AssertContainsExactElementsInAnyOrder(addInfoLookups.OriginTypeList, filterLookups.OriginTypes);
		}

		public void TestTransportArrangements()
		{
			AssertContainsExactElementsInAnyOrder(addInfoLookups.TransportArrangementList, filterLookups.TransportArrangements);
		}

		public void TestTransportModes()
		{
			AssertContainsExactElementsInAnyOrder(declarationLookups.TransportTypeList, filterLookups.TransportModes);
		}

		public void TestEntryStatusList()
		{
			AssertContainsExactElementsInAnyOrder(declarationLookups.EntryStatusList, filterLookups.EntryStatusList());
		}

		public void TestMessageStatusList()
		{
			AssertContainsExactElementsInAnyOrder(declarationLookups.MessageStatusList, filterLookups.MessageStatusList());
		}

		public void TestAllOrganisations()
		{
			AssertType<OrgHeaderCollection>(filterLookups.AllOrganisations);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.GetNull<EMCSJobDeclaration>();
			filterLookups = new DeclarationFilterLookups(new FilterStripBusinessObject());
			declarationLookups = new EMCSJobDeclarationLookups(declaration);
			addInfoLookups = declaration.AddInfoLookups;
		}
		DeclarationFilterLookups filterLookups;
		EMCSJobDeclarationLookups declarationLookups;
		EMCSAddInfoJobDeclarationLookups addInfoLookups;
	}
}
