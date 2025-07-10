using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineSupportingInfoCollection))]
	sealed class QuarantineSupportingInfoCollectionTest : CusSupportingInfoCollectionTest<QuarantineSupportingInfo>
	{
		public new void TestSetDefaultValuesForNewChild()
		{
			var collection = GetNewCollection();
			var element = collection.AddNew();
			AssertEquals(QuarantineSupportingInfoCollection.DeclarationConstant, element.CSI_Code);
			AssertEquals(QuarantineSupportingInfoCollection.DeclarationConstant, element.CSI_Type);
			AssertEquals(collection.Master.PK, element.Parent.PK);
		}

		public new void TestCreateRelationshipFilter()
		{
			var collection = GetNewCollection();
			collection.RemoveAll();

			var element1 = collection.AddNew();
			var element2 = collection.AddNew();
			element2.CSI_Code = "~~~";

			collection.Load();
			AssertEquals("contains element1", true, collection.Contains(element1));
			AssertEquals("should not contain element2", false, collection.Contains(element2));
		}

		protected override CusSupportingInfoCollection<QuarantineSupportingInfo> GetCusSupportingInfoCollection()
		{
			var qedHeader = Factory.New<QuarantineExDocHeader>();
			return qedHeader.SupportingInfos;
		}

		QuarantineSupportingInfoCollection GetNewCollection() => (QuarantineSupportingInfoCollection)GetCusSupportingInfoCollection();
	}
}
