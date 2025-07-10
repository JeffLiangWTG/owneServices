using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Asycuda;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class BISIFileExporterShipmentCountryDataTest : TestCaseWithFactory
	{
		[TestDate(2017, 12, 06, 12, 47, 31)]
		public void TestShipmentCountryDetailsIncludedInResult()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						var mock = new Mock<IShipmentData>();
						mock.Setup(m => m.ShipmentRef).Returns("12345678");
						mock.Setup(m => m.ImportDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ImporterAccountNumber).Returns("123456");
						mock.Setup(m => m.DutyType).Returns("AA");
						mock.Setup(m => m.BillingTerms).Returns("B");
						mock.Setup(m => m.MasterBillNumber).Returns("1231421421");
						mock.Setup(m => m.DischargePort).Returns("1234");
						mock.Setup(m => m.CustomsValue).Returns(1000m);
						mock.Setup(m => m.DVCCurrencyCode).Returns("USD");
						mock.Setup(m => m.CustomsExchangeRate).Returns(0.9m);
						mock.Setup(m => m.BISICustomsEntryStatus).Returns("CCC");
						mock.Setup(m => m.EntryType).Returns("DD");
						mock.Setup(m => m.CustomsStatus).Returns("EEE");
						mock.Setup(m => m.CustomsEntryNumber).Returns("1453543253");
						mock.Setup(m => m.CustomsEntryDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ThirdPartyIndicator).Returns("F");
						mock.Setup(m => m.BisiDeclarationUploadDate).Returns(new ZDate(2017, 12, 06));
						mock.Setup(m => m.IsAlreadyUploaded).Returns(false);
						mock.Setup(m => m.ShouldBeUploaded).Returns(true);
						mock.Setup(m => m.ShouldBeDownloaded).Returns(false);
						mock.Setup(m => m.StatisticalValue).Returns(200m);
						mock.Setup(m => m.CustomsOfficeNumber).Returns("ASD");
						mock.Setup(m => m.VATNumber).Returns("HHH");
						mock.Setup(m => m.ImporterVATDefermentNumber).Returns("IIII");
						mock.Setup(m => m.SplitDutyDefermentNumber).Returns("KKKK");
						var commoditiesData = System.Array.Empty<CommodityDetailData>();
						mock.Setup(m => m.CommoditiesData).Returns(commoditiesData);
						var chargesData = new ShipmentChargeData[1];
						chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 100, "SGD");
						mock.Setup(m => m.ChargesData).Returns(chargesData);
						var receiptsData = new ShipmentReceiptData[2];
						receiptsData[0] = new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, "10");
						receiptsData[1] = new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, "112-456875");
						mock.Setup(m => m.ReceiptsData).Returns(receiptsData);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(mock.Object);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-0612:47:3100000100001000003                                                                                                                                                                                                                                                                
