using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTS182EdiMessageParsingXMLPrettyDataProviderTest : TestCaseWithFactory
	{
		public void TestMessage() => Assert(ReferenceEquals(message, parsingXMLProvider.Message));
		public void TestMessageType() => AssertEquals("CC182C", parsingXMLProvider.MessageType);
		public void TestMessageRecipient() => AssertEquals("GB123456789000", parsingXMLProvider.MessageRecipient);
		public void TestMRN() => AssertEquals("23GB000246T9DW2SJ9", parsingXMLProvider.MRN);
		public void TestCustomsOfficeOfDeparture() => AssertEquals("GB000246", parsingXMLProvider.CustomsOfficeOfDeparture);

		[TestDate(2023, 07, 20)]
		public void TestConsignmentIncidents()
		{
			var transportEquipments = new List<INCTSTransportEquipment>
			{
				new NCTSPrettierIncidentTransportEquipmentData(transportEquipmentSequenceNumber: "1",
					transportEquipmentContainerIdentificationNumber: "WGPCGR",
					transportEquipmentNumberOfSeals: "1",
					transportEquipmentSeals: new List<INCTSSeal> { new NCTSPrettierSealData(sequenceNumber: "1", identifier: "1234") },
					goodsReferences: Enumerable.Empty<INCTSGoodsReference>().ToList())
			};
			var values = new List<INCTSIncidentData>
			{
				new NCTSPrettierIncidentData(sequenceNumber: "1",code: "3",text: "Change of tractor unit",transhipment: new NCTSPrettierTranshipmentData("1", "AF", "0012", "30"),locationAddress: null,transportEquipments)
				{
					EndorsementDate = ZDateTime.Today.ToDateTime(),
					EndorsementAuthority = "XIHMRC",
					EndorsementPlace = "GBBEL",
					EndorsementCountry = "XI",
					LocationQualifierOfIdentification = "U",
					LocationUNLocode = "GBBEL",
					LocationCountry = "XI",
					LocationLatitude = ZString.Empty,
					LocationLongitude = ZString.Empty
				}
			};
			var expectedIncidents = values;
			AssertType<NCTSPrettierIncidentData>(expectedIncidents[0]);
			AssertType<NCTSPrettierIncidentData>(parsingXMLProvider.Incidents.FirstOrDefault());
			AssertEquals(expectedIncidents[0].SequenceNumber, parsingXMLProvider.Incidents.FirstOrDefault().SequenceNumber);
			AssertEquals(expectedIncidents[0].Code, parsingXMLProvider.Incidents.FirstOrDefault().Code);
			AssertEquals(expectedIncidents[0].Text, parsingXMLProvider.Incidents.FirstOrDefault().Text);
			AssertEquals(expectedIncidents[0].Transhipment, parsingXMLProvider.Incidents.FirstOrDefault().Transhipment);
			AssertEquals(expectedIncidents[0].LocationAddress, parsingXMLProvider.Incidents.FirstOrDefault().LocationAddress);
			AssertEquals(expectedIncidents[0].EndorsementDate, parsingXMLProvider.Incidents.FirstOrDefault().EndorsementDate);
			AssertEquals(expectedIncidents[0].EndorsementAuthority, parsingXMLProvider.Incidents.FirstOrDefault().EndorsementAuthority);
			AssertEquals(expectedIncidents[0].EndorsementPlace, parsingXMLProvider.Incidents.FirstOrDefault().EndorsementPlace);
			AssertEquals(expectedIncidents[0].EndorsementCountry, parsingXMLProvider.Incidents.FirstOrDefault().EndorsementCountry);
			AssertEquals(expectedIncidents[0].LocationQualifierOfIdentification, parsingXMLProvider.Incidents.FirstOrDefault().LocationQualifierOfIdentification);
			AssertEquals(expectedIncidents[0].LocationUNLocode, parsingXMLProvider.Incidents.FirstOrDefault().LocationUNLocode);
			AssertEquals(expectedIncidents[0].LocationCountry, parsingXMLProvider.Incidents.FirstOrDefault().LocationCountry);
			AssertEquals(expectedIncidents[0].LocationLatitude, parsingXMLProvider.Incidents.FirstOrDefault().LocationLatitude);
			AssertEquals(expectedIncidents[0].LocationLongitude, parsingXMLProvider.Incidents.FirstOrDefault().LocationLongitude);
			AssertEquals(expectedIncidents[0].TransportEquipments.FirstOrDefault().SequenceNumber, parsingXMLProvider.Incidents.FirstOrDefault().TransportEquipments.FirstOrDefault().SequenceNumber);
			AssertEquals(expectedIncidents[0].TransportEquipments.FirstOrDefault().NumberOfSeals, parsingXMLProvider.Incidents.FirstOrDefault().TransportEquipments.FirstOrDefault().NumberOfSeals);
			AssertEquals(expectedIncidents[0].TransportEquipments.FirstOrDefault().ContainerIdentificationNumber, parsingXMLProvider.Incidents.FirstOrDefault().TransportEquipments.FirstOrDefault().ContainerIdentificationNumber);
			AssertEquals(expectedIncidents[0].TransportEquipments.FirstOrDefault().Seals.FirstOrDefault().SequenceNumber, parsingXMLProvider.Incidents.FirstOrDefault().TransportEquipments.FirstOrDefault().Seals.FirstOrDefault().SequenceNumber);
			AssertEquals(expectedIncidents[0].TransportEquipments.FirstOrDefault().Seals.FirstOrDefault().Identifier, parsingXMLProvider.Incidents.FirstOrDefault().TransportEquipments.FirstOrDefault().Seals.FirstOrDefault().Identifier);
		}

		public void TestMakeInboundPrettyForInterpretation()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			message.EM_MessageText = "Not a NCTS 5 Message";
			prettier = new NCTS182ResponsePrettyFormatter(message);
			AssertEquals(message.HumanReadableMessage, prettier.MakeInboundPrettyForInterpretation(header));
		}

		public void TestMakeInboundPrettyForPhase5Interpretation()
		{
			CreateZZRefTestValuesIfNeeded("GB000246", Core.Constants.CountryCodes.UnitedKingdom, ["DES"], "UK South Auth Consignor/nees");
			CreateZZRefTestValuesIfNeeded("XI000142", Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, ["DES"], "Belfast");
			AssertEquals(ExpectedHtml, prettier.MakeInboundPrettyForInterpretation(header));
		}

		public static void CreateZZRefTestValuesIfNeeded(ZString officeCode, ZString dataGroupingCode, ZString[] officePurpose, ZString desc)
		{
			var factory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);

			var eunzzz = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunzzz);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, desc, officePurpose);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			factory.Save();
		}
		protected override void SetUp()
		{
			base.SetUp();

			var retriever = new EmbeddedResourceRetriever();
			var testMessageContent = retriever.GetString(TestFilePath);

			message = Factory.New<EDIMessage>();
			message.EM_MessageText = testMessageContent;
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			parsingXMLProvider = new NCTS182EdiMessageParsingXMLPrettyDataProvider(message);
			prettier = new NCTS182ResponsePrettyFormatter(message);
		}

		EDIMessage message;
		NCTS182EdiMessageParsingXMLPrettyDataProvider parsingXMLProvider;
		NCTS182ResponsePrettyFormatter prettier;
		NctsHeader header;
		public const string TestFilePath = "Enterprise.Customs.EU.NCTS.Business.Testing.Ncts.EdiMessage.Prettier.TestFiles.Incoming.NCTS182TestMessage.xml";
		const string ExpectedHtml = "<h2>Events during the journey</h2><table><tr><td>MRN:</td><td>23GB000246T9DW2SJ9.</td></tr><tr><td>Date:</td><td>2023-07-20T14:24:45</td></tr><tr><td>Customs Office Departure:</td><td>GB000246 UK South Auth Consignor/nees</td></tr><tr><td>Customs Office Incident:</td><td>XI000142 Belfast</td></tr></table><h3>Incident</h3><table><tr><td>Number:</td><td>1</td></tr><tr><td>Code:</td><td>Under the supervision of the customs authority, goods are transferred from one means of transport to another means of transport.</td></tr><tr><td>Text:</td><td>Change of tractor unit</td></tr></table><h4>Endorsement</h4><table><tr><td>Authority:</td><td>XIHMRC</td></tr><tr><td>Country:</td><td>XI</td></tr><tr><td>Date:</td><td>20-07-2023</td></tr><tr><td>Place:</td><td>GBBEL</td></tr></table><h4>Location</h4><table><tr><td>Qualifier:</td><td>U</td></tr><tr><td>UNLOCODE:</td><td>GBBEL</td></tr><tr><td>GNNS Latitude:</td><td>&nbsp;</td></tr><tr><td>GNNS Longitude:</td><td>&nbsp;</td></tr></table><h4>Transhipment</h4><table><tr><td>Container:</td><td>Yes</td></tr><tr><td>Transport Means:</td><td>AF 0012 Registration Number of the Road Vehicle</td></tr></table><h4>Transport Equipment</h4><table><tr><td>Number:</td><td>1</td></tr><tr><td>Container:</td><td>WGPCGR</td></tr><tr><td>Number of Seals:</td><td>1</td></tr><tr><td>Seals:</td><td>1234</td></tr></table>";
	}
}
