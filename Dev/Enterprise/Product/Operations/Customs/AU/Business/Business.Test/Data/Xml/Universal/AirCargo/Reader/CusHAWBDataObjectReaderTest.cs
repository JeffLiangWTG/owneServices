using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using Currency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBDataObjectReaderTest : DataTransfer.Universal.AirManifest.Testing.AirManifestDataObjectReaderTestHelper
	{
		public void TestCS_VendorIdentifier_IsNotHVLV()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
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

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, false).ReadIntoBusinessObject();
			AssertEquals("", house.CS_VendorIdentifier);
		}

		public void TestCS_VendorIdentifier()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress"
					}
				});

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("", house.CS_VendorIdentifier);

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
			house = new CusHAWBDataObjectReader(hawbShipment, mawbShipment, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("123456", house.CS_VendorIdentifier);

			mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			house = new CusHAWBDataObjectReader(hawbShipment, mawbShipment, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("", house.CS_VendorIdentifier);
		}

		public void TestCS_VendorIdentifier_VendorIdExistsInTheXMLButConsignorOrganizationNotExistsInCW1()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
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
						Code = "ABN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("123456", house.CS_VendorIdentifier);

			hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB2",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
				VendorIdentifier = "123",
			};

			var address2 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsignorDocumentaryAddress",
			};
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address2 });
			address2.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "ABN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("123", house.CS_VendorIdentifier);
		}

		public void TestCA_VendorIdentifier_VendorIdNotExistsInTheXMLAndConsignorOrganizationExistsInCW1()
		{
			var importer = OrgHeader.LoadFromCode(Factory.BOFactory, "ABABEU");
			importer.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "11111", Core.Constants.CountryCodes.Australia);

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress",
						OrganizationCode = "ABABEU",
					}
				});

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("11111", house.CS_VendorIdentifier);
		}

		public void TestCS_VendorIdentifier_FallBackToeTailerLevel_VendorIdExistsInTheXMLButConsignorOrganizationNotExistsInCW1()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress"
					}
				});

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("", house.CS_VendorIdentifier);

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsignorDocumentaryAddress",
			};
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
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
			mawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address1 });
			house = new CusHAWBDataObjectReader(hawbShipment, mawbShipment, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("123456", house.CS_VendorIdentifier);

			mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				VendorIdentifier = "123",
			};
			var address2 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsignorDocumentaryAddress",
			};
			address2.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
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
			mawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address2 });
			house = new CusHAWBDataObjectReader(hawbShipment, mawbShipment, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("123", house.CS_VendorIdentifier);
		}

		public void TestCS_VendorIdentifier_FallBackToeTailerLevel_VendorIdNotExistsInTheXMLButConsignorOrganizationExistsInCW1()
		{
			var importer = OrgHeader.LoadFromCode(Factory.BOFactory, "ABABEU");
			importer.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "11111", Core.Constants.CountryCodes.Australia);

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress"
					}
				});

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("", house.CS_VendorIdentifier);

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress",
						OrganizationCode = "ABABEU"
					}
				});
			house = new CusHAWBDataObjectReader(hawbShipment, mawbShipment, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("11111", house.CS_VendorIdentifier);
		}

		public void TestCS_VendorIdentifier_ClearIfNoMatch()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_VendorIdentifier = "123";
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress"
					}
				});

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("", house.CS_VendorIdentifier);

			var mawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			mawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress",
						OrganizationCode = "ABABEU"
					}
				});
			house = new CusHAWBDataObjectReader(hawbShipment, mawbShipment, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();
			AssertEquals("", house.CS_VendorIdentifier);
		}

		public void TestCS_ConsignorIdentifier()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
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
						Code = "CID"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, false).ReadIntoBusinessObject();
			AssertEquals("123456", house.CS_ConsignorIdentifier);
		}

		public void TestCS_ConsigneeIdentifier()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsigneeDocumentaryAddress",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "ABN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123"
				},
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "CID"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, false).ReadIntoBusinessObject();
			AssertEquals("123456", house.CS_ConsigneeIdentifier);
		}

		public void TestCS_ConsigneeBusinessNumber()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};

			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "ConsigneeDocumentaryAddress",
			};
			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "ABN"
					},
					CountryOfIssue = new Country
					{
						Code = "AU"
					},
					Value = "123456"
				}
			});
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address });

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, false).ReadIntoBusinessObject();
			AssertEquals("123456", house.CS_ConsigneeBusinessNumber);
		}

		public void TestCheckUpdateHAWBDataIsAllowed()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var reader = new CusHAWBDataObjectReaderForTest(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), null, logger, helper, mawb, hawb, true);

			logger.ClearLogs();
			reader.CheckUpdateHAWBDataIsAllowed(hawb);
			AssertEquals(false, logger.Logs.Contains("it has been edited by users"));
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			foreach (var log in hawb.Logs.GetAllLogs().Cast<StmALog>())
			{
				log.SL_GS_NKUser = User.ServiceUserCode;
			}
			logger.ClearLogs();
			reader.CheckUpdateHAWBDataIsAllowed(hawb);
			AssertEquals(false, logger.Logs.Contains("it has been edited by users"));
			AssertEquals(true, logger.Logs.Contains("it has active messaging"));

			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			logger.ClearLogs();
			reader.CheckUpdateHAWBDataIsAllowed(hawb);
			AssertEquals(false, logger.Logs.Contains("it has been edited by users"));
			AssertEquals(false, logger.Logs.Contains("it has active messaging"));

			reader = new CusHAWBDataObjectReaderForTest(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), null, logger, helper, mawb, hawb, false);
			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			logger.ClearLogs();
			reader.CheckUpdateHAWBDataIsAllowed(hawb);
			AssertEquals(false, logger.Logs.Contains("it has been edited by users"));
			AssertEquals(true, logger.Logs.Contains("it has active messaging"));

			hawb.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			logger.ClearLogs();
			reader.CheckUpdateHAWBDataIsAllowed(hawb);
			AssertEquals(false, logger.Logs.Contains("it has been edited by users"));
			AssertEquals(false, logger.Logs.Contains("it has active messaging"));
		}

		public void TestGoodsDescription()
		{
			var masterHouseDataObject1 = SetupAirCargoHouse("MH1", ZBool.True, null, null);
			masterHouseDataObject1.MessageSubType = null;
			masterHouseDataObject1.GoodsValue = 500m;
			masterHouseDataObject1.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			masterHouseDataObject1.GoodsDescription = "HELLO";

			var masterHouseDataObject2 = SetupAirCargoHouse("MH2", ZBool.True, null, null);
			masterHouseDataObject2.MessageSubType = new CodeDescriptionPair() { Code = JobDeclaration.MessageSubType.SelfAssessedClearance };
			masterHouseDataObject2.GoodsValue = 500m;
			masterHouseDataObject2.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			masterHouseDataObject2.GoodsDescription = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";

			var mawbDataObject = SetupAirCargoMaster("MB2343", null);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(masterHouseDataObject1);
			mawbDataObject.SubShipmentCollection.Add(masterHouseDataObject2);

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);
			var masterHouseBO1 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "MH1");
			AssertEquals("Goods description length shorter than 35", "HELLO", masterHouseBO1.CS_GoodsDescription);
			AssertEquals("Goods description length shorter than 35", "HELLO", ((INeedRow)masterHouseBO1).Row[CusHAWBSchema.Constants.CS_GoodsDescription]);
			AssertEquals(0, masterHouseBO1.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);

			var masterHouseBO2 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "MH2");
			AssertEquals("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", ((INeedRow)masterHouseBO2).Row[CusHAWBSchema.Constants.CS_GoodsDescription]);

			var note = masterHouseBO2.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
			AssertEquals(0, note.Length);
			AssertEquals("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
				masterHouseBO2.CS_GoodsDescription);
		}

		public void TestPopulateFreightPrepaidCollect_WhenMAWBIncoTermsIsFreightCollect_ThenFreightPrepaidCollectIsSetToCC()
		{
			var ccIncoTerm = new IncoTerm { Code = "FOB", Description = "Free On Board" };
			AssertEquals("Precondition: Checking that FOB IncoTerm is freight collect", Enterprise.Core.Constants.PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, ccIncoTerm.Code));

			var consignment1 = SetupAirCargoHouse("C1", ZBool.False, null, null);
			consignment1.MessageSubType = null;
			consignment1.ShipmentIncoTerm = ccIncoTerm;
			consignment1.GoodsValue = 1500m;
			consignment1.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			consignment1.GoodsDescription = "Goodest goods";
			consignment1.PaymentMethod = null;

			var consignment2 = SetupAirCargoHouse("C2", ZBool.False, null, null);
			consignment2.MessageSubType = null;
			consignment2.ShipmentIncoTerm = ccIncoTerm;

			var shipment = SetupAirCargoMaster("Shipment", null);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			shipment.SubShipmentCollection.Add(consignment1);
			shipment.SubShipmentCollection.Add(consignment2);
			shipment.PaymentMethod = null;

			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "Shipment");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			var expectedResults = new List<string>() { CMRMethodsOfPayment.Codes.Collect, CMRMethodsOfPayment.Codes.Collect };
			var freightPrepaidCollectList = mawbBO?.ChildBills.Cast<CusHAWB>().Select(hawb => hawb.CS_FreightPrepaidCollect).ToList();

			AssertContainsExactElementsInAnyOrder(expectedResults, freightPrepaidCollectList);
		}

		public void TestPopulateFreightPrepaidCollect_WhenMAWBIncoTermsIsFreightPrepaid_ThenFreightPrepaidCollectIsSetToPP()
		{
			var ppIncoTerm = new IncoTerm { Code = "CFR", Description = "Cost And Freight" };
			AssertEquals("Precondition: Checking that CFR IncoTerm is freight prepaid", Enterprise.Core.Constants.PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, ppIncoTerm.Code));

			var consignment1 = SetupAirCargoHouse("C1", ZBool.False, null, null);
			consignment1.MessageSubType = null;
			consignment1.ShipmentIncoTerm = ppIncoTerm;
			consignment1.GoodsValue = 1500m;
			consignment1.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			consignment1.GoodsDescription = "Really good goods";
			consignment1.PaymentMethod = null;

			var consignment2 = SetupAirCargoHouse("C2", ZBool.False, null, null);
			consignment2.MessageSubType = null;
			consignment2.ShipmentIncoTerm = ppIncoTerm;
			consignment2.PaymentMethod = null;

			var shipment = SetupAirCargoMaster("Shipment", null);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			shipment.SubShipmentCollection.Add(consignment1);
			shipment.SubShipmentCollection.Add(consignment2);

			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "Shipment");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			var expectedResults = new List<string>() { CMRMethodsOfPayment.Codes.PrepaidBySeller, CMRMethodsOfPayment.Codes.PrepaidBySeller };
			var freightPrepaidCollectList = mawbBO?.ChildBills.Cast<CusHAWB>().Select(hawb => hawb.CS_FreightPrepaidCollect).ToList();

			AssertContainsExactElementsInAnyOrder(expectedResults, freightPrepaidCollectList);
		}

		public void TestPopulateFreightPrepaidCollect_WhenMAWBHasBothIncoTermsAndPaymentMethod_ThenPaymentMethodDecidesValueOfPrepaidPrepaidCollect()
		{
			var ppIncoTerm = new IncoTerm { Code = "CFR", Description = "Cost And Freight" };
			AssertEquals("Precondition: Checking CFR is a freight prepaid", Enterprise.Core.Constants.PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, ppIncoTerm.Code));

			var consignment = SetupAirCargoHouse("C1", ZBool.False, null, null);
			consignment.MessageSubType = null;
			consignment.ShipmentIncoTerm = ppIncoTerm;
			consignment.GoodsValue = 1500m;
			consignment.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			consignment.GoodsDescription = "Our goods ☭";
			consignment.PaymentMethod = new CodeDescriptionPair() { Code = Core.Constants.PaymentType.Collect };

			var shipment = SetupAirCargoMaster("Shipment", null);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			shipment.SubShipmentCollection.Add(consignment);

			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "Shipment");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			var expectedResults = CMRMethodsOfPayment.Codes.Collect;
			var cusHawb = mawbBO?.ChildBills.Cast<CusHAWB>().Single();

			AssertEquals(expectedResults, cusHawb.CS_FreightPrepaidCollect);
		}

		public void TestSelfAssessedClearanceCalculation()
		{
			TaxOrFeeTestHelper.SetUp();
			CMRReferenceFilesTestHelper.InsertThesaurusData(new BusinessObjectFactory(), "Bomb");

			var masterHouseDataObject1 = SetupAirCargoHouse("MH1", ZBool.True, null, null);
			masterHouseDataObject1.MessageSubType = null;
			masterHouseDataObject1.GoodsValue = 500m;
			masterHouseDataObject1.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			masterHouseDataObject1.GoodsDescription = "HELLO";

			var masterHouseDataObject2 = SetupAirCargoHouse("MH2", ZBool.True, null, null);
			masterHouseDataObject2.MessageSubType = new CodeDescriptionPair() { Code = JobDeclaration.MessageSubType.SelfAssessedClearance };
			masterHouseDataObject2.GoodsValue = 500m;
			masterHouseDataObject2.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			masterHouseDataObject2.GoodsDescription = "HELLO";

			var subHawbDataObject1 = SetupAirCargoHouse2("SB1", ZBool.False, "MH1", null);
			subHawbDataObject1.MessageSubType = null;
			subHawbDataObject1.GoodsValue = 500m;
			subHawbDataObject1.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			subHawbDataObject1.GoodsDescription = "HELLO";

			masterHouseDataObject1.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			masterHouseDataObject1.SubShipmentCollection.Add(subHawbDataObject1);

			var hawbDataObject1 = SetupAirCargoHouse2("HB1", ZBool.False, null, null);
			hawbDataObject1.MessageSubType = null;
			hawbDataObject1.GoodsValue = 1500m;
			hawbDataObject1.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			hawbDataObject1.GoodsDescription = "HELLO";

			var hawbDataObject2 = SetupAirCargoHouse2("HB2", ZBool.False, null, null);
			hawbDataObject2.MessageSubType = null;
			hawbDataObject2.GoodsValue = 500m;
			hawbDataObject2.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			hawbDataObject2.GoodsDescription = "BOMB";

			var hawbDataObject3 = SetupAirCargoHouse2("HB3", ZBool.False, null, null);
			hawbDataObject3.MessageSubType = null;
			hawbDataObject3.GoodsValue = 500m;
			hawbDataObject3.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			hawbDataObject3.GoodsDescription = "HELLO";

			var hawbDataObject4 = SetupAirCargoHouse2("HB4", ZBool.False, null, null);
			hawbDataObject4.MessageSubType = new CodeDescriptionPair() { Code = JobDeclaration.MessageSubType.SelfAssessedClearance };
			hawbDataObject4.GoodsValue = 1500m;
			hawbDataObject4.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			hawbDataObject4.GoodsDescription = "BOMB";

			var mawbDataObject = SetupAirCargoMaster("MB2343", null);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(masterHouseDataObject1);
			mawbDataObject.SubShipmentCollection.Add(masterHouseDataObject2);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject1);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject2);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject3);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject4);

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);
			AssertEquals("mawbBO.ChildBills.Count", 7, mawbBO.ChildBills.Count);
			var masterHouseBO1 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "MH1");
			AssertEquals("masterHouseBO1.CS_IsSelfAssessedClearance as is masterhouse", ZBool.False, masterHouseBO1.CS_IsSelfAssessedClearance);

			var masterHouseBO2 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "MH2");
			AssertEquals("masterHouseBO2.CS_IsSelfAssessedClearance as specified in xml", ZBool.True, masterHouseBO2.CS_IsSelfAssessedClearance);

			var subHawbBO1 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "SB1");
			AssertEquals("subHawbBO1.CS_IsSelfAssessedClearance", ZBool.True, subHawbBO1.CS_IsSelfAssessedClearance);

			var hawbBO1 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB1");
			AssertEquals("hawbBO1.CS_IsSelfAssessedClearance value is greater than 1000", ZBool.False, hawbBO1.CS_IsSelfAssessedClearance);

			var hawbBO2 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB2");
			AssertEquals("hawbBO2.CS_IsSelfAssessedClearance has thesaurus word 'BOMB'", ZBool.False, hawbBO2.CS_IsSelfAssessedClearance);

			var hawbBO3 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB3");
			AssertEquals("hawbBO3.CS_IsSelfAssessedClearance less than 1000 and no thesaurus word", ZBool.True, hawbBO3.CS_IsSelfAssessedClearance);

			var hawbBO4 = mawbBO.ChildBills.Cast<CusHAWB>().FirstOrDefault(x => x.CS_HAWB == "HB4");
			AssertEquals("hawbBO4.CS_IsSelfAssessedClearance specified in xml", ZBool.True, hawbBO4.CS_IsSelfAssessedClearance);
		}

		public void TestCustomsStatusIsUpdatedWhenThereIsCargoReportingMessage()
		{
			var hawbDataObject = SetupAirCargoHouse2("HB1", ZBool.False, null, null);
			hawbDataObject.ConsolidatedCargoStatus = new CodeDescriptionPair() { Code = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased };

			var mawbDataObject = SetupAirCargoMaster("MB2343", null);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);
			AssertEquals("mawbBO.ChildBills.Count", 1, mawbBO.ChildBills.Count);
			var hawbBO = mawbBO.ChildBills[0];
			AssertEquals("hawbBO.CS_CustomsStatus", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, hawbBO.CS_CustomsStatus);

			hawbBO.CS_CustomsStatus = ZString.Empty;
			var airCargoMessage = hawbBO.Messages.AddNew();
			airCargoMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			airCargoMessage.EM_MessageType = CMRMessage.CMRMessageTypes.AIRCR;
			airCargoMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.SaveForTesting();
			message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			hawbBO.Reload();
			AssertEquals("hawbBO.CS_CustomsStatus", AirCargoMessage.NewStatus.NotSent, hawbBO.CS_CustomsStatus);

			airCargoMessage.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			hawbBO.CS_CustomsStatus = ZString.Empty;
			Factory.SaveForTesting();
			message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			hawbBO.Reload();
			AssertEquals("hawbBO.CS_CustomsStatus", ZString.Empty, hawbBO.CS_CustomsStatus);
		}

		public void TestFreightPrepaidCollectCalculation()
		{
			var hawbDataObject = SetupAirCargoHouse2("HB1", ZBool.False, null, null);
			hawbDataObject.MessageSubType = null;
			hawbDataObject.GoodsValue = 1500m;
			hawbDataObject.GoodsValueCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			hawbDataObject.GoodsDescription = "HELLO";
			hawbDataObject.PaymentMethod = new CodeDescriptionPair() { Code = Core.Constants.PaymentType.Prepaid };

			var mawbDataObject = SetupAirCargoMaster("MB2343", null);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);
			AssertEquals("mawbBO.ChildBills.Count", 1, mawbBO.ChildBills.Count);
			var masterHouseBO = mawbBO.ChildBills[0];
			AssertEquals("masterHouseBO.CS_FreightPrepaidCollect", CMRMethodsOfPayment.Codes.PrepaidOnly, masterHouseBO.CS_FreightPrepaidCollect);
		}

		public void TestImportingCusHAWBData()
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.True, null, "RIP324324");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var receivingAgent = hawbDataObject.AddOrgAddress(writeManager, org1, DocAddressType.ReceivingForwarderAddress);
			var sendingAgent = SetupOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress));
			sendingAgent.Contact = "BOB THE BUILDER";
			sendingAgent.Phone = "3234 23432";
			hawbDataObject.OrganizationAddressCollection.Add(sendingAgent);

			var subHawbDataObject = SetupAirCargoHouse2("SB8965", ZBool.False, "HB2343", "IDS96854");
			var consignorData = subHawbDataObject.AddOrgAddress(writeManager, org2, DocAddressType.ConsignorDocumentaryAddress);
			var consigneeData = SetupOrganizationAddress2(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			consigneeData.Contact = "WENDY THE DESTROYER";
			consigneeData.Phone = "9685 5744";
			subHawbDataObject.OrganizationAddressCollection.Add(consigneeData);

			hawbDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>
			{
				CustomizedField.New("HAWBCustomField1", (ZString)"CustomField1Value"),
				CustomizedField.New("HAWBCustomField2", (ZString)"CustomField2Value")
			});

			hawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			hawbDataObject.SubShipmentCollection.Add(subHawbDataObject);
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);
			AssertEquals("mawbBO.ChildBills.Count", 2, mawbBO.ChildBills.Count);
			var hawbBO = mawbBO.ChildBills[0];
			var subHawbBO = mawbBO.ChildBills[1];
			if (subHawbBO.CS_CS_MasterHouseBill.IsEmpty)
			{
				hawbBO = mawbBO.ChildBills[1];
				subHawbBO = mawbBO.ChildBills[0];
			}

			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertCusHAWBContents(hawbBO, "HB2343", ZBool.True, ZString.Empty, "RIP324324");
				AssertEquals("hawbBO.CS_CS_MasterHouseBill", ZGuid.Empty, hawbBO.CS_CS_MasterHouseBill);
				AssertCusHAWBConsignee(hawbBO, org1.OH_FullName, org1.MainAddress.OA_Address1, org1.MainAddress.OA_Address2, org1.MainAddress.OA_City, org1.MainAddress.OA_State,
					org1.MainAddress.OA_PostCode, org1.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), ZString.Empty, org1.MainAddress.OA_Phone, org1.MainAddress.PK);
				AssertCusHAWBConsignor(hawbBO, "In The Moment", "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", "AU", "BOB THE BUILDER", "+61323423432", ZGuid.Empty);
				AssertEquals("hawbBO.HAWBCustomField1", "CustomField1Value", hawbBO.GetUserDefinedValue<ZString>("HAWBCustomField1"));
				AssertEquals("hawbBO.HAWBCustomField2", "CustomField2Value", hawbBO.GetUserDefinedValue<ZString>("HAWBCustomField2"));

				AssertCusHAWBContents2(subHawbBO, "SB8965", ZBool.False, "HB2343", "IDS96854");
				AssertEquals("subHawbBO.CS_CS_MasterHouseBill", hawbBO.PK, subHawbBO.CS_CS_MasterHouseBill);
				AssertCusHAWBConsignor(subHawbBO, org2.OH_FullName, org2.MainAddress.OA_Address1, org2.MainAddress.OA_Address2, org2.MainAddress.OA_City, org2.MainAddress.OA_State,
					org2.MainAddress.OA_PostCode, org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), ZString.Empty, org2.MainAddress.OA_Phone, org2.MainAddress.PK);
				AssertCusHAWBConsignee(subHawbBO, "Too Late To Apologise", "Unit 24, Level 10", "455 There St", "Big City", "Small State", "56845", "US", "WENDY THE DESTROYER", "9685 5744", ZGuid.Empty);
				var subHawbBOs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CS_MasterHouseBill, subHawbBO.PK));
				AssertEquals("subHawbBOs.Length", 0, subHawbBOs.Length);

				AssertMultilineASCIIEquals("logger.Logs", @"
No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'SendingForwarderAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Successfully matched organization with code 'WUFSHIJNB'.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Successfully matched organization with code 'CRAHOLSYD'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Added Air Cargo House (HAWB: SB8965 MHB: HB2343) from UniversalShipment.
Added Air Cargo House (HAWB: HB2343) from UniversalShipment.
Added AirCargo Report (MAWB: MB2-343 MHB: CL343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2-343 MHB: CL343) with 2 x CusHAWB.
".Trim(), message.GetLogNoteText());
			});

			#endregion
		}

		protected override void AssertCusHAWBConsignor(Customs.Business.CusHAWB hawbBO, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString contactName, ZString phone, ZGuid addressPK)
		{
			AssertEquals("hawbBO.CS_ConsignorName", name.ToUpper(), hawbBO.CS_ConsignorName.ToUpper());
			AssertEquals("hawbBO.CS_ConsignorStreet", address1.ToUpper(), hawbBO.CS_ConsignorStreet.ToUpper());
			AssertEquals("hawbBO.CS_ConsignorStreet2", address2.ToUpper(), hawbBO.CS_ConsignorStreet2.ToUpper());
			AssertEquals("hawbBO.CS_ConsignorCity", city.ToUpper(), hawbBO.CS_ConsignorCity.ToUpper());
			AssertEquals("hawbBO.CS_ConsignorState", state.ToUpper(), hawbBO.CS_ConsignorState.ToUpper());
			AssertEquals("hawbBO.CS_ConsignorPostcode", postCode.ToUpper(), hawbBO.CS_ConsignorPostcode.ToUpper());
			AssertEquals("hawbBO.CS_RN_NKConsignorCountry", countryCode.ToUpper(), hawbBO.CS_RN_NKConsignorCountry.ToUpper());
			AssertEquals("hawbBO.CS_ConsignorContactName", contactName.ToUpper(), hawbBO.CS_ConsignorContactName.ToUpper());
			AssertEquals("hawbBO.CS_ConsignorPhone", phone, hawbBO.CS_ConsignorPhone);
			AssertEquals("hawbBO.CS_OA_ConsignorAddress", addressPK, hawbBO.CS_OA_ConsignorAddress);
		}

		protected override void AssertCusHAWBConsignee(Customs.Business.CusHAWB hawbBO, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString contactName, ZString phone, ZGuid addressPK)
		{
			AssertEquals("hawbBO.CS_ConsigneeName", name.ToUpper(), hawbBO.CS_ConsigneeName.ToUpper());
			AssertEquals("hawbBO.CS_ConsigneeStreet", address1.ToUpper(), hawbBO.CS_ConsigneeStreet.ToUpper());
			AssertEquals("hawbBO.CS_ConsigneeStreet2", address2.ToUpper(), hawbBO.CS_ConsigneeStreet2.ToUpper());
			AssertEquals("hawbBO.CS_ConsigneeCity", city.ToUpper(), hawbBO.CS_ConsigneeCity.ToUpper());
			AssertEquals("hawbBO.CS_ConsigneeState", state.ToUpper(), hawbBO.CS_ConsigneeState.ToUpper());
			AssertEquals("hawbBO.CS_ConsigneePostcode", postCode.ToUpper(), hawbBO.CS_ConsigneePostcode.ToUpper());
			AssertEquals("hawbBO.CS_RN_NKConsigneeCountry", countryCode.ToUpper(), hawbBO.CS_RN_NKConsigneeCountry.ToUpper());
			AssertEquals("hawbBO.CS_ConsigneeContactName", contactName.ToUpper(), hawbBO.CS_ConsigneeContactName.ToUpper());
			AssertEquals("hawbBO.CS_ConsigneePhone", phone, hawbBO.CS_ConsigneePhone);
			AssertEquals("hawbBO.CS_OA_ConsigneeAddress", addressPK, hawbBO.CS_OA_ConsigneeAddress);
		}

		public void TestUpdateHouseBillWithMatchedOrgAddress_EnableOrgAddressForCargoReportsOn()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
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

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			hawbShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "ConsignorDocumentaryAddress",
						OrganizationCode = "XYZ01",
						Address1 = "72 O'RIORDAN STREET"
					}
				});

			logger.TopLevelDataObject = hawbShipment;
			var dataContext = DataContextFactory.New();
			dataContext.CodesMappedToTarget = true;
			hawbShipment.DataContext = dataContext;

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("CS_OA_ConsignorAddress", address1.PK, house.CS_OA_ConsignorAddress);
				AssertEquals("CS_OH_Consignor", ZGuid.Empty, house.CS_OH_Consignor);
			});
		}

		public void TestWarehouseLocation_WhenUXMLExceedsMaxSize_ShouldTruncateWithoutErrors()
		{
			var validWarehouseLocation = "WAREHOUSE1";

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.SaveForTesting();

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var hawbShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			hawbShipment.WarehouseLocation = "WAREHOUSE1 location exceeds 10chars from UXML";

			logger.TopLevelDataObject = hawbShipment;
			var dataContext = DataContextFactory.New();
			dataContext.CodesMappedToTarget = true;
			hawbShipment.DataContext = dataContext;

			var house = new CusHAWBDataObjectReader(hawbShipment, null, logger, helper, mawb, hawb, true).ReadIntoBusinessObject();

			AssertEquals("Warehouse Location should only contain 10 characters.", validWarehouseLocation, house.CS_WarehouseLocation);
			AssertEquals("Warehouse Location should have correct length.", 10, house.CS_WarehouseLocation.Length);
		}

		Shipment SetupAirCargoHouse(ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse, ZString? responsibleID)
		{
			return SetupAirCargoHouse(wayBillNumber, isMasterHouse, masterHouse, new UNLOCO() { Code = AirForeignPort2.RL_Code }, new UNLOCO() { Code = AirLocalPort3.RL_Code },
				1500.60m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 350, "GOODS FOR TESTING", 1304.50m, LocalCurrency, responsibleID, new CodeDescriptionPair() { Code = Business.AirCargo.Constants.ShipmentSubTypes.IsSpecialReporter },
				new CodeDescriptionPair() { Code = JobDeclaration.MessageSubType.SelfAssessedClearance }, new CodeDescriptionPair() { Code = CMRMethodsOfPayment.Codes.Collect }, new CodeDescriptionPair() { Code = "DOC" },
					ZBool.False, "WR324", "FL34", 2000.40m, new ServiceLevel() { Code = ServiceLevel1.RS_Code });
		}

		Shipment SetupAirCargoHouse2(ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse, ZString? responsibleID)
		{
			return SetupAirCargoHouse(wayBillNumber, isMasterHouse, masterHouse, new UNLOCO() { Code = AirForeignPort1.RL_Code }, new UNLOCO() { Code = AirLocalPort1.RL_Code },
				850.60m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds }, 350, "GOODS FOR TESTING 2", 1880.50m, ForeignCurrency, responsibleID, new CodeDescriptionPair() { Code = Business.AirCargo.Constants.ShipmentSubTypes.IsNotSpecialReporter },
				new CodeDescriptionPair() { Code = JobDeclaration.MessageSubType.FormalEntry }, new CodeDescriptionPair() { Code = CMRMethodsOfPayment.Codes.PrepaidBySeller }, new CodeDescriptionPair() { Code = "STD" },
					ZBool.True, "WR896", "FL98", 890.60m, null);
		}

		Shipment SetupAirCargoHouse(ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse, UNLOCO portOfOrigin, UNLOCO portOfDestination, ZDecimal? weight, UnitOfWeight weightUQ, ZInt? piecesManifested, ZString? goodsDescription, ZDecimal? goodsValue, Currency goodsValueCurrency, ZString? responsitresponsiblePartyID,
			CodeDescriptionPair shipmentSubType, CodeDescriptionPair messageSubType, CodeDescriptionPair freightPrepaidCollect, CodeDescriptionPair shipmentType, ZBool? isPersonalEffects, ZString? warehouseLocation, ZString? folioReference, ZDecimal? chargableWeight, ServiceLevel serviceLevel)
		{
			var result = SetupAirCargoHouse(wayBillNumber, isMasterHouse, masterHouse, portOfOrigin, portOfDestination, weight, weightUQ, piecesManifested, goodsDescription, goodsValue, goodsValueCurrency, responsitresponsiblePartyID);
			result.ShipmentSubType = shipmentSubType;
			result.MessageSubType = messageSubType;
			result.PaymentMethod = freightPrepaidCollect;
			result.ShipmentType = shipmentType;
			result.IsPersonalEffects = isPersonalEffects;
			result.WarehouseLocation = warehouseLocation;
			result.Folio = folioReference;
			result.ActualChargeable = chargableWeight;
			result.ServiceLevel = serviceLevel;

			return result;
		}

		void AssertCusHAWBContents(CusHAWB hawbBO, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse, ZString responsibleID)
		{
			AssertCusHAWBContents(hawbBO, wayBillNumber, isMasterHouse, masterHouse, AirForeignPort2.RL_Code, AirLocalPort3.RL_Code,
				1500.60m, Core.Constants.Weight.Kilograms, 350, "GOODS FOR TESTING", 1304.50m, JobDeclaration.LocalCurrencyConstantCode,
				responsibleID, ZBool.True, ZBool.True, CMRMethodsOfPayment.Codes.Collect, "DOC", ZBool.False, "WR324", "FL34", 2000.40m, ServiceLevel1.RS_Code);
		}

		void AssertCusHAWBContents2(CusHAWB hawbBO, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse, ZString responsibleID)
		{
			AssertCusHAWBContents(hawbBO, wayBillNumber, isMasterHouse, masterHouse, AirForeignPort1.RL_Code, AirLocalPort1.RL_Code, 850.60m, Core.Constants.Weight.Pounds,
				350, "GOODS FOR TESTING 2", 1880.50m, ForeignCurrencyBO.RX_Code, responsibleID, ZBool.False, ZBool.False, CMRMethodsOfPayment.Codes.PrepaidBySeller,
				"STD", ZBool.True, "WR896", "FL98", 890.60m, ZString.Empty);
		}

		void AssertCusHAWBContents(CusHAWB hawbBO, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse, ZString origin, ZString destination, ZDecimal weight, ZString weightUQ, ZShort piecesManifested, ZString goodsDescription, ZDecimal goodsValue, ZString goodsCurrency, ZString responsiblePartyID,
			ZBool isSpecialReporter, ZBool isSelfAssessedClearance, ZString freightPrepaidCollect, ZString shipmentType, ZBool isPersonalEffects, ZString warehouseLocation, ZString folioReference, ZDecimal chargableWeight, ZString serviceLevel)
		{
			AssertCusHAWBContents(hawbBO, wayBillNumber, isMasterHouse, masterHouse, origin, destination, weight, weightUQ, piecesManifested, goodsDescription, goodsValue, goodsCurrency, responsiblePartyID);
			AssertEquals("hawbBO.CS_IsSpecialReporter", isSpecialReporter, hawbBO.CS_IsSpecialReporter);
			AssertEquals("hawbBO.CS_IsSelfAssessedClearance", isSelfAssessedClearance, hawbBO.CS_IsSelfAssessedClearance);
			AssertEquals("hawbBO.CS_FreightPrepaidCollect", freightPrepaidCollect, hawbBO.CS_FreightPrepaidCollect);
			AssertEquals("hawbBO.CS_ShipmentType", shipmentType, hawbBO.CS_ShipmentType);
			AssertEquals("hawbBO.CS_IsPersonalEffects", isPersonalEffects, hawbBO.CS_IsPersonalEffects);
			AssertEquals("hawbBO.CS_WarehouseLocation", warehouseLocation, hawbBO.CS_WarehouseLocation);
			AssertEquals("hawbBO.CS_FolioReference", folioReference, hawbBO.CS_FolioReference);
			AssertEquals("hawbBO.CS_ChargableWeight", chargableWeight, hawbBO.CS_ChargableWeight);
			AssertEquals("hawbBO.CS_RS_NK_ServiceLevel", serviceLevel, hawbBO.CS_RS_NK_ServiceLevel);
		}

		sealed class CusHAWBDataObjectReaderForTest : CusHAWBDataObjectReader
		{
			public CusHAWBDataObjectReaderForTest(Shipment shipmentDataObject, Shipment mawbDataObject, IXmlImportLogger logger, AirManifestDataObjectReaderHelper helper, CusMAWB mawb, CusHAWB masterHouse, bool isHVLV, bool singleHAWBCheck = false) : base(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterHouse, isHVLV, singleHAWBCheck)
			{
			}

			public new bool CheckUpdateHAWBDataIsAllowed(CusHAWB hawb) => base.CheckUpdateHAWBDataIsAllowed(hawb);
		}
	}
}