12345678   SG1000002017-12-05ADD12345678   123456    AA1231421421    00000001000000000000020000USD0000900000000CCDD1453543253 2017-12-05ASDHHH         IIII    KKKK                                                                                                                                         
12345678   SG3000002017-12-05ADD00710                                                                                                                                                                                                                                                                       
12345678   SG3000002017-12-05ADD005112-456875                                                                                                                                                                                                                                                               
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetailsWithGIRO()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						var mock = new Mock<IShipmentData>();
						mock.Setup(m => m.ShipmentRef).Returns("12345678");
						mock.Setup(m => m.ImportDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ImporterAccountNumber).Returns("123456");
						mock.Setup(m => m.DutyType).Returns("AA");
						mock.Setup(m => m.BillingTerms).Returns("B");
						mock.Setup(m => m.MasterBillNumber).Returns("1231421421");
						mock.Setup(m => m.DischargePort).Returns("1234");
						mock.Setup(m => m.CustomsValue).Returns(1000m);
						mock.Setup(m => m.DVCCurrencyCode).Returns("USD");
						mock.Setup(m => m.CustomsExchangeRate).Returns(0.9m);
						mock.Setup(m => m.BISICustomsEntryStatus).Returns("CCC");
						mock.Setup(m => m.EntryType).Returns("DD");
						mock.Setup(m => m.CustomsStatus).Returns("EEE");
						mock.Setup(m => m.CustomsEntryNumber).Returns("1453543253");
						mock.Setup(m => m.CustomsEntryDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ThirdPartyIndicator).Returns("F");
						mock.Setup(m => m.BisiDeclarationUploadDate).Returns(new ZDate(2017, 12, 06));
						mock.Setup(m => m.IsAlreadyUploaded).Returns(false);
						mock.Setup(m => m.ShouldBeUploaded).Returns(true);
						mock.Setup(m => m.ShouldBeDownloaded).Returns(false);
						mock.Setup(m => m.StatisticalValue).Returns(200m);
						mock.Setup(m => m.CustomsOfficeNumber).Returns("ASD");
						mock.Setup(m => m.VATNumber).Returns("HHH");
						mock.Setup(m => m.ImporterVATDefermentNumber).Returns("IIII");
						mock.Setup(m => m.SplitDutyDefermentNumber).Returns("KKKK");
						var commoditiesData = System.Array.Empty<CommodityDetailData>();
						mock.Setup(m => m.CommoditiesData).Returns(commoditiesData);
						var chargesData = new ShipmentChargeData[1];
						chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 100, "SGD");
						mock.Setup(m => m.ChargesData).Returns(chargesData);
						var receiptsData = new ShipmentReceiptData[3];
						receiptsData[0] = new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, "10");
						receiptsData[1] = new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, "112-456875");
						receiptsData[2] = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidThruGIRO);
						mock.Setup(m => m.ReceiptsData).Returns(receiptsData);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(mock.Object);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000005                                                                                                                                                                                                                                                                
12345678   SG1000002017-12-05ADD12345678   123456    AA1231421421    00000001000000000000020000USD0000900000000CCDD1453543253 2017-12-05ASDHHH         IIII    KKKK                                                                                                                                         
12345678   SG3000002017-12-05ADD00710                                                                                                                                                                                                                                                                       
12345678   SG3000002017-12-05ADD005112-456875                                                                                                                                                                                                                                                               
12345678   SG3000002017-12-05ADD004G05                                                                                                                                                                                                                                                                      
12345678   SG5000002017-12-05ADD20100000000100000000SGD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2018, 1, 1, 10, 53, 0)]
		public void TestShipmentChargeLinesIncludedForAU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						var mock = new Mock<IShipmentData>();
						mock.Setup(m => m.ShipmentRef).Returns("12345678");
						mock.Setup(m => m.ImportDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ImporterAccountNumber).Returns("123456");
						mock.Setup(m => m.DutyType).Returns("AA");
						mock.Setup(m => m.BillingTerms).Returns("B");
						mock.Setup(m => m.MasterBillNumber).Returns("1231421421");
						mock.Setup(m => m.DischargePort).Returns("1234");
						mock.Setup(m => m.CustomsValue).Returns(1000m);
						mock.Setup(m => m.DVCCurrencyCode).Returns("USD");
						mock.Setup(m => m.CustomsExchangeRate).Returns(0.9m);
						mock.Setup(m => m.BISICustomsEntryStatus).Returns("CCC");
						mock.Setup(m => m.EntryType).Returns("DD");
						mock.Setup(m => m.CustomsStatus).Returns("EEE");
						mock.Setup(m => m.CustomsEntryNumber).Returns("1453543253");
						mock.Setup(m => m.CustomsEntryDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ThirdPartyIndicator).Returns("F");
						mock.Setup(m => m.BisiDeclarationUploadDate).Returns(new ZDate(2017, 12, 06));
						mock.Setup(m => m.IsAlreadyUploaded).Returns(false);
						mock.Setup(m => m.ShouldBeUploaded).Returns(true);
						mock.Setup(m => m.ShouldBeDownloaded).Returns(false);
						mock.Setup(m => m.StatisticalValue).Returns(200m);
						mock.Setup(m => m.CustomsOfficeNumber).Returns("ASD");
						mock.Setup(m => m.VATNumber).Returns("HHH");
						mock.Setup(m => m.ImporterVATDefermentNumber).Returns("IIII");
						mock.Setup(m => m.SplitDutyDefermentNumber).Returns("KKKK");
						var commoditiesData = System.Array.Empty<CommodityDetailData>();
						mock.Setup(m => m.CommoditiesData).Returns(commoditiesData);
						var chargesData = new ShipmentChargeData[1];
						chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 100, "AUD");
						mock.Setup(m => m.ChargesData).Returns(chargesData);
						var receiptsData = System.Array.Empty<ShipmentReceiptData>();
						mock.Setup(m => m.ReceiptsData).Returns(receiptsData);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(mock.Object);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2018-01-0110:53:0000000100001000002                                                                                                                                                                                                                                                                
