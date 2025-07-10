using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class PropertyNodeTest : BaseUpdateNodeTest
	{
		public void TestSetViaProperty()
		{
			var now = ZDateTime.Now;
			var fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(Field("Master+", DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")), new DummyOperationalActionFieldValuePair(Field("Master+", DummyBizoSchema.Z0_Date, "Date Field"), now.AddDays(5)), };
			var mockSub1 = New<DummyMaster>("mockSub1", Moq.MockBehavior.Loose);
			mockSub1.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mockSub1.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			var mockSub2 = New<DummyMaster>("mockSub2", Moq.MockBehavior.Loose);
			mockSub2.SetupProperty(m => m.Z0_VarCharMax, (ZString)"New Value");
			mockSub2.SetupProperty(m => m.Z0_Date, now.AddDays(5));
			var mock1 = New<DummyChild>("mock1", Moq.MockBehavior.Loose);
			mock1.Setup(m => m.Master).Returns(mockSub1.Object);
			var mock2 = New<DummyChild>("mock2", Moq.MockBehavior.Loose);
			mock2.Setup(m => m.Master).Returns(mockSub2.Object);
			var mock3 = New<DummyChild>("mock3", Moq.MockBehavior.Loose);
			mock3.Setup(m => m.Master).Returns(mockSub2.Object);
			var root = new RootUpdateNode(mock1.Object.GetType());
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { mock1.Object, mock2.Object, mock3.Object });
			AssertNoExceptionThrown(() => mockSub1.VerifyAll());
			AssertNoExceptionThrown(() => mockSub2.VerifyAll());
			AssertNoExceptionThrown(() => mock1.VerifyAll());
			AssertNoExceptionThrown(() => mock2.VerifyAll());
			AssertNoExceptionThrown(() => mock3.VerifyAll());
		}
	}
}
