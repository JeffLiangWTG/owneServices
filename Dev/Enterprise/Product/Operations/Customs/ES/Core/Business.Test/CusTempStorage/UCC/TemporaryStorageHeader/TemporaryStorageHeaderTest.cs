using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageHeader))]
sealed class TemporaryStorageHeaderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageHeaderAbstractTest<TemporaryStorageHeader>
{
	public void TestLookups()
	{
		AssertType<TemporaryStorageHeaderLookups>(header.Lookups);
	}

	public void TestValidation()
	{
		AssertType<TemporaryStorageHeaderValidation>(header.Validation);
	}

	public void TestBills()
	{
		AssertType<TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>>(header.Bills);
	}

	public void TestEntryStatus()
	{
		CodeDescriptionPairList circuitCodeList = Factory.GetCachedValue<CircuitCodeList>();

		var entryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		SetUpEntryData(entryType);

		CombineAssertions(() =>
		{
			header.EntryStatus = ZString.Empty;
			AssertEquals(ZString.Empty, header.EntryStatus);

			header.EntryStatus = CircuitCodeList.Codes.GREEN;
			AssertEquals(circuitCodeList.GetDescriptionFromCode(CircuitCodeList.Codes.GREEN), header.EntryStatus);

			header.EntryStatus = CircuitCodeList.Codes.RED;
			AssertEquals(circuitCodeList.GetDescriptionFromCode(CircuitCodeList.Codes.RED), header.EntryStatus);

			header.EntryStatus = CircuitCodeList.Codes.ORANGE;
			AssertEquals(circuitCodeList.GetDescriptionFromCode(CircuitCodeList.Codes.ORANGE), header.EntryStatus);
		});
	}

	public void TestAcceptanceDate()
	{
		var entryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		SetUpEntryData(entryType);
		header.Reload();
		AssertEquals($"{entryType} Issue Date can be loaded correctly", ZDate.BrettsBirthday, header.GetType().GetProperty(nameof(TemporaryStorageHeader.AcceptanceDate)).GetValue(header));
	}

	public void TestEntryDate()
	{
		var entryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
		SetUpEntryData(entryType);
		header.Reload();
		AssertEquals($"{entryType} Issue Date can be loaded correctly", ZDate.BrettsBirthday, header.GetType().GetProperty(nameof(TemporaryStorageHeader.EntryDate)).GetValue(header));
	}

	public void TestClearanceDate()
	{
		var entryType = CusEntryNumberTypes.Spain.ClearanceCSV;
		SetUpEntryData(entryType);
		header.Reload();
		AssertEquals($"{entryType} Issue Date can be loaded correctly", ZDate.BrettsBirthday, header.GetType().GetProperty(nameof(TemporaryStorageHeader.ClearanceDate)).GetValue(header));
	}

	public void TestEntryNumber()
	{
		var entryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
		SetUpEntryData(entryType);
		header.Reload();
		AssertEquals($"{entryType} number can be loaded correctly", "12345678901234567890", header.GetType().GetProperty(nameof(TemporaryStorageHeader.EntryNumber)).GetValue(header));
	}

	public void TestClearanceNumber()
	{
		var entryType = CusEntryNumberTypes.Spain.ClearanceCSV;
		SetUpEntryData(entryType);
		header.Reload();
		AssertEquals($"{entryType} number can be loaded correctly", "12345678901234567890", header.GetType().GetProperty(nameof(TemporaryStorageHeader.ClearanceNumber)).GetValue(header));
	}

	public void TestDsdtMrnNumber()
	{
		var entryType = CusEntryNumberTypes.Spain.SummaryEntryNumber;
		SetUpEntryData(entryType);
		header.Reload();
		AssertEquals($"{entryType} number can be loaded correctly", "12345678901234567890", header.GetType().GetProperty(nameof(TemporaryStorageHeader.DsdtMrnNumber)).GetValue(header));
	}

	public void TestDsdtMrnNumberSdFormat()
	{
		SetUpEntryData(CusEntryNumberTypes.Spain.SummaryEntryNumber);
		header.Reload();
		AssertEquals("DsdtMrnNumberSdFormat should be 78902234567", "78902234567", header.DsdtMrnNumberSdFormat);
	}

