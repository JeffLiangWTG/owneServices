using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IImportAdditionalDutyReferenceEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, importAdditionalDutyReferenceEqualityComparer.GetHashCode(originalImportAdditionalDutyReference));
		}

		public void TestEquals()
		{
			AssertEquals(true, importAdditionalDutyReferenceEqualityComparer.Equals(originalImportAdditionalDutyReference, ImportAdditionalDutyReferenceMock.Object));
		}

		public void TestEquals_NullString()
		{
			var modifiedAmountMock = ImportAdditionalDutyReferenceMock;
			modifiedAmountMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			AssertNoExceptionThrown(() => importAdditionalDutyReferenceEqualityComparer.Equals(modifiedAmountMock.Object, originalImportAdditionalDutyReference));
		}

		public void TestEquals_ReferenceNumber()
		{
			var modifiedAmountMock = ImportAdditionalDutyReferenceMock;
			modifiedAmountMock.Setup(x => x.ReferenceNumber).Returns("BBB");
			AssertEquals(false, importAdditionalDutyReferenceEqualityComparer.Equals(originalImportAdditionalDutyReference, modifiedAmountMock.Object));
		}

		public void TestEquals_DutyInterestedPartyPK()
		{
			var modifiedAmountMock = ImportAdditionalDutyReferenceMock;
			modifiedAmountMock.Setup(x => x.DutyInterestedPartyPK).Returns(new Guid("22222222-0000-0000-0000-000000000000"));
			AssertEquals(false, importAdditionalDutyReferenceEqualityComparer.Equals(originalImportAdditionalDutyReference, modifiedAmountMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalImportAdditionalDutyReference = ImportAdditionalDutyReferenceMock.Object;
			importAdditionalDutyReferenceEqualityComparer = new IImportAdditionalDutyReferenceEqualityComparer();
		}
		IImportAdditionalDutyReference originalImportAdditionalDutyReference;
		IImportAdditionalDutyReferenceEqualityComparer importAdditionalDutyReferenceEqualityComparer;

		Mock<IImportAdditionalDutyReference> ImportAdditionalDutyReferenceMock
		{
			get
			{
				var amountMock = new Mock<IImportAdditionalDutyReference>();
				amountMock.Setup(x => x.ReferenceNumber).Returns("AAA");
				amountMock.Setup(x => x.DutyInterestedPartyPK).Returns(new Guid("11111111-0000-0000-0000-000000000000"));
				return amountMock;
			}
		}
	}
}
