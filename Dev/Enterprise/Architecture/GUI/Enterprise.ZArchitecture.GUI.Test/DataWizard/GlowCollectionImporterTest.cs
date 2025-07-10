using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GlowCollectionImporterTest : TestCaseWithFactory
	{
		public void TestMapping_DetectHeaderAndValueMismatch()
		{
			var importPreviewHeader = new ImportPreviewHeader(string.Empty, new[] { "GB_Code", "GB_BranchName" });
			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AAA", 0),
				new ImportPreviewLineDetails("True branch", 1),
				new ImportPreviewLineDetails("True", 2),
				new ImportPreviewLineDetails("USLAX", 3)
			});

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader }, importPreviewLine) };
			AssertEquals(false, importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(1, log.CountWithoutVerbose);
			AssertEquals(LogType.Error, log.GetLogType(0));
			AssertEquals("There is an issue with your ADAW mapping. Please check that the ‘Map Data From’ and ‘Map Data To’ fields are correct, " +
						"and also other warnings that may help identify the problem. If you recently made changes to the mapping, " +
						"try waiting a few hours or re-importing the mapping.",
				log.GetLogMessage(0));

			importPreviewHeader = new ImportPreviewHeader(string.Empty, new[] { "GB_Code", "GB_BranchName" });
			importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[] { new ImportPreviewLineDetails("AAA", 0) });

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader }, importPreviewLine) };
			AssertEquals(false, importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(1, log.CountWithoutVerbose);
			AssertEquals(LogType.Error, log.GetLogType(0));
			AssertEquals("There is an issue with your ADAW mapping. Please check that the ‘Map Data From’ and ‘Map Data To’ fields are correct, " +
						"and also other warnings that may help identify the problem. If you recently made changes to the mapping, " +
						"try waiting a few hours or re-importing the mapping.",
				log.GetLogMessage(0));
		}

		public void TestPopulateFromDataRows()
		{
			Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(progressReporter.ItemsProcessed, 1);
			AssertEquals(importedElements.Count, 1);
			AssertEquals(importedElements[0]["GB_Code"], "AAA");
			AssertEquals(importedElements[0]["GB_BranchName"], "True branch");
			AssertEquals(importedElements[0]["GB_IsActive"], true);
			AssertEquals(importedElements[0]["GB_RL_NKHomePort"], "USLAX");

			var childCollection = importedElements[0]["GlbHolidays"] as IBusinessObjectCollection;
			AssertNotNull(childCollection);
			AssertEquals(childCollection.Count, 2);

			var childObjects = childCollection.ToArray();
			AssertEquals(childObjects[0]["GH_HolidayName"], "Queen birthday");
			AssertEquals(childObjects[0]["GH_Date"], new DateTime(2017, 6, 12));
			AssertEquals(childObjects[1]["GH_HolidayName"], "Constitution day");
			AssertEquals(childObjects[1]["GH_Date"], new DateTime(2017, 6, 24));
		}

		public void TestPopulateFromDataRows_JobUSComInvoiceLine()
		{
			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "JobUSComInvoiceLine.USI_DRW99ClaimedDuty", "JobUSComInvoiceLine.USI_DRW99ClaimedHMF" });

			var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IUSJobUSComInvoiceLine", "USI"),
				new MappingDataDefinition("IUSJobComInvoiceLine", "JI")
					.AddRelation("JobUSComInvoiceLine", "IUSJobUSComInvoiceLine", "USI_JI")
			});

			var invoiceLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("3", 0),
				new ImportPreviewLineDetails("44", 1)
			});
			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1 }, invoiceLine) };

			var declaration = (BusinessObject)Factory.New<Customs.US.IJobDeclaration>();
			declaration["JE_MessageType"] = "DRW";

			var invoices = (IBusinessObjectCollection)declaration["Invoices"];

			var invoice = invoices.AddNew();
			var invoiceLines = (IBusinessObjectCollection)invoice["InvoiceLines"];

			AssertEquals(true, importer.PopulateFromDataRows(invoiceLines, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(1, invoiceLines.Count);
			var importedInvoiceLine = (BusinessObject)invoiceLines[0];
			AssertEquals(new ZDecimal(3), importedInvoiceLine["USI_DRW99ClaimedDuty"]);
			AssertEquals(new ZDecimal(44), importedInvoiceLine["USI_DRW99ClaimedHMF"]);
		}

		public void TestPopulateFromDataRows_MultipleLevelsWithError()
		{
			var businessObjectType = ObjectFactory.GetType<IBMBoard>();
			collectionMock = new Mock<IBusinessObjectCollection>();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var board = (BusinessObject)Factory.New<IBMBoard>();
				importedElements.Add(board);
				return board;
			});

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "MB_Name", "MB_IsPublished", "Staff.GS_Code" });
			var importPreviewHeader2 = new ImportPreviewHeader("BMBoardSections", new[] { "BMBoardSections.MS_SectionType" });
			var importPreviewHeader3 = new ImportPreviewHeader("BMBoardSections.BMBoardSectionChannels", new[] { "BMBoardSections.BMBoardSectionChannels.MSC_Axis" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("BOARD", 0),
				new ImportPreviewLineDetails("True", 1),
				new ImportPreviewLineDetails("~BP", 2),
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("BMBoardSections", 1, new[]
			{
				new ImportPreviewLineDetails("AAA", 0)
			}));
			var secondLine = new ImportPreviewLine("BMBoardSections", 2, new[]
			{
				new ImportPreviewLineDetails("BBB", 0)
			});
			secondLine.ChildLines.Add(new ImportPreviewLine("BMBoardSections.BMBoardSectionChannels", 3, new[]
			{
				new ImportPreviewLineDetails("ZZZ", 0)
			}));
			secondLine.ChildLines.Add(new ImportPreviewLine("BMBoardSections.BMBoardSectionChannels", 4, new[]
			{
				new ImportPreviewLineDetails("XXX", 0)
			}));
			importPreviewLine.ChildLines.Add(secondLine);

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2, importPreviewHeader3 }, importPreviewLine) };

			Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(progressReporter.ItemsProcessed, 1);
			AssertEquals(log.CountWithoutVerbose, 1);
			AssertEquals(log.GetLogType(0), LogType.Warning);
			AssertEquals(log.GetLogMessage(0), "Importing into this list is not supported 'BMBoardSectionChannels'. Please log an eRequest and WiseTech Global will look at adding this functionality.");
			AssertEquals(importedElements.Count, 1);
			AssertEquals(importedElements[0]["MB_Name"], "BOARD");
			AssertEquals(importedElements[0]["MB_IsPublished"], true);
			AssertEquals(importedElements[0]["MB_GS_NKStaffCode"], "~BP");

			var childCollection = importedElements[0]["Sections"] as IBusinessObjectCollection;
			AssertNotNull(childCollection);
			AssertEquals(childCollection.Count, 2);
			var childObjects = childCollection.ToArray();
			AssertEquals(childObjects[0]["MS_SectionType"], "AAA");
			AssertEquals(childObjects[1]["MS_SectionType"], "BBB");
		}

		public void TestPopulateFromDataRows_MultipleLevels()
		{
			var businessObjectType = ObjectFactory.GetType<IWhsDocket>();
			collectionMock = new Mock<IBusinessObjectCollection>();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var docket = (BusinessObject)Factory.New<IWhsAdjustment>();
				importedElements.Add(docket);
				return docket;
			});

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "WD_DocketID", "WD_DocketType", "WD_ArrivalDate" });
			var importPreviewHeader2 = new ImportPreviewHeader("WhsDocketLines", new[] { "WhsDocketLines.WE_PartAttrib1", "WhsDocketLines.WE_StockOnHand" });
			var importPreviewHeader3 = new ImportPreviewHeader("WhsDocketLines.WhsInventoryViews", new[] { "WhsDocketLines.WhsInventoryViews.WI_InDocketLineUnits" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("987123", 0),
				new ImportPreviewLineDetails("ADJ", 1),
				new ImportPreviewLineDetails("2017-08-24", 2),
			});

			importPreviewLine.ChildLines.Add(new ImportPreviewLine("WhsDocketLines", 1, new[]
			{
				new ImportPreviewLineDetails("partattr", 0),
				new ImportPreviewLineDetails("99", 1),
			}));

			var secondLine = new ImportPreviewLine("WhsDocketLines", 2, new[]
			{
				new ImportPreviewLineDetails("parattr11", 0),
				new ImportPreviewLineDetails("88", 1),
			});

			secondLine.ChildLines.Add(new ImportPreviewLine("WhsDocketLines.WhsInventoryViews", 3, new[]
			{
				new ImportPreviewLineDetails("1", 0)
			}));

			secondLine.ChildLines.Add(new ImportPreviewLine("WhsDocketLines.WhsInventoryViews", 4, new[]
			{
				new ImportPreviewLineDetails("2", 0)
			}));

			secondLine.ChildLines.Add(new ImportPreviewLine("WhsDocketLines.WhsInventoryViews", 5, new[]
			{
				new ImportPreviewLineDetails("", 0)
			}));

			secondLine.ChildLines.Add(new ImportPreviewLine("WhsDocketLines.WhsInventoryViews", 6, new[]
			{
				new ImportPreviewLineDetails(null, 0)
			}));

			importPreviewLine.ChildLines.Add(secondLine);

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2, importPreviewHeader3 }, importPreviewLine) };

			Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(progressReporter.ItemsProcessed, 1);

			AssertEquals(importedElements.Count, 1);
			AssertEquals(importedElements[0]["WD_DocketID"], "987123");
			AssertEquals(importedElements[0]["WD_DocketType"], "ADJ");
			AssertEquals(importedElements[0]["WD_ArrivalDate"], new ZDateTimeOffset(2017, 8, 24));

			var childCollection = importedElements[0]["Lines"] as IBusinessObjectCollection;
			AssertNotNull(childCollection);
			AssertEquals(childCollection.Count, 2);
			var childObjects = childCollection.ToArray();
			AssertEquals(childObjects[0]["WE_PartAttrib1"], "partattr");
			AssertEquals(childObjects[0]["WE_StockOnHand"], 99m);
			AssertEquals(childObjects[1]["WE_PartAttrib1"], "parattr11");
			AssertEquals(childObjects[1]["WE_StockOnHand"], 88m);

			var childChildCollection = childObjects[0]["Inventory"] as IBusinessObjectCollection;
			AssertNotNull(childChildCollection);
			AssertEquals(childChildCollection.Count, 0);

			childChildCollection = childObjects[1]["Inventory"] as IBusinessObjectCollection;
			AssertNotNull(childChildCollection);
			AssertEquals(childChildCollection.Count, 4);
			childObjects = childChildCollection.ToArray();
			AssertEquals(childObjects[0]["WI_InDocketLineUnits"], 1m);
			AssertEquals(childObjects[1]["WI_InDocketLineUnits"], 2m);
			AssertEquals(childObjects[2]["WI_InDocketLineUnits"], 0m);
		}

		public void TestPopulateFromDataRows_MultipleCollections()
		{
			var businessObjectType = ObjectFactory.GetType<IDtbConsignment>();
			var collectionMock = new Mock<IBusinessObjectCollection>();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var consignment = (BusinessObject)Factory.New<IDtbConsignment>();
				importedElements.Add(consignment);
				return consignment;
			});

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "LTC_JobID", "LTC_Direction" });
			var importPreviewHeader2 = new ImportPreviewHeader("Addresses", new[] { "Addresses.E2_Address1", "Addresses.E2_AddressType" });
			var importPreviewHeader3 = new ImportPreviewHeader("DtbConsignmentAddresses", new[] { "DtbConsignmentAddresses.LTS_InstructionType", "DtbConsignmentAddresses.LTS_Status" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("consign1", 0),
				new ImportPreviewLineDetails("PIC", 1)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("Addresses", 1, new[]
			{
				new ImportPreviewLineDetails("addr 1", 0),
				new ImportPreviewLineDetails("LCT", 1)
			}));
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("Addresses", 2, new[]
			{
				new ImportPreviewLineDetails("addr 99", 0),
				new ImportPreviewLineDetails("TLC", 1)
			}));
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("DtbConsignmentAddresses", 3, new[]
			{
				new ImportPreviewLineDetails("AAA", 0),
				new ImportPreviewLineDetails("DLV", 1)
			}));
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("DtbConsignmentAddresses", 4, new[]
			{
				new ImportPreviewLineDetails("BBB", 0),
				new ImportPreviewLineDetails("HLD", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2, importPreviewHeader3 }, importPreviewLine) };

			Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(progressReporter.ItemsProcessed, 1);
			AssertEquals(importedElements.Count, 1);
			AssertEquals(importedElements[0]["LTC_JobID"], "consign1");
			AssertEquals(importedElements[0]["LTC_Direction"], "PIC");

			var jobDocAddressCollection = importedElements[0]["DocAddresses"] as IJobDocAddressDependentCollection;
			AssertNotNull(jobDocAddressCollection);
			AssertEquals(jobDocAddressCollection.Count, 2);
			var jobDocAddressObjectsOrdered = jobDocAddressCollection.Cast<BusinessObject>().OrderBy(x => x["E2_AddressType"]).ToArray();
			AssertEquals(jobDocAddressObjectsOrdered[0]["E2_Address1"], "addr 1");
			AssertEquals(jobDocAddressObjectsOrdered[0]["E2_AddressType"], "LCT");
			AssertEquals(jobDocAddressObjectsOrdered[1]["E2_Address1"], "addr 99");
			AssertEquals(jobDocAddressObjectsOrdered[1]["E2_AddressType"], "TLC");

			var dtbAddressCollection = importedElements[0]["Addresses"] as IBusinessObjectCollection;
			AssertNotNull(dtbAddressCollection);
			AssertEquals(dtbAddressCollection.Count, 2);
			var dtbAddressObjectsOrdered = dtbAddressCollection.Cast<BusinessObject>().OrderBy(x => x["LTS_InstructionType"]).ToArray();
			AssertEquals(dtbAddressObjectsOrdered[0]["LTS_InstructionType"], "AAA");
			AssertEquals(dtbAddressObjectsOrdered[0]["LTS_Status"], "DLV");
			AssertEquals(dtbAddressObjectsOrdered[1]["LTS_InstructionType"], "BBB");
			AssertEquals(dtbAddressObjectsOrdered[1]["LTS_Status"], "HLD");
		}

		public void TestPopulateFromDataRows_MultipleCollections_ReferenceNumbers()
		{
			var businessObjectType = ObjectFactory.GetType<ICommonContainer>();
			var collectionMock = new Mock<IBusinessObjectCollection>();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var container = (BusinessObject)Factory.New<ICommonContainer>();
				importedElements.Add(container);
				return container;
			});

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "JC_ContainerNum", "JC_ContainerJobID" });
			var importPreviewHeader2 = new ImportPreviewHeader("ReferenceNumbers", new[] { "ReferenceNumbers.CE_EntryType", "ReferenceNumbers.CE_EntryNum" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("CONT1234567", 0),
				new ImportPreviewLineDetails("JOB0001", 1)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("ReferenceNumbers", 1, new[]
			{
				new ImportPreviewLineDetails("VID", 0),
				new ImportPreviewLineDetails("VID1111", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine) };

			Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(progressReporter.ItemsProcessed, 1);
			AssertEquals(importedElements.Count, 1);
			AssertEquals(importedElements[0]["JC_ContainerNum"], "CONT1234567");
			AssertEquals(importedElements[0]["JC_ContainerJobID"], "JOB0001");

			var referenceNumberCollection = importedElements[0]["AdditionalReferenceNumbers"] as Customs.ICusEntryNumAdditionalReferenceCollection;
			AssertNotNull(referenceNumberCollection);
			AssertEquals(referenceNumberCollection.Count, 1);
			var referenceNumbers = referenceNumberCollection.ToArray();
			AssertEquals(referenceNumbers[0]["CE_EntryType"], "VID");
			AssertEquals(referenceNumbers[0]["CE_EntryNum"], "VID1111");
		}

		public void TestPopulateFromDataRows_ExceptionWhenMappingObject()
		{
			var mock = new Mock<IEntityMatcher>();
			mock.Setup(s => s.GetMatchingBusinessObject(It.IsAny<IEntityMatcherContext>(), It.IsAny<string>(), It.IsAny<IEnumerable<(string, string)>>(), false)).Throws(new EntityMatchingException("something wrong"));

			try
			{
				ObjectFactory.Substitute(mock.Object);

				Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
				AssertEquals(progressReporter.ItemsProcessed, 1);
				AssertEquals(log.CountWithoutVerbose, 1);
				AssertEquals(log.GetLogType(0), "Warning");
				AssertEquals(log.GetLogMessage(0), "something wrong");
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}

		public void TestPopulateFromDataRows_InvalidCollectionName()
		{
			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "GB_Code", "GB_BranchName", "GB_IsActive", "HomePort.RL_Code" });
			var importPreviewHeader2 = new ImportPreviewHeader("InvalidCollection", new[] { "InvalidCollection.GH_HolidayName", "InvalidCollection.GH_Date" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("Test branch", 1),
				new ImportPreviewLineDetails("True", 2),
				new ImportPreviewLineDetails("USLAX", 3)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("InvalidCollection", 1, new[]
			{
				new ImportPreviewLineDetails("Queen birthday", 0),
				new ImportPreviewLineDetails("12-06-2017", 1)
			}));
			var dataRow = new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine);
			dataRows = dataRows.Concat(new[] { dataRow }).ToArray();
			Assert(!importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals(importedElements.Count, 2);
			AssertEquals(progressReporter.ItemsProcessed, 2);
			AssertEquals(log.CountWithoutVerbose, 1);
			AssertEquals(log.GetLogType(0), "Error");
			AssertEquals(log.GetLogMessage(0), "Could not find element type for collection ID 'InvalidCollection'.");
		}

		public void TestPopulateFromDataRows_InvalidPropertyName()
		{
			var importPreviewHeader = new ImportPreviewHeader(string.Empty, new[] { "WrongProperty" });
			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("some invalid date value", 0),
			});
			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader }, importPreviewLine) };

			var businessObjectType = ObjectFactory.GetType<IWhsDocket>();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var docket = (BusinessObject)Factory.New<IWhsAdjustment>();
				importedElements.Add(docket);
				return docket;
			});

			Assert(!importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals(1, importedElements.Count);
			AssertEquals(1, progressReporter.ItemsProcessed);
			AssertEquals(1, log.CountWithoutVerbose);
			AssertEquals("Error", log.GetLogType(0));
			AssertEquals("WhsAdjustment does not have property WrongProperty in Row 0 in Column 0.", log.GetLogMessage(0));
		}

		public void TestPopulateFromDataRows_ValueExceedsMaxLength_InRateEntry()
		{
			var collectionMock = new Mock<IBusinessObjectCollection>(MockBehavior.Strict);
			var importedRateEntries = new List<BusinessObject>();
			var rateEntryType = Type.GetType("Enterprise.Rating.Business.RateEntry, Enterprise.Rating.Business");
			var rateLineType = Type.GetType("Enterprise.Rating.Business.RateLine, Enterprise.Rating.Business");
			var rateLineItemType = Type.GetType("Enterprise.Rating.Business.RateLineItem, Enterprise.Rating.Business");

			collectionMock.Setup(c => c.SuspendAdditionallyForImport()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(c => c.SuspendListChanged()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(x => x.TypeOfElements).Returns(rateEntryType);
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var rateEntry = Factory.New(rateEntryType);
				importedRateEntries.Add(rateEntry);
				return rateEntry;
			});

			var lineHeaders = new[]
			{
				new ImportPreviewHeader(string.Empty, new [] { "TI_DestinationLRC", "TI_Mode" }),
				new ImportPreviewHeader("RateLines", new [] { "RateLines.TL_RateCalculator" }),
				new ImportPreviewHeader("RateCalculators_MPU", new [] { "RateCalculators_MPU.MinimumAmount", "RateCalculators_MPU.PerUnitAmount" }),
			};

			var rateEntryPreview = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("LONGDESTCODE", 0),
				new ImportPreviewLineDetails("FCL", 1)
			});
			var rateLinePreview = new ImportPreviewLine("RateLines", 1, new[]
			{
				new ImportPreviewLineDetails("MPU", 0)
			});
			var rateLineCalculatorPreview = new ImportPreviewLine("RateCalculators_MPU", 2, new[]
			{
				new ImportPreviewLineDetails("100", 0),
				new ImportPreviewLineDetails("200", 1)
			});

			rateEntryPreview.ChildLines.Add(rateLinePreview);
			rateLinePreview.ChildLines.Add(rateLineCalculatorPreview);

			var importPreview = new ImportPreview(lineHeaders, rateEntryPreview);

			var mappingModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IRateEntry", "TI")
					.AddCollection("RateLines", "IRateLine"),
				new MappingDataDefinition("IRateLine", "TL")
					.AddRelation("RateEntry", "IRateEntry", "TL_TI")
					.AddCollection("RateCalculators_MPU", "RateCalculator.MPU"),
				new MappingDataDefinition("RateCalculator.MPU", ""),
			});

			Assert(!importer.PopulateFromDataRows(collectionMock.Object, new ImportPreview[] { importPreview }, mappingModel, log, progressReporter));

			var rateEntry = Factory.Load(rateEntryType, new ZQuery(Schema.RateEntrySchema.PK, importedRateEntries[0].PK));
			AssertEquals("There should be no imported rates since the long Destination code removes it",0, rateEntry.Length);
			var rateLines = Factory.Load(rateLineType, new ZQuery(Schema.RateLinesSchema.TL_TI, importedRateEntries[0].PK));
			AssertEquals("Child Lines should also not be added", 0, rateLines.Length);

			var logs = log.GetLogs().SplitByLine();
			AssertEquals(2, logs.Count());
			Assert(logs.Contains("The value 'LONGDESTCODE' exceeds character limit for the field 'TI_DestinationLRC' in Row 0 in Column 0. Max Length is 5."));
			Assert(logs.Contains("RateEntry importing has failed. The incomplete RateEntry has been deleted as a result"));
		}

		public void TestPopulateFromDataRows_ValueExceedsMaxLength_InRateLine()
		{
			var collectionMock = new Mock<IBusinessObjectCollection>(MockBehavior.Strict);
			var importedRateEntries = new List<BusinessObject>();
			var rateEntryType = Type.GetType("Enterprise.Rating.Business.RateEntry, Enterprise.Rating.Business");
			var rateLineType = Type.GetType("Enterprise.Rating.Business.RateLine, Enterprise.Rating.Business");
			var rateLineItemType = Type.GetType("Enterprise.Rating.Business.RateLineItem, Enterprise.Rating.Business");

			collectionMock.Setup(c => c.SuspendAdditionallyForImport()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(c => c.SuspendListChanged()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(x => x.TypeOfElements).Returns(rateEntryType);
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var rateEntry = Factory.New(rateEntryType);
				importedRateEntries.Add(rateEntry);
				return rateEntry;
			});

			var lineHeaders = new[]
			{
				new ImportPreviewHeader(string.Empty, new [] { "TI_DestinationLRC", "TI_Mode" }),
				new ImportPreviewHeader("RateLines", new [] { "RateLines.TL_RateCalculator" }),
				new ImportPreviewHeader("RateCalculators_MPU", new [] { "RateCalculators_MPU.MinimumAmount", "RateCalculators_MPU.PerUnitAmount" }),
			};

			var rateEntryPreview = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AUSYD", 0),
				new ImportPreviewLineDetails("FCL", 1)
			});
			var rateLinePreview = new ImportPreviewLine("RateLines", 1, new[]
			{
				new ImportPreviewLineDetails("MPULONGCALC", 0)
			});
			var rateLineCalculatorPreview = new ImportPreviewLine("RateCalculators_MPU", 2, new[]
			{
				new ImportPreviewLineDetails("100", 0),
				new ImportPreviewLineDetails("200", 1)
			});

			rateEntryPreview.ChildLines.Add(rateLinePreview);
			rateLinePreview.ChildLines.Add(rateLineCalculatorPreview);

			var importPreview = new ImportPreview(lineHeaders, rateEntryPreview);

			var mappingModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IRateEntry", "TI")
					.AddCollection("RateLines", "IRateLine"),
				new MappingDataDefinition("IRateLine", "TL")
					.AddRelation("RateEntry", "IRateEntry", "TL_TI")
					.AddCollection("RateCalculators_MPU", "RateCalculator.MPU"),
				new MappingDataDefinition("RateCalculator.MPU", ""),
			});

			Assert(!importer.PopulateFromDataRows(collectionMock.Object, new ImportPreview[] { importPreview }, mappingModel, log, progressReporter));

			var rateEntry = Factory.Load(rateEntryType, new ZQuery(Schema.RateEntrySchema.PK, importedRateEntries[0].PK));
			AssertEquals("RateEntryShouldStay", 1, rateEntry.Length);
			var rateLines = Factory.Load(rateLineType, new ZQuery(Schema.RateLinesSchema.TL_TI, importedRateEntries[0].PK));
			AssertEquals("Child Lines should not be added", 0, rateLines.Length);

			var logs = log.GetLogs().SplitByLine();
			AssertEquals(3, logs.Count());
			Assert(logs.Contains("The value 'MPULONGCALC' exceeds character limit for the field 'TL_RateCalculator' in Row 1 in Column 0. Max Length is 3."));
			Assert(logs.Contains("Calculator 'None' not supported"));
			Assert(logs.Contains("Rate Lines importing has failed. The incomplete Rate Lines has been deleted as a result"));
		}

		public void TestPopulateFromDataRows_ValueExceedsMaxLength_InRateLineItem()
		{
			var collectionMock = new Mock<IBusinessObjectCollection>(MockBehavior.Strict);
			var importedRateEntries = new List<BusinessObject>();
			var rateEntryType = Type.GetType("Enterprise.Rating.Business.RateEntry, Enterprise.Rating.Business");
			var rateLineType = Type.GetType("Enterprise.Rating.Business.RateLine, Enterprise.Rating.Business");
			var rateLineItemType = Type.GetType("Enterprise.Rating.Business.RateLineItem, Enterprise.Rating.Business");

			collectionMock.Setup(c => c.SuspendAdditionallyForImport()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(c => c.SuspendListChanged()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(x => x.TypeOfElements).Returns(rateEntryType);
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var rateEntry = Factory.New(rateEntryType);
				importedRateEntries.Add(rateEntry);
				return rateEntry;
			});

			var lineHeaders = new[]
			{
				new ImportPreviewHeader(string.Empty, new [] { "TI_DestinationLRC", "TI_Mode" }),
				new ImportPreviewHeader("RateLines", new [] { "RateLines.TL_RateCalculator" }),
				new ImportPreviewHeader("RateCalculators_NTE", new [] { "RateCalculators_NTE.ItemDescription", "RateCalculators_NTE.Amount" }),
			};

			var rateEntryPreview = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AUSYD", 0),
				new ImportPreviewLineDetails("FCL", 1)
			});
			var rateLinePreview = new ImportPreviewLine("RateLines", 1, new[]
			{
				new ImportPreviewLineDetails("NTE", 0)
			});
			var rateLineCalculatorPreview = new ImportPreviewLine("RateCalculators_NTE", 2, new[]
			{
				new ImportPreviewLineDetails(new string('A', 85), 0),
				new ImportPreviewLineDetails("200", 1)
			});

			rateEntryPreview.ChildLines.Add(rateLinePreview);
			rateLinePreview.ChildLines.Add(rateLineCalculatorPreview);

			var importPreview = new ImportPreview(lineHeaders, rateEntryPreview);

			var mappingModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IRateEntry", "TI")
					.AddCollection("RateLines", "IRateLine"),
				new MappingDataDefinition("IRateLine", "TL")
					.AddRelation("RateEntry", "IRateEntry", "TL_TI")
					.AddCollection("RateCalculators_NTE", "RateCalculator.NTE"),
				new MappingDataDefinition("RateCalculator.NTE", ""),
			});

			Assert(!importer.PopulateFromDataRows(collectionMock.Object, new ImportPreview[] { importPreview }, mappingModel, log, progressReporter));

			var rateEntry = Factory.Load(rateEntryType, new ZQuery(Schema.RateEntrySchema.PK, importedRateEntries[0].PK));
			AssertEquals("There should be imported rate entry", 1, rateEntry.Length);
			var rateLines = Factory.Load(rateLineType, new ZQuery(Schema.RateLinesSchema.TL_TI, importedRateEntries[0].PK));
			AssertEquals("Child Lines should also not be added since calculator invalid (Over 80 chars)", 0, rateLines.Length);

			var rateLineItems = Factory.Load(rateLineItemType, new ZQuery(Schema.RateLineItemsSchema.TM_Value, 200));
			AssertEquals("There are no rate line items", 0, rateLineItems.Length);

			var logs = log.GetLogs().SplitByLine();
			AssertEquals(2, logs.Count());
			AssertEquals("'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA' exceeds the max length that is allowed for the field (80) and Text will not be imported", logs.First());
			AssertEquals("RateLineItem importing has failed. The related incomplete RateLine has been deleted as a result", logs.Last());
		}

		public void TestPopulateFromDataRows_PropertyNotInBusinessObject_CheckIfImporterUsed()
		{
			var collectionMock = new Mock<IBusinessObjectCollection>(MockBehavior.Default);
			var importedRateEntries = new List<BusinessObject>();
			var rateEntryType = Type.GetType("Enterprise.Rating.Business.RateEntry, Enterprise.Rating.Business");

			collectionMock.Setup(c => c.SuspendAdditionallyForImport()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(c => c.SuspendListChanged()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(x => x.TypeOfElements).Returns(rateEntryType);
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var rateEntry = Factory.New(rateEntryType);
				importedRateEntries.Add(rateEntry);
				return rateEntry;
			});

			var lineHeaders = new[]
			{
				new ImportPreviewHeader(string.Empty, new [] { "TI_DestinationLRC", "PropertyNotInObject" })
			};

			var rateEntryPreview = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AUSYD", 0),
				new ImportPreviewLineDetails("1234", 1)
			});

			var importPreview = new ImportPreview(lineHeaders, rateEntryPreview);

			var mappingModel = new MappingDataModel(Array.Empty<MappingDataDefinition>());

			var customImporterMock = new Mock<IGlowCustomImporter>(MockBehavior.Default);
			customImporterMock.Setup(x => x.ConvertCustomLine(It.IsAny<BusinessObject>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<int>(), It.IsAny<string>()));

			var objectHandle = new Mock<ObjectHandle>();
			objectHandle.Setup(x => x.GetObject()).Returns(customImporterMock.Object);

			var hashTable = new Hashtable();
			hashTable.Add("RateEntry", objectHandle.Object);

			ObjectFactory.Substitute("GlowCustomImporters", hashTable);

			Assert(importer.PopulateFromDataRows(collectionMock.Object, new ImportPreview[] { importPreview }, mappingModel, log, progressReporter));

			customImporterMock.Verify(x => x.ConvertCustomLine(It.IsAny<BusinessObject>(), It.Is<string>(s => s.Contains("1234")), It.IsAny<INotifications>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once());
		}

		public void TestTestPopulateFromDataRows_ImportChildlessChildren()
		{
			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "GG_Code", "GG_Desc", "GG_IsActive" });
			var importPreviewHeader2 = new ImportPreviewHeader("GlbGroupLinks", new[] { "GlbGroupLinks.GK_MembershipType", "GlbGroupLinks.GK_SkillLevel" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AAA", 0),
				new ImportPreviewLineDetails("Test group", 1),
				new ImportPreviewLineDetails("True", 2)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("GlbGroupLinks", 1, new[]
			{
				new ImportPreviewLineDetails("Z", 0),
				new ImportPreviewLineDetails("1", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine) };

			var customImporterMock = new Mock<IGlowCustomImporter>(MockBehavior.Default);
			customImporterMock.Setup(x => x.ConvertCustomLine(It.IsAny<BusinessObject>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<int>(), It.IsAny<string>()));
			customImporterMock.Setup(x => x.ShouldCustomizeChildrenImport).Returns(false);

			var businessObjectType = Factory.New<IGlbGroup>().GetType();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var group = (BusinessObject)Factory.New<IGlbGroup>();
				importedElements.Add(group);
				return group;
			});

			var objectHandle = new Mock<ObjectHandle>();
			objectHandle.Setup(x => x.GetObject()).Returns(customImporterMock.Object);

			var hashTable = new Hashtable();
			hashTable.Add("GlbGroup", objectHandle.Object);

			ObjectFactory.Substitute("GlowCustomImporters", hashTable);

			Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			customImporterMock.Verify(x => x.ImportChildlessChildren(It.IsAny<BusinessObject>(), It.IsAny<INotifications>(), It.IsAny<int>(), It.IsAny<string[]>(), It.IsAny<ImportPreviewLineDetails[]>()), Times.Never());
		}

		public void TestDataFromCSVShouldNotBeOverriddenByDefaulting()
		{
			var tariff = Factory.New<Customs.US.IUSCTariff>();
			tariff.UE_Tariff = "112231450";
			tariff.UE_Unit1 = "KG";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_DutyComputationCode = "7";
			tariff.UE_Column1RateAdValorem = 24.31m;

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "JI_Tariff", "US_DRWLineDutyRateDesc", "US_DRWClaimAmountOverriden_New", "US_DRWAdValoremRate", "US_DRWCalcDutyWithAdValoremRate" });
			var importPreviewHeader2 = new ImportPreviewHeader("DrawbackAdditionalImportTariffNumbers", new[] { "DrawbackAdditionalImportTariffNumbers.US_FormattedTariff" });

			var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IUSJobComInvoiceLine", "JI")
					.AddCollection("DrawbackAdditionalImportTariffNumbers", "IUSDrawbackAdditionalImportTariffNumber"),
				new MappingDataDefinition("IUSDrawbackAdditionalImportTariffNumber", "B7")
			});

			var invoiceLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("112231450", 0),
				new ImportPreviewLineDetails("20.50%HI", 1),
				new ImportPreviewLineDetails("Y", 2),
				new ImportPreviewLineDetails("20.50", 3),
				new ImportPreviewLineDetails("Y", 4),
			});
			var additionalTariffNumber = new ImportPreviewLine("DrawbackAdditionalImportTariffNumbers", 1, new[]
			{
				new ImportPreviewLineDetails("10203040", 0),
			});
			invoiceLine.ChildLines.Add(additionalTariffNumber);
			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, invoiceLine) };

			var declaration = (BusinessObject)Factory.New<Customs.US.IJobDeclaration>();
			declaration["JE_MessageType"] = "DRW";

			var invoices = (IBusinessObjectCollection)declaration["Invoices"];
			AssertEquals("(pre-condition)", 0, invoices.Count);

			var invoice = invoices.AddNew();
			var invoiceLines = (IBusinessObjectCollection)invoice["InvoiceLines"];
			AssertEquals("(pre-condition)", 0, invoiceLines.Count);

			AssertEquals(true, importer.PopulateFromDataRows(invoiceLines, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(1, invoiceLines.Count);
			var importedInvoiceLine = (BusinessObject)invoiceLines[0];
			AssertEquals("112231450", importedInvoiceLine["JI_Tariff"]);
			AssertEquals(20.5m, importedInvoiceLine["US_DRWAdValoremRate"]);
			AssertEquals(true, importedInvoiceLine["US_DRWCalcDutyWithAdValoremRate"]);
			AssertEquals("20.50%HI", importedInvoiceLine["US_DRWLineDutyRateDesc"]);
		}

		public void TestPopulateFromDataRows_CollectionCannotBeDetermined()
		{
			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "GG_Code", "GG_Desc", "GG_IsActive" });
			var importPreviewHeader2 = new ImportPreviewHeader("GlbGroupLinks", new[] { "GlbGroupLinks.GK_MembershipType", "GlbGroupLinks.GK_SkillLevel" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AAA", 0),
				new ImportPreviewLineDetails("Test groupe", 1),
				new ImportPreviewLineDetails("True", 2)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("GlbGroupLinks", 1, new[]
			{
				new ImportPreviewLineDetails("Z", 0),
				new ImportPreviewLineDetails("1", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine) };

			var businessObjectType = Factory.New<IGlbGroup>().GetType();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var group = (BusinessObject)Factory.New<IGlbGroup>();
				importedElements.Add(group);
				return group;
			});

			Assert(importer.PopulateFromDataRows(collectionMock.Object, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(importedElements.Count, 1);
			AssertEquals(progressReporter.ItemsProcessed, 1);
			AssertEquals(log.CountWithoutVerbose, 1);
			AssertEquals(log.GetLogType(0), "Warning");
			AssertEquals(log.GetLogMessage(0), "Importing into this list is not supported 'GlbGroupLinks'. Please log an eRequest and WiseTech Global will look at adding this functionality.");
		}

		public void TestPopulateFromDataRows_CusAddInfo()
		{
			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "JI_PartNo", "US_FSISInd", "US_FDAIndicator" });
			var importPreviewHeader2 = new ImportPreviewHeader("FSISLines", new[] { "FSISLines.US_HealthCertificateNumber" });
			var importPreviewHeader3 = new ImportPreviewHeader("ACE_FDALines", new[] { "ACE_FDALines.US_ProgramCode" });
			var importPreviewHeader4 = new ImportPreviewHeader("ACE_FDALines.Lots", new[] { "ACE_FDALines.Lots.US_Temperature", "ACE_FDALines.Lots.US_DegreeType" });
			var importPreviewHeader5 = new ImportPreviewHeader("NMFSLines", new[] { "NMFSLines.US_ProgramType", "NMFSLines.US_SourceType" });
			var importPreviewHeader6 = new ImportPreviewHeader("NMFSLines.HarvestingDetails", new[] { "NMFSLines.HarvestingDetails.US_HarvestedCountry", "NMFSLines.HarvestingDetails.US_GearType" });

			var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IUSJobComInvoiceLine", "JI")
					.AddCollection("FSISLines", "IUSFSISLine")
					.AddCollection("ACE_FDALines", "IUSACEFDA")
					.AddCollection("NMFSLines", "IUSNMFSLine"),
				new MappingDataDefinition("IUSFSISLine", "B7"),
				new MappingDataDefinition("IUSACEFDA", "B7")
					.AddCollection("Lots", "IUSFDALot"),
				new MappingDataDefinition("IUSFDALot", "B7"),
				new MappingDataDefinition("IUSNMFSLine", "B7")
					.AddCollection("HarvestingDetails", "IUSNMFSHarvestingDetails"),
				new MappingDataDefinition("IUSNMFSHarvestingDetails", "B7")
			});

			var invoiceLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("PROD10001", 0),
				new ImportPreviewLineDetails("D", 1),
				new ImportPreviewLineDetails("D", 2)
			});
			var fsis = new ImportPreviewLine("FSISLines", 1, new[] { new ImportPreviewLineDetails("CERT100001", 0) });
			var fda1 = new ImportPreviewLine("ACE_FDALines", 2, new[] { new ImportPreviewLineDetails("BIO", 0) });
			var fda2 = new ImportPreviewLine("ACE_FDALines", 3, new[] { new ImportPreviewLineDetails("FOO", 0) });
			var lot11 = new ImportPreviewLine("ACE_FDALines.Lots", 4, new[] { new ImportPreviewLineDetails("101", 0), new ImportPreviewLineDetails("K", 1) });
			var lot12 = new ImportPreviewLine("ACE_FDALines.Lots", 5, new[] { new ImportPreviewLineDetails("102", 0), new ImportPreviewLineDetails("C", 1) });
			var lot21 = new ImportPreviewLine("ACE_FDALines.Lots", 6, new[] { new ImportPreviewLineDetails("201", 0), new ImportPreviewLineDetails("F", 1) });
			var nmfs1 = new ImportPreviewLine("NMFSLines", 7, new[] { new ImportPreviewLineDetails("SIM", 0), new ImportPreviewLineDetails("HCF", 1) });
			var harvestingDetails1 = new ImportPreviewLine("NMFSLines.HarvestingDetails", 8, new[] { new ImportPreviewLineDetails("AU", 0), new ImportPreviewLineDetails("HAN", 1) });
			var nmfs2 = new ImportPreviewLine("NMFSLines", 9, new[] { new ImportPreviewLineDetails("SIM", 0), new ImportPreviewLineDetails("HBA", 1) });
			var harvestingDetails2 = new ImportPreviewLine("NMFSLines.HarvestingDetails", 10, new[] { new ImportPreviewLineDetails("NZ", 0), new ImportPreviewLineDetails("HAR", 1) });

			invoiceLine.ChildLines.Add(fsis);
			invoiceLine.ChildLines.Add(fda1);
			invoiceLine.ChildLines.Add(fda2);
			invoiceLine.ChildLines.Add(nmfs1);
			invoiceLine.ChildLines.Add(nmfs2);
			fda1.ChildLines.Add(lot11);
			fda1.ChildLines.Add(lot12);
			fda2.ChildLines.Add(lot21);
			nmfs1.ChildLines.Add(harvestingDetails1);
			nmfs2.ChildLines.Add(harvestingDetails2);

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2, importPreviewHeader3, importPreviewHeader4, importPreviewHeader5, importPreviewHeader6 }, invoiceLine) };

			var declaration = (BusinessObject)Factory.New<Customs.US.IJobDeclaration>();
			declaration["JE_MessageType"] = "IMP";

			var invoices = (IBusinessObjectCollection)declaration["Invoices"];
			AssertEquals("(pre-condition)", 0, invoices.Count);

			var invoice = invoices.AddNew();
			var invoiceLines = (IBusinessObjectCollection)invoice["InvoiceLines"];
			AssertEquals("(pre-condition)", 0, invoiceLines.Count);

			AssertEquals(true, importer.PopulateFromDataRows(invoiceLines, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals(1, invoiceLines.Count);
			var importedInvoiceLine = (BusinessObject)invoiceLines[0];
			AssertEquals("PROD10001", importedInvoiceLine["JI_PartNo"]);
			AssertEquals("D", importedInvoiceLine["US_FSISInd"]);
			AssertEquals("D", importedInvoiceLine["US_FDAIndicator"]);

			var importedFSISCollection = (IBusinessObjectCollection)importedInvoiceLine["FSISLines"];
			AssertEquals(1, importedFSISCollection.Count);
			var importedFSIS = (BusinessObject)importedFSISCollection[0];
			AssertEquals("CERT100001", importedFSIS["US_HealthCertificateNumber"]);

			var importedFDACollection = (IBusinessObjectCollection)importedInvoiceLine["ACE_FDALines"];
			AssertEquals(2, importedFDACollection.Count);
			var importedFDA1 = (BusinessObject)importedFDACollection[0];
			var importedFDA2 = (BusinessObject)importedFDACollection[1];
			AssertEquals("BIO", importedFDA1["US_ProgramCode"]);
			AssertEquals("FOO", importedFDA2["US_ProgramCode"]);

			var importedLotsCollection1 = (IBusinessObjectCollection)importedFDA1["Lots"];
			AssertEquals(2, importedLotsCollection1.Count);
			var importedLot11 = (BusinessObject)importedLotsCollection1[0];
			var importedLot12 = (BusinessObject)importedLotsCollection1[1];

			var importedLotsCollection2 = (IBusinessObjectCollection)importedFDA2["Lots"];
			AssertEquals(1, importedLotsCollection2.Count);
			var importedLot21 = (BusinessObject)importedLotsCollection2[0];

			AssertEquals((ZDecimal)101, importedLot11["US_Temperature"]);
			AssertEquals((ZDecimal)102, importedLot12["US_Temperature"]);
			AssertEquals((ZDecimal)201, importedLot21["US_Temperature"]);
			AssertEquals("K", importedLot11["US_DegreeType"]);
			AssertEquals("C", importedLot12["US_DegreeType"]);
			AssertEquals("F", importedLot21["US_DegreeType"]);

			var importedNmfsLinesCollection = (IBusinessObjectCollection)importedInvoiceLine["NMFSLines"];
			AssertEquals(2, importedNmfsLinesCollection.Count);

			var importedNmfsLine1 = (BusinessObject)importedNmfsLinesCollection[0];
			AssertEquals("SIM", importedNmfsLine1["US_ProgramType"]);
			AssertEquals("HCF", importedNmfsLine1["US_SourceType"]);

			var importedNmfsLine2 = (BusinessObject)importedNmfsLinesCollection[1];
			AssertEquals("SIM", importedNmfsLine2["US_ProgramType"]);
			AssertEquals("HBA", importedNmfsLine2["US_SourceType"]);

			var importedHarvestingDetailsCollection1 = (IBusinessObjectCollection)importedNmfsLine1["HarvestingDetails"];
			AssertEquals(1, importedHarvestingDetailsCollection1.Count);
			var importedHarvestingDetails1 = (BusinessObject)importedHarvestingDetailsCollection1[0];
			AssertEquals("AU", importedHarvestingDetails1["US_HarvestedCountry"]);
			AssertEquals("HAN", importedHarvestingDetails1["US_GearType"]);

			var importedHarvestingDetailsCollection2 = (IBusinessObjectCollection)importedNmfsLine2["HarvestingDetails"];
			AssertEquals(1, importedHarvestingDetailsCollection2.Count);
			var importedHarvestingDetails2 = (BusinessObject)importedHarvestingDetailsCollection2[0];
			AssertEquals("NZ", importedHarvestingDetails2["US_HarvestedCountry"]);
			AssertEquals("HAR", importedHarvestingDetails2["US_GearType"]);
		}

		#region GetOrCreateBizo

		public void TestPopulateFromDataRows_ExistingParentBizoAndExistingChildBizo_DoNotMatchNonSpecifiedHeader()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory);
			var parent = Factory.New<DummyWithMatchingSupportingCollection>();
			parent.ZD1_Code = "";
			parent.ZD1_NumberUnitCode = "BEF";
			parent.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent);

			Factory.Save();

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode" });
			var importPreviewHeader2 = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_NumberUnitCode" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("MOM", 0)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals("Since the code was not in the headers, it should not have tried to match it", 2, parentCollection.Count);
			AssertEquals("There should be one child, but we dont care about its values in this test",
				1,
				(parentCollection[1] as DummyWithMatchingSupportingCollection).CollectionSupportsElementMatching.Count);
		}

		public void TestGivenAnExistingChildBizO_WhenBizOIsIImportCollectionElementMatchingSupporter_ThenCallPrepareForReuse()
		{
			var parent = Factory.New<DummyWithMatchingSupportingCollection>();
			parent.ZD1_Code = "MOM";
			parent.ZD1_NumberUnitCode = "BEF";

			var child = Factory.New<DummyWithMatchingSupportingCollection>();
			child.ZD1_NumberUnitCode = "BEF";
			child.ZD1_Code = "NIC";
			child.ZD1_Number = 0;

			parent.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parent.CollectionSupportsElementMatching.Add(child);

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory);
			parentCollection.Add(parent);

			Factory.Save();

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "ZD1_Code", "ZD1_NumberUnitCode" });
			var importPreviewHeader2 = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("MOM", 0),
				new ImportPreviewLineDetails("AFT", 1)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Expected test class DummyChildCollectionWithElementMatching to run PrepareForReuse() to change ZD1_NumberUnitCode to 'AFT' to assert that IImportCollectionElementMatchingSupporter.PrepareForReuse() in GlowCollectionImporter was run", "AFT", child.ZD1_NumberUnitCode);
		}

		/// <summary>
		/// Tests that it can match a bizo, and update it, based on
		/// IImportCollectionElementMatchingSupporter.MatchingColumnName being
		/// matched
		/// </summary>
		public void TestPopulateFromDataRows_MatchBySingleColumn()
		{
			var parent = Factory.New<DummyWithMatchingSupportingCollection>();
			parent.ZD1_Code = "MOM";
			parent.ZD1_NumberUnitCode = "BEF";
			parent.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory);
			parentCollection.Add(parent);

			Factory.Save();

			AssertEquals("Pre condition: parent bizo has 0 child bizo.", 0, parent.CollectionSupportsElementMatching.Count);

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "ZD1_Code", "ZD1_NumberUnitCode" });
			var importPreviewHeader2 = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("MOM", 0),
				new ImportPreviewLineDetails("AFT", 1)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));
			CombineAssertions(() =>
			{
				AssertEquals("MOM", parent.ZD1_Code);
				AssertEquals("AFT", parent.ZD1_NumberUnitCode);
				AssertEquals(1, parent.CollectionSupportsElementMatching.Count);
			});

			var child = (DummyWithMatchingSupportingCollection)parent.CollectionSupportsElementMatching.First();
			CombineAssertions(() =>
			{
				AssertEquals("NIC", child.ZD1_Code);
				AssertEquals(999, child.ZD1_Number);
			});
		}

		/// <summary>
		/// When the parentCollection contains the item that is being imported, and reuse is NOT allowed, then
		/// create a new parent for the incoming child and insert it into the parentCollection.
		/// </summary>
		public void TestPopulateFromDataRows_MatchedByManyColumns_UniqueMatch_ReuseNotAllowed()
		{
			var parent = Factory.New<DummyWithMatchingSupportingCollection>();
			parent.ZD1_NumberUnitCode = "ABC";
			parent.ZD1_Number = 3;
			parent.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: false);
			parentCollection.Add(parent);

			Factory.Save();

			AssertEquals("Pre condition: parent bizo has 0 child bizo.", 0, parent.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 2 items", 2, parentCollection.Count);
			AssertSame(parent, parentCollection[0]);
			AssertNotSame(parent, parentCollection[1]);

			var newParent = (parentCollection[1] as DummyWithMatchingSupportingCollection);
			var child = newParent.CollectionSupportsElementMatching.Single() as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent.ZD1_NumberUnitCode);
			AssertEquals(3, parent.ZD1_Number);
			AssertEquals(0, parent.CollectionSupportsElementMatching.Count);

			AssertEquals("ABC", newParent.ZD1_NumberUnitCode);
			AssertEquals(3, newParent.ZD1_Number);
			AssertEquals(1, newParent.CollectionSupportsElementMatching.Count);

			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(999, child.ZD1_Number);

			var allLogs = log.GetLogs();
			AssertContains("IDummyDependentBizo: Found a matching IDummyDependentBizo but appending to it is not supported. Creating a new IDummyDependentBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyDependentBizo but cannot decide which to use. Creating a new IDummyDependentBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyDependentBizo but appending to them is not supported. Creating a new IDummyDependentBizo instead.", allLogs);
		}

		/// <summary>
		/// When the parentCollection contains the item that is being imported, and findGenericMatches is not allowed
		/// then create a new parent and insert it into the parentCollection, without logging a match exists.
		/// </summary>
		public void TestPopulateFromDataRows_SkipMatchingByColumns()
		{
			var parent = Factory.New<DummyWithMatchingSupportingCollection>();
			parent.ZD1_NumberUnitCode = "ABC";
			parent.ZD1_Number = 3;

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: false, findGenericMatches: false);
			parentCollection.Add(parent);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});

			dataRows = new[] { new ImportPreview(new[] { headerForParent }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 2 items", 2, parentCollection.Count);
			AssertSame(parent, parentCollection[0]);
			AssertNotSame(parent, parentCollection[1]);

			var allLogs = log.GetLogs();
			AssertNotContains("IDummyDependentBizo: Found a matching IDummyDependentBizo but appending to it is not supported. Creating a new IDummyDependentBizo instead.", allLogs);
		}

		/// <summary>
		/// When the parentCollection contains the item that is being imported, and reuse IS allowed, then
		/// add the incoming child item into the existing parentCollection.
		/// </summary>
		public void TestPopulateFromDataRows_MatchedByManyColumns_UniqueMatch_ReuseAllowed()
		{
			var parent = Factory.New<DummyWithMatchingSupportingCollection>();
			parent.ZD1_NumberUnitCode = "ABC";
			parent.ZD1_Number = 3;
			parent.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);
			parentCollection.Add(parent);

			Factory.Save();

			AssertEquals("Pre condition: parent bizo has 0 child bizo.", 0, parent.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 1 items", 1, parentCollection.Count);
			AssertSame(parent, parentCollection[0]);

			var child = parent.CollectionSupportsElementMatching.Single() as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent.ZD1_NumberUnitCode);
			AssertEquals((ZByte)3, parent.ZD1_Number);
			AssertEquals(1, parent.CollectionSupportsElementMatching.Count);

			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(999, child.ZD1_Number);

			var allLogs = log.GetLogs();
			AssertNotContains("IDummyDependentBizo: Found a matching IDummyBizo but appending to it is not supported. Creating a new IDummyBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyBizo but cannot decide which to use. Creating a new IDummyBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyBizo but appending to them is not supported. Creating a new IDummyBizo instead.", allLogs);
		}

		/// <summary>
		/// When the parentCollection does NOT contain the item that is being imported, and reuse IS allowed, then
		/// add the incoming child item into a NEW parent in the parentCollection.
		/// </summary>
		public void TestPopulateFromDataRows_MatchedByManyColumns_NoMatch()
		{
			var parent = Factory.New<DummyWithMatchingSupportingCollection>();
			parent.ZD1_NumberUnitCode = "Not";
			parent.ZD1_Number = 5;
			parent.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);
			parentCollection.Add(parent);

			Factory.Save();

			AssertEquals("Pre condition: parent bizo has 0 child bizo.", 0, parent.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 2 items", 2, parentCollection.Count);
			AssertSame(parent, parentCollection[0]);
			AssertNotSame(parent, parentCollection[1]);

			var newParent = (parentCollection[1] as DummyWithMatchingSupportingCollection);
			var child = newParent.CollectionSupportsElementMatching.Single() as DummyWithMatchingSupportingCollection;

			AssertEquals("Not", parent.ZD1_NumberUnitCode);
			AssertEquals((ZByte)5, parent.ZD1_Number);
			AssertEquals(0, parent.CollectionSupportsElementMatching.Count);

			AssertEquals("ABC", newParent.ZD1_NumberUnitCode);
			AssertEquals((ZByte)3, newParent.ZD1_Number);
			AssertEquals(1, newParent.CollectionSupportsElementMatching.Count);

			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(999, child.ZD1_Number);

			var allLogs = log.GetLogs();
			AssertNotContains("IDummyDependentBizo: Found a matching IDummyBizo but appending to it is not supported. Creating a new IDummyBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyBizo but cannot decide which to use. Creating a new IDummyBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyBizo but appending to them is not supported. Creating a new IDummyBizo instead.", allLogs);
		}

		/// <summary>
		/// When the parentCollection contains multiple item that match what is being imported, and reuse IS allowed, then
		/// add the incoming child item into a new parent in the parentCollection
		/// </summary>
		public void TestPopulateFromDataRows_MatchedByManyColumns_ManyMatch_ReuseAllowed()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_NumberUnit = guid1;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			var parent2 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent2.ZD1_NumberUnitCode = "ABC";
			parent2.ZD1_Number = 3;
			parent2.ZD1_NumberUnit = guid2;
			parent2.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent2);

			Factory.Save();

			AssertEquals("Pre condition: parent1 bizo has 0 child bizo.", 0, parent1.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent2 bizo has 0 child bizo.", 0, parent2.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 2 item", 2, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 3 items", 3, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);
			AssertSame(parent2, parentCollection[1]);
			var parent3 = parentCollection[2] as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(guid1, parent1.ZD1_NumberUnit);
			AssertEquals(0, parent1.CollectionSupportsElementMatching.Count);

			AssertEquals("ABC", parent2.ZD1_NumberUnitCode);
			AssertEquals(3, parent2.ZD1_Number);
			AssertEquals(guid2, parent2.ZD1_NumberUnit);
			AssertEquals(0, parent2.CollectionSupportsElementMatching.Count);

			AssertEquals("ABC", parent3.ZD1_NumberUnitCode);
			AssertEquals(3, parent3.ZD1_Number);
			AssertEquals(1, parent3.CollectionSupportsElementMatching.Count);

			var child = parent3.CollectionSupportsElementMatching.Single() as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(999, child.ZD1_Number);

			var allLogs = log.GetLogs();
			AssertNotContains("IDummyDependentBizo: Found a matching IDummyDependentBizo but appending to it is not supported. Creating a new IDummyDependentBizo instead.", allLogs);
			AssertContains("IDummyDependentBizo: Found multiple matching IDummyDependentBizo but cannot decide which to use. Creating a new IDummyDependentBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyDependentBizo but appending to them is not supported. Creating a new IDummyDependentBizo instead.", allLogs);
		}

		/// <summary>
		/// When the parentCollection contains multiple item that match what is being imported, and reuse is NOT allowed, then
		/// add the incoming child item into a new parent in the parentCollection
		/// </summary>
		public void TestPopulateFromDataRows_MatchedByManyColumns_ManyMatch_ReuseNotAllowed()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: false);

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_NumberUnit = guid1;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			var parent2 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent2.ZD1_NumberUnitCode = "ABC";
			parent2.ZD1_Number = 3;
			parent2.ZD1_NumberUnit = guid2;
			parent2.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent2);

			Factory.Save();

			AssertEquals("Pre condition: parent1 bizo has 0 child bizo.", 0, parent1.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent2 bizo has 0 child bizo.", 0, parent2.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 2 item", 2, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 3 items", 3, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);
			AssertSame(parent2, parentCollection[1]);
			var parent3 = parentCollection[2] as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(guid1, parent1.ZD1_NumberUnit);
			AssertEquals(0, parent1.CollectionSupportsElementMatching.Count);

			AssertEquals("ABC", parent2.ZD1_NumberUnitCode);
			AssertEquals(3, parent2.ZD1_Number);
			AssertEquals(guid2, parent2.ZD1_NumberUnit);
			AssertEquals(0, parent2.CollectionSupportsElementMatching.Count);

			AssertEquals("ABC", parent3.ZD1_NumberUnitCode);
			AssertEquals(3, parent3.ZD1_Number);
			AssertEquals(1, parent3.CollectionSupportsElementMatching.Count);

			var child = parent3.CollectionSupportsElementMatching.Single() as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(999, child.ZD1_Number);

			var allLogs = log.GetLogs();
			AssertNotContains("IDummyDependentBizo: Found a matching IDummyDependentBizo but appending to it is not supported. Creating a new IDummyDependentBizo instead.", allLogs);
			AssertNotContains("IDummyDependentBizo: Found multiple matching IDummyDependentBizo but cannot decide which to use. Creating a new IDummyDependentBizo instead.", allLogs);
			AssertContains("IDummyDependentBizo: Found multiple matching IDummyDependentBizo but appending to them is not supported. Creating a new IDummyDependentBizo instead.", allLogs);
		}

		/// <summary>
		/// When the parentCollection contains a match to the parent being imported and the incoming
		/// child has an invalid column then confirm the logs have a message about the invalid column
		/// </summary>
		public void TestPopulateFromDataRows_InvalidColumn()
		{
			var guid1 = ZGuid.NewZGuid();
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_NumberUnit = guid1;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent1 bizo has 0 child bizo.", 0, parent1.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number_WRONGCOLUMN" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("999", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(!importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 1 items", 1, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(guid1, parent1.ZD1_NumberUnit);
			AssertEquals(1, parent1.CollectionSupportsElementMatching.Count);

			var child = parent1.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);

			var allLogs = log.GetLogs();
			AssertContains("DummyWithMatchingSupportingCollection does not have property ZD1_Number_WRONGCOLUMN in Row 1 in Column 1.", allLogs);
		}

		/// <summary>
		/// When the parentCollection contains a match to the parent being imported and the incoming child has
		/// data with an invalid type then confirm the logs have a message about the invalid type.
		/// </summary>
		public void TestPopulateFromDataRows_InvalidType()
		{
			var guid1 = ZGuid.NewZGuid();
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_NumberUnit = guid1;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent1 bizo has 0 child bizo.", 0, parent1.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("NOT A NUMBER HERE", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(!importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 1 items", 1, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(guid1, parent1.ZD1_NumberUnit);
			AssertEquals(1, parent1.CollectionSupportsElementMatching.Count);

			var child = parent1.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);

			var allLogs = log.GetLogs();
			AssertContains("Unable to set value 'NOT A NUMBER HERE' to the field 'ZD1_Number', as this field expects 'Integer'", allLogs);
		}

		public void TestErrorMessageWithRowIndexAndColumnIndex()
		{
			var guid1 = ZGuid.NewZGuid();
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_NumberUnit = guid1;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent1 bizo has 0 child bizo.", 0, parent1.CollectionSupportsElementMatching.Count);
			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("NOT A NUMBER HERE", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(!importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 1 items", 1, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(guid1, parent1.ZD1_NumberUnit);
			AssertEquals(1, parent1.CollectionSupportsElementMatching.Count);

			var child = parent1.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);

			var allLogs = log.GetLogs();
			AssertContains("Unable to set value 'NOT A NUMBER HERE' to the field 'ZD1_Number', as this field expects 'Integer'", allLogs);
		}

		/// <summary>
		/// When the parentCollection contains a match for the parent being imported and this
		/// match is based on a foreign key, then the parent can be matched and a child added
		/// to it.
		/// </summary>
		public void TestPopulateFromDataRows_ForeignKey_ZGuid_Matches()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0 = relatedDummy.PK;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Description" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("Apple", 2)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 1 items", 1, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(relatedDummy.PK, parent1.ZD1_Z0);
			AssertEquals(1, parent1.CollectionSupportsElementMatching.Count);

			var child = parent1.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(5, child.ZD1_Number);
		}

		/// <summary>
		/// When the parentCollection contains a match for the parent being imported and this
		/// match is based on a composite key, then the parent can be matched and a child added to it.
		/// </summary>
		public void TestPopulateFromDataRows_ForeignKey_CompositeKey_Matches()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";
			relatedDummy.Z0_AnotherNumber = 55;

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0 = relatedDummy.PK;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parent1.ZD1_Z0_TimesSet = 0;
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Description", "DummyBusinessObject.Z0_AnotherNumber" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("Apple", 2),
				new ImportPreviewLineDetails("55", 3)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 1 items", 1, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);
			AssertEquals("The foreign key should not have been set again as it was already correct", 0, parent1.ZD1_Z0_TimesSet);

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(relatedDummy.PK, parent1.ZD1_Z0);
			AssertEquals(1, parent1.CollectionSupportsElementMatching.Count);

			var child = parent1.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(5, child.ZD1_Number);
		}

		/// <summary>
		/// When the parentCollection does not contain a match for the parent being imported.
		/// Then the parent cannot be matched and a new parent is created with the incoming child.
		/// </summary>
		public void TestPopulateFromDataRows_ForeignKey_CompositeKey_NoMatchParent()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";
			relatedDummy.Z0_AnotherNumber = 55;

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 5;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0 = relatedDummy.PK;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Description", "DummyBusinessObject.Z0_AnotherNumber" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("Apple", 2),
				new ImportPreviewLineDetails("55", 3)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 2 items", 2, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);

			var parent2 = parentCollection[1] as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent2.ZD1_NumberUnitCode);
			AssertEquals(3, parent2.ZD1_Number);
			AssertEquals(relatedDummy.PK, parent2.ZD1_Z0);
			AssertEquals(1, parent2.CollectionSupportsElementMatching.Count);

			var child = parent2.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(5, child.ZD1_Number);
		}

		/// <summary>
		/// When the parentCollection contains a match for the parent being imported
		/// Then the parent can be matched, but the dependent bizo has a composite key
		/// without a match. So a null reference is set for it.
		/// </summary>
		public void TestPopulateFromDataRows_ForeignKey_CompositeKey_NoMatchChild()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";
			relatedDummy.Z0_AnotherNumber = 66;

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0 = relatedDummy.PK;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Description", "DummyBusinessObject.Z0_AnotherNumber" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("Apple", 2),
				new ImportPreviewLineDetails("55", 3)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 2 item", 2, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);

			var parent2 = parentCollection[1] as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent2.ZD1_NumberUnitCode);
			AssertEquals(3, parent2.ZD1_Number);
			AssertEquals(ZGuid.Empty, parent2.ZD1_Z0);
			AssertEquals(1, parent2.CollectionSupportsElementMatching.Count);

			var child = parent2.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(5, child.ZD1_Number);
		}

		/// <summary>
		/// When the parentCollection does not contain a match for the parent being imported and this
		/// match is based on a foreign key. Then the parent cannot be matched and a new parent is
		/// created with the incoming child.
		/// </summary
		public void TestPopulateFromDataRows_ForeignKey_ZGuid_NoMatch()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 5;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0 = relatedDummy.PK;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Description" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("Apple", 2)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 2 items", 2, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);
			AssertNotSame(parent1, parentCollection[1]);

			var parent2 = parentCollection[1] as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent2.ZD1_NumberUnitCode);
			AssertEquals(3, parent2.ZD1_Number);
			AssertEquals(relatedDummy.PK, parent2.ZD1_Z0);
			AssertEquals(1, parent2.CollectionSupportsElementMatching.Count);

			var child = parent2.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(5, child.ZD1_Number);
		}

		public void TestPopulateFromDataRows_ForeignKey_NaturalKey_MatchesColumn()
		{
			var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IDummyBizo", "Z0")
					.AddCollection("CollectionSupportsElementMatching", "IDummyBizo"),
				new MappingDataDefinition("IDummyDependentBizo", "ZD1")
					.AddRelation("DummyBusinessObject", "IDummyBizo", "ZD1_Z0_NKCode")
					.AddCollection("CollectionSupportsElementMatching", "IDummyDependentBizo")
			});

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";
			relatedDummy.Z0_Code = "X3333";

			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0_NKCode = "X3333";
			parent1.ZD1_Z0_NKCode_TimesSet = 0;
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Code", "DummyBusinessObject.Z0_Description" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("X3333", 2),
				new ImportPreviewLineDetails("Apple", 3)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 1 items", 1, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);
			AssertEquals("The natural foreign key should not have been updated if it was already set", 0, parent1.ZD1_Z0_NKCode_TimesSet);

			AssertEquals("ABC", parent1.ZD1_NumberUnitCode);
			AssertEquals(3, parent1.ZD1_Number);
			AssertEquals(relatedDummy.Z0_Code, parent1.ZD1_Z0_NKCode);
			AssertEquals(1, parent1.CollectionSupportsElementMatching.Count);

			var child = parent1.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(5, child.ZD1_Number);
		}

		public void TestPopulateFromDataRows_ForeignKey_NaturalKey_NoMatches()
		{
			var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IDummyBizo", "Z0")
					.AddCollection("CollectionSupportsElementMatching", "IDummyBizo"),
				new MappingDataDefinition("IDummyDependentBizo", "ZD1")
					.AddRelation("DummyBusinessObject", "IDummyBizo", "ZD1_Z0_NKCode")
					.AddCollection("CollectionSupportsElementMatching", "IDummyDependentBizo")
			});

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";
			relatedDummy.Z0_Code = "X3333";

			var relatedDummy2 = Factory.New<DummyBusinessObject>();
			relatedDummy2.Z0_Description = "Peach";
			relatedDummy2.Z0_Code = "X3334";
			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0_NKCode = "X3333";
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			AssertEquals("Pre condition: parent collection has 1 item", 1, parentCollection.Count);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Code" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("X3334", 2),
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			AssertEquals("Parent collection has 2 items", 2, parentCollection.Count);
			AssertSame(parent1, parentCollection[0]);
			var parent2 = parentCollection[1] as DummyWithMatchingSupportingCollection;

			AssertEquals("ABC", parent2.ZD1_NumberUnitCode);
			AssertEquals(3, parent2.ZD1_Number);
			AssertEquals(relatedDummy2.Z0_Code, parent2.ZD1_Z0_NKCode);
			AssertEquals(1, parent2.CollectionSupportsElementMatching.Count);

			var child = parent2.CollectionSupportsElementMatching[0] as DummyWithMatchingSupportingCollection;
			AssertEquals("NIC", child.ZD1_Code);
			AssertEquals(5, child.ZD1_Number);
		}

		public void TestPopulateFromDataRows_RelationIsCanCreate()
		{
			var hvlvItemCollection = new Mock<IBusinessObjectCollection>();
			hvlvItemCollection.Setup(c => c.Factory).Returns(Factory);
			hvlvItemCollection.Setup(c => c.TypeOfElements).Returns(ObjectFactory.GetType<IHVLVItem>());
			hvlvItemCollection.Setup(c => c.AddNew()).Returns(() =>
			{
				var item = (BusinessObject)Factory.New<IHVLVItem>();
				importedElements.Add(item);
				return item;
			});

			var header = new ImportPreviewHeader(string.Empty, new[] { "HVI_CurrentBarcode",
																		"OuterPackage.HVO_PackageReference",
																		"OuterPackage.HVO_F3_NKPackageType" }) { };

			var line1 = new ImportPreviewLine(string.Empty, 0, new[] {
							new ImportPreviewLineDetails("Item1",1),
							new ImportPreviewLineDetails("PKG011", 2),
							new ImportPreviewLineDetails("BAG", 3), });

			var line2 = new ImportPreviewLine(string.Empty, 0, new[] {
							new ImportPreviewLineDetails("Item2",1),
							new ImportPreviewLineDetails("PKG011", 2),
							new ImportPreviewLineDetails("BAG", 3), });

			var line3 = new ImportPreviewLine(string.Empty, 0, new[] {
							new ImportPreviewLineDetails("Item3",1),
							new ImportPreviewLineDetails("PKG012", 2),
							new ImportPreviewLineDetails("12L", 3), });

			var line4 = new ImportPreviewLine(string.Empty, 0, new[] {
							new ImportPreviewLineDetails("Item3",1),
							new ImportPreviewLineDetails("PKG012", 2),
							new ImportPreviewLineDetails("28L", 3), });

			dataRows = new[] {
				new ImportPreview(new[] { header }, line1),
				new ImportPreview(new[] { header }, line2),
				new ImportPreview(new[] { header }, line3),
				new ImportPreview(new[] { header }, line4),
			};

			var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
				{
					new MappingDataDefinition("IHVLVItem", "HVI")
					.AddRelation("OuterPackage", "IHVLVOuterPackage", "HVI_HVO_OuterPackage", true),
					new MappingDataDefinition("IHVLVOuterPackage", "HVO")
				}) ;

			Assert(importer.PopulateFromDataRows(hvlvItemCollection.Object, dataRows, mappingDataModel, log, progressReporter));
			var outerPackages = Factory.Load<IHVLVOuterPackage>(new ZQuery());
			CombineAssertions("new outer packages created", () => {
				AssertEquals("outer packages count", 3, outerPackages.Length);
				Assert(outerPackages.Any(package => package.HVO_PackageReference == "PKG011" && package.HVO_F3_NKPackageType == "BAG"));
				Assert(outerPackages.Any(package => package.HVO_PackageReference == "PKG012" && package.HVO_F3_NKPackageType == "12L"));
				Assert(outerPackages.Any(package => package.HVO_PackageReference == "PKG012" && package.HVO_F3_NKPackageType == "28L"));
			});
		}

		#endregion

		public void TestPopulateFromDataRows_CheckCreationSourceSet()
		{
			var mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IDummyBizo", "Z0")
					.AddCollection("CollectionSupportsElementMatching", "IDummyBizo"),
				new MappingDataDefinition("IDummyDependentBizo", "ZD1")
					.AddRelation("DummyBusinessObject", "IDummyBizo", "ZD1_Z0_NKCode")
					.AddCollection("CollectionSupportsElementMatching", "IDummyDependentBizo")
			});

			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: true);

			var relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Description = "Apple";
			relatedDummy.Z0_Code = "X3333";

			var relatedDummy2 = Factory.New<DummyBusinessObject>();
			relatedDummy2.Z0_Description = "Peach";
			relatedDummy2.Z0_Code = "X3334";
			var parent1 = Factory.New<DummyWithMatchingSupportingCollection>();
			parent1.ZD1_NumberUnitCode = "ABC";
			parent1.ZD1_Number = 3;
			parent1.ZD1_Code = "xyz";
			parent1.ZD1_Z0_NKCode = "X3333";
			parent1.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);
			parentCollection.Add(parent1);

			Factory.Save();

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Code" });
			var headerForChild = new ImportPreviewHeader("CollectionSupportsElementMatching", new[] { "CollectionSupportsElementMatching.ZD1_Code", "CollectionSupportsElementMatching.ZD1_Number" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("X3334", 2)
			});
			var dataForChild = new ImportPreviewLine("CollectionSupportsElementMatching", 1, new[]
			{
				new ImportPreviewLineDetails("NIC", 0),
				new ImportPreviewLineDetails("5", 1)
			});
			dataForParent.ChildLines.Add(dataForChild);

			dataRows = new[] { new ImportPreview(new[] { headerForParent, headerForChild }, dataForParent) };

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));

			var parent2 = parentCollection[1] as DummyWithMatchingSupportingCollection;
			AssertEquals("ADW", parent2.CreationSource);
		}

		public void TestPopulateFromDataRows_SuspendListChanged()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: false);

			var headerForParent = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode", "ZD1_Number", "DummyBusinessObject.Z0_Description" });

			var dataForParent = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
				new ImportPreviewLineDetails("3", 1),
				new ImportPreviewLineDetails("Apple", 2)
			});

			dataRows = new[] { new ImportPreview(new[] { headerForParent }, dataForParent) };

			var parentCollectionListChangedCount = 0;
			((IBindingList)parentCollection).ListChanged += new ListChangedEventHandler(ParentCollection_ListChanged);

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals("list changed should be suspended and only fire once on dispose of the suspender", 1, parentCollectionListChangedCount);

			void ParentCollection_ListChanged(object sender, ListChangedEventArgs e)
			{
				parentCollectionListChangedCount++;
			}
		}

		public void TestPopulateFromDataRows_SuspendAdditionallyForImport()
		{
			var parentCollection = new DummyParentCollectionWithElementMatching(Factory, allowReuseUponMatch: false, enableSuspendAdditionallyForImport: true);

			var header = new ImportPreviewHeader(string.Empty, new[] { "ZD1_NumberUnitCode" });

			var dataLine1 = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("ABC", 0),
			});

			dataRows = new[] { new ImportPreview(new[] { header }, dataLine1) };

			var countChangedCount = 0;
			parentCollection.CountChanged += new CollectionCountChangedEventHandler(ParentCollection_CountChanged);

			Assert(importer.PopulateFromDataRows(parentCollection, dataRows, mappingDataModel, log, progressReporter));
			AssertEquals("count changed was suspended within SuspendAdditionallyForImport", 0, countChangedCount);

			void ParentCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
			{
				countChangedCount++;
			}
		}

		#region Custom importing

		public void TestCustomImportingSupportsRateLine()
		{
			var collectionMock = new Mock<IBusinessObjectCollection>(MockBehavior.Strict);
			var importedRateEntries = new List<BusinessObject>();
			var rateEntryType = Type.GetType("Enterprise.Rating.Business.RateEntry, Enterprise.Rating.Business");
			var rateLineType = Type.GetType("Enterprise.Rating.Business.RateLine, Enterprise.Rating.Business");
			var rateLineItemType = Type.GetType("Enterprise.Rating.Business.RateLineItem, Enterprise.Rating.Business");

			collectionMock.Setup(c => c.SuspendAdditionallyForImport()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(c => c.SuspendListChanged()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(x => x.TypeOfElements).Returns(rateEntryType);
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var rateEntry = Factory.New(rateEntryType);
				importedRateEntries.Add(rateEntry);
				return rateEntry;
			});

			var lineHeaders = new[]
			{
				new ImportPreviewHeader(string.Empty, new [] { "TI_DestinationLRC", "TI_Mode" }),
				new ImportPreviewHeader("RateLines", new [] { "RateLines.TL_RateCalculator" }),
				new ImportPreviewHeader("RateCalculators_MPU", new [] { "RateCalculators_MPU.MinimumAmount", "RateCalculators_MPU.PerUnitAmount" }),
			};

			var rateEntryPreview = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AUSYD", 0),
				new ImportPreviewLineDetails("FCL", 1)
			});
			var rateLinePreview = new ImportPreviewLine("RateLines", 1, new[]
			{
				new ImportPreviewLineDetails("MPU", 0)
			});
			var rateLineCalculatorPreview = new ImportPreviewLine("RateCalculators_MPU", 2, new[]
			{
				new ImportPreviewLineDetails("100", 0),
				new ImportPreviewLineDetails("200", 1)
			});

			rateEntryPreview.ChildLines.Add(rateLinePreview);
			rateLinePreview.ChildLines.Add(rateLineCalculatorPreview);

			var importPreview = new ImportPreview(lineHeaders, rateEntryPreview);

			var mappingModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IRateEntry", "TI")
					.AddCollection("RateLines", "IRateLine"),
				new MappingDataDefinition("IRateLine", "TL")
					.AddRelation("RateEntry", "IRateEntry", "TL_TI")
					.AddCollection("RateCalculators_MPU", "RateCalculator.MPU"),
				new MappingDataDefinition("RateCalculator.MPU", ""),
			});

			Assert(importer.PopulateFromDataRows(collectionMock.Object, new ImportPreview[] { importPreview }, mappingModel, log, progressReporter));

			var rateLines = Factory.Load(rateLineType, new ZQuery(Schema.RateLinesSchema.TL_TI, importedRateEntries[0].PK));
			var rateLine = rateLines.Single();
			var rateLineItems = Factory.Load(rateLineItemType, new ZQuery(Schema.RateLineItemsSchema.TM_TL, rateLine.PK));

			AssertEquals("There should be two rate line items", 2, rateLineItems.Length);
			var minRateLineItem = rateLineItems.Single(i => (ZString)i[Schema.RateLineItemsSchema.TM_Type] == "MIN");
			var perUnitRateLineItem = rateLineItems.Single(i => (ZString)i[Schema.RateLineItemsSchema.TM_Type] == "UNT");

			var minimumValue = (int)(ZDecimal)minRateLineItem[Schema.RateLineItemsSchema.TM_Value];
			var perUnitValue = (int)(ZDecimal)perUnitRateLineItem[Schema.RateLineItemsSchema.TM_Value];
			AssertEquals("Minimum should be 100", 100, minimumValue);
			AssertEquals("Per Unit should be 200", 200, perUnitValue);
		}

		public void TestCustomImportingSupportsRateEntryEmptyStrings()
		{
			var collectionMock = new Mock<IBusinessObjectCollection>(MockBehavior.Strict);
			var importedRateEntries = new List<BusinessObject>();
			var rateEntryType = Type.GetType("Enterprise.Rating.Business.RateEntry, Enterprise.Rating.Business");

			collectionMock.Setup(c => c.SuspendAdditionallyForImport()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(c => c.SuspendListChanged()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(x => x.TypeOfElements).Returns(rateEntryType);
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var rateEntry = Factory.New(rateEntryType);
				importedRateEntries.Add(rateEntry);
				return rateEntry;
			});

			var lineHeaders = new[]
			{
				new ImportPreviewHeader(string.Empty, new [] { "TI_DestinationLRC", "TI_Mode", "Warehouse" }),
			};

			var rateEntryPreview = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AUSYD", 0),
				new ImportPreviewLineDetails("FCL", 1),
				new ImportPreviewLineDetails(string.Empty, 2)
			});

			var mappingModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IRateEntry", "TI")
			});

			var importPreview = new ImportPreview(lineHeaders, rateEntryPreview);

			Assert(importer.PopulateFromDataRows(collectionMock.Object, new ImportPreview[] { importPreview }, mappingModel, log, progressReporter));

			var rateEntry = Factory.Load(rateEntryType, new ZQuery(Schema.RateEntrySchema.PK, importedRateEntries[0].PK)).FirstOrDefault();

			var parentId = rateEntry[Schema.RateEntrySchema.TI_ParentID];
			var parentTableCode = rateEntry[Schema.RateEntrySchema.TI_ParentTableCode];

			AssertEquals("Parent ID Should be empty", Guid.Empty, parentId);
			AssertEquals("Parent Table Code should be WW", "WW", parentTableCode);
		}

		void TestCustomImportingSupportsRateLineChargeCodes(string assertionMessage, bool expectIsGlobalChargeCode)
		{
			var collectionMock = new Mock<IBusinessObjectCollection>(MockBehavior.Strict);
			var importedRateEntries = new List<BusinessObject>();
			var ratingHeaderType = Type.GetType("Enterprise.Rating.Business.RatingHeader, Enterprise.Rating.Business");
			var rateEntryType = Type.GetType("Enterprise.Rating.Business.RateEntry, Enterprise.Rating.Business");
			var rateLineType = Type.GetType("Enterprise.Rating.Business.RateLine, Enterprise.Rating.Business");
			var rateLineItemType = Type.GetType("Enterprise.Rating.Business.RateLineItem, Enterprise.Rating.Business");

			collectionMock.Setup(c => c.SuspendAdditionallyForImport()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(c => c.SuspendListChanged()).Returns(DisposableAction.NoAction);
			collectionMock.Setup(x => x.TypeOfElements).Returns(rateEntryType);
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var rateEntry = expectIsGlobalChargeCode ? Factory.NewWithValidTestData(rateEntryType) : Factory.New(rateEntryType);
				importedRateEntries.Add(rateEntry);

				if (expectIsGlobalChargeCode)
				{
					var ratingHeader = Factory.Load(ratingHeaderType, new ZQuery(Schema.RatingHeaderSchema.PK, rateEntry[Schema.RateEntrySchema.TI_TH])).Single();
					ratingHeader[Schema.RatingHeaderSchema.TH_GC] = null;
				}

				return rateEntry;
			});

			var (localChargeCode, globalChargeCode) = GenerateLocalAndGlobalChargeCode();

			var lineHeaders = new[]
			{
				new ImportPreviewHeader(string.Empty, new [] { "TI_DestinationLRC", "TI_Mode" }),
				new ImportPreviewHeader("RateLines", new [] { "RateLines.TL_RateCalculator", "RateLines.AccChargeCode.AC_Code" }),
				new ImportPreviewHeader("RateCalculators_MPU", new [] { "RateCalculators_MPU.MinimumAmount", "RateCalculators_MPU.PerUnitAmount" }),
			};

			var rateEntryPreview = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AUSYD", 0),
				new ImportPreviewLineDetails("FCL", 1)
			});
			var rateLinePreview = new ImportPreviewLine("RateLines", 1, new[]
			{
				new ImportPreviewLineDetails("MPU", 0),
				new ImportPreviewLineDetails("SKYWALKER", 1)
			});
			var rateLineCalculatorPreview = new ImportPreviewLine("RateCalculators_MPU", 2, new[]
			{
				new ImportPreviewLineDetails("100", 0),
				new ImportPreviewLineDetails("200", 1)
			});

			rateEntryPreview.ChildLines.Add(rateLinePreview);
			rateLinePreview.ChildLines.Add(rateLineCalculatorPreview);

			var importPreview = new ImportPreview(lineHeaders, rateEntryPreview);

			var mappingModel = new MappingDataModel(new MappingDataDefinition[]
			{
				new MappingDataDefinition("IRateEntry", "TI")
					.AddCollection("RateLines", "IRateLine"),
				new MappingDataDefinition("IRateLine", "TL")
					.AddRelation("RateEntry", "IRateEntry", "TL_TI")
					.AddRelation("AccChargeCode", "IAccChargeCode", "TL_AC")
					.AddCollection("RateCalculators_MPU", "RateCalculator.MPU"),
				new MappingDataDefinition("IAccChargeCode", "AC"),
				new MappingDataDefinition("RateCalculator.MPU", ""),
			});

			Assert(importer.PopulateFromDataRows(collectionMock.Object, new ImportPreview[] { importPreview }, mappingModel, log, progressReporter));

			var rateLines = Factory.Load(rateLineType, new ZQuery(Schema.RateLinesSchema.TL_TI, importedRateEntries[0].PK));
			var rateLine = rateLines.Single();

			var chargeCode = (ZGuid)rateLine[Schema.RateLinesSchema.TL_AC];

			var comparisonPk = expectIsGlobalChargeCode ? globalChargeCode.PK : localChargeCode.PK;
			AssertEquals(assertionMessage, comparisonPk, chargeCode);
		}

		public void TestCustomImportingSupportsRateLineChargeCodes_Global()
		{
			TestCustomImportingSupportsRateLineChargeCodes("The charge code should be the global charge code", true);
		}

		public void TestCustomImportingSupportsRateLineChargeCodes_Local()
		{
			TestCustomImportingSupportsRateLineChargeCodes("The charge code should be the local charge code", false);
		}

		(BusinessObject, BusinessObject) GenerateLocalAndGlobalChargeCode()
		{
			var accChargeCodeType = Type.GetType("Enterprise.MasterFiles.Business.AccChargeCode, Enterprise.MasterFiles.Business");

			var globalChargeCode = Factory.NewWithValidTestData(accChargeCodeType);
			globalChargeCode[Schema.AccChargeCodeSchema.AC_Code] = "SKYWALKER";
			globalChargeCode[Schema.AccChargeCodeSchema.AC_GC] = ZGuid.Empty;
			globalChargeCode[Schema.AccChargeCodeSchema.AC_ChargeType] = "MRG";
			globalChargeCode[Schema.AccChargeCodeSchema.AC_ChargeGroup] = "FRT";
			globalChargeCode[Schema.AccChargeCodeSchema.AC_Desc] = "PLACEHOLDER TEXT";

			Factory.Save();

			var query = new ZQuery(Schema.AccChargeCodeSchema.AC_Code, "SKYWALKER");
			query.AddToFilter(Schema.AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);

			var localChargeCode = Factory.Load(accChargeCodeType, query).Single();

			return (localChargeCode, globalChargeCode);
		}

		#endregion

		GlowCollectionImporter importer;
		ImportPreview[] dataRows;
		Mock<IBusinessObjectCollection> collectionMock;
		List<BusinessObject> importedElements;
		GlowLog log;
		IProgressReporter progressReporter;
		ZForm form;

		readonly MappingDataModel mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
		{
			new MappingDataDefinition("IGlbBranch", "GB")
				.AddRelation("HomePort", "IRefUNLOCO", "GB_RL_NKHomePort")
				.AddCollection("GlbHolidays", "IGlbHoliday"),
			new MappingDataDefinition("IGlbHoliday", "GH"),
			new MappingDataDefinition("IRefUNLOCO", "RL"),
			new MappingDataDefinition("IGlbStaff", "GS"),
			new MappingDataDefinition("IGlbGroup", "GG")
				.AddCollection("GlbGroupLinks", "IGlbGroupLink"),
			new MappingDataDefinition("IGlbGroupLink", "GK"),
			new MappingDataDefinition("IBMBoard", "MB")
				.AddRelation("Staff", "IGlbStaff", "MB_GS_NKStaffCode")
				.AddCollection("BMBoardSections", "IBMBoardSection"),
			new MappingDataDefinition("IBMBoardSection", "MS")
				.AddCollection("BMBoardSectionChannels", "IBMBoardSectionChannel"),
			new MappingDataDefinition("IBMBoardSectionChannel", "MSC"),
			new MappingDataDefinition("IDtbConsignment", "LTC")
				.AddCollection("Addresses", "IAddress[[IDtbConsignment]]")
				.AddCollection("DtbConsignmentAddresses", "IDtbConsignmentAddress"),
			new MappingDataDefinition("IAddress[[IDtbConsignment]]", "E2"),
			new MappingDataDefinition("IDtbConsignmentAddress", "LTS"),
			new MappingDataDefinition("IWhsDocket", "WD")
				.AddCollection("WhsDocketLines", "IWhsDocketLine"),
			new MappingDataDefinition("IWhsDocketLine", "WE")
				.AddCollection("WhsInventoryViews", "IWhsInventoryView"),
			new MappingDataDefinition("IWhsInventoryView", "WI"),
			new MappingDataDefinition("IJobContainer", "JC")
				.AddCollection("ReferenceNumbers", "IReferenceNumber[[IJobContainer]]"),
			new MappingDataDefinition("IReferenceNumber[[IJobContainer]]", "CE"),
			new MappingDataDefinition("IDummyBizo", "Z0")
				.AddCollection("CollectionSupportsElementMatching", "IDummyBizo"),
			new MappingDataDefinition("IDummyDependentBizo", "ZD1")
				.AddRelation("DummyBusinessObject", "IDummyBizo", "ZD1_Z0")
				.AddCollection("CollectionSupportsElementMatching", "IDummyDependentBizo")
		});

		protected override void SetUp()
		{
			base.SetUp();
			importedElements = new List<BusinessObject>();

			log = new GlowLog();
			form = new ZForm();

			var collectionPropertyHelperMock = new Mock<IDependentCollectionPropertyHelper>();
			collectionPropertyHelperMock.Setup(h => h.GetDependentCollection(It.IsAny<BusinessObject>(), It.IsAny<Type>(), It.IsAny<Type>())).Returns((BusinessObject obj, Type itemType, Type rootType) =>
			{
				if (itemType == null)
				{
					return null;
				}

				if (itemType.Name == "GlbHoliday")
				{
					return obj["GlbHolidays"] as IBusinessObjectCollection;
				}
				else if (itemType.Name == "BMBoardSection")
				{
					return obj["Sections"] as IBusinessObjectCollection;
				}
				else if (itemType.Name == "DtbConsignmentAddress")
				{
					return obj["Addresses"] as IBusinessObjectCollection;
				}
				else if (itemType.Name == "WhsDocketLine")
				{
					return obj["Lines"] as IBusinessObjectCollection;
				}
				else if (itemType.Name == "WhsInventoryView")
				{
					return obj["Inventory"] as IBusinessObjectCollection;
				}
				else if (itemType.Name == "DummyBaseBusinessObject")
				{
					return obj["CollectionSupportsElementMatching"] as IBusinessObjectCollection;
				}

				return null;
			});

			ObjectFactory.Substitute(collectionPropertyHelperMock.Object);

			var businessObjectType = ObjectFactory.GetType<IGlbBranch>();
			collectionMock = new Mock<IBusinessObjectCollection>();
			collectionMock.Setup(c => c.Factory).Returns(Factory);
			collectionMock.Setup(c => c.TypeOfElements).Returns(businessObjectType);
			collectionMock.Setup(c => c.AddNew()).Returns(() =>
			{
				var branch = (BusinessObject)Factory.New<IGlbBranch>();
				importedElements.Add(branch);
				return branch;
			});

			var importPreviewHeader1 = new ImportPreviewHeader(string.Empty, new[] { "GB_Code", "GB_BranchName", "GB_IsActive", "HomePort.RL_Code" });
			var importPreviewHeader2 = new ImportPreviewHeader("GlbHolidays", new[] { "GlbHolidays.GH_HolidayName", "GlbHolidays.GH_Date" });

			var importPreviewLine = new ImportPreviewLine(string.Empty, 0, new[]
			{
				new ImportPreviewLineDetails("AAA", 0),
				new ImportPreviewLineDetails("True branch", 1),
				new ImportPreviewLineDetails("True", 2),
				new ImportPreviewLineDetails("USLAX", 3)
			});
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("GlbHolidays", 1, new[]
			{
				new ImportPreviewLineDetails("Queen birthday", 0),
				new ImportPreviewLineDetails("12-06-2017", 1)
			}));
			importPreviewLine.ChildLines.Add(new ImportPreviewLine("GlbHolidays", 2, new[]
			{
				new ImportPreviewLineDetails("Constitution day", 0),
				new ImportPreviewLineDetails("24-06-2017", 1)
			}));

			dataRows = new[] { new ImportPreview(new[] { importPreviewHeader1, importPreviewHeader2 }, importPreviewLine) };

			importer = new GlowCollectionImporter();
			progressReporter = GetProgressFromProvider().CreateProgressReporter("Processed", 100);
		}

		IProgressReporterProvider GetProgressFromProvider()
		{
			return new DefaultProgressReporterProvider(form);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
			progressReporter.Dispose();
			form.Dispose();
		}
	}

	internal class DummyWithMatchingSupportingCollection : DummyDependantBusinessObject
	{
		public DummyWithMatchingSupportingCollection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			var stack = ObjectFactory.Get<IBusinessObjectCreationSourceStack>();
			CreationSource = stack.CurrentSource?.CreationSourceCode ?? "Potato";
		}

		public string CreationSource { get; }
		public int ZD1_Z0_NKCode_TimesSet { get; set; }
		public override ZString ZD1_Z0_NKCode
		{
			get
			{
				return base.ZD1_Z0_NKCode;
			}
			set
			{
				ZD1_Z0_NKCode_TimesSet++;
				base.ZD1_Z0_NKCode = value;
			}
		}

		public int ZD1_Z0_TimesSet { get; set; }
		public override ZGuid ZD1_Z0
		{
			get
			{
				return base.ZD1_Z0;
			}
			set
			{
				ZD1_Z0_TimesSet++;
				base.ZD1_Z0 = value;
			}
		}

		public DummyChildCollectionWithElementMatching CollectionSupportsElementMatching { get; set; }
	}

	internal class DummyParentCollectionWithElementMatching : DummyDependentBusinessObjectCollection, IImportCollectionElementMatchingSupporter
	{
		readonly bool allowReuse;
		readonly bool findGenericMatches;
		readonly bool enableSuspendAdditionallyForImport;

		protected override BusinessObject AddNewCore()
		{
			var newBizo = base.AddNewCore(typeof(DummyWithMatchingSupportingCollection)) as DummyWithMatchingSupportingCollection;
			newBizo.CollectionSupportsElementMatching = new DummyChildCollectionWithElementMatching(Factory);

			return newBizo;
		}

		public DummyParentCollectionWithElementMatching(BusinessObjectFactory factory, bool allowReuseUponMatch = false, bool findGenericMatches = true, bool enableSuspendAdditionallyForImport = false)
			: base(factory)
		{
			allowReuse = allowReuseUponMatch;
			this.findGenericMatches = findGenericMatches;
			this.enableSuspendAdditionallyForImport = enableSuspendAdditionallyForImport;
		}

		string IImportCollectionElementMatchingSupporter.MatchingColumnName => DummyDependentBizoSchema.ZD1_Code.Name;

		bool IImportCollectionElementMatchingSupporter.IsGenericColumnMatchingAllowed => allowReuse;

		bool IImportCollectionElementMatchingSupporter.FindGenericColumnMatches => findGenericMatches;

		BusinessObject IImportCollectionElementMatchingSupporter.GetMatchingBizObject(string matchingKey)
		{
			return Find(x => x.ZD1_Code == matchingKey).FirstOrDefault();
		}

		void IImportCollectionElementMatchingSupporter.PrepareForReuse(BusinessObject matchedBizO)
		{
		}

		public override IDisposable SuspendAdditionallyForImport()
		{
			if (enableSuspendAdditionallyForImport)
			{
				return SuspendCountChanged(null);
			}

			return base.SuspendAdditionallyForImport();
		}
	}

	internal class DummyChildCollectionWithElementMatching : DummyDependentBusinessObjectCollection, IImportCollectionElementMatchingSupporter
	{
		readonly bool allowReuse;

		public DummyChildCollectionWithElementMatching(BusinessObjectFactory factory, bool allowReuseUponMatch = false)
			: base(factory)
		{
			allowReuse = allowReuseUponMatch;
		}

		protected override BusinessObject AddNewCore()
		{
			return base.AddNewCore(typeof(DummyWithMatchingSupportingCollection));
		}

		bool IImportCollectionElementMatchingSupporter.IsGenericColumnMatchingAllowed => allowReuse;

		bool IImportCollectionElementMatchingSupporter.FindGenericColumnMatches => true;

		string IImportCollectionElementMatchingSupporter.MatchingColumnName => DummyDependentBizoSchema.ZD1_Code.Name;

		BusinessObject IImportCollectionElementMatchingSupporter.GetMatchingBizObject(string matchingKey)
		{
			return Find(x => x.ZD1_Code == matchingKey).FirstOrDefault();
		}

		void IImportCollectionElementMatchingSupporter.PrepareForReuse(BusinessObject matchedBizO)
		{
			((DummyWithMatchingSupportingCollection)matchedBizO).ZD1_NumberUnitCode = "AFT";
		}
	}
}
