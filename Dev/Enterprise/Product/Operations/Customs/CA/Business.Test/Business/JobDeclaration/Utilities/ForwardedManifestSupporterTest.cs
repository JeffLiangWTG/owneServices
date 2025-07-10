using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ForwardedManifestSupporterTest : TestCaseWithFactory
	{
		public void TestCreateJobDeclarationMatchingOnCCN()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+130828:2051+2678++++++1'
UNG+GOVCBR+CCR+U10207V1+20130828:2051+13+UN+D:11B'
UNH+1+GOVCBR:D:11B:UN'
BGM+714+8036X555+4'
RFF+AFM:10207:CB'
RFF+UCN:UCR555'
DOC+23+:24'
DOC+85+9165XXX4444'
RCS+15'
FTX+ACB+++SOME B 2 B COMMENTS'
TDT+11++1'
UNS+D'
HYN+3'
CNI+1'
STS++0'
MEA+AAX++MTQ:6'
HAN+:::SOME HANDLEING INSTRUCTIONS'
NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA'
CTA+AH'
NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU'
CTA+IC+:FRED NERCK'
CTA+AH'
COM+61290251100:TE'
NAD+DP+++DELIVERY PARTY NAME+DELIVERY PARTY ADDRESS+VICTORIA+BC+L0J 1C0+CA'
CTA+IC+:CONTACT NAME'
NAD+NI+++ABC CANADA+111 HURONTARIO STREET+TORONTO+ON+M5P 1A2+CA'
CTA+IC+:FRED'
CTA+AH'
COM+1 (905) 555-1247:TE'
NAD+NI+++A SECOND NOTIFY PARTY+NOTIFY ADDRESS+MASCOT+NSW+2020+AU'
NAD+ZZZ+++PLACE OF CONSOLIDATION+POC ADDRESS LINE+MASCOT+NSW+2020+AU'
CTA+AH'
NAD+PK+++FRED WIDGET'
CTA+AH'
LOC+8+0809+3380'
LOC+11+0495+3559'
DOC+714'
NAD+CS+++ABC FREIGHT FORWARDERS+6015 BOTANY ROAD+BANKSMEADOW+NSW+2019+AU'
RCS+15'
FTX+AAC+++SOME DG INSTRUCTIONS'
EQD+CN+OCLU1231230'
SEQ+4'
SEL+111'
SEQ+4'
SEL+222'
SEQ+4'
SEL+333'
SEQ+4'
EQD+CN+OCLU2342346'
SEQ+4'
SEL+444'
SEQ+4'
TDT+1'
SEQ+4'
PAC+100++:::PKG'
SEQ+4'
PCI++SOME MARKS'
GID+1'
FTX+AAA+++PACKAGED STUFF'
TCC+++7326.90.90:SRZ'
SEQ+4'
PAC+10++:::BOX'
SEQ+4'
GID+1'
FTX+AAA+++BOXES OF DG THINGOES'
TCC+++1201:SSC'
TCC+++1402:SSC'
UNS+S'
CNT+7:2:TNE'
UNT+69+1'
UNE+1+13'
UNZ+1+2678'";

			#endregion

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "VANCOUVER IMPORT/ EXPORT COMPANY";
			importer.MainAddress.OA_Address1 = "99 MAIN ST";
			importer.MainAddress.OA_City = "VANCOUVER";
			importer.MainAddress.OA_State = "BC";
			importer.MainAddress.OA_PostCode = "V6B3G2";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";

			var interchange = Enterprise.Messaging.Business.EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString.Replace("\r\n", ""),
				Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var tmpMessage = interchange.ContainedMessages[0];
			tmpMessage.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			Factory.NewWithPrimaryKey<EDIMessage>(new Guid("f0d4ec86-9e4d-4a51-aae1-b840ad6a53ec")).CopyPersistentValuesFrom(tmpMessage);
			tmpMessage.Delete();
			interchange.ContainedMessages.Load();
			var message = interchange.ContainedMessages[0] as ACIHouseBillMessage;
			AssertNotNull(message);
			message.PrimaryCCN = "9165XXX4444";
			Factory.Save();

			var supporter = new ForwardedManifestSupporter(message);
			var declaration = supporter.FindOrCreateJobDeclarationMatchingOnCCN();
			AssertNotNull(declaration);
			AssertEquals("Import", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("HBL", "X555", declaration.JE_HouseBill);
			var number = CusEntryNumber.Load(declaration, CanadaAdditionalReferenceNumberTypes.Codes.CCN, declaration.CountryCode);
			AssertNotNull(number);
			AssertEquals("CCN", "8036X555", number.CE_EntryNum);
			number = CusEntryNumber.Load(declaration, CanadaAdditionalReferenceNumberTypes.Codes.PCN, declaration.CountryCode);
			AssertNotNull(number);
			AssertEquals("PCN", "9165XXX4444", number.CE_EntryNum);
			number = CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.UniqueConsignementReference, declaration.CountryCode);
			AssertNotNull(number);
			AssertEquals("UCR", "UCR555", number.CE_EntryNum);
			var notes = declaration.Notes.FindByDescription("Forwarded Manifest B2B Notes");
			AssertEquals("Just one note", 1, notes.Length);
			AssertEquals("B2B Note", "SOME B 2 B COMMENTS", notes[0].ST_NoteDataAsText);
			AssertEquals("Transport Mode", "SEA", declaration.JE_TransportMode);
			AssertEquals("Volume", 6m, declaration.JE_TotalVolume);
			AssertEquals("Volume Units", "M3", declaration.JE_TotalVolumeUnit);
			AssertEquals("Gross Weight", 2m, declaration.JE_TotalWeight);
			AssertEquals("Gross Weight Units", "T", declaration.JE_TotalWeightUnit);
			notes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Just one note", 1, notes.Length);
			AssertEquals("HANDLEING INSTRUCTIONS", "SOME HANDLEING INSTRUCTIONS", notes[0].ST_NoteDataAsText);
			AssertNotNull(declaration.Importer);
			AssertEquals("Importer", importer.PK, declaration.Importer.PK);
			AssertEquals("Importer Name", "VANCOUVER IMPORT/ EXPORT COMPANY", declaration.Importer.OH_FullName);
			AssertNull(declaration.Supplier);
			notes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Just one note", 1, notes.Length);
			var noteBuilder = new ZStringBuilder("Unmatched Organization for Shipper");
			noteBuilder.Append("TREETOYS PTY LTD");
			noteBuilder.Append("105 WOMBAT DRIVE");
			noteBuilder.Append("KATOOMBA");
			noteBuilder.Append("NSW 2780");
			noteBuilder.Append("AU");
			noteBuilder.Append("FRED NERCK");
			noteBuilder.Append("61290251100");
			AssertEquals("UNMATCHED VENDOR", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[0].ST_NoteDataAsText);
			AssertEquals("Delivery Party", "DELIVERY PARTY NAME", declaration.ImporterDeliveryAddress.E2_CompanyName);
			AssertEquals("Delivery Party", "DELIVERY PARTY NAME DELIVERY PARTY ADDRESS VICTORIA BC L0J 1C0", declaration.ImporterDeliveryAddress.AddressAsASingleLine);
			AssertEquals("Delivery Party", "CA", declaration.ImporterDeliveryAddress.E2_RN_NKCountryCode);
			AssertEquals("Delivery Party", "CONTACT NAME", declaration.ImporterDeliveryAddress.E2_Contact);
			notes = declaration.Notes.FindByDescription("Forwarded Manifest Address");
			AssertEquals("5 notes", 5, notes.Length);
			noteBuilder = new ZStringBuilder("Name/Address for Notify Party");
			noteBuilder.Append("ABC CANADA");
			noteBuilder.Append("111 HURONTARIO STREET");
			noteBuilder.Append("TORONTO");
			noteBuilder.Append("ON M5P 1A2");
			noteBuilder.Append("CA");
			noteBuilder.Append("FRED");
			noteBuilder.Append("1 (905) 555-1247");
			AssertEquals("NOTIFY PARTY1", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[0].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for Notify Party");
			noteBuilder.Append("A SECOND NOTIFY PARTY");
			noteBuilder.Append("NOTIFY ADDRESS");
			noteBuilder.Append("MASCOT");
			noteBuilder.Append("NSW 2020");
			noteBuilder.Append("AU");
			AssertEquals("NOTIFY PARTY2", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[1].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for Place of Consolidation");
			noteBuilder.Append("PLACE OF CONSOLIDATION");
			noteBuilder.Append("POC ADDRESS LINE");
			noteBuilder.Append("MASCOT");
			noteBuilder.Append("NSW 2020");
			noteBuilder.Append("AU");
			AssertEquals("PLACE OF CONSOLIDATION", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[2].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for UNDG Contact");
			noteBuilder.Append("FRED WIDGET");
			noteBuilder.Append(" ");
			AssertEquals("UNDG CONTACT", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[3].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for Consolidator");
			noteBuilder.Append("ABC FREIGHT FORWARDERS");
			noteBuilder.Append("6015 BOTANY ROAD");
			noteBuilder.Append("BANKSMEADOW");
			noteBuilder.Append("NSW 2019");
			noteBuilder.Append("AU");
			AssertEquals("CONSOLIDATOR", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[4].ST_NoteDataAsText);
			AssertEquals("PORT OF CLEARANCE", "0809", declaration.JE_CustomsOffice);
			AssertEquals("SUB-LOCATION", "3380", declaration.JE_LocationOfGoods);
			AssertEquals("PORT OF DISCHARGE", "0495", declaration.CA_UnladingOffice);
			notes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description);
			AssertEquals("Just one note", 1, notes.Length);
			AssertEquals("DG Instructions", "SOME DG INSTRUCTIONS", notes[0].ST_NoteDataAsText);
			AssertEquals("2 containers", 2, declaration.CusContainers.Count);
			AssertEquals("First continer num", "OCLU1231230", declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("First continer seal", "111", declaration.CusContainers[0].CO_Seal);
			AssertEquals("First continer seal2", "222", declaration.CusContainers[0].CO_SecondSeal);
			AssertEquals("2nd continer num", "OCLU2342346", declaration.CusContainers[1].CO_ContainerNumber);
			AssertEquals("2nd continer seal", "444", declaration.CusContainers[1].CO_Seal);
			AssertEquals("2nd continer seal2", ZString.Empty, declaration.CusContainers[1].CO_SecondSeal);
			AssertEquals("Goods description", "PACKAGED STUFF", declaration.JE_GoodsDescription);
			AssertEquals("Declaration packages", 100, declaration.JE_TotalNoOfPacks);
			AssertEquals("Declaration package type", "PKG", declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("3 pack lines, 2 containers + 2nd pack line", 3, declaration.Packages.Count);
			var packLine = declaration.Packages[0];
			AssertEquals("1st pack line ctn", "OCLU1231230", packLine.CW_ContainerNoOrEquipmentNo);
			AssertEquals("1st pack line qty", 100, packLine.CW_PackQty);
			AssertEquals("1st pack line typ", "PKG", packLine.CW_PackType);
			packLine = declaration.Packages[2];
			AssertEquals("2nd pack line ctn", "OCLU1231230", packLine.CW_ContainerNoOrEquipmentNo);
			AssertEquals("2nd pack line qty", 10, packLine.CW_PackQty);
			AssertEquals("2nd pack line typ", "BOX", packLine.CW_PackType);
			notes = declaration.Notes.FindByDescription("Forwarded Manifest Other Information");
			AssertEquals("Just one note", 1, notes.Length);
			noteBuilder = new ZStringBuilder("Sub-location where discharged: 3559");
			noteBuilder.Append("Additional Seal: 333 on container: OCLU1231230");
			noteBuilder.Append("Goods Line 1 HS Code: 7326.90.90");
			noteBuilder.Append("Goods Line 1 Marks & Numbers: SOME MARKS");
			noteBuilder.Append("Goods Line 2 description of goods: BOXES OF DG THINGOES");
			noteBuilder.Append("Goods Line 2 UNDG Code: 1201");
			noteBuilder.Append("Goods Line 2 UNDG Code: 1402");
			AssertEquals("Other Information", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[0].ST_NoteDataAsText);
		}

		public void TestCreateJobDeclarationMatchingOnCCNWithMinimalMessage()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+130828:2051+2678++++++1'
UNG+GOVCBR+CCR+U10207V1+20130828:2051+13+UN+D:11B'
UNH+1+GOVCBR:D:11B:UN'
BGM+714+8036X555+4'
UNT+69+1'
UNE+1+13'
UNZ+1+2678'";

			#endregion

			var interchange = Enterprise.Messaging.Business.EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString.Replace("\r\n", ""),
				Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var tmpMessage = interchange.ContainedMessages[0];
			tmpMessage.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			Factory.NewWithPrimaryKey<EDIMessage>(new Guid("f0d4ec86-9e4d-4a51-aae1-b840ad6a53ec")).CopyPersistentValuesFrom(tmpMessage);
			tmpMessage.Delete();
			interchange.ContainedMessages.Load();
			var message = interchange.ContainedMessages[0] as ACIHouseBillMessage;
			AssertNotNull(message);
			Factory.Save();

			var supporter = new ForwardedManifestSupporter(message);
			var declaration = supporter.FindOrCreateJobDeclarationMatchingOnCCN();
			AssertNotNull(declaration);
		}

		public void TestAddNewShipmentToLoadList()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+130828:2051+2678++++++1'
UNG+GOVCBR+CCR+U10207V1+20130828:2051+13+UN+D:11B'
UNH+1+GOVCBR:D:11B:UN'
BGM+714+8036X555+4'
RFF+AFM:10207:WH'
RFF+UCN:UCR555'
DOC+23+:24'
DOC+85+9165XXX4444'
RCS+15'
FTX+ACB+++SOME B 2 B COMMENTS'
TDT+11++1'
UNS+D'
HYN+3'
CNI+1'
STS++0'
MEA+AAX++MTQ:6'
HAN+:::SOME HANDLEING INSTRUCTIONS'
NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA'
CTA+AH'
NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU'
CTA+IC+:FRED NERCK'
CTA+AH'
COM+61290251100:TE'
NAD+DP+++DELIVERY PARTY NAME+DELIVERY PARTY ADDRESS+VICTORIA+BC+L0J 1C0+CA'
CTA+IC+:CONTACT NAME'
NAD+NI+++ABC CANADA+111 HURONTARIO STREET+TORONTO+ON+M5P 1A2+CA'
CTA+IC+:FRED'
CTA+AH'
COM+1 (905) 555-1247:TE'
NAD+NI+++A SECOND NOTIFY PARTY+NOTIFY ADDRESS+MASCOT+NSW+2020+AU'
NAD+ZZZ+++PLACE OF CONSOLIDATION+POC ADDRESS LINE+MASCOT+NSW+2020+AU'
NAD+FW+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA'
CTA+AH'
NAD+PK+++FRED WIDGET'
CTA+AH'
LOC+8+0809+3380'
LOC+11+0495+3559'
DOC+714'
NAD+CS+++ABC FREIGHT FORWARDERS+6015 BOTANY ROAD+BANKSMEADOW+NSW+2019+AU'
RCS+15'
FTX+AAC+++SOME DG INSTRUCTIONS'
EQD+CN+OCLU1231230'
SEQ+4'
SEL+111'
SEQ+4'
SEL+222'
SEQ+4'
SEL+333'
SEQ+4'
EQD+CN+OCLU2342346'
SEQ+4'
SEL+444'
SEQ+4'
TDT+1'
SEQ+4'
PAC+100++:::PKG'
SEQ+4'
PCI++SOME MARKS'
GID+1'
FTX+AAA+++PACKAGED STUFF'
TCC+++7326.90.90:SRZ'
SEQ+4'
PAC+10++:::BOX'
SEQ+4'
GID+1'
FTX+AAA+++BOXES OF DG THINGOES'
TCC+++1201:SSC'
TCC+++1402:SSC'
UNS+S'
CNT+7:2:TNE'
UNT+69+1'
UNE+1+13'
UNZ+1+2678'";

			#endregion

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "VANCOUVER IMPORT/ EXPORT COMPANY";
			consignee.MainAddress.OA_Address1 = "99 MAIN ST";
			consignee.MainAddress.OA_City = "VANCOUVER";
			consignee.MainAddress.OA_State = "BC";
			consignee.MainAddress.OA_PostCode = "V6B3G2";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";

			var interchange = Enterprise.Messaging.Business.EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString.Replace("\r\n", ""),
				Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var tmpMessage = interchange.ContainedMessages[0];
			tmpMessage.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			Factory.NewWithPrimaryKey<EDIMessage>(new Guid("f0d4ec86-9e4d-4a51-aae1-b840ad6a53ec")).CopyPersistentValuesFrom(tmpMessage);
			tmpMessage.Delete();
			interchange.ContainedMessages.Load();
			var message = interchange.ContainedMessages[0] as ACIHouseBillMessage;
			AssertNotNull(message);

			var loadList = Factory.New<CFSLoadListConsol>();
			var container = loadList.Containers.AddNew();
			container.JC_ContainerNum = "OCLU2342346";
			var warehouse = CACSubLocationTest.CreateSubLocation(Factory, "3559").CusCodeList;

			var loco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_LocalPortCode = "0809";
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Oth;
			message.PrimaryCCN = "9165XXX4444";
			Factory.Save();

			var supporter = new ForwardedManifestSupporter(message);
			var shipment = supporter.AddNewShipmentToLoadList(loadList);

			AssertCollectionContains("New shipment was added to Load List", shipment, loadList.Shipments);

			AssertNotNull(shipment.ConsigneeDocumentaryAddress);
			AssertEquals("ConsigneeDocumentaryAddress", consignee.PK, shipment.ConsigneeDocumentaryAddress.OrganisationPK);
			AssertEquals("ConsigneeDocumentaryAddress CompanyName", "VANCOUVER IMPORT/ EXPORT COMPANY", shipment.ConsigneeDocumentaryAddress.E2_CompanyName);

			Assert("ConsignorDocumentaryAddress", shipment.ConsignorDocumentaryAddress.OrganisationPK.IsEmpty);

			AssertNotNull(shipment.ConsigneeDeliveryAddress);
			AssertEquals("Delivery Party", "DELIVERY PARTY NAME", shipment.ConsigneeDeliveryAddress.E2_CompanyName);
			AssertEquals("Delivery Party", "DELIVERY PARTY NAME DELIVERY PARTY ADDRESS VICTORIA BC L0J 1C0", shipment.ConsigneeDeliveryAddress.AddressAsASingleLine);
			AssertEquals("Delivery Party", "CA", shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode);
			AssertEquals("Delivery Party", "CONTACT NAME", shipment.ConsigneeDeliveryAddress.E2_Contact);

			AssertEquals("JS_OH_HandledOnBehalfOfForwarder", consignee.PK, shipment.JS_OH_HandledOnBehalfOfForwarder);

			var notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Just one note", 1, notes.Length);
			var noteBuilder = new ZStringBuilder("Unmatched Organization for Shipper");
			noteBuilder.Append("TREETOYS PTY LTD");
			noteBuilder.Append("105 WOMBAT DRIVE");
			noteBuilder.Append("KATOOMBA");
			noteBuilder.Append("NSW 2780");
			noteBuilder.Append("AU");
			noteBuilder.Append("FRED NERCK");
			noteBuilder.Append("61290251100");
			AssertEquals("UNMATCHED VENDOR", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[0].ST_NoteDataAsText);

			notes = shipment.Notes.FindByDescription("Forwarded Manifest Address");
			AssertEquals("5 notes", 5, notes.Length);
			noteBuilder = new ZStringBuilder("Name/Address for Notify Party");
			noteBuilder.Append("ABC CANADA");
			noteBuilder.Append("111 HURONTARIO STREET");
			noteBuilder.Append("TORONTO");
			noteBuilder.Append("ON M5P 1A2");
			noteBuilder.Append("CA");
			noteBuilder.Append("FRED");
			noteBuilder.Append("1 (905) 555-1247");
			AssertEquals("NOTIFY PARTY1", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[0].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for Notify Party");
			noteBuilder.Append("A SECOND NOTIFY PARTY");
			noteBuilder.Append("NOTIFY ADDRESS");
			noteBuilder.Append("MASCOT");
			noteBuilder.Append("NSW 2020");
			noteBuilder.Append("AU");
			AssertEquals("NOTIFY PARTY2", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[1].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for Place of Consolidation");
			noteBuilder.Append("PLACE OF CONSOLIDATION");
			noteBuilder.Append("POC ADDRESS LINE");
			noteBuilder.Append("MASCOT");
			noteBuilder.Append("NSW 2020");
			noteBuilder.Append("AU");
			AssertEquals("PLACE OF CONSOLIDATION", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[2].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for UNDG Contact");
			noteBuilder.Append("FRED WIDGET");
			noteBuilder.Append(" ");
			AssertEquals("UNDG CONTACT", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[3].ST_NoteDataAsText);
			noteBuilder = new ZStringBuilder("Name/Address for Consolidator");
			noteBuilder.Append("ABC FREIGHT FORWARDERS");
			noteBuilder.Append("6015 BOTANY ROAD");
			noteBuilder.Append("BANKSMEADOW");
			noteBuilder.Append("NSW 2019");
			noteBuilder.Append("AU");
			AssertEquals("CONSOLIDATOR", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[4].ST_NoteDataAsText);

			var number = CusEntryNumber.Load(shipment, CanadaAdditionalReferenceNumberTypes.Codes.CCN, Constants.CountryCodes.Canada);
			AssertNotNull(number);
			AssertEquals("CCN", "8036X555", number.CE_EntryNum);
			number = CusEntryNumber.Load(shipment, CanadaAdditionalReferenceNumberTypes.Codes.PCN, Constants.CountryCodes.Canada);
			AssertNotNull(number);
			AssertEquals("PCN", "9165XXX4444", number.CE_EntryNum);
			number = CusEntryNumber.Load(shipment, CusEntryNumberTypes.Standard.UniqueConsignementReference, Constants.CountryCodes.Canada);
			AssertNotNull(number);
			AssertEquals("UCR", "UCR555", number.CE_EntryNum);

			AssertEquals("Shipment Type", Constants.ShipmentTypes.StandardHouse, shipment.JS_ShipmentType);
			AssertEquals("House Bill", "X555", shipment.JS_HouseBill);
			AssertEquals("Transport Mode", "SEA", shipment.JS_TransportMode);

			notes = shipment.Notes.FindByDescription("Forwarded Manifest B2B Notes");
			AssertEquals("Just one note", 1, notes.Length);
			AssertEquals("B2B Note", "SOME B 2 B COMMENTS", notes[0].ST_NoteDataAsText);

			notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Just one note", 1, notes.Length);
			AssertEquals("HANDLEING INSTRUCTIONS", "SOME HANDLEING INSTRUCTIONS", notes[0].ST_NoteDataAsText);

			notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description);
			AssertEquals("Just one note", 1, notes.Length);
			AssertEquals("DG Instructions", "SOME DG INSTRUCTIONS", notes[0].ST_NoteDataAsText);

			AssertEquals("Declaration package type", loco.RL_Code, shipment.JS_RL_NKDestination);
			AssertEquals("Release warehouse", warehouse.PK, shipment.LocationWhsGuid);

			AssertEquals("Volume", 6m, shipment.JS_ActualVolume);
			AssertEquals("Volume Units", "M3", shipment.JS_UnitOfVolume);
			AssertEquals("Gross Weight", 2m, shipment.JS_ActualWeight);
			AssertEquals("Gross Weight Units", "T", shipment.JS_UnitOfWeight);
			AssertEquals("Goods description", "PACKAGED STUFF", shipment.JS_GoodsDescription);
			AssertEquals("OuterPacks", 100, shipment.JS_OuterPacks);
			AssertEquals("Package type", "PKG", shipment.JS_F3_NKPackType);
			AssertEquals("Marks And Numbers", "SOME MARKS", shipment.JS_MarksAndNumbers);

			AssertEquals("Volume", 6m, shipment.DefaultPackLine.JL_ActualVolume);
			AssertEquals("Volume Units", "M3", shipment.DefaultPackLine.JL_ActualVolumeUQ);
			AssertEquals("Gross Weight", 2m, shipment.DefaultPackLine.JL_ActualWeight);
			AssertEquals("Gross Weight Units", "T", shipment.DefaultPackLine.JL_ActualWeightUQ);
			AssertEquals("Goods description", "PACKAGED STUFF", shipment.DefaultPackLine.JL_Description);
			AssertEquals("OuterPacks", 100, shipment.DefaultPackLine.JL_PackageCount);
			AssertEquals("Package type", "PKG", shipment.DefaultPackLine.JL_F3_NKPackType);
			AssertEquals("Marks And Numbers", "SOME MARKS", shipment.DefaultPackLine.JL_MarksAndNumbers);

			AssertEquals("2 Outer Pack Lines", 2, shipment.OuterPackLines.Count);
			var packLine = shipment.OuterPackLines[1];

			AssertEquals("Goods description", "BOXES OF DG THINGOES", packLine.JL_Description);
			AssertEquals("OuterPacks", 10, packLine.JL_PackageCount);
			AssertEquals("Package type", "BOX", packLine.JL_F3_NKPackType);
			AssertEquals("Marks And Numbers", "", packLine.JL_MarksAndNumbers);

			notes = shipment.Notes.FindByDescription("Forwarded Manifest Other Information");
			AssertEquals("Just one note", 1, notes.Length);
			noteBuilder = new ZStringBuilder();
			noteBuilder.Append("Missing Seal: 111 on container: OCLU1231230");
			noteBuilder.Append("Missing Seal: 222 on container: OCLU1231230");
			noteBuilder.Append("Missing Seal: 333 on container: OCLU1231230");
			AssertEquals("Other Information", noteBuilder.ToStringWithNewLineBetweenAppends(), notes[0].ST_NoteDataAsText);
		}

		public void TestAddNewShipmentToLoadListWithMinimalMessage()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+130828:2051+2678++++++1'
UNG+GOVCBR+CCR+U10207V1+20130828:2051+13+UN+D:11B'
UNH+1+GOVCBR:D:11B:UN'
BGM+714+8036X555+4'
UNT+69+1'
UNE+1+13'
UNZ+1+2678'";

			#endregion

			var interchange = Enterprise.Messaging.Business.EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString.Replace("\r\n", ""),
				Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var tmpMessage = interchange.ContainedMessages[0];
			tmpMessage.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			Factory.NewWithPrimaryKey<EDIMessage>(new Guid("f0d4ec86-9e4d-4a51-aae1-b840ad6a53ec")).CopyPersistentValuesFrom(tmpMessage);
			tmpMessage.Delete();
			interchange.ContainedMessages.Load();
			var message = interchange.ContainedMessages[0] as ACIHouseBillMessage;
			AssertNotNull(message);
			Factory.Save();

			var loadList = Factory.New<CFSLoadListConsol>();
			var supporter = new ForwardedManifestSupporter(message);
			var shipment = supporter.AddNewShipmentToLoadList(loadList);

			AssertCollectionContains("New shipment was added to Load List", shipment, loadList.Shipments);
		}
	}
}
