using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AISMessageSendingActionParent))]
	class AISMessageSendingActionParentTest : CusEntryHeaderMessageSendingActionParentTest<AISMessageSendingActionParent, AISMessageSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(AISMessageSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AISMessageSendingActionParent(Factory.New<JobDeclaration>());
		}
	}
}
