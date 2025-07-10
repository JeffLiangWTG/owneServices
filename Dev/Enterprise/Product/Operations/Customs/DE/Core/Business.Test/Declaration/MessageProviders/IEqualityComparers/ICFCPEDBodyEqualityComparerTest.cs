using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ICFCPEDBodyEqualityComparerTest : TestCase
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

		public void TestEquals_AdditionalDutyReferences()
		{
			var modifiedBodyMock = BodyMock;
			modifiedBodyMock.Setup(x => x.AdditionalDutyReferences).Returns(new IImportAdditionalDutyReference[]
			{
				GetAdditionalDutyReferenceMock("BBB", new Guid("33333333-0000-0000-0000-000000000000")).Object
			});
			AssertEquals(false, bodyEqualityComparer.Equals(originalBody, modifiedBodyMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalBody = BodyMock.Object;
			bodyEqualityComparer = new ICFCPEDBodyEqualityComparer();
		}
		ICFCPEDBody originalBody;
		ICFCPEDBodyEqualityComparer bodyEqualityComparer;

		Mock<ICFCPEDBody> BodyMock
		{
			get
			{
				var bodyMock = new Mock<ICFCPEDBody>();
				bodyMock.Setup(x => x.ConsignorPK).Returns(new Guid("11111111-0000-0000-0000-000000000000"));
				bodyMock.Setup(x => x.AdditionalDutyReferences).Returns(new IImportAdditionalDutyReference[]
				{
					GetAdditionalDutyReferenceMock("AAA", new Guid("22222222-0000-0000-0000-000000000000")).Object,
					GetAdditionalDutyReferenceMock("BBB", new Guid("33333333-0000-0000-0000-000000000000")).Object
				});
				return bodyMock;
			}
		}

		Mock<IImportAdditionalDutyReference> GetAdditionalDutyReferenceMock(string referenceNumber, Guid dutyInterestedPartyPK)
		{
			var rateMock = new Mock<IImportAdditionalDutyReference>();
			rateMock.Setup(a => a.ReferenceNumber).Returns(referenceNumber);
			rateMock.Setup(a => a.DutyInterestedPartyPK).Returns(dutyInterestedPartyPK);
			return rateMock;
		}
	}
}
