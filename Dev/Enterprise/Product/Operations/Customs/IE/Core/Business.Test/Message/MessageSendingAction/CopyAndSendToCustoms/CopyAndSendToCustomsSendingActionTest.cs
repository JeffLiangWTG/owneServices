using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Message.MessageSendingActions.CopyAndSendToCustoms;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CopyAndSendToCustomsSendingAction))]
	class CopyAndSendToCustomsSendingActionTest : CusEntryHeaderMessageSendingActionTest<CopyAndSendToCustomsSendingAction>
	{
		protected override Type ExpectedLookupsType => null;

		protected override Type ExpectedSenderType => typeof(CopyAndSendToCustomsSender);

		protected override Type ExpectedValidationType => typeof(CopyAndSendToCustomsSendingActionValidation);

		protected override BusinessObject GetNewBusinessObject() => action;

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			action = new CopyAndSendToCustomsSendingAction(entryHeader);
		}

		CusEntryHeader entryHeader;
		CopyAndSendToCustomsSendingAction action;
	}
}
