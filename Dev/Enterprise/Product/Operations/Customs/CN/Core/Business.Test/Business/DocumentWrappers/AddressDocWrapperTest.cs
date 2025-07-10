using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AddressDocWrapper))]
	class AddressDocWrapperTest : AddressWrapperTest
	{
		public void TestSetupCompanyNameAndAddressInChinese()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "WiseTechGlobal", "5th road", "Mascot", "AU", "Sydney", "2354", "VIC", "AUSYD", "PHONE", "FAX", "email@domain.com", "MOBILE", "Sh1", Factory);
			var address = organisation.MainAddress;
			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress.OTA_CompanyName = "慧咨全球";
			translatedAddress.OTA_Address1 = "第五大道";
			translatedAddress.OTA_Address2 = "麦斯考特";
			translatedAddress.OTA_City = "悉尼";
			translatedAddress.OTA_State = "新南威尔士";
			translatedAddress.OTA_PostCode = "2100";
			var wrapper = new AddressDocWrapper(OrganisationUsageType.Buyer, address, Factory);
			AssertEquals("慧咨全球", wrapper.CompanyNameInChinese);
			AssertEquals("慧咨全球\n第五大道\n麦斯考特\n悉尼 新南威尔士 2100\n澳大利亚", wrapper.CompanyNameAndAddressInChinese);
			AssertEquals("第五大道 麦斯考特 悉尼 新南威尔士 2100 澳大利亚", wrapper.AddressAsASingleLineInChinese);
		}

		public override void TestWrapperMappingsEmpty()
		{
			base.TestWrapperMappingsEmpty();
			var wrapperEmpty = new AddressDocWrapper(OrganisationUsageType.Test, (OrgAddress)null, Factory);
			AssertEquals("wrapperEmpty.CompanyNameInChinese", ZString.Empty, wrapperEmpty.CompanyNameInChinese);
			AssertEquals("wrapperEmpty.CompanyNameAndAddressInChinese", ZString.Empty, wrapperEmpty.CompanyNameAndAddressInChinese);
			AssertEquals("wrapperEmpty.AddressAsASingleLineInChinese", ZString.Empty, wrapperEmpty.AddressAsASingleLineInChinese);
			wrapperEmpty = new AddressDocWrapper(OrganisationUsageType.Test, (JobDocAddress)null, Factory);
			AssertEquals("wrapperEmpty.CompanyNameInChinese", ZString.Empty, wrapperEmpty.CompanyNameInChinese);
			AssertEquals("wrapperEmpty.CompanyNameAndAddressInChinese", ZString.Empty, wrapperEmpty.CompanyNameAndAddressInChinese);
			AssertEquals("wrapperEmpty.AddressAsASingleLineInChinese", ZString.Empty, wrapperEmpty.AddressAsASingleLineInChinese);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => new AddressDocWrapper(OrganisationUsageType.Test, (OrgAddress)null, Factory);

		protected override ZString ExpectedDefaultFormatting =>
			@"
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

		protected override string ExpectedFieldMap =>
			@"
AddressDoc                      (Default Field: CompanyNameAndAddress)
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
AddressAsASingleLine                    String
AddressAsASingleLineInChinese           String
AddressCaption                          String
AddressLine1                            String
AddressLine2                            String
City                                    String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
CompanyNameAndAddressInChinese          String
CompanyNameInChinese                    String
ContactName                             String
DeliverFromTime                         String
DeliverTimetable                        String
DeliverToTime                           String
DeliveryRoute                           String
DeliveryRouteSequence                   Short
DoNotAttendFromTime                     String
DoNotAttendToTime                       String
Email                                   String
Fax                                     String
FurtherConstraints                      String
HasDockLeveller                         Bool
HasForkLift                             Bool
HasLoadingUnloadingConstraints          Bool
HasPalletJack                           Bool
HasWarehousing                          Bool
Mobile                                  String
OtherWarehouseFacilities                String
Phone                                   String
PickupFromTime                          String
PickupTimetable                         String
PickupToTime                            String
PostCode                                String
ShortCode                               String
State                                   String

CustomsCodes                            RegistrationNumberCode Collection";

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "AUMEL";
			var address = organisation.MainAddress;
			address.OA_AccessPoint = OrgConstants.AccessPoint.Code.Dock;
			address.OA_CommunicationRequired = OrgConstants.CommunicationRequired.Code.Appointment;
			address.OA_ContainerHandling = OrgConstants.ContainerHandling.Code.Ask;
			address.OA_Dock_Height = OrgConstants.DockHeight.Code.NonStandard;
			address.OA_LabourRequired = OrgConstants.LabourRequired.Code.Ask;
			return new AddressDocWrapper(OrganisationUsageType.Test, organisation.MainAddress, Factory);
		}
	}
}