12345678   AU1000002017-12-05ADD12345678   123456    AA1231421421    00000001000000000000020000USD0000900000000CCDD1453543253 2017-12-05ASDHHH         IIII    KKKK                                                                                                                                         
12345678   AU5000002017-12-05ADD20100000000100000000AUD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetails_WhenIsTaxCertificateAndGIROCodeIsNotG02_SG()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						var mock = new Mock<IShipmentData>();
						mock.Setup(m => m.ShipmentRef).Returns("12345678");
						mock.Setup(m => m.ImportDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ImporterAccountNumber).Returns("123456");
						mock.Setup(m => m.DutyType).Returns("AA");
						mock.Setup(m => m.BillingTerms).Returns("B");
						mock.Setup(m => m.MasterBillNumber).Returns("1231421421");
						mock.Setup(m => m.DischargePort).Returns("1234");
						mock.Setup(m => m.CustomsValue).Returns(1000m);
						mock.Setup(m => m.DVCCurrencyCode).Returns("USD");
						mock.Setup(m => m.CustomsExchangeRate).Returns(0.9m);
						mock.Setup(m => m.BISICustomsEntryStatus).Returns("CCC");
						mock.Setup(m => m.EntryType).Returns("DD");
						mock.Setup(m => m.CustomsStatus).Returns("EEE");
						mock.Setup(m => m.CustomsEntryNumber).Returns("1453543253");
						mock.Setup(m => m.CustomsEntryDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ThirdPartyIndicator).Returns("F");
						mock.Setup(m => m.BisiDeclarationUploadDate).Returns(new ZDate(2017, 12, 06));
						mock.Setup(m => m.IsAlreadyUploaded).Returns(false);
						mock.Setup(m => m.ShouldBeUploaded).Returns(true);
						mock.Setup(m => m.ShouldBeDownloaded).Returns(false);
						mock.Setup(m => m.StatisticalValue).Returns(200m);
						mock.Setup(m => m.CustomsOfficeNumber).Returns("ASD");
						mock.Setup(m => m.VATNumber).Returns("HHH");
						mock.Setup(m => m.ImporterVATDefermentNumber).Returns("IIII");
						mock.Setup(m => m.SplitDutyDefermentNumber).Returns("KKKK");
						var commoditiesData = System.Array.Empty<CommodityDetailData>();
						mock.Setup(m => m.CommoditiesData).Returns(commoditiesData);
						var chargesData = new ShipmentChargeData[1];
						chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 100, "SGD");
						mock.Setup(m => m.ChargesData).Returns(chargesData);
						var receiptsData = new ShipmentReceiptData[3];
						receiptsData[0] = new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, "10");
						receiptsData[1] = new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCTaxCertificateNumber, "Y729384572");
						receiptsData[2] = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidAtCheckPoint);
						mock.Setup(m => m.ReceiptsData).Returns(receiptsData);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(mock.Object);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000004                                                                                                                                                                                                                                                                
