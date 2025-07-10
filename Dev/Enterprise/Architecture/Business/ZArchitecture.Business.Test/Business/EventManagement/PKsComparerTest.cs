using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Moq;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class PKsComparerTest : TestCaseWithFactory
	{
		public void TestEquals_LinksAreNull_ReturnTrue()
		{
			var comparer = new CascadingLinkComparer.PKsComparer();

			AssertEquals("Comparision result", true, comparer.Equals(null, null));
		}

		public void TestEquals_OneLinkIsNull_ReturnFalse()
		{
			var comparer = new CascadingLinkComparer.PKsComparer();

			AssertEquals("Comparision result", false, comparer.Equals(new CascadingLink(), null));
			AssertEquals("Comparision result", false, comparer.Equals(null, new CascadingLink()));
		}

		public void TestEquals_LinksAreEqual_ReturnTrue()
		{
			var guids = Enumerable.Range(0, 3).Select(_ => (ZGuid)Guid.NewGuid()).ToArray();
			var stmALogParent1 = new Mock<IStmALogParent>();
			var stmALogParent2 = new Mock<IStmALogParent>();
			var processTask1 = new Mock<IBaseTrigger>();
			var processTask2 = new Mock<IBaseTrigger>();
			var processTask3 = new Mock<IBaseTrigger>();
			var processTask4 = new Mock<IBaseTrigger>();

			stmALogParent1.Setup(x => x.LogsParentPK).Returns(guids[0]);
			processTask1.Setup(x => x.Identifier).Returns(guids[1]);
			processTask2.Setup(x => x.Identifier).Returns(guids[2]);

			stmALogParent2.Setup(x => x.LogsParentPK).Returns(guids[0]);
			processTask3.Setup(x => x.Identifier).Returns(guids[1]);
			processTask4.Setup(x => x.Identifier).Returns(guids[2]);

			var cascadingLink1 = new CascadingLink();
			cascadingLink1.Parent = stmALogParent1.Object;
			cascadingLink1.Triggers = new[] { processTask1.Object, processTask2.Object };

			var cascadingLink2 = new CascadingLink();
			cascadingLink2.Parent = stmALogParent2.Object;
			cascadingLink2.Triggers = new[] { processTask3.Object, processTask4.Object };

			var comparer = new CascadingLinkComparer.PKsComparer();

			AssertEquals("Comparision result", true, comparer.Equals(cascadingLink1, cascadingLink2));
		}

		public void TestEquals_LinksHaveDifferentParent_ReturnFalse()
		{
			var guids = Enumerable.Range(0, 4).Select(_ => (ZGuid)Guid.NewGuid()).ToArray();
			var stmALogParent1 = new Mock<IStmALogParent>();
			var stmALogParent2 = new Mock<IStmALogParent>();
			var processTask1 = new Mock<IBaseTrigger>();
			var processTask2 = new Mock<IBaseTrigger>();
			var processTask3 = new Mock<IBaseTrigger>();
			var processTask4 = new Mock<IBaseTrigger>();

			stmALogParent1.Setup(x => x.LogsParentPK).Returns(guids[0]);
			processTask1.Setup(x => x.Identifier).Returns(guids[1]);
			processTask2.Setup(x => x.Identifier).Returns(guids[2]);

			stmALogParent2.Setup(x => x.LogsParentPK).Returns(guids[3]);
			processTask3.Setup(x => x.Identifier).Returns(guids[1]);
			processTask4.Setup(x => x.Identifier).Returns(guids[2]);

			var cascadingLink1 = new CascadingLink();
			cascadingLink1.Parent = stmALogParent1.Object;
			cascadingLink1.Triggers = new[] { processTask1.Object, processTask2.Object };

			var cascadingLink2 = new CascadingLink();
			cascadingLink2.Parent = stmALogParent2.Object;
			cascadingLink2.Triggers = new[] { processTask3.Object, processTask4.Object };

			var comparer = new CascadingLinkComparer.PKsComparer();

			AssertEquals("Comparision result", false, comparer.Equals(cascadingLink1, cascadingLink2));
		}

		public void TestEquals_LinksHaveDifferentProcessTasksSet_ReturnFalse()
		{
			var guids = Enumerable.Range(0, 4).Select(_ => (ZGuid)Guid.NewGuid()).ToArray();
			var stmALogParent1 = new Mock<IStmALogParent>();
			var stmALogParent2 = new Mock<IStmALogParent>();
			var processTask1 = new Mock<IBaseTrigger>();
			var processTask2 = new Mock<IBaseTrigger>();
			var processTask3 = new Mock<IBaseTrigger>();
			var processTask4 = new Mock<IBaseTrigger>();

			stmALogParent1.Setup(x => x.LogsParentPK).Returns(guids[0]);
			processTask1.Setup(x => x.Identifier).Returns(guids[1]);
			processTask2.Setup(x => x.Identifier).Returns(guids[2]);

			stmALogParent2.Setup(x => x.LogsParentPK).Returns(guids[0]);
			processTask3.Setup(x => x.Identifier).Returns(guids[2]);
			processTask4.Setup(x => x.Identifier).Returns(guids[3]);

			var cascadingLink1 = new CascadingLink();
			cascadingLink1.Parent = stmALogParent1.Object;
			cascadingLink1.Triggers = new[] { processTask1.Object, processTask2.Object };

			var cascadingLink2 = new CascadingLink();
			cascadingLink2.Parent = stmALogParent2.Object;
			cascadingLink2.Triggers = new[] { processTask3.Object, processTask4.Object };

			var comparer = new CascadingLinkComparer.PKsComparer();

			AssertEquals("Comparision result", false, comparer.Equals(cascadingLink1, cascadingLink2));
		}
	}
}
