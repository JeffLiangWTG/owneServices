using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Core.Constants;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	partial class CMRCusSCAOceanBillDataObjectReaderTest
	{
		public void TestHVLV_GoodsDescriptionFallBack()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();

			var container = Factory.NewWithValidTestData<CusSCAContainer>();
			container.CN_ContainerNumber = "containernumber";
			container.CN_CB = oceanBill.PK;

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbShipment.PortOfDestination = new UNLOCO { Code = "AUSYD" };
			mawbShipment.PortOfOrigin = new UNLOCO { Code = "USCHI" };

			var hawbShipment = GetShipment();

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "containernumber",
				GoodsDescription = "Item GoodsDescription"
			};

			hawbShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packingLine });

			var logger = new DummyLogger();

			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();
			var pivot = house.CusSCAPivotCollection.FirstOrDefault(c => c.Container.PK == container.PK);
			AssertEquals("Goods Description from Item when consignment.GoodsDescription is empty", "Item GoodsDescription", pivot.CV_GoodsDescription);

			hawbShipment.GoodsDescription = "GoodsDescription";
			house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();
			pivot = house.CusSCAPivotCollection.FirstOrDefault(c => c.Container.PK == container.PK);
			AssertEquals("Goods Description from Consignment when consignment.GoodsDescription is empty", "GoodsDescription", pivot.CV_GoodsDescription);
		}

		public void TestPopulateCusSCAPivot_AddNewPerContainer()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();

			var container1 = Factory.NewWithValidTestData<CusSCAContainer>();
			container1.CN_ContainerNumber = "containernumber1";
			container1.CN_CB = oceanBill.PK;

			var container2 = Factory.NewWithValidTestData<CusSCAContainer>();
			container2.CN_ContainerNumber = "containernumber2";
			container2.CN_CB = oceanBill.PK;

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbShipment.PortOfDestination = new UNLOCO { Code = "AUSYD" };
			mawbShipment.PortOfOrigin = new UNLOCO { Code = "USCHI" };

			var hawbShipment = GetShipment();

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "containernumber1",
				Weight = 10,
				Volume = 10,
				GoodsDescription = "GoodsDescription1"
			};
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "containernumber2",
				Weight = 11,
				Volume = 11,
				GoodsDescription = "GoodsDescription2"
			};
			var packingLine3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "containernumber1",
				Weight = 12,
				Volume = 12,
				GoodsDescription = "GoodsDescription3"
			};

			hawbShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packingLine1, packingLine2, packingLine3 });

			var logger = new DummyLogger();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			var pivot1 = house.CusSCAPivotCollection.FirstOrDefault(c => c.Container.PK == container1.PK);
			var pivot2 = house.CusSCAPivotCollection.FirstOrDefault(c => c.Container.PK == container2.PK);

			var expectGoodsDescription1 = @"GoodsDescription1
