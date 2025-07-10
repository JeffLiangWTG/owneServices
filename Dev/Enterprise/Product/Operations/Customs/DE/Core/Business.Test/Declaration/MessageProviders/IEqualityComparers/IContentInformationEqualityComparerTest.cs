using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IContentInformationEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, contentInfoEqualityComparer.GetHashCode(originalContentInfo));
		}

		public void TestEquals()
		{
			AssertEquals(true, contentInfoEqualityComparer.Equals(originalContentInfo, ContentInformationMock.Object));
		}

		public void TestEquals_ContentType()
		{
			var modifiedContentInfoMock = ContentInformationMock;
			modifiedContentInfoMock.Setup(x => x.ContentType).Returns("A");
			AssertEquals(false, contentInfoEqualityComparer.Equals(originalContentInfo, modifiedContentInfoMock.Object));
		}

		public void TestEquals_DegreePercentage()
		{
			var modifiedContentInfoMock = ContentInformationMock;
			modifiedContentInfoMock.Setup(x => x.DegreePercentage).Returns(12.3m);
			AssertEquals(false, contentInfoEqualityComparer.Equals(originalContentInfo, modifiedContentInfoMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalContentInfo = ContentInformationMock.Object;
			contentInfoEqualityComparer = new IContentInformationEqualityComparer();
		}
		IContentInformation originalContentInfo;
		IContentInformationEqualityComparer contentInfoEqualityComparer;

		internal static Mock<IContentInformation> ContentInformationMock
		{
			get
			{
				var contentInformationMock = new Mock<IContentInformation>();
				contentInformationMock.Setup(x => x.ContentType).Returns("C");
				contentInformationMock.Setup(x => x.DegreePercentage).Returns(45.10m);
				return contentInformationMock;
			}
		}
	}
}
