using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaManifestHeader))]
sealed class CGMAsycudaManifestHeaderTest : AsycudaManifestHeaderAbstractTest
{
	public void TestValidation()
	{
		AssertType<CGMAsycudaManifestHeaderValidation>(Header.Validation);
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(INManifestTypes.Codes.CGM, Header.AMA_ManifestType);
		AssertEquals(ApplicationCodeTypeList.Codes.Consolidator, Header.AMA_ApplicationCode);
		AssertEquals(Core.Constants.TransportModes.Air, Header.AMA_TransportMode);
		AssertEquals(ShipmentTypeList.Codes.Total, Header.MasterBill.ABL_SpecialCargoCode);
	}

	public void TestSetDefaultValuesWhenTransportModeChange()
	{
		var header = Header;
		header.GrossWeightUQ = ZString.Empty;
		header.ManifestUQ = ZString.Empty;
		header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

		CombineAssertions(() =>
		{
			AssertEquals("GrossWeightUQ when sea", ZString.Empty, header.GrossWeightUQ);
			AssertEquals("ManifestUQ when sea", ZString.Empty, header.ManifestUQ);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("GrossWeightUQ when air", Core.Constants.Weight.Kilograms, header.GrossWeightUQ);
			AssertEquals("ManifestUQ when air", Core.Constants.PkgUnit.Package, header.ManifestUQ);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.GrossWeightUQ = Core.Constants.Weight.Grams;
			header.ManifestUQ = Core.Constants.PkgUnit.Bag;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air and not empty GrossWeightUQ", Core.Constants.Weight.Grams, header.GrossWeightUQ);
			AssertEquals("Air and not empty ManifestUQ", Core.Constants.PkgUnit.Bag, header.ManifestUQ);
		});
	}

	public void TestSetDefaultActionWhenTransportModeChange()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.MasterBill.ABL_BillStatus = ZString.Empty;
		header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

