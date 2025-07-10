using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineTestGeneral : JobComInvoiceLineTest
	{
		public void TestTypeOfApportionedCharges()
		{
			AssertType<JobComInvApportionedChargeCollection<InvoiceLineApportionedCharge>>("ApportionedCharges' type", Factory.New<JobComInvoiceLine>().ApportionedCharges);
		}

		public void TestCustomsCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Afghanistan))
			{
				AssertEquals("CustomsCountryCodeCore should be AU", Core.Constants.CountryCodes.Australia, Factory.New<JobComInvoiceLine>().CustomsCountryCode);
			}
		}

		[ExpectNoExceptions]
		public void TestJI_NoPermitRequiredNullReference()
		{
			AssertNotNull(Factory.New<JobComInvoiceLine>().JI_NoPermitRequired);
		}

		public void TestASNRefresh()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);
			var config1 = collection.AddNew();
			config1.FieldType = DefaultOptions.Codes.Classification;
			var config2 = collection.AddNew();
			config2.FieldType = DefaultOptions.Codes.CountryOfOrigin;

			CustomsDataRegistry.Instance.ASNRefreshOptions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var invoice = Factory.New<JobComInvoiceHeader>();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPTEST";
			invoice.JZ_OH_Buyer = importer.PK;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPTEST";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_StandAloneInvoiceDirection = Common.Shared.SharedJobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = Common.Shared.SharedJobMessageTypeList.MoreCodes.AdvanceShippingNotice;

			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var classification = Factory.New<BaseCusClassification>();
			classification.FillWithValidTestData();

			var product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "AMYTEST";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_TariffNum = "1010101010";
			pivot.AddInfo.ZA_ORG = "CA";
			pivot.AddInfo.ZA_PST = "SPI";
			var relatedOrg1 = product.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = importer.PK;
			relatedOrg1.OU_Relationship = "OWN";
			var relatedOrg2 = product.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = supplier.PK;
			relatedOrg2.OU_Relationship = "SUP";

			line.JI_OP = product.PK;
			line.JI_PartNo = product.OP_PartNum;
			AssertEquals(pivot, line.Pivot);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the registry", pivot.CI_CC, line.JI_CC);
			AssertEquals("Refreshed by the registry", pivot.AddInfo.ZA_ORG, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.JI_Tariff);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.AddInfo.ZA_PST);

			var companyData = importer.CompanyData;
			companyData.ImporterOverride = true;
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,PREFF";

			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_MessageType = Common.Shared.SharedJobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = Common.Shared.SharedJobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			line.JI_CC = ZGuid.Empty;
			line.JI_CountryOfOrigin = ZString.Empty;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the consignee config", ZGuid.Empty, line.JI_CC);
			AssertEquals("Refreshed by the consignee config", ZString.Empty, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the consignee config", pivot.CI_TariffNum, line.JI_Tariff);
			AssertEquals("Refreshed by the consignee config", pivot.AddInfo.ZA_PST, line.AddInfo.ZA_PST);
		}

		public void TestTariffFormatter()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			new FakeDeclarationCreatorForInvoice(invoice);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";

			AssertEquals("It should have been formatted by AUImportTariffUniversalFormatter and retain all the 10 digits", "0000.00.00 00", invoiceLine.JI_FormattedTariff);
		}

		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var lookups = invoiceLine.Lookups;
			AssertEquals(typeof(EXDOCSJobComInvoiceLineLookups), lookups.GetType());
			AssertSame("The lookups is cached.", invoiceLine.Lookups, invoiceLine.Lookups);

			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			lookups = invoiceLine.Lookups;
			AssertEquals(typeof(NEXDOCSJobComInvoiceLineLookups), lookups.GetType());
			AssertSame("The lookups is cached.", lookups, invoiceLine.Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			lookups = invoiceLine.Lookups;
			AssertEquals(typeof(JobComInvoiceLineLookups), lookups.GetType());
			AssertSame("The lookups is cached.", lookups, invoiceLine.Lookups);
		}

		[ExpectNoExceptions]
		public void TestDeletingAFreeStandingLine()
		{
			var line = Factory.New<JobComInvoiceLine>();
			Assert("Should be able to delete it", ((ICanDelete)line).CanDelete);
			line.Delete();
		}

		[ExpectNoExceptions]
		public void TestDeletingDrawbackLineWithoutBOMChildren()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			Assert("Should be able to delete it", ((ICanDelete)Line).CanDelete);
			Line.Delete();
		}

		[ExpectNoExceptions]
		public void TestDeletingDrawbackLineWithBOMChildren()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			Line.IsBOMLineExpanded = true;
			Assert("Should not be able to delete it", !((ICanDelete)Line).CanDelete);
			Line.Delete();
		}

		[ExpectNoExceptions]
		public void TestDeletingQuarantineExDocLine_WI00641404()
		{
			JobDec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var quarantineExDocLine = Factory.New<QuarantineExDocLine>();
			quarantineExDocLine.QL_JI = Line.PK;
			Factory.Save();
			Assert(JobDec.QuarantineInvoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().QuarantineExDocLine != null);

			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Factory.Save();
			Assert(JobDec.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().QuarantineExDocLine == null);
		}

		public void TestDefaultingAQISInfoFromProduct()
		{
			JobDec.JE_OH_Importer = Consignee.PK;
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			Header.JZ_OH_Supplier = Consignee.PK;

			var factory2 = new BusinessObjectFactory();

			var importTariff = factory2.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "5407.72.00 20");
			var importClass = factory2.New<Classification>();
			importClass.CC_ClassificationType = ClassificationType.IMP;
			importClass.CC_Description = "Description";
			importClass.CC_TariffNum = importTariff.UJ_Code;
			importClass.CC_LookupCode = "Test";

			var newPart = factory2.New<AUOrgSupplierPart>();
			var pivot1 = newPart.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = importClass.PK;
			pivot1.AddInfo.ZA_TreatmentCode_Hidden = "110";
			newPart.OP_PartNum = "PartPartPart1";

			factory2.Save();

			var document = pivot1.AQISDocuments.AddNew();
			document.Type = "Type";
			document.Number = "Number";

			var premProc = pivot1.AQISPremisesIdAndProcessingTypes.AddNew();
			premProc.PremisesId = "Prem";
			premProc.ProcessingType = "Proc";

			pivot1.AddInfo.ZA_AQISPermitIds_Hidden = "Permit";
			pivot1.AddInfo.ZA_AQISProducerCodes_Hidden = "Prod";
			pivot1.AddInfo.ZA_AQISCommCodes_Hidden = "Com";
			pivot1.AddInfo.ZA_AQISEntityIds_Hidden = "Ent";

			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = Header.JZ_OH_Supplier;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			factory2.Save();

			Line.JI_PartNo = newPart.OP_PartNum;
			AssertEquals("Line should have an aqis documents", 1, Line.AQISDocuments.Count);
			AssertEquals("AQIS Document Type", "Type", Line.AQISDocuments[0].Type);
			AssertEquals("AQIS Document Number", "Number", Line.AQISDocuments[0].Number);

			AssertEquals("Line should have an AQISPremisesIdAndProcessingType", 1, Line.AQISPremisesIdAndProcessingTypes.Count);
			AssertEquals("AQIS Premises Id", "Prem", Line.AQISPremisesIdAndProcessingTypes[0].PremisesId);
			AssertEquals("AQIS Processing Type", "Proc", Line.AQISPremisesIdAndProcessingTypes[0].ProcessingType);

			AssertEquals("Permit Ids", "Permit", Line.AddInfo.ZA_AQISPermitIds_Hidden);
			AssertEquals("Producer Codes", "Prod", Line.AddInfo.ZA_AQISProducerCodes_Hidden);
			AssertEquals("Comm Codes", "Com", Line.AddInfo.ZA_AQISCommCodes_Hidden);
			AssertEquals("Entity Ids", "Ent", Line.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestDefaultingICSPermitInfoFromProduct()
		{
			JobDec.JE_OH_Importer = Consignee.PK;
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			Header.JZ_OH_Supplier = Consignee.PK;

			var factory2 = new BusinessObjectFactory();

			var importTariff = factory2.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "5407.72.00 20");
			var importClass = factory2.New<Classification>();
			importClass.CC_ClassificationType = ClassificationType.IMP;
			importClass.CC_Description = "Description";
			importClass.CC_TariffNum = importTariff.UJ_Code;
			importClass.CC_LookupCode = "Test";

			var newPart = factory2.New<AUOrgSupplierPart>();
			var pivot1 = newPart.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = importClass.PK;
			pivot1.AddInfo.ZA_WAR = "1255B";
			pivot1.ICSPermits.AddNew().CY_Data = "PERT3";
			pivot1.ICSPermits.AddNew().CY_Data = "PERT2";
			pivot1.ICSPermits.AddNew().CY_Data = "PERT1";
			newPart.OP_PartNum = "PartPartPart1";

			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = Header.JZ_OH_Supplier;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			factory2.Save();

			Line.JI_PartNo = newPart.OP_PartNum;
			AssertEquals("ICSPermits", 3, Line.ICSPermits.Count);
			AssertEquals("PERT1, PERT2, PERT3", Line.ICSPermits.GetSortedPermitNumbers());
			AssertEquals("ZA_WAR", "1255B", Line.AddInfo.ZA_WAR);
		}

		public void TestJI_ZA_WRU_List()
		{
			AssertEquals(Line.JI_ZA_WRU_List.Count, Line.UnitConverter.ConvertibleUQs.Count);
		}

		public void TestStateAndOriginDefaultedFromExporter()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.FillWithValidTestData();

			var orgFromAU = CreateOrgFromCountry("AUSYD");
			orgFromAU.MainAddress.OA_State = "NSW";
			var orgFromNZ = CreateOrgFromCountry("NZAKL");

			dec.JE_OH_Supplier = orgFromAU.PK;
			dec.JE_OH_Forwarder = orgFromNZ.PK;

			var header = dec.Invoices.AddNew();

			var line = header.JobComInvoiceLines.AddNew();

			AssertEquals("AU", line.JI_CountryOfOrigin);
			AssertEquals("NSW", line.JI_AUState);
		}

		OrgHeader CreateOrgFromCountry(string portCode)
		{
			var result = Factory.New<OrgHeader>();
			result.FillWithValidTestData();
			result.OH_IsConsignor = true;
			result.OH_IsConsignee = true;
			result.OH_IsForwarder = true;
			result.OH_RL_NKClosestPort = portCode;
			return result;
		}

		public void TestPartInformationDoesntGetReLoadedAfterSavingAndReloadingDeclarationIfUserOverridesValuesSetFromPart()
		{
			ZString treatmentCode1 = ImportClass.TreatmentCode_List[0].Code;
			ZString treatmentCode2 = ImportClass.TreatmentCode_List[1].Code;
			Assert("TreatmentCode1 != TreatmentCode2", treatmentCode1 != treatmentCode2);

			ImportClass.TreatmentCode = treatmentCode1;
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			pivot1.AddInfo.ZA_TreatmentCode_Hidden = treatmentCode1;

			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Precondition: Line.Part.OP_PartNum", Part.OP_PartNum, Line.Part.OP_PartNum);
			AssertEquals("Precondition: Line.JI_CC", ImportClass.PK, Line.JI_CC);
			AssertEquals("Precondition: Line.TreatmentCode", treatmentCode1, Line.TreatmentCode);

			Line.AddInfo.ZA_TreatmentCode_Hidden = treatmentCode2;
			JobDec.RunPreSaveValidation();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<JobDeclaration>(JobDec.PK);
			var invoiceLine2 = declaration2.FilteredInvoiceLines[0];
			invoiceLine2.JI_Description = "New Description";
			AssertEquals("InvoiceLine2.AddInfo.ZA_TreatmentCode_Hidden", treatmentCode2, invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden);
			declaration2.RunPreSaveValidation();
			AssertEquals("InvoiceLine2.AddInfo.ZA_TreatmentCode_Hidden", treatmentCode2, invoiceLine2.AddInfo.ZA_TreatmentCode_Hidden);
		}

		public void TestIsIsGoingIntoBondedWarehouse()
		{
			Line.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			AssertEquals("IsBond", true, Line.IsGoingIntoBondedWarehouse);
			Line.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			AssertEquals("IsBond", false, Line.IsGoingIntoBondedWarehouse);
		}

		public void TestJI_CCAccordingToClassificationManagerInPartSyncManager()
		{
			var pivot = Part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = ImportClass.PK;

			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			Header.JZ_JE = testDec.PK;
			AssertEquals("Line's declaration", testDec, Line.Declaration);

			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part from line", Part, Line.Part);
			AssertEquals("JI_CC is set", ImportClass.PK, Line.JI_CC);
		}

		public void TestJI_CountryOfOriginDoesntLoseItsValue()
		{
			//this triggers the setter of AddInfo.ZA_Org -> Setter of JI_AddInfo, which set the JI_CountryOfOrigin
			Line.JI_CountryOfOrigin = "AU";
			Assert("Origin stays there", !Line.JI_CountryOfOrigin.IsEmpty);
		}

		public void TestInvoiceUQValidation()
		{
			Line.JI_InvoiceUQ = "XX";
			Assert("The UQ entered is invalid", Line.JI_InvoiceUQInfo.HasNotifications());

			Line.JI_InvoiceUQ = "KG";
			Assert("Valid UQ", !Line.JI_InvoiceUQInfo.HasNotifications());
		}

		public void TestMultipleAUStates()
		{
			Line.JI_AddInfo = "AU=1.2345*PT=6.7890*ZN=2.3456*AUState_Hidden=NSW,ACT*PermitNumbers_Hidden=A123";
			AssertEquals("AU state", 2, Line.AUStateCodeCollection.Count);
			Assert("AU state Hidden", Line.AddInfo.ZA_AUState_Hidden == "NSW,ACT");
			Assert("AU state", Line.JI_AUState == "MULT");
		}

		public void TestGetValueFromAddInfoLine()
		{
			Line.JI_AddInfo = "AU=1.2345*PT=6.7890*ZN=2.3456*AUState_Hidden=NSW*PermitNumbers_Hidden=A123";
			Assert("AU state", Line.JI_AUState == "NSW");
			Assert("Permit Number", Line.AddInfo.ZA_PermitNumbers_Hidden == "A123");
		}

		public void TestNRInCustomsUnitQuantityDoesntResultInMessageErrorInCustomsQuantity()
		{
			Line.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			Line.JI_Tariff = "3922.90.03";//NR Not required
			Line.RunPreSaveValidation();
			AssertEquals("PreCondition:Quantity is not needed", false, Line.NeedsCustomsQuantity);
			AssertEquals("Customs Quantity", false, Line.JI_CustomsQuantityInfo.HasMessageErrors());
			AssertEquals("Customs Unit Quantity", false, Line.JI_CustomsUnitQtyInfo.HasMessageErrors());
		}

		public void TestAddInfoPermitNumber()
		{
			Assert("Add Info", Line.AddInfo.ZA_PermitNumbers_Hidden == "");

			Line.AddInfo.ZA_PermitNumbers_Hidden = "A123";
			Assert("Permit number in addinfo string", Line.JI_AddInfo.IndexOf("PermitNumbers_Hidden=A123") >= 0);
			Line.AddInfo.ZA_PermitNumbers_Hidden = "";
			Assert("Permit number in addinfo string", Line.JI_AddInfo.IndexOf("PermitNumbers_Hidden") < 0);
		}

		public void TestPermitNumbers()
		{
			Line.AddInfo.ZA_PermitNumbers_Hidden = "A123:456";
			var permitNumbers = Line.Permits;
			AssertEquals(1, permitNumbers.Count());
			AssertEquals("A123", permitNumbers.First());
		}

		public void TestInvalidPermitNumbers()
		{
			Line.AddInfo.ZA_PermitNumbers_Hidden = "A123:";
			var permitNumbers = Line.Permits;
			AssertEquals(1, permitNumbers.Count());
			AssertEquals("A123", permitNumbers.First());

			Line.AddInfo.ZA_PermitNumbers_Hidden = ":";
			permitNumbers = Line.Permits;
			AssertEquals(1, permitNumbers.Count());
			AssertEquals("", permitNumbers.First());
		}

		public void TestInvalidEncryptionNumbers()
		{
			Line.AddInfo.ZA_PermitNumbers_Hidden = ":";
			var encryptionNumbers = Line.EncryptionNumbers;
			AssertEquals(1, encryptionNumbers.Count());
			AssertEquals("", encryptionNumbers.First());

			Line.AddInfo.ZA_PermitNumbers_Hidden = ":456";
			encryptionNumbers = Line.EncryptionNumbers;
			AssertEquals(1, encryptionNumbers.Count());
			AssertEquals("456", encryptionNumbers.First());
		}

		public void TestPermitNumberValidation()
		{
			var addInfo = Line.AddInfo;
			Assert(!addInfo.ZA_PermitNumbers_HiddenInfo.HasErrors());
			addInfo.ZA_PermitNumbers_Hidden = "";
			Assert("Empty Permit number", !addInfo.ZA_PermitNumbers_HiddenInfo.HasMessageErrors());
			addInfo.ZA_PermitNumbers_Hidden = "PIH4025240";
			Assert("Valid Permit number", !addInfo.ZA_PermitNumbers_HiddenInfo.HasMessageErrors());
			addInfo.ZA_PermitNumbers_Hidden = "PIH4025240:123";
			Assert("Valid Permit number With Encyrption", !Line.HasErrors);
			addInfo.ZA_PermitNumbers_Hidden = "PIH4025240::";
			Assert("Two Colons", addInfo.ZA_PermitNumbers_HiddenInfo.HasMessageErrors());
			addInfo.ZA_PermitNumbers_Hidden = "::PIH4025240";
			Assert("Leading Two Colons", addInfo.ZA_PermitNumbers_HiddenInfo.HasMessageErrors());
			addInfo.ZA_PermitNumbers_Hidden = "PIH4025240,,";
			Assert("Two Commas", addInfo.ZA_PermitNumbers_HiddenInfo.HasMessageErrors());
		}

		public void TestAddInfoWithImportNumber()
		{
			Line.JI_TempImportNum = "B123";
			Assert("Import Number in addinfo string", Line.AddInfo.ZA_TemporaryImportNumbers_Hidden == Line.JI_TempImportNum);
			Line.AddInfo.ZA_PermitNumbers_Hidden = "A123";

			Assert("Add Info", Line.AddInfo.ZA_TemporaryImportNumbers_Hidden == "B123");
			Assert("AddInfo", Line.AddInfo.ZA_PermitNumbers_Hidden == "A123");
			Line.JI_TempImportNum = "";
			Assert("Import Number in addinfo string", Line.AddInfo.ZA_TemporaryImportNumbers_Hidden == "");
		}

		public void TestImportNumberAndDateBlankAndReadOnlyWhenNoPermitRequired_NEXDOC()
		{
			JobDec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			Line.InvoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("NEXDOC active", Line.InvoiceHeader.QuarantineExDocHeader.IsNEXDOCSActive);
			Line.JI_TempImportNum = "B123";
			Line.JI_TempImportDate = ZDate.Today.AddDays(-5);
			Assert("Import Number in addinfo string", Line.AddInfo.ZA_TemporaryImportNumbers_Hidden == Line.JI_TempImportNum);

			Line.JI_NoPermitRequired = true;
			AssertEquals("JI_TempImportNum", JobComInvoiceLine.CANConstants.NoPermitRequired, Line.JI_TempImportNum);
			AssertEquals("JI_TempImportDate", ZDate.Empty, Line.JI_TempImportDate);
			AssertEquals("JI_TempImportNum - ReadOnly", true, Line.JI_TempImportNumInfo.ReadOnly);
			AssertEquals("JI_TempImportDate - ReadOnly", true, Line.JI_TempImportNumInfo.ReadOnly);

			Line.JI_NoPermitRequired = false;
			AssertEquals("JI_TempImportNum - ReadOnly", false, Line.JI_TempImportNumInfo.ReadOnly);
			AssertEquals("JI_TempImportDate - ReadOnly", false, Line.JI_TempImportNumInfo.ReadOnly);
		}

		public void TestImportNumberAndDateEditable_EXDOC()
		{
			JobDec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			Assert("EXDOC", !Line.InvoiceHeader.QuarantineExDocHeader.IsNEXDOCSActive);
			Line.JI_TempImportNum = "B123";
			Line.JI_TempImportDate = ZDate.Today.AddDays(-5);
			Assert("Import Number in addinfo string", Line.AddInfo.ZA_TemporaryImportNumbers_Hidden == Line.JI_TempImportNum);

			Line.JI_NoPermitRequired = true;
			AssertEquals("JI_TempImportNum - ReadOnly", false, Line.JI_TempImportNumInfo.ReadOnly);
			AssertEquals("JI_TempImportDate - ReadOnly", false, Line.JI_TempImportNumInfo.ReadOnly);

			Line.JI_NoPermitRequired = false;
			AssertEquals("JI_TempImportNum - ReadOnly", false, Line.JI_TempImportNumInfo.ReadOnly);
			AssertEquals("JI_TempImportDate - ReadOnly", false, Line.JI_TempImportNumInfo.ReadOnly);
		}

		public void TestAddInfoProxy()
		{
			Line.JI_AUState = "NSW";
			Assert("AU State", Line.JI_AddInfo.IndexOf("AUState_Hidden=NSW") >= 0);

			Line.JI_CountryOfOrigin = "NZ";
			Assert("Add Info", Line.JI_AddInfo.IndexOf("ORG=NZ") >= 0);
		}

		public void TestOrgAddInfoWithInvalid()
		{
			Line.JI_AddInfo = "ORG=AU";
			AssertEquals("AU origin", "AU", Line.JI_CountryOfOrigin);

			Line.JI_AddInfo = "ORG=XX";
			AssertEquals("No origin", "XX", Line.JI_CountryOfOrigin);
		}

		public void TestEnterOriginFillAddInfo()
		{
			Line.JI_CountryOfOrigin = "AU";
			Assert("Add Info string should have Origin", Line.JI_AddInfo.IndexOf("ORG=AU") >= 0);
		}

		public void TestEnterAddInfoFillOrigin()
		{
			var addInfoString = "ORG=AU";
			Line.JI_AddInfo = addInfoString;
			AssertEquals("JI_CountryOfOrigin should be filled", "AU", Line.JI_CountryOfOrigin);

			using (new JobComInvoiceHeader.ResetLineValuesFromAddInfoSuspender(Header))
			{
				addInfoString = "ORG=SG";
				Line.JI_AddInfo = addInfoString;
				AssertEquals("JI_CountryOfOrigin should not be filled as invoice header suspend reset values from addInfo.", "AU", Line.JI_CountryOfOrigin);
			}
		}

		public void TestAuSupplierPartChangeWithSupplierChange()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part PK", Part.PK, Line.JI_OP);

			JobDec.JE_OH_Importer = new ZGuid();
			Header.JZ_OH_Supplier = new ZGuid();
			AssertNull("No part should be returned as neither the importer or the supplier code match", Line.Part);
		}

		public void TestSupplierImporterChangeForPartOnPersistedData()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			Factory.Save();
			AssertEquals("Initial Part PK", Part.PK, Line.JI_OP);

			var secondFactory = new BusinessObjectFactory();
			var secondFactoryDeclaration = secondFactory.Load<JobDeclaration>(JobDec.PK);
			var secondFactoryHeader = secondFactoryDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			var secondFactoryInvoiceLine = secondFactoryHeader.JobComInvoiceLines[0];
			AssertEquals("Second Factory Part PK", Part.PK, secondFactoryInvoiceLine.JI_OP);

			secondFactoryDeclaration.JE_OH_Importer = new ZGuid();
			secondFactoryHeader.JZ_OH_Supplier = new ZGuid();
			AssertNull("No part should be returned as neither the importer or the supplier code match", secondFactoryInvoiceLine.Part);
		}

		public void TestPartChangeInAnotherFactoryOnPersistedData()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var secondFactoryDeclaration = secondFactory.Load<JobDeclaration>(JobDec.PK);
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(secondFactory, JobDec.PK);
			var secondFactoryHeader = secondFactoryDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			var secondFactoryInvoiceLine = secondFactoryHeader.JobComInvoiceLines[0];
			secondFactoryDeclaration.ExternalFactoryRefreshEnabled = true;
			try
			{
				var thirdFactory = new BusinessObjectFactory();
				var thirdFactoryPart = thirdFactory.Load<AUOrgSupplierPart>(Part.PK);
				thirdFactoryPart.OP_Desc = "THIRD FACTORY DESCRIPTION";
				thirdFactory.Save();

				AssertEquals("Invoice Line Description", thirdFactoryPart.OP_Desc, secondFactoryInvoiceLine.JI_Description);
			}
			finally
			{
				secondFactoryDeclaration.ExternalFactoryRefreshEnabled = false;
			}
		}

		public void TestChangeMessageTypeValidateClassification()
		{
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;

			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("JI_CC calcualted", ExportClass.PK, Line.JI_CC);

			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Classification is not valid any more", true, Line.JI_CCInfo.HasNotifications());
		}

		public void TestJI_OP()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			Assert("PreCondition : Importer matches ", Line.InvoiceHeader.JZ_OH_Supplier == Part.RelatedOrganisations[0].OU_OH);
			AssertEquals("Part Calculated", Part.PK, Line.JI_OP);

			Line.JI_PartNo = "";
			AssertEquals("Part Calculated", ZGuid.Empty, Line.JI_OP);
		}

		public void TestJI_CCWithClassifiedPartEntered()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("CC calculated", ExportClass.PK, Line.JI_CC);
		}

		public void TestRemovePartShouldNotClearClassification()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("CC calculated", ExportClass.PK, Line.JI_CC);

			Line.JI_PartNo = "";
			AssertEquals("Classification should stay there", ExportClass.PK, Line.JI_CC);
		}

		public void TestRemovePartShouldNotClearTariff()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("CC calculated", ExportClass.PK, Line.JI_CC);

			Line.JI_PartNo = "";
			AssertEquals("Tariff should stay there", ExportClass.CC_TariffNum, Line.JI_Tariff);
		}

		public void TestJI_TariffNo()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Tariff Num Calculated", ExportTariffNum, Line.JI_Tariff);
		}

		public void TestJI_CCWithJI_TariffChangedThroughGUIKeepsRightDetails()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Tariff Num Calculated", ExportTariffNum, Line.JI_Tariff);

			Line.JI_Tariff = "Other";
			AssertEquals("JI_CC", ZGuid.Empty, Line.JI_CC);
			AssertEquals("JI_OP", Part.PK, Line.JI_OP);
			AssertEquals("JI_PartNo", Part.OP_PartNum, Line.JI_PartNo);
		}

		public void TestJI_OPJI_PartNoWithJI_CCChangedThroughGUIKeepsRightDetails()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Tariff Num Calculated", ExportTariffNum, Line.JI_Tariff);

			Line.JI_CC = ZGuid.NewZGuid();
			AssertEquals("JI_OP", Part.PK, Line.JI_OP);
			AssertEquals("JI_PartNo", Part.OP_PartNum, Line.JI_PartNo);
		}

		public void TestRemoveClassificationShouldNotClearTariff()
		{
			Line.JI_CC = ExportClass.PK;
			AssertEquals("Set Classification Should Set Tariff", ExportTariffNum, Line.JI_Tariff);

			Line.JI_CC = ZGuid.Empty;
			AssertEquals("Set Classification Should Set Tariff", ExportTariffNum, Line.JI_Tariff);
		}

		public void TestSetClassificationShouldSetTariff()
		{
			Line.JI_CC = ExportClass.PK;
			AssertEquals("Set Classification Should Set Tariff", ExportTariffNum, Line.JI_Tariff);
		}

		public void TestSetTariffShouldClearClassification()
		{
			Line.JI_Tariff = ExportClass.CC_TariffNum;
			AssertEquals("Set Tariff should clear classification", ZGuid.Empty, Line.JI_CC);
		}

		public void TestSetTariffShouldClearPart()
		{
			Line.JI_Tariff = ExportClass.CC_TariffNum;
			AssertEquals("Set Tariff should clear Part", ZGuid.Empty, Line.JI_OP);
		}

		public void TestClassificationAllAddInfoDefaulted()
		{
			Line.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var importClass = Factory.New<Classification>();
			importClass.AddInfo.ZA_InstrumentType_Hidden = "MD1";
			importClass.AddInfo.ZA_GSTE = "FOOD";

			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Line.JI_CC = importClass.PK;
			Assert("Line add info should be defaulted", Line.JI_AddInfo.IndexOf("InstrumentType_Hidden=MD1") >= 0);
			Assert("Line add info should be defaulted", Line.JI_AddInfo.IndexOf("GSTE=FOOD") >= 0);
		}

		public void TestDescriptionDefaultedOnClassification()
		{
			var @class = Factory.New<Classification>();
			@class.CC_Description = "Class Description";
			Line.JI_CC = @class.PK;
			AssertEquals("Description", @class.CC_Description.ToUpper(), Line.JI_Description);
		}

		public void TestDescriptionDefaultsToClassificationWhenMergeByCLSOverridingPart()
		{
			Line.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Line.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;

			var @class = Factory.New<Classification>();
			@class.CC_LookupCode = "TestLookup";
			@class.CC_Description = TestClassDescription;
			@class.CC_ClassificationType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Part.PivotsForBinding.RemoveAndDeleteAll();
			var pivot1 = Part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = @class.PK;
			Assert("Part Class should not be Empty", pivot1.CI_CC != ZGuid.Empty);
			Part.OP_Desc = TestPartDescription;
			Factory.Save();
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Line Description should default from Classifications when Mergy By is Classifications", TestClassDescription.ToUpper(), Line.JI_Description);
		}

		public void TestDescriptionDefaultedOnPart()
		{
			Part.OP_Desc = "Part Description";
			Factory.Save();
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Description", Part.OP_Desc.ToUpper(), Line.JI_Description);
		}

		public void TestDescriptionDefaultedOnTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND."
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "UQ");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "9999.99.99";
				AssertEquals("Description", "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND.", Line.JI_Description);
			}
		}

		public void TestUserEnteredDescriptionNotOverwritten()
		{
			var userEnteredDescription = "This is a test description";
			Line.JI_Description = userEnteredDescription;

			Part.OP_Desc = "Part Description";
			Factory.Save();
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Description", userEnteredDescription, Line.JI_Description);
		}

		public void TestDescriptionStaysAsPartDescriptionEvenWhenTariffOverridden()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");

			Part.OP_Desc = "Part Description";
			Factory.Save();
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Description", Part.OP_Desc.ToUpper(), Line.JI_Description);

			Line.JI_Tariff = "0000.00.00";
			AssertEquals("PreCondition:Part should stay", Part, Line.Part);
			AssertEquals("Description Should Stay at Part Description", Part.OP_Desc.ToUpper(), Line.JI_Description);
		}

		#region CustomsQuantity

		public void TestCutomsQtyZeroWhenNotRequired()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");

			Line.JI_Tariff = "0000.00.00";
			AssertEquals("Customs Qty zero", 0m, Line.JI_CustomsQuantity);
		}

		public void TestCustomsQtyReadOnlyWhenNotRequired()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "0000.00.00";
				AssertEquals("Customs Qty read only", true, Line.JI_CustomsQuantityInfo.ReadOnly);
			}
		}

		public void TestMappingFromM3ToCU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "CU");

			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_CommercialPack = "M3";
			refPacks.RP_CustomsPack = "CU";
			refPacks.RP_ConversionFactor = 1;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "0000.00.00";
				Line.JI_InvoiceUQ = "M3";
				Line.JI_InvoiceQuantity = 2m;
				AssertEquals("Customs Qty", 2m, Line.JI_CustomsQuantity);
			}
		}

		public void TestMappingFromM2ToSM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "SM");

			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_CommercialPack = "M2";
			refPacks.RP_CustomsPack = "SM";
			refPacks.RP_ConversionFactor = 1;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "0000.00.00";
				Line.JI_InvoiceUQ = "M2";
				Line.JI_InvoiceQuantity = 2m;
				AssertEquals("Customs Qty", 2m, Line.JI_CustomsQuantity);
			}
		}

		public void TestOldValidationSelection()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			AssertEquals(typeof(DrawbackJobComInvoiceLineValidation), Line.OldValidation.GetType());
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ExportJobComInvoiceLineValidation), Line.OldValidation.GetType());
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals(typeof(EDIFICEJobComInvoiceLineValidation), Line.OldValidation.GetType());
			JobDec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			AssertEquals(typeof(QuarantineJobComInvoiceLineValidation), Line.OldValidation.GetType());
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals(typeof(IMDJobComInvoiceLineValidation), Line.OldValidation.GetType());
			JobDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals(typeof(SACWithoutLinesJobComInvoiceLineValidation), Line.OldValidation.GetType());
			JobDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals(typeof(SACJobComInvoiceLineValidation), Line.OldValidation.GetType());
		}

		public void TestAddMessageErrorToCustomsQtyWhenNotConvertible()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 1 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = ExportClass.CC_TariffNum;
				Line.JI_InvoiceQuantity = 2;
				Line.JI_InvoiceUQ = "CS";//inconvertible unit
				Line.OldValidation.ValidateJI_CustomsQuantity();
				AssertEquals("Error Expected", true, Line.JI_CustomsQuantityInfo.HasNotifications());
			}
		}

		public void TestAddMessageErrorToCustomsQtyWhenNotConvertibleWithPartKnown()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 1 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_PartNo = Part.OP_PartNum;
				Line.JI_InvoiceQuantity = 2;
				Line.JI_InvoiceUQ = "CS";//inconvertible unit
				Line.JI_CustomsQuantity = 0;
				AssertEquals("Error Expected", true, Line.JI_CustomsQuantityInfo.HasNotifications());
			}
		}

		public void TestAddMessageErrorToCustomsQtyWhenZero()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 1 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = ExportClass.CC_TariffNum;
				Line.OldValidation.ValidateJI_CustomsQuantity();
				AssertEquals("Error Expected", true, Line.JI_CustomsQuantityInfo.HasNotifications());
			}
		}

		public void TestNoMessageErrorToCustomsQtyWhenInvoiceUQCustomsUQSame()
		{
			var @class = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "5407.72.00 20");
			Line.JI_InvoiceQuantity = 2m;
			Line.JI_InvoiceUQ = @class.UJ_UQ1;
			Line.OldValidation.ValidateJI_CustomsQuantity();
			AssertEquals("No Error Expected", false, Line.JI_CustomsQuantityInfo.HasNotifications());
		}

		public void TestNoMessageErrorToCustomsQtyWhenConvertible()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			Line.JI_InvoiceQuantity = 2m;
			Line.JI_InvoiceUQ = "KG";//Convertible unit
			Line.OldValidation.ValidateJI_CustomsQuantity();
			AssertEquals("No Error Expected", false, Line.JI_CustomsQuantityInfo.HasNotifications());
		}

		public void TestCustomsQuantityAndUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_PartNo = Part.OP_PartNum;
				AssertEquals("Customs Unit of Quantity", exportCustomsUQ, Line.JI_CustomsUnitQty);

				Line.JI_InvoiceQuantity = 2m;
				Line.JI_InvoiceUQ = "KG";
				Assert(Line.JI_CustomsQuantity == 2 * 1000m);
			}
		}

		public void TestSettingUQTwiceDoesNotChangeQuantity()
		{
			DataRig.InvoiceLine.JI_PartNo = DataRig.BeerPart.OP_PartNum;
			AssertNotNull(DataRig.InvoiceLine.JI_OP);
			AssertEquals("PreCondition", "UNT", DataRig.InvoiceLine.JI_InvoiceUQ);
			DataRig.InvoiceLine.JI_InvoiceQuantity = 10;
			AssertEquals("PreCondition", 10m, DataRig.InvoiceLine.JI_InvoiceQuantity);
			DataRig.InvoiceLine.JI_InvoiceUQ = "UNT";
			AssertEquals("After setting UQ to same value", 10m, DataRig.InvoiceLine.JI_InvoiceQuantity);
			DataRig.InvoiceLine.JI_InvoiceUQ = "CTN";
			AssertEquals("After setting UQ to equivalent value", 10m, DataRig.InvoiceLine.JI_InvoiceQuantity);
		}

		DefaultDataRig fDataRig;
		DefaultDataRig DataRig
		{
			get
			{
				if (fDataRig == null)
				{
					fDataRig = new DefaultDataRig(Factory);
				}
				return fDataRig;
			}
		}

		public void TestCustomsQuantityAndUQWithUQEnteredFirst()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_PartNo = Part.OP_PartNum;
				Assert(Line.JI_CustomsUnitQty == exportCustomsUQ);

				Line.JI_InvoiceUQ = "KG";
				Line.JI_InvoiceQuantity = 2m;
				Assert(Line.JI_CustomsQuantity == 2 * 1000m);
			}
		}

		public void TestCustomsQuantityWithSameTypeConversion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_PartNo = Part.OP_PartNum;
				Line.JI_InvoiceQuantity = 2m;
				Line.JI_InvoiceUQ = Core.Constants.Weight.Ounces;

				//Convert from OZ -> KG -> BO
				ZDecimal expected = Core.Constants.Weight.Convert(2m, "OZ", "KG") * 1000m;
				AssertEquals(decimal.Round(expected, 2), decimal.Round(Line.JI_CustomsQuantity, 2));
			}
		}

		public void TestCalculateCustomsQtyWithoutPartInvoiceUQEnteredFirst()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "29343000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "2934.30.00 30";
				Line.JI_InvoiceUQ = "KG";
				Line.JI_InvoiceQuantity = 2m;

				AssertEquals("Customs Qty calculated", 2m, Line.JI_CustomsQuantity);
			}
		}

		public void TestCustomsQuantityNotRequiredWhenConvertible()
		{
			Line.JI_Tariff = Class_KG.UJ_Code;
			Line.JI_InvoiceUQ = "KG";
			Line.JI_InvoiceQuantity = 0m;
			AssertEquals("Customs Qty required", false, Line.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestCustomsQuantityRequiredWhenNotConvertible()
		{
			Line.JI_Tariff = ExportTariffNum;//without part, there is no part units conversion factor known
			Line.JI_InvoiceUQ = "KG";
			Line.JI_InvoiceQuantity = 0m;
			AssertEquals("Customs Qty required", false, Line.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestCustomsUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 1 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "12345678", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 2 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff2, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NO");

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "33333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 3 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff3, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NR");

			var tariff4 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "44444444", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF 4 DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff4, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "ERR");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_PartNo = Part.OP_PartNum;
				AssertEquals("Customs UQ", exportCustomsUQ, Line.JI_CustomsUnitQty);

				Line.JI_Tariff = ImportTariffNum;
				AssertEquals("Customs UQ", "NO", Line.JI_CustomsUnitQty);

				Line.JI_Tariff = "3333.33.33";
				AssertEquals("Customs UQ should stay empty when tariff's main UOM is 'NR'", ZString.Empty, Line.JI_CustomsUnitQty);

				Line.JI_Tariff = "4444.44.44";
				AssertEquals("Customs UQ should stay empty when tariff's main UOM is 'ERR'", ZString.Empty, Line.JI_CustomsUnitQty);
			}
		}

		public void TestInvoiceQuantityAndUQAvailableWithoutPart()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			Assert("Net Quantity and UQ", !Line.JI_InvoiceUQInfo.ReadOnly);

			Line.JI_PartNo = "";
			Assert("Net Quantity and UQ", !Line.JI_InvoiceUQInfo.ReadOnly);
		}

		public void TestCalculateCustomsQtyWithoutPartSameTypeConversion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "33333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "3333.33.33";
				Line.JI_InvoiceQuantity = 2m;
				Line.JI_InvoiceUQ = "LB";

				ZDecimal expected = decimal.Round(Core.Constants.Weight.Convert(Line.JI_InvoiceQuantity, Line.JI_InvoiceUQ, "KG"), 5);
				AssertEquals("Customs Qty calculated", expected, Line.JI_CustomsQuantity);
			}
		}

		public void TestCalculateCustomsQtyWithoutPart()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "29343000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "2934.30.00 30";
				Line.JI_InvoiceQuantity = 2m;
				Line.JI_InvoiceUQ = "KG";

				AssertEquals("Customs Qty calculated", 2m, Line.JI_CustomsQuantity);
			}
		}

		public void TestSetClassificationShouldClearPart()
		{
			Line.JI_CC = ExportClass.PK;
			AssertEquals("Set Classification Should Clear Part", ZGuid.Empty, Line.JI_OP);
		}

		public void TestInvalidPartNumberLeavesQuantityEnabled()
		{
			Line.JI_PartNo = "123454654654";
			Assert("InvoiceQuantity should not be read only", !Line.JI_InvoiceQuantityInfo.ReadOnly);
			Assert("InvoiceUQQuantity should not be read only", !Line.JI_InvoiceUQInfo.ReadOnly);
		}

		public void TestInvalidPartNumAddWarning()
		{
			Line.JI_PartNo = partNum2;
			AssertEquals("Warning to PartNo", true, Line.JI_PartNoInfo.HasWarnings());
		}

		public void TestInvalidPartNumRemoveJI_CC()
		{
			Line.JI_PartNo = partNum2;
			AssertEquals("Blank Classification", ZGuid.Empty, Line.JI_CC);
		}

		public void TestInvalidPartNumRemoveJI_Tariff()
		{
			Line.JI_PartNo = partNum2;
			AssertEquals("Blank Tariff", "", Line.JI_Tariff);
		}

		#endregion

		#region Invoice Quantity

		public void TestInvoiceQuantityInWeightTypeMakeWeightAndUQReadonly()
		{
			Line.JI_InvoiceQuantity = 2;
			Line.JI_InvoiceUQ = "KG";
			AssertEquals("Net Weight UQ readonly", false, Line.JI_WeightUQInfo.ReadOnly);
			AssertEquals("Net Weight UQ readonly", false, Line.JI_WeightUQInfo.ReadOnly);
			AssertEquals("Net Weight readonly", false, Line.JI_WeightInfo.ReadOnly);
			AssertEquals("Net Weight readonly", false, Line.JI_WeightInfo.ReadOnly);
		}

		public void TestCustomsQtyReadOnlyWhenCalculableFromWeight()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "34060000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			Line.JI_Tariff = "3406.00.00";
			Line.JI_Weight = 2000m;
			Line.JI_WeightUQ = "G";
			AssertEquals("Customs Qty Calculated", false, Line.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestMakeJI_WeightReadWriteWhenPartUnitsHasWeightTypeAndInvoiceUQIsNotWeightType()
		{
			Line.JI_PartNo = Part.OP_PartNum;
			Line.JI_InvoiceUQ = exportCustomsUQ;
			Line.JI_InvoiceQuantity = 2000m;
			AssertEquals("Weight ReadOnly", true, !Line.JI_WeightInfo.ReadOnly);
			AssertEquals("Weight UQ ReadOnly", true, !Line.JI_WeightUQInfo.ReadOnly);
		}

		public void TestEnteringCustomsQuantityNotClearWeight()
		{
			Line.JI_CustomsUnitQty = "PKG";
			Line.JI_Weight = 20m;
			Line.JI_WeightUQ = "KG";

			Line.JI_CustomsQuantity = 10m;
			AssertEquals("Weight stays there", 20m, Line.JI_Weight);
		}

		public void TestRefPacksConversion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "SM");

			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = "M3";
			refPack.RP_CustomsPack = "SM";
			refPack.RP_ConversionFactor = 2m;
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "0000.00.00";
				Line.JI_InvoiceUQ = "M3";
				Line.JI_InvoiceQuantity = 20m;

				AssertEquals("Customs Quantity", 40m, Line.JI_CustomsQuantity);
			}
		}

		public void TestRefPacksConversion2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "12344565", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "BC");

			ExportClass.CC_TariffNum = "1234.45.65";

			var partUnitPK = Part.PK; // To flush Part to disk
			var partUnit = Factory.New<OrgPartUnit>();
			partUnit.OF_OP = partUnitPK;
			partUnit.OF_PackType = "BOX";
			partUnit.OF_ParentPackType = "CTN";
			partUnit.OF_QuantityInParent = 24m;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_PartNo = Part.OP_PartNum;
				Line.JI_InvoiceUQ = "BOX";
				Line.JI_InvoiceQuantity = 48m;
				AssertEquals("Customs Qty", 2m, Line.JI_CustomsQuantity);
			}
		}

		public void TestRefPacksConversion3WithSupplier()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "BC");

			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = "BOX";
			refPack.RP_CustomsPack = "BC";
			refPack.RP_OH_Supplier = Header.JZ_OH_Supplier;
			refPack.RP_ConversionFactor = 2m;

			var refPack2 = Factory.New<CusRefPacks>();
			refPack2.RP_CommercialPack = "BOX";
			refPack2.RP_CustomsPack = "BC";
			refPack2.RP_ConversionFactor = 1m;
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "0000.00.00";
				Line.JI_InvoiceUQ = "BOX";
				Line.JI_InvoiceQuantity = 20m; //this line has a supplier set in the parent header
				AssertEquals("Customs Qty for the supplier", 40m, Line.JI_CustomsQuantity);
			}
		}

		public void TestRefPacksConversion4WithEmptySupplier()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "EXPORT TARIFF DESCRIPTION"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "BC");

			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = "BOX";
			refPack.RP_CustomsPack = "BC";
			refPack.RP_OH_Supplier = Header.JZ_OH_Supplier;
			refPack.RP_ConversionFactor = 2m;

			var refPack2 = Factory.New<CusRefPacks>();
			refPack2.RP_CommercialPack = "BOX";
			refPack2.RP_CustomsPack = "BC";
			refPack2.RP_ConversionFactor = 1m;

			Header.JZ_OH_Supplier = ZGuid.Empty;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = "0000.00.00";
				Line.JI_InvoiceUQ = "BOX";
				Line.JI_InvoiceQuantity = 20m;
				AssertEquals("Customs Qty for the supplier", 20m, Line.JI_CustomsQuantity);
			}
		}

		#endregion

		#region FOB

		//Expected value was obtained using NZ Customs application by BEN
		public void TestFOBValueWithDTDLinesPreExw()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.LandingCharges, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Line total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithDTDLinesExw()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.LandingCharges, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 240);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Line total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithDTDLinesFOB()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Header.JZ_InvoiceAmount = 10360;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.LandingCharges, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Line total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithDTDLinesCIF()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Header.JZ_InvoiceAmount = 9800;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.LandingCharges, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Line total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithDTDLinesDDP()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Header.JZ_InvoiceAmount = 9600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Line total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIFLinePreEXW()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 10400;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9840m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10400m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIFLineEXW()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 10200;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9640m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10200m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 964m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3856m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4820m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIFLineFOB()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 10160;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Header CIF", 10160m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIFLineCIF()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 9600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9600m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithUCI_LineEXW()
		{
			var groupHeader = JobDec.JobComInvoiceGroupHeaders[0];
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 10200;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);
			charge.J7_IsIncludedInITOT = false;

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9840m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10400m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithUCI_LineFOB()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = JobDec.JobComInvoiceGroupHeaders[0];
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 10160;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);
			charge.J7_IsIncludedInITOT = false;

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9800m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10360m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 980m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3920m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4900m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithUCI_LineCIF()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = JobDec.JobComInvoiceGroupHeaders[0];
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 9600;
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);

			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);
			charge.J7_IsIncludedInITOT = false;

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9800m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9800m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 980m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3920m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4900m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LinePreEXW()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Header.JZ_InvoiceAmount = 10340;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9840m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10340m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LinePreEXWWithOverseasInsurance()
		{
			var groupHeader = JobDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 7095m, AUDCurrency.RX_Code);
			charge.J7_IsIncludedInITOT = true;
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 35.25m, AUDCurrency.RX_Code);

			var line1 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Header.JZ_InvoiceAmount = 13600;
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;

			line1.JI_LinePrice = 13600;
			JobDec.ResumeApportionment();
			AssertEquals("Line 1 FOB", 6505m, line1.JI_Calc_FOB, 0.01m);
		}

		public void TestFOBValueWithCFR_LineEXW()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Header.JZ_InvoiceAmount = 10140;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9640m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10140m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 964m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3856m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4820m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LineFOB()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Header.JZ_InvoiceAmount = 10100;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10100m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LineFOBWithGroupCharge()
		{
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Header.JZ_InvoiceAmount = 9600;

			Header.Charges.AddNew(AUChargeCodeList.Codes.Discount, 400);
			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 500m, AUDCurrency.RX_Code);
			oFT.J7_IsIncludedInITOT = true;
			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9100m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9600m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 910m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3640m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4550m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LineCFR()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Header.JZ_InvoiceAmount = 9600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9600m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithUCF_LineEXW()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			Header.JZ_InvoiceAmount = 10140;
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);

			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);
			charge.J7_IsIncludedInITOT = false;

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9840m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10340m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithUCF_LineFOB()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			Header.JZ_InvoiceAmount = 10100;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500);
			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);
			charge.J7_IsIncludedInITOT = false;
			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9800m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10300m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 980m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3920m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4900m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithUCF_LineCFR()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			Header.JZ_InvoiceAmount = 9600;
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);
			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 500, AUDCurrency.RX_Code);
			charge.J7_IsIncludedInITOT = true;

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9300m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9800m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 930m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3720m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4650m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIP_LinePreEXW()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			Header.JZ_InvoiceAmount = 10640;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 100);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 10540m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10640m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 1054m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 4216m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 5270m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIP_LineEXW()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			Header.JZ_InvoiceAmount = 10440;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 500);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9940m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10440m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 994m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3976m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4970m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIP_LineFOB()
		{
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			Header.JZ_InvoiceAmount = 10400;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 500);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9900m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10400m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 990m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3960m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4950m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueForFOB_LinePreEXW()
		{
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			Header.JZ_InvoiceAmount = 10140;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50, AUDCurrency.RX_Code);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 10140m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10190m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 1014m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 4056m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 5070m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueForFOB_LineEXW()
		{
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			Header.JZ_InvoiceAmount = 9940;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ExWorks, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50, AUDCurrency.RX_Code);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9940m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9990m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 994m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3976m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4970m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueForFOB_LineFOB()
		{
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			Header.JZ_InvoiceAmount = 9900;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50, AUDCurrency.RX_Code);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9900m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9950m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 990m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3960m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4950m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueForUFB_LineEXW()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = "LEG";
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;
			Header.JZ_InvoiceAmount = 10000;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 100);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50, AUDCurrency.RX_Code);
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 10200m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10250m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 1020m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 4080m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 5100m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueForUFB_LineFOB()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;
			Header.JZ_InvoiceAmount = 9900;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 300);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50, AUDCurrency.RX_Code);
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200, AUDCurrency.RX_Code);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 10100m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10150m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 1010m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 4040m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 5050m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueForPAF()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			Header.JZ_InvoiceAmount = 9800;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200);

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 300, AUDCurrency.RX_Code);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100, AUDCurrency.RX_Code);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 200, AUDCurrency.RX_Code);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 10100m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10400m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
		}

		public void TestFOBValueForUAF()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			var line1 = Header.JobComInvoiceLines.AddNew();
			var line2 = Header.JobComInvoiceLines.AddNew();
			var line3 = Header.JobComInvoiceLines.AddNew();
			SetHeaderCurrencies(Header, AUDCurrency.RX_Code);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			Header.JZ_InvoiceAmount = 9600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 300, AUDCurrency.RX_Code);
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200, AUDCurrency.RX_Code);
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 100, AUDCurrency.RX_Code);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			JobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, Header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9900m, Header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10200m, Header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, Header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 990m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3960m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4950m, line3.JI_Calc_FOB);
		}

		public void TestForeignInlandFreightAffectLineFOB()
		{
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			Header.JZ_InvoiceAmount = 10000m;
			Header.JZ_RX_NKInvoice_Currency = "AUD";

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 1000);

			Line.JI_LinePrice = 9000m;
			JobDec.ResumeApportionment();
			AssertEquals("Line FOB", 10000m, Line.JI_Calc_FOB);
		}

		public void TestCalculateCIFWithPAF()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 1);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 5);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			ZDecimal expected = 10600m;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / EXW ", expected, Header.JZ_Calc_CIFAmount);

			var groupHeader = Header.Master as JobComInvoiceGroupHeader;
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);

			expected = 10600 + 100m;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / EXW ", expected, Header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithUAF()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(AUChargeCodeList.Codes.BuyingCommission, 200);
			Header.Charges.AddNew(AUChargeCodeList.Codes.Discount, 1);
			Header.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 5);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			ZDecimal expected = 10600 - 200 - 5 + 1;
			AssertEquals("Real Invoice / UAF ", expected, Header.InvoiceLineTotal);
		}

		public void TestCalculateRealInvoiceTotalWithUCI()
		{
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 1);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			ZDecimal expected = 10600m - 200 - 300 - 400 - 5 + 1;
			AssertEquals("Real Invoice / UCI ", expected, Header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithUCI()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 1);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			ZDecimal expected = 10600m;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / UCI ", expected, Header.JZ_Calc_CIFAmount);

			var groupHeader = Header.Master as JobComInvoiceGroupHeader;

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 100, JobDeclaration.LocalCurrencyConstantCode);

			expected = 10600 + 100;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / UCI ", expected, Header.JZ_Calc_CIFAmount);
			AssertEquals("FOB Value / UCI", 10600m - 400m - 300m + 100, Header.JZ_Calc_FOBAmount);
		}

		public void TestCalculateRealInvoiceTotalWithUCF()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 1);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 400);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			ZDecimal expected = 10600m - 200 - 400 - 5 + 1;
			AssertEquals("Real Invoice / UCI ", expected, Header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithUCF()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 1);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 400);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			ZDecimal expected = 10600m;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / UCI ", expected, Header.JZ_Calc_CIFAmount);

			var groupHeader = Header.Master as JobComInvoiceGroupHeader;

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 100, JobDeclaration.LocalCurrencyConstantCode);
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50, JobDeclaration.LocalCurrencyConstantCode);

			expected = 10600 + 100 + 50;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / UCI ", expected, Header.JZ_Calc_CIFAmount);
		}
		public void TestCalculateRealInvoiceTotalWithLIS()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 1);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			BaseJobComInvHeaderCharge nonDutiable = Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 600);
			nonDutiable.J7_IsDutiable = false;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.LandingCharges, 500);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			ZDecimal expected = 10600m - 200 - 300 - 400 - 500 - 600 - 40 - 5 + 1;
			AssertEquals("Real Invoice / LIS ", expected, Header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithLIS()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 40);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 1);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 5);
			BaseJobComInvHeaderCharge nonDutiable = Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 600);
			nonDutiable.J7_IsDutiable = false;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 400);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.LandingCharges, 500);

			Header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			ZDecimal expected = 10600m - 500;
			AssertEquals("CIF value / LIS ", expected, Header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateCIFWithUFB()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Header.JZ_InvoiceAmount = 10600;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			Header.Charges.AddNew(AUChargeCodeList.Codes.OtherCommission, 10);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 5);

			var groupHeader = Header.Master;
			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.PackingCost, 90, JobDeclaration.LocalCurrencyConstantCode);

			ZDecimal expected = 10600m + 90;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / UFB ", expected, Header.JZ_Calc_CIFAmount);

			groupHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 240, JobDeclaration.LocalCurrencyConstantCode);

			expected = 10600m + 90 + 240;
			JobDec.ResumeApportionment();
			AssertEquals("CIF value / UFB ", expected, Header.JZ_Calc_CIFAmount);
		}

		#endregion

		public void TestJI_LinePriceMoney()
		{
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			Line.JI_LinePrice = 1000m;
			AssertEquals("Line total", 1000m, Line.JI_LinePriceMoney.Amount);
			AssertEquals("Line total", AUDCurrency, Line.JI_LinePriceMoney.Currency);

			Line.JI_LinePrice = 2000m;
			AssertEquals("Line total", 2000m, Line.JI_LinePriceMoney.Amount);
			AssertEquals("Line total", AUDCurrency, Line.JI_LinePriceMoney.Currency);
		}

		public void TestJI_BuyingCommssion()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_InvoiceAmount = 1000m;
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;

			Header.Charges.AddNew(AUChargeCodeList.Codes.BuyingCommission, 100);

			Line.JI_LinePrice = 900m;
			ZDecimal expected = 900 / (1000 - 100) * 100m;
			JobDec.ResumeApportionment();
			AssertEquals("Buying Commmission", expected, Line.JI_BuyingCommission.Amount);
			AssertEquals("Commssion Currency", AUDCurrency.PK, Line.JI_BuyingCommission.Currency.PK);
		}

		public void TestJI_OtherCommssion()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_InvoiceAmount = 1000m;
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;

			Header.Charges.AddNew(AUChargeCodeList.Codes.OtherCommission, 100);

			Line.JI_LinePrice = 900m;
			ZDecimal expected = 900 / (1000 - 100) * 100m;
			JobDec.ResumeApportionment();
			AssertEquals("Other Commmission", expected, Line.JI_OtherCommission.Amount);
			AssertEquals("Commssion Currency", AUDCurrency.PK, Line.JI_OtherCommission.Currency.PK);
		}

		public void TestTransportAndInsurance()
		{
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 1000m;
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 100);
			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50);

			Line.JI_LinePrice = 850m;
			ZDecimal expected = 850 / (1000 - 100 - 50) * (100 + 50m);
			JobDec.ResumeApportionment();
			AssertEquals("T And I cost", expected, Line.TransportAndInsurance.Amount);
			AssertEquals("T And I Currency", AUDCurrency, Line.TransportAndInsurance.Currency);
		}

		public void TestTransportAndInsuranceForeignCurrency()
		{
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Header.JZ_InvoiceAmount = 1000m;
			Header.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;

			Header.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 50);

			Line.JI_LinePrice = 950m;
			ZDecimal expected = 950 / (1000 - 50) * (50m);
			JobDec.ResumeApportionment();
			AssertEquals("T And I cost", expected, Line.TransportAndInsurance.Amount);
			AssertEquals("T And I Currency", USDCurrency, Line.TransportAndInsurance.Currency);
		}

		public void TestCorrectCacheForChargesHeaderChanged()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_InvoiceAmount = 1000m;
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			Header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;

			var lCHs = Header.Charges.Find(Header.IncoTermAndChargeFactory.GetCharge(Common.CustomsChargeTypeList.Codes.LandingCharges).ChargeCodeChargeKey);
			var lCH = lCHs[0];
			lCH.J7_Amount = 50;
			lCH.J7_IsIncludedInITOT = true;

			Line.JI_LinePrice = 1000;
			JobDec.ResumeApportionment();
			AssertEquals("Landing charges for Line", 50m, Line.JI_LandingCharges.Amount);

			lCH.J7_Amount = 10m;
			JobDec.ResumeApportionment();
			AssertEquals("Landing charges for Line", 10m, Line.JI_LandingCharges.Amount);
		}

		public void TestBuyingAndOtherCommission2()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Header.JZ_InvoiceAmount = 1000m;
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;

			Header.Charges.AddNew(AUChargeCodeList.Codes.BuyingCommission, 100);

			Line.JI_LinePrice = 900m;
			JobDec.ResumeApportionment();
			AssertEquals("Line level of Buying Commission", 100m, Line.JI_BuyingCommission.Amount);
			AssertEquals("Line level of Other Commission", 0m, Line.JI_OtherCommission.Amount);

			BaseJobComInvHeaderCharge commission = Header.Charges[AUChargeCodeList.Codes.BuyingCommission];
			commission.J7_ChargeType = AUChargeCodeList.Codes.OtherCommission;
			JobDec.ResumeApportionment();
			AssertEquals("Line level of Buying Commission", 00m, Line.JI_BuyingCommission.Amount);
			AssertEquals("Line level of Other Commission", 100m, Line.JI_OtherCommission.Amount);
		}

		public void TestRemovingExportFieldsOnSavingWithImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line = header.JobComInvoiceLines.AddNew();

			line.JI_Texco = true;
			line.JI_MotorVehiclePlan = true;
			line.JI_Drawback = true;

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			line.OnSaving();

			AssertEquals("No value for Texco", "", line.AddInfo.ZA_Texco_Hidden);
			AssertEquals("No value for MotorVehiclePlan", "", line.AddInfo.ZA_MotorVehiclePlan_Hidden);
			AssertEquals("No value for Drawback", "", line.AddInfo.ZA_Drawback_Hidden);
		}

		public void TestChangeParentLineInvoiceNoAffectChildLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var header1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_InvoiceNumber = "100";

			var header2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_InvoiceNumber = "200";

			var parentLine = declaration.FilteredInvoiceLines.AddNew();
			parentLine.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			parentLine.JI_Calc_Invoice = "100";

			var childLine = declaration.FilteredInvoiceLines.AddNew();
			childLine.JI_LinePrefix = "T";
			AssertEquals("Child Line should have invoice no of parent line", "100", childLine.JI_Calc_Invoice);

			parentLine.JI_Calc_Invoice = "200";
			childLine.RunPreSaveValidation();
			AssertEquals("Child Line should have invoice no of parent line", "100", childLine.JI_Calc_Invoice);
			AssertEquals("Child Line should have a message error", true, childLine.JI_Calc_InvoiceInfo.HasMessageErrors());
		}

		public void TestDeleteParentFreeUpChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			var line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line2.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;
			AssertEquals("Line 2 should be a trailer of line1", line1.PK.ToString(), line2.AddInfo.ZA_RelatedLinePK_Hidden);

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Normal;
			AssertEquals("Line 2 should be stand alone", "", line2.JI_LinePrefix);
			AssertEquals("Line2 invoice number editible", false, line2.JI_Calc_InvoiceInfo.ReadOnly);
			AssertEquals("Line2 Line prefix editible", false, line2.JI_LinePrefixInfo.ReadOnly);
		}

		public void TestAuSupplierPart()
		{
			var helper = new ZTestHelper(Factory);
			var importClassWithQuestions = Factory.New<Classification>();
			importClassWithQuestions.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			importClassWithQuestions.CC_LookupCode = "LookupCode1";
			importClassWithQuestions.CC_TariffNum = helper.TestTariffNumber1;
			importClassWithQuestions.CC_Description = "DESCRIPTION";
			importClassWithQuestions.CC_IsActive = false;
			importClassWithQuestions.CC_AddInfo = "ORG=AU";
			importClassWithQuestions.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var newPart = Factory.New<AUOrgSupplierPart>();
			newPart.OP_PartNum = "TestAUSupplierPart";
			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = Consignor.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			newPart.OP_StockKeepingUnit = "CT";
			var pivot1 = newPart.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = importClassWithQuestions.PK;

			Factory.Save(); // Must persist for query to work.

			var dec = Factory.New<JobDeclaration>();
			var header = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_OH_Supplier = Consignor.PK;
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_PartNo = newPart.OP_PartNum;

			AssertEquals("Part Org Owner should equal to new part", newPart.RelatedOrganisations[0].OU_OH, line.Part.RelatedOrganisations[0].OU_OH);
			AssertEquals("Part Code", newPart.OP_PartNum, line.Part.OP_PartNum);
			AssertEquals("Part should equal to new part", newPart.PK, line.Part.PK);
		}

		public void TestCustomsUnitQtyGetter()
		{
			AssertEquals("Customs Unit Qty", "", Line.JI_CustomsUnitQty);
		}

		public void TestSettingPartWithAddInfoPopulateAddInfo()
		{
			JobDec.JE_OH_Importer = Consignee.PK;
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var importTariff = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "5407.72.00 20");
			var importClass = Factory.New<Classification>();
			importClass.CC_ClassificationType = ClassificationType.IMP;
			importClass.CC_Description = "Description";
			importClass.CC_TariffNum = importTariff.UJ_Code;
			importClass.CC_LookupCode = "Test";

			var newPart = Factory.New<AUOrgSupplierPart>();
			var pivot1 = newPart.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = importClass.PK;
			pivot1.AddInfo.ZA_TreatmentCode_Hidden = "110";
			newPart.OP_PartNum = "PartPartPart1";

			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = JobDec.Importer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			Line.JI_PartNo = newPart.OP_PartNum;
			AssertEquals("Line should have an addinfo", "110", Line.TreatmentCode);
		}

		public void TestDefaultInvoiceQtyFromPartStockKeepingUnit()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			ImportClass.CC_TariffNum = (Factory.LoadTop1<AUCClass>(new ZQuery(AUCClassSchema.UJ_UQ1, "BC"))).UJ_Code;
			var newPart = Factory.New<AUOrgSupplierPart>();
			var pivot1 = newPart.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			newPart.OP_PartNum = "PartPartPart1";

			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = Header.JZ_OH_Supplier;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			newPart.OP_StockKeepingUnit = "CTN";

			Factory.Save();

			Line.JI_PartNo = newPart.OP_PartNum;
			AssertEquals("Invoice UQ filled in", "CTN", Line.JI_InvoiceUQ);
		}

		public void TestCalculateCustomsQuantityFromPartStockKeepingUnit()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			ImportClass.CC_TariffNum = (Factory.LoadTop1<AUCClass>(new ZQuery(AUCClassSchema.UJ_UQ1, "NO"))).UJ_Code;
			var newPart = Factory.New<AUOrgSupplierPart>();
			var pivot1 = newPart.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_CC = ImportClass.PK;
			newPart.OP_PartNum = "PartPartPart1";

			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = Header.JZ_OH_Supplier;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			newPart.OP_StockKeepingUnit = "BOX";

			var unitConversion = newPart.PartUnits.AddNew();
			unitConversion.OF_QuantityInParent = 20m;
			unitConversion.OF_PackType = "PCE";
			unitConversion.OF_ParentPackType = "BOX";

			Factory.Save();

			Line.JI_PartNo = newPart.OP_PartNum;
			AssertEquals("Invoice UQ filled in", "BOX", Line.JI_InvoiceUQ);
		}

		public void TestInvoiceUQDefaultingCalculateWeight()
		{
			Line.JI_InvoiceQuantity = 100m;
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Weight UQ", "KG", Line.JI_WeightUQ);
		}

		[ExpectNoExceptions]
		public void TestCloneDoesNotThrowException()
		{
			Line.JI_PartNo = "12345";
			Line.InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			Line.Clone();
		}

		public void TestJI_PartsListForMatchingImporter()
		{
			Header.JZ_OH_Supplier = ZGuid.Empty;
			JobDec.JE_OH_Supplier = ZGuid.Empty;
			Assert("PreCondition", JobDec.JE_OH_Importer.IsValid);
			var partCollection = Line.Lookups.PartsList;
			partCollection.Load();
			Part.RelatedOrganisations.RemoveAndDeleteAll();
			var importerRelation = Part.RelatedOrganisations.AddNew();
			importerRelation.OU_OH = JobDec.JE_OH_Importer;
			importerRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			Factory.Save();
			partCollection = Line.Lookups.PartsList;
			partCollection.Load();
			AssertEquals("PreCondition", 0, partCollection.Count);
			importerRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();
			partCollection = Line.Lookups.PartsList;
			partCollection.Load();
			AssertEquals("Assigning valid part should add to parts collection", 1, partCollection.Count);
		}

		public void TestPartUpdatedInAnotherFactoryUpdatesInvoiceLineDescription()
		{
			const string NewDescription = "NEWDESCRIPTION";

			var importerRelation = Part.RelatedOrganisations.AddNew();
			importerRelation.OU_OH = JobDec.JE_OH_Importer;
			importerRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();

			var line1 = Line;
			var line2 = Header.JobComInvoiceLines.AddNew();
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(line1.Factory, line1.PartSyncManagerActiveDeciderPK);
			line1.JI_PartNo = Part.OP_PartNum;
			line2.JI_PartNo = Part.OP_PartNum;
			line2.Delete();
			Assert("PreCondition", line1.Part != null);

			var factory2 = new BusinessObjectFactory();
			var partInOtherFactory = factory2.Load<AUOrgSupplierPart>(line1.Part.PK);

			Assert("PreCondition", line1.Part != partInOtherFactory);
			partInOtherFactory.OP_Desc = NewDescription;

			Assert("PreCondition", line1.JI_Description != NewDescription);
			factory2.Save();
			AssertEquals(NewDescription, line1.JI_Description);
		}

		public void TestJI_PartsListForMatchingSupplier()
		{
			JobDec.JE_OH_Importer = ZGuid.Empty;
			JobDec.JE_OH_Supplier = ZGuid.Empty;
			Assert("PreCondition", Header.JZ_OH_Supplier.IsValid);
			var partCollection = Line.Lookups.PartsList;
			partCollection.Load();
			Part.RelatedOrganisations.RemoveAndDeleteAll();
			var supplierRelation = Part.RelatedOrganisations.AddNew();
			supplierRelation.OU_OH = Header.JZ_OH_Supplier;
			supplierRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();
			partCollection = Line.Lookups.PartsList;
			partCollection.Load();
			AssertEquals("PreCondition", 0, partCollection.Count);
			supplierRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			Factory.Save();
			partCollection = Line.Lookups.PartsList;
			partCollection.Load();
			AssertEquals("Assigning valid part should add to parts collection", 1, partCollection.Count);
		}

		public void TestAddNewPartWithFullDefaults()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var newClass = Factory.New<Classification>();
			newClass.CC_LookupCode = "LOOKUP";
			newClass.CC_ClassificationType = ClassificationType.IMP;
			newClass.CC_TariffNum = "0206.29.00 26";
			Line.JI_PartNo = "PARTNUM";
			Line.JI_Description = "LINE DESCRIPTION";
			Line.JI_CC = newClass.PK;
			Line.JI_AddInfo = "InstrumentCode_Hidden=1234567*DRE=1.0000*TreatmentCode_Hidden=505*DXT=S*AMB=D*InstrumentType_Hidden=TC*DMP=1.00*DCX=ZA*ORG=FR*TCI=BL:2345678";
			Line.JI_InvoiceUQ = "DZ";
			var parts = (AUOrgSupplierPartCollection)Line.Lookups.PartsList;
			var newPart = parts.AddNew();
			AssertEquals("Part Description", "LINE DESCRIPTION", newPart.OP_Desc);
			AssertEquals("Units", "DZ", newPart.OP_StockKeepingUnit);
			var pivot = newPart.PivotsForBinding[0];
			AssertEquals("Class PK", newClass.PK, pivot.CI_CC);
			AssertEquals("Lookup Code", "LOOKUP", pivot.Classification.CC_LookupCode);
			AssertEquals("Tariff", "0206.29.00 26", pivot.TariffAndStatNumber);
			AssertEquals("Addinfo", "DCX=ZA*DXT=S*InstrumentCode_Hidden=1234567*InstrumentType_Hidden=TC*ORG=FR*TCI=BL:2345678*TreatmentCode_Hidden=505", pivot.CI_AddInfo);
			AssertEquals("Owner", OrgPartRelation.RelationshipTypes.Owner, newPart.RelatedOrganisations[0].OU_Relationship);
			AssertEquals("Owner", JobDec.JE_OH_Importer, newPart.RelatedOrganisations[0].OU_OH);
		}

		public void TestPermitsIncludingChildren()
		{
			Line.AddInfo.ZA_PermitNumbers_Hidden = "A123:456";
			Line.InvoiceHeader.AddInfo.ZA_PermitNumbers_Hidden = "B123:789";

			var permitNumbers = Line.PermitsIncludingHeader;
			AssertEquals(2, permitNumbers.Count);
			AssertEquals("A123", permitNumbers[0]);
			AssertEquals("B123", permitNumbers[1]);
		}

		public void TestMultiplePermitNumbers()
		{
			Line.AddInfo.ZA_PermitNumbers_Hidden = "";
			AssertEquals("PermitNumber1", ZString.Empty, Line.PermitNumber1);
			AssertEquals("PermitNumber2", ZString.Empty, Line.PermitNumber2);
			AssertEquals("PermitNumber3", ZString.Empty, Line.PermitNumber3);
			Line.AddInfo.ZA_PermitNumbers_Hidden = "P1";
			AssertEquals("PermitNumber1", "P1", Line.PermitNumber1);
			AssertEquals("PermitNumber2", ZString.Empty, Line.PermitNumber2);
			AssertEquals("PermitNumber3", ZString.Empty, Line.PermitNumber3);
			Line.AddInfo.ZA_PermitNumbers_Hidden = "P1,P2";
			AssertEquals("PermitNumber1", "P1", Line.PermitNumber1);
			AssertEquals("PermitNumber2", "P2", Line.PermitNumber2);
			AssertEquals("PermitNumber3", ZString.Empty, Line.PermitNumber3);
			Line.AddInfo.ZA_PermitNumbers_Hidden = "P1,P2,P3";
			AssertEquals("PermitNumber1", "P1", Line.PermitNumber1);
			AssertEquals("PermitNumber2", "P2", Line.PermitNumber2);
			AssertEquals("PermitNumber3", "P3", Line.PermitNumber3);
		}

		public void TestEncryptionNumbersIncludingHeader()
		{
			Line.AddInfo.ZA_PermitNumbers_Hidden = "A123:456";
			Line.InvoiceHeader.AddInfo.ZA_PermitNumbers_Hidden = "B123:789";
			var encryptionNumbers = Line.EncryptionNumbersIncludingHeader;
			AssertEquals(2, encryptionNumbers.Count);
			AssertEquals("456", encryptionNumbers[0]);
			AssertEquals("789", encryptionNumbers[1]);
		}

		public void TestPutTooManyCharactersInJITariff()
		{
			Line.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Line.JI_Tariff = "2505100009";
			Line.JI_Tariff = "12345678901234567890";
			AssertEquals("JI_Tariff", "1234.56.78 9012", Line.JI_Tariff);
		}

		public void TestOrder()
		{
			var newOrder = Factory.New<Order>();
			newOrder.JD_OrderNumber = "ordernum";
			var newOrderSplit = Factory.New<Order>();
			newOrderSplit.JD_OrderNumber = "ordernum";
			newOrderSplit.JD_OrderNumberSplit = 1;

			Line.JI_OrderNumber = "ordernum";
			AssertEquals("Should match on the primary order (not some split)", newOrder.PK, Line.Order.PK);
		}

		public void TestJI_WeightedCostByLocation()
		{
			var product = AUOrgSupplierPart.New(Factory);
			var importerOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importerOrg.OH_RL_NKClosestPort = "AUSYD";
			product.OP_PartNum = "somept";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importerOrg.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save(); // to suppress the dirty table query problem
			var location1 = product.Locations.AddNew();
			var location2 = product.Locations.AddNew();

			location1.OR_WeightCostThisLocation = 2m;
			location1.OR_Warehouse = "1111";
			location2.OR_WeightCostThisLocation = 4m;
			location2.OR_Warehouse = "2222";

			var addressOrg = OrgHeader.New(Factory);
			var address = addressOrg.Addresses.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "somept";
			declaration.ImporterDeliveryAddress.E2_OA_Address = address.PK;
			declaration.JE_OH_Importer = importerOrg.PK;
			var productRetrievedFromInvoiceLine = invoiceLine.Part;

			address.OA_Code = "1111";
			AssertEquals(2m, invoiceLine.JI_WeightedCostByLocation);

			address.OA_Code = "2222";
			AssertEquals(4m, invoiceLine.JI_WeightedCostByLocation);
		}

		public void TestISupportDataImporting()
		{
			var testObjectToImport = Factory.New(typeof(JobComInvoiceLine));
			Assert("Woolies importer requires the class to support ISupportDataImporting", testObjectToImport is ISupportDataImporting);
			((ISupportDataImporting)testObjectToImport).IsImportingData = true;
			AssertEquals("Importing should be set to true", true, ((ISupportDataImporting)testObjectToImport).IsImportingData);
			((ISupportDataImporting)testObjectToImport).IsImportingData = false;
			AssertEquals("Importing should be set to false", false, ((ISupportDataImporting)testObjectToImport).IsImportingData);
		}

		public void TestPartDetailsUpdatedIfNewPartCreatedInAnotherFactory()
		{
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(Line.Factory, Line.PartSyncManagerActiveDeciderPK);
			Line.JI_PartNo = "NEWPARTNUM";

			var factory2 = new BusinessObjectFactory();
			var newPart = factory2.New<AUOrgSupplierPart>();
			newPart.OP_PartNum = "NEWPARTNUM";
			newPart.OP_Desc = "NEW PART DESCRIPTION";
			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = Consignor.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			newPart.OP_StockKeepingUnit = "KG";
			factory2.Save();

			AssertEquals("Description", "NEW PART DESCRIPTION", Line.JI_Description);
		}

		public void TestJI_CustomsTotalValue()
		{
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			JobDec.DepotDocAddress.E2_OA_Address = GetValidDepotAddress().PK;
			Header.JZ_AddInfo = "WRN=123456"; //Nature30
			Header.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			Line.JI_LinePrice = 100m;
			AssertEquals("Customs Total Value", 100m, Line.JI_CustomsTotalValue.Amount);
			AssertEquals("Customs Total Value Currency", AUDCurrency, Line.JI_CustomsTotalValue.Currency);
		}

		public void TestJI_AUStatesList()
		{
			JobDec.JE_MessageType = "EXP";
			var jobComInvoiceHeader = JobDec.Invoices.AddNew();
			var jobComInvoiceLine = jobComInvoiceHeader.JobComInvoiceLines.AddNew();
			AssertContains("NSW", jobComInvoiceLine.JI_AUStatesList.CodesAsString);
			AssertNotContains("JER", jobComInvoiceLine.JI_AUStatesList.CodesAsString);
			JobDec.JE_MessageType = "AQS";
			AssertContains("NSW", jobComInvoiceLine.JI_AUStatesList.CodesAsString);
		}

		public void TestRecalculateWRQAndWRUWhenCustomsUQEntered()
		{
			var invoice = JobDec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("WRU", "KG", invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("WRQ", 10m, invoiceLine.AddInfo.ZA_WRQ);

			invoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("WRU", "KG", invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("WRQ", 10m, invoiceLine.AddInfo.ZA_WRQ);

			invoiceLine.JI_IsPackToBondForLine = false;
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			invoiceLine.AddInfo.ZA_WRU = ZString.Empty;
			invoiceLine.AddInfo.ZA_WRQ = ZDecimal.Zero;
			((ISupportDataImporting)invoiceLine).IsImportingData = true;
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("WRU", ZString.Empty, invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("WRQ", ZDecimal.Zero, invoiceLine.AddInfo.ZA_WRQ);

			invoiceLine.JI_InvoiceQuantity = 100m;
			AssertEquals("WRU", ZString.Empty, invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("WRQ", ZDecimal.Zero, invoiceLine.AddInfo.ZA_WRQ);

			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("WRU", "KG", invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("WRQ", 100m, invoiceLine.AddInfo.ZA_WRQ);

			invoiceLine.AddInfo.ZA_WRU = ZString.Empty;
			invoiceLine.AddInfo.ZA_WRQ = ZDecimal.Zero;
			invoiceLine.JI_InvoiceUQ = "T";
			AssertEquals("WRU", "T", invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("WRQ", 100m, invoiceLine.AddInfo.ZA_WRQ);
		}

		#region TestContainersForInvoiceLines

		public void TestNonPersistentContainer()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var container = dec.CusContainers.AddNew();
			container.CO_ContainerNumber = "OLCU0000000";
			var nonPersistentContainer = line.ContainersForInvoiceLinesForBindingOnly[0];
			nonPersistentContainer.IsForInvoiceLine = true;
			AssertEquals("OLCU0000000", nonPersistentContainer.ContainerNumber);
			Assert("Should be selected", nonPersistentContainer.IsForInvoiceLine);
		}

		public void TestContainersForInvoiceLines()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var container1 = dec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OLCU0000001";
			var container2 = dec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OLCU0000002";
			var container3 = dec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OLCU0000003";
			AssertEquals("Should have all three containers here", 3, line.ContainersForInvoiceLinesForBindingOnly.Count);
		}

		#endregion

		#region Test Properties from dbo.CusEntryLine

		public void TestJI_Calc_DutyAmount()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyAmount, BaseJobComInvoiceLine.Schema.JI_Calc_DutyAmount);
		}

		public void TestJI_Calc_InterimAntiDumpingDuty()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.InterimAntiDumpingDuty, JobComInvoiceLine.Schema.JI_Calc_InterimAntiDumpingDuty);
		}

		public void TestJI_Calc_InterimCountervailingDuty()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.InterimCountervailingDuty, JobComInvoiceLine.Schema.JI_Calc_InterimCountervailingDuty);
		}

		public void TestJI_Calc_WETAmount()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.WetAmount, JobComInvoiceLine.Schema.JI_Calc_WETAmount);
		}

		public void TestJI_Calc_GSTVATAmount()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.GSTAmount, BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATAmount);
		}

		public void TestJI_Calc_GSTVATDeferred()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.GSTDeferred, BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATDeferred);
		}

		public void TestJI_Calc_LCTAmount()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.LCTAmount, JobComInvoiceLine.Schema.JI_Calc_LCTAmount);
		}

		public void TestJI_Calc_FlatDutyPortion()
		{
			TestCalcMoneyAmountFromCusEntryLine(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.FlatDutyPortion, "JI_Calc_FlatDutyPortion");
		}

		public void TestJI_Calc_AQISContainerCharges()
		{
			TestCalcMoneyAmountFromCusEntryHeader(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISContainerCharges, "JI_Calc_AQISContainerCharges");
		}

		public void TestJI_Calc_AQISProcessingCharge()
		{
			TestCalcMoneyAmountFromCusEntryHeader(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISProcessingCharge, "JI_Calc_AQISProcessingCharge");
		}

		void TestCalcMoneyAmountFromCusEntryHeader(string entryHeaderChargeType, string invoiceHeaderAmountPropertyName)
		{
			JobDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			Line.JI_Tariff = "0101.1000/25";
			Line.JI_LinePrice = 1m;
			var line2 = Header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0101.1000/25";
			line2.JI_LinePrice = 2m;
			var line3 = Header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0101.1000/25";
			line3.JI_LinePrice = 3m;

			AssertEquals("PreCondition : Invoice Line Count", 3, JobDec.FilteredInvoiceLines.Count);

			Header.JZ_InvoiceAmount = 30m;
			Header.JZ_RX_NKInvoice_Currency = "AUD";
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			JobDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobDec.DoMerge();
			AssertEquals("PreCondition : Entries", 1, JobDec.CustomsEntryHeaders.Count);
			AssertEquals("PreCondition : Merged Lines", 1, JobDec.CustomsEntryHeaders[0].MergedLines.Count);
			JobDec.CustomsEntryHeaders[0].Charges[entryHeaderChargeType].C1_ChargeAmount = 30m;

			AssertEquals(invoiceHeaderAmountPropertyName, 5m, Line[invoiceHeaderAmountPropertyName]);
			AssertEquals(invoiceHeaderAmountPropertyName, 10m, line2[invoiceHeaderAmountPropertyName]);
			AssertEquals(invoiceHeaderAmountPropertyName, 15m, line3[invoiceHeaderAmountPropertyName]);
		}

		void TestCalcMoneyAmountFromCusEntryLine(string entryLineChargeType, string invoiceLineAmountProp)
		{
			JobDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			Line.JI_Tariff = "0101.1000/25";
			Line.JI_LinePrice = 1m;
			var line2 = Header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0101.1000/25";
			line2.JI_LinePrice = 2m;
			var line3 = Header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0101.1000/25";
			line3.JI_LinePrice = 3m;

			AssertEquals("PreCondition : Invoice Line Count", 3, JobDec.InvoiceLines.Count);
			AssertEquals("PreCondition : Invoice Line Count", 3, JobDec.FilteredInvoiceLines.Count);

			Header.JZ_InvoiceAmount = 30m;
			Header.JZ_RX_NKInvoice_Currency = "AUD";
			JobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDec.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			JobDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobDec.DoMerge();
			AssertEquals("PreCondition : Entries", 1, JobDec.CustomsEntryHeaders.Count);
			AssertEquals("PreCondition : Merged Lines", 1, JobDec.CustomsEntryHeaders[0].MergedLines.Count);
			Line.CusEntryLine.Fees.AddOrUpdate(entryLineChargeType, 30m);

			AssertEquals(invoiceLineAmountProp, 5m, Line[invoiceLineAmountProp]);
			AssertEquals(invoiceLineAmountProp, 10m, line2[invoiceLineAmountProp]);
			AssertEquals(invoiceLineAmountProp, 15m, line3[invoiceLineAmountProp]);
		}

		public void TestDateOfValuationComesFromOverride()
		{
			Line.InvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			AssertEquals(new ZDateTime(2005, 8, 17), Line.DateOfValuation);
		}

		[TestDate(2008, 1, 2)]
		public void TestDateOfValuationWhenNoInvoiceHeader()
		{
			var jobDec2 = Factory.New<JobDeclaration>();
			var line2 = jobDec2.InvoiceLines.AddNew();
			AssertEquals(new ZDateTime(2008, 1, 2), line2.DateOfValuation);
		}

		#endregion

		#region Implementation

		const string TestClassDescription = "Test Class Description";
		const string TestPartDescription = "Test Part Desciption";

		void SetHeaderCurrencies(JobComInvoiceHeader header, ZString currency)
		{
			header.JZ_RX_NKInvoice_Currency = currency;
		}

		OrgAddress GetValidDepotAddress()
		{
			var filter = new ZQuery(OrgHeaderSchema.OH_IsPackDepot, true);
			var header = OrgHeader.New(Factory);
			header.OH_Code = "ValidDepot";
			header.MainAddress.OA_Address1 = "Address1";
			header.OH_IsPackDepot = true;
			return header.Addresses.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
			distributeByForExport?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;
		IDisposable distributeByForExport;
		#endregion
	}
}
