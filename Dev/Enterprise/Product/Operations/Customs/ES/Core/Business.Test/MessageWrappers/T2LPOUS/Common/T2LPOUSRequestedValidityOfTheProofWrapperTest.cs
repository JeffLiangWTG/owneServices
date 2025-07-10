using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSRequestedValidityOfTheProofWrapperTest : WrapperHelperTest<T2LPOUSRequestedValidityOfTheProofWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown("Constructor Throws Exception if cusEntryInstruction is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","cusEntryInstruction"), () => new T2LPOUSRequestedValidityOfTheProofWrapper(null));
	}

	public void TestNumberOfDays()
	{
		entryInstruction.ZG_NumberOfDays = 200;
		AssertEquals("Expected filled NumberOfDays", 200, wrapper.NumberOfDays);
	}

	public void TestJustification()
	{
		entryInstruction.ZG_Justification = "Justification";
		AssertEquals("Expected filled Justification", "Justification", wrapper.Justification);
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryInstruction = Factory.New<CusEntryInstruction>();

		wrapper = new T2LPOUSRequestedValidityOfTheProofWrapper(entryInstruction);
	}
	CusEntryInstruction entryInstruction;
	T2LPOUSRequestedValidityOfTheProofWrapper wrapper;

	protected override T2LPOUSRequestedValidityOfTheProofWrapper GetProvider() => wrapper;
}
