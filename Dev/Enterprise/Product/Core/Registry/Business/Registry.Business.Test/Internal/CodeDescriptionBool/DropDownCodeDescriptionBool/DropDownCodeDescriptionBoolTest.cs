using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DropDownCodeDescriptionBool))]
	sealed class DropDownCodeDescriptionBoolTest : CodeDescriptionBoolTest
	{
		public void TestSetCustomDefaultValuesCore()
		{
			var regBizO = (DropDownCodeDescriptionBool)GetNewBusinessObject();
			AssertEquals(string.Empty, regBizO.Code);
			AssertEquals(string.Empty, regBizO.Description);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DropDownCodeDescriptionBool(new DropDownCodeDescriptionBoolCollection());
		}

		#endregion
	}
}
