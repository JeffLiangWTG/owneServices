using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Billing.Integration.Test
{
	public class FactorySnapshotTest : TestCaseWithFactory
	{
		public void TestExcept()
		{
			var anotherFactory = new BusinessObjectFactory();

			var result = new FactorySnapshot(anotherFactory).Except(new FactorySnapshot(Factory));
			Assert(!result.Any());

			var dummy = anotherFactory.New<DummyBusinessObject>();
			result = new FactorySnapshot(anotherFactory).Except(new FactorySnapshot(Factory));
			AssertEquals(1, result.Count());
			AssertEquals(dummy.PK, result.First().PK);
			AssertEquals(dummy.GetType(), result.First().Type);
			AssertEquals(ObjectStatusEnum.New, result.First().Status);
		}

		public void TestReportErrorWhenTryAddFailed()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			var mock = new Mock<FactorySnapshot>(factory) { CallBase = true };

			mock.Setup(m => m.TryAdd(It.IsAny<HashSet<ObjectSnapshot>>(), It.IsAny<ObjectSnapshot>())).Returns(false);

			ErrorReporter.Clear();
			mock.Object.InitializeObjSnapshots(factory);
			var expectedPattern = @"^Failed to add DummyBusinessObject\(HashCode:.+ PK:.+\) into HashSet ObjSnapshots, because the element is already present.$";
			var actual = ErrorReporter.LastMessageReported;     //example: Failed to add DummyBusinessObject(HashCode:53606218 PK:0adcee7b-69ce-465e-afd7-fcb4f3c2251e) into HashSet ObjSnapshots, because the element is already present.
			Assert(new Regex(expectedPattern).IsMatch(actual));
			ErrorReporter.Clear();

			mock.VerifyAll();
		}
	}

	public class ObjectSnapshotTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var objectSnapshot = new ObjectSnapshot(dummy);

			AssertEquals(ObjectStatusEnum.New, objectSnapshot.Status);
			AssertEquals(dummy.PK, objectSnapshot.PK);
			AssertEquals(dummy.GetType(), objectSnapshot.Type);
		}

		public void TestEquals()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Assert(new ObjectSnapshot(dummy).Equals(new ObjectSnapshot(dummy)));

			var snapshot1 = new ObjectSnapshot(dummy);
			Factory.Save();
			var snapshot2 = new ObjectSnapshot(dummy);
			Assert(snapshot1.Equals(snapshot2));
		}

		public void TestToString()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var objectSnapshot = new ObjectSnapshot(dummy);
			var expectedPattern = @"^DummyBusinessObject\(HashCode:.+ PK:.+\)$";
			var actual = objectSnapshot.ToString();     //example: DummyBusinessObject(HashCode:5020285 PK:a8a534f7-596d-4a71-82a9-390a8ccba210)
			Assert(new Regex(expectedPattern).IsMatch(actual));
		}
	}
}
