using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Test
{
	[TestedType(typeof(CommunicationModeMigratorDataModel))]
	sealed class CommunicationModeMigratorDataModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dataObject = new CommunicationModeMigratorDataModel(Factory);
			return dataObject;
		}
	}
}
