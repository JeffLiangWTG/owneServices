namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCAHECCTariffTest : TariffTestCase
	{
		public void TestAUCAHECCChildren()
		{
			Assert(childBusinessObject.Master == businessObject1);
			businessObject1.AUCAHECCs.Remove(childBusinessObject);
			businessObject2.AUCAHECCs.Add(childBusinessObject);
			Assert(childBusinessObject.Master == businessObject2);
		}

		public void TestGetHierarchyForPartialAHECC()
		{
			var chapterHierarchy = businessObject1.GetHierarchy();
			AssertEquals("Hierarchy depth", 3, chapterHierarchy.Length);
			AssertEquals("AHECC", businessObject1, chapterHierarchy[0]);
			AssertEquals("Chapter", chapter, chapterHierarchy[1]);
			AssertEquals("Section", section, chapterHierarchy[2]);
		}

		public void TestGetHierarchyForCompleteAHECC()
		{
			var chapterHierarchy = completeAHECC.GetHierarchy();
			AssertEquals("Hierarchy depth", 5, chapterHierarchy.Length);
			AssertEquals("Complete AHECC", completeAHECC, chapterHierarchy[0]);
			AssertEquals("Child Partial AHECC", childBusinessObject, chapterHierarchy[1]);
			AssertEquals("Partial AHECC", businessObject1, chapterHierarchy[2]);
			AssertEquals("Chapter", chapter, chapterHierarchy[3]);
			AssertEquals("Section", section, chapterHierarchy[4]);
		}

		public void TestGetHierarchyForNoChapter()
		{
			var chapterHierarchy = businessObject3.GetHierarchy();
			AssertEquals("Hierarchy depth", 1, chapterHierarchy.Length);
			AssertEquals("AHECC", businessObject3, chapterHierarchy[0]);
			AssertNull(businessObject3.Chapter);
		}

		AUCAHECC businessObject1;
		AUCAHECC businessObject2;
		AUCAHECC childBusinessObject;
		AUCAHECC completeAHECC;
		AUCAHECC businessObject3;

		protected override void SetUp()
		{
			base.SetUp();

			businessObject1 = Factory.New<AUCAHECC>();
			businessObject2 = Factory.New<AUCAHECC>();

			businessObject1.UA_AHECC = "0101";
			businessObject1.UA_ShortDescription = "Top Level 1";

			businessObject2.UA_AHECC = "0202";
			businessObject2.UA_ShortDescription = "Top Level 2";

			childBusinessObject = businessObject1.AUCAHECCs.AddNew();
			childBusinessObject.UA_AHECC = "0101.10";
			childBusinessObject.UA_ShortDescription = "Sub Level 1";

			completeAHECC = childBusinessObject.AUCAHECCs.AddNew();
			completeAHECC.UA_AHECC = "0101.10.10";
			completeAHECC.UA_ShortDescription = "Complete AHECC";

			businessObject3 = Factory.New<AUCAHECC>();
			businessObject3.UA_AHECC = "abcd";
			businessObject3.UA_ShortDescription = "Top Level 3";
		}
	}
}