GoodsDescription3";
			var expectGoodsDescription2 = @"GoodsDescription2";

			CombineAssertions("CusSCAPivotCollection", () =>
			{
				AssertEquals("2 pivot added", 2, house.CusSCAPivotCollection.Count());
				AssertEquals("pivot1 package count", 2, pivot1.CV_PackageCount);
				AssertEquals("pivot1 weight", 22m, pivot1.CV_Weight);
				AssertEquals("pivot1 Volumn", 22m, pivot1.CV_Volume);
				AssertEquals("pivot1 GoodsDescription", expectGoodsDescription1, pivot1.CV_GoodsDescription);

				AssertEquals("pivot2 package count", 1, pivot2.CV_PackageCount);
				AssertEquals("pivot2 weight", 11m, pivot2.CV_Weight);
				AssertEquals("pivot2 Volumn", 11m, pivot2.CV_Volume);
				AssertEquals("pivot2 GoodsDescription", expectGoodsDescription2, pivot2.CV_GoodsDescription);
			});
		}

		public void TestCA_VendorIdentifier_IsNotHVLV()
		{
			var hawbShipment = GetShipment();
			hawbShipment.PortOfDestination = new UNLOCO { Code = "AUSYD" };
			hawbShipment.PortOfOrigin = new UNLOCO { Code = "USCHI" };

			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsignorDocumentaryAddress",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "ARN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var logger = new DummyLogger();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, null, hawbShipment, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("", house.CA_VendorIdentifier);
			AssertEquals("CA_RL_NK_PortOfDestination", "AUSYD", house.CA_RL_NK_PortOfDestination);
			AssertEquals("CA_RL_NK_PortOfOrigin", "USCHI", house.CA_RL_NK_PortOfOrigin);
		}

		public void TestCA_VendorIdentifier_VendorIdExistsInTheXMLButConginorOrganizationNotExistsInCW1()
		{
			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbShipment.PortOfDestination = new UNLOCO { Code = "AUSYD" };
			mawbShipment.PortOfOrigin = new UNLOCO { Code = "USCHI" };
			var hawbShipment = GetShipment();
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsignorDocumentaryAddress",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "ARN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var logger = new DummyLogger();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("123456", house.CA_VendorIdentifier);
			AssertEquals("CA_RL_NK_PortOfDestination", "AUSYD", house.CA_RL_NK_PortOfDestination);
			AssertEquals("CA_RL_NK_PortOfOrigin", "USCHI", house.CA_RL_NK_PortOfOrigin);
		}

		public void TestCA_VendorIdentifier_VendorIdNotExistsInTheXMLAndConginorOrganizationExistsInCW1()
		{
			var importer = OrgHeader.LoadFromCode(Factory.BOFactory, "ABABEU");
			importer.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "11111", Core.Constants.CountryCodes.Australia);
			Factory.SaveForTesting();

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var hawbShipment = GetShipment();

			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationCode = "ABABEU",
				AddressType = "ConsignorDocumentaryAddress",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "ARN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var logger = new DummyLogger();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("123456", house.CA_VendorIdentifier);
		}

		public void TestCA_VendorIdentifier_VendorIdNotExistsInTheXMLButConginorOrganizationExistsInCW1()
		{
			var importer = OrgHeader.LoadFromCode(Factory.BOFactory, "ABABEU");
			importer.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "11111", Core.Constants.CountryCodes.Australia);
			Factory.SaveForTesting();

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var hawbShipment = GetShipment();
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OrganizationCode = "ABABEU",
					AddressType = "ConsignorDocumentaryAddress",
				}
			});

			var logger = new DummyLogger();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("11111", house.CA_VendorIdentifier);
		}

		public void TestCA_VendorIdentifier_FallBackToeTailerLevel_VendorIdExistsInTheXMLButConginorOrganizationNotExistsInCW1()
		{
			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsignorDocumentaryAddress",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "ARN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			mawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });
			var hawbShipment = GetShipment();

			var logger = new DummyLogger();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("123456", house.CA_VendorIdentifier);
		}

		public void TestCA_VendorIdentifier_FallBackToeTailerLevel_VendorIdNotExistsInTheXMLButConginorOrganizationExistsInCW1()
		{
			var importer = OrgHeader.LoadFromCode(Factory.BOFactory, "ABABEU");
			importer.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "11111", Core.Constants.CountryCodes.Australia);
			Factory.SaveForTesting();

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OrganizationCode = "ABABEU",
						AddressType = "ConsignorDocumentaryAddress",
					}
				});
			var hawbShipment = GetShipment();

			var logger = new DummyLogger();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("11111", house.CA_VendorIdentifier);
		}

		public void TestCA_VendorIdentifier_ClearIfNoMatch()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			house.CA_VendorIdentifier = "123";
			Factory.SaveForTesting();

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OrganizationCode = "ABABEU",
						AddressType = "ConsignorDocumentaryAddress",
					}
				});
			var hawbShipment = GetShipment();

			var logger = new DummyLogger();
			house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			AssertEquals("", house.CA_VendorIdentifier);
		}

		public void TestPopulatePrepaidCollectOther_WhenSCAIncoTermsIsFreightCollect_ThenIsSetToCC()
		{
			var shipment1 = GetShipment();
			var shipment2 = GetShipment();
			var shipment3 = GetShipment();
			shipment1.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.FreeOnBoard, Description = IncoTerms.Descriptions.FreeOnBoard };
			shipment2.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.FreeCarrier, Description = IncoTerms.Descriptions.FreeCarrier };
			shipment3.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.FreeCarrierSeller, Description = IncoTerms.Descriptions.FreeCarrierSeller };
			shipment1.PaymentMethod = null;
			shipment2.PaymentMethod = null;
			shipment3.PaymentMethod = null;

			CombineAssertions("Precondition: Expected ShipmentIncoTerm should be a freight collect", () =>
			{
				AssertEquals(PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment1.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment2.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment3.ShipmentIncoTerm.Code));
			});

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house1 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house2 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment2, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house3 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment3, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions("Expected PrepaidCollectOther should be CC", () =>
			{
				AssertEquals(house1.CA_PrepaidCollectOther, "CC");
				AssertEquals(house2.CA_PrepaidCollectOther, "CC");
				AssertEquals(house3.CA_PrepaidCollectOther, "CC");
			});
		}

		public void TestPopulatePrepaidCollectOther_WhenSCAIncoTermsIsFreightPrepaid_ThenIsSetToPP()
		{
			var shipment1 = GetShipment();
			var shipment2 = GetShipment();
			var shipment3 = GetShipment();
			shipment1.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.DeliveredDutyUnpaid, Description = IncoTerms.Descriptions.DeliveredDutyUnpaid };
			shipment2.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.DeliveredDutyPaid, Description = IncoTerms.Descriptions.DeliveredDutyPaid };
			shipment3.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.DeliveredAtTerminal, Description = IncoTerms.Descriptions.DeliveredAtTerminal };
			shipment1.PaymentMethod = null;
			shipment2.PaymentMethod = null;
			shipment3.PaymentMethod = null;

			CombineAssertions("Precondition: Expected ShipmentIncoTerm should be a freight prepaid", () =>
			{
				AssertEquals(PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment1.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment2.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment3.ShipmentIncoTerm.Code));
			});

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house1 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house2 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment2, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house3 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment3, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions("Expected PrepaidCollectOther should be PP", () =>
			{
				AssertEquals(house1.CA_PrepaidCollectOther, "PP");
				AssertEquals(house2.CA_PrepaidCollectOther, "PP");
				AssertEquals(house3.CA_PrepaidCollectOther, "PP");
			});
		}

		public void TestPopulatePrepaidCollectOther_WhenPaymentMethodIsFreightCollectAndIncoTermIsFreightPrepaid_ThenIsSetToCC()
		{
			var shipment1 = GetShipment();
			var shipment2 = GetShipment();
			var shipment3 = GetShipment();
			shipment1.PaymentMethod = new CodeDescriptionPair() { Code = PaymentType.Collect };
			shipment2.PaymentMethod = new CodeDescriptionPair() { Code = PaymentType.Collect };
			shipment3.PaymentMethod = new CodeDescriptionPair() { Code = PaymentType.Collect };
			shipment1.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.DeliveredDutyUnpaid, Description = IncoTerms.Descriptions.DeliveredDutyUnpaid };
			shipment2.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.DeliveredDutyPaid, Description = IncoTerms.Descriptions.DeliveredDutyPaid };
			shipment3.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.DeliveredAtTerminal, Description = IncoTerms.Descriptions.DeliveredAtTerminal };

			CombineAssertions("Precondition: Expected the payment method of the shipment to return CC", () =>
			{
				AssertEquals(CMRMethodsOfPayment.Codes.Collect, GetCMRPaymentCode((ZString)shipment1.PaymentMethod.Code));
				AssertEquals(CMRMethodsOfPayment.Codes.Collect, GetCMRPaymentCode((ZString)shipment2.PaymentMethod.Code));
				AssertEquals(CMRMethodsOfPayment.Codes.Collect, GetCMRPaymentCode((ZString)shipment3.PaymentMethod.Code));
			});

			CombineAssertions("Precondition: Expected shipment incoterms to be Freight Prepaid", () =>
			{
				AssertEquals(PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment1.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment2.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment3.ShipmentIncoTerm.Code));
			});

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house1 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house2 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment2, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house3 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment3, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions("Expected PrepaidCollectOther should be CC", () =>
			{
				AssertEquals(house1.CA_PrepaidCollectOther, "CC");
				AssertEquals(house2.CA_PrepaidCollectOther, "CC");
				AssertEquals(house3.CA_PrepaidCollectOther, "CC");
			});
		}

		public void TestPopulatePrepaidCollectOther_WhenPaymentMethodIsFreightPrepaidAndIncoTermIsFreightCollect_ThenIsSetToPP()
		{
			var shipment1 = GetShipment();
			var shipment2 = GetShipment();
			var shipment3 = GetShipment();
			shipment1.PaymentMethod = new CodeDescriptionPair() { Code = PaymentType.Prepaid };
			shipment2.PaymentMethod = new CodeDescriptionPair() { Code = PaymentType.Prepaid };
			shipment3.PaymentMethod = new CodeDescriptionPair() { Code = PaymentType.Prepaid };
			shipment1.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.FreeOnBoard, Description = IncoTerms.Descriptions.FreeOnBoard };
			shipment2.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.FreeCarrier, Description = IncoTerms.Descriptions.FreeCarrier };
			shipment3.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = IncoTerms.FreeCarrierSeller, Description = IncoTerms.Descriptions.FreeCarrierSeller };

			CombineAssertions("Precondition: Expected the payment method of the shipment to return PO", () =>
			{
				AssertEquals(CMRMethodsOfPayment.Codes.PrepaidOnly, GetCMRPaymentCode((ZString)shipment1.PaymentMethod.Code));
				AssertEquals(CMRMethodsOfPayment.Codes.PrepaidOnly, GetCMRPaymentCode((ZString)shipment2.PaymentMethod.Code));
				AssertEquals(CMRMethodsOfPayment.Codes.PrepaidOnly, GetCMRPaymentCode((ZString)shipment3.PaymentMethod.Code));
			});

			CombineAssertions("Precondition: Expected shipment incoterms to be Freight Collect", () =>
			{
				AssertEquals(PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment1.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment2.ShipmentIncoTerm.Code));
				AssertEquals(PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, shipment3.ShipmentIncoTerm.Code));
			});

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house1 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house2 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment2, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var house3 = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment3, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions("Expected PrepaidCollectOther should be PO", () =>
			{
				AssertEquals(house1.CA_PrepaidCollectOther, "PO");
				AssertEquals(house2.CA_PrepaidCollectOther, "PO");
				AssertEquals(house3.CA_PrepaidCollectOther, "PO");
			});
		}

		public void TestGetCMRPaymentCode()
		{
			var shipment1 = GetShipment();
			shipment1.PaymentMethod = new CodeDescriptionPair() { Code = CMRMethodsOfPayment.Codes.ReturnContainerFreightPaidByCustomer };
			var shipment2 = GetShipment();
			shipment2.PaymentMethod = new CodeDescriptionPair() { Code = Enterprise.Core.Constants.PaymentType.Collect };
			var shipment3 = GetShipment();
			shipment3.PaymentMethod = new CodeDescriptionPair() { Code = Enterprise.Core.Constants.PaymentType.Prepaid };

			var logger = new DummyLogger();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment1, logger, Factory).ReadIntoBusinessObject();
			AssertEquals(CMRMethodsOfPayment.Codes.ReturnContainerFreightPaidByCustomer, house.CA_PrepaidCollectOther);

			house = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment2, logger, Factory).ReadIntoBusinessObject();
			AssertEquals(CMRMethodsOfPayment.Codes.Collect, house.CA_PrepaidCollectOther);

			house = new CMRCusSCAHouseDataObjectReader(oceanBill, null, shipment3, logger, Factory).ReadIntoBusinessObject();
			AssertEquals(CMRMethodsOfPayment.Codes.PrepaidOnly, house.CA_PrepaidCollectOther);
		}

		public void TestUpdateHouseBillWithMatchedOrgAddress_EnableOrgAddressForCargoReportsOn()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			house.CA_VendorIdentifier = "123";
			Factory.SaveForTesting();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "XYZ01";
			var address1 = orgHeader.Addresses.AddNew();
			address1.Address1 = "72 O'Riordan Street";
			address1.Address2 = "Unit 3A";
			address1.City = "Alexandria";
			address1.State = "NSW";
			address1.Postcode = "2015";
			address1.OA_RN_NKCountryCode = "AU";

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var hawbShipment = GetShipment();
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress",
						OrganizationCode = "XYZ01",
						Address1 = "72 O'RIORDAN STREET"
					}
				});

			var logger = new DummyLogger();
			logger.TopLevelDataObject = hawbShipment;
			var dataContext = DataContextFactory.New();
			dataContext.CodesMappedToTarget = true;
			hawbShipment.DataContext = dataContext;
			house = new CMRCusSCAHouseDataObjectReader(oceanBill, new HVLVShipmentDataObjectWrapper(mawbShipment, null, Factory), hawbShipment, logger, Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("CA_OA_ConsignorAddress", address1.PK, house.CA_OA_ConsignorAddress);
				AssertEquals("CA_OH_Consignor", ZGuid.Empty, house.CA_OH_Consignor);
			});
		}

		Shipment GetShipment()
		{
			var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			result.WayBillNumber = "HB";
			result.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			return result;
		}

		ZString GetCMRPaymentCode(ZString paymentCode)
		{
			if (Factory.GetCachedValue<CMRMethodsOfPayment>().ContainsCode(paymentCode))
			{
				return paymentCode;
			}
			else if (paymentCode == Enterprise.Core.Constants.PaymentType.Collect)
			{
				return CMRMethodsOfPayment.Codes.Collect;
			}
			else
			{
				return CMRMethodsOfPayment.Codes.PrepaidOnly;
			}
		}

		protected override void AssertHouseBill1AllPropertiesSet(CusSCAHouse houseBill)
		{
			base.AssertHouseBill1AllPropertiesSet(houseBill);
			AssertEquals("CA_MasterHouseBill", "ABC", houseBill.CA_MasterHouseBill);
			AssertEquals("CA_IsMasterHouse", true, houseBill.CA_IsMasterHouse);
			AssertEquals("CA_PrepaidCollectOther", "CA", houseBill.CA_PrepaidCollectOther);
		}

		protected override void AssertHouseBill2AllPropertiesSet(CusSCAHouse houseBill)
		{
			base.AssertHouseBill2AllPropertiesSet(houseBill);
			AssertEquals("CA_MasterHouseBill", "", houseBill.CA_MasterHouseBill);
			AssertEquals("CA_IsMasterHouse", false, houseBill.CA_IsMasterHouse);
			AssertEquals("CA_PrepaidCollectOther", "A", houseBill.CA_PrepaidCollectOther);
		}
	}
}
