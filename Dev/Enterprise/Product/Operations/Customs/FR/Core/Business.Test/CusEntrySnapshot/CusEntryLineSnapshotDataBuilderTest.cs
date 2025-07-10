using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Interfaces.Snapshot;
using Moq;

namespace Enterprise.Customs.FR.Business.Snapshot.Testing
{
	public class EntrySnapshotDataBuilderTest : TestCaseWithFactory
	{
		public void TestGetSnapshot()
		{
			var expectedSnapshot = @"
<?xml version=""1.0"" encoding=""utf-16""?>
<FrenchEntryLineChildSnapshot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<CusEntryLine>
<LineNumber>1</LineNumber>
<ChildData Type=""CAN"">
<Code>V319</Code>
</ChildData>
<ChildData Type=""CAC"">
<Code>V305</Code>
</ChildData>
</CusEntryLine>
<CusEntryLine>
<LineNumber>2</LineNumber>
<ChildData Type=""DOC"">
<Code>V319</Code>
<Reference>XZ2828</Reference>
</ChildData>
</CusEntryLine>
</FrenchEntryLineChildSnapshot>";
			AssertXMLContains(expectedSnapshot.Replace(System.Environment.NewLine, ""), snapshotBuilder.GetSnapshot());
		}

		ISnapshottedCusEntryLine[] CreateSnapshotData()
		{
			var childMock1 = new Mock<IChildData>();
			childMock1.Setup(m => m.Code).Returns("V319");
			childMock1.Setup(m => m.Type).Returns(ChildType.CAN);
			childMock1.Setup(m => m.Reference).Returns("");

			var childMock2 = new Mock<IChildData>();
			childMock2.Setup(m => m.Code).Returns("V305");
			childMock2.Setup(m => m.Type).Returns(ChildType.CAC);
			childMock2.Setup(m => m.Reference).Returns("");

			var lineMock1 = new Mock<ISnapshottedCusEntryLine>();
			lineMock1.Setup(m => m.LineNumber).Returns(1);
			lineMock1.Setup(m => m.Children).Returns(new List<IChildData> { childMock1.Object, childMock2.Object }.ToArray());

			var childMock3 = new Mock<IChildData>();
			childMock3.Setup(m => m.Code).Returns("V319");
			childMock3.Setup(m => m.Type).Returns(ChildType.DOC);
			childMock3.Setup(m => m.Reference).Returns("XZ2828");

			var lineMock2 = new Mock<ISnapshottedCusEntryLine>();
			lineMock2.Setup(m => m.LineNumber).Returns(2);
			lineMock2.Setup(m => m.Children).Returns(new List<IChildData> { childMock3.Object }.ToArray());

			var snapshot = new List<ISnapshottedCusEntryLine> { lineMock1.Object, lineMock2.Object }.ToArray();

			return snapshot;
		}

		protected override void SetUp()
		{
			snapshotBuilder = new CusEntryLineSnapshotDataBuilder(CreateSnapshotData());
		}

		CusEntryLineSnapshotDataBuilder snapshotBuilder;
	}
}
