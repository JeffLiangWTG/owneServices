using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AddressWrapperWithIDocDocAddress))]
	sealed class AddressWrapperWithIDocDocAddressTest : GenericWrapperTest
	{
		public void TestSelectedContact()
		{
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Address1 = "OrgAddress1";

			JobDocAddress jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;

			OrgHeader orgHeader = Factory.Load<OrgHeader>(orgAddress.OA_OH);
			var deliveryContact1 = orgHeader.Contacts.AddNew();
			deliveryContact1.OC_ContactName = "Contact1";
			deliveryContact1.OC_Phone = "345678";

			AddressWrapperWithIDocDocAddress wrapper = new AddressWrapperWithIDocDocAddress(jobDocAddress, Factory);

			AssertEquals("No selected contact", ZString.Empty, wrapper.ContactName);
			AssertEquals("No selected phone", ZString.Empty, wrapper.ContactPhone);

			jobDocAddress.E2_Contact = "Contact1";

			wrapper = new AddressWrapperWithIDocDocAddress(jobDocAddress, Factory);

			AssertEquals("Contact1", wrapper.ContactName);
			AssertEquals("345678", wrapper.ContactPhone);
		}

		public override void TestWrapperMappingsEmpty()
		{
			AddressWrapperWithIDocDocAddress wrapperEmpty = new AddressWrapperWithIDocDocAddress(null, ContactType.All, Factory);

			// AddressWrapper properties
			AssertEquals("wrapperEmpty.CompanyCode", ZString.Empty, wrapperEmpty.CompanyCode);
			AssertEquals("wrapperEmpty.CompanyName", ZString.Empty, wrapperEmpty.CompanyName);
			AssertEquals("wrapperEmpty.CompanyNameAndAddress", ZString.Empty, wrapperEmpty.CompanyNameAndAddress);
			AssertEquals("wrapperEmpty.Address", ZString.Empty, wrapperEmpty.Address);
			AssertEquals("wrapperEmpty.AddressCaption", ZString.Empty, wrapperEmpty.AddressCaption);
			AssertEquals("wrapperEmpty.AddressAsASingleLine", ZString.Empty, wrapperEmpty.AddressAsASingleLine);
			AssertEquals("wrapperEmpty.AddressLine1", ZString.Empty, wrapperEmpty.AddressLine1);
			AssertEquals("wrapperEmpty.AddressLine2", ZString.Empty, wrapperEmpty.AddressLine2);
			AssertEquals("wrapperEmpty.City", ZString.Empty, wrapperEmpty.City);
			AssertEquals("wrapperEmpty.State", ZString.Empty, wrapperEmpty.State);
			AssertEquals("wrapperEmpty.PostCode", ZString.Empty, wrapperEmpty.PostCode);
			AssertEquals("wrapperEmpty.ContactName", ZString.Empty, wrapperEmpty.ContactName);
			AssertEquals("wrapperEmpty.Phone", ZString.Empty, wrapperEmpty.Phone);
			AssertEquals("wrapperEmpty.Fax", ZString.Empty, wrapperEmpty.Fax);
			AssertEquals("wrapperEmpty.Mobile", ZString.Empty, wrapperEmpty.Mobile);
			AssertEquals("wrapperEmpty.Email", ZString.Empty, wrapperEmpty.Email);
			AssertEquals("wrapperEmpty.PickupFromTime", ZString.Empty, wrapperEmpty.PickupFromTime);
			AssertEquals("wrapperEmpty.PickupToTime", ZString.Empty, wrapperEmpty.PickupToTime);
			AssertEquals("wrapperEmpty.DeliverFromTime", ZString.Empty, wrapperEmpty.DeliverFromTime);
			AssertEquals("wrapperEmpty.DeliverToTime", ZString.Empty, wrapperEmpty.DeliverToTime);
			AssertEquals("wrapperEmpty.DoNotAttendFromTime", ZString.Empty, wrapperEmpty.DoNotAttendFromTime);
			AssertEquals("wrapperEmpty.DoNotAttendToTime", ZString.Empty, wrapperEmpty.DoNotAttendToTime);
			AssertEquals("wrapperEmpty.FurtherConstraints", ZString.Empty, wrapperEmpty.FurtherConstraints);
			AssertEquals("wrapperEmpty.OtherWarehouseFacilities", ZString.Empty, wrapperEmpty.OtherWarehouseFacilities);

			AssertEquals("wrapperEmpty.HasDockLeveller", ZBool.False, wrapperEmpty.HasDockLeveller);
			AssertEquals("wrapperEmpty.HasPalletJack", ZBool.False, wrapperEmpty.HasPalletJack);
			AssertEquals("wrapperEmpty.HasForkLift", ZBool.False, wrapperEmpty.HasForkLift);
			AssertEquals("wrapperEmpty.HasWarehousing", ZBool.False, wrapperEmpty.HasWarehousing);
			AssertEquals("wrapperEmpty.HasLoadingUnloadingConstraints", ZBool.False, wrapperEmpty.HasLoadingUnloadingConstraints);

			AssertEquals("wrapperEmpty.AccessPoint", ZString.Empty, wrapperEmpty.AccessPoint.Code);
			AssertEquals("wrapperEmpty.CommunicationRequired", ZString.Empty, wrapperEmpty.CommunicationRequired.Code);
			AssertEquals("wrapperEmpty.ContainerHandling", ZString.Empty, wrapperEmpty.ContainerHandling.Code);
			AssertEquals("wrapperEmpty.DockHeight", ZString.Empty, wrapperEmpty.DockHeight.Code);
			AssertEquals("wrapperEmpty.LabourRequired", ZString.Empty, wrapperEmpty.LabourRequired.Code);

			AssertEquals("wrapperEmpty.CustomsCodes", 0, wrapperEmpty.CustomsCodes.Count);

			AssertEquals("wrapperEmpty.Country", ZString.Empty, wrapperEmpty.Country.Code);
			AssertEquals("wrapperEmpty.Location", ZString.Empty, wrapperEmpty.Location.UNLOCO);

			// IDocDocAddress properties
			AssertEquals("wrapperEmpty.Address1", ZString.Empty, wrapperEmpty.Address1);
			AssertEquals("wrapperEmpty.Address2", ZString.Empty, wrapperEmpty.Address2);
			AssertEquals("wrapperEmpty.AddressType", ZString.Empty, wrapperEmpty.AddressType);
			AssertEquals("wrapperEmpty.Code", ZString.Empty, wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.CompanyNameAddress1Address2City", "   ", wrapperEmpty.CompanyNameAddress1Address2City);
			AssertEquals("wrapperEmpty.CompanyNameOverride", ZString.Empty, wrapperEmpty.CompanyNameOverride);
			AssertEquals("wrapperEmpty.DepotLocalControlledPremisesID", ZString.Empty, wrapperEmpty.DepotLocalControlledPremisesID);
			AssertEquals("wrapperEmpty.Description", "Unspecified", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.Language", ZString.Empty, wrapperEmpty.Language);
			AssertEquals("wrapperEmpty.LocalControlledPremisesID", ZString.Empty, wrapperEmpty.LocalControlledPremisesID);
			AssertEquals("wrapperEmpty.PostalAddress", ZString.Empty, wrapperEmpty.PostalAddress);
			AssertEquals("wrapperEmpty.PostalAddressExcludeName", ZString.Empty, wrapperEmpty.PostalAddressExcludeName);
			AssertEquals("wrapperEmpty.SplitAddress1", ZString.Empty, wrapperEmpty.SplitAddress1);
			AssertEquals("wrapperEmpty.SplitAddress2", ZString.Empty, wrapperEmpty.SplitAddress2);
			AssertEquals("wrapperEmpty.State", ZString.Empty, wrapperEmpty.State);
			AssertEquals("wrapperEmpty.Type", ZString.Empty, wrapperEmpty.Type);
			AssertEquals("wrapperEmpty.UsageComment", ZString.Empty, wrapperEmpty.UsageComment);
			AssertEquals("wrapperEmpty.WarehouseLocalControlledPremisesID", ZString.Empty, wrapperEmpty.WarehouseLocalControlledPremisesID);

			AssertEquals("wrapperEmpty.DeliverFromTimeOnly", ZDateTime.Empty, wrapperEmpty.DeliverFromTimeOnly);
			AssertEquals("wrapperEmpty.DeliverToTimeOnly", ZDateTime.Empty, wrapperEmpty.DeliverToTimeOnly);
			AssertEquals("wrapperEmpty.DoNotAttendFrom", ZDateTime.Empty, wrapperEmpty.DoNotAttendFrom);
			AssertEquals("wrapperEmpty.DoNotAttendFromForCartageAdvice", ZDateTime.Empty, wrapperEmpty.DoNotAttendFromForCartageAdvice);
			AssertEquals("wrapperEmpty.DoNotAttendTo", ZDateTime.Empty, wrapperEmpty.DoNotAttendTo);
			AssertEquals("wrapperEmpty.DoNotAttendToForCartageAdvice", ZDateTime.Empty, wrapperEmpty.DoNotAttendToForCartageAdvice);
			AssertEquals("wrapperEmpty.PickupFromTimeOnly", ZDateTime.Empty, wrapperEmpty.PickupFromTimeOnly);
			AssertEquals("wrapperEmpty.PickupToTimeOnly", ZDateTime.Empty, wrapperEmpty.PickupToTimeOnly);

			AssertEquals("wrapperEmpty.HasWareHousing", ZBool.False, wrapperEmpty.HasWareHousing);
			AssertEquals("wrapperEmpty.Overridden", ZBool.False, wrapperEmpty.Overridden);

			AssertEquals("wrapperEmpty.DocAddressType", DocAddressType.None, wrapperEmpty.DocAddressType);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"AddressWithIDocDocAddress       (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
AviationSecurity                        AviationSecurity
AccessPoint                             CodeAndDescription
CommunicationRequired                   CodeAndDescription
ContainerHandling                       CodeAndDescription
DockHeight                              CodeAndDescription
LabourRequired                          CodeAndDescription
Country                                 Country
Location                                Location
Organization                            Organisation
Address                                 String
Address1                                String
Address2                                String
AddressAsASingleLine                    String
AddressCaption                          String
AddressLine1                            String
AddressLine2                            String
AddressType                             String
City                                    String
Code                                    String
CompanyCode                             String
CompanyName                             String
CompanyNameAddress1Address2City         String
CompanyNameAndAddress                   String
CompanyNameOverride                     String
ContactName                             String
ContactPhone                            String
DeliverFromTime                         String
DeliverFromTimeOnly                     DateTime
DeliverTimetable                        String
DeliverToTime                           String
DeliverToTimeOnly                       DateTime
DeliveryRoute                           String
DeliveryRouteSequence                   Short
DepotLocalControlledPremisesID          String
Description                             String
DoNotAttendFrom                         DateTime
DoNotAttendFromForCartageAdvice         DateTime
DoNotAttendFromTime                     String
DoNotAttendTo                           DateTime
DoNotAttendToForCartageAdvice           DateTime
DoNotAttendToTime                       String
Email                                   String
Fax                                     String
FurtherConstraints                      String
HasDockLeveller                         Bool
HasForkLift                             Bool
HasLoadingUnloadingConstraints          Bool
HasPalletJack                           Bool
HasWarehousing                          Bool
HasWareHousing                          Bool
Language                                String
LocalControlledPremisesID               String
Mobile                                  String
OtherWarehouseFacilities                String
Overridden                              Bool
Phone                                   String
PickupFromTime                          String
PickupFromTimeOnly                      DateTime
PickupTimetable                         String
PickupToTime                            String
PickupToTimeOnly                        DateTime
PostalAddress                           String
PostalAddressExcludeName                String
PostCode                                String
ShortCode                               String
SplitAddress1                           String
SplitAddress2                           String
State                                   String
Type                                    String
UsageComment                            String
WarehouseLocalControlledPremisesID      String

CustomsCodes                            RegistrationNumberCode Collection";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AccessPoint : DCK - Dock
AviationSecurity : (No Default Field Value Available on AviationSecurity)
CommunicationRequired : APP - Appointment Required
ContainerHandling : ASK - Ask
Country : AU - Australia
DockHeight : NON - Non-Standard Dock Height.
LabourRequired : ASK - Ask
Location : AUMEL - Melbourne
Organization : VIC\nAUSTRALIA
Registry : (No Default Field Value Available on Registry)
";
			}
		}
		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "AUMEL";

			OrgAddress address = organisation.MainAddress;
			address.OA_AccessPoint = OrgConstants.AccessPoint.Code.Dock;
			address.OA_CommunicationRequired = OrgConstants.CommunicationRequired.Code.Appointment;
			address.OA_ContainerHandling = OrgConstants.ContainerHandling.Code.Ask;
			address.OA_Dock_Height = OrgConstants.DockHeight.Code.NonStandard;
			address.OA_LabourRequired = OrgConstants.LabourRequired.Code.Ask;

			return new AddressWrapperWithIDocDocAddress(organisation.MainAddress, ContactType.All, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new AddressWrapperWithIDocDocAddress(null, ContactType.All, Factory);
		}
	}
}