12345678   SG1000002017-12-05ADD12345678   123456    AA1231421421    00000001000000000000020000USD0000900000000CCDD1453543253 2017-12-05ASDHHH         IIII    KKKK                                                                                                                                         
12345678   SG3000002017-12-05ADD00710                                                                                                                                                                                                                                                                       
12345678   SG3000002017-12-05ADD004G02                                                                                                                                                                                                                                                                      
12345678   SG5000002017-12-05ADD20100000000100000000SGD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetails_WhenIsTaxCertificateAndGIROCodeIsG02_SG()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						var mock = new Mock<IShipmentData>();
						mock.Setup(m => m.ShipmentRef).Returns("12345678");
						mock.Setup(m => m.ImportDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ImporterAccountNumber).Returns("123456");
						mock.Setup(m => m.DutyType).Returns("AA");
						mock.Setup(m => m.BillingTerms).Returns("B");
						mock.Setup(m => m.MasterBillNumber).Returns("1231421421");
						mock.Setup(m => m.DischargePort).Returns("1234");
						mock.Setup(m => m.CustomsValue).Returns(1000m);
						mock.Setup(m => m.DVCCurrencyCode).Returns("USD");
						mock.Setup(m => m.CustomsExchangeRate).Returns(0.9m);
						mock.Setup(m => m.BISICustomsEntryStatus).Returns("CCC");
						mock.Setup(m => m.EntryType).Returns("DD");
						mock.Setup(m => m.CustomsStatus).Returns("EEE");
						mock.Setup(m => m.CustomsEntryNumber).Returns("1453543253");
						mock.Setup(m => m.CustomsEntryDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ThirdPartyIndicator).Returns("F");
						mock.Setup(m => m.BisiDeclarationUploadDate).Returns(new ZDate(2017, 12, 06));
						mock.Setup(m => m.IsAlreadyUploaded).Returns(false);
						mock.Setup(m => m.ShouldBeUploaded).Returns(true);
						mock.Setup(m => m.ShouldBeDownloaded).Returns(false);
						mock.Setup(m => m.StatisticalValue).Returns(200m);
						mock.Setup(m => m.CustomsOfficeNumber).Returns("ASD");
						mock.Setup(m => m.VATNumber).Returns("HHH");
						mock.Setup(m => m.ImporterVATDefermentNumber).Returns("IIII");
						mock.Setup(m => m.SplitDutyDefermentNumber).Returns("KKKK");
						var commoditiesData = System.Array.Empty<CommodityDetailData>();
						mock.Setup(m => m.CommoditiesData).Returns(commoditiesData);
						var chargesData = new ShipmentChargeData[1];
						chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 100, "SGD");
						mock.Setup(m => m.ChargesData).Returns(chargesData);
						var receiptsData = new ShipmentReceiptData[3];
						receiptsData[0] = new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, "10");
						receiptsData[1] = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidAtCheckPoint);
						receiptsData[2] = new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCTaxCertificateNumber, "Y729384572");
						mock.Setup(m => m.ReceiptsData).Returns(receiptsData);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(mock.Object);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000005                                                                                                                                                                                                                                                                
12345678   SG1000002017-12-05ADD12345678   123456    AA1231421421    00000001000000000000020000USD0000900000000CCDD1453543253 2017-12-05ASDHHH         IIII    KKKK                                                                                                                                         
12345678   SG3000002017-12-05ADD00710                                                                                                                                                                                                                                                                       
12345678   SG3000002017-12-05ADD004G02                                                                                                                                                                                                                                                                      
12345678   SG3000002017-12-05ADD008Y729384572                                                                                                                                                                                                                                                               
12345678   SG5000002017-12-05ADD20100000000100000000SGD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetails_WhenIsTaxCertificateAndGIROCodeIsNotG02_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						var mock = new Mock<IShipmentData>();
						mock.Setup(m => m.ShipmentRef).Returns("12345678");
						mock.Setup(m => m.ImportDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ImporterAccountNumber).Returns("123456");
						mock.Setup(m => m.DutyType).Returns("AA");
						mock.Setup(m => m.BillingTerms).Returns("B");
						mock.Setup(m => m.MasterBillNumber).Returns("1231421421");
						mock.Setup(m => m.DischargePort).Returns("1234");
						mock.Setup(m => m.CustomsValue).Returns(1000m);
						mock.Setup(m => m.DVCCurrencyCode).Returns("USD");
						mock.Setup(m => m.CustomsExchangeRate).Returns(0.9m);
						mock.Setup(m => m.BISICustomsEntryStatus).Returns("CCC");
						mock.Setup(m => m.EntryType).Returns("DD");
						mock.Setup(m => m.CustomsStatus).Returns("EEE");
						mock.Setup(m => m.CustomsEntryNumber).Returns("1453543253");
						mock.Setup(m => m.CustomsEntryDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ThirdPartyIndicator).Returns("F");
						mock.Setup(m => m.BisiDeclarationUploadDate).Returns(new ZDate(2017, 12, 06));
						mock.Setup(m => m.IsAlreadyUploaded).Returns(false);
						mock.Setup(m => m.ShouldBeUploaded).Returns(true);
						mock.Setup(m => m.ShouldBeDownloaded).Returns(false);
						mock.Setup(m => m.StatisticalValue).Returns(200m);
						mock.Setup(m => m.CustomsOfficeNumber).Returns("ASD");
						mock.Setup(m => m.VATNumber).Returns("HHH");
						mock.Setup(m => m.ImporterVATDefermentNumber).Returns("IIII");
						mock.Setup(m => m.SplitDutyDefermentNumber).Returns("KKKK");
						var commoditiesData = System.Array.Empty<CommodityDetailData>();
						mock.Setup(m => m.CommoditiesData).Returns(commoditiesData);
						var chargesData = new ShipmentChargeData[1];
						chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 100, "SGD");
						mock.Setup(m => m.ChargesData).Returns(chargesData);
						var receiptsData = new ShipmentReceiptData[3];
						receiptsData[0] = new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, "10");
						receiptsData[1] = new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCTaxCertificateNumber, "Y729384572");
						receiptsData[2] = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidAtCheckPoint);
						mock.Setup(m => m.ReceiptsData).Returns(receiptsData);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(mock.Object);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000005                                                                                                                                                                                                                                                                
