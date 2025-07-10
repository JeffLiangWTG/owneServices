using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(ClientTariffAndLevelCollection))]
	public class ClientTariffAndLevelCollectionTest : DocumentBrandingCollectionTestCase<ClientTariffAndLevelCollection>
	{
		#region Implementation

		protected override ClientTariffAndLevelCollection GetCollectionToTest()
		{
			return new ClientTariffAndLevelCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClientTariffAndLevel();
		}

		#endregion
	}
}