	public void TestDsdtSummaryDeclarationUrl()
	{
		var summaryUrl = "https://url.com?Recinto=%recinto%&Anio=%anio%&Numero=%numero%&MRN=%mrn%";
		using (ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, summaryUrl))
		{
			AssertEquals($"No Summary Declaration", ZString.Empty, header.DsdtSummaryDeclarationUrl);

			var newEntryNumberSummary = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumberSummary.CE_EntryNum = "99994000152";
			newEntryNumberSummary.CE_EntryIsSystemGenerated = true;

			Factory.Save();

			var expected = "https://url.com?Recinto=9999&Anio=4&Numero=000152&MRN=";
			AssertEquals("Summary Declaration with length of 11 set", expected, header.DsdtSummaryDeclarationUrl);

			newEntryNumberSummary.CE_EntryNum = "24ES00999980002228";
			Factory.Save();

			expected = "https://url.com?Recinto=&Anio=&Numero=&MRN=24ES00999980002228";
			AssertEquals("Summary Declaration with length longer than 11 set", expected, header.DsdtSummaryDeclarationUrl);
		}
	}

	public void TestTrainingEntry_GenAddOnColumnAndDefault()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("TrainingEntry should be true by default", true, header.TrainingEntry);

			header.TrainingEntry = false;
			AssertEquals("TrainingEntry should be false when set", false, header.TrainingEntry);
		});
	}

	public void TestTrainingEntryPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "TrainingEntry");

		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.TrainingEntry = true;

		CombineAssertions(() =>
		{
			AssertEquals("TrainingEntry", true, header.TrainingEntry);
			AssertNotNull("TrainingEntry is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestTrainingEntryDefault()
	{
		var productRegistrationMock = new Mock<IProductRegistration>();
		productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
		using (ObjectFactory.Substitute(productRegistrationMock.Object))
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("TrainingEntry is false when not internal environment", false, header.TrainingEntry);
		}

		productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
		using (ObjectFactory.Substitute(productRegistrationMock.Object))
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("TrainingEntry is true when internal environment", true, header.TrainingEntry);
		}
	}

	public void TestStoVersion_GenAddOnColumnAndDefault()
	{
		CombineAssertions(() =>
		{
			AssertEquals("StoVersion should be V1 by default", "V1", header.StoVersion);

			header.StoVersion = "AA";
			AssertEquals("StoVersion should be AA when set", "AA", header.StoVersion);

			AssertEquals("StoVersion maxlength is 2", 2, header.StoVersionInfo.MaxLength);
		});
	}

	public void TestStoVersionPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "StoVersion");

		header.StoVersion = "AA";

		CombineAssertions(() =>
		{
			AssertEquals("StoVersion", "AA", header.StoVersion);
			AssertNotNull("StoVersion is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestDestinationCustomsOffice()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.DestinationCustomsOfficeInfo);
			AssertEquals("Caption", "Destination Customs Office", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Destination Customs Office", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Dest. Customs Office", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Customs Office of Destination", captionResourceString.FullDescription);

			AssertEquals("Empty when not declared", ZString.Empty, header.DestinationCustomsOffice);

			var codeData = header.DestinationCustomsOfficeCode;
			codeData.CY_ParentID = header.PK;
			codeData.CY_Type = "EUO";
			codeData.CY_Code = "G5D";
			codeData.CY_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
			AssertEquals("Empty when CY_Data is not declared", ZString.Empty, header.DestinationCustomsOffice);

			codeData.CY_Data = "7758258";
			AssertEquals("Has correct data", "7758258", header.DestinationCustomsOffice);
		});
	}

	public void TestDestinationCustomsOfficeCode()
	{
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
		CombineAssertions(() =>
		{
			AssertEquals("PK", header.PK, header.DestinationCustomsOfficeCode.CY_ParentID);
			AssertEquals("CY_ParentTableCode", AsycudaManifestHeaderSchema.Constants.Prefix, header.DestinationCustomsOfficeCode.CY_ParentTableCode);
			AssertEquals("CY_Type", "EUO", header.DestinationCustomsOfficeCode.CY_Type);
			AssertEquals("CY_Code", "G5D", header.DestinationCustomsOfficeCode.CY_Code);
		});
	}

	public void TestAMA_Calc_HasHouseConsignmentDescription()
	{
		AssertResourceStringData(header.AMA_Calc_HasHouseConsignmentInfo, "Has House Consignment?", "Has House Consig.?", "Has HC?", "If ticked, declaration will include House Consignments and if not, only Master Consignment");
	}

	public void TestG5_IsSimplifiedDescription()
	{
		AssertResourceStringData(header.IsSimplifiedInfo, "Is Simplified?", "Simplified?", "Simp.?", "If ticked a G5 Simplified will be sent otherwise a G5 General will be sent");
	}

	public void TestG5_IsSimplified_GenAddOnColumnAndDefault()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("IsSimplified should be false by default", false, header.IsSimplified);

			header.IsSimplified = true;
			AssertEquals("IsSimplified should be true when set", true, header.IsSimplified);
		});
	}

	public void TestG5_IsSimplifiedPersistence()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "G5_IsSimplfied");

		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.IsSimplified = true;

		CombineAssertions(() =>
		{
			AssertEquals("IsSimplified", true, header.IsSimplified);
			AssertNotNull("G5_IsSimplified is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestG5_MovementOfContainersOnlyDescription()
	{
		AssertResourceStringData(header.MovementOfContainersOnlyInfo, "Movement of Containers Only?", "Mov. Of Containers?", "Mov. Cont.?", "If ticked, declaration will include only a Master Consignment with the containers to move");
	}

	public void TestG5_MovementOfContainersOnly_GenAddOnColumnAndDefault()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("MovementOfContainersOnly should be false by default", false, header.MovementOfContainersOnly);

			header.MovementOfContainersOnly = true;
			AssertEquals("MovementOfContainersOnly should be true when set", true, header.MovementOfContainersOnly);
		});
	}

	public void TestG5_MovementOfContainersOnlyPersistence()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "G5_MovementOfContainersOnly");

		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.MovementOfContainersOnly = true;

		CombineAssertions(() =>
		{
			AssertEquals("MovementOfContainersOnly", true, header.MovementOfContainersOnly);
			AssertNotNull("G5_MovementOfContainersOnly is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestG5_MovementOfContainersOnlyDefaultAndReadOnly()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("MovementOfContainersOnly should be false by default", false, header.MovementOfContainersOnly);

			header.MovementOfContainersOnly = true;
			AssertEquals("MovementOfContainersOnly should be true when set", true, header.MovementOfContainersOnly);

			header.AMA_Calc_HasHouseConsignment = true;
			AssertEquals("MovementOfContainersOnly should be readonly when Has House Consignment", true, header.MovementOfContainersOnlyInfo.ReadOnly);
			AssertEquals("MovementOfContainersOnly should be false when Has House Consignment", false, header.MovementOfContainersOnly);

			header.MovementOfContainersOnly = true;
			AssertEquals("MovementOfContainersOnly should be true when set", true, header.MovementOfContainersOnly);
		});
	}

	public void TestCheckBillTypeWhenMovementOfContainersOnly()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			var bill = header.MasterBill;
			AssertEquals("Master Bill type should be BOL by default", TemporaryStorageBill.ChildBolCode, bill.ABL_BolType);

			header.MovementOfContainersOnly = true;
			AssertEquals("Master Bill type should be MOC when Movement of Containers Only is true", "MOC", bill.ABL_BolType);

			header.MovementOfContainersOnly = false;
			AssertEquals("All Bill types (including Master) should be BOL when Movement of Containers Only is false", TemporaryStorageBill.ChildBolCode, bill.ABL_BolType);

			var bill2 = header.Bills.AddNew();
			AssertEquals("When adding new Bill, Bill type should be HWB when Movement of Containers Only is false", TemporaryStorageBillKindList.Codes.HWB, bill2.ABL_BolType);

			header.MovementOfContainersOnly = true;
			AssertEquals("Master Bill should be MOC when Movement of Containers Only is true", TemporaryStorageBill.ChildMocCode, bill.ABL_BolType);
			AssertEquals("Bill2 type should be MOC when Movement of Containers Only is true", TemporaryStorageBill.ChildMocCode, bill2.ABL_BolType);

			header.MovementOfContainersOnly = false;
			AssertEquals("All Bills type should be BOL when Movement of Containers Only is false", TemporaryStorageBill.ChildBolCode, bill.ABL_BolType);
			AssertEquals("All Bills type should be BOL when Movement of Containers Only is false", TemporaryStorageBill.ChildBolCode, bill2.ABL_BolType);
		});
	}

	public void TestUnionGoods_GenAddOnColumnAndDefault()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("UnionGoods should be false by default", false, header.UnionGoods);

			header.UnionGoods = true;
			AssertEquals("UnionGoods should be true when set", true, header.UnionGoods);
		});
	}

	public void TestUnionGoodsPersistance()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "UnionGoods");
		filterQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentID, header.PK);

		header.UnionGoods = true;

		CombineAssertions(() =>
		{
			AssertEquals("UnionGoods", true, header.UnionGoods);
			AssertNotNull("UnionGoods is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestSetEmptyValuesAndReadOnlyForUnionGoods_UnionGoods()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		header.DsdtMrnNumber = "Test";
		header.Guarantee.PW_BondNumber = "TestNumber";
		header.Guarantee.PW_Override = true;
		header.Guarantee.PW_BondAmount = 15.0m;
		var bill = header.Bills.AddNew();
		var item1 = bill.PackedItems.AddNew();
		item1.DutiesAndTaxes.AddNew();
		item1.DutiesAndTaxes.AddNew();
		var item2 = bill.PackedItems.AddNew();
		item2.DutiesAndTaxes.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("[PreReq] DSDT Mrn not readonly", false, header.DsdtMrnNumberInfo.ReadOnly);
			AssertEquals("[PreReq] DSDT Mrn not empty", false, header.DsdtMrnNumber.IsEmpty);
			AssertEquals("[PreReq] Guarantee Number not empty", false, header.Guarantee.PW_BondNumber.IsEmpty);
			AssertEquals("[PreReq] Guarantee Override true", true, header.Guarantee.PW_Override);
			AssertEquals("[PreReq] Guarantee Amount not empty", false, header.Guarantee.PW_BondAmount.IsEmpty);
			AssertEquals("[PreReq] Item1 Duties not empty", 2, item1.DutiesAndTaxes.Count);
			AssertEquals("[PreReq] Item2 Duties not empty", 1, item2.DutiesAndTaxes.Count);
		});

		CombineAssertions(() =>
		{
			header.UnionGoods = true;
			AssertEquals("DSDT Mrn readonly", true, header.DsdtMrnNumberInfo.ReadOnly);
			AssertEquals("DSDT Mrn empty", true, header.DsdtMrnNumber.IsEmpty);
			AssertEquals("Guarantee Number empty", true, header.Guarantee.PW_BondNumber.IsEmpty);
			AssertEquals("Guarantee Override false", false, header.Guarantee.PW_Override);
			AssertEquals("Guarantee Amount empty", true, header.Guarantee.PW_BondAmount.IsEmpty);
			AssertEquals("Item1 Duties empty", 0, item1.DutiesAndTaxes.Count);
			AssertEquals("Item2 Duties empty", 0, item2.DutiesAndTaxes.Count);
		});
	}

	public void TestDestinationGoodsLocationDescription()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.DestinationGoodsLocationDescriptionInfo);
			AssertEquals("Caption", "Destination Goods Location", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Dest. Goods Location", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Dest. Location", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Goods Location of Destination", captionResourceString.FullDescription);

			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;

			AssertEquals("Empty when not declared", ZString.Empty, header.DestinationGoodsLocationDescription);

			AssertNotNull(header.DestinationGoodsLocation);
			header.DestinationGoodsLocation.CGL_Qualifier = "Q";
			header.DestinationGoodsLocation.CGL_Type = "T";
			AssertEquals("Has correct data", "Q;T", header.DestinationGoodsLocationDescription);
		});
	}

	public void TestManualDestinationCustomsOffice()
	{
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.ManualDestinationCustomsOfficeInfo);
		AssertEquals("Caption", "Goods Location", captionResourceString.Caption);
		AssertEquals("MediumCaption", "Goods Location", captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", "Location", captionResourceString.ShortCaption);
		AssertEquals("Maxlength equals to CusGoodsLocationAddress.Schema.E2_GovRegNumMaxLength", CusGoodsLocationAddress.Schema.E2_GovRegNumMaxLength, header.ManualDestinationCustomsOfficeInfo.MaxLength);

		CombineAssertions(() =>
		{
			header.ManualDestinationCustomsOffice = "ANCDEFG";
			AssertEquals("CY_ParentTableCode", AsycudaManifestHeaderSchema.Constants.Prefix, header.DestinationCustomsOfficeCode.CY_ParentTableCode);
			AssertEquals("CGL_Qualifier", "Y", header.DestinationGoodsLocation.CGL_Qualifier);
			AssertEquals("CGL_Type", "B", header.DestinationGoodsLocation.CGL_Type);
			AssertEquals("CGL_LocationUse", "G5D", header.DestinationGoodsLocation.CGL_LocationUse);
			AssertEquals("E2_GovRegNum", "ANCDEFG", header.DestinationGoodsLocation.Address.E2_GovRegNum);
		});
	}

	public void TestWhenMessageTypeTSMEntryNumberMRN()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		header.MRN = "AAAAA";
		header.AcceptanceDate = ZDate.BrettsBirthday;

		CombineAssertions(() =>
		{
			var newEntryNumber = CusEntryNumber.Load(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			AssertEquals("MRN number saved when is TSM", "AAAAA", newEntryNumber?.CE_EntryNum);
			AssertEquals("AcceptanceDate saved when is TSM", ZDate.BrettsBirthday, newEntryNumber?.CE_IssueDate);
		});
	}

	public void TestWhenMessageTypeTSMEntryNumberSUM()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		header.DsdtMrnNumber = "12345";

		CombineAssertions(() =>
		{
			var newEntryNumber = CusEntryNumber.Load(header, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
			AssertEquals("SUM number saved when is TSM", "12345", newEntryNumber?.CE_EntryNum);
		});
	}

	public void TestWhenMessageTypeLAMEntryNumberASY()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		header.EntryNumber = "12345";

		CombineAssertions(() =>
		{
			var newEntryNumber = CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Spain);
			AssertEquals("ASY number saved when is LAM", "12345", newEntryNumber?.CE_EntryNum);
		});
	}

	public void TestWhenMessageTypeLAMEntryDateASY()
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
		header.EntryDate = ZDate.BrettsBirthday;

		CombineAssertions(() =>
		{
			var newEntryDate = CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Spain);
			AssertEquals("ASY number saved when is LAM", ZDate.BrettsBirthday, newEntryDate?.CE_IssueDate);
		});
	}

	public void TestIsMessageTypeLAM()
	{
		header.AMA_MessageType = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_MessageType is empty the value expected false", false, header.IsMessageTypeLAM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("When AMA_MessageType is LAM the value expected true", true, header.IsMessageTypeLAM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("When AMA_MessageType is TSM the value expected false", false, header.IsMessageTypeLAM);
		});
	}

	public void TestIsMessageTypeTSM()
	{
		header.AMA_MessageType = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_MessageType is empty the value expected false", false, header.IsMessageTypeTSM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("When AMA_MessageType is TSM the value expected true", true, header.IsMessageTypeTSM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("When AMA_MessageType is LAM the value expected false", false, header.IsMessageTypeTSM);
		});
	}

	public void TestIsMessageTypeG5EOrG5R()
	{
		header.AMA_MessageType = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_MessageType is empty the value expected false", false, header.IsMessageType_G5E_G5R);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("When AMA_MessageType is TSM the value expected false", false, header.IsMessageType_G5E_G5R);
			header.AMA_MessageType = "G5E";
			AssertEquals("When AMA_MessageType is G5E the value expected true", true, header.IsMessageType_G5E_G5R);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertEquals("When AMA_MessageType is G5X the value expected false", false, header.IsMessageType_G5E_G5R);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("When AMA_MessageType is LAM the value expected false", false, header.IsMessageType_G5E_G5R);
			header.AMA_MessageType = "G5R";
			AssertEquals("When AMA_MessageType is G5R the value expected true", true, header.IsMessageType_G5E_G5R);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertEquals("When AMA_MessageType is G5P the value expected false", false, header.IsMessageType_G5E_G5R);
			header.AMA_MessageType = "AAA";
			AssertEquals("When AMA_MessageType is AAA the value expected false", false, header.IsMessageType_G5E_G5R);
		});
	}

	public void TestIsMessageTypeTSMAndIsUnionGoods()
	{
		header.AMA_MessageType = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_MessageType is empty the value expected false", false, header.IsMessageTypeTSMAndIsUnionGoods);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("When AMA_MessageType is TSM but UnionGoods is false expected false", false, header.IsMessageTypeTSMAndIsUnionGoods);
			header.UnionGoods = true;
			AssertEquals("When AMA_MessageType is TSM and UnionGoods is true expected true", true, header.IsMessageTypeTSMAndIsUnionGoods);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("When AMA_MessageType is not LAM the value expected false", false, header.IsMessageTypeTSMAndIsUnionGoods);
		});
	}

	public void TestIsMessageTypeG5_LAM_TSM()
	{
		header.AMA_MessageType = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_MessageType is empty the value expected false", expected: false, header.IsMessageTypeG5_LAM_TSM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("When AMA_MessageType is TSM the value expected true", expected: true, header.IsMessageTypeG5_LAM_TSM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("When AMA_MessageType is LAM the value expected true", expected: true, header.IsMessageTypeG5_LAM_TSM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertEquals("When AMA_MessageType is G5P the value expected true", expected: true, header.IsMessageTypeG5_LAM_TSM);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertEquals("When AMA_MessageType is G5X the value expected true", expected: true, header.IsMessageTypeG5_LAM_TSM);
			header.AMA_MessageType = "AAA";
			AssertEquals("When AMA_MessageType is AAA the value expected false", expected: false, header.IsMessageTypeG5_LAM_TSM);
		});
	}

	public void TestIsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty()
	{
		header.AMA_MessageType = ZString.Empty;
		header.CustomsStatus = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_MessageType is empty and customs status is empty the value expected true", true, header.IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("When AMA_MessageType is TSM and customs status is empty the value expected false", false, header.IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty);
			header.CustomsStatus = "AA";
			AssertEquals("When AMA_MessageType is TSM and customs status is not empty the value expected true", true, header.IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("When AMA_MessageType is LAM and customs status is not empty the value expected true", true, header.IsNotReadOnlyMemberWhenMessageTypeTSMAndCustomsStatusEmpty);
		});
	}

	public void TestIsMessageTypeManual()
	{
		header.AMA_MessageType = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("When AMA_MessageType is empty the value expected  is", false, header.IsMessageTypeManual);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			AssertEquals("When AMA_MessageType is TSM the value expected  is", true, header.IsMessageTypeManual);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertEquals("When AMA_MessageType is G5X the value expected  is", false, header.IsMessageTypeManual);
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			AssertEquals("When AMA_MessageType is LAM the value expected  is", true, header.IsMessageTypeManual);
		});
	}

	public void TestAMA_CustomsProfileCaptions()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.AMA_CustomsProfileInfo);
			AssertEquals("Caption", "Certificate", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Certif.", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Cert.", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job", captionResourceString.FullDescription);
		});
	}

	public void TestAMA_CustomsProfileDefaulted()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert2";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var staff2 = Factory.New<GlbStaff>();
		staff2.GS_Code = "AA";
		staff2.GS_LoginName = "aatest";
		var wrapper2 = GlbStaffWrapper.Get(staff2);
		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert3";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert4";
		cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

		var staff3 = Factory.New<GlbStaff>();
		staff3.GS_Code = "AZ";
		staff3.GS_LoginName = "aztest";

		Factory.Save();

		CombineAssertions(() =>
		{
			header.AMA_GS_NKCustomsAgent = staff.GS_Code;
			AssertEquals("AMA_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, header.AMA_CustomsProfile);

			header.AMA_GS_NKCustomsAgent = staff2.GS_Code;
			AssertEquals("AMA_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", header.AMA_CustomsProfile);

			header.AMA_GS_NKCustomsAgent = staff3.GS_Code;
			AssertEquals("AMA_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, header.AMA_CustomsProfile);

			header.AMA_GS_NKCustomsAgent = staff.GS_Code;
			header.AMA_CustomsProfile = "TestCert2";
			AssertEquals("AMA_CustomsProfile has value when broker not empty", "TestCert2", header.AMA_CustomsProfile);

			header.AMA_GS_NKCustomsAgent = ZString.Empty;
			AssertEquals("AMA_CustomsProfile has been cleared when broker is empty", ZString.Empty, header.AMA_CustomsProfile);

			header.AMA_CustomsProfile = "TESTCERT1";
			header.AMA_GS_NKCustomsAgent = staff.GS_Code;
			AssertEquals("AMA_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", header.AMA_CustomsProfile);

			header.AMA_GS_NKCustomsAgent = ZString.Empty;
			header.AMA_CustomsProfile = "TestCert2";
			header.AMA_GS_NKCustomsAgent = staff.GS_Code;
			AssertEquals("AMA_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, header.AMA_CustomsProfile);
		});
	}

	public void TestAMA_CustomsProfileEmptyWhenCopy()
	{
		header.AMA_CustomsProfile = "TestCert2";

		var copiedDeclaration = (TemporaryStorageHeader)header.TemplateCopy();
		AssertEquals("AMA_CustomsProfile is empty when copied", ZString.Empty, copiedDeclaration.AMA_CustomsProfile);
	}

	public void TestAMA_CustomsOfficeCaptions()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.AMA_CustomsOfficeInfo);
			AssertEquals("Caption", "Departure Customs Office", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Departure Customs Office", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Dept. Customs Office", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Customs Office of Departure", captionResourceString.FullDescription);
		});
	}

	public void TestGoodsLocationDescriptionCaptions()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(header.GoodsLocationDescriptionInfo);
			AssertEquals("Caption", "Departure Goods Location", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Departure Goods Location", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Dept. Goods Location", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Departure Location of Goods", captionResourceString.FullDescription);
		});
	}

	public void TestICusGoodsLocationProvider_ProviderKey()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		AssertEquals("ES Provider Key is ES Declaration one", "ESDECL", (header as ICusGoodsLocationProvider).ProviderKey);
	}

	public void TestGetGoodsLocation()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var goodsLocation = header.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertType<CusGoodsLocation>(goodsLocation);
			AssertEquals("CGL_ParentID", header.PK, goodsLocation.CGL_ParentID);
			AssertSame("Cached", goodsLocation, header.GoodsLocation);
		});
	}

	public void TestMRN_ReadOnly()
	{
		CombineAssertions(() =>
		{
			header.CustomsStatus = ZString.Empty;
			header.AMA_MessageType = ZString.Empty;
			AssertEquals("Customs Status is empty and MessageType empty", expected: true, header.MRNInfo.ReadOnly);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertEquals("Customs Status is empty and MessageType != G5P", expected: true, header.MRNInfo.ReadOnly);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertEquals("Customs Status is empty and MessageType == G5P", expected: false, header.MRNInfo.ReadOnly);

			header.CustomsStatus = "AH";
			AssertEquals("Customs Status is not empty and MessageType == G5P", expected: true, header.MRNInfo.ReadOnly);
		});
	}

	public void TestAMA_MessageType_ReadOnly()
	{
		CombineAssertions(() =>
		{
			header.MRN = "AH3MRN";
			header.AMA_MessageType = ZString.Empty;
			AssertEquals("MRN is not empty and MessageType empty", expected: false, header.AMA_MessageTypeInfo.ReadOnly);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertEquals("MRN is not empty and MessageType != G5P", expected: false, header.AMA_MessageTypeInfo.ReadOnly);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertEquals("MRN is not empty and MessageType == G5P", expected: true, header.AMA_MessageTypeInfo.ReadOnly);

			header.MRN = ZString.Empty;
			AssertEquals("MRN is empty and MessageType == G5P", expected: false, header.AMA_MessageTypeInfo.ReadOnly);
		});
	}

	public void TestIsMessageTypeG5V1Reception()
	{
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertEquals("MessageType == G5P", expected: true, header.IsMessageTypeG5V1Reception);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertEquals("MessageType == G5X", expected: false, header.IsMessageTypeG5V1Reception);
		});
	}

	public void TestIsMessageTypeG5V1Expedition()
	{
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			AssertEquals("MessageType == G5X", expected: true, header.IsMessageTypeG5V1Expedition);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			AssertEquals("MessageType == G5P", expected: false, header.IsMessageTypeG5V1Expedition);
		});
	}

	public void TestGetUrlToLaunch()
	{
		CombineAssertions(() =>
		{
			var expectedMRN = "AH3RRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADDS-JDIT/CtrlG5Sede?op=detCab&mrn=" + expectedMRN;

			AssertEquals("Empty because Customs there is no MRN", ZString.Empty, header.GetUrlToLaunch());

			header.MRN = expectedMRN;
			AssertEquals("Correct url because there is a MRN", expectedUrl, header.GetUrlToLaunch());

			header.Delete();
			AssertEquals("Empty because Deleted", ZString.Empty, header.GetUrlToLaunch());
		});
	}

	public void TestIESMessageInfoProvider_Broker()
	{
		CombineAssertions(() =>
		{
			var esMessageInfoProvider = header as IESMessageInfoProvider;
			header.AMA_GS_NKCustomsAgent = ZString.Empty;
			AssertNull("Broker is null", esMessageInfoProvider.Broker);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZM";
			header.AMA_GS_NKCustomsAgent = staff.GS_Code;
			AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
		});
	}

	public void TestIESMessageInfoProvider_EntryReference()
	{
		var esMessageInfoProvider = header as IESMessageInfoProvider;
		header.AMA_JobReference = "Reference";
		AssertEquals("EntryReference has the correct value", "Reference", esMessageInfoProvider.EntryReference);
	}

	public void TestIESMessageInfoProvider_MRN()
	{
		var esMessageInfoProvider = header as IESMessageInfoProvider;
		CombineAssertions(() =>
		{
			AssertEquals("MRN has the correct value (empty when nctsHeader has no mrn)", ZString.Empty, esMessageInfoProvider.MRN);

			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999830001277";
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals("MRN has the correct value", "20ES00999830001277", esMessageInfoProvider.MRN);
		});
	}

	public void TestGuaranteeBondType()
	{
		AssertEquals("GuaranteeBondType should be equal to TST for Spain.", EUGuaranteeTypeList.Codes.TST, header.GuaranteeBondType);
	}

	public void TestIESResponseBusinessObject_BranchPK()
	{
		var esResponseBusinessObject = header as IESResponseBusinessObject;
		AssertEquals("BranchPK has the correct value", header.Branch.PK, esResponseBusinessObject.BranchPK);
	}

	public void TestIESResponseBusinessObject_MessageCollection()
	{
		var esResponseBusinessObject = header as IESResponseBusinessObject;
		header.Messages.AddNew();
		AssertEquals("MessageCollection has the correct value", header.Messages, esResponseBusinessObject.MessageCollection);
	}

	public void TestIESResponseBOMessageStatus_MessageStatus()
	{
		var esResponseBusinessObject = header as IESResponseBOMessageStatus;
		esResponseBusinessObject.MessageStatus = "AAA";
		AssertEquals("MessageStatus has set the correct value", "AAA", header.AMA_MessageStatus);
	}

	#region CreateTemporaryStorageData
	public void TestCreateTemporaryStorageData_G5P_WithAllData_LiabilityAmountFromGuarantee()
	{
		var (temporaryStorageHeader, premisesPK, _, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage(G5MessageTypeCodeList.Codes.G5v1Reception);

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99984000037");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader is the same as the one returned by the method", returnedRegHeader, regHeader);
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99984000037", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.G5Reception, regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premisesPK, regHeader.SRH_SRP_Premises);
			AssertEquals("regHeader.SRH_InternalReference", ZString.Empty, regHeader.SRH_InternalReference);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 20.0m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeaderPK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;
			var expectedTransactionDate = new ZDateTimeOffset(2025, 01, 01, 02, 01, 00);

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (SRL_PackageType, SRL_PackageMarks, SRL_GoodsOwnerIdentifier, SRL_GrossWeightUQ, SRL_CustomsStatus, SRL_UnionStatus, SRL_LimitDate)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks2", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks5", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks7", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks8", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("VG", "marks10", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks1", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 2.5m, 5, 0.67m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks2", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 3.5m, 7, 0.93m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks4", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 2.2m, 2, 0.59m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithOnePivot(regLines, "marks5", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 8.8m, 8, 2.34m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithTwoPivots(regLines, "marks7", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 3.0m, 33, 0.80m, expectedTransactionDate,
										1.0m, 4, "444444", "CUSCODE4", "Description4",
										2.0m, 5, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks8", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 16.77916m, 5, 4.45m, expectedTransactionDate, 7, "777777", "CUSCODE7", "Description7");

			AssertMultipleRegLinesWithTwoPivotsEach(regLines, "marks9", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements,
													24.78714m, 7, 6.60m, expectedTransactionDate,
													13.22751m, 20, 3.50m, expectedTransactionDate,
													23.49084m, 7, "777777", "CUSCODE7", "Description7",
													1.29630m, 6, "666666", "CUSCODE6", "Description6",
													3.7037m, 6, "666666", "CUSCODE6", "Description6",
													9.52381m, 8, "888888", "CUSCODE8", "Description8");

			AssertRegLineWithOnePivot(regLines, "marks10", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 0.47619m, ZInt.Zero, 0.12m, expectedTransactionDate, 8, "888888", "CUSCODE8", "Description8");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 9, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 12, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 7, regLineItems.Length);

			AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
		});
	}

	public void TestCreateTemporaryStorageData_CalculateSRT_BondAmountWithPW_OverrideFalse()
	{
		var (temporaryStorageHeader, premisesPK, _, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage(G5MessageTypeCodeList.Codes.G5v1Reception, pwOverride: false);

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99984000037");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader is the same as the one returned by the method", returnedRegHeader, regHeader);
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99984000037", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.G5Reception, regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premisesPK, regHeader.SRH_SRP_Premises);
			AssertEquals("regHeader.SRH_InternalReference", ZString.Empty, regHeader.SRH_InternalReference);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 554m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeaderPK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;
			var expectedTransactionDate = new ZDateTimeOffset(2025, 01, 01, 02, 01, 00);

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (SRL_PackageType, SRL_PackageMarks, SRL_GoodsOwnerIdentifier, SRL_GrossWeightUQ, SRL_CustomsStatus, SRL_UnionStatus, SRL_LimitDate)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks2", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks5", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks7", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks8", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("VG", "marks10", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks1", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 2.5m, 5, 41.67m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks2", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 3.5m, 7, 58.33m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks4", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 2.2m, 2, 44m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithOnePivot(regLines, "marks5", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 8.8m, 8, 176m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithTwoPivots(regLines, "marks7", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 3.0m, 33, 35m, expectedTransactionDate,
										1.0m, 4, "444444", "CUSCODE4", "Description4",
										2.0m, 5, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks8", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 16.77916m, 5, 25.83m, expectedTransactionDate, 7, "777777", "CUSCODE7", "Description7");

			AssertMultipleRegLinesWithTwoPivotsEach(regLines, "marks9", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements,
													24.78714m, 7, 49.66m, expectedTransactionDate,
													13.22751m, 20, 89.94m, expectedTransactionDate,
													23.49084m, 7, "777777", "CUSCODE7", "Description7",
													1.29630m, 6, "666666", "CUSCODE6", "Description6",
													3.7037m, 6, "666666", "CUSCODE6", "Description6",
													9.52381m, 8, "888888", "CUSCODE8", "Description8");

			AssertRegLineWithOnePivot(regLines, "marks10", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 0.47619m, ZInt.Zero, 2.57m, expectedTransactionDate, 8, "888888", "CUSCODE8", "Description8");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 9, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 12, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 7, regLineItems.Length);

			AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
		});
	}

	public void TestCreateTemporaryStorageData_G5P_WithAllData_LiabilityAmountFromGuarantee_WithLiabilityNotDivided()
	{
		Factory.SetBulkTypeHelper();

		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var bill = temporaryStorageHeader.Bills.AddNew();
		temporaryStorageHeader.Bills.FirstOrDefault().ABL_OA_Consignee = orgAddress.PK;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999880000373";
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.ClearanceDate = new ZDateTime(2025, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "1234567890";

		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = temporaryStorageHeader.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 2000.0m;
		guarantee.PW_Override = true;
		guarantee.PW_RX_NKCurrency = "USD";

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "TSLoc";

		var item = bill.PackedItems.AddNew();
		item.SetValues(1, "111111", "CUSCODE", "Description", 6);

		var pack = bill.Packs.AddNew();
		pack.SetValues("BX", "marks", 5);
		item.PackagesPivot.AddPivotFor(pack);

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99984000037");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader is the same as the one returned by the method", returnedRegHeader, regHeader);
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "99984000037", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.G5Reception, regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);
			AssertEquals("regHeader.SRH_InternalReference", ZString.Empty, regHeader.SRH_InternalReference);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 2000.0m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, 6.00000m, 5, 2000.0m, new ZDateTimeOffset(2025, 01, 01, 02, 01, 00), 1, "111111", "CUSCODE", "Description");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 1, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 1, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 1, regLineItems.Length);

			AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
		});
	}

	public void TestCreateTemporaryStorageData_TSM_WithAllData_LiabilityAmountFromGuarantee()
	{
		var (temporaryStorageHeader, premisesPK, _, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry);

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "24ES00999880000373");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader is the same as the one returned by the method", returnedRegHeader, regHeader);
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "24ES00999880000373", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.ManualEntries, regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premisesPK, regHeader.SRH_SRP_Premises);
			AssertEquals("regHeader.SRH_InternalReference", ZString.Empty, regHeader.SRH_InternalReference);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 20.0m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeaderPK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;
			var expectedTransactionDate = new ZDateTimeOffset(2023, 01, 01, 02, 01, 00);

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (SRL_PackageType, SRL_PackageMarks, SRL_GoodsOwnerIdentifier, SRL_GrossWeightUQ, SRL_CustomsStatus, SRL_UnionStatus, SRL_LimitDate)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks2", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks5", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks7", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks8", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("VG", "marks10", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks1", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 2.5m, 5, 0.67m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks2", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 3.5m, 7, 0.93m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks4", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 2.2m, 2, 0.59m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithOnePivot(regLines, "marks5", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 8.8m, 8, 2.34m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithTwoPivots(regLines, "marks7", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 3.0m, 33, 0.80m, expectedTransactionDate,
										1.0m, 4, "444444", "CUSCODE4", "Description4",
										2.0m, 5, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks8", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 16.77916m, 5, 4.45m, expectedTransactionDate, 7, "777777", "CUSCODE7", "Description7");

			AssertMultipleRegLinesWithTwoPivotsEach(regLines, "marks9", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry,
													24.78714m, 7, 6.60m, expectedTransactionDate,
													13.22751m, 20, 3.50m, expectedTransactionDate,
													23.49084m, 7, "777777", "CUSCODE7", "Description7",
													1.29630m, 6, "666666", "CUSCODE6", "Description6",
													3.7037m, 6, "666666", "CUSCODE6", "Description6",
													9.52381m, 8, "888888", "CUSCODE8", "Description8");

			AssertRegLineWithOnePivot(regLines, "marks10", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 0.47619m, ZInt.Zero, 0.12m, expectedTransactionDate, 8, "888888", "CUSCODE8", "Description8");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 9, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 12, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 7, regLineItems.Length);

			AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
		});
	}

	public void TestCreateTemporaryStorageData_TSM_WithAllData_LiabilityAmountFromGuarantee_WithLiabilityNotDivided()
	{
		Factory.SetBulkTypeHelper();

		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var bill = temporaryStorageHeader.Bills.AddNew();
		temporaryStorageHeader.Bills.FirstOrDefault().ABL_OA_Consignee = orgAddress.PK;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999880000373";
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "1234567890";

		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = temporaryStorageHeader.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 2000.0m;
		guarantee.PW_Override = true;
		guarantee.PW_RX_NKCurrency = "USD";

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "TSLoc";

		var item = bill.PackedItems.AddNew();
		item.SetValues(1, "111111", "CUSCODE", "Description", 6);

		var pack = bill.Packs.AddNew();
		pack.SetValues("BX", "marks", 5);
		item.PackagesPivot.AddPivotFor(pack);

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "24ES00999880000373");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader is the same as the one returned by the method", returnedRegHeader, regHeader);
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "24ES00999880000373", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.ManualEntries, regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);
			AssertEquals("regHeader.SRH_InternalReference", ZString.Empty, regHeader.SRH_InternalReference);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", 2000.0m, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks", "GB555555555", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 6.00000m, 5, 2000.0m, new ZDateTimeOffset(2023, 01, 01, 02, 01, 00), 1, "111111", "CUSCODE", "Description");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 1, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 1, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 1, regLineItems.Length);

			AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
		});
	}

	public void TestCreateTemporaryStorageData_TSM_WithAllData_UnionGoods()
	{
		var (temporaryStorageHeader, premisesPK, _, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage(G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry);
		temporaryStorageHeader.UnionGoods = true;

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "1234567890");
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader is the same as the one returned by the method", returnedRegHeader, regHeader);
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "1234567890", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.ManualEntries, regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", ZString.Empty, regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premisesPK, regHeader.SRH_SRP_Premises);
			AssertEquals("regHeader.SRH_InternalReference", "1234567890", regHeader.SRH_InternalReference);

			var regLines = regHeader.CusTempStorageRegLines;
			var expectedTransactionDate = new ZDateTimeOffset(2023, 01, 01, 02, 01, 00);

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (SRL_PackageType, SRL_PackageMarks, SRL_GoodsOwnerIdentifier, SRL_GrossWeightUQ, SRL_CustomsStatus, SRL_UnionStatus, SRL_LimitDate)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("BX", "marks2", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("CT", "marks4", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("BX", "marks5", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("BX", "marks7", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("BX", "marks8", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("BX", "marks9", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
															("VG", "marks10", "GB555555555", "KGM", "OPN", "COM", new ZDate(2023, 04, 01)),
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks1", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 2.5m, 5, ZDecimal.Zero, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks2", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 3.5m, 7, ZDecimal.Zero, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks4", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 2.2m, 2, ZDecimal.Zero, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithOnePivot(regLines, "marks5", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 8.8m, 8, ZDecimal.Zero, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithTwoPivots(regLines, "marks7", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 3.0m, 33, ZDecimal.Zero, expectedTransactionDate,
										1.0m, 4, "444444", "CUSCODE4", "Description4",
										2.0m, 5, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks8", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 16.77916m, 5, ZDecimal.Zero, expectedTransactionDate, 7, "777777", "CUSCODE7", "Description7");

			AssertMultipleRegLinesWithTwoPivotsEach(regLines, "marks9", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry,
													24.78714m, 7, ZDecimal.Zero, expectedTransactionDate,
													13.22751m, 20, ZDecimal.Zero, expectedTransactionDate,
													23.49084m, 7, "777777", "CUSCODE7", "Description7",
													1.29630m, 6, "666666", "CUSCODE6", "Description6",
													3.7037m, 6, "666666", "CUSCODE6", "Description6",
													9.52381m, 8, "888888", "CUSCODE8", "Description8");

			AssertRegLineWithOnePivot(regLines, "marks10", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ManualEntry, 0.47619m, ZInt.Zero, ZDecimal.Zero, expectedTransactionDate, 8, "888888", "CUSCODE8", "Description8");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 9, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 12, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 7, regLineItems.Length);

			AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
		});
	}

	public void TestCreateTemporaryStorageData_LAM_WithAllData()
	{
		var (temporaryStorageHeader, _, premisesPK, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage(G5MessageTypeCodeList.Codes.LameManualEntry);

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_AppCode, CusTempStorageRegHeader.ESAppCode);
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader is the same as the one returned by the method", returnedRegHeader, regHeader);
			AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
			AssertEquals("regHeader.SRH_Reference", "LAMREFPLACEHOLDER", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2024, 02, 02), regHeader.SRH_ArrivalDate);
			AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2024, 02, 02, 03, 03, 00), regHeader.SRH_PresentationDate);
			AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceTypeCodeList.Codes.LameEntries, regHeader.SRH_PreviousReferenceType);
			AssertEquals("regHeader.SRH_PreviousReference", ZString.Empty, regHeader.SRH_PreviousReference);
			AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
			AssertEquals("regHeader.SRH_SRP_Premises", premisesPK, regHeader.SRH_SRP_Premises);
			AssertEquals("regHeader.SRH_InternalReference", ZString.Empty, regHeader.SRH_InternalReference);

			var regHeaderGuarantee = regHeader.Guarantee;
			AssertEquals("regHeaderGuarantee.PW_BondNumber", ZString.Empty, regHeaderGuarantee.PW_BondNumber);
			AssertEquals("regHeaderGuarantee.PW_BondAmount", ZDecimal.Zero, regHeaderGuarantee.PW_BondAmount);
			AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", ZString.Empty, regHeaderGuarantee.PW_RX_NKCurrency);
			AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", ZGuid.Empty, regHeaderGuarantee.PW_CPH_Guarantee);

			var regLines = regHeader.CusTempStorageRegLines;
			var expectedTransactionDate = new ZDateTimeOffset(2024, 02, 02, 03, 03, 00);

			AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
													new ZInt[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, regLines.Select(x => x.SRL_LineNumber));
			AssertContainsExactElementsInAnyOrder("regLines (SRL_PackageType, SRL_PackageMarks, SRL_GoodsOwnerIdentifier, SRL_GrossWeightUQ, SRL_CustomsStatus, SRL_UnionStatus, SRL_LimitDate)",
													new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
													{
															("BX", "marks1", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("BX", "marks2", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("CT", "marks4", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("BX", "marks5", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("BX", "marks7", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("BX", "marks8", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("BX", "marks9", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("BX", "marks9", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
															("VG", "marks10", "ES555555555", "KGM", "OPN", "NAT", ZDate.Empty),
													}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

			AssertRegLineWithOnePivot(regLines, "marks1", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 2.5m, 5, 0m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks2", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 3.5m, 7, 0m, expectedTransactionDate, 1, "111111", "CUSCODE1", "Description1");

			AssertRegLineWithOnePivot(regLines, "marks4", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 2.2m, 2, 0m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithOnePivot(regLines, "marks5", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 8.8m, 8, 0m, expectedTransactionDate, 2, "222222", "CUSCODE2", "Description2");

			AssertRegLineWithTwoPivots(regLines, "marks7", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 3.0m, 33, 0m, expectedTransactionDate,
										1.0m, 4, "444444", "CUSCODE4", "Description4",
										2.0m, 5, "555555", "CUSCODE5", "Description5");

			AssertRegLineWithOnePivot(regLines, "marks8", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 16.77916m, 5, 0m, expectedTransactionDate, 7, "777777", "CUSCODE7", "Description7");

			AssertMultipleRegLinesWithTwoPivotsEach(regLines, "marks9", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry,
													24.78714m, 7, 0m, expectedTransactionDate,
													13.22751m, 20, 0m, expectedTransactionDate,
													23.49084m, 7, "777777", "CUSCODE7", "Description7",
													1.29630m, 6, "666666", "CUSCODE6", "Description6",
													3.7037m, 6, "666666", "CUSCODE6", "Description6",
													9.52381m, 8, "888888", "CUSCODE8", "Description8");

			AssertRegLineWithOnePivot(regLines, "marks10", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry, 0.47619m, ZInt.Zero, 0m, expectedTransactionDate, 8, "888888", "CUSCODE8", "Description8");

			var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
			AssertEquals("regLineTransactions created", 9, regLineTransactions.Length);

			var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
			AssertEquals("regLineItemPivots created", 12, regLineItemPivots.Length);

			var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
			AssertEquals("regLineItems created", 7, regLineItems.Length);

			AssertEquals("Temporary Storage Customs Status changed", "TSA", temporaryStorageHeader.CustomsStatus);
		});
	}

	public void TestCreateTemporaryStorageData_LAM_EntryNumberNotEmpty()
	{
		var (temporaryStorageHeader, _, premisesPK, cusPermitHeaderPK) = SetUpDataToCreateTemporaryStorage(G5MessageTypeCodeList.Codes.LameManualEntry);
		temporaryStorageHeader.EntryNumber = "LAM1";

		CombineAssertions(() =>
		{
			var (response, returnedRegHeader) = temporaryStorageHeader.CreateTemporaryStorageData();
			AssertEquals("CreateTemporaryStorageData response is true", true, response);

			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_AppCode, CusTempStorageRegHeader.ESAppCode);
			var regHeaders = Factory.Load<CusTempStorageRegHeader>(query);
			AssertEquals("regHeaders created", 1, regHeaders.Length);

			var regHeader = regHeaders[0];
			AssertEquals("regHeader.SRH_Reference", "LAM1", regHeader.SRH_Reference);
			AssertEquals("regHeader.SRH_InternalReference", "LAM1", regHeader.SRH_InternalReference);
		});
	}

	void AssertRegLineWithOnePivot(CusTempStorageRegLineCollection regLines, ZString marks, ZString internalReferenteType, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount, ZDateTimeOffset transactionDate, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		var regLine = regLines.First(x => x.SRL_PackageMarks == marks);
		var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Transactions created", 1, regLineTransactions.Length);
		AssertRegLineTransaction("regLine with marks " + marks + " Transactions[0]", regLineTransactions[0], internalReferenteType, grossWeight, packageQty, bondAmount, transactionDate);
		var regLinePivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Pivots created", 1, regLinePivots.Length);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot1", regLinePivots[0], grossWeight, itemNumber, tariff, cusCode, description);
	}

	void AssertMultipleRegLinesWithTwoPivotsEach(CusTempStorageRegLineCollection regLines, ZString marks, ZString internalReferenteType,
									ZDecimal transaction1GrossWeight, ZInt transaction1packageQty, ZDecimal transaction1BondAmount, ZDateTimeOffset transaction1Date,
									ZDecimal transaction2GrossWeight, ZInt transaction2packageQty, ZDecimal transaction2BondAmount, ZDateTimeOffset transaction2Date,
									ZDecimal pivot1GrossWeight, ZInt itemNumberPivot1, ZString tariffPivot1, ZString cusCodePivot1, ZString descriptionPivot1,
									ZDecimal pivot2GrossWeight, ZInt itemNumberPivot2, ZString tariffPivot2, ZString cusCodePivot2, ZString descriptionPivot2,
									ZDecimal pivot3GrossWeight, ZInt itemNumberPivot3, ZString tariffPivot3, ZString cusCodePivot3, ZString descriptionPivot3,
									ZDecimal pivot4GrossWeight, ZInt itemNumberPivot4, ZString tariffPivot4, ZString cusCodePivot4, ZString descriptionPivot4)
	{
		var regLinesForMarks = regLines.Where(x => x.SRL_PackageMarks == marks).ToArray();
		AssertEquals("regLines with marks " + marks, 2, regLinesForMarks.Length);

		var transactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery()).Where(t => regLinesForMarks.Select(x => x.PK).Contains(t.SRT_SRL));

		AssertContainsExactElementsInAnyOrder("regLinesPivots (SRT_GrossWeight, SRT_PackageQty, SRT_TransactionType, SRT_InternalReferenceNumber, SRT_InternalReferenceType, SRT_TransactionDate, SRT_PhysicalInOutDate, SRT_BondAmount)",
													new (ZDecimal, ZInt, ZString, ZString, ZString, ZDateTimeOffset, ZDateTimeOffset, ZDecimal)[]
													{
															(transaction1GrossWeight, transaction1packageQty, "OBL", "TS00000001", internalReferenteType, transaction1Date, transaction1Date, transaction1BondAmount),
															(transaction2GrossWeight, transaction2packageQty, "OBL", "TS00000001", internalReferenteType, transaction2Date, transaction2Date, transaction2BondAmount),
													}, transactions.Select(x => (x.SRT_GrossWeight, x.SRT_PackageQty, x.SRT_TransactionType, x.SRT_InternalReferenceNumber, x.SRT_InternalReferenceType, x.SRT_TransactionDate, x.SRT_PhysicalInOutDate, x.SRT_BondAmount)));

		var pivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery()).Where(p => regLinesForMarks.Select(x => x.PK).Contains(p.SRV_SRL_Line));

		AssertContainsExactElementsInAnyOrder("regLinesPivots (SRV_GrossWeight, RegLineItem.SRI_GoodsItemNumber, RegLineItem.SRI_Tariff, RegLineItem.SRI_CusC4Number, RegLineItem.SRI_GoodsDescription)",
													new (ZDecimal, ZInt, ZString, ZString, ZString)[]
													{
															(pivot1GrossWeight, itemNumberPivot1, tariffPivot1, cusCodePivot1, descriptionPivot1),
															(pivot2GrossWeight, itemNumberPivot2, tariffPivot2, cusCodePivot2, descriptionPivot2),
															(pivot3GrossWeight, itemNumberPivot3, tariffPivot3, cusCodePivot3, descriptionPivot3),
															(pivot4GrossWeight, itemNumberPivot4, tariffPivot4, cusCodePivot4, descriptionPivot4),
													}, pivots.Select(x => (x.SRV_GrossWeight, x.RegLineItem.SRI_GoodsItemNumber, x.RegLineItem.SRI_Tariff, x.RegLineItem.SRI_CusC4Number, x.RegLineItem.SRI_GoodsDescription)));
	}

	void AssertRegLineWithTwoPivots(CusTempStorageRegLineCollection regLines, ZString marks, ZString internalReferenteType, ZDecimal transactionGrossWeight, ZInt packageQty, ZDecimal transactionBondAmount, ZDateTimeOffset transactionDate,
									ZDecimal grossWeightPivot1, ZInt itemNumberPivot1, ZString tariffPivot1, ZString cusCodePivot1, ZString descriptionPivot1,
									ZDecimal grossWeightPivot2, ZInt itemNumberPivot2, ZString tariffPivot2, ZString cusCodePivot2, ZString descriptionPivot2)
	{
		var regLine = regLines.First(x => x.SRL_PackageMarks == marks);
		var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Transactions created", 1, regLineTransactions.Length);
		AssertRegLineTransaction("regLine with marks " + marks + " Transactions[0]", regLineTransactions[0], internalReferenteType, transactionGrossWeight, packageQty, transactionBondAmount, transactionDate);
		var regLinePivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Pivots created", 2, regLinePivots.Length);
		var regLinePivot1 = regLinePivots.First(x => x.SRV_GrossWeight == grossWeightPivot1);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot1", regLinePivot1, grossWeightPivot1, itemNumberPivot1, tariffPivot1, cusCodePivot1, descriptionPivot1);
		var regLinePivot2 = regLinePivots.First(x => x.SRV_GrossWeight == grossWeightPivot2);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot2", regLinePivot2, grossWeightPivot2, itemNumberPivot2, tariffPivot2, cusCodePivot2, descriptionPivot2);
	}

	void AssertRegLineTransaction(ZString assertMessage, CusTempStorageRegLineTransaction transaction, ZString internalReferenteType, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount, ZDateTimeOffset expectedDate)
	{
		AssertEquals(assertMessage + ".SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
		AssertEquals(assertMessage + ".SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
		AssertEquals(assertMessage + ".SRT_TransactionType", "OBL", transaction.SRT_TransactionType);
		AssertEquals(assertMessage + ".SRT_InternalReferenceNumber", "TS00000001", transaction.SRT_InternalReferenceNumber);
		AssertEquals(assertMessage + ".SRT_InternalReferenceType", internalReferenteType, transaction.SRT_InternalReferenceType);
		AssertEquals(assertMessage + ".SRT_TransactionDate", expectedDate, transaction.SRT_TransactionDate);
		AssertEquals(assertMessage + ".SRT_PhysicalInOutDate", expectedDate, transaction.SRT_PhysicalInOutDate);
		AssertEquals(assertMessage + ".SRT_BondAmount", bondAmount, transaction.SRT_BondAmount);
	}

	void AssertRegLineItemPivotAndItem(ZString assertMessage, EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot pivot, ZDecimal grossWeight, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		AssertEquals(assertMessage + ".SRV_GrossWeight", grossWeight, pivot.SRV_GrossWeight);
		var regLineItem = pivot.RegLineItem;
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsItemNumber", itemNumber, regLineItem.SRI_GoodsItemNumber);
		AssertEquals(assertMessage + ".RegLineItem.SRI_Tariff", tariff, regLineItem.SRI_Tariff);
		AssertEquals(assertMessage + ".RegLineItem.SRI_CusC4Number", cusCode, regLineItem.SRI_CusC4Number);
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsDescription", description, regLineItem.SRI_GoodsDescription);
	}

	(TemporaryStorageHeader temporaryStorageHeader, ZGuid premisesADTPK, ZGuid premisesLAMPK, ZGuid guaranteePK) SetUpDataToCreateTemporaryStorage(ZString messageType, bool pwOverride = true)
	{
		Factory.SetBulkTypeHelper();

		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = messageType;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.OH_Code = "AH2";
		orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ES555555555", "ES");
		var orgAddress2 = Factory.New<OrgAddress>();
		orgAddress2.OA_OH = orgHeader2.PK;
		orgAddress2.OA_Address1 = "Address2";

		var bill = temporaryStorageHeader.Bills.AddNew();
		var firstBill = temporaryStorageHeader.Bills.FirstOrDefault();
		firstBill.ABL_OA_Consignee = orgAddress.PK;
		firstBill.ABL_OA_Shipper = orgAddress2.PK;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999880000373";
		temporaryStorageHeader.EntryDate = new ZDateTime(2024, 02, 02, 03, 03, 00);
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.ClearanceDate = new ZDateTime(2025, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "1234567890";

		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader1 = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader1.CPH_Number = "GUARANTEEREF";
		cusPermitHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader1.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader1.CPH_Type = "TST";
		cusPermitHeader1.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader1.CPH_EndDate = ZDate.Today.AddDays(1);

		var cusPermitHeader2 = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader2.CPH_Number = "GUARANTEEREF";
		cusPermitHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader2.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader2.CPH_Type = "TRA";
		cusPermitHeader2.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader2.CPH_EndDate = ZDate.Today.AddDays(1);

		var guarantee = temporaryStorageHeader.Guarantee;
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20.0m;
		guarantee.PW_Override = pwOverride;
		guarantee.PW_RX_NKCurrency = "USD";

		var premises1 = Factory.New<CusTempStorageRegPremises>();
		premises1.SRP_Type = "ADT";
		premises1.SRP_CustomsLocation = "TSLoc";
		var premises2 = Factory.New<CusTempStorageRegPremises>();
		premises2.SRP_Type = "LAM";
		premises2.SRP_CustomsLocation = "TSLoc";
		var premises3 = Factory.New<CusTempStorageRegPremises>();
		premises3.SRP_Type = "ADT";
		premises3.SRP_CustomsLocation = "TSLocExtra";

		SetUpItemsToCreateTemporaryStorage(bill);

		return (temporaryStorageHeader, premises1.PK, premises2.PK, cusPermitHeader1.PK);
	}

	void SetUpItemsToCreateTemporaryStorage(TemporaryStorageBill bill)
	{
		var item1 = bill.PackedItems.AddNew();
		item1.SetValues(1, "111111", "CUSCODE1", "Description1", 6);
		AddDutyAmount(item1, 100);

		var pack1 = bill.Packs.AddNew();
		pack1.SetValues("BX", "marks1", 5);
		item1.PackagesPivot.AddPivotFor(pack1);

		var pack2 = bill.Packs.AddNew();
		pack2.SetValues("BX", "marks2", 7);
		item1.PackagesPivot.AddPivotFor(pack2);

		var pack3 = bill.Packs.AddNew();
		pack3.SetValues("BX", "marks3", 9);

		var item2 = bill.PackedItems.AddNew();
		item2.SetValues(2, "222222", "CUSCODE2", "Description2", 11);
		AddDutyAmount(item2, 220);

		var pack4 = bill.Packs.AddNew();
		pack4.SetValues("CT", "marks4", 2);
		item2.PackagesPivot.AddPivotFor(pack4);

		var pack5 = bill.Packs.AddNew();
		pack5.SetValues("BX", "marks5", 8);
		item2.PackagesPivot.AddPivotFor(pack5);

		var item3 = bill.PackedItems.AddNew();
		item3.SetValues(3, "333333", "CUSCODE3", "Description3", 3, isMissing: true);
		AddDutyAmount(item3, 85);

		var pack6 = bill.Packs.AddNew();
		pack6.SetValues("BX", "marks6", 5);
		item3.PackagesPivot.AddPivotFor(pack6);

		var item4 = bill.PackedItems.AddNew();
		item4.SetValues(4, "444444", "CUSCODE4", "Description4", 1);
		AddDutyAmount(item4, 10);

		var pack7 = bill.Packs.AddNew();
		pack7.SetValues("BX", "marks7", 33);
		item4.PackagesPivot.AddPivotFor(pack7);

		var item5 = bill.PackedItems.AddNew();
		item5.SetValues(5, "555555", "CUSCODE5", "Description5", 2);
		item5.PackagesPivot.AddPivotFor(pack7);
		AddDutyAmount(item5, 25);

		var item6 = bill.PackedItems.AddNew();
		item6.SetValues(6, "666666", "CUSCODE6", "Description6", 5);
		AddDutyAmount(item6, 52);

		var item7 = bill.PackedItems.AddNew();
		item7.SetValues(7, "777777", "CUSCODE7", "Description7", 40.27m);
		AddDutyAmount(item7, 62);

		var pack8 = bill.Packs.AddNew();
		pack8.SetValues("BX", "marks8", 5);
		item7.PackagesPivot.AddPivotFor(pack8);

		var pack9 = bill.Packs.AddNew();
		pack9.SetValues("BX", "marks9", 7);
		item7.PackagesPivot.AddPivotFor(pack9);
		item6.PackagesPivot.AddPivotFor(pack9);

		var pack9Bis = bill.Packs.AddNew();
		pack9Bis.SetValues("BX", "marks9", 20);
		item6.PackagesPivot.AddPivotFor(pack9Bis);

		var item8 = bill.PackedItems.AddNew();
		item8.SetValues(8, "888888", "CUSCODE8", "Description8", 10m);
		item8.PackagesPivot.AddPivotFor(pack9Bis);
		AddDutyAmount(item8, 54);

		var pack10 = bill.Packs.AddNew();
		pack10.SetValues("VG", "marks10", 0);
		item8.PackagesPivot.AddPivotFor(pack10);

		static void AddDutyAmount(TemporaryStoragePackedItem item, ZDecimal amount)
		{
			var dutyAndTax = item.DutiesAndTaxes.AddNew();
			dutyAndTax.AET_ChargeType = TemporaryStoragePackedItem.ChargeType.Duty;
			dutyAndTax.AET_ChargeAmount = amount;
		}
	}

	#endregion

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_MultipleEoris_EUandNonEU_UseEUN()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		var customsCode = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789A", "AU");
		customsCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

		var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1230789654", "BE");
		customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("22EORI1230789654000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_MultipleEoris_EUandGB_UseEUN()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		var customsCode = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI123456789A", "GB");
		customsCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

		var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1230789654", "BE");
		customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("22EORI1230789654000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_MultipleEoris_NonEUandGBWithXI_UseGB()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		var customsCode = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "789456123", "AU");
		customsCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);

		var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XIRI1230789654", "GB");
		customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("22XIRI1230789654000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_MultipleEoris_NonEUandGBWithoutXI_UseNonEU()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		var customsCode = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1230789654", "GB");
		customsCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

		var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "789456123", "AU");
		customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("2278945612300000000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_ESPAS()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("2233333333300000000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_ESNIF()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "123456789A", "ES");

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("22123456789A0000000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_ESEoriStartsWithCountryCode()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ESA12345678", "ES");

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("22A1234567800000000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_ESEoriDoesNotStartWithCountryCode()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var organization = temporaryStorageHeader.Branch.OrgProxy;

		organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("22A1234567800000000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 12, 19)]
	public void TestGenerateLocalReferenceNumber_EoriFromCompany()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.Branch.Company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1230789654", "BE");

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals("2212307896540000000002", temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 10, 20)]
	public void TestGenerateLocalReferenceNumber_NoEori()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals(ZString.Empty, temporaryStorageHeader.LRN);
	}

	[TestDate(2022, 10, 20)]
	public void TestGenerateLocalReferenceNumber_InvalidEori()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1234567890123456", "BE");

		(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
		Factory.Save();
		AssertEquals(ZString.Empty, temporaryStorageHeader.LRN);
	}

	[TestDate(2024, 01, 01, 10, 11, 12)]
	public void TestAddNewGuaranteeTransactionForG5V1Reception()
	{
		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.LRN = "LRNTest";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999912345678";
		temporaryStorageHeader.ClearanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "24ES009998987654321";

		var org1 = Factory.New<OrgHeader>();
		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = org1.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var tsGuarantee = temporaryStorageHeader.Guarantee;
		tsGuarantee.PW_BondNumber = "Test1";
		tsGuarantee.PW_BondAmount = 5.0m;

		CombineAssertions(() =>
		{
			var cusGuaranteeHeader = tsGuarantee.CusGuarantee;
			cusGuaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			AssertEquals("[PreReq] no transactions", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Reception();

			var transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == "TRA");
			var transaction = transactions.FirstOrDefault();
			AssertEquals("With Liability Amount not 0, transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2023, 01, 01, 02, 01, 00), "99994234567", -5.0m, "G5P LRNTest. MRN: 24ES009998987654321");

			tsGuarantee.PW_BondAmount = 0.0m;
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Reception();
			AssertEquals("With Liability Amount 0, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			tsGuarantee.PW_BondAmount = 5.0m;
			temporaryStorageHeader.DsdtMrnNumber = "23ES00999912345678";
			temporaryStorageHeader.ClearanceDate = ZDateTime.Empty;
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Reception();
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99993234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("Transaction Date is Today if Clearance Date is empty", new ZDateTime(2024, 01, 01, 10, 11, 12), transaction.CPL_TransactionDate);
		});
	}

	[TestDate(2024, 01, 01, 10, 11, 12)]
	public void TestAddNewGuaranteeTransactionForG5V1Expedition()
	{
		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.LRN = "LRNTest";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999912345678";
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "24ES009998987654321";

		var org1 = Factory.New<OrgHeader>();
		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = org1.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var tsGuarantee = temporaryStorageHeader.Guarantee;
		tsGuarantee.PW_BondAmount = 5.0m;
		CombineAssertions(() =>
		{
			tsGuarantee.PW_BondNumber = "Test";
			AssertNoExceptionThrown("When Guarantee.CusGuarantee is null no error should be thrown", () => temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition());

			tsGuarantee.PW_BondNumber = "Test1";
			var cusGuaranteeHeader = tsGuarantee.CusGuarantee;
			AssertEquals("[PreReq] no transactions", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition();
			AssertEquals("When no OBL transaction exists, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition();
			var transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var transaction = transactions.FirstOrDefault();
			AssertEquals("With OBL transaction exists and valid dates of guarantee and Liability Amount not 0, transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2023, 01, 01, 02, 01, 00), "99994234567", -5.0m, "G5X LRNTest. MRN: 24ES009998987654321");

			tsGuarantee.PW_BondAmount = 0.0m;
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition();
			AssertEquals("With OBL transaction and valid dates of guarantee, but Liability Amount 0, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			tsGuarantee.PW_BondAmount = 5.0m;
			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(1);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition();
			AssertEquals("With OBL transaction but Guarantee start date is in the future, Liability Amount not 0, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition();
			AssertEquals("With OBL transaction but Guarantee end date is in the past, Liability Amount not 0, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			temporaryStorageHeader.DsdtMrnNumber = "23ES00999912345678";
			temporaryStorageHeader.AcceptanceDate = ZDateTime.Empty;
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1Expedition();
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99993234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("Transaction Date is Today if Acceptance Date is empty", new ZDateTime(2024, 01, 01, 10, 11, 12), transaction.CPL_TransactionDate);
		});
	}

	[TestDate(2024, 01, 01, 10, 11, 12)]
	public void TestAddNewGuaranteeTransactionForTSM()
	{
		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.LRN = "LRNTest";
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999912345678";
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "24ES009998987654321";

		var org1 = Factory.New<OrgHeader>();
		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = org1.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var tsGuarantee = temporaryStorageHeader.Guarantee;
		tsGuarantee.PW_BondNumber = "Test1";
		tsGuarantee.PW_BondAmount = 5.0m;

		CombineAssertions(() =>
		{
			var cusGuaranteeHeader = tsGuarantee.CusGuarantee;
			cusGuaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			AssertEquals("[PreReq] no transactions", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));

			temporaryStorageHeader.AddNewGuaranteeTransactionForTSM();

			var transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == "TRA");
			var transaction = transactions.FirstOrDefault();
			AssertEquals("With Liability Amount not 0, transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2023, 01, 01, 02, 01, 00), "24ES00999912345678", -5.0m, "TSM TS00000001. MRN: 24ES009998987654321");

			tsGuarantee.PW_BondAmount = 0.0m;
			temporaryStorageHeader.AddNewGuaranteeTransactionForTSM();
			AssertEquals("With Liability Amount 0, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			tsGuarantee.PW_BondAmount = 5.0m;
			temporaryStorageHeader.DsdtMrnNumber = "23ES00999912345678";
			temporaryStorageHeader.AcceptanceDate = ZDateTime.Empty;
			temporaryStorageHeader.AddNewGuaranteeTransactionForTSM();
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "23ES00999912345678");
			transaction = transactions.FirstOrDefault();
			AssertEquals("Transaction Date is Today if Acceptance Date is empty", new ZDateTime(2024, 01, 01, 10, 11, 12), transaction.CPL_TransactionDate);
		});
	}

	[TestDate(2024, 01, 01, 10, 11, 12)]
	public void TestAddNewGuaranteeTransactionForG5V1ExpeditionCancel()
	{
		var cancelationDate = new ZDateTime(2023, 10, 10, 12, 20, 00);

		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.LRN = "LRNTest";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999912345678";
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "24ES009998987654321";

		temporaryStorageHeader.Bills.RemoveAndDeleteAll();
		var masterBill = temporaryStorageHeader.Bills.AddNew();
		masterBill.ABL_BolType = "BOL";

		var org1 = Factory.New<OrgHeader>();
		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = org1.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var orgHDeclarant = Factory.New<OrgHeader>();
		orgHDeclarant.OH_Code = "AAA";
		orgHDeclarant.OH_Category = OrgConstants.Category.Business;
		var orgADeclarant = orgHDeclarant.Addresses.AddNew();
		orgADeclarant.Address1 = "Declarant address";
		temporaryStorageHeader.AMA_OA_Declarant = orgADeclarant.PK;

		var tsGuarantee = temporaryStorageHeader.Guarantee;
		tsGuarantee.PW_Override = true;
		CombineAssertions(() =>
		{
			tsGuarantee.PW_BondNumber = "Test";
			AssertNoExceptionThrown("When Guarantee.CusGuarantee is null no error should be thrown", () => temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate));

			tsGuarantee.PW_BondNumber = "Test1";
			var cusGuaranteeHeader = tsGuarantee.CusGuarantee;
			AssertEquals("[PreReq] no transactions", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When no OBL transaction exists, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists but there are no CON transactions, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.AddTransaction("REFERENCE", "REFERENCE", ZString.Empty, ZString.Empty, -50.30m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists but there are no CON transactions with the correct reference, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.AddTransaction("99994234567", "SUM REFERENCE", ZString.Empty, ZString.Empty, -25.13m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			cusGuaranteeHeader.AddTransaction("99994234567", "SUM REFERENCE", ZString.Empty, ZString.Empty, -30.20m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference but destination goods location is not in premises, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			premises.SRP_CustomsLocation = "TSLoc";
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises but Declarant is not the same as Consignee, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			var orgAConsignee = orgHDeclarant.Addresses.AddNew();
			orgAConsignee.Address1 = "Consignee address";
			masterBill.ABL_OA_Consignee = orgAConsignee.PK;
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			var transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same orgH), new transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, cancelationDate, "99994234567", 55.33m, "G5X LRNTest. MRN: 24ES009998987654321 (Canceled)");

			cusGuaranteeHeader.AddTransaction("99993234567", "SUM REFERENCE 2", ZString.Empty, ZString.Empty, -45.23m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.DsdtMrnNumber = "23ES00999912345678";
			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(1);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same orgH) but Guarantee start date is in the future, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same orgH) but Guarantee end date is in the past, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(ZDateTime.Empty);
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99993234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same orgH), new transaction is created, with date now when cancelation date is empty", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2024, 01, 01, 10, 11, 12), "99993234567", 45.23m, "G5X LRNTest. MRN: 24ES009998987654321 (Canceled)");

			masterBill.ABL_OA_Consignee = ZGuid.Empty;
			masterBill.ABL_ConsigneeRegNo = "ESA12345678";
			cusGuaranteeHeader.AddTransaction("99992234567", "SUM REFERENCE 2", ZString.Empty, ZString.Empty, -35.23m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.DsdtMrnNumber = "22ES00999912345678";
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises but Declarant is not the same as Consignee (no EORI in declarant), no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "22ES00999912345678"));

			orgHDeclarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ESA12345678", "ES");
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99992234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same EORI), new transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, cancelationDate, "99992234567", 35.23m, "G5X LRNTest. MRN: 24ES009998987654321 (Canceled)");

			masterBill.ABL_ConsigneeRegNo = "ES123456789A";
			cusGuaranteeHeader.AddTransaction("99991234567", "SUM REFERENCE 2", ZString.Empty, ZString.Empty, -25.23m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.DsdtMrnNumber = "21ES00999912345678";
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises but Declarant is not the same as Consignee (no NIF in declarant), no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "21ES00999912345678"));

			orgHDeclarant.CustomsCodes.RemoveAndDeleteAll();
			orgHDeclarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "123456789A", "ES");
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99991234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same NIF), new transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, cancelationDate, "99991234567", 25.23m, "G5X LRNTest. MRN: 24ES009998987654321 (Canceled)");
		});
	}

	[TestDate(2024, 01, 01, 10, 11, 12)]
	public void TestAddNewGuaranteeTransactionForG5V1ExpeditionAmendment()
	{
		EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.LRN = "LRNTest";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = "TSLoc";

		temporaryStorageHeader.DsdtMrnNumber = "24ES00999912345678";
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = "24ES009998987654321";

		temporaryStorageHeader.Bills.RemoveAndDeleteAll();
		var masterBill = temporaryStorageHeader.Bills.AddNew();
		masterBill.ABL_BolType = "BOL";

		var org1 = Factory.New<OrgHeader>();
		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = org1.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "COD";
		premises.SRP_Description = "Desc";
		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var orgHDeclarant = Factory.New<OrgHeader>();
		orgHDeclarant.OH_Code = "AAA";
		orgHDeclarant.OH_Category = OrgConstants.Category.Business;
		var orgADeclarant = orgHDeclarant.Addresses.AddNew();
		orgADeclarant.Address1 = "Declarant address";
		temporaryStorageHeader.AMA_OA_Declarant = orgADeclarant.PK;

		var tsGuarantee = temporaryStorageHeader.Guarantee;
		tsGuarantee.PW_Override = true;
		tsGuarantee.PW_BondAmount = 0m;
		CombineAssertions(() =>
		{
			tsGuarantee.PW_BondNumber = "Test";
			AssertNoExceptionThrown("When Guarantee.CusGuarantee is null no error should be thrown", () => temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment());

			tsGuarantee.PW_BondNumber = "Test1";
			var cusGuaranteeHeader = tsGuarantee.CusGuarantee;
			AssertEquals("[PreReq] no transactions", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When no OBL transaction exists, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists but there are no CON transactions and PW_BondAmount is 0, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.AddTransaction("REFERENCE", "REFERENCE", ZString.Empty, ZString.Empty, -50.30m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists but there are no CON transactions with the correct reference and PW_BondAmount is 0, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.AddTransaction("99994234567", "SUM REFERENCE", ZString.Empty, ZString.Empty, -25.13m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			cusGuaranteeHeader.AddTransaction("99994234567", "SUM REFERENCE", ZString.Empty, ZString.Empty, -30.20m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference and PW_BondAmount is 0 but destination goods location is not in premises, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			premises.SRP_CustomsLocation = "TSLoc";
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference and PW_BondAmount is 0, destination goods location is in premises but Declarant is not the same as Consignee, no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			var orgAConsignee = orgHDeclarant.Addresses.AddNew();
			orgAConsignee.Address1 = "Consignee address";
			masterBill.ABL_OA_Consignee = orgADeclarant.PK;
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			var transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference and PW_BondAmount is 0, destination goods location is in premises and Declarant is the same as Consignee (same orgH), new transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2023, 01, 01, 02, 01, 00), "99994234567", 55.33m, "G5X LRNTest. MRN: 24ES009998987654321 (Customs Adj)");

			tsGuarantee.PW_BondAmount = 50.13m;
			cusGuaranteeHeader.AddTransaction("99993234567", "SUM REFERENCE 2", ZString.Empty, ZString.Empty, -45.23m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.DsdtMrnNumber = "23ES00999912345678";
			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(1);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same orgH) but Guarantee start date is in the future, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises and Declarant is the same as Consignee (same orgH) but Guarantee end date is in the past, no new transaction is created", 1, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));

			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			temporaryStorageHeader.AcceptanceDate = ZDateTime.Empty;
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99993234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference and PW_BondAmount is not 0 (and not like CON sum), destination goods location is in premises and Declarant is the same as Consignee (same orgH), new transaction is created, with date now when acceptance date is empty", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2024, 01, 01, 10, 11, 12), "99993234567", -4.90m, "G5X LRNTest. MRN: 24ES009998987654321 (Customs Adj)");

			temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
			masterBill.ABL_OA_Consignee = ZGuid.Empty;
			masterBill.ABL_ConsigneeRegNo = "ESA12345678";
			temporaryStorageHeader.DsdtMrnNumber = "22ES00999912345678";
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists, there are no CON transactions with the correct reference and PW_BondAmount is not 0 (and not like CON sum), destination goods location is in premises but Declarant is not the same as Consignee (no EORI in declarant), no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "22ES00999912345678"));

			orgHDeclarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ESA12345678", "ES");
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99992234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are no CON transactions with the correct reference and PW_BondAmount is not 0 (and not like CON sum), destination goods location is in premises and Declarant is the same as Consignee (same EORI), new transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2023, 01, 01, 02, 01, 00), "99992234567", -50.13m, "G5X LRNTest. MRN: 24ES009998987654321");

			tsGuarantee.PW_BondAmount = 20.03m;
			masterBill.ABL_ConsigneeRegNo = "ES123456789A";
			cusGuaranteeHeader.AddTransaction("99991234567", "SUM REFERENCE 2", ZString.Empty, ZString.Empty, -25.23m, 0, status: PermitTransactionStatusList.Codes.Confirmed, transactionType: Customs.Business.PermitTransactionTypeList.Codes.CUS);
			temporaryStorageHeader.DsdtMrnNumber = "21ES00999912345678";
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference, destination goods location is in premises but Declarant is not the same as Consignee (no NIF in declarant), no transaction is created", 0, cusGuaranteeHeader.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "21ES00999912345678"));

			orgHDeclarant.CustomsCodes.RemoveAndDeleteAll();
			orgHDeclarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "123456789A", "ES");
			temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionAmendment();
			transactions = tsGuarantee.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA && x.CPL_Reference == "99991234567");
			transaction = transactions.FirstOrDefault();
			AssertEquals("When OBL transaction exists, there are CON transactions with the correct reference and PW_BondAmount is not 0 (and not like CON sum), destination goods location is in premises and Declarant is the same as Consignee (same NIF), new transaction is created", 1, transactions.Count());
			AssertNewGuaranteeTransaction(transaction, new ZDateTime(2023, 01, 01, 02, 01, 00), "99991234567", 5.20m, "G5X LRNTest. MRN: 24ES009998987654321 (Customs Adj)");
		});
	}

	void AssertNewGuaranteeTransaction(SharedCusPermitLineTransaction transaction, ZDateTime date, ZString reference, ZDecimal tranValue, ZString comment)
	{
		AssertEquals("Transaction Date", date, transaction.CPL_TransactionDate);
		AssertEquals("Transaction Type", Customs.Business.PermitTransactionTypeList.Codes.TRA, transaction.CPL_TransactionType);
		AssertEquals("Reference", reference, transaction.CPL_Reference);
		AssertEquals("Value", tranValue, transaction.CPL_TranValue);
		AssertEquals("Comment", comment, transaction.CPL_Comment);
		AssertEquals("Status", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
	}

	public void TestCreateG5V1Reception()
	{
		header.AMA_JobReference = "JobReference";
		header.LRN = "LRNTest";
		header.AMA_MessageStatus = "AAA";
		header.CustomsStatus = "CLR";
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		header.DsdtMrnNumber = "24ES00999912345678";
		header.ClearanceNumber = "12345678901234567890";
		header.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		header.MRN = "24ES009998987654321";
		header.EntryStatus = "4";
		header.AMA_TransportMode = "AIR";
		header.TransportType = "41";
		header.ArrivalTransportMeansCode = "AIRCRAFT";
		header.AMA_CustomsOffice = "ES009999";
		header.DestinationCustomsOffice = "ES009998";
		header.AuthorizationType = "TST";
		header.AuthorizationNumber = "AUTHNUM";
		header.TrainingEntry = false;

		var orgHDeclarant = Factory.New<OrgHeader>();
		var orgADeclarant = orgHDeclarant.Addresses.AddNew();
		orgADeclarant.Address1 = "Declarant address";
		header.AMA_OA_Declarant = orgADeclarant.PK;

		var orgHRepresentative = Factory.New<OrgHeader>();
		var orgARepresentative = orgHRepresentative.Addresses.AddNew();
		orgARepresentative.Address1 = "Representative address";
		header.AMA_OA_Representative = orgARepresentative.PK;

		var orgHAuthOwner = Factory.New<OrgHeader>();
		header.AuthorizationOwner = orgHAuthOwner.PK;

		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
		goodsLocation.CGL_AdditionalIdentifier = "EORICODE";

		var destinationGoodsLocation = header.DestinationGoodsLocation;
		destinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		destinationGoodsLocation.CGL_CustomsOffice = "FR001234";

		var tsGuarantee = header.Guarantee;
		tsGuarantee.PW_BondNumber = "Test1";
		tsGuarantee.PW_Override = true;
		tsGuarantee.PW_BondAmount = 5.0m;

		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		header.AMA_GS_NKCustomsAgent = staff.GS_Code;
		header.AMA_CustomsProfile = "TestCert1";

		var container = header.Containers.AddNew();
		container.ACN_ContainerNumber = "CONT1";
		container.ACN_Seal1 = "SEAL1";
		var addSeal = container.AdditionalSeals.AddNew();
		addSeal.BK_SealNumber = "ADDSEAL";

		header.Bills.RemoveAndDeleteAll();
		var bill = header.Bills.AddNew();
		bill.TypeOfBillDocument = "A001";
		bill.ABL_BillNumber = "TranspDoc";

		bill.ABL_ShipperName = "BOB";
		bill.ABL_ConsigneeName = "CAT";

		var prevDoc = bill.PreviousDocuments.AddNew();
		prevDoc.CSI_Code = "A002";
		prevDoc.CSI_ReferenceNumber = "PrevDocBill";

		var supDoc = bill.SupportingDocuments.AddNew();
		supDoc.CSI_Code = "A003";
		supDoc.CSI_ReferenceNumber = "SupDocBill";

		var addInfo = bill.AdditionalInfos.AddNew();
		addInfo.CSI_Code = "A004";
		addInfo.CSI_ReferenceNumber = "AddInfoBill";

		var item1 = bill.PackedItems.AddNew();
		item1.SetValues(1, "11111", "CUSCODE1", "Description1", 11, false);
		item1.SetLiabilityCalculationValues(Core.Constants.CountryCodes.Spain, 100m, Core.Constants.CurrencyCodes.Spain, 1.1m, Core.Constants.Weight.Grams, 2.2m, Core.Constants.Weight.Kilograms, 3.3m, Core.Constants.Weight.Hectograms, new ZString[] { "ABCD" });

		var pack1 = bill.Packs.AddNew();
		pack1.SetValues("CT", "marks1", 2);
		item1.PackagesPivot.AddPivotFor(pack1);

		var item2 = bill.PackedItems.AddNew();
		item2.SetValues(2, "22222", "CUSCODE2", "Description2", 22, true);
		item2.SetLiabilityCalculationValues(Core.Constants.CountryCodes.Portugal, 200m, Core.Constants.CurrencyCodes.Portugal, 4.4m, Core.Constants.Weight.Grams, 5.5m, Core.Constants.Weight.Kilograms, 6.6m, Core.Constants.Weight.Hectograms, new ZString[] { "EFGH", "IJKL" });

		var pack2 = bill.Packs.AddNew();
		pack2.SetValues("BX", "marks2", 2);
		item2.PackagesPivot.AddPivotFor(pack2);

		CombineAssertions(() =>
		{
			var newG5v1Reception = header.CreateG5V1Reception();

			AssertEquals("AMA_MessageType is G5P", G5MessageTypeCodeList.Codes.G5v1Reception, newG5v1Reception.AMA_MessageType);
			AssertEquals("MRN is copied", "24ES009998987654321", newG5v1Reception.MRN);

			AssertEquals("LRN is not copied", ZString.Empty, newG5v1Reception.LRN);
			AssertEquals("AMA_JobReference is not copied", ZString.Empty, newG5v1Reception.AMA_JobReference);
			AssertEquals("AMA_MessageStatus is not copied", ZString.Empty, newG5v1Reception.AMA_MessageStatus);
			AssertEquals("CustomsStatus is not copied", ZString.Empty, newG5v1Reception.CustomsStatus);
			AssertEquals("DsdtMrnNumber is not copied", ZString.Empty, newG5v1Reception.DsdtMrnNumber);
			AssertEquals("ClearanceNumber is not copied", ZString.Empty, newG5v1Reception.ClearanceNumber);
			AssertEquals("AcceptanceDate is not copied", ZDateTime.Empty, newG5v1Reception.AcceptanceDate);
			AssertEquals("EntryStatus is not copied", ZString.Empty, newG5v1Reception.EntryStatus);

			AssertEquals("AMA_TransportMode is copied", "AIR", newG5v1Reception.AMA_TransportMode);
			AssertEquals("TransportType is copied", "41", newG5v1Reception.TransportType);
			AssertEquals("ArrivalTransportMeansCode is copied", "AIRCRAFT", newG5v1Reception.ArrivalTransportMeansCode);
			AssertEquals("AMA_CustomsOffice is copied", "ES009999", newG5v1Reception.AMA_CustomsOffice);
			AssertEquals("DestinationCustomsOffice is copied", "ES009998", newG5v1Reception.DestinationCustomsOffice);
			AssertEquals("AuthorizationType is copied", "TST", newG5v1Reception.AuthorizationType);
			AssertEquals("AuthorizationNumber is copied", "AUTHNUM", newG5v1Reception.AuthorizationNumber);
			AssertEquals("AMA_OA_Declarant is copied", orgADeclarant.PK, newG5v1Reception.AMA_OA_Declarant);
			AssertEquals("AMA_OA_Representative is copied", orgARepresentative.PK, newG5v1Reception.AMA_OA_Representative);
			AssertEquals("AuthorizationOwner is copied", orgHAuthOwner.PK, newG5v1Reception.AuthorizationOwner);
			AssertEquals("AMA_GS_NKCustomsAgent is copied", staff.GS_Code, newG5v1Reception.AMA_GS_NKCustomsAgent);
			AssertEquals("AMA_CustomsProfile is copied", "TestCert1", newG5v1Reception.AMA_CustomsProfile);
			AssertEquals("TrainingEntry is copied", false, newG5v1Reception.TrainingEntry);

			var goodsLocationG5P = newG5v1Reception.GoodsLocation;
			AssertEquals("goodsLocationG5P.CGL_Qualifier is copied", CusGoodsLocationQualifierList.Codes.EoriNumber, goodsLocationG5P.CGL_Qualifier);
			AssertEquals("goodsLocationG5P.CGL_AdditionalIdentifier is copied", "EORICODE", goodsLocationG5P.CGL_AdditionalIdentifier);

			var destinationGoodsLocationG5P = newG5v1Reception.DestinationGoodsLocation;
			AssertEquals("destinationGoodsLocationG5P.CGL_Qualifier is copied", CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, destinationGoodsLocationG5P.CGL_Qualifier);
			AssertEquals("destinationGoodsLocationG5P.CGL_CustomsOffice is copied", "FR001234", destinationGoodsLocationG5P.CGL_CustomsOffice);

			var tsGuaranteeG5P = newG5v1Reception.Guarantee;
			AssertEquals("tsGuaranteeG5P.PW_BondNumber is copied", "Test1", tsGuaranteeG5P.PW_BondNumber);
			AssertEquals("tsGuaranteeG5P.PW_Override is copied", true, tsGuaranteeG5P.PW_Override);
			AssertEquals("tsGuaranteeG5P.PW_BondAmount is copied", 5.0m, tsGuaranteeG5P.PW_BondAmount);

			AssertEquals("newG5v1Reception.Containers are copied, only one container exists", 1, newG5v1Reception.Containers.Count);
			var containerG5P = newG5v1Reception.Containers[0];
			AssertEquals("containerG5P.ACN_ContainerNumber is copied", "CONT1", containerG5P.ACN_ContainerNumber);
			AssertEquals("containerG5P.ACN_Seal1 is copied", "SEAL1", containerG5P.ACN_Seal1);
			AssertEquals("containerG5P.AdditionalSeals are copied, only one additional seal exists", 1, containerG5P.AdditionalSeals.Count);
			AssertEquals("containerG5P.AdditionalSeals[0].BK_SealNumber is copied", "ADDSEAL", containerG5P.AdditionalSeals[0].BK_SealNumber);

			AssertEquals("newG5v1Reception.Bills are copied, only one bill exists", 1, newG5v1Reception.Bills.Count);
			var billG5P = newG5v1Reception.Bills[0];
			AssertEquals("billG5P.TypeOfBillDocument is copied", "A001", billG5P.TypeOfBillDocument);
			AssertEquals("billG5P.ABL_BillNumber is copied", "TranspDoc", billG5P.ABL_BillNumber);
			AssertEquals("billG5P.ABL_ShipperName is copied", "BOB", billG5P.ABL_ShipperName);
			AssertEquals("billG5P.ABL_ConsigneeName is copied", "CAT", billG5P.ABL_ConsigneeName);

			AssertEquals("billG5P.PreviousDocuments are copied, only one prevdoc exists", 1, billG5P.PreviousDocuments.Count);
			var prevDocG5P = billG5P.PreviousDocuments[0];
			AssertEquals("prevDocG5P.CSI_Code is copied", "A002", prevDocG5P.CSI_Code);
			AssertEquals("prevDocG5P.CSI_ReferenceNumber is copied", "PrevDocBill", prevDocG5P.CSI_ReferenceNumber);

			AssertEquals("billG5P.SupportingDocuments are copied, only one supdoc exists", 1, billG5P.SupportingDocuments.Count);
			var supDocG5P = billG5P.SupportingDocuments[0];
			AssertEquals("supDocG5P.CSI_Code is copied", "A003", supDocG5P.CSI_Code);
			AssertEquals("supDocG5P.CSI_ReferenceNumber is copied", "SupDocBill", supDocG5P.CSI_ReferenceNumber);

			AssertEquals("billG5P.AdditionalInfos are copied, only one addinfo exists", 1, billG5P.AdditionalInfos.Count);
			var addInfoG5P = billG5P.AdditionalInfos[0];
			AssertEquals("addInfoG5P.CSI_Code is copied", "A004", addInfoG5P.CSI_Code);
			AssertEquals("addInfoG5P.CSI_ReferenceNumber is copied", "AddInfoBill", addInfoG5P.CSI_ReferenceNumber);

			AssertContainsExactElementsInAnyOrder("billG5P.Packs are copied",
				new (ZString, ZString, ZInt)[]
				{
					("CT", "marks1", 2),
					("BX", "marks2", 2)
				},
				billG5P.Packs.Select(p => (p.APA_PackUQ, p.APA_MarksAndNumbers, p.APA_PackQty)));

			AssertContainsExactElementsInAnyOrder("billG5P.PackedItems are copied",
				new (ZInt, ZBool, ZString, ZString, ZString, ZString, ZDecimal, ZString, ZDateTime, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
				{
					(1, false, "11111", "CUSCODE1", "Description1", "UCRCode", 11, "KG", ZDateTime.BrettsBirthday, Core.Constants.CountryCodes.Spain, 100m, Core.Constants.CurrencyCodes.Spain, 1.1m, Core.Constants.Weight.Grams, 2.2m, Core.Constants.Weight.Kilograms, 3.3m, Core.Constants.Weight.Hectograms, "ABCD"),
					(2, true, "22222", "CUSCODE2", "Description2", "UCRCode", 22, "KG", ZDateTime.BrettsBirthday, Core.Constants.CountryCodes.Portugal, 200m, Core.Constants.CurrencyCodes.Portugal, 4.4m, Core.Constants.Weight.Grams, 5.5m, Core.Constants.Weight.Kilograms, 6.6m, Core.Constants.Weight.Hectograms, "EFGH,IJKL")
				},
				billG5P.PackedItems.Select(i => (i.API_LineNo, i.IsMissing, i.API_Tariff, i.API_ChemicalSubstanceCode, i.API_GoodsDescription, i.UCR, i.API_GrossWeight, i.API_GrossWeightUQ, i.PresentationDate, i.API_RN_NKGoodsOrigin, i.API_GoodsValue, i.API_RX_NKGoodsValueCurrency, i.API_CustomsQty, i.API_CustomsUQ, i.API_CustomsQty2, i.API_CustomsUQ2, i.API_CustomsQty3, i.API_CustomsUQ3, i.API_Supplements)));

			var pack1G5P = billG5P.Packs.FirstOrDefault(p => ((EU.Business.CusTempStorage.TemporaryStoragePack)p).APA_PackUQ == "CT");
			var pack2G5P = billG5P.Packs.FirstOrDefault(p => ((EU.Business.CusTempStorage.TemporaryStoragePack)p).APA_PackUQ == "BX");

			var packedItem1G5P = billG5P.PackedItems.Cast<TemporaryStoragePackedItem>().FirstOrDefault(i => i.API_Tariff == "11111");
			var packedItem2G5P = billG5P.PackedItems.Cast<TemporaryStoragePackedItem>().FirstOrDefault(i => i.API_Tariff == "22222");

			AssertContainsExactElementsInAnyOrder("packedItem1G5P.PackagesPivot are copied",
				new ZGuid[]
				{
					pack1G5P.PK
				},
				packedItem1G5P.PackagesPivot.Select(p => p.APP_APA_Pack));

			AssertContainsExactElementsInAnyOrder("packedItem2G5P.PackagesPivot are copied",
				new ZGuid[]
				{
					pack2G5P.PK
				},
				packedItem2G5P.PackagesPivot.Select(p => p.APP_APA_Pack));
		});
	}

	public void TestClone()
	{
		header.AMA_TransportMode = "AIR";
		header.TransportType = "41";
		header.ArrivalTransportMeansCode = "AIRCRAFT";
		header.AMA_CustomsOffice = "ES009999";
		header.DestinationCustomsOffice = "ES009998";
		header.AuthorizationType = "TST";
		header.AuthorizationNumber = "AUTHNUM";

		var orgHAuthOwner = Factory.New<OrgHeader>();
		header.AuthorizationOwner = orgHAuthOwner.PK;

		var goodsLocation = header.GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
		goodsLocation.CGL_AdditionalIdentifier = "EORICODE";

		var destinationGoodsLocation = header.DestinationGoodsLocation;
		destinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		destinationGoodsLocation.CGL_CustomsOffice = "FR001234";

		var tsGuarantee = header.Guarantee;
		tsGuarantee.PW_BondNumber = "Test1";
		tsGuarantee.PW_Override = true;
		tsGuarantee.PW_BondAmount = 5.0m;

		CombineAssertions(() =>
		{
			var clonedHeader = (TemporaryStorageHeader)header.Clone();

			AssertEquals("AMA_TransportMode is copied", "AIR", clonedHeader.AMA_TransportMode);
			AssertEquals("TransportType is copied", "41", clonedHeader.TransportType);
			AssertEquals("ArrivalTransportMeansCode is copied", "AIRCRAFT", clonedHeader.ArrivalTransportMeansCode);
			AssertEquals("AMA_CustomsOffice is copied", "ES009999", clonedHeader.AMA_CustomsOffice);
			AssertEquals("DestinationCustomsOffice is copied", "ES009998", clonedHeader.DestinationCustomsOffice);
			AssertEquals("AuthorizationType is copied", "TST", clonedHeader.AuthorizationType);
			AssertEquals("AuthorizationNumber is copied", "AUTHNUM", clonedHeader.AuthorizationNumber);
			AssertEquals("AuthorizationOwner is copied", orgHAuthOwner.PK, clonedHeader.AuthorizationOwner);

			var goodsLocation = clonedHeader.GoodsLocation;
			AssertEquals("goodsLocation.CGL_Qualifier is copied", CusGoodsLocationQualifierList.Codes.EoriNumber, goodsLocation.CGL_Qualifier);
			AssertEquals("goodsLocation.CGL_AdditionalIdentifier is copied", "EORICODE", goodsLocation.CGL_AdditionalIdentifier);

			var destinationGoodsLocation = clonedHeader.DestinationGoodsLocation;
			AssertEquals("destinationGoodsLocation.CGL_Qualifier is copied", CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, destinationGoodsLocation.CGL_Qualifier);
			AssertEquals("destinationGoodsLocation.CGL_CustomsOffice is copied", "FR001234", destinationGoodsLocation.CGL_CustomsOffice);

			var tsGuarantee = clonedHeader.Guarantee;
			AssertEquals("tsGuarantee.PW_BondNumber is copied", "Test1", tsGuarantee.PW_BondNumber);
			AssertEquals("tsGuarantee.PW_Override is copied", true, tsGuarantee.PW_Override);
			AssertEquals("tsGuarantee.PW_BondAmount is copied", 5.0m, tsGuarantee.PW_BondAmount);
		});
	}

	public void TestGuarantee()
	{
		AssertType<TemporaryStorageHeaderGuarantee>(header.Guarantee);
	}

	public void TestStorageHeaderPopulateGuaranteeWhenLocationIsSelectedWithTemporaryStorage()
	{
		var org1 = Factory.New<OrgHeader>();
		org1.OH_Code = "REP";
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		const string customsLocation1 = "ES001234";
		const string guaranteeNumber1 = "Guarantee1";
		const string guaranteeNumber2 = "Guarantee2";

		var registryPNTSEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		CombineAssertions(() =>
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				temporaryStorageHeader.DestinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				temporaryStorageHeader.DestinationGoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = customsLocation1;

				var guarantee1 = Factory.New<CusGuaranteeHeader>();
				guarantee1.CPH_Number = guaranteeNumber1;
				guarantee1.CPH_OH_PermitHolder = org1.PK;
				guarantee1.CPH_Type = EUGuaranteeTypeList.Codes.TST;
				guarantee1.CPH_StartDate = ZDate.BrettsBirthday;
				var rule1 = guarantee1.CusGuaranteeRules.AddNew();
				rule1.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.TSP;
				rule1.CPR_ValueFrom = customsLocation1;
				Factory.Save();

				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = ZString.Empty;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				temporaryStorageHeader.Guarantee.PW_BondNumber = ZString.Empty;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = customsLocation1;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				AssertEquals("PNTS Enabled, 1 Guarantee, PW_Bond empty", guaranteeNumber1, temporaryStorageHeader.Guarantee.PW_BondNumber);

				temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = ZString.Empty;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				temporaryStorageHeader.Guarantee.PW_BondNumber = ZString.Empty;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = customsLocation1;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				AssertNullOrEmpty("PNTS Enabled, 1 Guarantee, PW_Bond empty, MessageType = LAM", temporaryStorageHeader.Guarantee.PW_BondNumber);

				temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
				temporaryStorageHeader.Guarantee.PW_BondNumber = guaranteeNumber2;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = ZString.Empty;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = customsLocation1;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				AssertEquals("PNTS Enabled, 1 Guarantee, PW_Bond not empty", guaranteeNumber2, temporaryStorageHeader.Guarantee.PW_BondNumber);

				var guarantee2 = Factory.New<CusGuaranteeHeader>();
				guarantee2.CPH_Number = guaranteeNumber2;
				guarantee2.CPH_OH_PermitHolder = org1.PK;
				guarantee2.CPH_Type = EUGuaranteeTypeList.Codes.TST;
				guarantee2.CPH_StartDate = ZDate.BrettsBirthday;
				var rule2 = guarantee2.CusGuaranteeRules.AddNew();
				rule2.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.TSP;
				rule2.CPR_ValueFrom = customsLocation1;
				Factory.Save();

				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = ZString.Empty;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				temporaryStorageHeader.Guarantee.PW_BondNumber = ZString.Empty;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = customsLocation1;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				AssertNullOrEmpty("PNTS Enabled, 2 Guarantee, PW_Bond empty", temporaryStorageHeader.Guarantee.PW_BondNumber);

				guarantee1.Delete();
				guarantee2.Delete();
				Factory.Save();
				temporaryStorageHeader.DestinationGoodsLocation.CGL_CustomsOffice = customsLocation1;
				temporaryStorageHeader.DestinationGoodsLocation.CGL_CustomsOffice = ZString.Empty;
				temporaryStorageHeader.Guarantee.PW_BondNumber = ZString.Empty;
				AssertNullOrEmpty("PNTS Enabled, no Guarantee, PW_Bond empty", temporaryStorageHeader.Guarantee.PW_BondNumber);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				temporaryStorageHeader.DestinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				temporaryStorageHeader.DestinationGoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = customsLocation1;

				var guarantee1 = Factory.New<CusGuaranteeHeader>();
				guarantee1.CPH_Number = guaranteeNumber1;
				guarantee1.CPH_OH_PermitHolder = org1.PK;
				guarantee1.CPH_Type = EUGuaranteeTypeList.Codes.TST;
				guarantee1.CPH_StartDate = ZDate.BrettsBirthday;
				var rule1 = guarantee1.CusGuaranteeRules.AddNew();
				rule1.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.TSP;
				rule1.CPR_ValueFrom = customsLocation1;
				Factory.Save();

				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = ZString.Empty;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				temporaryStorageHeader.Guarantee.PW_BondNumber = ZString.Empty;
				temporaryStorageHeader.DestinationGoodsLocation.Address.AuthorisationNumber = customsLocation1;
				_ = temporaryStorageHeader.DestinationGoodsLocationDescription;
				AssertNullOrEmpty("PNTS disabled", temporaryStorageHeader.Guarantee.PW_BondNumber);
			}
		});
	}

	public void TestIsMessageTypeMatchingToTSRegisterManagement() => CombineAssertions(() =>
	{
		var validMessageTypes = new[] { G5MessageTypeCodeList.Codes.G5v1Expedition, "G5E" };
		var allMessageTypes = new G5MessageTypeCodeList().GetAllCodes();

		var header = Factory.New<TemporaryStorageHeader>();
		AssertEquals("Initial value with empty AMA_MessageType.", false, header.IsMessageTypeMatchingToTSRegisterManagementSelectInventory);
		foreach (var code in allMessageTypes)
		{
			header.AMA_MessageType = code;
			AssertEquals($"AMA_MessageType = '{header.AMA_MessageType}'.", validMessageTypes.Contains(code), header.IsMessageTypeMatchingToTSRegisterManagementSelectInventory);
		}
	});

	#region ReserveTemporaryStorageGoods

	const string InternalReference = "ES00001";

	public void TestGetEntryLineDataDeclaredToReserveTSGoods()
	{
		Factory.SetBulkTypeHelper();

		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";

		temporaryStorageHeader.Bills.RemoveAndDeleteAll();
		var bill1 = temporaryStorageHeader.Bills.AddNew();
		var packedItem1 = bill1.PackedItems.AddNew();
		var packedItem2 = bill1.PackedItems.AddNew();
		var packedItem3 = bill1.PackedItems.AddNew();
		var packedItem4 = bill1.PackedItems.AddNew();

		CombineAssertions(() =>
		{
			var (declarationDataToReserveTSGoodsList, messageReturned) = temporaryStorageHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in the declaration", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no documents in the declaration", ZString.Empty, messageReturned);

			var previousDoc1 = packedItem1.PreviousDocuments.AddNew();
			previousDoc1.CSI_ReferenceNumber = "24ES00999980001282";
			previousDoc1.CSI_Code = "BBB";
			previousDoc1.CSI_ItemNumber = 4;
			previousDoc1.CSI_LineNo = 1;
			packedItem1.API_GrossWeight = 200.446m;
			packedItem1.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var pack1 = bill1.Packs.AddNew();
			pack1.SetValues("FR", "VIN1", 5);
			var linkPackage1 = packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack1);
			linkPackage1.IsLinked = true;

			var pack2 = bill1.Packs.AddNew();
			pack2.SetValues("FR", "VIN2", 1);
			var linkPackage2 = packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack2);
			linkPackage2.IsLinked = true;

			var previousDoc2 = packedItem2.PreviousDocuments.AddNew();
			previousDoc2.CSI_ReferenceNumber = "24ES00999980001282";
			previousDoc2.CSI_Code = "BBB";
			previousDoc2.CSI_ItemNumber = 4;
			previousDoc2.CSI_LineNo = 2;
			packedItem2.API_GrossWeight = 2000.446m;
			packedItem2.API_GrossWeightUQ = Core.Constants.Weight.Grams;

			var pack3 = bill1.Packs.AddNew();
			pack3.SetValues("CT", "marks", 9);
			var linkPackage3 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack3);
			linkPackage3.IsLinked = true;

			var pack4 = bill1.Packs.AddNew();
			pack4.SetValues("NE", "marks2", 4);
			var linkPackage4 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack4);
			linkPackage4.IsLinked = true;

			var pack5 = bill1.Packs.AddNew();
			pack5.SetValues("VG", "bulk gas marks", 2);
			var linkPackage5 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack5);
			linkPackage5.IsLinked = true;

			var previousDoc3 = packedItem3.PreviousDocuments.AddNew();
			previousDoc3.CSI_ReferenceNumber = "24ES00999980001283";
			previousDoc3.CSI_Code = "BBB";
			previousDoc3.CSI_ItemNumber = 2;
			previousDoc3.CSI_LineNo = 2;
			previousDoc3.CSI_Quantity = 100.446m;
			previousDoc3.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			packedItem3.API_GrossWeight = 20m;
			packedItem3.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var pack6 = bill1.Packs.AddNew();
			pack6.SetValues("CT", "marks4", 8);
			var linkPackage6 = packedItem3.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack6);
			linkPackage6.IsLinked = true;

			var pack7 = bill1.Packs.AddNew();
			pack7.SetValues("CT", "marks", 6);
			var linkPackage7 = packedItem3.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack7);
			linkPackage7.IsLinked = true;

			var pack8 = bill1.Packs.AddNew();
			pack8.SetValues("VG", "marks6", 6);
			var linkPackage8 = packedItem3.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack8);
			linkPackage8.IsLinked = true;

			var linkPackage3Bis = packedItem3.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack3);
			linkPackage3Bis.IsLinked = true;

			var previousDoc4 = bill1.PreviousDocuments.AddNew();
			previousDoc4.CSI_ReferenceNumber = "24ES00999980001272";
			previousDoc4.CSI_Code = "AAA";
			previousDoc4.CSI_ItemNumber = 1;
			previousDoc4.CSI_LineNo = 2;
			packedItem4.API_GrossWeight = 0.989m;
			packedItem4.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var pack9 = bill1.Packs.AddNew();
			pack9.SetValues("BX", "marks5", 7);
			var linkPackage9 = packedItem4.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == pack9);
			linkPackage9.IsLinked = true;

			var expectedPackagesForDoc1 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
			{
				("FR", 5, "VIN1", false),
				("FR", 1, "VIN2", false)
			};
			var expectedPackagesForDoc2 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
			{
					("CT", 15, "marks", false),
					("NE", 4, "marks2", false),
					("VG", 2, "bulk gas marks", true),
					("VG", 6, "marks6", true),
					("CT", 8, "marks4", false)
			};
			var expectedPackagesForDoc3 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> { ("BX", 7, "marks5", false) };

			(declarationDataToReserveTSGoodsList, messageReturned) = temporaryStorageHeader.GetEntryLineDataDeclaredToReserveTSGoods();
			AssertEquals("declarationDataToReserveTSGoodsList has 3 elements", 3, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned even when there are documents in the declaration", ZString.Empty, messageReturned);

			var declarationDataToReserveTSGoods1 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 200.446m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods1", declarationDataToReserveTSGoods1, "24ES00999980001282", 1, 200.446m, 200.446m, expectedPackagesForDoc1);

			var declarationDataToReserveTSGoods2 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 22.000446m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods2", declarationDataToReserveTSGoods2, "24ES00999980001282", 2, 22.000446m, 0, expectedPackagesForDoc2);

			var declarationDataToReserveTSGoods3 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 0.989m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods3", declarationDataToReserveTSGoods3, "24ES00999980001272", 2, 0.989m, 0, expectedPackagesForDoc3);
		});

		void AssertDeclarationDataToReserveTSGoods(ZString message, DeclarationDataToReserveTSGoods declarationData, ZString expectedDocRef, ZInt expectedLineNo, ZDecimal expectedGrossWeight, ZDecimal expectedGrossWeightForVINs, List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> expectedPackages)
		{
			AssertEquals(message + " docRef", expectedDocRef, declarationData.Document.CSI_ReferenceNumber);
			AssertEquals(message + " docLineNo", expectedLineNo, declarationData.Document.CSI_LineNo);
			AssertEquals(message + " totalGrossWeight", expectedGrossWeight, declarationData.TotalGrossWeight);
			AssertEquals(message + " totalGrossWeightForVINs", expectedGrossWeightForVINs, declarationData.TotalGrossWeightForVINs);
			AssertContainsExactElementsInAnyOrder(message + " packages", expectedPackages, declarationData.Packages);
		}
	}

	public void TestTemporaryStorageTransactionInternalReferenceNumberAndType()
	{
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.LRN = InternalReference;

		CombineAssertions(() =>
		{
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to LRN", InternalReference, temporaryStorageHeader.TemporaryStorageTransactionInternalReferenceNumber);

			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set to G5", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, temporaryStorageHeader.TemporaryStorageTransactionInternalReferenceType);
		});
	}

	#region GetDataToReserveTemporaryStorageGoods

	public void TestGetDataToReserveTemporaryStorageGoods_Without337doc()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(false, ZString.Empty, ZString.Empty);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithoutRegHeader()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(regHeaderReference: "reference");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithoutPremises_DocRefShorterThan18()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "TS00000001/ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithoutPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "TS00000001/ES00001: Goods in TSD Number 24ES00999980001282 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithoutPremises_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "TS00000001/ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithoutRegLineItem_DocRefShorterThan18()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(prevDocLineNo: 2, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(prevDocLineNo: 2, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 24ES00999980001282. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(prevDocLineNo: 2);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithVINError_DocRefShorterThan18()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 24ES00999980001282, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefShorterThan18()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefShorterThan18()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, _, _, _, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightError_DocRefShorterThan18()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_DocRefShorterThan18()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(5, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageVINs, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is not empty", "ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReference, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(5, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageVINs, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is not empty", "ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 24ES00999980001282, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(5, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageVINs, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is not empty", "ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(prevDocReference: FormattedPrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightTransactionForVINs_DocRefShorterThan18()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 11m, prevDocReference: FormattedPrevDocReference, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(5, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageVINs, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is empty", ZString.Empty, resultMessageVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightTransactionForVINs_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 11m, regHeaderReference: PrevDocReference, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(5, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageVINs, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is empty", ZString.Empty, resultMessageVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithGrossWeightTransactionForVINs_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 11m, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(5, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageVINs, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is empty", ZString.Empty, resultMessageVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithMultipleDocs_DocRefShorterThan18()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference, addExtraDoc: true, secondDocRef: FormattedSecondDocRef);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithMultipleDocs_DocRefLongerThan18_WithoutFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReference, addExtraDoc: true, secondRegHeaderReference: SecondDocRef);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 24ES00999898765432, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_With337docWithRegHeaderWithPremises_WithMultipleDocs_DocRefLongerThan18_WithFormatting()
	{
		var (header, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, addExtraDoc: true);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 5, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, _, dataToReserveGoodsList) = GetDataToReserveTemporaryStorageGoods(Factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, PrevDocCode, LocationInEntry, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	#endregion

	(TemporaryStorageHeader header, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine1, CusTempStorageRegLine regLine2, CusTempStorageRegLine regLine3, CusTempStorageRegLine regLine4, CusTempStorageRegLineItem regLineItem)
		SetUpDataForForGetDataToReserveTemporaryStorageGoods(bool shouldSetPreviousDocuments = true, string prevDocCode = PrevDocCode, string prevDocReference = PrevDocReference, int prevDocLineNo = 1,
		string locationInPremises = LocationInEntry, string regHeaderReference = FormattedPrevDocReference, string packageVin = "VIN1", int packageQtyNotBulk = 9, decimal transactionGrossWeight = 40m,
		bool addExtraDoc = false, decimal transactionGrossWeightForExtraDoc = 6m, string bulkPackageTypeForRegLine = "VG", string secondDocRef = SecondDocRef, string secondRegHeaderReference = FormattedSecondDocRef, bool createInTransaction1OBL = false)
	{
		Factory.SetBulkTypeHelper();

		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_JobReference = "TS00000001";
		temporaryStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		temporaryStorageHeader.LRN = EntryReference;

		temporaryStorageHeader.Bills.RemoveAndDeleteAll();
		var bill1 = temporaryStorageHeader.Bills.AddNew();

		var packedItem1 = bill1.PackedItems.AddNew();
		packedItem1.API_GrossWeight = 11m;
		packedItem1.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		var vehicle1 = bill1.Packs.AddNew();
		vehicle1.SetValues("FR", "VIN1", 5);
		var linkVehicle1 = packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == vehicle1);
		linkVehicle1.IsLinked = true;

		var packedItem2 = bill1.PackedItems.AddNew();
		packedItem2.API_GrossWeight = 11m;
		packedItem2.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		var package1 = bill1.Packs.AddNew();
		package1.SetValues("BX", "marks", 9);
		var linkPackage1 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package1);
		linkPackage1.IsLinked = true;

		var package2 = bill1.Packs.AddNew();
		package2.SetValues("VG", "bulk gas marks", 0);
		var linkPackage2 = packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package2);
		linkPackage2.IsLinked = true;

		if (shouldSetPreviousDocuments)
		{
			var previousDoc1 = packedItem1.PreviousDocuments.AddNew();
			previousDoc1.CSI_Code = prevDocCode;
			previousDoc1.CSI_ReferenceNumber = prevDocReference;
			previousDoc1.CSI_LineNo = prevDocLineNo;

			var previousDoc2 = packedItem2.PreviousDocuments.AddNew();
			previousDoc2.CSI_Code = prevDocCode;
			previousDoc2.CSI_ReferenceNumber = prevDocReference;
			previousDoc2.CSI_LineNo = prevDocLineNo;
		}

		if (addExtraDoc)
		{
			var packedItem3 = bill1.PackedItems.AddNew();
			packedItem3.API_GrossWeight = 20m;
			packedItem3.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var package3 = bill1.Packs.AddNew();
			package3.SetValues("VG", "bulk gas marks2", 0);
			var linkPackage3 = packedItem3.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package3);
			linkPackage3.IsLinked = true;

			if (shouldSetPreviousDocuments)
			{
				var previousDoc3 = packedItem3.PreviousDocuments.AddNew();
				previousDoc3.CSI_Code = prevDocCode;
				previousDoc3.CSI_ReferenceNumber = secondDocRef;
				previousDoc3.CSI_LineNo = 2;
			}
		}

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = premises.PK;
		var regLine1 = Factory.New<CusTempStorageRegLine>();
		regLine1.SRL_LineNumber = 1;
		regLine1.SRL_CustomsStatus = "OPN";
		regLine1.SRL_PackageType = "FR";
		regLine1.SRL_PackageMarks = packageVin;
		regLine1.SRL_SRH = regHeader.PK;
		var regLineTransactionPND = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND.SRT_InternalReferenceNumber = EntryReference;
		regLineTransactionPND.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		if (createInTransaction1OBL)
		{
			var regLineTransaction11 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction11.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction11.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction11.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction11.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
			regLineTransaction11.SRT_GrossWeight = transactionGrossWeight;
		}

		var regLine2 = Factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 3;
		regLine2.SRL_CustomsStatus = "OPN";
		regLine2.SRL_PackageType = bulkPackageTypeForRegLine;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction2.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

		var regLine3 = Factory.New<CusTempStorageRegLine>();
		regLine3.SRL_LineNumber = 4;
		regLine3.SRL_CustomsStatus = "OPN";
		regLine3.SRL_PackageType = "BX";
		regLine3.SRL_SRH = regHeader.PK;
		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

		var regLineItemPivot2 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

		var regLineItemPivot3 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

		var regLine4 = (CusTempStorageRegLine)null;
		if (addExtraDoc)
		{
			var regHeader2 = Factory.New<CusTempStorageRegHeader>();
			regHeader2.SRH_AppCode = "BBB";
			regHeader2.SRH_Reference = secondRegHeaderReference;
			regHeader2.SRH_SRP_Premises = premises.PK;

			regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VG";
			regLine4.SRL_SRH = regHeader2.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
			regLineTransaction4.SRT_GrossWeight = transactionGrossWeightForExtraDoc;

			var regLineItem2 = Factory.New<CusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 2;

			var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;
		}

		Factory.Save();

		return (temporaryStorageHeader, regLineTransactionPND, regLine1, regLine2, regLine3, regLine4, regLineItem);
	}

	const string EntryReference = "ES00001";
	const string LocationInEntry = "9999000002";
	const string PrevDocCode = "337";
	const string PrevDocReference = "24ES00999980001282";
	const string FormattedPrevDocReference = "99994000128";
	const string SecondDocRef = "24ES00999898765432";
	const string FormattedSecondDocRef = "99984876543";
	const string MRNCode = "20ES00999930006184";
	readonly ZDateTime issueDate = new ZDateTime(2024, 06, 10, 11, 11, 11);

	#endregion

	#region ConfirmTemporaryStorageGoodsConsumption

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("CustomsStatus is changed to CLR", "CLR", header.CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_EmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInEntry: ZString.Empty);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("CustomsStatus is changed to CLR", "CLR", header.CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_LocationNotManagedInPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("CustomsStatus is changed to CLR", "CLR", header.CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoAction_CustomsStatusCLR()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			SetUpRefData();
			Factory.Save();

			var oldComment = "Extra Old Comment";
			var expectedComment = "G5 JOB: TS00000001";

			var orgHeader = SetUpOrgHeader();
			var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			header.ClearanceDate = issueDate;

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "NE";
			var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 10, 6);
			var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2);

			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);
			var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 6);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_PackageType = "AA";
			var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";

			var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 5, 6);
			regLineTransaction6.SRT_Comments = oldComment;
			var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("CustomsStatus is changed to the correct value", "CLR", header.CustomsStatus);

				AssertTransactionAfterConfirmAction("regLineTransaction1", regLineTransaction1, expectedComment);
				AssertTransactionAfterConfirmAction("regLineTransaction2", regLineTransaction2, expectedComment);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction4", regLineTransaction4, expectedComment);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction6", regLineTransaction6, expectedComment + " - " + oldComment);
				AssertTransactionAfterConfirmAction("regLineTransaction7", regLineTransaction7, expectedComment);

				AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine3 CustomsStatus is changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
				AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_CustomsStatusTUC()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl;

				AssertEquals("CustomsStatus is changed to TUC", "TUC", header.CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusCAN()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled;

				AssertEquals("CustomsStatus is changed to CAN", "CAN", header.CustomsStatus);
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_WriteOff()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			header.ClearanceDate = issueDate;
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, -3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 2, grossWeight: -2);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 1, grossWeight: -1);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("2 Write off transactions in Guarantee are created", 2, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);

				var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<CusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", FormattedPrevDocReference, 2.0m);
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", FormattedPrevDocReference, 1.0m);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_NoPendingAmount()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 0.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_PendingAmountPositive()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

				var expectedError = "|RES=Reference 99994000128 has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
				AssertEquals("New event in logs", expectedError, header.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			});
		}
	}

	void AssertTransactionAfterConfirmAction(ZString transactionName, CusTempStorageRegLineTransaction transaction, string comment, string transactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, string referenceType = "MRN", string mrnCode = MRNCode, ZDateTimeOffset? expectedIssueDate = null)
	{
		AssertEquals(transactionName + "'s SRT_TransactionStatus is correct", transactionStatus, transaction.SRT_TransactionStatus);
		AssertEquals(transactionName + "'s SRT_ReferenceType is correct", referenceType, transaction.SRT_ReferenceType);
		AssertEquals(transactionName + "'s SRT_Reference is correct", mrnCode, transaction.SRT_Reference);
		AssertEquals(transactionName + "'s SRT_Comments is correct", comment, transaction.SRT_Comments);
		AssertEquals(transactionName + "'s SRT_TransactionDate is correct", expectedIssueDate != null ? expectedIssueDate : issueDate.ToOffset(), transaction.SRT_TransactionDate);
		AssertEquals(transactionName + "'s SRT_PhysicalInOutDate is correct", expectedIssueDate != null ? expectedIssueDate : issueDate.ToOffset(), transaction.SRT_PhysicalInOutDate);
	}

	void AssertWriteOffTransaction(CusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZDecimal expectedTranValue)
	{
		AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
		AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
		AssertEquals(transactionName + "'s CPL_TransactionDate is Entry Release Date", issueDate, transaction.CPL_TransactionDate);
		AssertEquals(transactionName + "'s CPL_Comment is Write-off + reference / Type + MRN", string.Format("Write-off TS {0} / {1} {2}", expectedReference, "G5", MRNCode), transaction.CPL_Comment);
		AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
		AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
	}

	#region ConfirmTemporaryStorageGoodsConsumptionAfterAddingPNDTransactions

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenCCC_WithPNDTransaction()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = "CCC";

				AssertEquals("CustomsStatus is changed to CCC", "CCC", header.CustomsStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenCCC_WithCONTransaction()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldSetPreviousDocuments: true);
			regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = "CCC";

				AssertEquals("CustomsStatus is changed to CCC", "CCC", header.CustomsStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenIsCCC_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = "CCC";

				AssertEquals("CustomsStatus is changed to CCC", "CCC", header.CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenIsCCC_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: PrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = "CCC";

				AssertEquals("CustomsStatus is changed to CCC", "CCC", header.CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenIsCCC_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: FormattedPrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = "CCC";

				AssertEquals("CustomsStatus is changed to CCC", "CCC", header.CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenIsCLR_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("CustomsStatus is changed to CLR", EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance, header.CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", "G5 JOB: TS00000001", transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenIsCCR_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: PrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("CustomsStatus is changed to CLR", EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance, header.CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", "G5 JOB: TS00000001", transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenIsCCR_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: FormattedPrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("CustomsStatus is changed to CLR", EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance, header.CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", "G5 JOB: TS00000001", transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionWhenEntryStatusNotEmpty_WhenIsEmpty()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInEntry: ZString.Empty, createTransaction: false, shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				header.CustomsStatus = ZString.Empty;
				AssertEquals("CustomsStatus is changed to empty", ZString.Empty, header.CustomsStatus);
				AssertEquals("regLineTransaction was not created", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	#endregion

	public void TestSetClearanceNumber()
	{
		CombineAssertions(() =>
		{
			header.SetClearanceNumber("CLEARANCE1234567");
			AssertEquals("CSV clearance has been changed when the pop up was accepted", "CLEARANCE1234567", header.ClearanceNumber);
			AssertEquals("New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code|TYP=CSV", header.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			header.SetClearanceNumber("CLEARANCE1234567");
			AssertEquals("When CSV Clearance Number remain the same value", "CLEARANCE1234567", header.ClearanceNumber);
			AssertEquals("The Last Event is still the same cause the CSV Clearance does not change", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code|TYP=CSV", header.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			Thread.Sleep(5);

			header.SetClearanceNumber("AAAAAAAAAAAAAAAA");
			AssertEquals("CSV clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", header.ClearanceNumber);
			AssertEquals("New event in logs for second change", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code|TYP=CSV", header.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestSetClearanceDate()
	{
		CombineAssertions(() =>
		{
			header.SetClearanceDate(new ZDateTime(2023, 06, 14, 11, 12, 13));
			AssertEquals("ClearanceDate is updated", new ZDateTime(2023, 06, 14, 11, 12, 13), header.ClearanceDate);
			AssertEquals("New event in logs", "|NEW=2023-06-14T11:12:13|RES=Manually Added Clearance Date|TYP=CSV", header.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			header.SetClearanceDate(new ZDateTime(2023, 06, 14, 11, 12, 13));
			AssertEquals("The Last Event is still the same", "|NEW=2023-06-14T11:12:13|RES=Manually Added Clearance Date|TYP=CSV", header.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

			Thread.Sleep(5);

			header.SetClearanceDate(new ZDateTime(2024, 04, 28, 10, 09, 08));
			AssertEquals("CSV Exit Certificate has been changed when the pop up was accepted a second time", new ZDateTime(2024, 04, 28, 10, 09, 08), header.ClearanceDate);
			AssertEquals("New event in logs for second change", "|NEW=2024-04-28T10:09:08|OLD=2023-06-14T11:12:13|RES=Manually Added Clearance Date|TYP=CSV", header.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
		});
	}

	public void TestG5XOnCustomsStatusChangeToCLRNotUseClearanceDateforSRT_PhysicalInOutDateAndSRT_TransactionDateWhenEmpty()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			CombineAssertions(() =>
			{
				var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();
				header.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);

				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("When we confirm the TemporaryStorageGoods the PhysicalInOutDate is the AcceptanceDate if ClearanceDate is empty", new ZDateTime(2023, 01, 01, 02, 01, 00), regLineTransaction.PhysicalInOutDate);
				AssertEquals("When we confirm the TemporaryStorageGoods the date TransactionDate the AcceptanceDate if ClearanceDate is empty", new ZDateTime(2023, 01, 01, 02, 01, 00), regLineTransaction.TransactionDate);
			});
		}
	}

	public void TestG5XOnCustomsStatusChangeToCLRUseClearanceDateforSRT_PhysicalInOutDateAndSRT_TransactionDate()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			CombineAssertions(() =>
			{
				var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();
				header.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
				header.ClearanceDate = new ZDateTime(2025, 01, 01, 02, 01, 00);

				header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

				AssertEquals("When we confirm the TemporaryStorageGoods the PhysicalInOutDate is the ClearanceDate if ClearanceDate is not empty", new ZDateTime(2025, 01, 01, 02, 01, 00), regLineTransaction.PhysicalInOutDate);
				AssertEquals("When we confirm the TemporaryStorageGoods the date TransactionDate the ClearanceDate if ClearanceDate is not empty", new ZDateTime(2025, 01, 01, 02, 01, 00), regLineTransaction.TransactionDate);
			});
		}
	}

	TemporaryStorageHeader SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(OrgAddress orgAddress, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry
		, bool shouldSetPreviousDocuments = false, string docReference = FormattedPrevDocReference)
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_JobReference = "TS00000001";
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		header.LRN = EntryReference;

		header.Bills.RemoveAndDeleteAll();
		var bill = header.Bills.AddNew();

		var packedItem = bill.PackedItems.AddNew();
		packedItem.API_GrossWeight = 11m;
		packedItem.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		var package = bill.Packs.AddNew();
		package.SetValues("CT", "marks", 9);
		var linkPackage = packedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package);
		linkPackage.IsLinked = true;

		if (shouldSetPreviousDocuments)
		{
			var previousDoc = packedItem.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = PrevDocCode;
			previousDoc.CSI_ReferenceNumber = docReference;
			previousDoc.CSI_LineNo = 1;
		}

		CusEntryNumber cusEntryNumber = CusEntryNumber.LoadOrCreate(header, "MRN", header.CountryCode);
		cusEntryNumber.CE_EntryIsSystemGenerated = true;
		cusEntryNumber.CE_EntryNum = MRNCode;
		cusEntryNumber.CE_IssueDate = issueDate;

		header.GoodsLocation.Address.AuthorisationNumber = locationInEntry;

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		Factory.Save();

		return header;
	}

	(TemporaryStorageHeader header, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine) SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction
		(string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, bool createTransaction = true, bool shouldSetPreviousDocuments = false
		, string regHeaderReference = FormattedPrevDocReference, string docReference = FormattedPrevDocReference)
	{
		var orgHeader = SetUpOrgHeader();
		var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, locationInEntry: locationInEntry, locationInPremises: locationInPremises, shouldSetPreviousDocuments: shouldSetPreviousDocuments, docReference: docReference);

		var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_CustomsStatus = "OPN";
		regLine.SRL_PackageType = "CT";
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction = (CusTempStorageRegLineTransaction)null;
		if (createTransaction)
		{
			regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		}

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot.SRV_SRL_Line = regLine.PK;

		SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		return (header, regLineTransaction, regLine);
	}

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString transactionStatus, int packageQty = 0, decimal grossWeight = 0, string transactionType = "TRN", decimal bondAmount = 0.0m)
	{
		var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = transactionType;
		regLineTransaction.SRT_TransactionStatus = transactionStatus;
		regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		regLineTransaction.SRT_PackageQty = packageQty;
		regLineTransaction.SRT_GrossWeight = grossWeight;

		if (transactionType == "OBL")
		{
			regLineTransaction.SRT_BondAmount = bondAmount;
		}
		else
		{
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
		}

		return regLineTransaction;
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", "AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
	}

	CusTempStorageRegHeader SetUpTmpRegHeader(string appCode = "AAA", string reference = FormattedPrevDocReference)
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = appCode;
		regHeader.SRH_Reference = reference;

		return regHeader;
	}

	EU.Business.CusGuaranteeHeader SetUpGauranteeForTempStorage(CusTempStorageRegHeader regHeader, ZDecimal value, OrgHeader orgHeader)
	{
		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
		cusGuarantee.CPH_Balance = 1000.0m;

		var commonGuarantee = Factory.New<EU.Business.Declaration.CommonGuarantee>();
		commonGuarantee.PW_BondNumber = "Test1";
		commonGuarantee.PW_ParentID = regHeader.PK;
		commonGuarantee.PW_ParentTableCode = CusBondDetailSchema.Constants.Prefix;
		commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		var guarantee = regHeader.Guarantee.CusGuarantee;
		var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
		guaranteeLineTransaction.CPL_Reference = FormattedPrevDocReference;
		guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		guaranteeLineTransaction.CPL_TranValue = value;

		guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		return guarantee;
	}

	CusTempStorageRegLine SetUpRegLine(CusTempStorageRegHeader regHeader)
	{
		var regLine = regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_PackageType = "BX";

		return regLine;
	}

	OrgHeader SetUpOrgHeader()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		return orgHeader;
	}

	#endregion

	#region IDocumentSupporter

	public void TestIsIDocumentSupportable()
	{
		var documentSupportable = header as IDocumentSupportable;

		CombineAssertions(() =>
		{
			AssertNotNull(nameof(IDocumentSupportable), documentSupportable);
			AssertEquals(nameof(IDocumentSupportable.TableName), "AsycudaManifestHeader", documentSupportable.TableName);
			AssertType<TemporaryStorageHeaderDocumentSupporter>("DocumentSupporter", documentSupportable.DocumentSupporter);
		});
	}

	#endregion

	void SetUpEntryData(ZString entryType)
	{
		var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
		entryNumber.CE_ParentID = header.PK;
		entryNumber.CE_ParentTable = header.TableName;
		entryNumber.CE_EntryType = entryType;
		entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
		entryNumber.CE_EntryNum = "12345678901234567890";
		entryNumber.CE_EntryStatus = "4";
		entryNumber.CE_IssueDate = ZDate.BrettsBirthday;
		Factory.Save();
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
	}
	TemporaryStorageHeader header;

	protected override Type ExpectedTypeOfContainer => typeof(ManifestBase.AsycudaContainerCollection<EU.Business.CusTempStorage.TemporaryStorageContainer, EU.Business.CusTempStorage.TemporaryStorageHeader>);

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		=> new TemporaryStorageHeaderLightValidationTester(bizObjToTest);

	sealed class TemporaryStorageHeaderLightValidationTester : LightValidationTester
	{
		public TemporaryStorageHeaderLightValidationTester(BusinessObject bo) : base(bo) { }

		protected override bool ShouldTestProperty(ZPropertyInfo info)
			=> base.ShouldTestProperty(info) && info.Name != ManifestBase.AutoAsycudaBill.Schema.ABL_RL_NKPortOfDischarge;
	}

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", caption, captionResourceString.Caption);
			AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
			AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
		});
	}

	public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
	{
		var result = base.GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues();
		result.Add(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_OA_Declarant);
		return result;
	}
}
