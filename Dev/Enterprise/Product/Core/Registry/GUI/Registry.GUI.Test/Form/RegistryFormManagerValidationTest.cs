using System.Reflection;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryFormManagerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOverrideDefault()
		{
			AssertNoErrors("Precondition: OverrideDefault should not have errors.", Parent.OverrideDefaultInfo);
			FieldInfo fieldInfo = typeof(RegistryFormManager).GetField("fValidationErrorMessage", BindingFlags.NonPublic | BindingFlags.Instance);

			fieldInfo.SetValue(Parent, "An error!");
			Parent.Validation.ValidateOverrideDefault();
			AssertHasError(Parent.OverrideDefaultInfo, "An error!");

			fieldInfo.SetValue(Parent, null);
			Parent.Validation.ValidateOverrideDefault();
			AssertNoErrors(Parent.OverrideDefaultInfo);
		}

		#region Implementation

		RegistryFormManager Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = new RegistryFormManager();
				}
				return fParent;
			}
		}

		RegistryFormManager fParent;

		#endregion
	}
}
