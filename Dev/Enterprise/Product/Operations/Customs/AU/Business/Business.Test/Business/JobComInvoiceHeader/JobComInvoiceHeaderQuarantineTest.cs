using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	class JobComInvoiceHeaderQuarantineTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestJZ_ExporterReference()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			Assert(invoice.JZ_ExporterReferenceInfo.ReadOnly);
			AssertEquals(35, invoice.JZ_ExporterReferenceInfo.MaxLength);
		}

		public void TestNEXDOCExporterNumber()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TEST";
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, au, "99999");
			var invHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			invHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals("99999", invHeader.NEXDOCExporterNumber);
		}

		public void TestCusStorageDocPivotInterfaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc2 = (entry as IDocManagerSupport).DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc3 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment.pdf", "CIV");

			var pivotParent = (ICusStorageDocPivotParent)invoice;
			AssertNotNull(pivotParent.EDocPivotCollection);
			AssertEquals(3, pivotParent.EDocCollections.Count());
			Assert("ICusStorageDocPivotParent method", pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()) != null));
			Assert("ICusStorageDocPivotParent method", pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc2.UniqueKey.ToGuid()) != null));
			Assert("ICusStorageDocPivotParent method", pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc3.UniqueKey.ToGuid()) != null));

			var newPivot = pivotParent.EDocPivotCollection.AddNew();
			newPivot.CSD_DocType = "T1";
			Factory.Save();

			var loadedPivot = new BusinessObjectFactory().Load<BaseCusStorageDocPivot>(newPivot.PK);
			AssertType("ICusStorageDocPivotTypeSupporter method, should have returned the correct type", typeof(CusStorageDocPivot), loadedPivot);

			var fakeDeclaration = Factory.New<JobDeclaration>();
			fakeDeclaration.MakeNonPersistent();

			var standaloneInvoice = Factory.New<JobComInvoiceHeader>();
			standaloneInvoice.JZ_JE = fakeDeclaration.PK;

			var eDoc4 = standaloneInvoice.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

			pivotParent = standaloneInvoice;

			AssertNotNull(pivotParent.EDocPivotCollection);
			AssertEquals(1, pivotParent.EDocCollections.Count());
			Assert("ICusStorageDocPivotParent method", pivotParent.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc4.UniqueKey.ToGuid()) != null));
		}

		public void TestIDocAddresses()
		{
			var invHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var docAddresses = (IDocAddresses)invHeader;

			AssertNotNull(docAddresses.DocAddresses);
			AssertNull(docAddresses.GetCanOverrideCheckpoint(null));
			Assert(docAddresses.CanDeleteAddress(null));
			AssertNull(docAddresses.GetOrgHeaderList(DocAddressType.AQISResponsiblePerson));
			AssertNull(docAddresses.GetDocAddressRequirement(DocAddressType.AQISResponsiblePerson));
			AssertNotNull(docAddresses.PiggyBackedDocAddressValidation(Factory.New<JobDocAddress>()));
		}

		public new void TestSupportedAddressTypes()
		{
			var invHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				DocAddressType.AQISResponsiblePerson,
				DocAddressType.AQISTransitDestination,
				DocAddressType.AQISEUContactPerson,
				DocAddressType.AQISLoadingEstablishment,
			}, ((IDocAddresses)invHeader).SupportedAddressTypes);

			var declaration = invHeader.JobDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(0, ((IDocAddresses)invHeader).SupportedAddressTypes.Count);
		}

		[ExpectNoExceptions]
		public void TestAQISJobDelete()
		{
			var quarantineJob = ((JobComInvoiceHeader)GetNewBusinessObject()).JobDeclaration;
			var quarantineHeader = quarantineJob.QuarantineInvoice.QuarantineExDocHeader;// touch to set cached values
			quarantineJob.Invoices[0].Delete();
			quarantineJob.JE_ExportDate = ZDateTime.Now;
			AssertEquals("No headers, so delete did happen", 0, quarantineJob.Invoices.Count);

			quarantineJob = ((JobComInvoiceHeader)GetNewBusinessObject()).JobDeclaration;
			quarantineHeader = quarantineJob.QuarantineInvoice.QuarantineExDocHeader;
			var aQISResponsiblePerson = quarantineJob.QuarantineInvoice.AQISResponsiblePerson;
			var aQISTransitDestination = quarantineJob.QuarantineInvoice.AQISTransitDestination;
			AssertNotNull(quarantineHeader);
			AssertNotNull(aQISResponsiblePerson);
			AssertNotNull(aQISTransitDestination);
			var invoiceHeader = quarantineJob.Invoices[0];
			quarantineJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNotNull(quarantineHeader);
			AssertNotNull(aQISResponsiblePerson);
			AssertNotNull(aQISTransitDestination);
			invoiceHeader.Delete();
			var loadQuarantine = Factory.LoadTop1<QuarantineExDocHeader>(new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invoiceHeader.PK));
			AssertNull(loadQuarantine);
		}

		public void TestInvoiceLineLineNumberGenerator()
		{
			var invHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var dec = invHeader.JobDeclaration;

			dec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invLineNumGen = invHeader.InvoiceLineLineNumberGenerator;
			AssertNotNull(invLineNumGen);
			AssertEquals(typeof(QuarantineSequenceNumberGenerator), invLineNumGen.GetType());

			dec.JE_MessageType = ZString.Empty; // Anything else, nothing in particular
			invLineNumGen = invHeader.InvoiceLineLineNumberGenerator;
			AssertNotNull(invLineNumGen);
			AssertEquals(typeof(ShortSequenceNumberGenerator), invLineNumGen.GetType());
		}

		public void TestInvoiceARPATDAddress()
		{
			var invHeader = (JobComInvoiceHeader)GetNewBusinessObject();
			var aQISResponsiblePerson = invHeader.AQISResponsiblePerson;
			var aQISTransitDestination = invHeader.AQISTransitDestination;
			var aQISEUContactPerson = invHeader.AQISEUContactPerson;
			CombineAssertions(() =>
			{
				AssertEquals(invHeader.PK, aQISResponsiblePerson.E2_ParentID);
				AssertEquals("Should be JZ", invHeader.TablePrefix, aQISResponsiblePerson.E2_ParentTableCode);
				AssertEquals("Should be ARP", DocAddressTypes.Codes.AQISResponsiblePerson, aQISResponsiblePerson.E2_AddressType);
				AssertEquals(invHeader.PK, aQISTransitDestination.E2_ParentID);
				AssertEquals("Should be JZ", invHeader.TablePrefix, aQISTransitDestination.E2_ParentTableCode);
				AssertEquals("Should be ATD", DocAddressTypes.Codes.AQISTransitDestination, aQISTransitDestination.E2_AddressType);
				AssertEquals(invHeader.PK, aQISEUContactPerson.E2_ParentID);
				AssertEquals("Should be JZ", invHeader.TablePrefix, aQISEUContactPerson.E2_ParentTableCode);
				AssertEquals("Should be AEC", DocAddressTypes.Codes.AQISEUContactPerson, aQISEUContactPerson.E2_AddressType);
			});
		}

		public void TestSyncEDNWhenAttachedToDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			var invoice1 = Factory.New<JobComInvoiceHeader>();
			invoice1.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			invoice1.AddInfo.ZA_EDN_Hidden = "TST0000";

			AssertEquals("Precondition.", string.Empty, declaration.DeclarationNumber);

			invoice1.JZ_JE = declaration.PK;
			AssertEquals("Should sync EDN from invoice header to declaration.", "TST0000", declaration.DeclarationNumber);

			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.AddInfo.ZA_EDN_Hidden = "TST0001";

			invoice2.JZ_JE = declaration.PK;
			AssertEquals("Should not sync EDN as the declaration has a valid DeclarationNumber.", "TST0000", declaration.DeclarationNumber);

			declaration.DeclarationNumber = string.Empty;
			declaration.MakeNonPersistent();

			var invoice3 = Factory.New<JobComInvoiceHeader>();
			invoice3.AddInfo.ZA_EDN_Hidden = "TST0002";

			AssertEquals("Should not sync EDN as the declaration is not persistent.", string.Empty, declaration.DeclarationNumber);
		}

		public void TestSyncContainersWhenAttachedToPersistentDeclaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line1.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304711";
			line1.AddInfo.ZA_AQISTempContainerSeal_Hidden = "SL12345";
			var line2 = invoice.InvoiceLines.AddNew();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			invoice.JZ_JE = declaration.PK;
			var container = declaration.AUCusContainers[0];
			AssertEquals("Should sync Container Number from invoice line AddInfo to declaration.", "MAEU9304711", container.CO_ContainerNumber);
			AssertEquals("Should sync Container Seal from invoice line AddInfo to declaration.", "SL12345", container.SealNumberForBinding);
			AssertSame("Should create Container on invoice line 1.", container, line1.ContainersPivot[0].Container);
			AssertEquals("Container links to line 1", true, line1.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container).IsForInvoiceLine);
			AssertEquals("Container does not link to line 2", false, line2.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container).IsForInvoiceLine);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedInvoice = otherFactory.Load<JobComInvoiceHeader>(invoice.PK);
			var loadedDec = loadedInvoice.JobDeclaration;
			var loadedLine = (JobComInvoiceLine)loadedInvoice.InvoiceLines[0];
			AssertSame("Should save Container on invoice line 1.", loadedDec.AUCusContainers[0], loadedLine.ContainersPivot[0].Container);
			AssertEquals("Should save Container Number.", "MAEU9304711", loadedDec.AUCusContainers[0].CO_ContainerNumber);
			AssertEquals("Should clear the Container Number from AddInfo.", ZString.Empty, loadedLine.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("Should clear the Container Seal from AddInfo.", ZString.Empty, loadedLine.AddInfo.ZA_AQISTempContainerSeal_Hidden);
		}

		public void TestSyncContainersWhenAttachedToNonPersistentDeclaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line1.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304711";
			line1.AddInfo.ZA_AQISTempContainerSeal_Hidden = "SL12345";
			var line2 = invoice.InvoiceLines.AddNew();

			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			AssertEquals(JobMessageTypeList.Codes.Quarantine, declaration.JE_MessageType);

			var container = declaration.AUCusContainers[0];
			AssertSame("Should create Container on invoice line 1.", container, line1.ContainersPivot[0].Container);
			AssertEquals("Should sync Container Number from invoice line AddInfo to declaration.", "MAEU9304711", container.CO_ContainerNumber);
			AssertEquals("Should sync Container Seal from invoice line AddInfo to declaration.", "SL12345", container.SealNumberForBinding);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedInvoice = otherFactory.Load<JobComInvoiceHeader>(invoice.PK);
			var loadedDec = loadedInvoice.JobDeclaration;
			var loadedLine = (JobComInvoiceLine)loadedInvoice.InvoiceLines[0];
			AssertNull("Declaration is not persisted", loadedDec);
			AssertEquals("Should not save Container pivot.", 0, loadedLine.ContainersPivot.Count);
			AssertEquals("Should not clear the Container Number from AddInfo.", "MAEU9304711", loadedLine.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("Should not clear the Container Seal from AddInfo.", "SL12345", loadedLine.AddInfo.ZA_AQISTempContainerSeal_Hidden);
		}

		public void TestSyncQuarantineExDocEstablishmentWhenAttachedToPersistentDeclaration()
		{
			var aqisEstablishment = Factory.NewWithValidTestData<OrgHeader>();
			aqisEstablishment.OH_Code = "TAQS";
			aqisEstablishment.OH_FullName = "AQIS SYDNEY";
			var mainAddress = aqisEstablishment.MainAddress;
			mainAddress.Address1 = "185 O'RIORDAN ST";
			mainAddress.City = "MASCOT";
			mainAddress.State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "2020";
			var esnCusCode = mainAddress.CustomsCodes.AddNew();
			esnCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			esnCusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			esnCusCode.OK_CustomsRegNo = "77";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineLine = line1.QuarantineExDocLine;
			var processBO = quarantineLine.Processes.AddNew();
			processBO.EE_AuthorisationEstablishmentID = "77";
			var line2 = invoice.InvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, processBO.EE_E2_Address);

			invoice.JZ_JE = declaration.PK;
			AssertNotEquals(ZGuid.Empty, processBO.EE_E2_Address);
			AssertEquals("Creates a JobDocAddress that maps to the AQIS Establishment for code 77", aqisEstablishment.PK, processBO.Address.OrganisationPK);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedInvoice = otherFactory.Load<JobComInvoiceHeader>(invoice.PK);
			var loadedLine = (JobComInvoiceLine)loadedInvoice.InvoiceLines[0];
			var loadedProcess = loadedLine.QuarantineExDocLine.Processes[0];

			AssertEquals("77", loadedProcess.EE_AuthorisationEstablishmentID);
			AssertEquals("DocAddress did not change", processBO.EE_E2_Address, loadedProcess.EE_E2_Address);
			AssertEquals("Stored JobDocAddress maps to the AQIS Establishment for code 77", aqisEstablishment.PK, loadedProcess.Address.OrganisationPK);
			AssertEquals(false, loadedProcess.HasChanges);
		}

		public void TestSyncQuarantineExDocEstablishmentWhenAttachedToNonPersistentDeclaration()
		{
			var aqisEstablishment = Factory.NewWithValidTestData<OrgHeader>();
			aqisEstablishment.OH_Code = "TAQS";
			aqisEstablishment.OH_FullName = "AQIS SYDNEY";
			var mainAddress = aqisEstablishment.MainAddress;
			mainAddress.Address1 = "185 O'RIORDAN ST";
			mainAddress.City = "MASCOT";
			mainAddress.State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "2020";
			var esnCusCode = mainAddress.CustomsCodes.AddNew();
			esnCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			esnCusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			esnCusCode.OK_CustomsRegNo = "77";

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineLine = line1.QuarantineExDocLine;
			var processBO = quarantineLine.Processes.AddNew();
			processBO.EE_AuthorisationEstablishmentID = "77";
			var line2 = invoice.InvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, processBO.EE_E2_Address);

			var fakeDec = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			AssertEquals(JobMessageTypeList.Codes.Quarantine, fakeDec.JE_MessageType);
			AssertNotEquals(ZGuid.Empty, processBO.EE_E2_Address);
			AssertEquals("Creates a JobDocAddress that maps to the AQIS Establishment for code 77", aqisEstablishment.PK, processBO.Address.OrganisationPK);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedInvoice = otherFactory.Load<JobComInvoiceHeader>(invoice.PK);
			AssertNull(loadedInvoice.JobDeclaration);

			var fakeDec2 = (JobDeclaration)new FakeDeclarationCreatorForInvoice(loadedInvoice).HeaderData;
			AssertEquals(JobMessageTypeList.Codes.Quarantine, fakeDec2.JE_MessageType);
			var loadedLine = (JobComInvoiceLine)loadedInvoice.InvoiceLines[0];
			var loadedProcess = loadedLine.QuarantineExDocLine.Processes[0];
			AssertEquals("77", loadedProcess.EE_AuthorisationEstablishmentID);
			AssertNotEquals("DocAddress changes with each fake dec", processBO.EE_E2_Address, loadedProcess.EE_E2_Address);
			AssertEquals("Creates a new JobDocAddress that maps to the AQIS Establishment for code 77", aqisEstablishment.PK, loadedProcess.Address.OrganisationPK);
			AssertEquals(false, loadedProcess.HasChanges);
		}

		public void TestQuarantineHeaderIsSingularAcrossFactories()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInOtherFactory = factory2.Load<JobDeclaration>(declaration.PK);

			AssertNotNull("quarantineHeaderIn1", declaration.QuarantineInvoice.QuarantineExDocHeader);
			AssertNotNull("quarantineHeaderIn2", decInOtherFactory.QuarantineInvoice.QuarantineExDocHeader);

			Factory.Save();
			factory2.Save();

			var quarantineHeadersQuery = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invoice.PK);
			var headers = Factory.Load<QuarantineExDocHeader>(quarantineHeadersQuery);
			AssertEquals(1, headers.Length);
		}

		public void TestQuarantineHeaderIsSingular_ExistingInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInOtherFactory = factory2.Load<JobDeclaration>(declaration.PK);

			AssertNotNull("quarantineHeaderIn1", declaration.QuarantineInvoice.QuarantineExDocHeader);
			AssertNotNull("quarantineHeaderIn2", decInOtherFactory.QuarantineInvoice.QuarantineExDocHeader);

			Factory.Save();
			factory2.Save();

			var quarantineHeadersQuery = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invoice.PK);
			var headers = Factory.Load<QuarantineExDocHeader>(quarantineHeadersQuery);
			AssertEquals(1, headers.Length);
		}

		public void TestQuarantineHeaderIsSingular_ExistingInvoiceConcurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInOtherFactory = factory2.Load<JobDeclaration>(declaration.PK);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertNotNull("quarantineHeaderIn1", declaration.QuarantineInvoice.QuarantineExDocHeader);

			decInOtherFactory.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertNotNull("quarantineHeaderIn2", decInOtherFactory.QuarantineInvoice.QuarantineExDocHeader);

			Factory.Save();

			var concurrencyException = AssertExceptionThrown<ZSaveConcurrencyException>("Reports Concurrency Error", () => factory2.Save());
			var prop = factory2.GetChanges().GetChangedObjects()[0].NonMergeableProperties[0];
			AssertEquals("Message Type is non-mergeable preventing save.  Changes are lost.", nameof(JobDeclaration.JE_MessageType), prop.ColumnName);

			var quarantineHeadersQuery = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invoice.PK);
			var headers = Factory.Load<QuarantineExDocHeader>(quarantineHeadersQuery);
			AssertEquals(1, headers.Length);
		}

		public void TestQuarantineHeaderIsSingular_CommercialInvoice()
		{
			var commercialInvoice = Factory.New<JobComInvoiceHeader>();
			commercialInvoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			commercialInvoice.JZ_InvoiceNumber = "Com001";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Factory.Save();
			Factory.RefreshEnabled = false;
			commercialInvoice.JZ_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.Invoices.Add(commercialInvoice);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInOtherFactory = factory2.Load<JobDeclaration>(declaration.PK);
			var invoiceInOtherFactory = factory2.Load<JobComInvoiceHeader>(commercialInvoice.PK);
			invoiceInOtherFactory.JZ_MessageType = JobMessageTypeList.Codes.Quarantine;
			decInOtherFactory.Invoices.Add(invoiceInOtherFactory);

			AssertNotNull("quarantineHeaderIn1", declaration.QuarantineInvoice.QuarantineExDocHeader);
			AssertNotNull("quarantineHeaderIn2", decInOtherFactory.QuarantineInvoice.QuarantineExDocHeader);

			Factory.Save();
			var concurrencyException = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory2.Save());
			AssertContains("DB Changed", concurrencyException.Message);

			var quarantineHeadersQuery = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, commercialInvoice.PK);
			var headers = Factory.Load<QuarantineExDocHeader>(quarantineHeadersQuery);
			AssertEquals(1, headers.Length);
		}

		public void TestQuarantineHeaderIsSingular_MultipleInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			AssertNotNull("Declaration has a Quarantine Header", declaration.QuarantineInvoice.QuarantineExDocHeader);

			var quarantineHeadersQuery = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, declaration.Invoices.Select(x => x.PK));
			var quarantineHeaders = Factory.Load<QuarantineExDocHeader>(quarantineHeadersQuery);
			AssertEquals("There is one Quarantine Header per invoice", 3, quarantineHeaders.Length);
		}

		protected override void AfterInitialise(BaseJobDeclaration declaration)
		{
			foreach (JobComInvoiceHeader inv in declaration.Invoices)
			{
				QuarantineExDocHeader x = inv.QuarantineExDocHeader;
			}
			foreach (JobComInvoiceLine invLine in declaration.InvoiceLines)
			{
				QuarantineExDocLine y = invLine.QuarantineExDocLine;
			}
		}

		public override void TestLocalCurrencyCodeCoreOverride()
		{
			Assert(true);
		}

		protected override IEnumerable<string> MessageTypesForDefaultCurrencyToLocalCurrency(BaseJobDeclaration declaration)
		{
			return base.MessageTypesForDefaultCurrencyToLocalCurrency(declaration).Except(AUJobMessageTypeList.Codes.ExWarehouse);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			return groupHeader.JobComInvoiceHeaders.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobComInvoiceHeader header = (JobComInvoiceHeader)GetNewBusinessObject();
			QuarantineExDocHeader quarantineExDocHeader = header.QuarantineExDocHeader;
			return header;
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionedCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		#endregion
	}
}
