using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IImportSpecificRateEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, importSpecificRateEqualityComparer.GetHashCode(originalSpecificRate));
		}

		public void TestEquals()
		{
			AssertEquals(true, importSpecificRateEqualityComparer.Equals(originalSpecificRate, ImportSpecificRate.Object));
		}

		public void TestEquals_Type()
		{
			var modifiedImportPackageMock = ImportSpecificRate;
			modifiedImportPackageMock.Setup(x => x.Type).Returns("Y");
			AssertEquals(false, importSpecificRateEqualityComparer.Equals(originalSpecificRate, modifiedImportPackageMock.Object));
		}

		public void TestEquals_Value()
		{
			var modifiedImportPackageMock = ImportSpecificRate;
			modifiedImportPackageMock.Setup(x => x.Value).Returns(9.02m);
			AssertEquals(false, importSpecificRateEqualityComparer.Equals(originalSpecificRate, modifiedImportPackageMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalSpecificRate = ImportSpecificRate.Object;
			importSpecificRateEqualityComparer = new IImportSpecificRateEqualityComparer();
		}
		IImportSpecificRate originalSpecificRate;
		IImportSpecificRateEqualityComparer importSpecificRateEqualityComparer;

		Mock<IImportSpecificRate> ImportSpecificRate
		{
			get
			{
				var importSpecificRate = new Mock<IImportSpecificRate>();
				importSpecificRate.Setup(x => x.Type).Returns("X");
				importSpecificRate.Setup(x => x.Value).Returns(10.02m);
				return importSpecificRate;
			}
		}
	}
}
