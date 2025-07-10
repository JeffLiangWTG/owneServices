using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	public static class AddressValidationHelperTest
	{
		public static void TestCheckAddressStatusAndStreetNumber(BusinessObjectFactory factory, ZPropertyInfo addressInfo, bool shouldAddNotification = true)
		{
			var organization = factory.New<OrgHeader>();
			organization.OH_FullName = "Test Org";
			var address = organization.MainAddress;
			address.OA_Address1 = "356 Test Org Main Address Line";

			var invalidStatusMessage = "Address \"356 Test Org Main Address Line\" has a verification status of Invalid.";
			var streetNumberEmptyMessage = "Address \"356 Test Org Main Address Line\" has no Address Number. Address number will not be sent.";

			foreach (var status in new[] { AddressValidationStatus.Invalid, AddressValidationStatus.ToBeVerified, AddressValidationStatus.Verified })
			{
				address.OA_ValidationStatus = status;

				TestCaseWithFactory.CombineAssertions(status, () =>
				{
					if (shouldAddNotification)
					{
						if (status == AddressValidationStatus.Invalid || status == AddressValidationStatus.ToBeVerified)
						{
							addressInfo.Value = ZGuid.Empty;
							addressInfo.Value = address.PK;
							TestCaseWithFactory.AssertHasWarning("Invalid Status", addressInfo, invalidStatusMessage);

							address.OA_AddressMap = ZString.Empty;
							addressInfo.Value = ZGuid.Empty;
							addressInfo.Value = address.PK;
							TestCaseWithFactory.AssertHasMessageError("No Street Number", addressInfo, streetNumberEmptyMessage);

							address.OA_AddressMap = "SNA1[0-2]";
							addressInfo.Value = ZGuid.Empty;
							addressInfo.Value = address.PK;
							TestCaseWithFactory.AssertNoMessageError("Has Street Number" + address.StreetNumber, addressInfo, streetNumberEmptyMessage);
						}
						else
						{
							addressInfo.Value = ZGuid.Empty;
							addressInfo.Value = address.PK;
							TestCaseWithFactory.AssertNoWarning("Valid Status", addressInfo, invalidStatusMessage);

							address.OA_AddressMap = ZString.Empty;
							addressInfo.Value = ZGuid.Empty;
							addressInfo.Value = address.PK;
							TestCaseWithFactory.AssertHasWarning("No Street Number", addressInfo, streetNumberEmptyMessage);

							address.OA_AddressMap = "SNA1[0-2]";
							addressInfo.Value = ZGuid.Empty;
							addressInfo.Value = address.PK;
							TestCaseWithFactory.AssertNoWarning("Has Street Number" + address.StreetNumber, addressInfo, streetNumberEmptyMessage);
						}
					}
					else
					{
						TestCaseWithFactory.AssertNoNotifications(addressInfo);
					}
				});
			}
		}
	}
}
