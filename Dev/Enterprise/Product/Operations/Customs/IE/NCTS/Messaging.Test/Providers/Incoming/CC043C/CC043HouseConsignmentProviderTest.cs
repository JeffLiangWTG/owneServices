using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043HouseConsignmentProvider))]
	class CC043HouseConsignmentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("houseConsignment missing", () => new CC043HouseConsignmentProvider(null));
			});
		}

		public void TestGrossMass()
		{
			AssertEquals("Gross mass", 123.45m, provider.GrossMass);
		}

		public void TestSecurityIndicatorFromExportDeclaration()
		{
			AssertEquals("Security indicator", "D", provider.SecurityIndicatorFromExportDeclaration);
		}

		public void TestSupportingDocument()
		{
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			provider = new CC043HouseConsignmentProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.HouseConsignmentType04());
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals("Empty", 0, provider.SupportingDocument.Count);
		}

		public void TestTransportDocument()
		{
			AssertType<CC043CDocumentProvider[]>(provider.TransportDocument);
			provider = new CC043HouseConsignmentProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.HouseConsignmentType04());
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals("Empty", 0, provider.TransportDocument.Count);
		}

		public void TestAdditionalReference()
		{
			AssertType<CC043CDocumentProvider[]>(provider.AdditionalReference);
			provider = new CC043HouseConsignmentProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.HouseConsignmentType04());
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals("Empty", 0, provider.AdditionalReference.Count);
		}

		public void TestConsignmentItem()
		{
			AssertType<CC043CHouseConsignmentItemProvider[]>(provider.ConsignmentItem);
			provider = new CC043HouseConsignmentProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.HouseConsignmentType04());
			AssertType<CC043CHouseConsignmentItemProvider[]>(provider.ConsignmentItem);
			AssertEquals("Empty", 0, provider.ConsignmentItem.Count);
		}

		protected override void SetUp()
		{
			provider = new CC043HouseConsignmentProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.HouseConsignmentType04()
			{
				GrossMass = 123.45m,
				SecurityIndicatorFromExportDeclaration = "D",
				TransportDocument = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.TransportDocumentType02>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.TransportDocumentType02()
					{
						Type = "AAA",
						ReferenceNumber = "BBB",
						SequenceNumber = "1"
					}
				},
				AdditionalInformation = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.AdditionalInformationType02>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.AdditionalInformationType02()
					{
						SequenceNumber = "1",
						Code = "A",
						Text = "B",
					}
				},
				ConsignmentItem = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.ConsignmentItemType04()
					{
						CountryOfDestination = "IE",
						DeclarationType = "D",
					}
				}
			});
		}
		CC043HouseConsignmentProvider provider;
	}
}
