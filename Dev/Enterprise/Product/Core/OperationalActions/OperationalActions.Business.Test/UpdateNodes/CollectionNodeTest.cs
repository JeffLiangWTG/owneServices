using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class CollectionNodeTest : BaseUpdateNodeTest
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetViaCollection()
		{
			var now = ZDateTime.Now;
			IOperationalActionFieldValuePair[] fieldValuePairs = { new DummyOperationalActionFieldValuePair(Field("Children.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")), new DummyOperationalActionFieldValuePair(Field("Children.", DummyBizoSchema.Z0_Date, "Date Field"), now.AddDays(5)), new DummyOperationalActionFieldValuePair(Field("Children.", DummyBizoSchema.Z0_DateTimeOffset, "DateTimeOffset Field"), new ZDateTimeOffset(now.AddDays(5))), new DummyOperationalActionFieldValuePair(Field("Children.", DummyBizoSchema.Z0_Geography, "Geography Field"), new ZGeography("POINT (-121 48)")), };
			var mockSub1 = New<DummyChild>("mockSub1", MockBehavior.Loose);
			mockSub1.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mockSub1.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			mockSub1.SetupProperty(m => m.Z0_DateTimeOffset, new ZDateTimeOffset(now.AddDays(5)));
			mockSub1.SetupProperty(m => m.Z0_Geography, new ZGeography("POINT (-121 48)"));
			var mockSub2 = New<DummyChild>("mockSub2", MockBehavior.Loose);
			mockSub2.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mockSub2.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			mockSub2.SetupProperty(m => m.Z0_DateTimeOffset, new ZDateTimeOffset(now.AddDays(5)));
			mockSub2.SetupProperty(m => m.Z0_Geography, new ZGeography("POINT (-121 48)"));
			var mockSub3 = New<DummyChild>("mockSub3", MockBehavior.Loose);
			mockSub3.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mockSub3.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			mockSub3.SetupProperty(m => m.Z0_DateTimeOffset, new ZDateTimeOffset(now.AddDays(5)));
			mockSub3.SetupProperty(m => m.Z0_Geography, new ZGeography("POINT (-121 48)"));
			var mock1 = New<DummyMaster>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(NewDummyChildCollection(mockSub1.Object, mockSub2.Object));
			var mock2 = New<DummyMaster>("mock2", MockBehavior.Loose);
			mock2.Setup(m => m.Children).Returns(NewDummyChildCollection(mockSub2.Object, mockSub3.Object));
			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			var affectedTargets = root.Apply(new BusinessObject[] { mock1.Object, mock2.Object });
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { mock1.Object, mock2.Object, mockSub1.Object, mockSub2.Object, mockSub3.Object }, affectedTargets);
			mockSub1.VerifyAll();
			mockSub2.VerifyAll();
			mockSub3.VerifyAll();
			mock1.VerifyAll();
			mock2.VerifyAll();
		}

		public void TestApplyCollectionUpdateNode()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs = { new DummyOperationalActionFieldValuePair(Field("Children.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")), };
			var mock1 = New<DummyMaster>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns((DummyChildCollection)null);
			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(new BusinessObject[] { mock1.Object }); });
			mock1.VerifyAll();
		}

		public void TestApplyNothingToCollectionUpdateNode()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs =
			{
				new DummyOperationalActionFieldValuePair(Field("Children.ChildDummyCollectionOne.ChildDummyCollectionTwo.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")),
			};

			var outerMostCollection = new DummyWithMultiNestedCollections(Factory);
			var middleCollection = new DummyWithAnotherNestedCollection(Factory);
			var innerCollection = new DummyChildCollection(Factory);

			var dummyChild = innerCollection.AddNew();
			outerMostCollection.ChildDummyCollectionOne = middleCollection;
			middleCollection.ChildDummyCollectionTwo = innerCollection;

			var mock1 = New<DummyMasterWithMultiNestedCollections>("mock1", MockBehavior.Loose);

			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(System.Array.Empty<BusinessObject>()); });
			mock1.Verify(m => m.Children, Times.Never);
		}

		public void TestApplyToMultiNestedCollection()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs =
			{
				new DummyOperationalActionFieldValuePair(Field("Children.ChildDummyCollectionOne.ChildDummyCollectionTwo.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")),
				new DummyOperationalActionFieldValuePair(Field("MoreChildren.ChildDummyCollectionOne.ChildDummyCollectionTwo.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value"))
			};

			var innerObject = Factory.New<DummyBusinessObject>();
			var outerMostCollection = new DummyWithMultiNestedCollections(Factory);
			var middleCollection = new DummyWithAnotherNestedCollection(Factory);
			var innerCollection = new DummyChildCollection(Factory);

			var dummyChild = innerCollection.AddNew();
			outerMostCollection.ChildDummyCollectionOne = middleCollection;
			middleCollection.ChildDummyCollectionTwo = innerCollection;

			var otherInnerObject = Factory.New<DummyBusinessObject>();
			var otherOuterMostCollection = new DummyWithMultiNestedCollections(Factory);
			var otherMiddleCollection = new DummyWithAnotherNestedCollection(Factory);
			var otherInnerCollection = new DummyChildCollection(Factory);

			var otherDummyChild = otherInnerCollection.AddNew();
			otherOuterMostCollection.ChildDummyCollectionOne = otherMiddleCollection;
			otherMiddleCollection.ChildDummyCollectionTwo = otherInnerCollection;

			var mock1 = New<DummyMasterWithMultiNestedCollections>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(outerMostCollection);
			mock1.Setup(m => m.MoreChildren).Returns(otherOuterMostCollection);

			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(new BusinessObject[] { mock1.Object }); });
			mock1.VerifyAll();
			AssertEquals("Property was updated correctly", "New Value", dummyChild.Z0_VarCharMax);
			AssertEquals("Property was updated correctly", "New Value", otherDummyChild.Z0_VarCharMax);
		}

		public void TestApplyToBizoOnCollection()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs =
			{
				new DummyOperationalActionFieldValuePair(Field("Children.ChildDummyCollectionOne.ChildDummyCollectionTwo.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")),
				new DummyOperationalActionFieldValuePair(Field("MoreChildren.DummyChildBizO.ChildDummyCollection.ChildDummyCollectionTwo.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value"))
			};

			var outerMostCollection = new DummyWithMultiNestedCollections(Factory);
			var middleCollection = new DummyWithAnotherNestedCollection(Factory);
			var innerCollection = new DummyChildCollection(Factory);

			var dummyChild = innerCollection.AddNew();
			outerMostCollection.ChildDummyCollectionOne = middleCollection;
			middleCollection.ChildDummyCollectionTwo = innerCollection;

			var otherOuterMostCollection = new DummyWithMultiNestedCollections(Factory);
			var otherMiddleCollection = new DummyWithAnotherNestedCollection(Factory);
			var otherInnerCollection = new DummyChildCollection(Factory);
			var otherDummyBizO = New<DummyChild>("mockDummyChild", MockBehavior.Loose);

			var otherDummyChild = innerCollection.AddNew();
			otherOuterMostCollection.DummyChildBizO = otherDummyBizO.Object;
			otherMiddleCollection.ChildDummyCollectionTwo = otherInnerCollection;
			otherDummyBizO.Setup(o => o.ChildDummyCollection).Returns(otherMiddleCollection);

			var mock1 = New<DummyMasterWithMultiNestedCollections>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(outerMostCollection);
			mock1.Setup(m => m.MoreChildren).Returns(otherOuterMostCollection);

			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(new BusinessObject[] { mock1.Object }); });
			mock1.VerifyAll();
			AssertEquals("Property was updated correctly", "New Value", dummyChild.Z0_VarCharMax);
			AssertEquals("Property was updated correctly", "New Value", otherDummyChild.Z0_VarCharMax);
		}

		public void TestCollectionAndElementsHaveSubCollectionsWithSameName()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs =
			{
				new DummyOperationalActionFieldValuePair(Field("Children.DummyDuplicateOne.DummyDuplicateTwo.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")),
			};

			var outerMostCollection = new DummyWithMultiNestedCollections(Factory);
			var middleCollection = new DummyWithAnotherNestedCollection(Factory);
			var innerCollection = new DummyChildCollection(Factory);

			var outerBizO = outerMostCollection.AddNew();
			outerBizO.DummyDuplicateOne = middleCollection;
			var middleBizO = middleCollection.AddNew();
			middleBizO.DummyDuplicateTwo = innerCollection;
			var innerBizO = innerCollection.AddNew();

			var mock1 = New<DummyMasterWithMultiNestedCollections>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(outerMostCollection);
			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(new BusinessObject[] { mock1.Object }); });
			mock1.VerifyAll();
			AssertEquals("Property was updated correctly", "New Value", innerBizO.Z0_VarCharMax);
		}

		public void TestCollectionAndElementsHavePropertiesWithSameName()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs =
{
				new DummyOperationalActionFieldValuePair(Field("Children.DummyDuplicateBizOOne.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")),
			};

			var outerMostCollection = new DummyWithMultiNestedCollections(Factory);
			var outerBizO = outerMostCollection.AddNew();
			outerBizO.DummyDuplicateBizOOne = Factory.New<DummyChild>();

			var mock1 = New<DummyMasterWithMultiNestedCollections>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(outerMostCollection);
			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(new BusinessObject[] { mock1.Object }); });
			mock1.VerifyAll();
			AssertEquals("Property was updated correctly", "New Value", outerBizO.DummyDuplicateBizOOne.Z0_VarCharMax);
		}

		public void TestMoreNodesAfterFinalCollectionNode()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs =
			{
				new DummyOperationalActionFieldValuePair(Field("Children.ChildDummyCollectionOne.ChildDummyCollectionTwo.DummyDuplicateBizOOne.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")),
			};

			var outerMostCollection = new DummyWithMultiNestedCollections(Factory);
			var middleCollection = new DummyWithAnotherNestedCollection(Factory);
			var innerCollection = new DummyChildCollection(Factory);

			var dummyChild = innerCollection.AddNew();
			var dummyGrandChild = Factory.New<DummyChild>();
			dummyChild.DummyDuplicateBizOOne = dummyGrandChild;
			outerMostCollection.ChildDummyCollectionOne = middleCollection;
			middleCollection.ChildDummyCollectionTwo = innerCollection;

			var mock1 = New<DummyMasterWithMultiNestedCollections>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(outerMostCollection);

			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(new BusinessObject[] { mock1.Object }); });
			mock1.VerifyAll();
			AssertEquals("Property was updated correctly", "New Value", dummyGrandChild.Z0_VarCharMax);
		}

		public void TestCollectionOnBizOAfterCollectionOnCollectionChain()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs =
			{
				new DummyOperationalActionFieldValuePair(Field("Children.ChildDummyCollectionOne.ChildDummyCollectionTwo.DummyFinalCollection.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")),
			};

			var outerMostCollection = new DummyWithMultiNestedCollections(Factory);
			var middleCollection = new DummyWithAnotherNestedCollection(Factory);
			var innerCollection = new DummyChildCollection(Factory);
			var finalCollection = new DummyChildCollection(Factory);

			outerMostCollection.ChildDummyCollectionOne = middleCollection;
			middleCollection.ChildDummyCollectionTwo = innerCollection;
			var innerObject = innerCollection.AddNew();
			innerObject.DummyFinalCollection = finalCollection;
			var finalObject = finalCollection.AddNew();

			var mock1 = New<DummyMasterWithMultiNestedCollections>("mock1", MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(outerMostCollection);

			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			AssertNoExceptionThrown(() => { root.Apply(new BusinessObject[] { mock1.Object }); });
			mock1.VerifyAll();
			AssertEquals("Property was updated correctly", "New Value", finalObject.Z0_VarCharMax);
		}
	}
}
