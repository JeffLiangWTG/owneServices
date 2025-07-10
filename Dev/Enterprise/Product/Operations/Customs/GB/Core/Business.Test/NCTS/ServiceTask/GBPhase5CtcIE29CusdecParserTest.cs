using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc029c;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.custom_ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GBPhase5CtcIE29CusdecParserTest : TestCaseWithFactory
	{
		public void TestParse()
		{
			var messageObject = CreateTestObject();

			var message = Factory.New<NctsEdiMessage>();
			message.EM_MessageText = SerializationHelper.Serialize(messageObject);

			var parser = new GBPhase5CtcIE29CusdecParser { EdiMessage = message };
			var data = parser.Parse();

			AssertNotNull("Should return an NctsPhase5IE29CusdecResponseData", data);

			AssertEquals("LocalReferenceNumber", messageObject.TransitOperation.Lrn, data.LocalReferenceNumber);
			AssertEquals("MovementReferenceNumber", messageObject.TransitOperation.Mrn, data.MovementReferenceNumber);
			AssertEquals("DeclarationType", messageObject.TransitOperation.DeclarationType, data.DeclarationType);
			AssertEquals("CountryOfDestination", messageObject.Consignment.CountryOfDestination, data.CountryOfDestination);
			var expectedAgreedLocationOfGoods =
				messageObject.Consignment.LocationOfGoods.TypeOfLocation +
				messageObject.Consignment.LocationOfGoods.QualifierOfIdentification +
				messageObject.Consignment.LocationOfGoods.AuthorisationNumber;
			AssertEquals("AgreedLocationOfGoods", expectedAgreedLocationOfGoods, data.AgreedLocationOfGoods);
			AssertEquals("PlaceOfLoadingCode", messageObject.Consignment.PlaceOfLoading.Location, data.PlaceOfLoadingCode);
			AssertEquals("CountryOfDispatch", messageObject.Consignment.PlaceOfLoading.Country, data.CountryOfDispatch);
			AssertEquals("InlandTransportMode", messageObject.Consignment.InlandModeOfTransport, data.InlandTransportMode);
			AssertEquals("TransportModeAtBorder", messageObject.Consignment.ModeOfTransportAtTheBorder, data.TransportModeAtBorder);
			AssertEquals("MeansOfTransportCrossingBorderIdentity", messageObject.Consignment.ActiveBorderTransportMeans[0].IdentificationNumber, data.MeansOfTransportCrossingBorderIdentity);
			AssertEquals("MeansOfTransportCrossingBorderNationality", messageObject.Consignment.ActiveBorderTransportMeans[0].Nationality, data.MeansOfTransportCrossingBorderNationality);
			AssertEquals("MeansOfTransportCrossingBorderType", messageObject.Consignment.ActiveBorderTransportMeans[0].TypeOfIdentification, data.MeansOfTransportCrossingBorderType);
			AssertEquals("MeansOfTransportAtDepartureIdentity", messageObject.Consignment.DepartureTransportMeans[0].IdentificationNumber, data.MeansOfTransportAtDepartureIdentity);
			AssertEquals("MeansOfTransportAtDepartureNationality", messageObject.Consignment.DepartureTransportMeans[0].Nationality, data.MeansOfTransportAtDepartureNationality);
			AssertEquals("IsContainerised", expected: true, data.IsContainerised);
			AssertEquals("AcceptanceDate", messageObject.TransitOperation.DeclarationAcceptanceDate.ToString("yyyy-MM-dd"), data.AcceptanceDate);
			AssertEquals("IssuingDate", messageObject.TransitOperation.ReleaseDate.ToString("yyyy-MM-dd"), data.IssuingDate);
			AssertEquals("DialogLanguageIndicatorAtDeparture", messageObject.TransitOperation.CommunicationLanguageAtDeparture, data.DialogLanguageIndicatorAtDeparture);
			AssertEquals("NctsAccompanyingDocumentLanguageCode", messageObject.TransitOperation.CommunicationLanguageAtDeparture, data.NctsAccompanyingDocumentLanguageCode);
			AssertEquals("TotalNumberOfItems", "3", data.TotalNumberOfItems);
			AssertEquals("TotalNumberOfPackages", "240", data.TotalNumberOfPackages);
			AssertEquals("TotalGrossMassInKilograms", messageObject.Consignment.GrossMass.ToString(NumberFormatInfo.InvariantInfo), data.TotalGrossMassInKilograms);
			AssertEquals("BindingItinerary", "1", data.BindingItinerary);
			AssertEquals("AuthorisationId", messageObject.Authorisation[0].ReferenceNumber, data.AuthorisationId);
			AssertEquals("DeclarationDate", messageObject.PreparationDateAndTime.ToString("yyyy-MM-dd"), data.DeclarationDate);
			AssertEquals("CommercialReferenceNumber", messageObject.Consignment.ReferenceNumberUcr, data.CommercialReferenceNumber);
			AssertEquals("IsSecurity", expected: true, data.IsSecurity);
			AssertEquals("ConveyanceReferenceNumber", messageObject.Consignment.ActiveBorderTransportMeans[0].ConveyanceReferenceNumber, data.ConveyanceReferenceNumber);
			AssertEquals("PlaceOfUnloadingCode", messageObject.Consignment.PlaceOfUnloading.Location, data.PlaceOfUnloadingCode);

			AssertAddressResponseData("Principal",
				messageObject.HolderOfTheTransitProcedure.IdentificationNumber,
				messageObject.HolderOfTheTransitProcedure.Name,
				messageObject.HolderOfTheTransitProcedure.Address.StreetAndNumber,
				messageObject.HolderOfTheTransitProcedure.Address.City,
				messageObject.HolderOfTheTransitProcedure.Address.Postcode,
				messageObject.HolderOfTheTransitProcedure.Address.Country,
				data.Principal);
			AssertAddressResponseData("Consignor",
				messageObject.Consignment.Consignor.IdentificationNumber,
				messageObject.Consignment.Consignor.Name,
				messageObject.Consignment.Consignor.Address.StreetAndNumber,
				messageObject.Consignment.Consignor.Address.City,
				messageObject.Consignment.Consignor.Address.Postcode,
				messageObject.Consignment.Consignor.Address.Country,
				data.Consignor);
			AssertAddressResponseData("Consignee",
				messageObject.Consignment.Consignee.IdentificationNumber,
				messageObject.Consignment.Consignee.Name,
				messageObject.Consignment.Consignee.Address.StreetAndNumber,
				messageObject.Consignment.Consignee.Address.City,
				messageObject.Consignment.Consignee.Address.Postcode,
				messageObject.Consignment.Consignee.Address.Country,
				data.Consignee);

			AssertEquals("DepartureCustomsOfficeCode", messageObject.CustomsOfficeOfDeparture.ReferenceNumber, data.DepartureCustomsOfficeCode);
			AssertEquals("TransitCustomsOffices.Count", messageObject.CustomsOfficeOfTransitDeclared.Count, data.TransitCustomsOffices.Count);
			AssertEquals("TransitCustomsOffices[0].OfficeCode", messageObject.CustomsOfficeOfTransitDeclared[0].ReferenceNumber, data.TransitCustomsOffices[0].OfficeCode);
			AssertEquals("TransitCustomsOffices[1].OfficeCode", messageObject.CustomsOfficeOfTransitDeclared[1].ReferenceNumber, data.TransitCustomsOffices[1].OfficeCode);
			AssertEquals("DestinationCustomsOfficeCode", messageObject.CustomsOfficeOfDestinationDeclared.ReferenceNumber, data.DestinationCustomsOfficeCode);
			AssertEquals("ControlResultControlDate", messageObject.ControlResult.Date.ToString("yyyy-MM-dd"), data.ControlResultControlDate);
			AssertEquals("ControlResultControlResultCode", messageObject.ControlResult.Code, data.ControlResultControlResultCode);
			AssertEquals("ControlResultControlledBy", messageObject.ControlResult.ControlledBy, data.ControlResultControlledBy);
			AssertEquals("ControlResultTimeLimit", messageObject.ControlResult.Date.ToString("yyyy-MM-dd"), data.ControlResultTimeLimit);
			AssertEquals("RepresentativeName", messageObject.Representative.ContactPerson.Name, data.RepresentativeName);
			AssertEquals("RepresentativeCapacity", messageObject.Representative.Status, data.RepresentativeCapacity);
			AssertEquals("Representative.IdentificationNumber", messageObject.Representative.IdentificationNumber, data.Representative.IdentificationNumber);
			AssertEquals("Representative.Status", messageObject.Representative.Status, data.Representative.Status);

			AssertAddressResponseData("Carrier",
				messageObject.Consignment.Carrier.IdentificationNumber,
				string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
				data.Carrier);

			AssertAddressResponseData("SecurityConsignor",
				messageObject.Consignment.Consignor.IdentificationNumber,
				messageObject.Consignment.Consignor.Name,
				messageObject.Consignment.Consignor.Address.StreetAndNumber,
				messageObject.Consignment.Consignor.Address.City,
				messageObject.Consignment.Consignor.Address.Postcode,
				messageObject.Consignment.Consignor.Address.Country,
				data.SecurityConsignor);
			AssertAddressResponseData("SecurityConsignee",
				messageObject.Consignment.Consignee.IdentificationNumber,
				messageObject.Consignment.Consignee.Name,
				messageObject.Consignment.Consignee.Address.StreetAndNumber,
				messageObject.Consignment.Consignee.Address.City,
				messageObject.Consignment.Consignee.Address.Postcode,
				messageObject.Consignment.Consignee.Address.Country,
				data.SecurityConsignee);

			AssertEquals("Guarantees.Count", 2, data.Guarantees.Count);
			AssertEquals("Guarantees[0].GuaranteeType", messageObject.Guarantee[0].GuaranteeType, data.Guarantees[0].GuaranteeType);
			AssertEquals("Guarantees[0].GuaranteeReferenceNumber", messageObject.Guarantee[0].GuaranteeReference[0].Grn, data.Guarantees[0].GuaranteeReferenceNumber);
			AssertEquals("Guarantees[0].OtherGuaranteeReference", messageObject.Guarantee[0].OtherGuaranteeReference, data.Guarantees[0].OtherGuaranteeReference);
			AssertEquals("Guarantees[0].AccessCode", messageObject.Guarantee[0].GuaranteeReference[0].AccessCode, data.Guarantees[0].AccessCode);
			AssertEquals("Guarantees[1].GuaranteeType", messageObject.Guarantee[1].GuaranteeType, data.Guarantees[1].GuaranteeType);
			AssertEquals("Guarantees[1].GuaranteeReferenceNumber", messageObject.Guarantee[1].GuaranteeReference[0].Grn, data.Guarantees[1].GuaranteeReferenceNumber);
			AssertEquals("Guarantees[1].OtherGuaranteeReference", messageObject.Guarantee[1].OtherGuaranteeReference, data.Guarantees[1].OtherGuaranteeReference);
			AssertEquals("Guarantees[1].AccessCode", messageObject.Guarantee[1].GuaranteeReference[0].AccessCode, data.Guarantees[1].AccessCode);

			AssertEquals("Seals.Count", 3, data.Seals.Count);
			AssertEquals("Seals[0]", messageObject.Consignment.TransportEquipment[0].Seal[0].Identifier, data.Seals[0]);
			AssertEquals("Seals[1]", messageObject.Consignment.TransportEquipment[0].Seal[1].Identifier, data.Seals[1]);
			AssertEquals("Seals[2]", messageObject.Consignment.TransportEquipment[1].Seal[0].Identifier, data.Seals[2]);

			AssertEquals("Itinerary", "GB ZA NZ JP NO", data.Itinerary);

			AssertEquals("GoodsItems.Count", 3, data.GoodsItems.Count);
			AssertGoodsItem("GoodsItems[0]", messageObject.Consignment.HouseConsignment[0].ConsignmentItem[0], data.GoodsItems[0]);

			AssertEquals("GoodsItems[0].ContainerNumbers.Count", 2, data.GoodsItems[0].ContainerNumbers.Count);
			AssertEquals("GoodsItems[0].ContainerNumbers[0]", "Container1", data.GoodsItems[0].ContainerNumbers[0]);
			AssertEquals("GoodsItems[0].ContainerNumbers[1]", "Container2", data.GoodsItems[0].ContainerNumbers[1]);
			AssertEquals("GoodsItems[1].ContainerNumbers.Count", 1, data.GoodsItems[1].ContainerNumbers.Count);
			AssertEquals("GoodsItems[1].ContainerNumbers[0]", "Container2", data.GoodsItems[1].ContainerNumbers[0]);
			AssertEquals("GoodsItems[2].ContainerNumbers.Count", 1, data.GoodsItems[2].ContainerNumbers.Count);
			AssertEquals("GoodsItems[2].ContainerNumbers[0]", "Container1", data.GoodsItems[2].ContainerNumbers[0]);

			AssertEquals("ReferenceNumberUCR", messageObject.Consignment.ReferenceNumberUcr, data.ReferenceNumberUCR);

			AssertEquals("Authorisations.Count", 2, data.Authorisations.Count);
			AssertEquals("Authorisations[0].SequenceNumber", "1", data.Authorisations[0].SequenceNumber);
			AssertEquals("Authorisations[0].Type", "A1", data.Authorisations[0].Type);
			AssertEquals("Authorisations[0].ReferenceNumber", "AuthReference1", data.Authorisations[0].ReferenceNumber);
			AssertEquals("Authorisations[1].SequenceNumber", "2", data.Authorisations[1].SequenceNumber);
			AssertEquals("Authorisations[1].Type", "A2", data.Authorisations[1].Type);
			AssertEquals("Authorisations[1].ReferenceNumber", "AuthReference2", data.Authorisations[1].ReferenceNumber);
		}

		public void TestParse_Empty()
		{
			var message = Factory.New<NctsEdiMessage>();
			message.EM_MessageText = "";

			var parser = new GBPhase5CtcIE29CusdecParser();
			parser.EdiMessage = message;

			AssertNoExceptionThrown(() => parser.Parse());
		}

		public void TestParse_WithNulls()
		{
			var messageObject = CreateTestObject();
			messageObject.Consignment.LocationOfGoods = null;
			messageObject.Consignment.PlaceOfLoading = null;
			messageObject.Consignment.ActiveBorderTransportMeans[0] = null;
			messageObject.Consignment.DepartureTransportMeans[0] = null;
			messageObject.Consignment.PlaceOfUnloading = null;
			messageObject.Consignment.Consignor = null;
			messageObject.Consignment.Consignee = null;
			messageObject.Consignment.Carrier = null;
			messageObject.Consignment.TransportEquipment[0].Seal[0] = null;
			messageObject.Consignment.TransportEquipment[1] = null;
			messageObject.Consignment.CountryOfRoutingOfConsignment[1] = null;
			messageObject.Consignment.HouseConsignment[0].ConsignmentItem[1] = null;
			messageObject.Consignment.HouseConsignment[1] = null;
			messageObject.Authorisation[0] = null;
			messageObject.HolderOfTheTransitProcedure = null;
			messageObject.CustomsOfficeOfDeparture = null;
			messageObject.CustomsOfficeOfTransitDeclared[1] = null;
			messageObject.CustomsOfficeOfDestinationDeclared = null;
			messageObject.ControlResult = null;
			messageObject.Representative.ContactPerson = null;
			messageObject.Guarantee[0].GuaranteeReference[0] = null;
			messageObject.Guarantee[1] = null;

			var message = Factory.New<NctsEdiMessage>();
			message.EM_MessageText = SerializationHelper.Serialize(messageObject);

			var parser = new GBPhase5CtcIE29CusdecParser { EdiMessage = message };
			AssertNoExceptionThrown(() => parser.Parse());
		}

		public void TestParse_Security()
		{
			var messageObject = new Cc029CType
			{
				TransitOperation = new TransitOperationType12(),
				Consignment = new CustomConsignmentType04
				{
					Consignee = new ConsigneeType04
					{
						Name = "Consignee",
					},
					Consignor = new ConsignorType03
					{
						Name = "Consignor",
					},
				},
			};
			var message = Factory.New<NctsEdiMessage>();
			message.EM_MessageText = SerializationHelper.Serialize(messageObject);
			var parser = new GBPhase5CtcIE29CusdecParser { EdiMessage = message };
			var data = parser.Parse();

			AssertEquals("Empty Security => Security", expected: false, data.IsSecurity);
			AssertNull("When not security (empty), SecurityConsignee", data.SecurityConsignee);
			AssertNull("When not security (empty), SecurityConsignee", data.SecurityConsignor);

			messageObject.TransitOperation.Security = "1";
			message.EM_MessageText = SerializationHelper.Serialize(messageObject);
			data = parser.Parse();

			AssertEquals("Security 1 => Security", expected: true, data.IsSecurity);
			AssertNotNull("When security (1), SecurityConsignee", data.SecurityConsignee);
			AssertNotNull("When security (1), SecurityConsignee", data.SecurityConsignor);

			messageObject.TransitOperation.Security = "2";
			message.EM_MessageText = SerializationHelper.Serialize(messageObject);
			data = parser.Parse();

			AssertEquals("Security 2 => Security", expected: true, data.IsSecurity);
			AssertNotNull("When security (2), SecurityConsignee", data.SecurityConsignee);
			AssertNotNull("When security (2), SecurityConsignee", data.SecurityConsignor);

			messageObject.TransitOperation.Security = "0";
			message.EM_MessageText = SerializationHelper.Serialize(messageObject);
			data = parser.Parse();

			AssertEquals("Security 0 => Security", expected: false, data.IsSecurity);
			AssertNull("When not security (0), SecurityConsignee", data.SecurityConsignee);
			AssertNull("When not security (0), SecurityConsignee", data.SecurityConsignor);
		}

		void AssertAddressResponseData(string id, string expectedIdNumber, string expectedName, string expectedAddress1, string expectedCity, string expectedPostcode, string expectedCountry, AddressResponseData actualData)
		{
			AssertEquals(id + ".CompanyName", expectedName, actualData.CompanyName);
			AssertEquals(id + ".Address1", expectedAddress1, actualData.Address1);
			AssertEquals(id + ".Postcode", expectedPostcode, actualData.Postcode);
			AssertEquals(id + ".City", expectedCity, actualData.City);
			AssertEquals(id + ".CountryCode", expectedCountry, actualData.CountryCode);
			AssertEquals(id + ".GovRegNum", expectedIdNumber, actualData.GovRegNum);
		}

		void AssertGoodsItem(string id, CustomConsignmentItemType03 expectedItem, NctsGoodsItemResponseData actualItem)
		{
			AssertEquals(id + ".ItemNumber", expectedItem.DeclarationGoodsItemNumber, actualItem.ItemNumber);
			AssertEquals(id + ".CommodityCode", expectedItem.Commodity.CommodityCode.HarmonizedSystemSubHeadingCode, actualItem.CommodityCode);
			AssertEquals(id + ".DeclarationType", expectedItem.DeclarationType, actualItem.DeclarationType);
			AssertEquals(id + ".DescriptionOfGoods", expectedItem.Commodity.DescriptionOfGoods, actualItem.DescriptionOfGoods);
			AssertEquals(id + ".GrossMassInKilograms", expectedItem.Commodity.GoodsMeasure.GrossMass.ToString(NumberFormatInfo.InvariantInfo), actualItem.GrossMassInKilograms);
			AssertEquals(id + ".NetMassInKilograms", expectedItem.Commodity.GoodsMeasure.NetMass?.ToString(NumberFormatInfo.InvariantInfo), actualItem.NetMassInKilograms);
			AssertEquals(id + ".CountryOfDispatch", expectedItem.CountryOfDispatch, actualItem.CountryOfDispatch);
			AssertEquals(id + ".CountryOfDestination", expectedItem.CountryOfDestination, actualItem.CountryOfDestination);
			AssertEquals(id + ".TransportChargesMoP", expectedItem.TransportCharges.MethodOfPayment, actualItem.TransportChargesMoP);
			AssertEquals(id + ".CommercialReferenceNumber", expectedItem.ReferenceNumberUcr, actualItem.CommercialReferenceNumber);
			AssertEquals(id + ".PreviousDocuments.Count", expectedItem.PreviousDocument.Count, actualItem.PreviousDocuments.Count);
			for (var i = 0; i < actualItem.PreviousDocuments.Count; i++)
			{
				AssertEquals(id + $".PreviousDocuments[{i}].TypeCode", expectedItem.PreviousDocument[i].Type, actualItem.PreviousDocuments[i].TypeCode);
				AssertEquals(id + $".PreviousDocuments[{i}].Reference", expectedItem.PreviousDocument[i].ReferenceNumber, actualItem.PreviousDocuments[i].Reference);
				AssertEquals(id + $".PreviousDocuments[{i}].MoreInfo", expectedItem.PreviousDocument[i].ComplementOfInformation, actualItem.PreviousDocuments[i].MoreInfo);
			}
			AssertEquals(id + ".PreviousDocuments.Count", expectedItem.SupportingDocument.Count, actualItem.SupportingDocuments.Count);
			for (var i = 0; i < actualItem.SupportingDocuments.Count; i++)
			{
				AssertEquals(id + $".SupportingDocuments[{i}].TypeCode", expectedItem.SupportingDocument[i].Type, actualItem.SupportingDocuments[i].TypeCode);
				AssertEquals(id + $".SupportingDocuments[{i}].Reference", expectedItem.SupportingDocument[i].ReferenceNumber, actualItem.SupportingDocuments[i].Reference);
				AssertEquals(id + $".SupportingDocuments[{i}].Reason", expectedItem.SupportingDocument[i].ComplementOfInformation, actualItem.SupportingDocuments[i].Reason);
			}
			AssertEquals(id + ".Packages.Count", expectedItem.Packaging.Count, actualItem.Packages.Count);
			for (var i = 0; i < actualItem.Packages.Count; i++)
			{
				AssertEquals(id + $".Packages[{i}].MarksAndNumbers", expectedItem.Packaging[i].ShippingMarks, actualItem.Packages[i].MarksAndNumbers);
				AssertEquals(id + $".Packages[{i}].PackageType", expectedItem.Packaging[i].TypeOfPackages, actualItem.Packages[i].PackageType);
				AssertEquals(id + $".Packages[{i}].NumberOfPackages", expectedItem.Packaging[i].NumberOfPackages, actualItem.Packages[i].NumberOfPackages);
			}
		}

		Cc029CType CreateTestObject()
		{
			return new Cc029CType
			{
				TransitOperation = new TransitOperationType12
				{
					Lrn = "LRN1234",
					Mrn = "MRN5678",
					DeclarationType = "Z",
					DeclarationAcceptanceDate = new DateTime(2023, 1, 1),
					ReleaseDate = new DateTime(2023, 1, 2),
					CommunicationLanguageAtDeparture = "ZZ",
					BindingItinerary = Flag.Item1,
					Security = "1",
				},
				Consignment = new CustomConsignmentType04
				{
					CountryOfDestination = "NZ",
					LocationOfGoods = new LocationOfGoodsType02
					{
						TypeOfLocation = "A",
						QualifierOfIdentification = "B",
						AuthorisationNumber = "987654321",
					},
					PlaceOfLoading = new PlaceOfLoadingType02
					{
						Location = "XY4242",
						Country = "NZ",
					},
					InlandModeOfTransport = "HOV",
					ModeOfTransportAtTheBorder = "MAG",
					ActiveBorderTransportMeans = new Collection<ActiveBorderTransportMeansType01>
					{
						new ActiveBorderTransportMeansType01
						{
							IdentificationNumber = "90210",
							Nationality = "AU",
							TypeOfIdentification = "T",
							ConveyanceReferenceNumber = "333221",
						}
					},
					DepartureTransportMeans = new Collection<DepartureTransportMeansType02>
					{
						new DepartureTransportMeansType02()
						{
							IdentificationNumber = "000",
							Nationality = "NZ",
						}
					},
					ContainerIndicator = Flag.Item1,
					GrossMass = 1234.567m,

					ReferenceNumberUcr = "UCR999",
					PlaceOfUnloading = new PlaceOfUnloadingType02
					{
						Location = "GB123",
					},
					Consignor = new ConsignorType03
					{
						IdentificationNumber = "Consignor1",
						Name = "Consignment Consignor",
						Address = new AddressType07
						{
							StreetAndNumber = "1 Consignor St",
							City = "Consignor City",
							Postcode = "CO1 1OR",
							Country = "GB",
						}
					},
					Consignee = new ConsigneeType04
					{
						IdentificationNumber = "Consignee1",
						Name = "Consignment Consignee",
						Address = new AddressType07
						{
							StreetAndNumber = "1 Consignee St",
							City = "Consignee City",
							Postcode = "CO1 1EE",
							Country = "GB",
						}
					},
					Carrier = new CarrierType03
					{
						IdentificationNumber = "CarrierID"
					},
					TransportEquipment = new Collection<TransportEquipmentType05>
					{
						new TransportEquipmentType05
						{
							Seal = new Collection<SealType04>
							{
								new SealType04
								{
									Identifier = "Seal1",
								},
								new SealType04
								{
									Identifier = "Seal2",
								}
							},
							GoodsReference = new Collection<GoodsReferenceType02>
							{
								new GoodsReferenceType02 { DeclarationGoodsItemNumber = "Item1" },
								new GoodsReferenceType02 { DeclarationGoodsItemNumber = "Item3" },
							},
							ContainerIdentificationNumber = "Container1",
						},
						new TransportEquipmentType05
						{
							Seal = new Collection<SealType04> { new SealType04 { Identifier = "Seal3" } },
							GoodsReference = new Collection<GoodsReferenceType02>
							{
								new GoodsReferenceType02 { DeclarationGoodsItemNumber = "Item1" },
								new GoodsReferenceType02 { DeclarationGoodsItemNumber = "Item2" },
							},
							ContainerIdentificationNumber = "Container2",
						},
					},
					CountryOfRoutingOfConsignment = CreateCountryOfRoutingOfConsignments("GB", "ZA", "NZ", "JP", "NO"),
					HouseConsignment = new Collection<CustomHouseConsignmentType03>
					{
						new CustomHouseConsignmentType03
						{
							ConsignmentItem = new Collection<CustomConsignmentItemType03>
							{
								CreateConsignmentItem1(),
								CreateConsignmentItem2(),
							},
						},
						new CustomHouseConsignmentType03
						{
							ConsignmentItem = new Collection<CustomConsignmentItemType03>
							{
								CreateConsignmentItem3(),
							},
						},
					}
				},
				Authorisation = new Collection<AuthorisationType02>
				{
					new AuthorisationType02
					{
						SequenceNumber = "2",
						Type = "A2",
						ReferenceNumber = "AuthReference2",
					},
					new AuthorisationType02
					{
						SequenceNumber = "1",
						Type = "A1",
						ReferenceNumber = "AuthReference1",
					},
				},
				PreparationDateAndTime = new DateTime(2022, 12, 31),
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType05
				{
					IdentificationNumber = "Holder1",
					Name = "HolderOfTheTransitProcedure",
					Address = new AddressType07
					{
						StreetAndNumber = "1 Holder St",
						City = "Holder City",
						Postcode = "HO1 1DR",
						Country = "GB",
					}
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "CD122333"
				},
				CustomsOfficeOfTransitDeclared = new Collection<CustomsOfficeOfTransitDeclaredType04>
				{
					new CustomsOfficeOfTransitDeclaredType04 { ReferenceNumber = "CT1" },
					new CustomsOfficeOfTransitDeclaredType04 { ReferenceNumber = "CT2" },
				},
				CustomsOfficeOfDestinationDeclared = new CustomsOfficeOfDestinationDeclaredType01
				{
					ReferenceNumber = "CD1",
				},
				ControlResult = new ControlResultType02
				{
					Date = new DateTime(2023, 1, 3),
					ControlledBy = "ME",
					Code = "CC0",
				},
				Representative = new RepresentativeType02
				{
					ContactPerson = new ContactPersonType01 { Name = "Representative Name" },
					IdentificationNumber = "Number6",
					Status = "Important",
				},
				Guarantee = new Collection<GuaranteeType03>
				{
					new GuaranteeType03
					{
						GuaranteeType = "GT1",
						GuaranteeReference = new Collection<GuaranteeReferenceType01>
						{
							new GuaranteeReferenceType01
							{
								Grn = "GRN1",
								AccessCode = "AX1",
							},
						},
						OtherGuaranteeReference = "OG1",
					},
					new GuaranteeType03
					{
						GuaranteeType = "GT2",
						GuaranteeReference = new Collection<GuaranteeReferenceType01>
						{
							new GuaranteeReferenceType01
							{
								Grn = "GRN2",
								AccessCode = "AX2",
							}
						},
						OtherGuaranteeReference = "OG2",
					},
				},
			};
		}

		Collection<CountryOfRoutingOfConsignmentType01> CreateCountryOfRoutingOfConsignments(params string[] countries)
		{
			var rnd = new Random(0);
			var list = countries
				.Select((country, index) => (country, index, rnd.Next()))
				.OrderBy(x => x.Item3)
				.Select(x => new CountryOfRoutingOfConsignmentType01
				{
					Country = x.country,
					SequenceNumber = x.index.ToString(),
				}).ToList();
			return new Collection<CountryOfRoutingOfConsignmentType01>(list);
		}

		CustomConsignmentItemType03 CreateConsignmentItem1()
		{
			return new CustomConsignmentItemType03
			{
				DeclarationGoodsItemNumber = "Item1",
				Commodity = new CustomCommodityType08
				{
					CommodityCode = new CommodityCodeType05
					{
						HarmonizedSystemSubHeadingCode = "89001234",
					},
					DescriptionOfGoods = "Actual Description 1",
					GoodsMeasure = new GoodsMeasureType03
					{
						GrossMass = 100.12m,
						NetMass = 104.62m,
					},
				},
				DeclarationType = "DT1",
				CountryOfDispatch = "NZ",
				CountryOfDestination = "GB",
				TransportCharges = new TransportChargesType
				{
					MethodOfPayment = "D",
				},
				ReferenceNumberUcr = "UCR.Item1",
				PreviousDocument = new Collection<PreviousDocumentType03>
				{
					new PreviousDocumentType03
					{
						Type = "DT1",
						ReferenceNumber = "PDREF1",
						ComplementOfInformation = "Yes",
					},
					new PreviousDocumentType03
					{
						Type = "DT2",
						ReferenceNumber = "PDREF2",
						ComplementOfInformation = "No",
					},
					new PreviousDocumentType03
					{
						Type = "DT3",
						ReferenceNumber = "PDREF3",
						ComplementOfInformation = "Maybe",
					},
				},
				SupportingDocument = new Collection<SupportingDocumentType06>
				{
					new SupportingDocumentType06
					{
						Type = "DT4",
						ReferenceNumber = "PDREF5",
						ComplementOfInformation = "Because",
					},
					new SupportingDocumentType06
					{
						Type = "DT5",
						ReferenceNumber = "PDREF4",
						ComplementOfInformation = "AskedFor",
					},
				},
				Packaging = new Collection<PackagingType02>
				{
					new PackagingType02
					{
						ShippingMarks = "MACH1",
						TypeOfPackages = "WrongType",
						NumberOfPackages = "128",
					}
				},
			};
		}

		CustomConsignmentItemType03 CreateConsignmentItem2()
		{
			return new CustomConsignmentItemType03
			{
				DeclarationGoodsItemNumber = "Item2",
				Packaging = new Collection<PackagingType02>
				{
					new PackagingType02
					{
						NumberOfPackages = "32"
					},
					new PackagingType02
					{
						NumberOfPackages = "16"
					}
				},
			};
		}

		CustomConsignmentItemType03 CreateConsignmentItem3()
		{
			return new CustomConsignmentItemType03
			{
				DeclarationGoodsItemNumber = "Item3",
				Packaging = new Collection<PackagingType02>
				{
					new PackagingType02
					{
						NumberOfPackages = "64"
					}
				},
			};
		}
	}
}
