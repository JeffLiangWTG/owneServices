using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class WebUserDefaultSettingsForDocAddressTest : TestCase
	{
		public void TestConvertionToStringAndBack()
		{
			ZGuid organisationPK = ZGuid.NewZGuid();
			ZGuid addressPK = ZGuid.NewZGuid();
			ZGuid contactPK = ZGuid.NewZGuid();
			ZString docAddressType = "DeliveryAddress";

			WebUserDefaultSettingsForDocAddress obj = new WebUserDefaultSettingsForDocAddress(organisationPK, addressPK, contactPK, docAddressType);

			AssertEquals("ToString", string.Format("{0};{1};{2};{3}", organisationPK, addressPK, contactPK, docAddressType), obj.ToString());

			WebUserDefaultSettingsForDocAddress obj2 = new WebUserDefaultSettingsForDocAddress(obj.ToString());

			AssertEquals("OrganisationPK", organisationPK, obj2.OrganisationPK);
			AssertEquals("AddressPK", addressPK, obj2.AddressPK);
			AssertEquals("ContactPK", contactPK, obj2.ContactPK);
			AssertEquals("DocAddressType", docAddressType, obj2.DocAddressType);
		}
	}
}
