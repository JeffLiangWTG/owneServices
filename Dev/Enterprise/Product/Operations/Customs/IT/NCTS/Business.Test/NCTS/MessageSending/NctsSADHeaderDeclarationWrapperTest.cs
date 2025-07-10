using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsSADHeaderDeclarationWrapperTest<TWrapper> : TestCaseWithFactory
	where TWrapper : NctsSADHeaderDeclarationWrapper
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsDepartureMovementHeader parameter is null", () => GetWrapper(null));
		AssertNoExceptionThrown(() => GetWrapper(nctsMovementHeader));
	}

	public void TestTypeDeclarationSubType1()
	{
		AssertEquals(nameof(headerDeclarationWrapper.TypeDeclarationSubType1), ZString.Empty, headerDeclarationWrapper.TypeDeclarationSubType1);
	}

	public void TestTypeDeclarationSubType2()
	{
		AssertEquals(nameof(headerDeclarationWrapper.TypeDeclarationSubType2), ExpectedTypeDeclarationSubType2, headerDeclarationWrapper.TypeDeclarationSubType2);
	}

	protected abstract ZString ExpectedTypeDeclarationSubType2 { get; }

	public void TestTypeDeclarationSubType3()
	{
		AssertEquals(nameof(headerDeclarationWrapper.TypeDeclarationSubType3), ZString.Empty, headerDeclarationWrapper.TypeDeclarationSubType3);

		nctsMovementHeader.BM_InBondEntryType = "XXX";
		AssertEquals(nameof(headerDeclarationWrapper.TypeDeclarationSubType3), "XXX", headerDeclarationWrapper.TypeDeclarationSubType3);
	}

	protected abstract TWrapper GetWrapper(NctsDepartureMovementHeader nctsDepartureMovementHeader);

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		headerDeclarationWrapper = GetWrapper(nctsMovementHeader);
	}
	NctsDepartureMovementHeader nctsMovementHeader;
	TWrapper headerDeclarationWrapper;
}
