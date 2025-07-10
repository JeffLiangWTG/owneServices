using CargoWise.Customs.DE.MessageContracts.Import;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IImportPackageEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, importPackageEqualityComparer.GetHashCode(originalImportPackage));
		}

		public void TestEquals()
		{
			AssertEquals(true, importPackageEqualityComparer.Equals(originalImportPackage, ImportPackageMock.Object));
		}

		public void TestEquals_Kind()
		{
			var modifiedImportPackageMock = ImportPackageMock;
			modifiedImportPackageMock.Setup(x => x.Kind).Returns("BX");
			AssertEquals(false, importPackageEqualityComparer.Equals(originalImportPackage, modifiedImportPackageMock.Object));
		}

		public void TestEquals_Quantity()
		{
			var modifiedImportPackageMock = ImportPackageMock;
			modifiedImportPackageMock.Setup(x => x.Quantity).Returns(9);
			AssertEquals(false, importPackageEqualityComparer.Equals(originalImportPackage, modifiedImportPackageMock.Object));
		}

		public void TestEquals_MarksNumbers()
		{
			var modifiedImportPackageMock = ImportPackageMock;
			modifiedImportPackageMock.Setup(x => x.MarksNumbers).Returns("1 of 9");
			AssertEquals(false, importPackageEqualityComparer.Equals(originalImportPackage, modifiedImportPackageMock.Object));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalImportPackage = ImportPackageMock.Object;
			importPackageEqualityComparer = new IImportPackageEqualityComparer();
		}
		IImportPackage originalImportPackage;
		IImportPackageEqualityComparer importPackageEqualityComparer;

		internal static Mock<IImportPackage> ImportPackageMock
		{
			get
			{
				var importPackageMock = new Mock<IImportPackage>();
				importPackageMock.Setup(x => x.Kind).Returns("CT");
				importPackageMock.Setup(x => x.Quantity).Returns(10);
				importPackageMock.Setup(x => x.MarksNumbers).Returns("1 of 10");
				return importPackageMock;
			}
		}
	}
}
