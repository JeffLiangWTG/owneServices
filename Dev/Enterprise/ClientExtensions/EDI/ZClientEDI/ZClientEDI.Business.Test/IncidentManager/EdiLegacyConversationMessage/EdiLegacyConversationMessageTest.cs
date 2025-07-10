using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EdiLegacyConversationMessage))]
	internal class EdiLegacyConversationMessageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<EdiLegacyConversationMessage>();
		}
	}
}
