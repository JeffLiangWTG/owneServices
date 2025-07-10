using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeader))]
sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestTopLevelBusinessObject()
	{
		ITopLevelBusinessObjectProvider provider = departureMovement;
		AssertSame("When nctsHeader.Shipment is null", nctsHeader, provider.TopLevelBusinessObject);

		var shipment = Factory.New<ForwardingShipment>();
		nctsHeader.BH_ParentID = shipment.PK;
		nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
		AssertSame("When nctsHeader.Shipment is not null", shipment, provider.TopLevelBusinessObject);
	}

	public void TestMessages()
	{
		var messages = departureMovement.Messages;
		AssertEquals(departureMovement, messages.Master);
		AssertType<ITEDIMessageCollection>(messages);
	}

	public void TestUseElectronicFolderDefaultValue()
	{
		Assert("Default value", !departureMovement.UseElectronicFolder);
	}

	public void TestUseElectronicFolder()
	{
		departureMovement.UseElectronicFolder = true;
		Factory.Save();
		AssertUseElectronicFolderOnSeparateFactory(expectedUseElectronicFolder: true);

		departureMovement.UseElectronicFolder = false;
		Factory.Save();
		AssertUseElectronicFolderOnSeparateFactory(expectedUseElectronicFolder: false);

		void AssertUseElectronicFolderOnSeparateFactory(bool expectedUseElectronicFolder)
		{
			var separateFactory = new BusinessObjectFactory();
			var deparartureMovementOnSeparateFactory = separateFactory.Load<NctsDepartureMovementHeader>(departureMovement.PK);
			AssertEquals($"Expected {expectedUseElectronicFolder}", expectedUseElectronicFolder, deparartureMovementOnSeparateFactory.UseElectronicFolder);
		}
	}

	public void TestPaymentParty()
	{
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		Factory.Save();

		departureMovement = new BusinessObjectFactory().Load<NctsDepartureMovementHeader>(departureMovement.PK);
		AssertEquals("PaymentParty has been saved and loaded", NctsPaymentPartyList.Codes.ConsigneesAccount, departureMovement.PaymentParty);
	}

	public void TestPaymentPartyMaxLength()
	{
		AssertEquals("PaymentPartyInfo MaxLength should be", 1, departureMovement.PaymentPartyInfo.MaxLength);
	}

	public void TestDefermentAccountNumber()
	{
		departureMovement.DefermentAccountNumber = "1234567";
		Factory.Save();

		departureMovement = new BusinessObjectFactory().Load<NctsDepartureMovementHeader>(departureMovement.PK);
		AssertEquals("DefermentAccountNumber has been saved and loaded", "1234567", departureMovement.DefermentAccountNumber);
	}

	public void TestDefermentAccountNumberMaxLength()
	{
		AssertEquals("DefermentAccountNumber MaxLength should be", 35, departureMovement.DefermentAccountNumberInfo.MaxLength);
	}

	public void TestIsSimplifiedNctsProcedure_Phase5()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_ExportDate = new ZDateTime(2022, 11, 18);
		departureMovement.IsSimplifiedNctsProcedure = false;
		AssertEquals(new ZDateTime(2022, 11, 18), departureMovement.BM_ExportDate);
	}

	public void TestSetPaymentPartyTriggerDefermentAccountNumberDefaulting()
	{
		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		SimulatePaymentPartyValueChange(NctsPaymentPartyList.Codes.ConsigneesAccount);
		AssertEquals("Source trader is not set, defaulting is not triggered", ZString.Empty, departureMovement.DefermentAccountNumber);

		var requirement = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
		requirement.E2_OA_Address = consignee.MainAddress.PK;
		SimulatePaymentPartyValueChange(NctsPaymentPartyList.Codes.ConsigneesAccount);
		AssertEquals("Source trader has no customs codes, defaulting is not triggered", ZString.Empty, departureMovement.DefermentAccountNumber);

		consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1", Core.Constants.CountryCodes.Italy);
		var cusCodeDAT = consignee.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "2", Core.Constants.CountryCodes.Italy);
		requirement.E2_OA_Address = consignee.MainAddress.PK;
		SimulatePaymentPartyValueChange(NctsPaymentPartyList.Codes.ConsigneesAccount);
		AssertEquals("Source trader has 2 customs codes, defaulting is not triggered", ZString.Empty, departureMovement.DefermentAccountNumber);

		consignee.CustomsCodes.RemoveAndDelete(cusCodeDAT);
		SimulatePaymentPartyValueChange(NctsPaymentPartyList.Codes.ConsigneesAccount);
		AssertEquals("Source trader has 1 customs code, defaulting is triggered", "1", departureMovement.DefermentAccountNumber);

		void SimulatePaymentPartyValueChange(ZString valueToSet)
		{
			departureMovement.PaymentParty = ZString.Empty;
			departureMovement.PaymentParty = valueToSet;
			ClearConsigneeDefermentAccountNumberListCachedValue(consignee);
		}
	}

	public void TestClearPaymentPartyClearAlsoDefermentAccountNumber()
	{
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		departureMovement.DefermentAccountNumber = "1234567";
		AssertEquals("PRE-CONDITION", "1234567", departureMovement.DefermentAccountNumber);

		departureMovement.PaymentParty = ZString.Empty;
		AssertEquals("POST-CONDITION", ZString.Empty, departureMovement.DefermentAccountNumber);
	}

	public void TestResetOrDefaultDefermentAccountNumberIfSingleCustomsCodeFound()
	{
		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1", Core.Constants.CountryCodes.Italy);
		var cusCodeDAT = consignee.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "2", Core.Constants.CountryCodes.Italy);

		var requirement = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
		requirement.E2_OA_Address = consignee.MainAddress.PK;
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		departureMovement.DefermentAccountNumber = "AABBCCDD";
		departureMovement.ResetOrDefaultDefermentAccountNumberIfSingleCustomsCodeFound();
		AssertEquals("Source trader has 2 customs codes, reset is triggered", ZString.Empty, departureMovement.DefermentAccountNumber);

		consignee.CustomsCodes.RemoveAndDelete(cusCodeDAT);
		ClearConsigneeDefermentAccountNumberListCachedValue(consignee);
		departureMovement.ResetOrDefaultDefermentAccountNumberIfSingleCustomsCodeFound();
		AssertEquals("Source trader has 1 customs code, defaulting is triggered", "1", departureMovement.DefermentAccountNumber);

		consignee.CustomsCodes.RemoveAndDeleteAll();
		ClearConsigneeDefermentAccountNumberListCachedValue(consignee);
		departureMovement.DefermentAccountNumber = "AABBCCDD";
		departureMovement.ResetOrDefaultDefermentAccountNumberIfSingleCustomsCodeFound();
		AssertEquals("Source trader has no customs codes, reset is triggered", ZString.Empty, departureMovement.DefermentAccountNumber);
	}

	public void TestBM_LocationOfGoodsCode_Phase4Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_LocationOfGoodsCodeInfo, EU.NCTS.Business.NctsHeader.Phase4CaptionKey);
		AssertEquals("Caption", "[30] Goods Location", captionResourceString.Caption);
	}

	public void TestLookups_Phase4()
	{
		nctsHeader.BH_ApplicationCode = "NCT";

		CombineAssertions(() =>
		{
			AssertType<NctsDepartureMovementHeaderPhase4Lookups>("Lookups Type", departureMovement.Lookups);
			AssertType<NctsDepartureMovementHeaderPhase4Lookups>("ITLookups Type", departureMovement.ITLookups);
		});
	}

	public void TestLookups_Phase5()
	{
		nctsHeader.BH_ApplicationCode = "NC5";

		CombineAssertions(() =>
		{
			AssertType<NctsDepartureMovementHeaderPhase5Lookups>("Lookups Type", departureMovement.Lookups);
			AssertType<NctsDepartureMovementHeaderPhase5Lookups>("ITLookups Type", departureMovement.ITLookups);
		});
	}

	public void TestValidationType_Phase4()
	{
		nctsHeader.BH_ApplicationCode = "NCT";
		AssertType<NctsDepartureMovementHeaderPhase4Validation>("Validation type", departureMovement.Validation);
	}

	public void TestValidationType_Phase5()
	{
		nctsHeader.BH_ApplicationCode = "NC5";
		AssertType<NctsDepartureMovementHeaderPhase5Validation>("Validation type", departureMovement.Validation);
	}

	public void TestWarehouseCode()
	{
		var organization = Factory.New<OrgHeader>();

		departureMovement.BM_OA_WarehouseAddress = ZGuid.Empty;
		AssertEquals("No warehouse selected", ZString.Empty, departureMovement.WarehouseCode);

		departureMovement.BM_OA_WarehouseAddress = organization.MainAddress.PK;
		AssertEquals("Selected warehouse does not have a CCP customs code", ZString.Empty, departureMovement.WarehouseCode);

		organization.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "XXX", Core.Constants.CountryCodes.Italy);
		departureMovement.BM_OA_WarehouseAddress = organization.MainAddress.PK;
		AssertEquals("Selected warehouse has a CCP customs code", "XXX", departureMovement.WarehouseCode);

		organization.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "YYY", Core.Constants.CountryCodes.Italy);
		departureMovement.BM_OA_WarehouseAddress = organization.MainAddress.PK;
		AssertEquals("Edge case (system does not allow user to link more than one CCP code each address)", "XXX", departureMovement.WarehouseCode);
	}

	public void TestRepresentative()
	{
		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader1.Addresses.AddNew();

		departureMovement.BM_InBondEntryType = "T-";
		AssertEquals("When Departure Movement is not a TIR, Representative->ReadOnly", false, departureMovement.Representative.ReadOnly);
		departureMovement.Representative.OrganisationPK = orgHeader1.PK;

		departureMovement.BM_InBondEntryType = "TIR";
		AssertEquals("When Departure Movement has been changed to TIR, Representative", ZGuid.Empty, departureMovement.Representative.OrganisationPK);
		AssertEquals("When Departure Movement it TIR, Representative-> ReadOnly", true, departureMovement.Representative.ReadOnly);
	}

	public void TestBM_ExportDate()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_ExportDate = new ZDateTime(2022, 11, 18);
		departureMovement.IsSimplifiedNctsProcedure = false;
		departureMovement.BM_InBondEntryType = "T-";
		AssertEquals("BM_InBondEntryType != TIR, DateLimit is not empty", new ZDateTime(2022, 11, 18), departureMovement.BM_ExportDate);

		departureMovement.BM_InBondEntryType = "TIR";
		AssertEquals("BM_InBondEntryType = TIR, DateLimit is empty", ZDateTime.Empty, departureMovement.BM_ExportDate);
	}

	public void TestRepresentativeAdditionalValidation()
	{
		AssertType<RepresentativeJobDocAddressValidation>("Representative AdditionalValidation", departureMovement.Representative.AdditionalValidation);
	}

	public void TestHeader()
	{
		AssertType<NctsHeader>(nameof(departureMovement.Header), departureMovement.Header);
	}

	public void TestGoodsItems()
	{
		AssertType<NctsDepartureCargoDescCollection>(nameof(departureMovement.GoodsItems), departureMovement.GoodsItems);
	}

	public void TestIElectronicFolderSupporterMembers()
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		var electronicFolderSupporter = (IElectronicFolderSupporter)movementHeader;

		movementHeader.UseElectronicFolder = false;
		AssertEquals(nameof(electronicFolderSupporter.UseElectronicFolder), false, electronicFolderSupporter.UseElectronicFolder);

		movementHeader.UseElectronicFolder = true;
		AssertEquals(nameof(electronicFolderSupporter.UseElectronicFolder), true, electronicFolderSupporter.UseElectronicFolder);
	}

	public void TestBM_LocationOfGoodsCodeMaxLength()
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		AssertEquals("BM_LocationOfGoodsCode MaxLength", 7, movementHeader.BM_LocationOfGoodsCodeInfo.MaxLength);
	}

	public void TestSetPreLodgedForAgreedLocationOfGoodsCodeDoesNotChangeBM_LocationOfGoodsCode()
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		movementHeader.BM_LocationOfGoodsCode = "123456A";

		movementHeader.PreLodgedForAgreedLocationOfGoodsCode = true;
		AssertEquals("BM_LocationOfGoodsCode when ticking PreLodgedForAgreedLocationOfGoodsCode", "123456A", movementHeader.BM_LocationOfGoodsCode);

		movementHeader.PreLodgedForAgreedLocationOfGoodsCode = false;
		AssertEquals("BM_LocationOfGoodsCode when unticking PreLodgedForAgreedLocationOfGoodsCode", "123456A", movementHeader.BM_LocationOfGoodsCode);
	}

	public void TestPreLodgedForAgreedLocationOfGoodsCode()
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		AssertEquals("PreLodgedForAgreedLocationOfGoodsCode is not applicable for Italy", false, movementHeader.PreLodgedForAgreedLocationOfGoodsCode);
	}

	public void TestControlChannelReadOnly()
	{
		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		AssertEquals($"{nameof(NctsDepartureMovementHeader.BM_ControlChannel)} Read Only", true, movementHeader.BM_ControlChannelInfo.ReadOnly);
	}

	public void TestWarehouseAddressDefault()
	{
		var organization = Factory.New<OrgHeader>();
		AssertEquals("PRE-CONDITION: Addresses count", 1, organization.Addresses.Count);

		var movementHeader = Factory.NewDepartureNctsHeader().MovementHeader;
		movementHeader.BM_OA_WarehouseAddress_ZAddress.OrgPK = organization.PK;
		AssertEquals("When setting an organisation with 1 address, BM_OA_WarehouseAddress", organization.Addresses[0].PK, movementHeader.BM_OA_WarehouseAddress);

		organization.Addresses.AddNew();
		movementHeader.BM_OA_WarehouseAddress = ZGuid.Empty;
		movementHeader.BM_OA_WarehouseAddress_ZAddress.OrgPK = organization.PK;
		AssertEquals("When setting an organisation with more than 1 address, BM_OA_WarehouseAddress", ZGuid.Empty, movementHeader.BM_OA_WarehouseAddress);
	}

	public void TestSetBM_ExportDateOnlyConsiderDatePart()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_ExportDate = ZDateTime.Empty;
			AssertEquals(nameof(departureMovement.BM_ExportDate), ZDateTime.Empty, departureMovement.BM_ExportDate);

			departureMovement.BM_ExportDate = ZDateTime.Invalid;
			AssertEquals(nameof(departureMovement.BM_ExportDate), ZDateTime.Invalid, departureMovement.BM_ExportDate);

			departureMovement.BM_ExportDate = new ZDateTime(2021, 01, 01, 10, 22, 48);
			AssertEquals(nameof(departureMovement.BM_ExportDate), new ZDateTime(2021, 01, 01, 00, 00, 00), departureMovement.BM_ExportDate);
		});
	}

	public void TestTIRCarnetSupportingDocumentCode()
	{
		var departureMovementHeaderForTest = Factory.New<NctsDepartureMovementHeaderForTest>();
		AssertEquals("TIRCarnetSupportingDocumentCode", IT.Business.UniversalReferenceConstants.SupportingDocumentTypes.TIRCarnet, departureMovementHeaderForTest.TIRCarnetSupportingDocumentCodeExposed);
	}

	public void TestAddTIRCarnetSupportingDocument()
	{
		departureMovement.TirCarnetNumber = "ABC";
		var goodsItem = departureMovement.GoodsItems[0];
		var tirDocument = goodsItem.SupportingDocuments.Cast<NctsSupportingDocument>().Single(x => x.CSI_Code == IT.Business.UniversalReferenceConstants.SupportingDocumentTypes.TIRCarnet);
		AssertEquals("TIR document is added.", "ABC", tirDocument.CSI_ReferenceNumber);

		departureMovement.TirCarnetNumber = "DEF";
		AssertEquals("When TirCarnetNumber changes, the old TIR document is deleted.", true, tirDocument.IsDeleted);

		var tirDocument2 = goodsItem.SupportingDocuments.Cast<NctsSupportingDocument>().Single(x => x.CSI_Code == IT.Business.UniversalReferenceConstants.SupportingDocumentTypes.TIRCarnet);
		AssertEquals("A new TIR document is added.", "DEF", tirDocument2.CSI_ReferenceNumber);
	}

	public void TestBM_PlaceOfUnloadingMaxLength()
	{
		AssertEquals("BM_PlaceOfUnloading MaxLength", 35, departureMovement.BM_PlaceOfUnloadingInfo.MaxLength);
	}

	public void TestBM_ForeignDestPortKCodeCaptions()
	{
		var foreignDestPortKCodeStringdata = departureMovement.BM_ForeignDestPortKCodeInfo.GetAttribute<ResourceStringDataAttribute>();
		AssertEquals("Caption", "Place of Unloading", foreignDestPortKCodeStringdata.Caption);
	}

	public void TestSetBM_EntryDateOnlyConsiderDateTimePart()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_EntryDate = ZDateTime.Empty;
			AssertEquals(nameof(departureMovement.BM_EntryDate), ZDateTime.Empty, departureMovement.BM_EntryDate);

			departureMovement.BM_EntryDate = ZDateTime.Invalid;
			AssertEquals(nameof(departureMovement.BM_EntryDate), ZDateTime.Invalid, departureMovement.BM_EntryDate);

			departureMovement.BM_EntryDate = new ZDateTime(2021, 01, 01, 10, 22, 48);
			AssertEquals(nameof(departureMovement.BM_EntryDate), new ZDateTime(2021, 01, 01, 10, 22, 48), departureMovement.BM_EntryDate);
		});
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name != nameof(NctsDepartureMovementHeader.PreLodgedForAgreedLocationOfGoodsCode))
		{
			base.TestBizObjectField(info);
		}
	}

	public void TestParticipantTypeMaxLength()
	{
		AssertEquals("MaxLength", 3, departureMovement.ParticipantTypeInfo.MaxLength);
	}

	public void TestParticipantType()
	{
		CombineAssertions("Assert expected values on the participants type field", () =>
		{
			departureMovement.ParticipantType = "STD";
			AssertEquals("Value STD", "STD", departureMovement.ParticipantType);

			departureMovement.ParticipantType = "GRP";
			AssertEquals("Value GRP", "GRP", departureMovement.ParticipantType);

			departureMovement.ParticipantType = "XYZ";
			AssertEquals("Value XYZ", "XYZ", departureMovement.ParticipantType);
		});
	}

	public void TestSetDefaultValuesParticipantType()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var departureMovement = nctsHeader.MovementHeader;
		AssertEquals(nameof(departureMovement.ParticipantType), NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter, departureMovement.ParticipantType);
	}

	public void TestCloneInternal_CopyBM_EntryDate()
	{
		departureMovement.BM_EntryDate = ZDateTime.Now;
		var clonedDepartureMovement = (NctsDepartureMovementHeader)departureMovement.Clone();
		AssertEquals("BM_EntryDate should be empty", ZDateTime.Empty, clonedDepartureMovement.BM_EntryDate);
	}

	public void TestPlaceOfLoading()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var departureMovement = nctsHeader.MovementHeader;

		ZString countryString = "IT";
		departureMovement.BM_PlaceOfLoading = ZString.Empty;
		AssertEquals("PlaceOfLoadingCore should return IT", ZString.Empty, departureMovement.PlaceOfLoading);

		departureMovement.BM_PlaceOfLoading = countryString;
		AssertEquals("PlaceOfLoadingCore should return IT", countryString, departureMovement.PlaceOfLoading);
	}

	public void TestIsGroupage()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var departureMovement = nctsHeader.MovementHeader;

		departureMovement.ParticipantType = "";
		AssertEquals("When ParticipantType is empty, IsGroupage", false, departureMovement.IsGroupage);

		departureMovement.ParticipantType = NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter;
		AssertEquals("When ParticipantType is STD, IsGroupage", false, departureMovement.IsGroupage);

		departureMovement.ParticipantType = NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;
		AssertEquals("When ParticipantType is GRP, IsGroupage", true, departureMovement.IsGroupage);
	}

	public void TestBM_ExportTransportModeCaptions_Phase4()
	{
		var exportTransportModeResStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_ExportTransportModeInfo, NctsHeader.Phase4CaptionKey);

		CombineAssertions(() =>
		{
			AssertEquals("Caption", "[21] Transport Mode (Frontier)", exportTransportModeResStringData.Caption);
			AssertEquals("ShortCaption", "Frontier Transp. Mode", exportTransportModeResStringData.ShortCaption);
			AssertEquals("FullDescription", "Frontier Mode of Transport (Code)", exportTransportModeResStringData.FullDescription);
		});
	}

	public void TestBM_ExportTransportModeCaptions_Phase5()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_ExportTransportModeInfo, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Border Method of Transport", captionResourceString.Caption);
		AssertEquals("ShortCaption", "M.O.T.", captionResourceString.ShortCaption);
		AssertEquals("MediumCaption", "Border M.O.T.", captionResourceString.MediumCaption);
	}

	public void TestBM_PlaceOfLoadingMaxLength()
	{
		AssertEquals("BM_PlaceOfLoading MaxLength", 35, departureMovement.BM_PlaceOfLoadingInfo.MaxLength);
	}

	public void TestPayInfoCollection()
	{
		AssertType<NctsDeparturePayInfoCollection>("PayInfoCollection Type", departureMovement.PayInfoCollection);
	}

	public void TestPayInfoType()
	{
		AssertEquals("PayInfoType", typeof(NctsDeparturePayInfo), departureMovement.PayInfoType);
	}

	public void TestGoodsLocation()
	{
		AssertNotNull("GoodsLocation", departureMovement.GoodsLocation);
	}

	public void TestClearGoodsLocation()
	{
		var goodsLocation = departureMovement.GoodsLocation;
		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_Type = "B";
		goodsLocation.CGL_CustomsOffice = "ABC";

		ICusGoodsLocationProvider goodsLocationProvider = departureMovement;
		goodsLocationProvider.ClearGoodsLocation();

		CombineAssertions("When Goods Location cleared", () =>
		{
			AssertEquals("", goodsLocation.CGL_Qualifier);
			AssertEquals("", goodsLocation.CGL_Type);
			AssertEquals("", goodsLocation.CGL_CustomsOffice);
		});
	}

	public void TestBM_PresentationDateTime_Caption_Phase5()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_PresentationDateTimeInfo, EU.NCTS.Business.NctsHeader.Phase5CaptionKey);

		Assertion.AssertEquals("Caption", "Date and Time of presentation", captionResourceString.Caption);
		Assertion.AssertEquals("ShortCaption", "Pres. Date", captionResourceString.ShortCaption);
		Assertion.AssertEquals("MediumCaption", "Presentation Date", captionResourceString.MediumCaption);
		Assertion.AssertEquals("FullDescription", "Date and Time of presentation of the goods", captionResourceString.FullDescription);
	}

	public void TestBM_TOLCarrierIDCaptions_Phase5()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Transport Identification", captionResourceString.Caption);
		AssertEquals("ShortCaption", "Transp. ID", captionResourceString.ShortCaption);
		AssertEquals("MediumCaption", "Transport ID", captionResourceString.MediumCaption);
	}

	public void TestBM_CustomsOfficeAtBorderCaptions_Phase5()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_CustomsOfficeAtBorderInfo, NctsHeader.Phase5CaptionKey);

		AssertEquals("Caption", "Customs Office", captionResourceString.Caption);
		AssertEquals("ShortCaption", "Office", captionResourceString.ShortCaption);
	}

	public void TestTypeOfAdditionalTransportAtBorderList()
	{
		AssertType(typeof(DepartureCusTransportMeansCollection<DepartureCusTransportMeans>), departureMovement.AdditionalTransportAtBorderList);
	}

	public void TestBillsDepartureTransportMeansWiper()
	{
		var billsDepartureTransportMeansWiper = departureMovement.BillsDepartureTransportMeansWiper;
		AssertType<NctsBillsDepartureTransportMeansWiper>("Type", billsDepartureTransportMeansWiper);
		AssertSame("BillsDepartureTransportMeansWiper Cached", billsDepartureTransportMeansWiper, departureMovement.BillsDepartureTransportMeansWiper);
	}

	public void TestGetDepartureTransportMeansProperties()
	{
		AssertArrayEqualsByElements(
			"DepartureTransportMeansProperties",
			new ZPropertyInfo[]
			{
				departureMovement.BM_TransportAtDepartureTypeInfo,
				departureMovement.BM_TransportAtDepartureInfo,
				departureMovement.BM_RN_NKTransportAtDepartureCountryInfo,
				departureMovement.BM_AircraftIDAtDepartureInfo,
				departureMovement.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo,
				departureMovement.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo,
				departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo,
				departureMovement.BM_TransportAtDepartureTrailer2RegNoInfo
			},
			departureMovement.GetDepartureTransportMeansProperties().ToArray());
	}

	public void TestBM_ExportTransportModeAsNumber()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_ExportTransportMode = "12";
			AssertEquals("BM_ExportTransportMode is 12", 12, departureMovement.BM_ExportTransportModeAsNumber);

			departureMovement.BM_ExportTransportMode = "1A";
			AssertEquals("BM_ExportTransportMode is 1A", null, departureMovement.BM_ExportTransportModeAsNumber);

			departureMovement.BM_ExportTransportMode = "";
			AssertEquals("BM_ExportTransportMode is empty", null, departureMovement.BM_ExportTransportModeAsNumber);
		});
	}

	public void TestBM_PaperlessInbondNumNotReadOnlyForPhase5Departure()
	{
		nctsHeader.BH_ApplicationCode = "NC5";
		AssertEquals("BM_PaperlessInbondNum ReadOnly", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);
	}

	public void TestShouldGenerateLocalReferenceNumberOnSaving_OptOutForPhase5Departure()
	{
		nctsHeader.BH_ApplicationCode = "NC5";
		AssertEquals("ShouldGenerateLocalReferenceNumberOnSaving", false, departureMovement.ShouldGenerateLocalReferenceNumberOnSaving);
	}

	public void TestBM_PaperlessInbondNum_EmptyOnSave()
	{
		nctsHeader.Branch.OrgProxy.CustomsCodes.AddNew("EOR", "1234");
		nctsHeader.BH_ApplicationCode = "NC5";
		Factory.Save();
		AssertEquals("BM_PaperlessInbondNum after save", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
	}

	public void TestIsInAmendmentPhase()
	{
		AssertNotEquals("[PRE-CONDITION], BM_Phase", "013", departureMovement.BM_Phase);
		AssertEquals("When Phase is not 013", expected: false, departureMovement.IsInAmendmentPhase);

		departureMovement.BM_Phase = "013";
		AssertEquals("When Phase is 013", expected: true, departureMovement.IsInAmendmentPhase);
	}

	public void TestGuarantees()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsGuaranteeCollection<NctsGuarantee>>(nctsHeader.MovementHeader.Guarantees);
	}

	public void TestICustomsLinkedObjectAdapterProviderMembers()
	{
		var customsLinkedObjectAdapterProvider = departureMovement as ICustomsLinkedObjectAdapterProvider;

		AssertNotNull("NctsDepartureMovementHeader must implement ICustomsLinkedObjectAdapterProvider", customsLinkedObjectAdapterProvider);
		CombineAssertions("Assert ICustomsLinkedObjectAdapterProviderMembers members", () =>
		{
			AssertExceptionThrown<NotSupportedException>("GetSadCustomsLinkedObjectAdapter", () => customsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter());
			AssertExceptionThrown<NotSupportedException>("GetNewSingleWindowCustomsLinkedObjectAdapter", () => customsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter());
			AssertType<NctsHeaderPhase5CustomsLinkedObjectAdapter>("GetNewXmlCustomsLinkedObjectAdapter", customsLinkedObjectAdapterProvider.GetNewXmlCustomsLinkedObjectAdapter());
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => departureMovement;

	protected override BusinessObject GetNewBusinessObject() => departureMovement;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		departureMovement = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;

	#region Implementation

	void ClearConsigneeDefermentAccountNumberListCachedValue(OrgHeader orgHeader)
	{
		Factory.ClearCachedValue<CodeDescriptionPairList>($"{orgHeader.PK}_Consignee");
	}

	#endregion

	#region NctsDepartureMovementHeaderForTest

	class NctsDepartureMovementHeaderForTest : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString TIRCarnetSupportingDocumentCodeExposed => TIRCarnetSupportingDocumentCode;
	}

	#endregion
}