		CombineAssertions(() =>
		{
			AssertEquals("Sea, Empty Customs Status, Empty Message Status", ZString.Empty, header.MasterBill.ABL_BillStatus);

			header.AMA_MessageStatus = RegistrationStatusList.Codes.ManifestRegistered;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air, Empty Customs Status, not Empty Message Status", ZString.Empty, header.MasterBill.ABL_BillStatus);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.AMA_MessageStatus = ZString.Empty;
			header.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air, not Empty Customs Status, Empty Message Status", ZString.Empty, header.MasterBill.ABL_BillStatus);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.RegistrationStatus = ZString.Empty;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air, Empty Customs Status, Empty Message Status", ManifestMessageTypeList.Codes.Fresh, header.MasterBill.ABL_BillStatus);
		});
	}

	public void TestActionSetterPopupMessage()
	{
		CombineAssertions(() =>
		{
			var bill = Header.Bills.AddNew();
			Header.Action = ManifestMessageTypeList.Codes.Fresh;
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			Header.Action = ManifestMessageTypeList.Codes.Amendment;
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			Globals.IsUserInteractive = false;
			Header.Action = ManifestMessageTypeList.Codes.Delete;
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			Header.Action = ManifestMessageTypeList.Codes.Fresh;
			Globals.IsUserInteractive = true;
			Header.Action = ManifestMessageTypeList.Codes.Delete;
			AssertEquals("Action = D - Delete at Header level will also set all house bills Action to D - Delete.", UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestSetActionToAmendmentOnSaving()
	{
		AssertDefaultSetInDifferentCustomsStatus(RegistrationStatusList.Codes.ManifestRegistered, ManifestMessageTypeList.Codes.Amendment);
		AssertDefaultSetInDifferentCustomsStatus(RegistrationStatusList.Codes.ManifestIsUnderAmendment, ManifestMessageTypeList.Codes.Amendment);
		AssertDefaultSetInDifferentCustomsStatus(ZString.Empty, ManifestMessageTypeList.Codes.Fresh);

		void AssertDefaultSetInDifferentCustomsStatus(string registrationStatus, string billStatus)
		{
			var header = Factory.New<CGMAsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			header.RegistrationStatus = registrationStatus;
			header.ImportGeneralManifestNumber = "ASD";
			header.ImportGeneralManifestDate = new ZDate(2024, 09, 18);

			CombineAssertions($"{registrationStatus}", () =>
			{
				Factory.Save();
				AssertEquals("ImportGeneralManifestNumber not Change when first create entry number", ManifestMessageTypeList.Codes.Fresh, header.Action);

				header.ImportGeneralManifestNumber = "ASDD";
				header.OnSaving();
				AssertEquals("ImportGeneralManifestNumber Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();
				AssertNotNull("ImportGeneralManifestEntryNumber created", CusEntryNumber.Load(header, CusEntryNumberTypes.Indian.ImportGeneralManifest, header.AMA_RN_NKCountry));

				header.ImportGeneralManifestNumber = ZString.Empty;
				header.OnSaving();
				AssertEquals("ImportGeneralManifestNumber Change to Empty", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.ImportGeneralManifestDate = new ZDate(2024, 09, 11);
				header.OnSaving();
				AssertEquals("ImportGeneralManifestDate Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.ImportGeneralManifestDate = ZDate.Empty;
				header.OnSaving();
				AssertEquals("ImportGeneralManifestDate Change to Empty", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();
				AssertNull("ImportGeneralManifestEntryNumber deleted", CusEntryNumber.Load(header, CusEntryNumberTypes.Indian.ImportGeneralManifest, header.AMA_RN_NKCountry));

				header.ImportGeneralManifestNumber = "Set";
				header.ImportGeneralManifestNumber = ZString.Empty;
				header.OnSaving();
				AssertEquals("ImportGeneralManifestNumber set a value then set to empty", ManifestMessageTypeList.Codes.Fresh, header.Action);
				Factory.Save();

				header.AMA_E_ARV = new ZDateTime(2024, 09, 10);
				header.OnSaving();
				AssertEquals("AMA_E_ARV Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.AMA_MasterBillIssueDate = new ZDate(2024, 09, 10);
				header.OnSaving();
				AssertEquals("AMA_MasterBillIssueDate Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.AMA_RL_NKOrigin = "XY";
				header.OnSaving();
				AssertEquals("AMA_RL_NKOrigin Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.MasterBill.ABL_SpecialCargoCode = "X";
				header.OnSaving();
				AssertEquals("ABL_SpecialCargoCode Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.AMA_GoodsDescription = "XYZ";
				header.OnSaving();
				AssertEquals("AMA_GoodsDescription Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.AMA_Voyage = "XYZ";
				header.OnSaving();
				AssertEquals("AMA_Voyage Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.AMA_RL_NKFinalDestination = "XY";
				header.OnSaving();
				AssertEquals("AMA_RL_NKFinalDestination Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.ManifestQty = 12;
				header.OnSaving();
				AssertEquals("ManifestQty Change", billStatus, header.Action);
				header.Action = ManifestMessageTypeList.Codes.Fresh;
				Factory.Save();

				header.GrossWeight = 1m;
				header.OnSaving();
				AssertEquals("GrossWeight Change", billStatus, header.Action);
			});
		}
	}

	public void TestAction()
	{
		AssertEquals("MaxLength", CGMAsycudaManifestHeader.Schema.ActionMaxLength, Header.ActionInfo.MaxLength);

		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(Header.ActionInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Action", resourceStringData.Caption);
			AssertEquals("Medium Caption", "Action", resourceStringData.MediumCaption);
			AssertEquals("Short Caption", "Act.", resourceStringData.ShortCaption);
		});

		CombineAssertions(() =>
		{
			AssertEquals("MessageStatus and CustomsStatus are empty", true, Header.ActionInfo.ReadOnly);
			Header.AMA_MessageStatus = "REG";
			AssertEquals("MessageStatus is not empty", false, Header.ActionInfo.ReadOnly);
			Header.AMA_MessageStatus = ZString.Empty;
			Header.RegistrationStatus = "REG";
			AssertEquals("CustomsStatus is not empty", false, Header.ActionInfo.ReadOnly);

			Header.Action = ManifestMessageTypeList.Codes.Fresh;
			AssertEquals("Action = Fresh, not in db", false, Header.ActionInfo.ReadOnly);
			Header.Action = ManifestMessageTypeList.Codes.Delete;
			AssertEquals("Action = Delete, not in db", false, Header.ActionInfo.ReadOnly);
			Header.Action = ManifestMessageTypeList.Codes.Amendment;
			AssertEquals("Action = Amendment, not in db", false, Header.ActionInfo.ReadOnly);
			Factory.Save();

			AssertEquals("Action = Amendment, Action = Amendment in db", false, Header.ActionInfo.ReadOnly);
			Header.Action = ManifestMessageTypeList.Codes.Fresh;
			AssertEquals("Action = Fresh, Action = Amendment in db", false, Header.ActionInfo.ReadOnly);
			Header.Action = ManifestMessageTypeList.Codes.Delete;
			AssertEquals("Action = Delete, Action = Amendment in db", false, Header.ActionInfo.ReadOnly);
			Factory.Save();

			AssertEquals("Action = Delete in db", true, Header.ActionInfo.ReadOnly);
		});
	}

	public void TestImportGeneralManifestNumberWithEntryNum()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When EntryNum not present", ZString.Empty, Header.ImportGeneralManifestNumber);

			var entryNum = CreateNewEntryNum();
			entryNum.CE_EntryNum = "123";
			AssertEquals("When CE_EntryNum = 123, ImportGeneralManifestNumber", "123", Header.ImportGeneralManifestNumber);

			Header.ImportGeneralManifestNumber = "999";
			AssertEquals("When ImportGeneralManifestNumber = 999, CE_EntryNum", "999", entryNum.CE_EntryNum);

			Header.ImportGeneralManifestNumber = "";
			Factory.Save();
			AssertNull("When ImportGeneralManifestNumber empty", Factory.Load<CusEntryNumber>(entryNum.PK));
		});
	}

	public void TestImportGeneralManifestNumber()
	{
		AssertEquals("MaxLength", 7, Header.ImportGeneralManifestNumberInfo.MaxLength);

		var resData = DataBoundResourceStrings.GetDataForProperty(Header.ImportGeneralManifestNumberInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "IGM Number", resData.Caption);
			AssertEquals("MediumCaption", "IGM No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "IGM No.", resData.ShortCaption);
		});
	}

	public void TestImportGeneralManifestDateWithEntryNum()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When EntryNum not present", ZDate.Empty, Header.ImportGeneralManifestDate);

			var entryNum = CreateNewEntryNum();
			entryNum.CE_IssueDate = new ZDate(2022, 3, 16);
			AssertEquals("When CE_IssueDate = 123", new ZDate(2022, 3, 16), Header.ImportGeneralManifestDate);

			Header.ImportGeneralManifestDate = new ZDate(2022, 3, 19);
			AssertEquals("When ImportGeneralManifestDate = 999", new ZDate(2022, 3, 19), entryNum.CE_IssueDate);

			Header.ImportGeneralManifestDate = ZDate.Empty;
			Factory.Save();
			AssertNull("When ImportGeneralManifestNumber empty", Factory.Load<CusEntryNumber>(entryNum.PK));
		});
	}

	public void TestImportGeneralManifestDateCaptions()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Header.ImportGeneralManifestDateInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "IGM Date", resData.Caption);
			AssertEquals("MediumCaption", "IGM Date", resData.MediumCaption);
			AssertEquals("ShortCaption", "IGM Dt.", resData.ShortCaption);
		});
	}

	public void TestImportGeneralManifestEntryNumberWithIGMNumberAndDate()
	{
		CombineAssertions(() =>
		{
			AssertNull("When IGM number and date are not entered", CusEntryNumber.Load(Header, "IGM", "IN"));

			Header.ImportGeneralManifestDate = new ZDate(2022, 3, 16);
			Header.ImportGeneralManifestNumber = "1234";
			AssertNotNull("When IGM number and date are present", CusEntryNumber.Load(Header, "IGM", "IN"));

			Header.ImportGeneralManifestDate = ZDate.Empty;
			AssertNotNull("When IGM number filled and date is empty", CusEntryNumber.Load(Header, "IGM", "IN"));

			Header.ImportGeneralManifestDate = new ZDate(2022, 3, 16);
			Header.ImportGeneralManifestNumber = ZString.Empty;
			AssertNotNull("When IGM number empty and date is filled", CusEntryNumber.Load(Header, "IGM", "IN"));

			Header.ImportGeneralManifestDate = ZDate.Empty;
			Factory.Save();
			AssertNull("When IGM number and date are empty", CusEntryNumber.Load(Header, "IGM", "IN"));
		});
	}

	public void TestAMA_RL_NKOriginCaptions()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.AMA_RL_NKOriginInfo, CGMAsycudaManifestHeader.CaptionKeySea);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Port Of Shipment", resData.Caption);
			AssertEquals("MediumCaption", "Port Of Shp.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Port Of Shp.", resData.ShortCaption);
		});

		resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.AMA_RL_NKOriginInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Port of Origin", resData.Caption);
			AssertEquals("MediumCaption", "Origin", resData.MediumCaption);
			AssertEquals("ShortCaption", "Origin", resData.ShortCaption);
		});
	}

	public void TestABL_RL_NKFinalDestinationCaptions()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Header.AMA_RL_NKFinalDestinationInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Port Of Destination", resData.Caption);
			AssertEquals("MediumCaption", "Destination", resData.MediumCaption);
			AssertEquals("ShortCaption", "Destination", resData.ShortCaption);
		});
	}

	public void TestCustomsDischargePortCaptions()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Header.AMA_CustomsDischargePortInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Port Of Destination", resData.Caption);
			AssertEquals("MediumCaption", "Port Of Dest.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Port Of Dest.", resData.ShortCaption);
		});
	}

	public void TestAMA_CarrierReference()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When ABL_CarrierReference not present", ZString.Empty, Header.AMA_CarrierReference);

			Header.MasterBill.ABL_CarrierReference = "123";
			AssertEquals("When set ABL_CarrierReference = 123, AMA_CarrierReference ", "123", Header.AMA_CarrierReference);

			Header.AMA_CarrierReference = "234";
			AssertEquals("When set AMA_CarrierReference = 234, ABL_CarrierReference ", "234", Header.MasterBill.ABL_CarrierReference);
		});
	}

	public void TestAMA_CarrierReferenceCaptions()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Header.AMA_CarrierReferenceInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Line Number", resData.Caption);
			AssertEquals("MediumCaption", "Line No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Line No.", resData.ShortCaption);
		});
	}

	public void TestAMA_CarrierReferenceMaxLength()
	{
		AssertEquals("MaxLength", 4, Header.AMA_CarrierReferenceInfo.MaxLength);
	}

	public void TestLookups()
	{
		AssertType<CGMAsycudaManifestHeaderLookups>(Header.Lookups);
	}

	public void TestGetBillType()
	{
		AssertEquals("Bill type", typeof(CGMAsycudaBill), Header.GetBillType());
	}

	public void TestBills()
	{
		AssertType<CGMAsycudaBillCollection>("Type", Header.Bills);
	}

	public void TestGetContainerType()
	{
		AssertEquals("Container Type", typeof(CGMAsycudaContainer), Header.GetContainerType());
	}

	public void TestAMA_ContainerMode()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.AMA_ContainerModeInfo, CGMAsycudaManifestHeader.CaptionKeySea);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Nature Of Cargo", resData.Caption);
			AssertEquals("MediumCaption", "Cargo", resData.MediumCaption);
			AssertEquals("ShortCaption", "Cargo", resData.ShortCaption);
		});

		resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.AMA_ContainerModeInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		AssertEquals("Caption", "Container Mode", resData.Caption);
	}

	public void TestAMA_MasterBillIssueDate()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.AMA_MasterBillIssueDateInfo, CGMAsycudaManifestHeader.CaptionKeySea);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "BOL Date", resData.Caption);
			AssertEquals("MediumCaption", "BOL Dt.", resData.MediumCaption);
			AssertEquals("ShortCaption", "BL Dt.", resData.ShortCaption);
		});

		resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.AMA_MasterBillIssueDateInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "MAWB Date", resData.Caption);
			AssertEquals("MediumCaption", "MAWB Dt.", resData.MediumCaption);
			AssertEquals("ShortCaption", "MAWB Dt.", resData.ShortCaption);
		});
	}

	public void TestMultipleKeysToUse()
	{
		Header.AMA_TransportMode = "AIR";
		CombineAssertions(() =>
		{
			AssertEquals("AIR", CGMAsycudaManifestHeader.CaptionKeyAir, Header.MultipleKeysToUse.Single());
			Header.AMA_TransportMode = "SEA";
			AssertEquals("SEA", CGMAsycudaManifestHeader.CaptionKeySea, Header.MultipleKeysToUse.Single());
		});
	}

	public void TestAMA_GoodsDescriptionResData()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Header.AMA_GoodsDescriptionInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Cargo Description", resData.Caption);
			AssertEquals("MediumCaption", "Cargo Desc.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Desc.", resData.ShortCaption);
		});
	}

	public void TestMasterBillMaxLength()
	{
		AssertEquals("MaxLength", CGMAsycudaManifestHeader.Schema.MasterBillMaxLength, Header.AMA_MasterBillInfo.MaxLength);
	}

	public void TestGetDocWrapper()
	{
		Header.AMA_TransportMode = ZString.Empty;
		AssertType<ASYCUDA.Business.AsycudaManifestHeaderDocWrapper>("TransportMode cleared", Header.GetDocWrapper());

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = "AIR";
			AssertType<CGMAirAsycudaManifestHeaderDocWrapper>("For Air", Header.GetDocWrapper());

			Header.AMA_TransportMode = "SEA";
			AssertType<CGMSeaAsycudaManifestHeaderDocWrapper>("For Sea", Header.GetDocWrapper());
		});
	}

	public void TestGrossWeightResData()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.GrossWeightInfo, AsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Total Gross Weight", resData.Caption);
			AssertEquals("MediumCaption", "Gross Weight", resData.MediumCaption);
			AssertEquals("ShortCaption", "Gr. Wt.", resData.ShortCaption);
		});
	}

	public void TestGrossWeightDefault()
	{
		Header.GrossWeightUQ = ZString.Empty;
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		AssertEquals("CGM SEA doesn't defaults GrossWeightUQ", ZString.Empty, Header.GrossWeightUQ);

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		AssertEquals("CGM AIR defaults GrossWeightUQ", Core.Constants.Weight.Kilograms, Header.GrossWeightUQ);
	}

	public void TestGrossWeightUQ()
	{
		Header.GrossWeightUQ = Core.Constants.Weight.Kilograms;
		AssertEquals("GrossWeightUQ", Core.Constants.Weight.Kilograms, Header.MasterBill.ABL_GrossWeightUQ);
	}

	public void TestGrossWeight()
	{
		Header.GrossWeight = 100m;
		AssertEquals("GrossWeight", 100m, Header.MasterBill.ABL_GrossWeight);
	}

	public void TestPacksWithContainerType()
	{
		AssertType<CGMAsycudaPack[]>(Header.PacksWithContainer);
	}

	public void TestPacksWithContainerCached()
	{
		AssertSame(Header.PacksWithContainer, Header.PacksWithContainer);
	}

	public void TestPacksWithContainer()
	{
		CombineAssertions(() =>
		{
			var container = Header.Containers.AddNew();
			var bill1 = Header.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			bill1.Packs.AddNew();
			AssertEquals("Packs", 2, bill1.Packs.Count);
			AssertEquals("Before assigning containers to Packs, PacksWithContainer count", 0, Header.PacksWithContainer.Count);

			pack1.ContainerPK = container.PK;
			AssertEquals("After assigning containers to Packs, PacksWithContainer count", 1, Header.PacksWithContainer.Count);

			var bill2 = Header.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.ContainerPK = container.PK;
			AssertEquals("Add container on another Bill, PacksWithContainer count", 2, Header.PacksWithContainer.Count);
		});
	}

	public void TestManifestQtyResData()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Header.ManifestQtyInfo, AsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Total Packages", resData.Caption);
			AssertEquals("MediumCaption", "Packages", resData.MediumCaption);
			AssertEquals("ShortCaption", "Packs.", resData.ShortCaption);
		});
	}

	public void TestManifestUQ()
	{
		Header.ManifestUQ = Core.Constants.PkgUnit.Bag;
		AssertEquals("ManifestUQ", Core.Constants.PkgUnit.Bag, header.MasterBill.ABL_ManifestUQ);
	}

	public void TestManifestQty()
	{
		Header.ManifestQty = 10;
		AssertEquals("ManifestQty", 10, header.MasterBill.ABL_ManifestQty);
	}

	public void TestConsolSynchronizer()
	{
		var consol = Factory.New<ForwardingConsol>();
		Header.SetParent(consol);
		AssertType<CGMAsycudaManifestHeaderSynchroniser>("Type", Header.Synchroniser);
	}

	public void TestReadOnlyMembersAfterAcceptedAtCustoms()
	{
		string GetMessage() => $"AMA_TransportMode = {Header.AMA_TransportMode}, RegistrationStatus = {Header.RegistrationStatus}";

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Header.RegistrationStatus = "ACK";
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_LloydsNumberInfo, true);
			AssertReadOnlyProperty(Header.AMA_RadioCallSignInfo, true);
			AssertReadOnlyProperty(Header.AMA_VoyageInfo, true);
			AssertReadOnlyProperty(Header.AMA_CustomsOfficeInfo, true);
			AssertReadOnlyProperty(Header.AMA_MasterBillInfo, false);
		});

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Header.RegistrationStatus = ZString.Empty;
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_LloydsNumberInfo, false);
			AssertReadOnlyProperty(Header.AMA_RadioCallSignInfo, false);
			AssertReadOnlyProperty(Header.AMA_VoyageInfo, false);
			AssertReadOnlyProperty(Header.AMA_CustomsOfficeInfo, false);
			AssertReadOnlyProperty(Header.AMA_MasterBillInfo, false);
		});

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		Header.RegistrationStatus = "ACK";
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_LloydsNumberInfo, false);
			AssertReadOnlyProperty(Header.AMA_RadioCallSignInfo, false);
			AssertReadOnlyProperty(Header.AMA_VoyageInfo, false);
			AssertReadOnlyProperty(Header.AMA_CustomsOfficeInfo, true);
			AssertReadOnlyProperty(Header.AMA_MasterBillInfo, true);
		});

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		Header.RegistrationStatus = ZString.Empty;
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_LloydsNumberInfo, false);
			AssertReadOnlyProperty(Header.AMA_RadioCallSignInfo, false);
			AssertReadOnlyProperty(Header.AMA_VoyageInfo, false);
			AssertReadOnlyProperty(Header.AMA_CustomsOfficeInfo, false);
			AssertReadOnlyProperty(Header.AMA_MasterBillInfo, false);
		});
	}

	public void TestReadOnlyMembersAfterFirstMessageSent()
	{
		string GetMessage() => $"AMA_TransportMode = {Header.AMA_TransportMode}, Messages count = {Header.Messages.Count}";

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Header.Messages.AddNew();
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_TransportModeInfo, false);
			AssertReadOnlyProperty(Header.AMA_ManifestTypeInfo, false);
			AssertReadOnlyProperty(Header.AMA_NatureInfo, false);
		});

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Header.Messages.RemoveAll();
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_TransportModeInfo, false);
			AssertReadOnlyProperty(Header.AMA_ManifestTypeInfo, false);
			AssertReadOnlyProperty(Header.AMA_NatureInfo, false);
		});

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		Header.Messages.AddNew();
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_TransportModeInfo, true);
			AssertReadOnlyProperty(Header.AMA_ManifestTypeInfo, true);
			AssertReadOnlyProperty(Header.AMA_NatureInfo, true);
		});

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		Header.Messages.RemoveAll();
		CombineAssertions(GetMessage(), () =>
		{
			AssertReadOnlyProperty(Header.AMA_TransportModeInfo, false);
			AssertReadOnlyProperty(Header.AMA_ManifestTypeInfo, false);
			AssertReadOnlyProperty(Header.AMA_NatureInfo, false);
		});
	}

	public void TestStatusOverrideResData()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Header.StatusOverrideInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Override", resData.Caption);
			AssertEquals("FullDescription", "Override Message Status and Customs Status", resData.FullDescription);
		});
	}

	public void TestSetCustomsStatusEntryNumberOnSaving()
	{
		var headerAir = Factory.New<CGMAsycudaManifestHeader>();
		headerAir.AMA_TransportMode = Core.Constants.TransportModes.Air;
		headerAir.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
		headerAir.AMA_MasterBill = "AMA";

		var headerSea = Factory.New<CGMAsycudaManifestHeader>();
		headerSea.AMA_TransportMode = Core.Constants.TransportModes.Sea;
		headerSea.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
		headerSea.AMA_MasterBill = "AMB";
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Air Entry Number not in database", "AMA", headerAir.RegistrationEntryNumber.CE_EntryNum);
			AssertEquals("Sea Entry Number not in database", "AMB", headerSea.RegistrationEntryNumber.CE_EntryNum);

			headerAir.RegistrationStatus = RegistrationStatusList.Codes.ManifestIsUnderAmendment;
			headerAir.AMA_MasterBill = "POS";
			headerSea.RegistrationStatus = RegistrationStatusList.Codes.ManifestIsUnderAmendment;
			headerSea.AMA_MasterBill = "NEG";
			Factory.Save();
			AssertEquals("Air Entry Number in dataBase", "AMA", headerAir.RegistrationEntryNumber.CE_EntryNum);
			AssertEquals("Sea Entry Number in dataBase", "AMB", headerSea.RegistrationEntryNumber.CE_EntryNum);
		});
	}

	public void TestMessageStatusReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertReadOnlyProperty(Header.AMA_MessageStatusInfo, true);
			Header.StatusOverride = true;
			AssertReadOnlyProperty(Header.AMA_MessageStatusInfo, false);
		});
	}

	public void TestRegistrationStatusReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertReadOnlyProperty(Header.RegistrationStatusInfo, true);
			Header.StatusOverride = true;
			AssertReadOnlyProperty(Header.RegistrationStatusInfo, false);
		});
	}

	public void TestStatusOverride()
	{
		Header.StatusOverride = true;
		Header.AMA_MessageStatus = "ACP";
		Header.RegistrationStatus = "REG";
		Header.StatusOverride = false;
		AssertEquals("Not in database and untick override, AMA_MessageStatus", ZString.Empty, Header.AMA_MessageStatus);
		AssertEquals("Not in database and untick override, RegistrationStatus", ZString.Empty, Header.RegistrationStatus);

		Header.StatusOverride = true;
		Header.AMA_MessageStatus = "ACP";
		Header.RegistrationStatus = "REG";
		Factory.Save();
		Assert("Status Override untick after saving", !Header.StatusOverride);

		Header.StatusOverride = true;
		Header.AMA_MessageStatus = "ERR";
		Header.RegistrationStatus = "FAL";
		Header.StatusOverride = false;
		AssertEquals("Before save and untick override, AMA_MessageStatus", "ACP", Header.AMA_MessageStatus);
		AssertEquals("Before save and untick override, RegistrationStatus", "REG", Header.RegistrationStatus);

		Header.StatusOverride = true;
		Header.AMA_MessageStatus = "ERR";
		Header.RegistrationStatus = "FAL";
		Header.AMA_OA_Carrier = ZGuid.Invalid;
		AssertExceptionThrown<ZSaveException>(Factory.Save);
		Assert("Status Override unchanged after Save failed", Header.StatusOverride);
		AssertEquals("Save failed, AMA_MessageStatus", "ERR", Header.AMA_MessageStatus);
		AssertEquals("Save failed, RegistrationStatus", "FAL", Header.RegistrationStatus);

		Header.StatusOverride = false;
		AssertEquals("Untick override, AMA_MessageStatus", "ACP", Header.AMA_MessageStatus);
		AssertEquals("Untick override, RegistrationStatus", "REG", Header.RegistrationStatus);

		Header.StatusOverride = true;
		Header.AMA_MessageStatus = "ERR";
		Header.RegistrationStatus = "FAL";
		Header.AMA_OA_Carrier = ZGuid.Empty;
		Factory.Save();
		Assert("Status Override untick after saving", !Header.StatusOverride);
		AssertEquals("After save, AMA_MessageStatus", "ERR", Header.AMA_MessageStatus);
		AssertEquals("After save, RegistrationStatus", "FAL", Header.RegistrationStatus);

		Header.StatusOverride = true;
		Header.AMA_MessageStatus = "ACP";
		Header.RegistrationStatus = "REG";
		Header.StatusOverride = false;
		AssertEquals("Untick override, AMA_MessageStatus", "ERR", Header.AMA_MessageStatus);
		AssertEquals("Untick override, RegistrationStatus", "FAL", Header.RegistrationStatus);
	}

	void AssertReadOnlyProperty(ZPropertyInfo info, bool expectedReadOnly)
	{
		ZString message = $"The readonly of {info.Name} should be {expectedReadOnly}.";
		AssertEquals(message, expectedReadOnly, info.ReadOnly);
	}

	public void TestPopulateAMA_CustomsOffice()
	{
		var today = ZDateTime.Now;
		var startDate = today.AddDays(-10);
		var endDate = today.AddDays(10);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "India Customs EDI Location");
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "AB1234", startDate, endDate, RefTransportModeList.Codes.AIR);
		Factory.Save();

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Header.AMA_CustomsOffice = ZString.Empty;
			Header.AMA_RL_NKFinalDestination = "AB123";
			AssertEquals("When NotAir and AMA_CustomsOffice is not empty, should not populate", ZString.Empty, Header.AMA_CustomsOffice);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Header.AMA_CustomsOffice = "CD1234";
			Header.AMA_RL_NKFinalDestination = "AB123";
			AssertEquals("When IsAir and AMA_CustomsOffice is not empty, should not populate", "CD1234", Header.AMA_CustomsOffice);

			Header.AMA_CustomsOffice = ZString.Empty;
			Header.AMA_RL_NKFinalDestination = "AB456";
			Header.AMA_RL_NKFinalDestination = "AB123";
			AssertEquals("When IsAir and AMA_CustomsOffice is empty and code is valid, should populate", "AB1234", Header.AMA_CustomsOffice);

			Header.AMA_CustomsOffice = ZString.Empty;
			Header.AMA_RL_NKFinalDestination = "EF123";
			AssertEquals("When IsAir and AMA_CustomsOffice is empty and code is invalid, should not populate", ZString.Empty, Header.AMA_CustomsOffice);

			Header.AMA_CustomsOffice = ZString.Empty;
			Header.AMA_RL_NKFinalDestination = ZString.Empty;
			AssertEquals("When IsAir and AMA_CustomsOffice is empty and code is invalid, should not populate", ZString.Empty, Header.AMA_CustomsOffice);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Header.AMA_RL_NKFinalDestination = "AB123";
			Header.AMA_CustomsOffice = ZString.Empty;
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("When switch to Air and AMA_CustomsOffice is empty and code is valid, should populate", "AB1234", Header.AMA_CustomsOffice);
		});

		void CreateCustomsOfficeCode(ZString dataGroupingCode, ZString code, ZDateTime startDate, ZDateTime endDate, params string[] transportModes)
		{
			var codeList = helper.CreateCusCodeList(dataGroupingCode, RefCusCodeListTypes.Codes.CustomsOffice, code, startDate, endDate);
			Array.ForEach(transportModes, x => helper.CreateTransportModeForCusCodeList(codeList.PK, x));
		}
	}

	protected override ZString GetExpectedCustomsStatus(ZString messageType)
	{
		return messageType.ToString() switch
		{
			ManifestMessageTypeList.Codes.Amendment => RegistrationStatusList.Codes.ManifestIsUnderAmendment,
			ManifestMessageTypeList.Codes.Delete => RegistrationStatusList.Codes.ManifestDeleteRequisition,
			_ => string.Empty
		};
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		header.SuspendCheckBusinessObjectType();
		return header;
	}

	protected override Type ExpectedTypeOfContainer => typeof(CGMAsycudaContainerCollection);

	CusEntryNumber CreateNewEntryNum()
	{
		var entryNum = Factory.New<CusEntryNumber>();
		entryNum.CE_ParentID = Header.PK;
		entryNum.CE_ParentTable = Header.TableName;
		entryNum.CE_EntryType = "IGM";
		entryNum.CE_RN_NKCountryCode = "IN";
		return entryNum;
	}

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
