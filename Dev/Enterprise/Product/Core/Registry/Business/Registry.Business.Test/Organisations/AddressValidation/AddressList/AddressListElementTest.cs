using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressListElement))]
	sealed class AddressListElementTest : RegistryBusinessObjectTestCaseBase
	{
		#region Related Entities

		#region TestParent

		public void TestParent()
		{
			var collection = new AddressListCollection();
			var element = new AddressListElement();
			collection.Add(element);

			AssertEquals(collection, element.Parent);
		}

		#endregion

		#endregion

		#region Properties

		#region TestControllerName

		public void TestControllerName()
		{
			BizObj.ControllerName = "JobShipment";
			AssertEquals("JobShipment", BizObj.ControllerName);
		}

		#endregion

		#region TestAddressType

		public void TestAddressType()
		{
			BizObj.AddressType = "BuyerDocumentaryAddress";
			AssertEquals("BuyerDocumentaryAddress", BizObj.AddressType);
		}

		#endregion

		#endregion

		#region Validation

		#region TestValidations

		public void TestValidateAddressType()
		{
			AssertNoErrors("Precondition", BizObj.AddressTypeInfo);

			BizObj.AddressType = "";
			AssertHasErrors(BizObj.AddressTypeInfo);
			AssertEquals(1, BizObj.AddressTypeInfo.Notifications.Count());

			BizObj.AddressType = "Random Address Type";
			AssertHasError(BizObj.AddressTypeInfo, "Enter a valid Address Type.");

			BizObj.AddressType = "BuyerDocumentaryAddress";
			AssertNoErrors(BizObj.AddressTypeInfo);

			var collection = new AddressListCollection();
			collection.Add(BizObj);

			var abcCategory = collection.AddNew();
			abcCategory.AddressType = "ConsignorDocumentaryAddress";
			AssertNoErrors(BizObj.AddressTypeInfo);
			AssertNoErrors(abcCategory.AddressTypeInfo);
		}

		public void TestValidateControllerName()
		{
			AssertNoErrors("Precondition", BizObj.ControllerNameInfo);

			BizObj.ControllerName = "";
			AssertHasErrors(BizObj.ControllerNameInfo);
			AssertEquals(1, BizObj.ControllerNameInfo.Notifications.Count());

			BizObj.ControllerName = "Random ControllerName";
			AssertHasError(BizObj.ControllerNameInfo, "Enter a valid Controller.");

			BizObj.ControllerName = "JobShipment";
			AssertNoErrors(BizObj.ControllerNameInfo);

			var collection = new AddressListCollection();
			collection.Add(BizObj);

			var abcCategory = collection.AddNew();
			abcCategory.ControllerName = "JobShipment";
			AssertNoErrors(BizObj.ControllerNameInfo);
			AssertNoErrors(abcCategory.ControllerNameInfo);
		}

		#endregion

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new AddressListElement();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool IsCodeMandatory => false;

		new AddressListElement BizObj
		{
			get { return (AddressListElement)base.BizObj; }
		}

		#endregion
	}
}
