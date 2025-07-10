using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5CommonLineWrapperTest : WrapperHelperTest<G5CommonLineWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if item is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "tempItem"), () => GetWrapper(null));
		}

		public void TestLineNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Prepreq: API_LineNo is 1", (ZShort)1, item.API_LineNo);

				AssertEquals("Expected filled LineNumber", "1", wrapper.LineNumber);
			});
		}

		public void TestPreviousDocument()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<NullReferenceException>("Expected null PreviousDocument when no document declared", () => wrapper.PreviousDocument.ToString());

				var prevDocBill = bill.PreviousDocuments.AddNew();
				prevDocBill.CSI_Code = "AAAA";

				wrapper = GetWrapper(item);
				var document = wrapper.PreviousDocument;
				AssertNotNull("Expected filled PreviousDocument with document from Bill when no document is declared in item", document);
				AssertSame("Cached PreviousDocument", wrapper.PreviousDocument, document);
				AssertEquals("Expected filled PreviousDocument.PreviousGeneric.Name", "AAAA", document.PreviousGeneric.Name);

				var prevDocItem = item.PreviousDocuments.AddNew();
				prevDocItem.CSI_Code = "BBBB";

				wrapper = GetWrapper(item);
				document = wrapper.PreviousDocument;
				AssertNotNull("Expected filled PreviousDocument with document from Item when declared, even if there are documents declared in the Bill", document);
				AssertSame("Cached PreviousDocument", wrapper.PreviousDocument, document);
				AssertEquals("Expected filled PreviousDocument.PreviousGeneric.Name", "BBBB", document.PreviousGeneric.Name);
			});
		}

		public void TestPackagesNum()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PackagesNum", 0, wrapper.PackagesNum);

				AddPackagesWithContainers();

				AssertEquals("Expected filled PackagesNum with full packages", 19, wrapper.PackagesNum);
			});
		}

		public void TestPackages()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Packages list", false, wrapper.Packages.Any());

				AddPackagesWithContainers();

				wrapper = GetWrapper(item);
				var packages = wrapper.Packages;
				AssertEquals("Expected filled Packages list", 5, packages.Count);
				AssertSame("Cached Packages", wrapper.Packages, packages);
				AssertContainsExactElementsInAnyOrder("Expected Packages with correct data",
					new (ZString, ZLong, ZString)[]
					{
						("BX", 10, "Marks1"),
						("NE", 4, "Marks2"),
						("FR", 3, "Marks3"),
						("VG", 2, "Marks4"),
						("EE", 0, "Marks6")
					},
					packages.Select(x => (x.ElementsType, x.NumberOfElements, x.Tag)).ToArray());
			});
		}

		public void TestGrossWeightInKG()
		{
			CombineAssertions(() =>
			{
				item.API_GrossWeight = 200.4455M;
				item.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Expected filled GrossWeightInKG when weight > 1 rounded to the upper integer unit", 201M, wrapper.GrossWeightInKG);

				item.API_GrossWeight = 0.9886678M;
				AssertEquals("Expected filled GrossWeightInKG when weight < 1", 0.989M, wrapper.GrossWeightInKG);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected empty TransportDocument", wrapper.TransportDocument);

				var addInfo1 = item.AdditionalInfos.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "INF";

				var addInfo2 = item.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "REF";

				var addInfo3 = item.AdditionalInfos.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "TRA";

				wrapper = GetWrapper(item);
				var documents = wrapper.TransportDocument;
				AssertNotNull("Expected filled TransportDocument (document with TRA)", documents);
				AssertSame("Cached TransportDocument", wrapper.TransportDocument, documents);
				AssertEquals("Expected correct TransportDocument.Name", "9003", wrapper.TransportDocument.Name);
			});
		}

		public void TestUCRCode()
		{
			item.UCR = "UCRCode";
			AssertEquals("Expected filled UCRCode", "UCRCode", wrapper.UCRCode);
		}

		public void TestCommodityCode()
		{
			CombineAssertions(() =>
			{
				item.API_Tariff = "2203001023";
				AssertEquals("Expected filled CommodityCode trimmed to 8 chars", "22030010", wrapper.CommodityCode);

				item.API_Tariff = "220300";
				AssertEquals("Expected filled CommodityCode not trimmed when less than 8 chars", "220300", wrapper.CommodityCode);
			});
		}

		public void TestGoodsDescription()
		{
			item.API_GoodsDescription = "Description";
			AssertEquals("Expected filled GoodsDescription", "Description", wrapper.GoodsDescription);
		}

		public void TestCusCode()
		{
			item.API_ChemicalSubstanceCode = "1234";
			AssertEquals("Expected filled CusCode", "1234", wrapper.CusCode);
		}

		public void TestTransportEquipments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportEquipments list", false, wrapper.TransportEquipments.Any());

				AddPackagesWithContainers();

				wrapper = GetWrapper(item);
				var transportEquipments = wrapper.TransportEquipments;
				AssertEquals("Expected filled TransportEquipments list", 2, transportEquipments.Count);
				AssertSame("Cached TransportEquipments", wrapper.TransportEquipments, transportEquipments);
				AssertContainsExactElementsInAnyOrder("Expected docs with correct Name", new ZString[] { "CONT1", "CONT2" }, transportEquipments.Select(x => x.Id).ToArray());
			});
		}

		public void TestPresentationDateAtOrigin()
		{
			CombineAssertions(() =>
			{
				item.PresentationDate = new ZDateTime(2021, 3, 12);
				header.AMA_CustomsOffice = "ES009999";
				AssertEquals("Expected empty PresentationDateAtOrigin when customsOffice starts with ES", ZDateTime.Empty, wrapper.PresentationDateAtOrigin);

				header.AMA_CustomsOffice = "FR009999";
				AssertEquals("Expected filled PresentationDateAtOrigin when customsOffice doesn't start with ES", new ZDateTime(2021, 3, 12), wrapper.PresentationDateAtOrigin);
			});
		}

		public void TestSupportingDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

				var supdoc1 = item.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";

				var supdoc2 = item.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "9002";

				var supdoc3 = bill.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "9003";

				wrapper = GetWrapper(item);
				var documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments (only included those in item)", 2, documents.Count);
				AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
				AssertContainsExactElementsInAnyOrder("Expected docs with correct Name", new ZString[] { "9001", "9002" }, documents.Select(x => x.Name).ToArray());
			});
		}

		public void TestAdditionalInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalInfo list", 0, wrapper.AdditionalInfo.Count);

				var addInfo1 = item.AdditionalInfos.AddNew();
				addInfo1.CSI_Code = "9001";
				addInfo1.CSI_SubType = "INF";

				var addInfo2 = item.AdditionalInfos.AddNew();
				addInfo2.CSI_Code = "9002";
				addInfo2.CSI_SubType = "INF";

				var addInfo3 = item.AdditionalInfos.AddNew();
				addInfo3.CSI_Code = "9003";
				addInfo3.CSI_SubType = "TRA";

				var addInfo4 = item.AdditionalInfos.AddNew();
				addInfo4.CSI_Code = "9004";
				addInfo4.CSI_SubType = "REF";

				var addInfo5 = bill.AdditionalInfos.AddNew();
				addInfo5.CSI_Code = "9005";
				addInfo5.CSI_SubType = "INF";

				wrapper = GetWrapper(item);
				var documents = wrapper.AdditionalInfo;
				AssertEquals("Expected filled AdditionalInfo (only included those in item and SubType INF)", 2, documents.Count);
				AssertSame("Cached AdditionalInfo", wrapper.AdditionalInfo, documents);
				AssertContainsExactElementsInAnyOrder("Expected docs with correct Name", new ZString[] { "9001", "9002" }, documents.Select(x => x.Name).ToArray());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			bill = header.Bills.AddNew();
			item = bill.PackedItems.AddNew();
			wrapper = GetWrapper(item);
		}

		TemporaryStorageHeader header;
		TemporaryStorageBill bill;
		TemporaryStoragePackedItem item;
		G5CommonLineWrapper wrapper;

		G5CommonLineWrapper GetWrapper(TemporaryStoragePackedItem tempItem) => new G5CommonLineWrapper(tempItem);

		protected override G5CommonLineWrapper GetProvider() => wrapper;

		void AddPackagesWithContainers()
		{
			item.API_LineNo = 2;
			var newItem = bill.PackedItems.AddNew();
			newItem.API_LineNo = 1;

			var cont1 = header.Containers.AddNew();
			cont1.ACN_ContainerNumber = "CONT1";
			var cont2 = header.Containers.AddNew();
			cont2.ACN_ContainerNumber = "CONT2";
			var cont3 = header.Containers.AddNew();
			cont3.ACN_ContainerNumber = "CONT3";

			var package1 = bill.Packs.AddNew();
			package1.APA_PackUQ = "BX";
			package1.APA_MarksAndNumbers = "Marks1";
			package1.APA_PackQty = 5;
			package1.ContainerPK = cont1.PK;
			var package2 = bill.Packs.AddNew();
			package2.APA_PackUQ = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
			package2.APA_MarksAndNumbers = "Marks2";
			package2.APA_PackQty = 4;
			package2.ContainerPK = cont1.PK;
			var package3 = bill.Packs.AddNew();
			package3.APA_PackUQ = "FR";
			package3.APA_MarksAndNumbers = "Marks3";
			package3.APA_PackQty = 3;
			package3.ContainerPK = cont2.PK;
			var package4 = bill.Packs.AddNew();
			package4.APA_PackUQ = "VG";
			package4.APA_MarksAndNumbers = "Marks4";
			package4.APA_PackQty = 2;
			var package5 = bill.Packs.AddNew();
			package5.APA_PackUQ = "AA";
			package5.APA_MarksAndNumbers = "Marks5";
			package5.APA_PackQty = 2;
			var package6 = bill.Packs.AddNew();
			package6.APA_PackUQ = "BX";
			package6.APA_MarksAndNumbers = "Marks1";
			package6.APA_PackQty = 5;
			package6.ContainerPK = cont1.PK;
			var package7 = bill.Packs.AddNew();
			package7.APA_PackUQ = "EE";
			package7.APA_MarksAndNumbers = "Marks6";
			package7.APA_PackQty = 20;

			var linkPackage1 = item.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package1);
			linkPackage1.IsLinked = true;
			var linkPackage2 = item.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package2);
			linkPackage2.IsLinked = true;
			var linkPackage3 = item.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package3);
			linkPackage3.IsLinked = true;
			var linkPackage4 = item.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package4);
			linkPackage4.IsLinked = true;
			var linkPackage5 = item.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package5);
			linkPackage5.IsLinked = false;
			var linkPackage6 = item.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package6);
			linkPackage6.IsLinked = true;
			var linkPackage7 = item.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package7);
			linkPackage7.IsLinked = true;
			var linkPackage8 = newItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package7);
			linkPackage8.IsLinked = true;
		}
	}
}
