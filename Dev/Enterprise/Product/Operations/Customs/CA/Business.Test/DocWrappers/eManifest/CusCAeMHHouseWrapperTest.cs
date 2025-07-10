using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("CusCAeMHHouseWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", house.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public void TestCusCAeMHHouseWrapperProperties()
		{
			AssertEquals(2, wrapper.Items.Count);
			var items = wrapper.Items.Cast<CusCAeMHItemWrapper>().OrderBy(x => x.Index).ToArray();

			var item1 = items[0];
			var item2 = items[1];
			AssertEquals(new ZShort(1), item1.Index);
			AssertEquals(new ZShort(2), item2.Index);
			AssertEquals("DGContactName", "DG CONTACT", wrapper.DGContactName);
			AssertEquals("DGPhoneNumber", "1112223355", wrapper.DGPhoneNumber);
			AssertEquals("CCC", wrapper.CargoControlNumber);
			AssertEquals("0OFC 0OFC-Desc", wrapper.ReleasePort);
			AssertEquals("2222 2222-Desc", wrapper.Sub_Location);
			AssertEquals(eMHMovementTypeList.Descriptions.Inbond, wrapper.MovementType);
			AssertEquals("C2", wrapper.ContainerIdentifier1);
			AssertEquals("S2", wrapper.SealNumber1);
			AssertEquals("DDD", wrapper.UCR);
			AssertEquals("85 Change request delayed by CBSA systems outage", wrapper.AmendmentReason);
			AssertEquals(true, wrapper.ConsolidatedIndicator);
			AssertEquals(456m, wrapper.Weight);
			AssertEquals(EManifestUnitOfWeightList.Descriptions.Kilogram, wrapper.WeightUQName);
			AssertEquals(123m, wrapper.Volume);
			AssertEquals(CustomsUnitOfMeasureList.Descriptions.CubicCentimetre, wrapper.VolumeUQName);
			AssertEquals("B2B TEST", wrapper.Business2Business);
			AssertEquals("DG Special Instruction", wrapper.DGSpecialInstructions);
			AssertEquals("Instructions", wrapper.SpecialInstructions);
			AssertEquals(shipper.OrganisationPK, wrapper.Shipper.OrganisationPK);
			AssertEquals(consignee.OrganisationPK, wrapper.Consignee.OrganisationPK);
			AssertEquals("DELIVERY DELIVERY2 ", wrapper.FormattedDeliveryAddresses.Replace("\r\n", ""));
			AssertEquals("NOTIFY NOTIFY2 ", wrapper.FormattedNotifyParties.Replace("\r\n", ""));
			AssertEquals("MASPOC", wrapper.PlaceOfConsolidation.Organisation.OH_Code);
			AssertEquals("MASCON", wrapper.Consolidator.Organisation.OH_Code);
		}

		protected override void SetUp()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(otherFactory);
			helper.CreateCusCodeType("SUBLC", "Sub Location Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "SUBLC", "2222", "2222-Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0OFC", "0OFC-Desc", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			otherFactory.Save();

			var poc = Factory.New<OrgHeader>();
			poc.OH_Code = "MASPOC";
			poc.OH_FullName = "Master POC";
			var pocAddress = poc.Addresses.AddNew();
			pocAddress.OA_Address1 = "Master POC Address";

			var con = Factory.New<OrgHeader>();
			con.OH_Code = "MASCON";
			con.OH_FullName = "Master CON";
			var conAddress = con.Addresses.AddNew();
			conAddress.OA_Address1 = "Master CON Address";
			Factory.Save();

			var dgContact = Factory.New<OrgContact>();
			dgContact.OC_ContactName = "DG CONTACT";
			dgContact.OC_Phone = "1112223355";

			base.SetUp();
			var master = Factory.New<CusCAeMHMaster>();
			master.PlaceOfConsolidation.OrganisationPK = poc.PK;
			master.Consolidator.OrganisationPK = con.PK;
			house = master.HouseBills.AddNew();
			house.BW_HouseCCN = "CCC";
			house.BW_CBSAReleasePort = "0OFC";
			house.BW_CBSAReleaseSubLocation = "2222";
			house.BW_MovementType = eMHMovementTypeList.Codes.Inbond;
			house.BW_UCR = "DDD";
			house.BW_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;
			house.BW_IsMasterHouse = true;
			house.BW_Weight = 456m;
			house.BW_WeightUQ = EManifestUnitOfWeightList.Codes.Kilogram;
			house.BW_Volume = 123m;
			house.BW_VolumeUQ = CustomsUnitOfMeasureList.Codes.CubicCentimetre;
			house.BW_B2BComments = "B2B TEST";
			house.BW_DGSpecialInstructions = "DG Special Instruction";
			house.BW_HandlingInstructions = "Instructions";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "SHIPPER";

			shipper = house.DocAddresses.AddNew();
			shipper.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
			shipper.OrganisationPK = org.PK;

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "CONSIGNEE";
			consignee = house.DocAddresses.AddNew();
			consignee.DocAddressType = DocAddressType.ConsigneeDocumentaryAddress;
			consignee.OrganisationPK = org.PK;

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "DELIVERY";
			var delivery = house.DocAddresses.AddNew();
			delivery.DocAddressType = DocAddressType.ConsigneePickupDeliveryAddress;
			delivery.OrganisationPK = org.PK;

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "DELIVERY2";
			var delivery2 = house.DocAddresses.AddNew();
			delivery2.DocAddressType = DocAddressType.ConsigneePickupDeliveryAddress;
			delivery2.OrganisationPK = org.PK;

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "NOTIFY";
			var notifyParty = house.DocAddresses.AddNew();
			notifyParty.DocAddressType = DocAddressType.NotifyParty;
			notifyParty.OrganisationPK = org.PK;

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "NOTIFY2";
			var notifyParty2 = house.DocAddresses.AddNew();
			notifyParty2.DocAddressType = DocAddressType.NotifyParty;
			notifyParty2.OrganisationPK = org.PK;

			house.BW_MessageReference = "XXX123456";
			house.BW_MessageStatus = "AAA";
			house.BW_CustomsStatus = "BBB";
			master.BP_PrimaryCCN = "EEE";
			master.BP_ModeOfTransport = "SEA";
			master.BP_CBSADischargePort = "JJJ";
			master.BP_CBSADischargeSubLocation = "KKK";

			var item1 = house.Items.AddNew();
			item1.BX_Description = "LINE1";
			item1.UNDGs.AddNew().DI_OC_DGContact = dgContact.PK;

			var item2 = house.Items.AddNew();
			item2.BX_Description = "LINE2";

			var container = master.Containers.AddNew();
			container.BQ_ContainerNumber = "C1";
			container.BQ_Seal1 = "S1";

			var container2 = master.Containers.AddNew();
			container2.BQ_ContainerNumber = "C2";
			container2.BQ_Seal1 = "S2";
			house.Pivots.AddNew().BPA_BQ_Container = container2.PK;

			wrapper = new CusCAeMHHouseWrapper(house);
		}

		CusCAeMHHouseWrapper wrapper;
		CAeMHDocAddress shipper;
		CAeMHDocAddress consignee;
		CusCAeMHHouse house;
	}
}
