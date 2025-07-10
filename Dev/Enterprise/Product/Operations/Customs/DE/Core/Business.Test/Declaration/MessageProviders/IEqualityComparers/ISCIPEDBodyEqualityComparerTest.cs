using System;
using Enterprise.Customs.DE.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ISCIPEDBodyEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, bodyEqualityComparer.GetHashCode(originalBody));
		}

		public void TestEquals()
		{
			AssertEquals(true, bodyEqualityComparer.Equals(originalBody, BodyMock.Object));
		}

		public void TestEquals_ConsignorPK()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.ConsignorPK).Returns(new Guid("44444444-0000-0000-0000-000000000000"));
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalBody = BodyMock.Object;
			bodyEqualityComparer = new ISCIPEDBodyEqualityComparer();
		}
		ISCIPEDBody originalBody;
		ISCIPEDBodyEqualityComparer bodyEqualityComparer;

		Mock<ISCIPEDBody> BodyMock
		{
			get
			{
				var bodyMock = new Mock<ISCIPEDBody>();
				bodyMock.Setup(x => x.ConsignorPK).Returns(new Guid("11111111-0000-0000-0000-000000000000"));
				return bodyMock;
			}
		}
	}
}
