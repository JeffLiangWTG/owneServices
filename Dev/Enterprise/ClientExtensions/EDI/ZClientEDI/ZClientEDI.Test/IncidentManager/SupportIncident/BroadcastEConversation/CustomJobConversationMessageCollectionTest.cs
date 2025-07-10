using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	[TestedType(typeof(CustomJobConversationMessageCollection))]
	public class CustomJobConversationMessageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomJobConversationMessageCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new CustomJobConversationMessage(null);

		protected override CustomJobConversationMessageCollection GetCollectionToTest() => new CustomJobConversationMessageCollection(Factory);
	}
}
