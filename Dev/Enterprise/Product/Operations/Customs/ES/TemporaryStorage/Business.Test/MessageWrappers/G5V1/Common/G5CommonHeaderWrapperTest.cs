using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5CommonHeaderWrapperTest : WrapperHelperTest<G5CommonHeaderWrapper>
	{
		public void TestOriginCustomsOffice()
		{
			header.AMA_CustomsOffice = "ES009999";
			AssertEquals("Expected filled OriginCustomsOffice", "ES009999", wrapper.OriginCustomsOffice);
		}

		public void TestGoodsLocationOrigin()
		{
			var locationOfGoods = wrapper.GoodsLocationOrigin;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GoodsLocationOrigin", locationOfGoods);
				AssertSame("Cached GoodsLocationOrigin", wrapper.GoodsLocationOrigin, locationOfGoods);
			});
		}

		public void TestDestinationCustomsOffice()
		{
			var codeData = Factory.New<TemporaryStorageOfficeCode>();
			codeData.CY_ParentID = header.PK;
			codeData.CY_ParentTableCode = header.TablePrefix;
			codeData.CY_Type = "EUO";
			codeData.CY_Code = "G5D";
			codeData.CY_Data = "7758258";

			var codeData2 = Factory.New<TemporaryStorageOfficeCode>();
			codeData2.CY_ParentID = header.PK;
			codeData2.CY_ParentTableCode = header.TablePrefix;
			codeData2.CY_Type = "EUO";
			codeData2.CY_Code = "DES";
			codeData2.CY_Data = "1212121";

			AssertEquals("Expected filled DestinationCustomsOffice with correct G5D OfficeCode", "7758258", wrapper.DestinationCustomsOffice);

			header.DestinationCustomsOffice = "ES009999";
			AssertEquals("Expected filled DestinationCustomsOffice with new code added to property DestinationCustomsOffice", "ES009999", wrapper.DestinationCustomsOffice);
		}

		public void TestGoodsLocationDestination()
		{
			var locationOfGoods = wrapper.GoodsLocationDestination;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GoodsLocationDestination", locationOfGoods);
				AssertSame("Cached GoodsLocationDestination", wrapper.GoodsLocationDestination, locationOfGoods);
			});
		}

		public void TestTSWarehouse()
		{
			header.AuthorizationNumber = "reference";
			AssertEquals("Expected filled TSWarehouse", "reference", wrapper.TSWarehouse);
		}

		public void TestArrivalTransportMeans()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected null ArrivalTransportMeans when no data declared", wrapper.ArrivalTransportMeans);

				header.TransportType = "00";
				header.ArrivalTransportMeansCode = "identification";
				wrapper = GetWrapper(header);
				var arrivalTransportMeans = wrapper.ArrivalTransportMeans;
				AssertNotNull("Expected filled ArrivalTransportMeans when there is data declared", arrivalTransportMeans);
				AssertSame("Cached ArrivalTransportMeans", wrapper.ArrivalTransportMeans, arrivalTransportMeans);

				AssertEquals("Expected filled Type", "00", arrivalTransportMeans.Type);
				AssertEquals("Expected filled Id", "identification", arrivalTransportMeans.Id);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected null TransportDocument when no data declared", wrapper.TransportDocument);

				bill.TypeOfBillDocument = "C665";
				bill.ABL_BillNumber = "reference";
				wrapper = GetWrapper(header);
				var transportDocument = wrapper.TransportDocument;
				AssertNotNull("Expected filled TransportDocument when there is data declared", transportDocument);
				AssertSame("Cached TransportDocument", wrapper.TransportDocument, transportDocument);

				AssertEquals("Expected filled Name", "C665", transportDocument.Name);
				AssertEquals("Expected filled Number", "reference", transportDocument.Number);
			});
		}

		public void TestNullConsignor()
		{
			bill.Delete();
			wrapper = GetWrapper(header);
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignor.ToString());
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				SetUpTestState_List();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				bill.ABL_OA_Shipper = orgAddress.PK;
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Phone = "123456789";
				bill.ABL_ShipperRegNoType = "3";
				bill.ABL_ShipperName = "Name";
				bill.ABL_ShipperStreet1 = "Street 147";
				bill.ABL_ShipperStreet2 = "987 Extra Street 369";
				bill.ABL_ShipperState = "M";
				bill.ABL_RN_NKShipperCountry = "37";
				bill.ABL_ShipperPostcode = "28010";
				bill.ABL_ShipperCity = "madrid";
				bill.ABL_ShipperPhone = "mail.mail@mail.com";

				wrapper = GetWrapper(header);
				var consignor = wrapper.Consignor;
				AssertNotNull("Expected filled Consignor", consignor);
				AssertSame("Cached Consignor", wrapper.Consignor, consignor);

				AssertEquals("Expected filled Id", "FR22222222", consignor.Id);
				AssertEquals("Expected filled Name", "Name", consignor.Name);
				AssertEquals("Expected filled Type", "3", consignor.Type);
				AssertEquals("Expected filled Street", "Street ", consignor.Street);
				AssertEquals("Expected filled StreetAddLine", "987 Extra Street 369", consignor.StreetAddLine);
				AssertEquals("Expected filled Number", "147", consignor.Number);
				AssertEquals("Expected filled POBox", ZString.Empty, consignor.POBox);
				AssertEquals("Expected filled State", "MADRID", consignor.State);
				AssertEquals("Expected filled Country", "37", consignor.Country);
				AssertEquals("Expected filled PostCode", "28010", consignor.PostCode);
				AssertEquals("Expected filled City", "madrid", consignor.City);
				AssertEquals("Expected filled CommunicationType", "TE", consignor.CommunicationType);
				AssertEquals("Expected filled CommunicationId", "123456789", consignor.CommunicationId);
			});
		}
		public void TestNullConsignee()
		{
			bill.Delete();
			wrapper = GetWrapper(header);
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignee.ToString());
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				SetUpTestState_List();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				bill.ABL_OA_Consignee = orgAddress.PK;
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Email = "mail.mail@mail.com";
				bill.ABL_ConsigneeRegNoType = "3";
				bill.ABL_ConsigneeName = "Name";
				bill.ABL_ConsigneeStreet1 = "147 Street";
				bill.ABL_ConsigneeStreet2 = "987 Extra Street n369";
				bill.ABL_ConsigneeState = "B";
				bill.ABL_RN_NKConsigneeCountry = "37";
				bill.ABL_ConsigneePostcode = "28010";
				bill.ABL_ConsigneeCity = "madrid";
				bill.ABL_ConsigneePhone = "123456";

				wrapper = GetWrapper(header);
				var consignee = wrapper.Consignee;
				AssertNotNull("Expected filled Consignee", consignee);
				AssertSame("Cached Consignee", wrapper.Consignee, consignee);

				AssertEquals("Expected filled Id", "FR22222222", consignee.Id);
				AssertEquals("Expected filled Name", "Name", consignee.Name);
				AssertEquals("Expected filled Type", "3", consignee.Type);
				AssertEquals("Expected filled Street", "147 Street", consignee.Street);
				AssertEquals("Expected filled StreetAddLine", "987 Extra Street n", consignee.StreetAddLine);
				AssertEquals("Expected filled Number", "369", consignee.Number);
				AssertEquals("Expected filled POBox", ZString.Empty, consignee.POBox);
				AssertEquals("Expected filled State", "BARCELONA", consignee.State);
				AssertEquals("Expected filled Country", "37", consignee.Country);
				AssertEquals("Expected filled PostCode", "28010", consignee.PostCode);
				AssertEquals("Expected filled City", "madrid", consignee.City);
				AssertEquals("Expected filled CommunicationType", "EM", consignee.CommunicationType);
				AssertEquals("Expected filled CommunicationId", "mail.mail@mail.com", consignee.CommunicationId);
			});
		}

		public void TestSupportingDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

				var supdoc1 = bill.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_ReferenceNumber = "REF1";
				supdoc1.CSI_Description = "DESC1";

				var supdoc2 = bill.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "9002";
				supdoc2.CSI_ReferenceNumber = "REF2";
				supdoc2.CSI_Description = "DESC2";

				var item = bill.PackedItems.AddNew();
				var supdoc3 = item.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "9003";
				supdoc3.CSI_ReferenceNumber = "REF3";
				supdoc3.CSI_Description = "DESC3";

				wrapper = GetWrapper(header);
				var documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments (only included those in bill)", 2, documents.Count);
				AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
				AssertContainsExactElementsInAnyOrder("Expected docs with correct Name and Number",
					new (ZString, ZString)[]
					{
						("9001", "REF1"),
						("9002", "REF2")
					}, documents.Select(x => (x.Name, x.Number)).ToArray());
			});
		}

		public void TestTotalLinesNum()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TotalLinesNum", "0", wrapper.TotalLinesNum);

				var packedItem1 = bill.PackedItems.AddNew();
				var packedItem2 = bill.PackedItems.AddNew();
				var packedItem3 = bill.PackedItems.AddNew();
				packedItem3.IsMissing = true;

				AssertEquals("Expected 2 TotalLinesNum", "2", wrapper.TotalLinesNum);
			});
		}

		public void TestTotalPackagesNum()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TotalPackagesNum", 0, wrapper.TotalPackagesNum);

				var package1 = bill.Packs.AddNew();
				package1.APA_PackUQ = "BX";
				package1.APA_PackQty = 5;
				var package2 = bill.Packs.AddNew();
				package2.APA_PackUQ = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
				package2.APA_PackQty = 4;
				var package3 = bill.Packs.AddNew();
				package3.APA_PackUQ = "FR";
				package3.APA_PackQty = 3;
				var package4 = bill.Packs.AddNew();
				package4.APA_PackUQ = "VG";
				package4.APA_PackQty = 3;

				var item1 = bill.PackedItems.AddNew();
				var linkPackage1 = item1.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package1);
				linkPackage1.IsLinked = true;
				var linkPackage2 = item1.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package2);
				linkPackage2.IsLinked = true;

				var item2 = bill.PackedItems.AddNew();
				item2.IsMissing = true;
				var linkPackage3 = item2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package3);
				linkPackage3.IsLinked = true;
				var linkPackage4 = item2.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package4);
				linkPackage4.IsLinked = true;

				AssertEquals("Expected filled TotalPackagesNum with full packages", 9, wrapper.TotalPackagesNum);
			});
		}

		public void TestTotalGrossWeightInKG()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TotalGrossWeightInKG", 0m, wrapper.TotalGrossWeightInKG);

				var item1 = bill.PackedItems.AddNew();
				item1.API_GrossWeight = 500.20m;
				item1.API_GrossWeightUQ = "KG";

				var item2 = bill.PackedItems.AddNew();
				item2.API_GrossWeight = 100000.00m;
				item2.API_GrossWeightUQ = "G";

				var item3 = bill.PackedItems.AddNew();
				item3.API_GrossWeight = 200200000.00m;
				item3.API_GrossWeightUQ = Core.Constants.Weight.Milligrams;

				var item4 = bill.PackedItems.AddNew();
				item4.API_GrossWeight = 2002.00m;
				item4.API_GrossWeightUQ = Core.Constants.Weight.Hectograms;
				item4.IsMissing = true;

				AssertEquals("Expected filled TotalGrossWeightInKG", 802m, wrapper.TotalGrossWeightInKG);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			bill = header.Bills.FirstOrDefault() ?? header.Bills.AddNew();
			wrapper = GetWrapper(header);
		}

		TemporaryStorageHeader header;
		TemporaryStorageBill bill;
		G5CommonHeaderWrapper wrapper;

		G5CommonHeaderWrapper GetWrapper(TemporaryStorageHeader header) => new G5CommonHeaderWrapper(header);

		protected override G5CommonHeaderWrapper GetProvider() => wrapper;

		public void SetUpTestState_List()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "37";
			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "M";
			state1.RW_Description = "MADRID";
			state2.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_Code = "B";
			state2.RW_Description = "BARCELONA";
		}
	}
}
