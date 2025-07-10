using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AISUCC5MessageSendingAction))]
	sealed class AISUCC5MessageSendingActionTest : CusEntryHeaderMessageSendingActionTest<AISUCC5MessageSendingAction>
	{
		protected override Type ExpectedLookupsType => typeof(AISUCC5MessageSendingActionLookups);

		protected override Type ExpectedSenderType => typeof(AISUCC5MessageSender);

		protected override Type ExpectedValidationType => typeof(AISUCC5MessageSendingActionValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AISUCC5MessageSendingAction((CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew());
		}
	}
}
