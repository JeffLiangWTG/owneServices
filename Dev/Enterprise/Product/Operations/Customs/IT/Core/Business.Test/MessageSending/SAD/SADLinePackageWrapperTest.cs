using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADLinePackageWrapperTest : TestCaseWithFactory
{
	public void TestNumberOfPacks()
	{
		packageProviderMock.Setup(m => m.NumberOfPackages).Returns(114);

		packageProviderMock.Setup(m => m.PackageType).Returns("VG");
		AssertNull($"When PackageType is bulk, {nameof(linePackageWrapper.NumberOfPacks)}", linePackageWrapper.NumberOfPacks);

		packageProviderMock.Setup(m => m.PackageType).Returns("XX");
		AssertEquals($"When PackageType is not bulk, {nameof(linePackageWrapper.NumberOfPacks)}", 114, linePackageWrapper.NumberOfPacks);
	}

	public void TestPackageType()
	{
		packageProviderMock.Setup(m => m.PackageType).Returns("XX");
		AssertEquals(nameof(linePackageWrapper.PackageType), "XX", linePackageWrapper.PackageType);
	}

	public void TestNumberOfPieces()
	{
		packageProviderMock.Setup(m => m.NumberOfPackages).Returns(114);

		packageProviderMock.Setup(m => m.PackageType).Returns("XX");
		AssertNull($"When PackageType is not bulk, {nameof(linePackageWrapper.NumberOfPieces)}", linePackageWrapper.NumberOfPieces);

		packageProviderMock.Setup(m => m.PackageType).Returns("NE");
		AssertEquals($"When PackageType is bulk, {nameof(linePackageWrapper.NumberOfPieces)}", 114, linePackageWrapper.NumberOfPieces);
	}

	public void TestMarksAndNumbers()
	{
		packageProviderMock.Setup(m => m.MarksAndNumbers).Returns("MARK AND NOS 1");
		AssertEquals(nameof(linePackageWrapper.MarksAndNumbers), "MARK AND NOS 1", linePackageWrapper.MarksAndNumbers);
	}

	public void TestMarksAndNumbersMaximumLength()
	{
		packageProviderMock.Setup(m => m.MarksAndNumbers).Returns(new ZString('0', 50));
		AssertEquals($"{nameof(linePackageWrapper.MarksAndNumbers)} is truncated to maximum length", 42, linePackageWrapper.MarksAndNumbers.Length);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("An entry line is required", () => { new SADLinePackageWrapper(null); });
		AssertNoExceptionThrown("An entry line is required", () => { new SADLinePackageWrapper(packageProviderMock.Object); });
	}

	protected override void SetUp()
	{
		base.SetUp();
		packageProviderMock = new Mock<IPackageProvider>();
		linePackageWrapper = new SADLinePackageWrapper(packageProviderMock.Object);
	}

	Mock<IPackageProvider> packageProviderMock;
	SADLinePackageWrapper linePackageWrapper;
}
