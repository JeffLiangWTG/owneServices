using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CHouseConsignmentItemProvider))]
	class CC043CConsignmentItemProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("item missing", () => new CC043CHouseConsignmentItemProvider(null));
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("Goods Item Number", "1", provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("Declaration Goods Item Number", "2", provider.DeclarationGoodsItemNumber);
		}

		public void TestCommodity()
		{
			AssertType<CC043CCommodityProvider>(provider.Commodity);
		}

		public void TestDeclarationType()
		{
			AssertEquals("Declaration Type", "A1", provider.DeclarationType);
		}

		public void TestCountryOfDestination()
		{
			AssertEquals("Country of Destination", "IE", provider.CountryOfDestination);
		}

		public void TestPackaging()
		{
			AssertType<CC043CPackagingProvider[]>(provider.Packaging);
			provider = new CC043CHouseConsignmentItemProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04());
			AssertType<CC043CPackagingProvider[]>(provider.Packaging);
			AssertEquals("Empty", 0, provider.Packaging.Count);
		}

		public void TestPreviousDocument()
		{
			AssertType<CC043CPreviousDocumentWithGoodsItemNumberProvider[]>(provider.PreviousDocument);
			provider = new CC043CHouseConsignmentItemProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04());
			AssertType<CC043CPreviousDocumentWithGoodsItemNumberProvider[]>(provider.PreviousDocument);
			AssertEquals("Empty", 0, provider.PreviousDocument.Count);
		}

		public void TestSupportingDocument()
		{
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			provider = new CC043CHouseConsignmentItemProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04());
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals("Empty", 0, provider.SupportingDocument.Count);
		}

		public void TestTransportDocument()
		{
			AssertType<CC043CDocumentProvider[]>(provider.TransportDocument);
			provider = new CC043CHouseConsignmentItemProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04());
			AssertType<CC043CDocumentProvider[]>(provider.TransportDocument);
			AssertEquals("Empty", 0, provider.TransportDocument.Count);
		}

		public void TestAdditionalReference()
		{
			AssertType<CC043CDocumentProvider[]>(provider.AdditionalReference);
			provider = new CC043CHouseConsignmentItemProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04());
			AssertType<CC043CDocumentProvider[]>(provider.AdditionalReference);
			AssertEquals("Empty", 0, provider.AdditionalReference.Count);
		}

		public void TestAdditionalInformation()
		{
			AssertType<CC043AdditionalInformationProvider[]>(provider.AdditionalInformation);
			provider = new CC043CHouseConsignmentItemProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04());
			AssertType<CC043AdditionalInformationProvider[]>(provider.AdditionalInformation);
			AssertEquals("Empty", 0, provider.AdditionalInformation.Count);
		}

		protected override void SetUp()
		{
			provider = new CC043CHouseConsignmentItemProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04()
			{
				GoodsItemNumber = "1",
				DeclarationGoodsItemNumber = "2",
				DeclarationType = "A1",
				CountryOfDestination = "IE",
				Packaging = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.PackagingType02>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.PackagingType02
					{
						NumberOfPackages = "1",
						SequenceNumber = "1",
						ShippingMarks = "Marks 1",
						TypeOfPackages = "BOX",
					},
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.PackagingType02
					{
						NumberOfPackages = "1",
						SequenceNumber = "2",
						ShippingMarks = "Marks 2",
						TypeOfPackages = "BAG",
					}
				},
				Commodity = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.CommodityType08()
				{
					DescriptionOfGoods = "Test item 1",
					CusCode = "TEST",
				},
				SupportingDocument = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.SupportingDocumentType02>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.SupportingDocumentType02()
					{
						Type = "TRA",
						ReferenceNumber = "123",
						SequenceNumber = "1",
						ComplementOfInformation = "Test"
					}
				},
				TransportDocument = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.TransportDocumentType02>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.TransportDocumentType02()
					{
						SequenceNumber = "1",
						Type = "ABC",
						ReferenceNumber = "TEST989"
					}
				},
				AdditionalReference = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.AdditionalReferenceType02>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.AdditionalReferenceType02()
					{
						SequenceNumber = "1",
						ReferenceNumber = "FFFF",
						Type = "ZZ1"
					}
				},
				AdditionalInformation = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.AdditionalInformationType02>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.AdditionalInformationType02()
					{
						SequenceNumber = "1",
						Code = "WXY9",
						Text = "Text"
					}
				}
			});
		}
		CC043CHouseConsignmentItemProvider provider;
	}
}