12345678   AU1000002017-12-05ADD12345678   123456    AA1231421421    00000001000000000000020000USD0000900000000CCDD1453543253 2017-12-05ASDHHH         IIII    KKKK                                                                                                                                         
12345678   AU3000002017-12-05ADD00710                                                                                                                                                                                                                                                                       
12345678   AU3000002017-12-05ADD008Y729384572                                                                                                                                                                                                                                                               
12345678   AU3000002017-12-05ADD004G02                                                                                                                                                                                                                                                                      
12345678   AU5000002017-12-05ADD20100000000100000000SGD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetails_WhenIsTaxCertificateAndGIROCodeIsG02_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						var mock = new Mock<IShipmentData>();
						mock.Setup(m => m.ShipmentRef).Returns("12345678");
						mock.Setup(m => m.ImportDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ImporterAccountNumber).Returns("123456");
						mock.Setup(m => m.DutyType).Returns("AA");
						mock.Setup(m => m.BillingTerms).Returns("B");
						mock.Setup(m => m.MasterBillNumber).Returns("1231421421");
						mock.Setup(m => m.DischargePort).Returns("1234");
						mock.Setup(m => m.CustomsValue).Returns(1000m);
						mock.Setup(m => m.DVCCurrencyCode).Returns("USD");
						mock.Setup(m => m.CustomsExchangeRate).Returns(0.9m);
						mock.Setup(m => m.BISICustomsEntryStatus).Returns("CCC");
						mock.Setup(m => m.EntryType).Returns("DD");
						mock.Setup(m => m.CustomsStatus).Returns("EEE");
						mock.Setup(m => m.CustomsEntryNumber).Returns("1453543253");
						mock.Setup(m => m.CustomsEntryDate).Returns(new ZDate(2017, 12, 05));
						mock.Setup(m => m.ThirdPartyIndicator).Returns("F");
						mock.Setup(m => m.BisiDeclarationUploadDate).Returns(new ZDate(2017, 12, 06));
						mock.Setup(m => m.IsAlreadyUploaded).Returns(false);
						mock.Setup(m => m.ShouldBeUploaded).Returns(true);
						mock.Setup(m => m.ShouldBeDownloaded).Returns(false);
						mock.Setup(m => m.StatisticalValue).Returns(200m);
						mock.Setup(m => m.CustomsOfficeNumber).Returns("ASD");
						mock.Setup(m => m.VATNumber).Returns("HHH");
						mock.Setup(m => m.ImporterVATDefermentNumber).Returns("IIII");
						mock.Setup(m => m.SplitDutyDefermentNumber).Returns("KKKK");
						var commoditiesData = System.Array.Empty<CommodityDetailData>();
						mock.Setup(m => m.CommoditiesData).Returns(commoditiesData);
						var chargesData = new ShipmentChargeData[1];
						chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 100, "SGD");
						mock.Setup(m => m.ChargesData).Returns(chargesData);
						var receiptsData = new ShipmentReceiptData[3];
						receiptsData[0] = new ShipmentReceiptData(ShipmentReceiptTypeCode.CycleNumber, "10");
						receiptsData[2] = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidAtCheckPoint);
						receiptsData[1] = new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCTaxCertificateNumber, "Y729384572");
						mock.Setup(m => m.ReceiptsData).Returns(receiptsData);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(mock.Object);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000005                                                                                                                                                                                                                                                                
