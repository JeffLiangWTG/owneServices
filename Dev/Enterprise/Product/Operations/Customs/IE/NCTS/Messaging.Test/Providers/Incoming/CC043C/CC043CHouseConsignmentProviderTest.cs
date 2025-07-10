using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CHouseConsignmentProvider))]
	class CC043CHouseConsignmentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("houseConsignment missing", () => new CC043CHouseConsignmentProvider(null));
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

		public void TestTransportDocument()
		{
			AssertType<CC043CDocumentProvider[]>(provider.TransportDocument);
			provider = new CC043CHouseConsignmentProvider(new HouseConsignmentType04());
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals("Empty", 0, provider.TransportDocument.Count);
		}

		public void TestAdditionalReference()
		{
			AssertType<CC043CDocumentProvider[]>(provider.AdditionalReference);
			provider = new CC043CHouseConsignmentProvider(new HouseConsignmentType04());
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals("Empty", 0, provider.AdditionalReference.Count);
		}

		public void TestConsignmentItem()
		{
			AssertType<CC043CHouseConsignmentItemProvider[]>(provider.ConsignmentItem);
			provider = new CC043CHouseConsignmentProvider(new HouseConsignmentType04());
			AssertType<CC043CHouseConsignmentItemProvider[]>(provider.ConsignmentItem);
			AssertEquals("Empty", 0, provider.ConsignmentItem.Count);
		}

		public void TestSupportingDocument()
		{
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals(2, provider.SupportingDocument.Count);
			provider = new CC043CHouseConsignmentProvider(new HouseConsignmentType04());
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocument);
			AssertEquals("Empty", 0, provider.SupportingDocument.Count);
		}

		public void TestDepartureTransportMeans()
		{
			AssertType<CC043CDepartureTransportMeansProvider[]>(provider.DepartureTransportMeans);
			AssertEquals(2, provider.DepartureTransportMeans.Count);
			provider = new CC043CHouseConsignmentProvider(new HouseConsignmentType04());
			AssertType<CC043CDepartureTransportMeansProvider[]>(provider.DepartureTransportMeans);
			AssertEquals("Empty", 0, provider.DepartureTransportMeans.Count);
		}

		protected override void SetUp()
		{
			provider = new CC043CHouseConsignmentProvider(new HouseConsignmentType04()
			{
				GrossMass = 123.45m,
				SecurityIndicatorFromExportDeclaration = "D",
				TransportDocument = new Collection<TransportDocumentType02>()
				{
					new TransportDocumentType02()
					{
						Type = "AAA",
						ReferenceNumber = "BBB",
						SequenceNumber = "1"
					}
				},
				AdditionalInformation = new Collection<AdditionalInformationType02>()
				{
					new AdditionalInformationType02()
					{
						SequenceNumber = "1",
						Code = "A",
						Text = "B",
					}
				},
				ConsignmentItem = new Collection<ConsignmentItemType04>()
				{
					new ConsignmentItemType04()
					{
						CountryOfDestination = "IE",
						DeclarationType = "D",
					}
				},
				SupportingDocument = new Collection<SupportingDocumentType02>()
				{
					new SupportingDocumentType02()
					{
						SequenceNumber = "1",
						Type = "001",
						ComplementOfInformation = "Text",
						ReferenceNumber = "Reference",
					},
					new SupportingDocumentType02()
					{
						SequenceNumber = "2",
						Type = "202",
						ComplementOfInformation = "Test text",
						ReferenceNumber = "Number",
					}
				},
				DepartureTransportMeans = new Collection<DepartureTransportMeansType02>
				{
					new DepartureTransportMeansType02
					{
						SequenceNumber = "1",
						TypeOfIdentification = "11",
						IdentificationNumber = "3",
						Nationality = "IE",
					},
					new DepartureTransportMeansType02
					{
						SequenceNumber = "2",
						TypeOfIdentification = "20",
						IdentificationNumber = "231",
						Nationality = "IE",
					},
				},
			});
		}
		CC043CHouseConsignmentProvider provider;
	}
}
