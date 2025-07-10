using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestImportAddInfo()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo()
				{
					Key = "ServiceOption",
					Value = "IID"
				},
				new AddInfo()
				{
					Key = "AssesmentOption",
					Value = "1"
				},
				new AddInfo()
				{
					Key = "ATDExCode",
					Value = "A1"
				},
				new AddInfo()
				{
					Key = "HCInd",
					Value = "Y"
				},
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			Assert(declarationBO.IsInDatabase);
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
			CombineAssertions(() =>
			{
				AssertEquals("IID", declaration.CA_ServiceOption);
				AssertEquals("1", declaration.CA_AssesmentOption);
				AssertEquals("A1", declaration.CA_ATDExCode);
				AssertNotContains("HCInd", declaration.JE_AddInfo);
			});
		}

		public void TestSetIsFromNumbersTabForCargoControlNumber()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);

			declarationDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference()
				{
					ReferenceNumber = "CCN111",
					Type = new EntryType() { Code = "CCN" }
				}
			});

			declarationDataObject.SetCustomsReferenceCollection(() => new List<CustomsReference>()
			{
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "CCN111" },
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "CCN222" }
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(1, declarationBO.AdditionalReferenceNumbers.Count);
				var cusEntryNum = declarationBO.AdditionalReferenceNumbers[0];
				AssertEquals("CCN", cusEntryNum.CE_EntryType);
				AssertEquals("CCN111", cusEntryNum.CE_EntryNum);

				AssertEquals(2, declarationBO.CargoControlNumbers.Count);
				var addInfo1 = declarationBO.CargoControlNumbers.First(x => x.CA_CCNInfoNumber == "CCN111");
				Assert(addInfo1.CA_IsFromNumbersTab);

				var addInfo2 = declarationBO.CargoControlNumbers.First(x => x.CA_CCNInfoNumber == "CCN222");
				Assert(!addInfo2.CA_IsFromNumbersTab);
			});
		}

		public void TestImportDeclaration()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			declarationDataObject.SetDateCollection(() => new List<Date>()
			{
				new Date() { Type = DateType.WarehouseRelease, IsEstimate = false, Value = new ZDateTime(2015, 9, 28, 0, 34, 0) },
				new Date() { Type = DateType.EntryAuthorisation, IsEstimate = false, Value = new ZDateTime(2015, 10, 1) }
			});
			declarationDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>()
			{
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "CAC" },
					AddInfoCollection = new List<AddInfo>() { new AddInfo() { Key = "CCNInfoNumber", Value = "80367346464544" } }
				},
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "CAC" },
					AddInfoCollection = new List<AddInfo>() { new AddInfo() { Key = "CCNInfoNumber", Value = "80367346464544B" } }
				}
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(new ZDateTime(2015, 9, 28, 0, 34, 0), declarationBO.JE_WarehouseReleaseDate);
				AssertEquals(new ZDateTime(2015, 10, 1), declarationBO.JE_EntryAuthorisationDate);
				AssertEquals(2, declarationBO.CargoControlNumbers.Count);
				Assert(declarationBO.CargoControlNumbers.Any(x => x.CA_CCNInfoNumber == "80367346464544"));
				Assert(declarationBO.CargoControlNumbers.Any(x => x.CA_CCNInfoNumber == "80367346464544B"));
			});
		}

		public void TestCustomReferenceCCNNumberReader()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = "NetWeight", Value = "24.900" }
				}
			};
			invoiceHeaderDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine()
				{
					CountryOfOrigin = new Country() { Code = "US" },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "99TariffCode", Value = "9912" }
					}
				}
			});

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { invoiceHeaderDataObject }
			};

			var refCollection = declarationDataObject.CommercialInfo.CommercialInvoiceCollection.First();
			refCollection.CustomsReferenceCollection = new List<CustomsReference>()
			{
						new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "HHH111" },
						new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "HHH222" }
			};

			declarationDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>()
			{
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "CAC" },
					AddInfoCollection = new List<AddInfo>() { new AddInfo() { Key = "CCNInfoNumber", Value = "80367346464544" } }
				},
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = "CAC" },
					AddInfoCollection = new List<AddInfo>() { new AddInfo() { Key = "CCNInfoNumber", Value = "80367346464544B" } }
				}
			});

			declarationDataObject.SetCustomsReferenceCollection(() => new List<CustomsReference>()
			{
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "CCN111" },
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "CCN222" }
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(2, declarationBO.CargoControlNumbers.Count);

				Assert("CCN 1", declarationBO.CargoControlNumbers.Any(x => x.CA_CCNInfoNumber == "CCN111"));
				Assert("CCN 2", declarationBO.CargoControlNumbers.Any(x => x.CA_CCNInfoNumber == "CCN222"));
				Assert("AddInfo CCN 1", !declarationBO.CargoControlNumbers.Any(x => x.CA_CCNInfoNumber == "80367346464544"));
				Assert("AddInfo CCN 2", !declarationBO.CargoControlNumbers.Any(x => x.CA_CCNInfoNumber == "80367346464544B"));
			});

			AssertEquals(1, declarationBO.Invoices.Count);
			var invoiceBO = declarationBO.Invoices[0];
			AssertEquals(2, invoiceBO.CargoControlNumbersList.Count);

			if (invoiceBO.CargoControlNumbersList[0].J2_ReferenceNumber.Equals("HHH111"))
			{
				AssertEquals("HHH111", invoiceBO.CargoControlNumbersList[0].J2_ReferenceNumber);
				AssertEquals("HHH222", invoiceBO.CargoControlNumbersList[1].J2_ReferenceNumber);
			}
			else
			{
				AssertEquals("HHH222", invoiceBO.CargoControlNumbersList[0].J2_ReferenceNumber);
				AssertEquals("HHH111", invoiceBO.CargoControlNumbersList[1].J2_ReferenceNumber);
			}
		}

		public void TestB2AdjustmentDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mailToOrg = Factory.New<OrgHeader>();
			mailToOrg.OH_Code = "INCMAILTO";
			declaration.JE_OH_NotifyParty = mailToOrg.PK;

			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.B2Adjustments, ZString.Empty);
			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			declarationDataObject.AddOrgAddress(writingManager, declaration.NotifyParty, Constants.AddressType.MailTo);
			declarationDataObject.SetDateCollection(() => new List<Date>()
			{
				new Date() { Type = DateType.EntryAuthorisation, IsEstimate = false, Value = new ZDateTime(2015, 10, 1) }
			});

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
				AssertEquals("INCMAILTO", newDeclarationBO.NotifyParty.OH_Code);
			});
		}

		public void TestFillRealFieldFromAddInfo()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo()
				{
					Key = Constants.AddInfoKeys.Declaration.PortOfClearance,
					Value = "0809"
				},
				new AddInfo()
				{
					Key = Constants.AddInfoKeys.Declaration.SubLocationCode,
					Value = "3368"
				},
				new AddInfo()
				{
					Key = Constants.AddInfoKeys.Declaration.PARSETA,
					Value = "2018-07-11T00:00:00"
				}
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("0809", declarationBO.JE_CustomsOffice);
				AssertEquals("3368", declarationBO.JE_LocationOfGoods);
				AssertEquals(new ZDateTime(2018, 7, 11), declarationBO.JE_DateOfFirstArrival);
			});
		}

		public void TestFillServiceOptionFromAddInfo()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>());
			declarationDataObject.AddInfoCollection.Add(new AddInfo() { Key = CAAddInfoSchema.CA_ServiceOption.Name.Substring(3), Value = ServiceOptions.Codes.IID });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals(ServiceOptions.Codes.IID, declarationBO.CA_ServiceOption);
		}

		public void TestNotImportInvoiceLineLevelPackagePivotsWhenInvoiceLevelDataExist()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "OB323",
					ContainerNumber = "CONT1",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					Link = 1,
					PackQty = 555
				};
				packingLine.SetPackedItemCollection(() => new List<PackedItem>()
				{
					new PackedItem()
					{
						CommercialInvoiceLineLink = 1,
						PackedQuantity = 555
					}
				});
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MessageType = new CodeDescriptionPair()
					{
						Code = "IMP",
						Description = "Import"
					},
					WayBillNumber = "OB323",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>
					{
						new AddInfo { Key = CAAddInfoSchema.CA_ServiceOption.Name.Substring(3), Value = ACROSSServiceOptions.Codes.IID }
					});
				declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[]
					{
						new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT1" }
					}));
				declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packingLine });
				declarationDataObject.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								PackingLinkCollection = new List<PackingLink>()
								{
									new PackingLink()
									{
										PackingLineLink = 1,
										PackedQuantity = 444
									}
								},
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>()
								{
									new CommercialInvoiceLine()
									{
										Link = 1
									}
								}))
						}
				};

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoice = declarationBO.Invoices[0];
				var invoiceLine = invoice.InvoiceLines[0];

				AssertEquals("Invoice line level data isn't imported", 0, invoiceLine.PackagesPivot.Count);
				AssertEquals("Invoice level data is imported", 1, invoice.PackagesPivot.Count);
				var pivot = (InvoiceHeaderPackagePivot)invoice.PackagesPivot.ToArray()[0];
				AssertEquals(declarationBO.PK, pivot.CHZ_JE);
				AssertEquals(invoice.PK, pivot.CHZ_JZ);
				AssertEquals(444, pivot.CHZ_NumberOfPacks);
			}
		}

		public void TestImportInvoiceLevelPackagePivots()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MessageType = new CodeDescriptionPair()
					{
						Code = "IMP",
						Description = "Import"
					},
					WayBillNumber = "OB323",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};

				declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>
					{
						new AddInfo { Key = CAAddInfoSchema.CA_ServiceOption.Name.Substring(3), Value = ACROSSServiceOptions.Codes.IID }
					});
				declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[]
				{
					new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT1" }
				}));
				declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							BillNumber = "OB323",
							ContainerNumber = "CONT1",
							BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
							Link = 1,
							PackQty = 555
						}
					});
				declarationDataObject.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								PackingLinkCollection = new List<PackingLink>()
								{
									new PackingLink()
									{
										PackingLineLink = 1,
										PackedQuantity = 444
									}
								}
							}
						}
				};

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoice = declarationBO.Invoices[0];

				AssertEquals(1, invoice.PackagesPivot.Count);
				var pivot = (InvoiceHeaderPackagePivot)invoice.PackagesPivot.ToArray()[0];
				AssertEquals(declarationBO.PK, pivot.CHZ_JE);
				AssertEquals(invoice.PK, pivot.CHZ_JZ);
				AssertEquals(444, pivot.CHZ_NumberOfPacks);
			}
		}

		public void TestPopulateTransactionNumberUsingImporterAccountSecurityNumber()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "54321");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFGH");

			var importer = CreateOrganisation("PET", "Peter");
			importer.OH_RL_NKClosestPort = "CABLO";

			var supplier = CreateOrganisation("CAT", "Catey");
			supplier.OH_RL_NKClosestPort = "USCHI";

			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "12345678";

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
			};
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo { Key = CAAddInfoSchema.CA_UseImporterAccountSecurityNumber.Name.Substring(3), Value = "Y" }
				});

			declarationDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			declarationDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals("Security No", "12345", declarationBO.TransactionNumber.AccountSecurityCode);
		}

		public void TestFillCollections_PageNumberCalculation()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, null);
			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								PackingLinkCollection = new List<PackingLink>()
								{
									new PackingLink()
									{
										PackingLineLink = 1,
										PackedQuantity = 444
									},
									new PackingLink()
									{
										PackingLineLink = 2,
										PackedQuantity = 1
									}
								},
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>()
								{
									new CommercialInvoiceLine()
									{
										Link = 1
									},
									new CommercialInvoiceLine()
									{
										Link = 2
									}
								})),
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								PackingLinkCollection = new List<PackingLink>()
								{
									new PackingLink()
									{
										PackingLineLink = 1,
										PackedQuantity = 444
									}
								},
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>()
								{
									new CommercialInvoiceLine()
									{
										Link = 1
									}
								}))
						}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 2, 3 }, declarationBO.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.CA_PageNumber));
		}

		public void TestFillCollections_PageNumberCalculation_HVLV()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, null);

			declarationDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C00029310");
			declarationDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S900053111");

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S900053111");
			var hvlvShipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				ShipmentType = new CodeDescriptionPair()
				{
					Code = Core.Constants.ShipmentTypes.HighVolumeLowValue
				}
			};

			hvlvShipmentDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								PackingLinkCollection = new List<PackingLink>()
								{
									new PackingLink()
									{
										PackingLineLink = 1,
										PackedQuantity = 444
									},
									new PackingLink()
									{
										PackingLineLink = 2,
										PackedQuantity = 1
									}
								},
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>()
								{
									new CommercialInvoiceLine()
									{
										Link = 1
									},
									new CommercialInvoiceLine()
									{
										Link = 2
									}
								})),
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								PackingLinkCollection = new List<PackingLink>()
								{
									new PackingLink()
									{
										PackingLineLink = 1,
										PackedQuantity = 444
									}
								},
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>()
								{
									new CommercialInvoiceLine()
									{
										Link = 1
									}
								}))
						}
			};

			declarationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>()
			{
				hvlvShipmentDataObject
			});

			Assert("Precondition: DataObject is HVLV", declarationDataObject.IsHVLV());

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 2, 3 }, declarationBO.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.CA_PageNumber));
		}

		UniversalShipment SetupDeclaration(ZString messageType, ZString messageSubType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			return new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair() { Code = messageType },
				MessageSubType = new CodeDescriptionPair() { Code = messageSubType }
			};
		}

		OrgHeader CreateOrganisation(ZString name, ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = name + " ADDRESS 1";
			return org;
		}
	}
}
