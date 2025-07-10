using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITInterchangeProviderTestBaseOnlyTest : TestCaseWithFactory
{
	public void TestInstructionHowToSetInterchangeSenderID()
	{
		var interchangeProvider = GetNewInterchangeProviderForTest();
		AssertEquals("Must be empty", ZString.Empty, interchangeProvider.InstructionHowToSetInterchangeSenderIDExposed);
	}

	public void TestGetCollationKey()
	{
		var interchangeProvider = GetNewInterchangeProviderForTest();
		AssertEquals("Must not collate", "DONOTCOLLATE", interchangeProvider.GetCollationKeyExposed(message: null));
	}

	public void TestInterchangeType()
	{
		var interchangeProvider = GetNewInterchangeProviderForTest();
		AssertEquals($"Must be an {nameof(ITEDIInterchange)}", typeof(ITEDIInterchange), interchangeProvider.InterchangeTypeExposed);
	}

	public void TestGetFooterText()
	{
		var interchangeProvider = GetNewInterchangeProviderForTest();
		AssertEquals("Must be empty", ZString.Empty, interchangeProvider.GetFooterTextExposed(interchange: null, messages: null));
	}

	ITInterchangeProviderForTest GetNewInterchangeProviderForTest() => new ITInterchangeProviderForTest(new NonDependentEDIMessageCollection(Factory));

	#region ITInterchangeProviderForTest

	class ITInterchangeProviderForTest : ITInterchangeProvider
	{
		public ITInterchangeProviderForTest(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		public string InstructionHowToSetInterchangeSenderIDExposed => InstructionHowToSetInterchangeSenderID;
		public string GetCollationKeyExposed(EDIMessage message) => GetCollationKey(message);
		public Type InterchangeTypeExposed => InterchangeType;
		public ZString GetFooterTextExposed(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => GetFooterText(interchange, messages);

		protected override IInterchangeFileNameStrategy GetInterchangeFileNameStrategy(EDIMessage message, CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo)
		{
			throw new NotImplementedException();
		}

		protected override IInterchangeHeaderTextStrategy GetInterchangeHeaderTextStrategy(IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}
