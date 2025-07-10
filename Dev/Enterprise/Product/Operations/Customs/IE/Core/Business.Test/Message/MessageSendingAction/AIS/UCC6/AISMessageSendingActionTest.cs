using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AISMessageSendingAction))]
	class AISMessageSendingActionTest : CusEntryHeaderMessageSendingActionTest<AISMessageSendingAction>
	{
		protected override Type ExpectedLookupsType => typeof(AISMessageSendingActionLookups);

		protected override Type ExpectedSenderType => typeof(AISMessageSender);

		protected override Type ExpectedValidationType => typeof(AISMessageSendingActionValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AISMessageSendingAction((CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew());
		}
	}
}
