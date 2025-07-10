using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SADDeclarationWrapperTest<TDeclaration> : TestCaseWithFactory
	where TDeclaration : SADDeclarationWrapper
{
	public void TestTypeDeclarationSubType1()
	{
		jobDeclaration.JE_MessageSubType = "ABC";
		AssertEquals("AB", sadDeclarationWrapper.TypeDeclarationSubType1);
		jobDeclaration.JE_MessageSubType = "C";
		AssertEquals("C", sadDeclarationWrapper.TypeDeclarationSubType1);
		jobDeclaration.JE_MessageSubType = ZString.Empty;
		AssertEquals(ZString.Empty, sadDeclarationWrapper.TypeDeclarationSubType1);
	}

	public void TestTypeDeclarationSubType2()
	{
		entryInstruction.CEI_SubStyle = "";
		AssertEquals("", sadDeclarationWrapper.TypeDeclarationSubType2);
		entryInstruction.CEI_SubStyle = "A";
		AssertEquals("A", sadDeclarationWrapper.TypeDeclarationSubType2);
		entryInstruction.CEI_SubStyle = "C";
		AssertEquals("C", sadDeclarationWrapper.TypeDeclarationSubType2);
	}

	public abstract void TestTypeDeclarationSubType3();

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => GetDeclarationWrapper(null, null));
		AssertExceptionThrown<ArgumentNullException>(() => GetDeclarationWrapper(jobDeclaration, null));
		AssertExceptionThrown<ArgumentNullException>(() => GetDeclarationWrapper(null, entryInstruction));
		AssertNoExceptionThrown(() => GetDeclarationWrapper(jobDeclaration, entryInstruction));
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		sadDeclarationWrapper = GetDeclarationWrapper(jobDeclaration, entryInstruction);
	}

	protected abstract TDeclaration GetDeclarationWrapper(JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction);

	protected JobDeclaration jobDeclaration;
	CusEntryInstruction entryInstruction;
	protected TDeclaration sadDeclarationWrapper;
}
