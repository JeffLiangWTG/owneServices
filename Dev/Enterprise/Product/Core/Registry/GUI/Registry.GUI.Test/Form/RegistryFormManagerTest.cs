using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryFormManager))]
	sealed class RegistryFormManagerTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestSetAndRemoveValidationErrorMessage()
		{
			AssertNoErrors(Manager.OverrideDefaultInfo);

			Manager.SetValidationErrorMessage("Error!");
			AssertHasError(Manager.OverrideDefaultInfo, "Error!");

			Manager.RemoveValidationErrorMessage();
			AssertNoErrors(Manager.OverrideDefaultInfo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RegistryFormManager();
		}

		RegistryFormManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new RegistryFormManager();
				}
				return fManager;
			}
		}

		RegistryFormManager fManager;

		#endregion
	}
}
