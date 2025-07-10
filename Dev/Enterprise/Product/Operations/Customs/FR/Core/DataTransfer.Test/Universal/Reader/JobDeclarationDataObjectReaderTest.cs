using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.UniversalReferenceConstants;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using LineMerger = Enterprise.Customs.FR.Business.Declaration.LineMerger;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing;

sealed class JobDeclarationDataObjectReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
{
	#region TestReadIntoBusinessObject_PackInvoiceLinesPivots

	public void TestReadIntoBusinessObject_PackInvoiceLinesPivots_ByDefault()
	{
		(var declaration, _, var reader, _) = GetInfoForSnapshotRevertingTest();
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[0], 2, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[1], 10, 15, "1B");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[0], 8, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[1], 5, 15, "1B");

		reader.ReadIntoBusinessObject();

		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[0], 3, 12, "2A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[1], 13, 20, "2B");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[0], 9, 12, "2A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[1], 7, 20, "2B");
	}

	public void TestReadIntoBusinessObject_PackInvoiceLinesPivots_SnapshotRevertingWithStrategyOverride()
	{
		(var declaration, _, var reader, var entryHeaderToRevert) = GetInfoForSnapshotRevertingTest();
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[0], 2, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[1], 10, 15, "1B");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[0], 8, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[1], 5, 15, "1B");

		using (reader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, SnapshotRevertingStrategy.Override, new TestErrorLogger()))
		{
			reader.ReadIntoBusinessObject();
		}

		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[0], 3, 12, "2A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[1], 13, 20, "2B");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[0], 9, 12, "2A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[1], 7, 20, "2B");
	}

	public void TestReadIntoBusinessObject_PackInvoiceLinesPivots_SnapshotRevertingWithStrategySkip()
	{
		(var declaration, _, var reader, var entryHeaderToRevert) = GetInfoForSnapshotRevertingTest();
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[0], 2, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[1], 10, 15, "1B");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[0], 8, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[1], 5, 15, "1B");

		using (reader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, SnapshotRevertingStrategy.Skip, new TestErrorLogger()))
		{
			reader.ReadIntoBusinessObject();
		}

		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[0], 2, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[0].InvoiceLines[1], 13, 20, "2B");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[0], 8, 10, "1A");
		AssertInvoiceLinePivotProperties(declaration.Invoices[1].InvoiceLines[1], 7, 20, "2B");
	}

	void AssertInvoiceLinePivotProperties(JobComInvoiceLine line, int numberOfPacks, int linkedPackQty, string linkedPackType)
	{
		InvoiceLinePackagePivot pivot = null;
		line.PackagesPivot.Reload(true);
		AssertNoExceptionThrown(() => { pivot = line.PackagesPivot.Cast<InvoiceLinePackagePivot>().Single(); });
		AssertEquals("CHC_NumberOfPacks:", numberOfPacks, pivot.CHC_NumberOfPacks);
		AssertEquals("Package.CW_PackQty:", linkedPackQty, pivot.Package.CW_PackQty);
		AssertEquals("Package.CW_PackType:", linkedPackType, pivot.Package.CW_PackType);
	}

	#endregion

	#region TestReadIntoBusinessObject_JEProperties

	public void TestReadIntoBusinessObject_JEProperties_ByDefault()
	{
		using var a = eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		(var declaration, var shipmentDataObject, var reader, _) = GetInfoForSnapshotRevertingTest();
		CombineAssertions("By default, JobDeclarationDataObjectReader should have the values for JobDeclaration read into BO without exceptions.", () =>
		{
			declaration.JE_RL_NKOrigin = "FRPAR";
			new JobDeclarationDataObjectReader(shipmentDataObject, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("CNSHP", declaration.JE_RL_NKOrigin);

			AssertEquals("JE_MessageType is not a exception by default logic.", "EXP", declaration.JE_MessageType);
			AssertEquals("When IsUXMLImportingData of declaration is true, JE_CustomsProfile will not update when other fields change.", "56789", declaration.JE_CustomsProfile);
		});
	}

	public void TestReadIntoBusinessObject_JEProperties_SnapshotRevertingWithStrategyOverride()
	{
		(var declaration, _, var reader, var entryHeaderToRevert) = GetInfoForSnapshotRevertingTest();
		CombineAssertions("When JobDeclarationDataObjectReader is used for snapshot reverting and the strategy is Override, values for JobDeclaration should be read into BO with exceptions.", () =>
		{
			declaration.JE_RL_NKOrigin = "FRPAR";
			using (reader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Override, new TestErrorLogger()))
			{
				reader.ReadIntoBusinessObject();
			}
			AssertEquals("CNSHP", declaration.JE_RL_NKOrigin);

			AssertEquals("[Exception1]: JE_MessageType should not be reverted.", "IMP", declaration.JE_MessageType);
			AssertEquals("[Exception2]: JE_CustomsProfile should not be reverted.", "12345", declaration.JE_CustomsProfile);
		});
	}

	public void TestReadIntoBusinessObject_OrganisationsShouldNotChange_SnapshotReverting()
	{
		(var declaration, _, var reader, var entryHeaderToRevert) = GetInfoForSnapshotRevertingTest();
		declaration.SetupImporter(CreateOrganisation(Factory, "MAX", "MAX!@#1"));
		declaration.SetupSupplier(CreateOrganisation(Factory, "SUP", "SUP!@#2"));
		var declarantHeader = CreateOrganisation(Factory, "DEC", "DEC!@#2");
		declaration.SetupDeclarant(declarantHeader.MainAddress);

		CombineAssertions("Supplier, Importer and Declarant should not be reverted when JobDeclarationDataObjectReader is used for snapshot reverting with strategy Override or Skip.", () =>
		{
			using (reader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Override, new TestErrorLogger()))
			{
				reader.ReadIntoBusinessObject();
			}
			AssertEquals("[Exception1]: Importer should not be reverted (Strategy: Override).", "MAX", declaration.Importer.OH_FullName);
			AssertEquals("[Exception2]: Supplier should not be reverted (Strategy: Override).", "SUP", declaration.Supplier.OH_FullName);
			AssertEquals("[Exception3]: Declarant should not be reverted (Strategy: Override).", "DEC", declaration.Declarant.Header.OH_FullName);

			using (reader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Skip, new TestErrorLogger()))
			{
				reader.ReadIntoBusinessObject();
			}

			AssertEquals("[Exception1]: Importer should not be reverted (Strategy: Skip).", "MAX", declaration.Importer.OH_FullName);
			AssertEquals("[Exception2]: Supplier should not be reverted (Strategy: Skip).", "SUP", declaration.Supplier.OH_FullName);
			AssertEquals("[Exception3]: Declarant should not be reverted (Strategy: Skip).", "DEC", declaration.Declarant.Header.OH_FullName);
		});

		CombineAssertions("Supplier, Importer and Declarant should be reverted when JobDeclarationDataObjectReader is not used for snapshot reverting.", () =>
		{
			reader.ReadIntoBusinessObject();

			AssertEquals("Importer should be reverted.", "MAT", declaration.Importer.OH_FullName);
			AssertEquals("Supplier should be reverted.", "CATE", declaration.Supplier.OH_FullName);
			AssertEquals("Declarant should be reverted.", "BOB", declaration.Declarant.Header.OH_FullName);
		});
	}

	static OrgHeader CreateOrganisation(UniversalObjectFactory factory, ZString name, ZString code)
	{
		var org = factory.New<OrgHeader>();
		org.OH_FullName = name;
		org.OH_Code = code;
		org.MainAddress.OA_Address1 = name + " ADDRESS 1";
		return org;
	}

	public void TestReadIntoBusinessObject_JEProperties_SnapshotRevertingWithStrategySkip()
	{
		(var declaration, _, var reader, var entryHeaderToRevert) = GetInfoForSnapshotRevertingTest();
		CombineAssertions("When JobDeclarationDataObjectReader is used for snapshot reverting and the strategy is skip, values for JobDeclaration should not be read into BO.", () =>
		{
			declaration.JE_RL_NKOrigin = "FRPAR";
			using (reader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(entryHeaderToRevert, Customs.Business.SnapshotRevertingStrategy.Skip, new TestErrorLogger()))
			{
				reader.ReadIntoBusinessObject();
			}
			AssertEquals("FRPAR", declaration.JE_RL_NKOrigin);

			AssertEquals("[Exception1]: JE_MessageType should not be reverted.", "IMP", declaration.JE_MessageType);
			AssertEquals("[Exception2]: JE_CustomsProfile should not be reverted.", "12345", declaration.JE_CustomsProfile);
		});
	}

	#endregion

	#region SubDataObjectReaders

	public void TestCreateNewCommercialInvoiceHeaderDataObjectReader()
	{
		(_, var shipmentDataObject, var reader, _) = GetInfoForSnapshotRevertingTest();
		AssertType<CommercialInvoiceHeaderDataObjectReader>(reader.CreateNewCommercialInvoiceHeaderDataObjectReader_Exposed(null, new CommercialInvoiceHeader(), shipmentDataObject, null));
	}

	public void TestCreateNewCustomsPackingLineDataObjectReader()
	{
		(var declaration, _, var reader, _) = GetInfoForSnapshotRevertingTest();
		AssertType<CustomsPackingLineDataObjectReader>(reader.CreateNewCustomsPackingLineDataObjectReader_Exposed(new PackingLine(), declaration, declaration.PrimaryHouseBill, true));
	}

	public void TestCreateNewCustomsSupportingInformationCollectionDataObjectReader()
	{
		(_, _, var reader, _) = GetInfoForSnapshotRevertingTest();
		AssertType<CustomsSupportingInformationCollectionDataObjectReader>(reader.CreateNewCustomsSupportingInformationCollectionDataObjectReader_Exposed());
	}

	#endregion

	public void TestBothModelViewAndBaseEUAddInfoAreRead_DefaultDisabled() => AssertBothModelViewAndBaseEUAddInfoAreRead(false);
	public void TestBothModelViewAndBaseEUAddInfoAreRead_DefaultEnabled() => AssertBothModelViewAndBaseEUAddInfoAreRead(true);
	void AssertBothModelViewAndBaseEUAddInfoAreRead(bool enableDefaulting)
	{
		using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableDefaulting))
		{
			var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });

			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo { Key = EUAddInfoSchema.Constants.ZG_VATDeferType.Substring(3), Value = "2" },
				new AddInfo { Key = FRJobDeclarationSchema.Constants.JE_AirRouteType.Substring(3), Value = "6" },
				new AddInfo { Key = FRJobDeclarationSchema.Constants.JE_TariffType.Substring(3), Value = "IMP" },
			});

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = (JobDeclaration)reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("VATDeferType", VATProcedureList.Codes._2, declarationBO.ZG_VATDeferType);
				AssertEquals("AirRouteType", AirRouteTypeList.Codes._6_CountryDOMDir, declarationBO.JE_AirRouteType);
				AssertEquals("TariffType", CusTariffTypes.ImportTariff, declarationBO.JE_TariffType);
			});
		}
	}

	public void TestApplicationCodeMappingWithNoDeclarationMatchedIfMessagingApplicationCodeIsNotInTheList()
	{
		var customsInterface = new LocalCountryCustomsInterface();
		customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;

		using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair()
				{
					Code = JobMessageTypeList.Codes.Export
				},
				MessagingApplicationCode = new CodeDescriptionPair()
				{
					Code = "DI"
				}
			};
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
				"*Bool=Y" +
				"*Code=Z1K" +
				"*Date=2020-11-25 17:36:48" +
				"*DateOnly=2019-01-31" +
				"*AnotherDecimal=873.2974" +
				"*Description=HI BOB" +
				"*Number=78234" +
				"*Short=845" +
				"*IsSystem=Y"));

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
			var manager = new UniversalMessageProcessingManager(xmlSessionTracker);

			var expectedMsg =
				"MessagingApplicationCode 'DI' is not a valid application code for FR Customs Declaration with message type 'EXP' based on registry 'Local Country Customs Interface' setting.";

			AssertExceptionThrown<MessageProcessingBusinessFailureException>("Should throw exception message", expectedMsg, () => manager.Process(message));

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(declaration);
		}
	}

	public void TestApplicationCodeMappingWithNoDeclarationMatchedIfMessagingApplicationCodeIsInTheList()
	{
		var customsInterface = new LocalCountryCustomsInterface();
		customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;

		using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair()
				{
					Code = JobMessageTypeList.Codes.Export
				},
				MessagingApplicationCode = new CodeDescriptionPair()
				{
					Code = "DI"
				}
			};
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
				"*Bool=Y" +
				"*Code=Z1K" +
				"*Date=2020-11-25 17:36:48" +
				"*DateOnly=2019-01-31" +
				"*AnotherDecimal=873.2974" +
				"*Description=HI BOB" +
				"*Number=78234" +
				"*Short=845" +
				"*IsSystem=Y"));

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
			var manager = new UniversalMessageProcessingManager(xmlSessionTracker);

			manager.Process(message);

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull(declaration);
			AssertEquals("DI", declaration.JE_ApplicationCode);
		}
	}

	(JobDeclaration, UniversalShipment, JobDeclarationDataObjectReader_ForTest, CusEntryHeader) GetInfoForSnapshotRevertingTest()
	{
		var declaration = GetJobDeclaration(Factory);
		var entryHeaderToRevert = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.InvoiceLines.Any(line => line.EntryInstruction.CEI_Style == "A"));
		var shipmentDataObject = CreateShipment(Factory, declaration.JE_DeclarationReference);
		var reader = new JobDeclarationDataObjectReader_ForTest(shipmentDataObject, new TestErrorLogger(), Factory);

		return (declaration, shipmentDataObject, reader, entryHeaderToRevert);
	}

	static internal UniversalShipment CreateShipment(UniversalObjectFactory factory, ZString? declarationReference = null)
	{
		var dataContext = DataContextFactory.New();
		dataContext.SetCompanyAndDataProviderDetails(factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		dataContext.CodesMappedToTarget = true;
		dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declarationReference);
		var commercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { CommercialInvoiceHeaderDataObjectReaderTest.GetCommercialHeader1(), CommercialInvoiceHeaderDataObjectReaderTest.GetCommercialHeader2() };
		commercialInvoiceCollection.Content = CollectionContent.Complete;
		var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
		{
			DataContext = dataContext,
			WayBillNumber = "MB123",
			WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			PortOfOrigin = new UNLOCO() { Code = "CNSHP", Name = "China Shanghai Port" },
			MessageType = new CodeDescriptionPair
			{
				Code = EUJobMessageTypeList.Codes.Export,
				Description = "Export"
			},
			CustomsProfileIdentifier = new ValueTypePair
			{
				Value = "56789"
			},
			CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = commercialInvoiceCollection
			},
		};
		shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { CustomsPackingLineDataObjectReaderTest.GetPackingLine1(), CustomsPackingLineDataObjectReaderTest.GetPackingLine2() });

		var importer = CreateOrganisation(factory, "MAT", "ABC!@#6");
		var supplier = CreateOrganisation(factory, "CATE", "ABC!@#11");
		var declantHeader = CreateOrganisation(factory, "BOB", "BOB!@#1");

		var writeManager = new DataWritingManager(new ActionInfo(null, factory.New<DummyBusinessObject>()));
		shipment.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
		shipment.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
		shipment.AddOrgAddress(writeManager, declantHeader.MainAddress, DocAddressType.Declarant);
		return shipment;
	}

	static internal JobDeclaration GetJobDeclaration(UniversalObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_HouseBill = "BILL1234";
		declaration.JE_CustomsProfile = "12345";
		var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
		var package1 = packingGroup.Packages.AddNew();
		package1.CW_PackType = "1A";
		package1.CW_PackQty = 10;
		var package2 = packingGroup.Packages.AddNew();
		package2.CW_PackType = "1B";
		package2.CW_PackQty = 15;

		var cei1 = declaration.CustomsEntryInstructions.AddNew();
		cei1.CEI_Style = "A";
		var cei2 = declaration.CustomsEntryInstructions.AddNew();
		cei2.CEI_Style = "F";

		var invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceHeader1.JZ_InvoiceNumber = "INV1";
		invoiceHeader1.JZ_InvoiceAmount = 0;
		var invoiceHeader2 = declaration.Invoices.AddNew();
		invoiceHeader2.JZ_InvoiceNumber = "INV2";
		invoiceHeader1.JZ_InvoiceAmount = 0;
		var invoiceLine11 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine11.JI_CEI = cei1.PK;
		invoiceLine11.JI_Tariff = "11111111";
		invoiceLine11.JI_MatchingKey = "MK11";
		var pivot11 = invoiceLine11.PackagesPivot.AddNew();
		pivot11.CHC_CW = package1.PK;
		pivot11.CHC_NumberOfPacks = 2;

		var invoiceLine12 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine12.JI_CEI = cei1.PK;
		invoiceLine12.JI_Tariff = "22222222";
		invoiceLine12.JI_MatchingKey = "MK12";
		var pivot12 = invoiceLine12.PackagesPivot.AddNew();
		pivot12.CHC_CW = package2.PK;
		pivot12.CHC_NumberOfPacks = 10;

		var invoiceLine21 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine21.JI_CEI = cei2.PK;
		invoiceLine21.JI_Tariff = "33333333";
		invoiceLine21.JI_MatchingKey = "MK21";
		var pivot21 = invoiceLine21.PackagesPivot.AddNew();
		pivot21.CHC_CW = package1.PK;
		pivot21.CHC_NumberOfPacks = 8;

		var invoiceLine22 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine22.JI_CEI = cei1.PK;
		invoiceLine22.JI_Tariff = "44444444";
		invoiceLine22.JI_MatchingKey = "MK22";
		var pivot22 = invoiceLine22.PackagesPivot.AddNew();
		pivot22.CHC_CW = package2.PK;
		pivot22.CHC_NumberOfPacks = 5;

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		factory.SaveForTesting();

		return declaration;
	}
}

class JobDeclarationDataObjectReader_ForTest : JobDeclarationDataObjectReader
{
	public JobDeclarationDataObjectReader_ForTest(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null) : base(declarationDataObject, logger, factory, forwardingShipment)
	{
	}

	public CommercialInvoiceHeaderDataObjectReader<EU.Business.Declaration.JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader_Exposed(EU.Business.Declaration.JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, UniversalShipment dataObject, ILandedCostDataReader landedCostDataReader) => base.CreateNewCommercialInvoiceHeaderDataObjectReader(groupHeader, invoiceData, dataObject, landedCostDataReader);

	public Customs.DataTransfer.Universal.CustomsPackingLineDataObjectReader CreateNewCustomsPackingLineDataObjectReader_Exposed(PackingLine packingLineDataObject, BaseJobDeclaration declaration, IColumnIndexer billRow, bool supportsParentPackage) => base.CreateNewCustomsPackingLineDataObjectReader(packingLineDataObject, declaration, billRow, supportsParentPackage);

	public Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader_Exposed() => base.CreateNewCustomsSupportingInformationCollectionDataObjectReader();
}
