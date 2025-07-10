using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class EManifestHouseBillMessageBuilderTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOriginalMessageText()
		{
			var expectedMessage = ExpectedCreateMessage.Replace("\r\n", "");
			var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\ACIHouseMessageInterpretation.html");

			using (CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertOriginalMessageText(expectedMessage, expectedInterpretation);
			}
			using (CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				AssertOriginalMessageText(expectedMessage.Replace(":ACIHG", ""), expectedInterpretation.Replace(":ACIHG", ""));
			}
		}

		public void TestRelasePortDefaultsToDischarge()
		{
			var messageBuilder = new ACIHouseBillMessageBuilder(GetACIHouseBillMockData(Factory, releasePort: "", releaseSubLocation: ""), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			Assert("release defaults to disc", message.EM_MessageText.Contains("LOC+8+0454+1451'"));
			Assert("discharge is mandatory", message.EM_MessageText.Contains("LOC+11+0454+1451'"));
		}

		public void TestSameRelaseAndDischargePorts()
		{
			var messageBuilder = new ACIHouseBillMessageBuilder(GetACIHouseBillMockData(Factory, releasePort: "0454", releaseSubLocation: "1451"), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			Assert("release same as disc", message.EM_MessageText.Contains("LOC+8+0454+1451'"));
			Assert("discharge is mandatory", message.EM_MessageText.Contains("LOC+11+0454+1451'"));
		}

		public void TestLogSegments()
		{
			var messageBuilder = new ACIHouseBillMessageBuilder(GetACIHouseBillMockData(Factory, releasePort: "0454", releaseSubLocation: "1452"), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			Assert("release same as disc", message.EM_MessageText.Contains("LOC+8+0454+1452'"));
			Assert("release same , sub-location different", message.EM_MessageText.Contains("LOC+11+0454+1451'"));
		}

		public void TestMessageSubTypeForPostArrivalChange()
		{
			var messageBuilder = new ACIHouseBillMessageBuilder(GetACIHouseBillMockData(Factory), MessageSubTypes.Request);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			AssertEquals(MessageSubTypeCodes.Codes.Change, message.EM_MessageSubType);
		}

		public void TestTCCSegmentsWithuNDgCodeIsEmpty()
		{
			var messageBuilder = new ACIHouseBillMessageBuilder(GetACIHouseBillMockData(Factory, releasePort: "", releaseSubLocation: ""), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			Assert("if DGCodes is empty, no TCC segment", !message.EM_MessageText.Contains("TCC+++:SSC"));
		}

		public void TestSameAddressBetweenDeliveryAndConsignee()
		{
			var messageBuilder = new ACIHouseBillMessageBuilder(GetACIHouseBillMockAddressData(Factory, releasePort: "0454", releaseSubLocation: "1451"), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			AssertNotContains("NAD+DP+++", message.EM_MessageText);
		}

		static IACIHouseBillProvider GetACIHouseBillMockAddressData(BusinessObjectFactory factory, string releasePort = "0453", string releaseSubLocation = "2955")
		{
			var bo = factory.New<CusCAeMHHouse>();

			var mock = new Mock<IACIHouseBillProvider>();
			mock.Setup(m => m.MessageStatus).Returns(ZString.Empty);
			mock.Setup(m => m.JobStatus).Returns(ZString.Empty);
			mock.Setup(m => m.Messages).Returns(bo.Messages);
			mock.Setup(m => m.TopLevelBusinessObject).Returns(bo);
			mock.Setup(m => m.Factory).Returns(factory);
			mock.Setup(m => m.HouseCCN).Returns("999912345678");
			mock.Setup(m => m.JobIdentification).Returns("S12345678");
			mock.Setup(m => m.UCR).Returns("UNIQUE CONSIGNMENT REFERENCE NUMBER");
			var mockSecondaryNotifyPartyBroker = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyBroker.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.CustomsBroker);
			mockSecondaryNotifyPartyBroker.Setup(m => m.Identifier).Returns("12345");
			mockSecondaryNotifyPartyBroker.Setup(m => m.NoticeType).Returns("MF");
			var mockSecondaryNotifyPartyForwarder = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyForwarder.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.FreightForwarder);
			mockSecondaryNotifyPartyForwarder.Setup(m => m.Identifier).Returns("8XXX");
			mockSecondaryNotifyPartyForwarder.Setup(m => m.NoticeType).Returns("MF");
			var mockSecondaryNotifyPartyCarrier = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyCarrier.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.Carrier);
			mockSecondaryNotifyPartyCarrier.Setup(m => m.Identifier).Returns("081");
			mockSecondaryNotifyPartyCarrier.Setup(m => m.NoticeType).Returns("MF");
			var mockSecondaryNotifyPartyWH = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyWH.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.WarehouseKeeper);
			mockSecondaryNotifyPartyWH.Setup(m => m.Identifier).Returns("555");
			mockSecondaryNotifyPartyWH.Setup(m => m.NoticeType).Returns("MF");
			mock.Setup(m => m.SecondaryNotifyParties).Returns(new[] { mockSecondaryNotifyPartyBroker.Object, mockSecondaryNotifyPartyForwarder.Object, mockSecondaryNotifyPartyCarrier.Object, mockSecondaryNotifyPartyWH.Object });
			mock.Setup(m => m.MovementType).Returns("27");
			mock.Setup(m => m.PrimaryCCN).Returns("9XXX12345");
			mock.Setup(m => m.B2BComments).Returns("BUSINESS TO BUSINESS COMMENTS");
			mock.Setup(m => m.AmendmentReason).Returns("35");
			mock.Setup(m => m.TransportMode).Returns("1");
			mock.Setup(m => m.IsConsolidatedCargo).Returns(true);
			mock.Setup(m => m.Volume).Returns(12.3m);
			mock.Setup(m => m.VolumeUOM).Returns("MTQ");
			mock.Setup(m => m.SpecialHandlingInstructions).Returns("SPECIAL INSTRUCTIONS");

			var consignee = factory.New<JobDocAddress>();
			consignee.E2_CompanyName = "NAME777777777778888888888889999999999977777777778888888888999999222222";
			consignee.E2_Address1 = "ADDRESS LINE 1";
			consignee.E2_Address2 = "ADDRESS LINE 2";
			consignee.E2_RN_NKCountryCode = "CA";
			consignee.E2_City = "CITY";
			consignee.E2_State = "STATE";
			consignee.E2_Postcode = "POST CD";
			consignee.E2_Contact = "C333IGNEEQCONTACTQNAME222222222333333333344444444445555555555666666666677777777777";
			consignee.E2_Phone = "12345678";
			consignee.E2_AddressOverride = true;

			mock.Setup(m => m.Consignee).Returns(consignee);

			var mockShipper = new Mock<IJobDocAddress>();
			mockShipper.Setup(m => m.E2_CompanyName).Returns("ÄNÄME");
			mockShipper.Setup(m => m.E2_Address1).Returns("ÄDDRESS LINE 1");
			mockShipper.Setup(m => m.E2_Address2).Returns("ADDRESS LINE 2 6789012345678901234><789012345678901234567890X>");
			mockShipper.Setup(m => m.E2_City).Returns("CITY");
			mockShipper.Setup(m => m.E2_State).Returns("STATE");
			mockShipper.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockShipper.Setup(m => m.E2_RN_NKCountryCode).Returns("AU");
			mockShipper.Setup(m => m.E2_Contact).Returns("SHIPPER CONTACT NAME");
			mockShipper.Setup(m => m.E2_Phone).Returns("");
			mock.Setup(m => m.Shipper).Returns(mockShipper.Object);

			var delivery = factory.New<JobDocAddress>();
			delivery.E2_CompanyName = "NAME777777777778888888888889999999999977777777778888888888999999222222";
			delivery.E2_Address1 = "ADDRESS LINE 1";
			delivery.E2_Address2 = "ADDRESS LINE 2";
			delivery.E2_City = "CITY";
			delivery.E2_State = "STATE";
			delivery.E2_Postcode = "POST CD";
			delivery.E2_RN_NKCountryCode = "CA";
			delivery.E2_Contact = "C333IGNEEQCONTACTQNAME222222222333333333344444444445555555555666666666677777777777";
			delivery.E2_Phone = "12345678";
			delivery.E2_AddressOverride = true;
			mock.Setup(m => m.DeliveryAddresses).Returns(new[] { delivery });

			var mockNotify1 = new Mock<IJobDocAddress>();
			mockNotify1.Setup(m => m.E2_CompanyName).Returns("NOTIFY 1 NAME");
			mockNotify1.Setup(m => m.E2_Address1).Returns("ADDRESS LINE 1");
			mockNotify1.Setup(m => m.E2_Address2).Returns("");
			mockNotify1.Setup(m => m.E2_City).Returns("CITY");
			mockNotify1.Setup(m => m.E2_State).Returns("STATE");
			mockNotify1.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockNotify1.Setup(m => m.E2_RN_NKCountryCode).Returns("US");
			mockNotify1.Setup(m => m.E2_Contact).Returns("");
			mockNotify1.Setup(m => m.E2_Phone).Returns("12345678");
			mock.Setup(m => m.NotifyParties).Returns(new[] { mockNotify1.Object });

			var mockPlaceOfConsolidation = new Mock<IJobDocAddress>();
			mockPlaceOfConsolidation.Setup(m => m.E2_CompanyName).Returns("PLACE OF CONSOLIDATION NAME");
			mockPlaceOfConsolidation.Setup(m => m.E2_Address1).Returns("ADDRESS LINE 1");
			mockPlaceOfConsolidation.Setup(m => m.E2_Address2).Returns("ADDRESS LINE 2");
			mockPlaceOfConsolidation.Setup(m => m.E2_City).Returns("CITY");
			mockPlaceOfConsolidation.Setup(m => m.E2_State).Returns("STATE");
			mockPlaceOfConsolidation.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockPlaceOfConsolidation.Setup(m => m.E2_RN_NKCountryCode).Returns("US");
			mockPlaceOfConsolidation.Setup(m => m.E2_Contact).Returns("CONTACT NAME");
			mockPlaceOfConsolidation.Setup(m => m.E2_Phone).Returns("1234567");
			mock.Setup(m => m.PlaceOfConsolidation).Returns(mockPlaceOfConsolidation.Object);

			var mockConsolidator = new Mock<IJobDocAddress>();
			mockConsolidator.Setup(m => m.E2_CompanyName).Returns("CONSOLIDATOR");
			mockConsolidator.Setup(m => m.E2_Address1).Returns("ADDRESS LINE 1");
			mockConsolidator.Setup(m => m.E2_Address2).Returns("ADDRESS LINE 2");
			mockConsolidator.Setup(m => m.E2_City).Returns("CITY");
			mockConsolidator.Setup(m => m.E2_State).Returns("STATE");
			mockConsolidator.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockConsolidator.Setup(m => m.E2_RN_NKCountryCode).Returns("US");
			mockConsolidator.Setup(m => m.E2_Contact).Returns("CONSOLIDATOR CONTACT NAME");
			mockConsolidator.Setup(m => m.E2_Phone).Returns("1234567");
			mock.Setup(m => m.Consolidator).Returns(mockConsolidator.Object);

			var mockUNDGContact = new Mock<IOrgContact>();
			mockUNDGContact.Setup(m => m.OC_ContactName).Returns("UNDG CONTACT NAME");
			mockUNDGContact.Setup(m => m.OC_Phone).Returns("12346789");
			mock.Setup(m => m.UNDGContact).Returns(mockUNDGContact.Object);

			mock.Setup(m => m.ReleasePortCode).Returns(releasePort);
			mock.Setup(m => m.ReleaseSubLocationCode).Returns(releaseSubLocation);
			mock.Setup(m => m.DischargePortCode).Returns("0454");
			mock.Setup(m => m.DischargeSubLocationCode).Returns("1451");
			mock.Setup(m => m.DGSpecialInstructions).Returns("DANGEROUS GOODS SPECIAL INSTRUCTIONS");

			var mockContainer1 = new Mock<IHouseBillContainer>();
			mockContainer1.Setup(m => m.ContainerNumber).Returns("CONT1231230");
			mockContainer1.Setup(m => m.Seals).Returns(Array.Empty<ZString>());
			var mockContainer2 = new Mock<IHouseBillContainer>();
			mockContainer2.Setup(m => m.ContainerNumber).Returns("CONT2222220");
			mockContainer2.Setup(m => m.Seals).Returns(new ZString[] { "SEAL1123456789012345", "SEAL2123456789012345" });
			mock.Setup(m => m.Containers).Returns(new[] { mockContainer1.Object, mockContainer2.Object });

			var mockLine1 = new Mock<IHouseBillLine>();
			mockLine1.Setup(m => m.Packs).Returns(9);
			mockLine1.Setup(m => m.PacksUOM).Returns("BOX");
			mockLine1.Setup(m => m.Marks).Returns(new ZString[] { "MARK1", "MARK2" });
			mockLine1.Setup(m => m.LineNumber).Returns(1);
			mockLine1.Setup(m => m.GoodsDescription).Returns("CARGO DESCRIPTION");
			mockLine1.Setup(m => m.HSCode).Returns("0000700000");
			mockLine1.Setup(m => m.DGCodes).Returns(new ZString[] { "UNDG1", "UNDG2" });
			mock.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			mock.Setup(m => m.TotalWeight).Returns(8.9m);
			mock.Setup(m => m.TotalWeightUOM).Returns("LBR");

			return mock.Object;
		}

		static IACIHouseBillProvider GetACIHouseBillMockData(BusinessObjectFactory factory, string releasePort = "0453", string releaseSubLocation = "2955")
		{
			var bo = factory.New<CusCAeMHHouse>();

			var mock = new Mock<IACIHouseBillProvider>();
			mock.Setup(m => m.MessageStatus).Returns(ZString.Empty);
			mock.Setup(m => m.JobStatus).Returns(ZString.Empty);
			mock.Setup(m => m.Messages).Returns(bo.Messages);
			mock.Setup(m => m.TopLevelBusinessObject).Returns(bo);
			mock.Setup(m => m.Factory).Returns(factory);
			mock.Setup(m => m.HouseCCN).Returns("999912345678");
			mock.Setup(m => m.JobIdentification).Returns("S12345678");
			mock.Setup(m => m.UCR).Returns("UNIQUE CONSIGNMENT REFERENCE NUMBER");
			var mockSecondaryNotifyPartyBroker = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyBroker.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.CustomsBroker);
			mockSecondaryNotifyPartyBroker.Setup(m => m.Identifier).Returns("12345");
			mockSecondaryNotifyPartyBroker.Setup(m => m.NoticeType).Returns("MF");
			var mockSecondaryNotifyPartyForwarder = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyForwarder.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.FreightForwarder);
			mockSecondaryNotifyPartyForwarder.Setup(m => m.Identifier).Returns("8XXX");
			mockSecondaryNotifyPartyForwarder.Setup(m => m.NoticeType).Returns("MF");
			var mockSecondaryNotifyPartyCarrier = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyCarrier.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.Carrier);
			mockSecondaryNotifyPartyCarrier.Setup(m => m.Identifier).Returns("081");
			mockSecondaryNotifyPartyCarrier.Setup(m => m.NoticeType).Returns("MF");
			var mockSecondaryNotifyPartyWH = new Mock<ISecondaryNotifyParty>();
			mockSecondaryNotifyPartyWH.Setup(m => m.SecondaryNotifyType).Returns(PartyFunctionCodeQualifierList.WarehouseKeeper);
			mockSecondaryNotifyPartyWH.Setup(m => m.Identifier).Returns("555");
			mockSecondaryNotifyPartyWH.Setup(m => m.NoticeType).Returns("MF");
			mock.Setup(m => m.SecondaryNotifyParties).Returns(new[] { mockSecondaryNotifyPartyBroker.Object, mockSecondaryNotifyPartyForwarder.Object, mockSecondaryNotifyPartyCarrier.Object, mockSecondaryNotifyPartyWH.Object });
			mock.Setup(m => m.MovementType).Returns("27");
			mock.Setup(m => m.PrimaryCCN).Returns("9XXX12345");
			mock.Setup(m => m.B2BComments).Returns("BUSINESS TO BUSINESS COMMENTS");
			mock.Setup(m => m.AmendmentReason).Returns("35");
			mock.Setup(m => m.TransportMode).Returns("1");
			mock.Setup(m => m.IsConsolidatedCargo).Returns(true);
			mock.Setup(m => m.Volume).Returns(12.3m);
			mock.Setup(m => m.VolumeUOM).Returns("MTQ");
			mock.Setup(m => m.SpecialHandlingInstructions).Returns("SPECIAL INSTRUCTIONS");

			var consignee = factory.New<JobDocAddress>();
			consignee.E2_AddressOverride = true;
			consignee.E2_CompanyName = "NAME 67890123456789012345678901234567890123456789012345678901234567890XXXXXX9";
			consignee.E2_Address1 = "ADDRESS LINE 1 6789012345678901234><78901234567890";
			consignee.E2_Address2 = "ADDRESS LINE 2 6789012345678901234><78901234567890";
			consignee.E2_City = "CITY           6789012345678901234><78901234567890";
			consignee.E2_State = "STATE 78>XXXXX";
			consignee.E2_Postcode = "POST CD8>X";
			consignee.E2_RN_NKCountryCode = "US";
			consignee.E2_Contact = "C333IGNEEQCONTACTQNAME222222222333333333344444444445555555555666666666677777777777";
			consignee.E2_Phone = "12345678";
			mock.Setup(m => m.Consignee).Returns(consignee);

			var mockShipper = new Mock<IJobDocAddress>();
			mockShipper.Setup(m => m.E2_CompanyName).Returns("ÄNÄME");
			mockShipper.Setup(m => m.E2_Address1).Returns("ÄDDRESS LINE 1");
			mockShipper.Setup(m => m.E2_Address2).Returns("ADDRESS LINE 2 6789012345678901234><789012345678901234567890X>");
			mockShipper.Setup(m => m.E2_City).Returns("CITY");
			mockShipper.Setup(m => m.E2_State).Returns("STATE");
			mockShipper.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockShipper.Setup(m => m.E2_RN_NKCountryCode).Returns("AU");
			mockShipper.Setup(m => m.E2_Contact).Returns("SHIPPER CONTACT NAME");
			mockShipper.Setup(m => m.E2_Phone).Returns("");
			mock.Setup(m => m.Shipper).Returns(mockShipper.Object);

			var delivery1 = factory.New<JobDocAddress>();
			delivery1.E2_AddressOverride = true;
			delivery1.E2_CompanyName = "DELIVERY NAME";
			delivery1.E2_Address1 = "ADDRESS LINE 1 6789012345678901234><78901234567890";
			delivery1.E2_City = "CITY";
			delivery1.E2_State = "STATE";
			delivery1.E2_Postcode = "POST CD";
			delivery1.E2_RN_NKCountryCode = "US";

			var delivery2 = factory.New<JobDocAddress>();
			delivery2.E2_AddressOverride = true;
			delivery2.E2_CompanyName = "DELIVERY 2 NAME";
			delivery2.E2_Address1 = "ADDRESS LINE 1";
			delivery2.E2_Address2 = "ADDRESS LINE 2";
			delivery2.E2_City = "CITY";
			delivery2.E2_State = "STATE";
			delivery2.E2_Postcode = "POST CD";
			delivery2.E2_RN_NKCountryCode = "US";
			delivery2.E2_Contact = "DELIVERY CONTACT";

			mock.Setup(m => m.DeliveryAddresses).Returns(new[] { delivery1, delivery2 });

			var mockNotify1 = new Mock<IJobDocAddress>();
			mockNotify1.Setup(m => m.E2_CompanyName).Returns("NOTIFY 1 NAME");
			mockNotify1.Setup(m => m.E2_Address1).Returns("ADDRESS LINE 1");
			mockNotify1.Setup(m => m.E2_Address2).Returns("");
			mockNotify1.Setup(m => m.E2_City).Returns("CITY");
			mockNotify1.Setup(m => m.E2_State).Returns("STATE");
			mockNotify1.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockNotify1.Setup(m => m.E2_RN_NKCountryCode).Returns("US");
			mockNotify1.Setup(m => m.E2_Contact).Returns("");
			mockNotify1.Setup(m => m.E2_Phone).Returns("12345678");
			var mockNotify2 = new Mock<IJobDocAddress>();
			mockNotify2.Setup(m => m.E2_CompanyName).Returns("NOTIFY 2 NAME");
			mockNotify2.Setup(m => m.E2_Address1).Returns("ADDRESS LINE 1");
			mockNotify2.Setup(m => m.E2_Address2).Returns("");
			mockNotify2.Setup(m => m.E2_City).Returns("CITY");
			mockNotify2.Setup(m => m.E2_State).Returns("STATE");
			mockNotify2.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockNotify2.Setup(m => m.E2_RN_NKCountryCode).Returns("US");
			mockNotify2.Setup(m => m.E2_Contact).Returns("");
			mockNotify2.Setup(m => m.E2_Phone).Returns("");
			mock.Setup(m => m.NotifyParties).Returns(new[] { mockNotify1.Object, mockNotify2.Object });

			var mockPlaceOfConsolidation = new Mock<IJobDocAddress>();
			mockPlaceOfConsolidation.Setup(m => m.E2_CompanyName).Returns("PLACE OF CONSOLIDATION NAME");
			mockPlaceOfConsolidation.Setup(m => m.E2_Address1).Returns("ADDRESS LINE 1");
			mockPlaceOfConsolidation.Setup(m => m.E2_Address2).Returns("ADDRESS LINE 2");
			mockPlaceOfConsolidation.Setup(m => m.E2_City).Returns("CITY");
			mockPlaceOfConsolidation.Setup(m => m.E2_State).Returns("STATE");
			mockPlaceOfConsolidation.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockPlaceOfConsolidation.Setup(m => m.E2_RN_NKCountryCode).Returns("US");
			mockPlaceOfConsolidation.Setup(m => m.E2_Contact).Returns("CONTACT NAME");
			mockPlaceOfConsolidation.Setup(m => m.E2_Phone).Returns("1234567");
			mock.Setup(m => m.PlaceOfConsolidation).Returns(mockPlaceOfConsolidation.Object);

			var mockConsolidator = new Mock<IJobDocAddress>();
			mockConsolidator.Setup(m => m.E2_CompanyName).Returns("CONSOLIDATOR");
			mockConsolidator.Setup(m => m.E2_Address1).Returns("ADDRESS LINE 1");
			mockConsolidator.Setup(m => m.E2_Address2).Returns("ADDRESS LINE 2");
			mockConsolidator.Setup(m => m.E2_City).Returns("CITY");
			mockConsolidator.Setup(m => m.E2_State).Returns("STATE");
			mockConsolidator.Setup(m => m.E2_Postcode).Returns("POST CD");
			mockConsolidator.Setup(m => m.E2_RN_NKCountryCode).Returns("US");
			mockConsolidator.Setup(m => m.E2_Contact).Returns("CONSOLIDATOR CONTACT NAME");
			mockConsolidator.Setup(m => m.E2_Phone).Returns("1234567");
			mock.Setup(m => m.Consolidator).Returns(mockConsolidator.Object);

			var mockUNDGContact = new Mock<IOrgContact>();
			mockUNDGContact.Setup(m => m.OC_ContactName).Returns("UNDG CONTACT NAME");
			mockUNDGContact.Setup(m => m.OC_Phone).Returns("12346789");
			mock.Setup(m => m.UNDGContact).Returns(mockUNDGContact.Object);

			mock.Setup(m => m.ReleasePortCode).Returns(releasePort);
			mock.Setup(m => m.ReleaseSubLocationCode).Returns(releaseSubLocation);
			mock.Setup(m => m.DischargePortCode).Returns("0454");
			mock.Setup(m => m.DischargeSubLocationCode).Returns("1451");
			mock.Setup(m => m.DGSpecialInstructions).Returns("DANGEROUS GOODS SPECIAL INSTRUCTIONS");

			var mockContainer1 = new Mock<IHouseBillContainer>();
			mockContainer1.Setup(m => m.ContainerNumber).Returns("CONT1231230");
			mockContainer1.Setup(m => m.Seals).Returns(Array.Empty<ZString>());
			var mockContainer2 = new Mock<IHouseBillContainer>();
			mockContainer2.Setup(m => m.ContainerNumber).Returns("CONT2222220");
			mockContainer2.Setup(m => m.Seals).Returns(new ZString[] { "SEAL1123456789012345", "SEAL2123456789012345" });
			mock.Setup(m => m.Containers).Returns(new[] { mockContainer1.Object, mockContainer2.Object });

			var mockLine1 = new Mock<IHouseBillLine>();
			mockLine1.Setup(m => m.Packs).Returns(9);
			mockLine1.Setup(m => m.PacksUOM).Returns("BOX");
			mockLine1.Setup(m => m.Marks).Returns(new ZString[] { "MARK1", "MARK2" });
			mockLine1.Setup(m => m.LineNumber).Returns(1);
			mockLine1.Setup(m => m.GoodsDescription).Returns("CARGO DESCRIPTION");
			mockLine1.Setup(m => m.HSCode).Returns("0000700000");
			mockLine1.Setup(m => m.DGCodes).Returns(new ZString[] { "UNDG1", "UNDG2" });
			var mockLine2 = new Mock<IHouseBillLine>();
			mockLine2.Setup(m => m.Packs).Returns(1);
			mockLine2.Setup(m => m.PacksUOM).Returns("UNT");
			mockLine2.Setup(m => m.Marks).Returns(new ZString[] { "MARKS" });
			mockLine2.Setup(m => m.LineNumber).Returns(2);
			mockLine2.Setup(m => m.GoodsDescription).Returns("CARGO DESCRIPTION 2");
			mockLine2.Setup(m => m.HSCode).Returns("");
			mockLine2.Setup(m => m.DGCodes).Returns(Array.Empty<ZString>());
			var mockLine3 = new Mock<IHouseBillLine>();
			mockLine3.Setup(m => m.Packs).Returns(1);
			mockLine3.Setup(m => m.PacksUOM).Returns("UNT");
			mockLine3.Setup(m => m.Marks).Returns(new ZString[] { "MARKS" });
			mockLine3.Setup(m => m.LineNumber).Returns(3);
			mockLine3.Setup(m => m.GoodsDescription).Returns("CARGO DESCRIPTION 3");
			mockLine3.Setup(m => m.HSCode).Returns("");
			mockLine3.Setup(m => m.DGCodes).Returns(new ZString[] { ZString.Empty });
			mock.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object, mockLine3.Object });

			mock.Setup(m => m.TotalWeight).Returns(8.9m);
			mock.Setup(m => m.TotalWeightUOM).Returns("LBR");

			return mock.Object;
		}

		void AssertOriginalMessageText(ZString expectedMessage, ZString expectedInterpretation)
		{
			var messageBuilder = new ACIHouseBillMessageBuilder(GetACIHouseBillMockData(Factory), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			AssertMultilineASCIIEquals("Fully Populated Create Message", expectedMessage, message.EM_MessageText);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertMultilineASCIIEquals("Fully Populated Create Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		const string ExpectedCreateMessage = @"UNH+<<MSGNO PLACEHOLDER>>+GOVCBR:D:11B:UN:ACIHG'
BGM+714+999912345678+9'
RFF+ABO:S12345678'
RFF+UCN:UNIQUE CONSIGNMENT REFERENCE NUMBER'
NAD+CB+12345'
IFD++++MF'
NAD+FW+8XXX'
IFD++++MF'
NAD+CA+081'
IFD++++MF'
NAD+WH+555'
IFD++++MF'
DOC+23+:27'
DOC+85+9XXX12345'
RCS+15'
FTX+ACB+++BUSINESS TO BUSINESS COMMENTS'
AJT+ZZZ+35'
TDT+11++1'
UNS+D'
HYN+3'
CNI+1'
STS++1'
MEA+AAX++MTQ:12'
HAN+:::SPECIAL INSTRUCTIONS'
NAD+CN+++NAME 67890123456789012345678901234567890123456789012345678901234567890+ADDRESS LINE 1 6789012345678901234>:<78901234567890:ADDRESS LINE 2 6789012345678901234>+CITY           6789012345678901234>+STATE 78>+POST CD8>+US'
CTA+IC+:C333IGNEEQCONTACTQNAME222222222333333333344444444445555555555666666666'
CTA+AH'
COM+12345678:TE'
NAD+CZ+++ANAME+ADDRESS LINE 1:ADDRESS LINE 2 6789012345678901234>:<789012345678901234567890X>+CITY+++AU'
CTA+IC+:SHIPPER CONTACT NAME'
NAD+DP+++DELIVERY NAME+ADDRESS LINE 1 6789012345678901234>:<78901234567890+CITY+STATE+POST CD+US'
NAD+DP+++DELIVERY 2 NAME+ADDRESS LINE 1:ADDRESS LINE 2+CITY+STATE+POST CD+US'
CTA+IC+:DELIVERY CONTACT'
NAD+NI+++NOTIFY 1 NAME+ADDRESS LINE 1+CITY+STATE+POST CD+US'
CTA+AH'
COM+12345678:TE'
NAD+NI+++NOTIFY 2 NAME+ADDRESS LINE 1+CITY+STATE+POST CD+US'
NAD+ZZZ+++PLACE OF CONSOLIDATION NAME+ADDRESS LINE 1:ADDRESS LINE 2+CITY+STATE+POST CD+US'
CTA+IC+:CONTACT NAME'
CTA+AH'
COM+1234567:TE'
NAD+PK+++UNDG CONTACT NAME'
CTA+AH'
COM+12346789:TE'
LOC+8+0453+2955'
LOC+11+0454+1451'
DOC+714'
NAD+CS+++CONSOLIDATOR+ADDRESS LINE 1:ADDRESS LINE 2+CITY+STATE+POST CD+US'
CTA+IC+:CONSOLIDATOR CONTACT NAME'
CTA+AH'
COM+1234567:TE'
RCS+15'
FTX+AAC+++DANGEROUS GOODS SPECIAL INSTRUCTIONS'
EQD+CN+CONT1231230'
SEQ+4'
EQD+CN+CONT2222220'
SEQ+4'
SEL+SEAL11234567890'
SEQ+4'
SEL+SEAL21234567890'
SEQ+4'
TDT+1'
SEQ+4'
PAC+9++:::BOX'
SEQ+4'
PCI++MARK1'
PCI++MARK2'
GID+1'
FTX+AAA+++CARGO DESCRIPTION'
TCC+++0000700000:SRZ'
TCC+++UNDG1:SSC'
TCC+++UNDG2:SSC'
SEQ+4'
PAC+1++:::UNT'
SEQ+4'
PCI++MARKS'
GID+2'
FTX+AAA+++CARGO DESCRIPTION 2'
SEQ+4'
PAC+1++:::UNT'
SEQ+4'
PCI++MARKS'
GID+3'
FTX+AAA+++CARGO DESCRIPTION 3'
UNS+S'
CNT+7:9:LBR'
UNT+87+<<MSGNO PLACEHOLDER>>'";
	}
}
