using System;
using System.Collections;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ZAddress))]
	sealed class ZAddressTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReSetValidateOrgPK()
		{
			ZAddress.Validate validation1 = (info) => { info.AddWarning("x"); };
			ZAddress.Validate validation2 = (info) => { info.AddWarning("y"); };
			ZAddress address = DummyWithAddy.Addy;

			address.OrgPKValidation = validation1;

			AssertExceptionThrown(typeof(InvalidOperationException), "OrgPKValidation has already been set for this ZAddress", delegate
			{
				address.OrgPKValidation = validation2;
			});
		}

		public void TestValidateOrgPK()
		{
			const string error = "BLATICUS!!!";

			ZGuid superMagic = ZGuid.NewZGuid();
			var validation = new Mock<ZAddress.Validate>();

			validation.Setup(p => p(It.IsAny<ZPropertyInfo>()))
				.Callback(new ZAddress.Validate((info) =>
				{
					if (superMagic.Equals(info.Value))
					{
						info.AddError(error);
					}
				}));

			DummyWithAddy.Addy.OrgPKValidation = validation.Object;

			DummyWithAddy.Addy.OrgPK = superMagic;
			AssertHasError("Setting OrgPK: Sholud have error", DummyWithAddy.Addy.OrgPKInfo, error);

			DummyWithAddy.Addy.OrgPK = ZGuid.Empty;
			AssertNoError("Setting OrgPK: Validation should be cleared", DummyWithAddy.Addy.OrgPKInfo, error);

			DummyWithAddy.Addy.SetOrgWithoutSettingDefaultAddress(superMagic);
			AssertHasError("Calling Set Method: Sholud have error", DummyWithAddy.Addy.OrgPKInfo, error);

			DummyWithAddy.Addy.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			AssertNoError("Calling Set Method: Validation should be cleared", DummyWithAddy.Addy.OrgPKInfo, error);
		}

		public void TestOrgPKReadOnly()
		{
			DummyWithAddy.Z0_Guid_ReadOnly = false;
			AssertEquals("Z0_Guid", false, DummyWithAddy.Z0_GuidInfo.ReadOnly);
			AssertEquals("OrgPK", false, DummyWithAddy.Z0_Guid_ZAddress.OrgPKInfo.ReadOnly);

			DummyWithAddy.Z0_Guid_ReadOnly = true;
			AssertEquals("Z0_Guid", true, DummyWithAddy.Z0_GuidInfo.ReadOnly);
			AssertEquals("OrgPK", true, DummyWithAddy.Z0_Guid_ZAddress.OrgPKInfo.ReadOnly);

			DummyWithAddy.Z0_Guid_ReadOnly = false;
			AssertEquals("Z0_Guid", false, DummyWithAddy.Z0_GuidInfo.ReadOnly);
			AssertEquals("OrgPK", false, DummyWithAddy.Z0_Guid_ZAddress.OrgPKInfo.ReadOnly);
		}

		public void TestAddress()
		{
			AssertEquals("Incorrect Address",  "* NO ORGANIZATION IS SELECTED", DummyWithAddy.Addy.Address);

			DummyWithAddy.Addy.OrgPK = ZGuid.Invalid; // simulate user setting a crap Organisation
			AssertEquals("Incorrect Address", "* THE SELECTED ORGANIZATION IS NOT VALID", DummyWithAddy.Addy.Address);

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK; // simulate user selecting an Organisation
			AssertEquals("Incorrect Address", "* NO ADDRESS IS SELECTED", DummyWithAddy.Addy.Address);

			DummyWithAddy.Z0_Guid = OrgAddress.PK;
			AssertEquals("Incorrect Address", "Hugo Boss" + System.Environment.NewLine + "Collins St. Melbourne VIC 2000", DummyWithAddy.Addy.Address);

			DummyWithAddy.Z0_Guid = OrgAddressWithEmptyAddy.PK;
			AssertEquals("Incorrect Address",  "* NO ADDITIONAL ADDRESS DETAILS ON FILE", DummyWithAddy.Addy.Address);
		}

		public void TestAddressDetailed()
		{
			AssertEquals("Incorrect Address", "* NO ORGANIZATION IS SELECTED", DummyWithAddy.Addy.AddressDetailed);

			DummyWithAddy.Addy.OrgPK = ZGuid.Invalid; // simulate user setting a crap Organisation
			AssertEquals("Incorrect Address", "* THE SELECTED ORGANIZATION IS NOT VALID", DummyWithAddy.Addy.AddressDetailed);

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK; // simulate user selecting an Organisation
			AssertEquals("Incorrect Address", "* NO ADDRESS IS SELECTED", DummyWithAddy.Addy.AddressDetailed);

			DummyWithAddy.Z0_Guid = OrgAddress.PK;

			string expectedAddress =
				"Hugo Boss" + System.Environment.NewLine +
				"101" + System.Environment.NewLine +
				"Collins St." + System.Environment.NewLine +
				"Melbourne VIC 2000" + System.Environment.NewLine;

			AssertEquals("Incorrect Address", expectedAddress, DummyWithAddy.Addy.AddressDetailed);

			DummyWithAddy.Z0_Guid = OrgAddressWithEmptyAddy.PK;
			AssertEquals("Incorrect Address", "Address 1" + System.Environment.NewLine, DummyWithAddy.Addy.AddressDetailed);
		}

		public void TestAddressFullFormatted_ValidEntry_FormatsResults()
		{
			// Arrange
			var expectedAddress = "HUGO BOSS\n101\nCOLLINS ST.\nMELBOURNE VIC 2000\nAUSTRALIA";

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK;
			DummyWithAddy.Z0_Guid = OrgAddress.PK;

			// Act / Assert
			AssertEquals("The address has been formatted", expectedAddress, DummyWithAddy.Addy.AddressFullFormatted);
		}

		public void TestAddressFullFormatted_InvalidAndNotEmptyPK_DisplayError()
		{
			// Arrange
			DummyWithAddy.Addy.OrgPK = ZGuid.Invalid;

			// Act / Assert
			AssertEquals("The selected organisation is not valid", "* THE SELECTED ORGANIZATION IS NOT VALID", DummyWithAddy.Addy.AddressFullFormatted);
		}

		public void TestAddressFullFormatted_NullHeader_DisplayError()
		{
			// Act / Assert
			AssertEquals("The organisation has not been selected", "* NO ORGANIZATION IS SELECTED", DummyWithAddy.Addy.AddressFullFormatted);
		}

		public void TestAddressFullFormatted_NoAddressSelected_DisplayError()
		{
			// Arrange
			DummyWithAddy.Addy.OrgPK = OrgHeader.PK;

			// Act / Assert
			AssertEquals("No address is selected", "* NO ADDRESS IS SELECTED", DummyWithAddy.Addy.AddressFullFormatted);
		}

		public void TestAddressWithNonDbPK()
		{
			DummyWithAddy.Z0_Guid = ZGuid.NewZGuid(); // simulate user setting a dumbass non-db existant address
			AssertEquals("Incorrect Address", "* NO ORGANIZATION IS SELECTED", DummyWithAddy.Addy.Address);
		}

		public void TestOrgAddressList()
		{
			AssertEquals("Org Address list should be empty.", 0, DummyWithAddy.Addy.OrgAddress_List.Count);
			AssertEquals("Org Address list should be empty.", 0, ((IZAddress)DummyWithAddy.Addy).AddressList.Count);

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK; // simulate user selecting an Organisation
			AssertEquals("Org Address list should have 1 address.", 1, DummyWithAddy.Addy.OrgAddress_List.Count);
			AssertEquals("Org Address list should have 1 address.", 1, ((IZAddress)DummyWithAddy.Addy).AddressList.Count);

			AssertEquals(DummyWithAddy.Addy.OrgAddress_List.List[0].Code, ((IZAddress)DummyWithAddy.Addy).AddressList.List[0].Code);
		}

		public void TestAddresssListOverride()
		{
			ZAddress address = DummyWithAddy.Addy;
			address.OrgPK = OrgHeader.PK;
			AssertEquals("Org Address list should have 1 address.", 1, address.OrgAddress_List.Count);
			ZAddressItem addressItem = (ZAddressItem)((IList)address.OrgAddress_List)[0];
			AssertNotEquals("AddressDescription", "Hello World", addressItem.AddressDescription);

			address.AddresssListOverride = AddresssListOverride;
			AssertEquals("Org Address list should have 1 address.", 1, address.OrgAddress_List.Count);
			addressItem = (ZAddressItem)((IList)address.OrgAddress_List)[0];
			AssertEquals("AddressDescription", "Hello World", addressItem.AddressDescription);
		}

		ZAddressList AddresssListOverride(BusinessObjectFactory factory, ZAddressList addressList)
		{
			ZAddressList result = new ZAddressList();
			foreach (ZAddressItem item in addressList)
			{
				result.AddAddress(item.PK, item.UsageComment, "Hello World", item.Capabilities);
			}
			return result;
		}

		public void TestInitialiseZAddress()
		{
			Dummy.Z0_Guid = ZGuid.Empty;
			ZAddressExposed addy1 = new ZAddressExposed(Dummy.Z0_GuidInfo);

			Assert("Address should be ReadOnly.", addy1.OuterAddressInfoExposed.ReadOnly);

			Dummy.Z0_Guid = OrgAddress.PK;
			ZAddressExposed addy2 = new ZAddressExposed(Dummy.Z0_GuidInfo);

			Assert("Address should not be ReadOnly.", !addy2.OuterAddressInfoExposed.ReadOnly);
		}

		public void TestSettingBadOrgSetsValidationOnOAProperty()
		{
			Dummy.Z0_Guid = ZGuid.Empty;
			ZAddress addy1 = new ZAddress(Dummy.Z0_GuidInfo);
			addy1.IsOrgVisible = true;
			AssertEquals(false, Dummy.Z0_GuidInfo.HasErrors());
			addy1.OrgPK = ZGuid.Invalid;
			Dummy.Validation.ValidateZ0_Guid();
			AssertEquals(true, Dummy.Z0_GuidInfo.HasErrors());
			addy1.OrgPK = ZGuid.Empty;
			AssertEquals(false, Dummy.Z0_GuidInfo.HasErrors());
			addy1.OrgPK = ZGuid.Missing;
			AssertHasError(Dummy.Z0_GuidInfo, "The selected organization is not valid. Please choose a new organization or amend the organization using F3.");
		}

		public void TestSettingBadAddressSetsValidationOnOAProperty()
		{
			Dummy.Z0_Guid = ZGuid.Empty;
			ZAddressExposed addy1 = new ZAddressExposed(Dummy.Z0_GuidInfo);
			addy1.IsOrgVisible = true;

			addy1.AddressIsMandatoryIfOrgIsValid = false;
			addy1.OrgPK = ZGuid.NewZGuid();
			AssertEquals(false, Dummy.Z0_GuidInfo.HasErrors());
			AssertEquals(true, Dummy.Z0_GuidInfo.HasWarnings());

			addy1.AddressIsMandatoryIfOrgIsValid = true;
			addy1.OrgPK = ZGuid.NewZGuid();
			AssertEquals(true, Dummy.Z0_GuidInfo.HasErrors());
			AssertEquals(false, Dummy.Z0_GuidInfo.HasWarnings());

			addy1.AddressIsMandatoryIfOrgIsValid = false;
			addy1.OuterAddressInfoExposed.Value = ZGuid.Invalid;
			AssertEquals(true, Dummy.Z0_GuidInfo.HasErrors());
			AssertEquals(false, Dummy.Z0_GuidInfo.HasWarnings());

			addy1.AddressIsMandatoryIfOrgIsValid = true;
			addy1.OuterAddressInfoExposed.Value = ZGuid.Invalid;
			AssertEquals(true, Dummy.Z0_GuidInfo.HasErrors());
			AssertEquals(false, Dummy.Z0_GuidInfo.HasWarnings());

			addy1.AddressIsMandatoryIfOrgIsValid = false;
			addy1.OuterAddressInfoExposed.Value = ZGuid.NewZGuid();
			AssertEquals(false, Dummy.Z0_GuidInfo.HasErrors());
			AssertEquals(false, Dummy.Z0_GuidInfo.HasWarnings());

			addy1.AddressIsMandatoryIfOrgIsValid = true;
			addy1.OuterAddressInfoExposed.Value = ZGuid.NewZGuid();
			AssertEquals(false, Dummy.Z0_GuidInfo.HasErrors());
			AssertEquals(false, Dummy.Z0_GuidInfo.HasWarnings());
		}

		public void TestInactiveOrgAddress_HasError()
		{
			OrgAddress.OA_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);
			address.AddressFK = OrgAddress.PK;

			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertHasError("Should have error when inactive address set.", address.AddressFKInfo.InnerInfo, "This Id is inactive - it may not be used.");
		}

		public void TestInactiveOrgAddress_DummyInDatabaseNoChanges_HasWarning()
		{
			OrgAddress.OA_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);
			address.AddressFK = OrgAddress.PK;

			Factory.Save();

			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertHasWarning("Should have warning when in DB with no changes.", address.AddressFKInfo.InnerInfo, "This Id is inactive.");
		}

		public void TestInactiveOrgAddress_DummyInDatabaseWithChanges_HasError()
		{
			OrgAddress.OA_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);

			Factory.Save();

			address.AddressFK = OrgAddress.PK;
			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertHasError("Should have error when inactive address set.", address.AddressFKInfo.InnerInfo, "This Id is inactive - it may not be used.");
		}

		public void TestInactiveOrgHeader_HasError()
		{
			OrgHeader.OH_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);
			address.OrgPK = OrgHeader.PK;
			address.AddressFK = OrgAddress.PK;

			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertHasError("Should have error when inactive organisation set.", address.AddressFKInfo.InnerInfo, "This Id is inactive - it may not be used.");
		}

		public void TestInactiveOrgHeader_DummyInDatabaseNoChanges_HasWarning()
		{
			OrgHeader.OH_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);
			address.OrgPK = OrgHeader.PK;
			address.AddressFK = OrgAddress.PK;

			Factory.Save();

			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertHasWarning("Should have warning when in DB with no changes.", address.AddressFKInfo.InnerInfo, "This Id is inactive.");
		}

		public void TestInactiveOrgHeader_DummyInDatabaseHasChanges_HasError()
		{
			OrgHeader.OH_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);
			address.OrgPK = OrgHeader.PK;

			Factory.Save();

			address.AddressFK = OrgAddress.PK;
			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertHasError("Should have error when inactive address set.", address.AddressFKInfo.InnerInfo, "This Id is inactive - it may not be used.");
		}

		public void TestInactiveOrgAddress_DeletedObject_NoAddressValidation()
		{
			OrgAddress.OA_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);
			var businessObject = address.AddressFKInfo.InnerInfo.BizObj;
			businessObject.Delete();

			address.AddressFK = OrgAddress.PK;

			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertNoErrors("Deleted objects should not have their address validated.", address.AddressFKInfo.InnerInfo);
		}

		public void TestInactiveOrgHeader_DeletedObject_NoAddressValidation()
		{
			OrgHeader.OH_IsActive = false;

			var address = new ZAddress(DummyWithAddy.Z0_GuidInfo);
			address.OrgPK = OrgHeader.PK;

			var businessObject = address.AddressFKInfo.InnerInfo.BizObj;
			businessObject.Delete();

			address.AddressFK = OrgAddress.PK;

			DummyWithAddy.Validation.ValidateZ0_Guid();

			AssertNoErrors("Deleted objects should not have their address validated.", address.AddressFKInfo.InnerInfo);
		}

		#region Test Org PK

		public void TestOrgPK()
		{
			object consol = Factory.New<Enterprise.Integration.Freight.ICommonConsol>();
			IOrgHeader org = Factory.New<IOrgHeader>();
			IOrgAddress addy = Factory.New<IOrgAddress>();

			PropertyInfo addy_OA_OH = ObjectFactory.GetType<IOrgAddress>().GetProperty("OA_OH");
			PropertyInfo consol_ZAddress = GetZAddressPropertyInfo();
			PropertyInfo consol_Address = ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonConsol>().GetProperty("JK_OA_ArrivalCTOAddress");
			PropertyInfo consol_ZAddress_OrgPK = GetZAddressOrgPKPropertyInfo(consol);

			addy_OA_OH.SetValue(addy, org.PK, null);
			consol_Address.SetValue(consol, addy.PK, null);
			ZGuid temp = GetZAddressOrgPK(consol); // access ZAddress to lazy-create it
			AssertEquals("The Consol's _ZAddress.OrgPK should match the Organisation's PK.", org.PK, GetZAddressOrgPK(consol));

			ZGuid g = ZGuid.NewZGuid();
			consol_ZAddress_OrgPK.SetValue(consol_ZAddress.GetValue(consol, null), g, null);
			AssertEquals("The Consol's _ZAddress.OrgPK should match the given ZGuid 'G'.", g, GetZAddressOrgPK(consol));
		}

		public void TestSettingOrgPkFiresOnOrgChanged()
		{
			AssertEquals("Precondition - ZAddressObject.IsAddyOnOrgChangedFired should be false.", false, DummyWithAddy.IsAddyOnOrgChangedFired);

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK;
			AssertEquals("ZAddressObject.IsAddyOnOrgChangedFired should be true.", true, DummyWithAddy.IsAddyOnOrgChangedFired);
		}

		public void TestSetOrgWithoutSettingDefaultAddressFiresOnOrgChanged()
		{
			AssertEquals("Precondition - ZAddressObject.IsAddyOnOrgChangedFired should be false.", false, DummyWithAddy.IsAddyOnOrgChangedFired);

			DummyWithAddy.Addy.SetOrgWithoutSettingDefaultAddress(OrgHeader.PK);
			AssertEquals("ZAddressObject.IsAddyOnOrgChangedFired should be true.", true, DummyWithAddy.IsAddyOnOrgChangedFired);
		}

		PropertyInfo GetZAddressPropertyInfo()
		{
			return ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonConsol>().GetProperty("JK_OA_ArrivalCTOAddress_ZAddress");
		}

		PropertyInfo GetZAddressOrgPKPropertyInfo(object consol)
		{
			PropertyInfo zAddress = GetZAddressPropertyInfo();
			return zAddress.GetValue(consol, null).GetType().GetProperty("OrgPK");
		}

		ZGuid GetZAddressOrgPK(object consol)
		{
			PropertyInfo zAddress = GetZAddressPropertyInfo();
			PropertyInfo orgPK = GetZAddressOrgPKPropertyInfo(consol);
			return (ZGuid)orgPK.GetValue(zAddress.GetValue(consol, null), null);
		}

		#endregion

		public void TestEffectiveAddress()
		{
			IOrgAddress orgAddress2 = Factory.New<IOrgAddress>();
			BusinessObject addressBizObj2 = (BusinessObject)orgAddress2;
			addressBizObj2["OA_OH"] = OrgHeader.PK;
			addressBizObj2["OA_Address1"] = "202";

			DummyWithAddy.Z0_GuidTest = OrgAddress.PK;
			ZAddress address = new ZAddress(DummyWithAddy.Z0_GuidWrappedInfo);
			AssertEquals(OrgHeader.PK, address.OrgPK);

			address.OrgPK = ZGuid.Empty;
			AssertEquals(OrgHeader.PK, address.OrgPK);

			DummyWithAddy.Z0_GuidTest = ZGuid.Empty;
			address.OrgPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, address.OrgPK);
			AssertEquals(ZGuid.Empty, DummyWithAddy.Z0_GuidWrapped);

			address.GetDefaultAddress = new ZAddress.DefaultAddressHandler((orgHeader) => { return orgHeader == null ? ZGuid.Empty : orgAddress2.PK; });
			address.OrgPK = OrgHeader.PK;
			AssertEquals(OrgHeader.PK, address.OrgPK);
			AssertEquals(orgAddress2.PK, DummyWithAddy.Z0_GuidWrapped);

			address.OrgPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, address.OrgPK);
			AssertEquals(ZGuid.Empty, DummyWithAddy.Z0_GuidWrapped);

			address.GetDefaultAddress = null;
			address.OrgPK = OrgHeader.PK;
			AssertEquals(OrgHeader.PK, address.OrgPK);
			AssertEquals(ZGuid.Empty, DummyWithAddy.Z0_GuidWrapped);
		}

		public void TestInactiveSelectedAddressIsInList()
		{
			BusinessObject address2 = (BusinessObject)Factory.New<IOrgAddress>();
			address2[OrgAddressSchema.OA_OH] = OrgHeader.PK;
			address2[OrgAddressSchema.OA_Address1] = "Address 2";
			address2[OrgAddressSchema.OA_City] = "Sydney";

			BusinessObject capability = (BusinessObject)Factory.New<IOrgAddressCapability>();
			capability[OrgAddressCapabilitySchema.PZ_AddressType] = Enterprise.MasterFiles.Business.OrgConstants.AddressType.Pickup;
			capability[OrgAddressCapabilitySchema.PZ_IsMainAddress] = true;
			capability[OrgAddressCapabilitySchema.PZ_OA] = address2.PK;

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK;
			DummyWithAddy.Z0_Guid = address2.PK;
			AssertEquals(2, DummyWithAddy.Addy.OrgAddress_List.Count);
			address2[OrgAddressSchema.OA_IsActive] = false;
			AssertEquals(2, DummyWithAddy.Addy.OrgAddress_List.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject address2Reloaded = (BusinessObject)factory2.Load<IOrgAddress>(address2.PK);
			DummyWithZAddress dummyReloaded = factory2.Load<DummyWithZAddress>(DummyWithAddy.PK);
			AssertEquals(false, address2Reloaded[OrgAddressSchema.OA_IsActive]);
			AssertEquals(address2Reloaded.PK, dummyReloaded.Z0_Guid);
			AssertEquals(2, dummyReloaded.Addy.OrgAddress_List.Count);
			ZAddressItem addressItem = (ZAddressItem)((IList)dummyReloaded.Addy.OrgAddress_List)[1];
			AssertEquals("Inactive", addressItem.Capabilities[0].Capability);
			AssertEquals(false, addressItem.Capabilities[0].IsDefault);
			AssertEquals("Inactive: " + ((IOrgAddress)address2).OA_Code, addressItem.UsageComment);
			AssertEquals("Inactive: " + ((IOrgAddress)address2).AddressDetailedOnSingleLine, addressItem.AddressDescription);
		}

		public void TestInactiveSelectedAddressWhenDeleted()
		{
			BusinessObject address2 = (BusinessObject)Factory.New<IOrgAddress>();
			address2[OrgAddressSchema.OA_OH] = OrgHeader.PK;
			address2[OrgAddressSchema.OA_Address1] = "Address 2";
			address2[OrgAddressSchema.OA_City] = "Sydney";

			BusinessObject capability = (BusinessObject)Factory.New<IOrgAddressCapability>();
			capability[OrgAddressCapabilitySchema.PZ_AddressType] = Enterprise.MasterFiles.Business.OrgConstants.AddressType.Pickup;
			capability[OrgAddressCapabilitySchema.PZ_IsMainAddress] = true;
			capability[OrgAddressCapabilitySchema.PZ_OA] = address2.PK;

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK;
			DummyWithAddy.Z0_Guid = address2.PK;
			AssertEquals(2, DummyWithAddy.Addy.OrgAddress_List.Count);
			address2[OrgAddressSchema.OA_IsActive] = false;
			AssertEquals(2, DummyWithAddy.Addy.OrgAddress_List.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject address2Reloaded = (BusinessObject)factory2.Load<IOrgAddress>(address2.PK);
			DummyWithZAddress dummyReloaded = factory2.Load<DummyWithZAddress>(DummyWithAddy.PK);
			AssertEquals(false, address2Reloaded[OrgAddressSchema.OA_IsActive]);
			AssertEquals(address2Reloaded.PK, dummyReloaded.Z0_Guid);
			AssertEquals(2, dummyReloaded.Addy.OrgAddress_List.Count);

			//delete address2 in original factory, refresh factory2
			address2.Delete();
			Factory.Save();

			ErrorReporter.Clear();
			var list = ((IList)dummyReloaded.Addy.OrgAddress_List);
			AssertEquals("No error reported due to accessing property on a deleted BizO", "", ErrorReporter.LastKeyReported);
		}

		public void TestDeletedOrgAddress()
		{
			var address = Factory.New<IOrgAddress>();
			address.OA_OH = OrgHeader.PK;
			address.OA_Address1 = "Address 1";
			address.OA_City = "Sydney";

			DummyWithAddy.Addy.OrgPK = OrgHeader.PK;
			DummyWithAddy.Z0_Guid = address.PK;

			AssertSame(address, DummyWithAddy.Addy.OrgAddress);

			address.Delete();

			AssertNull(DummyWithAddy.Addy.OrgAddress);
		}

		#region Implementation

		DummyWithZAddress DummyWithAddy;
		IOrgHeader OrgHeader;
		IOrgAddress OrgAddress;
		IOrgAddress OrgAddressWithEmptyAddy;

		protected override void SetUp()
		{
			base.SetUp();

			Dummy = (DummyBusinessObject)Factory.New(TypeOfDummy);
			AssertNotNull("Dummy should be created!", Dummy);

			DummyWithAddy = Factory.New<DummyWithZAddress>();

			// create OrgHeaders
			OrgHeader = Factory.New<IOrgHeader>();
			BusinessObject orgHeaderBizObj = (BusinessObject)OrgHeader;
			orgHeaderBizObj["OH_RL_NKClosestPort"] = "AUMLB";
			orgHeaderBizObj[OrgHeaderSchema.OH_Code] = "OH" + new Random().Next(10000).ToString();

			// create OrgAddresses
			OrgAddressWithEmptyAddy = Factory.New<IOrgAddress>();
			((BusinessObject)OrgAddressWithEmptyAddy)["OA_Address1"] = "Address 1";

			OrgAddress = Factory.New<IOrgAddress>();
			BusinessObject addressBizObj = (BusinessObject)OrgAddress;
			addressBizObj["OA_OH"] = OrgHeader.PK;
			addressBizObj["OA_Address1"] = "101";
			addressBizObj["OA_Address2"] = "Collins St.";
			addressBizObj["OA_City"] = "Melbourne";
			addressBizObj["OA_State"] = "VIC";
			addressBizObj["OA_PostCode"] = "2000";
			addressBizObj["OA_CompanyNameOverride"] = "Hugo Boss";

			// create an OrgAddress with an empty Address
			EnterpriseBusinessObject addressWithEmptyAddyBizObj = (EnterpriseBusinessObject)OrgAddressWithEmptyAddy;
			addressWithEmptyAddyBizObj["OA_OH"] = OrgHeader.PK;
		}

		DummyBusinessObject Dummy;

		Type TypeOfDummy { get { return typeof(DummyBusinessObject); } }

		protected override BusinessObject GetNewBusinessObject()
		{
			return (Factory.New<DummyWithZAddress>()).Addy;
		}

		#endregion

		#region Empty Tests

		public override void TestBizObjectFields()
		{
			Assert("Do nothing", true);
		}

		#endregion
	}
}
