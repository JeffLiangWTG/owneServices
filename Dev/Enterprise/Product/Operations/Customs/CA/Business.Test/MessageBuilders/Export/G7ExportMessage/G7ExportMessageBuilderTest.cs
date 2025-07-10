using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class G7ExportMessageBuilderTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestG7ExportMessageBuilder()
		{
			var mock = GetMock();
			var g7ExportData = mock.Object;

			var builder = new G7ExportMessageBuilder(g7ExportData, MessageSubTypes.Create);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			AssertMultilineASCIIEquals("Message text", ExpectedResult, msg.EM_FormattedMessageText);

			var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\G7ExportMessageInterpretation.html");
			AssertMultilineASCIIEquals("G7 Export Message Interpretation", expectedInterpretation, msg.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		Mock<IG7Export> GetMock()
		{
			//GlbCertificates BrokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			//BrokerLicence.GL_LicenseNumber = "21311";
			//BrokerLicence.GL_LicenseType = CertificateTypePairList.Codes.BR1;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B12345678";
			var newExporter = Factory.New<OrgHeader>();
			newExporter.OH_Code = "CONSIGNOR";
			newExporter.OH_FullName = "MICHAELROSEN";
			newExporter.MainAddress.OA_Address1 = "1107 TELESAT DRIVE";
			newExporter.MainAddress.OA_Address2 = "2NDADDRESSLN";
			newExporter.MainAddress.OA_City = "GLOUCESTER";
			newExporter.MainAddress.OA_RL_NKRelatedPortCode = "USXXX";
			newExporter.MainAddress.OA_State = "ME";
			newExporter.MainAddress.OA_PostCode = "12345";
			newExporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForExport, "123456789RM0001");
			newExporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AuthorizationID, "KC3333");
			dec.JE_OA_SupplierAddress = newExporter.MainAddress.PK;
			var newImporter = Factory.New<OrgHeader>();
			newImporter.OH_Code = "CONSIGNEE";
			newImporter.OH_FullName = "STATIONARY STORE INC";
			newImporter.MainAddress.OA_Address1 = "301 TREEMONT AVENUE";
			newImporter.MainAddress.OA_Address2 = "2ND ADDRESS LINE";
			newImporter.MainAddress.OA_City = "PORTLAND";
			newImporter.MainAddress.OA_RL_NKRelatedPortCode = "USXXX";
			newImporter.MainAddress.OA_State = "ME";
			newImporter.MainAddress.OA_PostCode = "04071";
			newImporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "987654321RM0002");
			dec.JE_OA_ImporterAddress = newImporter.MainAddress.PK;
			var exporter = dec.SupplierAddress;
			var importer = dec.ImporterAddress;

			var mock = new Mock<IG7Export>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New<JobDeclaration>()));
			mock.Setup(m => m.DocumentMessageNumber).Returns("ABCD1234");
			mock.Setup(m => m.PortOfExit).Returns("0009");
			mock.Setup(m => m.PlaceOfReport).Returns("0351");
			mock.Setup(m => m.DateOfExport).Returns(new ZDateTime(2005, 12, 28, 11, 20, 0));
			mock.Setup(m => m.CommodityGrossWeight).Returns(1000m);
			mock.Setup(m => m.CommodityGrossWeightUnitOfMeasure).Returns("KG");

			var mockContainer1 = new Mock<IG7Container>();
			mockContainer1.Setup(m => m.ContainerNumber).Returns("ABC12345678");
			mockContainer1.Setup(m => m.CountryOfRegistration).Returns("CA");
			mockContainer1.Setup(m => m.ContainerSizeCode).Returns("BPG0");

			var mockContainer2 = new Mock<IG7Container>();
			mockContainer2.Setup(m => m.ContainerNumber).Returns("ABC12345679");
			mockContainer2.Setup(m => m.CountryOfRegistration).Returns(ZString.Empty);
			mockContainer2.Setup(m => m.ContainerSizeCode).Returns(ZString.Empty);

			mock.Setup(m => m.Containers).Returns(new[] { mockContainer1.Object, mockContainer2.Object });
			mock.Setup(m => m.ModeOfTransport).Returns("1");
			mock.Setup(m => m.VesselName).Returns("SEAPRINCESS");
			mock.Setup(m => m.CarrierCode).Returns("9991");
			mock.Setup(m => m.CarrierName).Returns("CARRIER NAME");
			mock.Setup(m => m.TransactionNumber).Returns("B12345678");
			mock.Setup(m => m.TransportationDocumentNumber).Returns("9991KZKICARGO-01");
			mock.Setup(m => m.CAEDAuthorizationID).Returns("KC3333");
			mock.Setup(m => m.ServiceOption).Returns("661");
			mock.Setup(m => m.NumberOfPackages).Returns(1234);
			mock.Setup(m => m.TypeOfPackages).Returns("BOX");
			mock.Setup(m => m.Exporter).Returns(exporter);
			mock.Setup(m => m.ExporterBusinessNumber).Returns("123456789RM0001");
			mock.Setup(m => m.DeliveryParty).Returns(importer);
			mock.Setup(m => m.DeliveryPartyBusinessNumber).Returns("987654321RM0002");
			mock.Setup(m => m.BrokerSecurityNumber).Returns("21311");
			mock.Setup(m => m.CountryOfFinalDestination).Returns("BE");
			mock.Setup(m => m.InvoiceTotal).Returns(5000m);
			mock.Setup(m => m.InvoiceCurrencyCode).Returns("AUD");
			mock.Setup(m => m.FreightChargesInCAD).Returns(299.99m);
			mock.Setup(m => m.References).Returns(new ZString[] { "1223", "22222", "12345678901234567890", "12", "TRUNCATED" });
			mock.Setup(m => m.ReasonForExport).Returns("26");
			mock.Setup(m => m.Consignee).Returns(importer);
			mock.Setup(m => m.Authentication).Returns("12345678");

			var mockLine1 = new Mock<IG7ItemLine>();
			mockLine1.Setup(m => m.CountryOfOrigin).Returns("US");
			mockLine1.Setup(m => m.ProvinceOfOrigin).Returns("CO");
			mockLine1.Setup(m => m.VINs).Returns(new ZString[] { "VIN1", "VIN2" });
			mockLine1.Setup(m => m.Permits).Returns(new ZString[] { "PERMITNUMBER1", "PERMITNUMBER2" });
			mockLine1.Setup(m => m.ProductDescription).Returns("BIGCARS");
			mockLine1.Setup(m => m.InvoiceLineNumber).Returns(1);
			mockLine1.Setup(m => m.ClassificationNumber).Returns("87011010");
			mockLine1.Setup(m => m.Quantity).Returns(1m);
			mockLine1.Setup(m => m.UnitOfMeasure).Returns("DZN");
			mockLine1.Setup(m => m.CustomsValue).Returns(5000m);
			mockLine1.Setup(m => m.CurrencyCode).Returns("USD");
			var line1 = mockLine1.Object;

			var mockLine2 = new Mock<IG7ItemLine>();
			mockLine2.Setup(m => m.CountryOfOrigin).Returns("AU");
			mockLine2.Setup(m => m.ProvinceOfOrigin).Returns("");
			mockLine2.Setup(m => m.VINs).Returns(System.Array.Empty<ZString>());
			mockLine2.Setup(m => m.Permits).Returns(System.Array.Empty<ZString>());
			mockLine2.Setup(m => m.ProductDescription).Returns("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789>EXTRA STUFF TO BE TRUNCATED");
			mockLine2.Setup(m => m.InvoiceLineNumber).Returns(2);
			mockLine2.Setup(m => m.ClassificationNumber).Returns("8701101299");
			mockLine2.Setup(m => m.Quantity).Returns(2m);
			mockLine2.Setup(m => m.UnitOfMeasure).Returns("BOX");
			mockLine2.Setup(m => m.CustomsValue).Returns(123.45m);
			mockLine2.Setup(m => m.CurrencyCode).Returns("AUD");
			var line2 = mockLine2.Object;

			var detailLines = new List<IG7ItemLine>();
			detailLines.Add(line1);
			detailLines.Add(line2);
			mock.Setup(m => m.Details).Returns(detailLines);

			return mock;
		}

		const string ExpectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+GSIMEX:D:00A:CC:EX1STP