12345678   AU1000002017-12-05ADD12345678   123456    AA1231421421    00000001000000000000020000USD0000900000000CCDD1453543253 2017-12-05ASDHHH         IIII    KKKK                                                                                                                                         
12345678   AU3000002017-12-05ADD00710                                                                                                                                                                                                                                                                       
12345678   AU3000002017-12-05ADD008Y729384572                                                                                                                                                                                                                                                               
12345678   AU3000002017-12-05ADD004G02                                                                                                                                                                                                                                                                      
12345678   AU5000002017-12-05ADD20100000000100000000SGD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetails_TradeNetDeclaration_G04()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						UPEDataRegistry.Instance.EnableUPECustomisations = true;
						var dec = Factory.NewWithValidTestData<JobDeclaration>();
						dec.JE_IsCancelled = ZBool.False;
						dec.JE_DeclarationReference = "dec ref";
						dec.JE_HouseBill = "housebill";
						dec.JE_MasterBill = "masterbill";
						dec.JE_DateOfArrival = new ZDateTime(2000, 1, 1);
						dec.JE_RS_NKServiceLevel = "slv";
						dec.JE_TotalNoOfPacks = 110;
						dec.JE_GB = Env.CurrentBranchPK;
						var importer = Factory.NewWithValidTestData<OrgHeader>();
						importer.OH_Code = "importer";
						importer.OH_FullName = "importer fullname";
						importer.MainAddress.OA_Address1 = "importer address 1";
						importer.MainAddress.OA_Address2 = "importer address 2";
						importer.MainAddress.OA_City = "importer city";
						importer.MainAddress.OA_Phone = "importer phone";
						dec.JE_OH_Importer = importer.PK;
						dec.JE_VoyageFlightNo = "Flight123";
						dec.JE_MessageType = MessageTypeCodeList.Codes.IPT;
						dec.JE_MasterBill = "123";
						var entry = dec.CustomsEntryHeaders.AddNew();
						var entryLine = entry.MergedLines.AddNew();
						var invoice = dec.Invoices.AddNew();
						invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
						invoice.JZ_InvoiceCurrExRate = 1.39m;
						var invoiceline = entryLine.InvoiceLines.AddNew();
						invoiceline.JI_JZ = invoice.PK;
						var cusEntryNum = Factory.New<CusEntryNumber>();
						cusEntryNum.CE_EntryNum = "MC123";
						cusEntryNum.CE_EntryIsSystemGenerated = true;
						cusEntryNum.CE_ParentID = entry.PK;
						cusEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
						cusEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
						cusEntryNum.CE_EntryType = "CER";
						entry.EntryNumber = "123";
						var entryPayInfo = entry.EntryPayInfos.AddNew();
						entryPayInfo.C9_PaymentReference = "123";
						entryPayInfo.C9_PaymentParty = UPEOrgRematch.OrgTypes.Importer;
						dec.Logs.AddNew(Events.CustomsEntryStatus, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, ZDateTimeOffset.Now);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.GST, 10m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.Duty, 110m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.Excise, 210m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.OtherTax, 310m);
						var tradenet = new TradeNetIShipmentDataProxy("DOK", ZDateTime.UtcNow, dec);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(tradenet);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000007                                                                                                                                                                                                                                                                
