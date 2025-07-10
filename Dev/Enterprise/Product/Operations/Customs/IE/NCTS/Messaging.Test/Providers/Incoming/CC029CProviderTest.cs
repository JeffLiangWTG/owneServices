using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC029C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC029CProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException when calling constructor passing a null input", () => new CC029CProvider(null));
		}

		public void TestConstructorAndFieldsWithEmptyInputs()
		{
			var test = new ZDateTime(null);
			CombineAssertions("Should not throw exception when creating using empty input and accessing fields", () =>
			{
				CC029CProvider testProvider = null;
				AssertNoExceptionThrown("Should not throw exception when calling constructor passing an ", () => testProvider = new CC029CProvider(new Cc029CType()));
				AssertEquals("MRN empty", ZString.Empty, testProvider.MRN);
				AssertEquals("LRN empty", ZString.Empty, testProvider.LRN);
				AssertEquals("ReleaseDate empty", ZDateTime.Empty, testProvider.ReleaseDate);
				AssertEquals("AdditionalDeclarationType empty", ZString.Empty, testProvider.AdditionalDeclarationType);
				AssertEquals("ControlResultCode empty", ZString.Empty, testProvider.ControlResultCode);
				AssertEquals("ControlResultDate empty", ZDateTime.Empty, testProvider.ControlResultDate);
				AssertEquals("ControlResultController empty", ZString.Empty, testProvider.ControlResultController);
				AssertEquals("ControlResultText empty", ZString.Empty, testProvider.ControlResultText);
			});
		}

		public void TestDateTimeFields_InvalidInput()
		{
			var provider = CreateStandardProvider();
			provider.TransitOperation.ReleaseDate
				= provider.TransitOperation.DeclarationAcceptanceDate
				= provider.ControlResult.Date
				= new DateTime();
			var testProvider = new CC029CProvider(provider);
			CombineAssertions("ZDateTime fields should return ZDateTime.Empty with invalid input.", () =>
			{
				AssertEquals("ReleaseDate", ZDateTime.Empty, testProvider.ReleaseDate);
				AssertEquals("ControlResultDate", ZDateTime.Empty, testProvider.ControlResultDate);
			});
		}

		public void TestFieldsWithValidInputs()
		{
			var testProvider = new CC029CProvider(CreateStandardProvider());

			CombineAssertions("CC029CProvider fields, acceptance: 2023/7/19, control result date: 2023/7/20, release date: 2023/7/21", () =>
			{
				AssertEquals("MRN", "19MRNCC055C0123456", testProvider.MRN);
				AssertEquals("LRN", "LRNCC056C0123456789012", testProvider.LRN);
				AssertEquals("ReleaseDate", new ZDateTime(2023, 7, 21, 11, 11, 0), testProvider.ReleaseDate);
				AssertEquals("AdditionalDeclarationType", "A", testProvider.AdditionalDeclarationType);

				AssertEquals("ControlResultCode", "R1", testProvider.ControlResultCode);
				AssertEquals("ControlResultDate", new ZDateTime(2023, 7, 20, 10, 10, 0), testProvider.ControlResultDate);
				AssertEquals("ControlResultController", "IEDUB499", testProvider.ControlResultController);
				AssertEquals("ControlResultText", "Control Result Text.", testProvider.ControlResultText);
			});
		}

		public static Cc029CType CreateStandardProvider(string mrn = "19MRNCC055C0123456", string lrn = "LRNCC056C0123456789012") => new Cc029CType
		{
			MessageType = MessageTypes.Cc029C,
			TransitOperation = new TransitOperationType12()
			{
				Lrn = lrn,
				Mrn = mrn,
				DeclarationType = "DEC1",
				ReleaseDate = new DateTime(2023, 7, 21, 11, 11, 0),
				AdditionalDeclarationType = "A",
				DeclarationAcceptanceDate = new DateTime(2023, 7, 19, 11, 11, 0),
				Security = "0"
			},
			Representative = new RepresentativeType02()
			{
				IdentificationNumber = "REP1",
				Status = "1",
				ContactPerson = new ContactPersonType01()
				{
					EMailAddress = "test@test.com",
					Name = "Contact",
					PhoneNumber = "123"
				}
			},
			ControlResult = new ControlResultType02()
			{
				Code = "R1",
				ControlledBy = "IEDUB499",
				Date = new DateTime(2023, 7, 20, 10, 10, 0),
				Text = "Control Result Text."
			},
			Guarantee = new Collection<GuaranteeType03>()
				{
					new GuaranteeType03()
					{
						SequenceNumber = "1",
						GuaranteeType = "1",
						OtherGuaranteeReference = "A",
						GuaranteeReference = new Collection<GuaranteeReferenceType01>()
						{
							new GuaranteeReferenceType01()
							{
								SequenceNumber = "1",
								Currency = "EUR",
								AccessCode = "1234",
								AmountToBeCovered = 100m,
								Grn = "05IE0011200000001"
							}
						}
					}
				},
			CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
			{
				ReferenceNumber = "RNALPHN8"
			},
			CustomsOfficeOfDestinationDeclared = new CustomsOfficeOfDestinationDeclaredType01()
			{
				ReferenceNumber = "RNALPHN9"
			},
			HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType05()
			{
				IdentificationNumber = "IN928",
				TirHolderIdentificationNumber = "TIRHIN928",
				Name = "BOB THE BUILDER",
				Address = new AddressType07
				{
					City = "CITY",
					Country = "IE",
					Postcode = "2020",
					StreetAndNumber = "123 WHERE ST"
				}
			},
			Consignment = new ConsignmentType04()
			{
				DepartureTransportMeans = new Collection<DepartureTransportMeansType02>()
					{
						new DepartureTransportMeansType02()
						{
							IdentificationNumber = "IE1234",
							Nationality = "IE",
							SequenceNumber = "1",
							TypeOfIdentification = "04"
						}
					},
				CountryOfRoutingOfConsignment = new Collection<CountryOfRoutingOfConsignmentType01>()
					{
						new CountryOfRoutingOfConsignmentType01()
						{
							SequenceNumber = "1",
							Country = "IE"
						}
					},
				ActiveBorderTransportMeans = new Collection<ActiveBorderTransportMeansType01>()
					{
						new ActiveBorderTransportMeansType01()
						{
							IdentificationNumber = "FR1222",
							Nationality = "FR",
							SequenceNumber = "1",
							TypeOfIdentification = "04",
							ConveyanceReferenceNumber = "CONVREF",
							CustomsOfficeAtBorderReferenceNumber = "FRPAR100"
						}
					},
				PlaceOfLoading = new PlaceOfLoadingType02()
				{
					Country = "IE",
					Location = "Dublin",
					UnLocode = "IEDUB"
				},
				Consignee = new ConsigneeType04()
				{
					Name = "Consignee",
					IdentificationNumber = "CONSEEID",
					Address = new AddressType07()
					{
						City = "DUBLIN",
						Country = "IE",
						Postcode = "1234 AB",
						StreetAndNumber = "123 Fake Street"
					}
				},
				Consignor = new ConsignorType03()
				{
					Name = "Consignor",
					IdentificationNumber = "CONSORID",
					Address = new AddressType07()
					{
						City = "DUBLIN",
						Country = "IE",
						Postcode = "1234 AB",
						StreetAndNumber = "123 Fake Street"
					}
				},
				GrossMass = 123.45m,
				ReferenceNumberUcr = "UCR",
				Carrier = new CarrierType03()
				{
					IdentificationNumber = "IE1234567",
					ContactPerson = new ContactPersonType01()
					{
						EMailAddress = "test@test.com",
						Name = "Contact",
						PhoneNumber = "123"
					}
				},
				TransportEquipment = new Collection<TransportEquipmentType05>()
					{
						new TransportEquipmentType05()
						{
							SequenceNumber = "1",
							ContainerIdentificationNumber = "CONT1",
							NumberOfSeals = "1",
							Seal = new Collection<SealType04>()
							{
								new SealType04()
								{
									SequenceNumber = "1",
									Identifier = "S1"
								}
							},
							GoodsReference = new Collection<GoodsReferenceType02>()
							{
								new GoodsReferenceType02()
								{
									SequenceNumber = "1",
									DeclarationGoodsItemNumber = "1"
								}
							}
						}
					},
				LocationOfGoods = new LocationOfGoodsType02()
				{
					TypeOfLocation = "A",
					QualifierOfIdentification = "Q",
				},
				AdditionalSupplyChainActor = new Collection<AdditionalSupplyChainActorType>()
					{
						new AdditionalSupplyChainActorType()
						{
							SequenceNumber = "1",
							IdentificationNumber = "ASCA",
							Role = "A",
						}
					},
				PreviousDocument = new Collection<PreviousDocumentType06>()
					{
						new PreviousDocumentType06()
						{
							ComplementOfInformation = "C",
							ReferenceNumber = "PREREF",
							SequenceNumber = "1",
							Type = "PRE1"
						}
					},
				SupportingDocument = new Collection<SupportingDocumentType06>()
					{
						new SupportingDocumentType06()
						{
							ComplementOfInformation = "C",
							ReferenceNumber = "SUPDOC",
							SequenceNumber = "1",
							Type = "SUP1",
							DocumentLineItemNumber = "1"
						}
					},
				AdditionalReference = new Collection<AdditionalReferenceType03>()
					{
						new AdditionalReferenceType03()
						{
							ReferenceNumber = "ADDREF",
							Type = "ADD1",
							SequenceNumber = "1"
						}
					},
				TransportDocument = new Collection<TransportDocumentType02>()
					{
						new TransportDocumentType02()
						{
							ReferenceNumber = "TRAREF",
							SequenceNumber = "1",
							Type = "TRA1"
						}
					},
				AdditionalInformation = new Collection<AdditionalInformationType02>()
					{
						new AdditionalInformationType02()
						{
							Text = "REF",
							SequenceNumber = "1",
							Code = "AINF1"
						}
					},
				TransportCharges = new TransportChargesType()
				{
					MethodOfPayment = "F"
				},
				HouseConsignment = new Collection<HouseConsignmentType03>()
					{
						new HouseConsignmentType03()
						{
							SequenceNumber = "1",
							SecurityIndicatorFromExportDeclaration = "1",
							Consignee = new ConsigneeType04()
							{
								Name = "Consignee",
								IdentificationNumber = "CONSEEID",
								Address = new AddressType07()
								{
									City = "DUBLIN",
									Country = "IE",
									Postcode = "1234 AB",
									StreetAndNumber = "123 Fake Street"
								}
							},
							Consignor = new ConsignorType04()
							{
								Name = "Consignor",
								IdentificationNumber = "CONSORID",
								Address = new AddressType07()
								{
									City = "DUBLIN",
									Country = "IE",
									Postcode = "1234 AB",
									StreetAndNumber = "123 Fake Street"
								}
							},
							GrossMass = 1.2m,
							ReferenceNumberUcr = "UCR",
							AdditionalSupplyChainActor = new Collection<AdditionalSupplyChainActorType>()
							{
								new AdditionalSupplyChainActorType()
								{
									SequenceNumber = "1",
									IdentificationNumber = "ASCA",
									Role = "A",
								}
							},
							PreviousDocument = new Collection<PreviousDocumentType07>()
							{
								new PreviousDocumentType07()
								{
									ComplementOfInformation = "C",
									ReferenceNumber = "PREREF",
									SequenceNumber = "1",
									Type = "PRE1"
								}
							},
							SupportingDocument = new Collection<SupportingDocumentType06>()
							{
								new SupportingDocumentType06()
								{
									ComplementOfInformation = "C",
									ReferenceNumber = "SUPDOC",
									SequenceNumber = "1",
									Type = "SUP1",
									DocumentLineItemNumber = "1"
								}
							},
							AdditionalReference = new Collection<AdditionalReferenceType03>()
							{
								new AdditionalReferenceType03()
								{
									ReferenceNumber = "ADDREF",
									Type = "ADD1",
									SequenceNumber = "1"
								}
							},
							AdditionalInformation = new Collection<AdditionalInformationType02>()
							{
								new AdditionalInformationType02()
								{
									Text = "REF",
									SequenceNumber = "1",
									Code = "AINF1"
								}
							},
							TransportCharges = new TransportChargesType()
							{
								MethodOfPayment = "F"
							},
							ConsignmentItem = new Collection<ConsignmentItemType03>()
							{
								new ConsignmentItemType03()
								{
									TransportCharges = new TransportChargesType()
									{
										MethodOfPayment = "F"
									},
									ReferenceNumberUcr = "UCR",
									DeclarationType = "T1",
									Consignee = new ConsigneeType03()
									{
										Name = "Consignee",
										IdentificationNumber = "CONSEEID",
										Address = new AddressType09()
										{
											City = "DUBLIN",
											Country = "IE",
											Postcode = "1234 AB",
											StreetAndNumber = "123 Fake Street"
										}
									},
									AdditionalReference = new Collection<AdditionalReferenceType02>()
									{
										new AdditionalReferenceType02()
										{
											ReferenceNumber = "ADDREF",
											Type = "ADD1",
											SequenceNumber = "1"
										}
									},
									AdditionalInformation = new Collection<AdditionalInformationType02>()
									{
										new AdditionalInformationType02()
										{
											Text = "REF",
											SequenceNumber = "1",
											Code = "AINF1"
										}
									},
									AdditionalSupplyChainActor = new Collection<AdditionalSupplyChainActorType>()
									{
										new AdditionalSupplyChainActorType()
										{
											SequenceNumber = "1",
											IdentificationNumber = "ASCA",
											Role = "A",
										}
									},
									PreviousDocument = new Collection<PreviousDocumentType03>()
									{
										new PreviousDocumentType03()
										{
											ComplementOfInformation = "C",
											ReferenceNumber = "PREREF",
											SequenceNumber = "1",
											Type = "PRE1"
										}
									},
									TransportDocument = new Collection<TransportDocumentType02>()
									{
										new TransportDocumentType02()
										{
											ReferenceNumber = "TRAREF",
											SequenceNumber = "1",
											Type = "TRA1"
										}
									},
									DeclarationGoodsItemNumber = "1",
									SupportingDocument = new Collection<SupportingDocumentType06>()
									{
										new SupportingDocumentType06()
										{
											ComplementOfInformation = "C",
											ReferenceNumber = "SUPDOC",
											SequenceNumber = "1",
											Type = "SUP1",
											DocumentLineItemNumber = "1"
										}
									},
									CountryOfDestination = "US",
									CountryOfDispatch = "IE",
									GoodsItemNumber = "1",
									Commodity = new CommodityType08()
									{
										CommodityCode = new CommodityCodeType05()
										{
											CombinedNomenclatureCode = "88",
											HarmonizedSystemSubHeadingCode = "809999"
										},
										CusCode = "889900000",
										DescriptionOfGoods = "Desc",
										DangerousGoods = new Collection<DangerousGoodsType01>()
										{
											new DangerousGoodsType01()
											{
												SequenceNumber = "1",
												UnNumber = "4444"
											}
										},
										GoodsMeasure = new GoodsMeasureType03()
										{
											GrossMass = 1.2m,
											NetMass = 1.1m,
											NetMassValue = 1.1m,
											NetMassValueSpecified = true
										}
									},
									Packaging = new Collection<PackagingType02>()
									{
										new PackagingType02()
										{
											NumberOfPackages = "1",
											SequenceNumber = "1",
											ShippingMarks = "MARKS",
											TypeOfPackages = "PK"
										}
									}
								}
							}
						}
					}
			}
		};
	}
}
