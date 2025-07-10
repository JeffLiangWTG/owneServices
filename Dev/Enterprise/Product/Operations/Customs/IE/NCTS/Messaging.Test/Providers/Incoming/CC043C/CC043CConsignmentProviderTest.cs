using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CConsignmentProvider))]
	class CC043CConsignmentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("consignment missing", () => new CC043CConsignmentProvider(null));
			});
		}

		public void TestGrossMass()
		{
			AssertEquals("GrossMass", 1584.5m, provider.GrossMass);
		}

		public void TestContainerIndicator()
		{
			AssertEquals("ContainerIndicator", ZBool.True, provider.ContainerIndicator);
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Consignor Name", "Consignor Name", provider.Consignor.Name);
				AssertEquals("Consignor IdentificationNumber", "C002", provider.Consignor.IdentificationNumber);
				AssertEquals("Consignor Address Street and Number", "567 WHERE ST", provider.Consignor.Address.StreetAndNumber);
				AssertEquals("Consignor Address City", "Dublin", provider.Consignor.Address.City);
				AssertEquals("Consignor Address Postcode", "D1 234", provider.Consignor.Address.Postcode);
				AssertEquals("Consignor Address Country", "IE", provider.Consignor.Address.Country);
			});
		}

		public void TestConsignor_Null()
		{
			var consignorNullProvider = new CC043CConsignmentProvider(new ConsignmentType05());
			AssertNull(consignorNullProvider.Consignor);
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Consignee Name", "Consignee Name", provider.Consignee.Name);
				AssertEquals("Consignee IdentificationNumber", "C001", provider.Consignee.IdentificationNumber);
				AssertEquals("Consignee Address Street and Number", "89 WHERE ST", provider.Consignee.Address.StreetAndNumber);
				AssertEquals("Consignee Address City", "Dublin", provider.Consignee.Address.City);
				AssertEquals("Consignee Address Postcode", "D1 289", provider.Consignee.Address.Postcode);
				AssertEquals("Consignee Address Country", "IE", provider.Consignee.Address.Country);
			});
		}

		public void TestConsignee_Null()
		{
			var consigneeNullProvider = new CC043CConsignmentProvider(new ConsignmentType05());
			AssertNull(consigneeNullProvider.Consignee);
		}

		public void TestTransportEquipment()
		{
			AssertType<CC043CTransportEquipmentProvider[]>(provider.TransportEquipment);
		}

		public void TestDepartureTransportMeans()
		{
			AssertType<CC043CDepartureTransportMeansProvider[]>(provider.DepartureTransportMeans);
		}

		public void TestSupportingDocuments()
		{
			AssertType<CC043CSupportingDocumentProvider[]>(provider.SupportingDocuments);
		}

		public void TestTransportDocuments()
		{
			AssertType<CC043CDocumentProvider[]>(provider.TransportDocuments);
		}

		public void TestAdditionalReferences()
		{
			AssertType<CC043CDocumentProvider[]>(provider.AdditionalReferences);
		}

		public void TestInlandModeOfTransport()
		{
			AssertEquals("Inland mode of transport", "MOT", provider.InlandModeOfTransport);
		}

		public void TestAdditionalInformation()
		{
			AssertType<CC043AdditionalInformationProvider[]>(provider.AdditionalInformation);
			AssertEquals("Additional information count", 2, provider.AdditionalInformation.Count);
		}

		public void TestPreviousDocument()
		{
			AssertType<CC043CPreviousDocumentProvider[]>(provider.PreviousDocument);
			AssertEquals("Previous document count", 2, provider.PreviousDocument.Count);
		}

		public void TestIncident()
		{
			AssertType<CC043CIncidentProvider[]>(provider.Incidents);
			AssertEquals("Incident count", 2, provider.Incidents.Count);
		}

		public void TestHouseConsignment()
		{
			AssertType<CC043CHouseConsignmentProvider[]>(provider.HouseConsignment);
			AssertEquals("House consignment count", 1, provider.HouseConsignment.Count);
		}

		protected override void SetUp()
		{
			provider = new CC043CConsignmentProvider(new ConsignmentType05()
			{
				GrossMass = 1584.5m,
				ContainerIndicator = CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl.Flag.Item1,
				InlandModeOfTransport = "MOT",
				Consignee = new ConsigneeType04
				{
					IdentificationNumber = "C001",
					Name = "Consignee Name",
					Address = new AddressType07
					{
						StreetAndNumber = "89 WHERE ST",
						City = "Dublin",
						Postcode = "D1 289",
						Country = "IE",
					}
				},
				Consignor = new ConsignorType05
				{
					IdentificationNumber = "C002",
					Name = "Consignor Name",
					Address = new AddressType07
					{
						StreetAndNumber = "567 WHERE ST",
						City = "Dublin",
						Postcode = "D1 234",
						Country = "IE",
					}
				},
				AdditionalInformation = new System.Collections.ObjectModel.Collection<AdditionalInformationType02>
				{
					new AdditionalInformationType02()
					{
						Code = "AAA",
						Text = "Test text 1",
						SequenceNumber = "1",
					},
					new AdditionalInformationType02()
					{
						Code = "BBB",
						Text = "Test text 2",
						SequenceNumber = "2",
					}
				},
				PreviousDocument = new System.Collections.ObjectModel.Collection<PreviousDocumentType06>()
				{
					new PreviousDocumentType06()
					{
						SequenceNumber = "1",
						Type = "ZZ1",
						ReferenceNumber = "12345",
						ComplementOfInformation = "Information placeholder 1",
					},
					new PreviousDocumentType06()
					{
						SequenceNumber = "2",
						Type = "XXX",
						ReferenceNumber = "98765",
						ComplementOfInformation = "Information placeholder 2",
					}
				},
				Incident = new System.Collections.ObjectModel.Collection<IncidentType04>()
				{
					new IncidentType04()
					{
						SequenceNumber = "1",
						Code = "001",
						Text = "Incident 1 text",
						Endorsement = new EndorsementType03()
						{
							Authority = "End Auth",
							Date = new DateTime(2023, 08, 22, 12, 30, 15),
							Place = "Meath",
							Country = "IE",
						},
						Location = new LocationType02()
						{
							QualifierOfIdentification = "U",
							UnLocode = "IEMTH",
							Country = "IE"
						}
					},
					new IncidentType04()
					{
						SequenceNumber = "2",
						Code = "002",
						Text = "Incident 2 text",
						Endorsement = new EndorsementType03()
						{
							Authority = "End Auth 2",
							Date = new DateTime(2023, 08, 22, 12, 48, 50),
							Place = "Dublin",
							Country = "IE",
						},
						Location = new LocationType02()
						{
							QualifierOfIdentification = "U",
							UnLocode = "IEDUB",
							Country = "IE"
						}
					},
				},
				HouseConsignment = new System.Collections.ObjectModel.Collection<HouseConsignmentType04>()
				{
					new HouseConsignmentType04()
					{
						SequenceNumber = "1",
						GrossMass = 12.6m,
						SecurityIndicatorFromExportDeclaration = "A",
						TransportDocument = new System.Collections.ObjectModel.Collection<TransportDocumentType02>()
						{
							new TransportDocumentType02()
							{
								SequenceNumber = "1",
								ReferenceNumber = "ABC123",
								Type = "D",
							},
							new TransportDocumentType02()
							{
								SequenceNumber = "2",
								ReferenceNumber = "DEF456",
								Type = "E",
							}
						},
						AdditionalReference = new System.Collections.ObjectModel.Collection<AdditionalReferenceType03>()
						{
							new AdditionalReferenceType03()
							{
								SequenceNumber = "1",
								ReferenceNumber = "22222",
								Type = "W",
							},
							new AdditionalReferenceType03()
							{
								SequenceNumber = "2",
								ReferenceNumber = "33333",
								Type = "S",
							}
						},
						ConsignmentItem = new System.Collections.ObjectModel.Collection<ConsignmentItemType04>()
						{
							new ConsignmentItemType04()
							{
								GoodsItemNumber = "1",
								DeclarationGoodsItemNumber = "2",
								Commodity = new CommodityType08()
								{
									DescriptionOfGoods = "Test item 1",
									CusCode = "A",
									CommodityCode = new CommodityCodeType05()
									{
										HarmonizedSystemSubHeadingCode = "123456",
										CombinedNomenclatureCode = "12345678"
									},
									GoodsMeasure = new GoodsMeasureType03()
									{
										GrossMass = 50.2m,
										NetMass = 53.75m,
									},
								},
								DeclarationType = "D1",
								CountryOfDestination = "GB",
								Packaging = new System.Collections.ObjectModel.Collection<PackagingType02>()
								{
									new PackagingType02()
									{
										SequenceNumber = "1",
										TypeOfPackages = "BOX",
										NumberOfPackages = "1",
										ShippingMarks = "MARK1",
									},
									new PackagingType02()
									{
										SequenceNumber = "2",
										TypeOfPackages = "BAG",
										NumberOfPackages = "1",
										ShippingMarks = "MARK2",
									}
								},
								SupportingDocument = new System.Collections.ObjectModel.Collection<SupportingDocumentType02>()
								{
									new SupportingDocumentType02()
									{
										SequenceNumber = "1",
										Type = "N380",
										ReferenceNumber = "99999",
										ComplementOfInformation = "Information 1"
									},
									new SupportingDocumentType02()
									{
										SequenceNumber = "2",
										Type = "ZZ23",
										ReferenceNumber = "88888",
										ComplementOfInformation = "Information 2"
									}
								},
								TransportDocument = new System.Collections.ObjectModel.Collection<TransportDocumentType02>()
								{
									new TransportDocumentType02()
									{
										SequenceNumber = "1",
										Type = "F33",
										ReferenceNumber = "24680",
									},
									new TransportDocumentType02()
									{
										SequenceNumber = "2",
										Type = "Z24",
										ReferenceNumber = "13579",
									}
								},
								AdditionalReference = new System.Collections.ObjectModel.Collection<AdditionalReferenceType02>()
								{
									new AdditionalReferenceType02()
									{
										SequenceNumber = "1",
										Type = "A1",
										ReferenceNumber = "AAAAA",
									},
									new AdditionalReferenceType02()
									{
										SequenceNumber = "2",
										Type = "A2",
										ReferenceNumber = "BBBBB",
									}
								},
								AdditionalInformation = new System.Collections.ObjectModel.Collection<AdditionalInformationType02>()
								{
									new AdditionalInformationType02()
									{
										SequenceNumber = "1",
										Code = "A",
										Text = "Add 1",
									},
									new AdditionalInformationType02()
									{
										SequenceNumber = "2",
										Code = "B",
										Text = "Add 2",
									}
								}
							}
						},
						SupportingDocument = new System.Collections.ObjectModel.Collection<SupportingDocumentType02>()
						{
							new SupportingDocumentType02()
							{
								SequenceNumber = "1",
								Type = "S",
								ReferenceNumber = "S1",
								ComplementOfInformation = "Supporting Info 1",
							},
							new SupportingDocumentType02()
							{
								SequenceNumber = "2",
								Type = "T",
								ReferenceNumber = "S2",
								ComplementOfInformation = "Supporting Info 2",
							}
						}
					}
				}
			});
		}
		CC043CConsignmentProvider provider;
	}
}