BGM+914+ABCD1234+9
LOC+42+0009::96
LOC+172+0351::96
DTM+129:200512281120:203
MEA+WT+AAD+KGM:1000
EQD+CN+ABC12345678CABPG0::5
EQD+CN+ABC12345679
TDT+11++1++9991::96+++:::SEAPRINCESS
RFF+ABT:B12345678
RFF+AAS:9991KZKICARGO-01
PAC+1234++BOX
RFF+AIJ:KC3333
CST++661:117:96
NAD+EX+123456789RM0001::96++MICHAELROSEN+1107 TELESAT DRIVE:2NDADDRESSLN+GLOUCESTER+ME+12345+US
NAD+AG+21311::96
NAD+DP+987654321RM0002::96++STATIONARY STORE INC+301 TREEMONT AVENUE:2ND ADDRESS LINE+PORTLAND+ME+04071+US
MOA+39:5000.00:AUD
MOA+64:300:CAD
UNS+D
SEQ++1
DMS+1223,22222,12345678901234567890,12
GEI+3+26
NAD+CN+++STATIONARY STORE INC+301 TREEMONT AVENUE:2ND ADDRESS LINE+PORTLAND+ME+04071+US
LIN+1
LOC+27+US::5+CO::96
RFF+AKG:VIN1
RFF+AKG:VIN2
DOC+811::96+PERMITNUMBER1
DOC+811::96+PERMITNUMBER2
IMD+++:::BIGCARS
RFF+LI:1
GID+1
CST++87011010:169:96
MEA+AAR++DZN:1
MOA+40:5000.00:USD
LIN+2
LOC+27+AU::5
IMD+++:::1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789>
RFF+LI:2
GID+1
CST++8701101299:169:96
MEA+AAR++BOX:2
MOA+40:123.45:AUD
UNS+S
AUT+12345678
UNT+47+<<MSGNO PLACEHOLDER>>";
	}
}
