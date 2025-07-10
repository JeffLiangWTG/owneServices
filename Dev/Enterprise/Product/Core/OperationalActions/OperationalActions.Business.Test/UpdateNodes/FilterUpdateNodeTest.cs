using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FilterUpdateNodeTest : BaseUpdateNodeTest
	{
		public void TestRootFilter()
		{
			var now = ZDateTime.Now;
			var fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(Field("", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value"), FilterParser.Parse("Z0_Code == \"TMP\"")), new DummyOperationalActionFieldValuePair(Field("", DummyBizoSchema.Z0_Date, "Date Field"), now.AddDays(5), FilterParser.Parse("Z0_Code == \"TMP\"")), };
			var mock1 = New<DummyMaster>("mock1", Moq.MockBehavior.Loose);
			mock1.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mock1.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			mock1.Setup(m => m.Z0_Code).Returns((ZString)"TMP");
			var mock2 = New<DummyMaster>("mock2", Moq.MockBehavior.Loose);
			mock2.Setup(m => m.Z0_Code).Returns((ZString)"ALP");
			var mock3 = New<DummyMaster>("mock3", Moq.MockBehavior.Loose);
			mock3.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mock3.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			mock3.Setup(m => m.Z0_Code).Returns((ZString)"TMP");
			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { mock1.Object, mock2.Object, mock3.Object });
			AssertNoExceptionThrown(() => mock1.VerifyAll());
			AssertNoExceptionThrown(() => mock2.VerifyAll());
			AssertNoExceptionThrown(() => mock3.VerifyAll());
		}

		public void TestCollectionFilter()
		{
			var now = ZDateTime.Now;
			var fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(Field("Children.", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value"), FilterParser.Parse("Children.Z0_Code == \"TMP\"")), new DummyOperationalActionFieldValuePair(Field("Children.", DummyBizoSchema.Z0_Date, "Date Field"), now.AddDays(5), FilterParser.Parse("Children.Z0_Code ==\"TMP\"")), };
			var mockSub1 = New<DummyChild>("mockSub1", Moq.MockBehavior.Loose);
			mockSub1.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mockSub1.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			mockSub1.Setup(m => m.Z0_Code).Returns((ZString)"TMP");
			var mockSub2 = New<DummyChild>("mockSub2", Moq.MockBehavior.Loose);
			mockSub2.Setup(m => m.Z0_Code).Returns((ZString)"ALP");
			var mockSub3 = New<DummyChild>("mockSub3", Moq.MockBehavior.Loose);
			mockSub3.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mockSub3.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			mockSub3.Setup(m => m.Z0_Code).Returns((ZString)"TMP");
			var mock1 = New<DummyMaster>("mock1", Moq.MockBehavior.Loose);
			mock1.Setup(m => m.Children).Returns(NewDummyChildCollection(mockSub1.Object, mockSub2.Object));
			var mock2 = New<DummyMaster>("mock2", Moq.MockBehavior.Loose);
			mock2.Setup(m => m.Children).Returns(NewDummyChildCollection(mockSub2.Object, mockSub3.Object));
			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { mock1.Object, mock2.Object });
			AssertNoExceptionThrown(() => mockSub1.VerifyAll());
			AssertNoExceptionThrown(() => mockSub2.VerifyAll());
			AssertNoExceptionThrown(() => mockSub3.VerifyAll());
			AssertNoExceptionThrown(() => mock1.VerifyAll());
			AssertNoExceptionThrown(() => mock2.VerifyAll());
		}
	}
}