housebill  SG1000002000-01-01ADDhousebill            01123           00000000000000000000000000SGD000139000000002  123        2017-12-20                                                                                                                                                                    
housebill  SG3000002000-01-01ADD004G04                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD005123                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD0062017-12-20                                                                                                                                                                                                                                                               
housebill  SG3000002000-01-01ADD00701                                                                                                                                                                                                                                                                       
housebill  SG3000002000-01-01ADD009123                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD0100001LOT                                                                                                                                                                                                                                                                  
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetails_TradeNetDeclaration_G05()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						UPEDataRegistry.Instance.EnableUPECustomisations = true;
						var dec = Factory.NewWithValidTestData<JobDeclaration>();
						dec.JE_IsCancelled = ZBool.False;
						dec.JE_DeclarationReference = "dec ref";
						dec.JE_HouseBill = "housebill";
						dec.JE_MasterBill = "masterbill";
						dec.JE_DateOfArrival = new ZDateTime(2000, 1, 1);
						dec.JE_RS_NKServiceLevel = "slv";
						dec.JE_TotalNoOfPacks = 110;
						dec.JE_GB = Env.CurrentBranchPK;
						var importer = Factory.NewWithValidTestData<OrgHeader>();
						importer.OH_Code = "importer";
						importer.OH_FullName = "importer fullname";
						importer.MainAddress.OA_Address1 = "importer address 1";
						importer.MainAddress.OA_Address2 = "importer address 2";
						importer.MainAddress.OA_City = "importer city";
						importer.MainAddress.OA_Phone = "importer phone";
						dec.JE_OH_Importer = importer.PK;
						dec.JE_VoyageFlightNo = "Flight123";
						dec.JE_MessageType = MessageTypeCodeList.Codes.IPT;
						dec.JE_MasterBill = "123";
						var entry = dec.CustomsEntryHeaders.AddNew();
						var entryLine = entry.MergedLines.AddNew();
						var invoice = dec.Invoices.AddNew();
						invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
						invoice.JZ_InvoiceCurrExRate = 1.39m;
						var invoiceline = entryLine.InvoiceLines.AddNew();
						invoiceline.JI_JZ = invoice.PK;
						var cusEntryNum = Factory.New<CusEntryNumber>();
						cusEntryNum.CE_EntryNum = "MC123";
						cusEntryNum.CE_EntryIsSystemGenerated = true;
						cusEntryNum.CE_ParentID = entry.PK;
						cusEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
						cusEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
						cusEntryNum.CE_EntryType = "CER";
						entry.EntryNumber = "123";
						var entryPayInfo = entry.EntryPayInfos.AddNew();
						entryPayInfo.C9_PaymentReference = "123";
						entryPayInfo.C9_PaymentParty = UPEOrgRematch.OrgTypes.Broker;
						dec.Logs.AddNew(Events.CustomsEntryStatus, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, ZDateTimeOffset.Now);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.GST, 10m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.Duty, 110m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.Excise, 210m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.OtherTax, 310m);
						var tradenet = new TradeNetIShipmentDataProxy("DOK", ZDateTime.UtcNow, dec);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(tradenet);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000009                                                                                                                                                                                                                                                                
housebill  SG1000002000-01-01ADDhousebill            01123           00000000000000000000000000SGD000139000000002  123        2017-12-20                                                                                                                                                                    
housebill  SG3000002000-01-01ADD004G05                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD005123                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD0062017-12-20                                                                                                                                                                                                                                                               
housebill  SG3000002000-01-01ADD00701                                                                                                                                                                                                                                                                       
housebill  SG3000002000-01-01ADD009123                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD0100001LOT                                                                                                                                                                                                                                                                  
housebill  SG5000002000-01-01ADD20100000000110000000SGD                                                                                                                                                                                                                                                     
housebill  SG5000002000-01-01ADD20500000000010000000SGD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}

		[TestDate(2017, 12, 20, 12, 15, 20)]
		public void TestShipmentCountryDetails_TradeNetDeclaration_CustomCharge()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var template = Factory.New<ProcessTaskTemplate>();
				template.P0_Name = "Snakey";
				template.P0_ProcessType = "BRK";
				var column1 = template.GenCustomColumnDefinitions.AddNew();
				column1.XC_Name = "Code 1";
				column1.XC_Type = AddOnColumnDataType.Codes.String;
				var column2 = template.GenCustomColumnDefinitions.AddNew();
				column2.XC_Name = "Charge 1";
				column2.XC_Type = AddOnColumnDataType.Codes.Decimal;
				var notifications = new NotificationBufferForTesting();
				var exporter = new BISIFileExporterForTest(notifications);
				using (var memoryStream = new MemoryStream())
				{
					using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
					{
						UPEDataRegistry.Instance.EnableUPECustomisations = true;
						var dec = Factory.NewWithValidTestData<JobDeclaration>();
						dec.JE_IsCancelled = ZBool.False;
						dec.JE_DeclarationReference = "dec ref";
						dec.JE_HouseBill = "housebill";
						dec.JE_MasterBill = "masterbill";
						dec.JE_DateOfArrival = new ZDateTime(2000, 1, 1);
						dec.JE_RS_NKServiceLevel = "slv";
						dec.JE_TotalNoOfPacks = 110;
						dec.JE_GB = Env.CurrentBranchPK;
						var importer = Factory.NewWithValidTestData<OrgHeader>();
						importer.OH_Code = "importer";
						importer.OH_FullName = "importer fullname";
						importer.MainAddress.OA_Address1 = "importer address 1";
						importer.MainAddress.OA_Address2 = "importer address 2";
						importer.MainAddress.OA_City = "importer city";
						importer.MainAddress.OA_Phone = "importer phone";
						dec.JE_OH_Importer = importer.PK;
						dec.JE_VoyageFlightNo = "Flight123";
						dec.JE_MessageType = MessageTypeCodeList.Codes.IPT;
						dec.JE_MasterBill = "123";
						var entry = dec.CustomsEntryHeaders.AddNew();
						var entryLine = entry.MergedLines.AddNew();
						var invoice = dec.Invoices.AddNew();
						invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
						invoice.JZ_InvoiceCurrExRate = 1.39m;
						var invoiceline = entryLine.InvoiceLines.AddNew();
						invoiceline.JI_JZ = invoice.PK;
						var cusEntryNum = Factory.New<CusEntryNumber>();
						cusEntryNum.CE_EntryNum = "MC123";
						cusEntryNum.CE_EntryIsSystemGenerated = true;
						cusEntryNum.CE_ParentID = entry.PK;
						cusEntryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
						cusEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
						cusEntryNum.CE_EntryType = "CER";
						entry.EntryNumber = "123";
						var entryPayInfo = entry.EntryPayInfos.AddNew();
						entryPayInfo.C9_PaymentReference = "123";
						entryPayInfo.C9_PaymentParty = UPEOrgRematch.OrgTypes.Importer;
						dec.Logs.AddNew(Events.CustomsEntryStatus, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, ZDateTimeOffset.Now);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.GST, 10m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.Duty, 110m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.Excise, 210m);
						entryLine.Fees.AddOrUpdate(Customs.SG.Registry.EntryChargeTypeList.Codes.OtherTax, 310m);
						var customPropertiesCollection = new UserDefinedPropertyCollection(dec);
						customPropertiesCollection.Add(new ProcessTaskTemplateMatches(template));
						foreach (var column in customPropertiesCollection)
						{
							if (column.Info.Type == typeof(ZString))
							{
								column.TrySetValue(dec, new ZString("100"));
							}
							else
							{
								column.TrySetValue(dec, new ZDecimal(10));
							}
						}

						var tradenet = new TradeNetIShipmentDataProxy("DOK", ZDateTime.UtcNow, dec);
						var recordList = new BISIUploadRecordList();
						recordList.AddFromShipment(tradenet);
						exporter.DoExport(streamWriter, 1, recordList);
						streamWriter.Flush();
						memoryStream.Seek(0, SeekOrigin.Begin);
						using (var streamReader = new StreamReader(memoryStream))
						{
							var expectedResult =
@"2017-12-2012:15:2000000100001000008                                                                                                                                                                                                                                                                
housebill  SG1000002000-01-01ADDhousebill            01123           00000000000000000000000000SGD000139000000002  123        2017-12-20                                                                                                                                                                    
housebill  SG3000002000-01-01ADD004G04                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD005123                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD0062017-12-20                                                                                                                                                                                                                                                               
housebill  SG3000002000-01-01ADD00701                                                                                                                                                                                                                                                                       
housebill  SG3000002000-01-01ADD009123                                                                                                                                                                                                                                                                      
housebill  SG3000002000-01-01ADD0100001LOT                                                                                                                                                                                                                                                                  
housebill  SG5000002000-01-01ADD10000000000010000000SGD                                                                                                                                                                                                                                                     
";
							var actualResult = streamReader.ReadToEnd();
							AssertEquals(expectedResult, actualResult);
						}
					}
				}
			}
		}
	}
}
