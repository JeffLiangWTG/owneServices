using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class QuarantineJobComInvoiceLineTest : JobComInvoiceLineTest
	{
		public void TestRFPNumbers()
		{
			const string messageError = "At least one RFP Number should be entered for Certificate Request message.";

			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertNoRowMessageErrorContaining(Line, messageError);

			Line.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(Line, messageError);

			Line.Declaration.IsAQISCertificateRequest = true;
			using (Line.GetValidationSuspender())
			{
				Line.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(Line, messageError);
			}

			Line.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(Line, messageError);

			Line.RFPNumbers.AddNew();
			AssertNoRowMessageErrorContaining(Line, messageError);

			Line.RFPNumbers.DeleteAll();
			AssertHasRowMessageErrorContaining(Line, messageError);
		}

		public void TestQuarantineExDocLine()
		{
			AssertNull(Line.QuarantineExDocLine);
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertNotNull(Line.QuarantineExDocLine);
		}

		public void TestJI_CustomsQuantityUpdatesQuarantine()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_NetQuantity.IsEmpty);
			Line.JI_CustomsQuantity = 12.3m;
			AssertEquals("Quarantine Line is updated", 12.3m, Line.QuarantineExDocLine.QL_NetQuantity);
			Line.JI_CustomsQuantity = 34.5m;
			AssertEquals("Quarantine Line has not changed", 34.5m, Line.QuarantineExDocLine.QL_NetQuantity);
		}

		public void TestJI_CustomsUnitQtyUpdatesQuarantine()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_NetQuantityUnit.IsEmpty);
			Line.JI_CustomsUnitQty = "KG";
			AssertEquals("Quarantine Line has mapped code unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, Line.QuarantineExDocLine.QL_NetQuantityUnit);
			Line.QuarantineExDocLine.QL_NetQuantityUnit = ZString.Empty;
			Line.JI_CustomsUnitQty = "BC";
			AssertEquals("Quarantine Line cannot be mapped, defaults to Customs Line QU", "BC", Line.QuarantineExDocLine.QL_NetQuantityUnit);
			Line.JI_CustomsUnitQty = "KG";
			AssertEquals("Quarantine Line has mapped code unit", EXDOCMetricWeightUnitCodes.Codes.Kilogram, Line.QuarantineExDocLine.QL_NetQuantityUnit);
		}

		public void TestJI_InvoiceQuantityUpdatesQuarantine()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_OuterPackCount.IsEmpty);
			Line.JI_InvoiceQuantity = 666;
			AssertEquals("Quarantine Line is updated", 666, Line.QuarantineExDocLine.QL_OuterPackCount);
			Line.JI_InvoiceQuantity = 34.5m;
			AssertEquals("Quarantine Line is updated", 35, Line.QuarantineExDocLine.QL_OuterPackCount);
			Line.JI_InvoiceQuantity = 23.4m;
			AssertEquals("Quarantine Line is updated", 23, Line.QuarantineExDocLine.QL_OuterPackCount);
			Line.JI_InvoiceQuantity = 28.7m;
			AssertEquals("Quarantine Line is updated", 29, Line.QuarantineExDocLine.QL_OuterPackCount);
		}

		public void TestJI_InvoiceUQUpdatesQuarantine()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobDec.JE_RL_NKFinalDestination = "";
			Line.JI_InvoiceUQ = "PCE";
			AssertEquals("QL_OuterPackType remains empty when final destination is not set", ZString.Empty, Line.QuarantineExDocLine.QL_OuterPackType);
			Line.JI_InvoiceUQ = "PCS";
			AssertEquals("QL_OuterPackType remains empty when final destination is not set", ZString.Empty, Line.QuarantineExDocLine.QL_OuterPackType);

			JobDec.JE_RL_NKFinalDestination = "FRMRS";
			Line.QuarantineExDocLine.QL_OuterPackType = ZString.Empty;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_OuterPackType.IsEmpty);
			Line.JI_InvoiceUQ = "PCE";
			AssertEquals("QL_OuterPackType is updated for EU destination", EXDOCPacakgeTypeCodes.Codes.Piece, Line.QuarantineExDocLine.QL_OuterPackType);
			Line.QuarantineExDocLine.QL_OuterPackType = ZString.Empty;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_OuterPackType.IsEmpty);
			Line.JI_InvoiceUQ = "PCS";
			AssertEquals("QL_OuterPackType is updated for EU destination", EXDOCPacakgeTypeCodes.Codes.Piece, Line.QuarantineExDocLine.QL_OuterPackType);

			JobDec.JE_RL_NKFinalDestination = "AUBNE";
			Line.QuarantineExDocLine.QL_OuterPackType = ZString.Empty;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_OuterPackType.IsEmpty);
			Line.JI_InvoiceUQ = "PCE";
			AssertEquals("QL_OuterPackType is updated for non EU and Turkey destination", EXDOCPacakgeTypeCodes.Codes.Pieces, Line.QuarantineExDocLine.QL_OuterPackType);
			Line.QuarantineExDocLine.QL_OuterPackType = ZString.Empty;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_OuterPackType.IsEmpty);
			Line.JI_InvoiceUQ = "PCS";
			AssertEquals("QL_OuterPackType is updated for non EU and Turkey destination", EXDOCPacakgeTypeCodes.Codes.Pieces, Line.QuarantineExDocLine.QL_OuterPackType);

			JobDec.JE_RL_NKFinalDestination = "TRIST";
			Line.QuarantineExDocLine.QL_OuterPackType = ZString.Empty;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_OuterPackType.IsEmpty);
			Line.JI_InvoiceUQ = "PCE";
			AssertEquals("QL_OuterPackType is updated for Turkey destination", EXDOCPacakgeTypeCodes.Codes.Piece, Line.QuarantineExDocLine.QL_OuterPackType);
			Line.QuarantineExDocLine.QL_OuterPackType = ZString.Empty;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_OuterPackType.IsEmpty);
			Line.JI_InvoiceUQ = "PCS";
			AssertEquals("QL_OuterPackType is updated for Turkey destination", EXDOCPacakgeTypeCodes.Codes.Piece, Line.QuarantineExDocLine.QL_OuterPackType);
		}

		public void TestJI_WeightUpdatesQuarantine()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_GrossMetricWeight.IsEmpty);
			Line.JI_Weight = 12.4m;
			AssertEquals("Quarantine Line is updated", 12.4m, Line.QuarantineExDocLine.QL_GrossMetricWeight);
			Line.JI_Weight = 14.67m;
			AssertEquals("Quarantine Line has not changed", 14.67m, Line.QuarantineExDocLine.QL_GrossMetricWeight);
		}

		[ExpectNoExceptions]
		public void TestJI_WeightUpdatingQuarantineDoesNotLoop()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertEquals("Pre-Condition", true, Line.QuarantineExDocLine.QL_GrossMetricWeight.IsEmpty);
			Line.JI_Weight = 12.4758m;
			AssertEquals("Quarantine Line is updated with rounded weight", 12.476m, Line.QuarantineExDocLine.QL_GrossMetricWeight);
		}

		public void TestJI_WeightUQUpdatesQuarantine()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertEquals("Pre-Condition, Default values for first header ", EXDOCMetricWeightUnitCodes.Codes.Kilogram, Line.QuarantineExDocLine.QL_GrossMetricWeightUnit);
			Line.JI_WeightUQ = "LB";
			AssertEquals("Quarantine Line cannot be mapped", ZString.Empty, Line.QuarantineExDocLine.QL_GrossMetricWeightUnit);
			Line.JI_WeightUQ = "G";
			AssertEquals("Quarantine Line has mapped code unit", EXDOCMetricWeightUnitCodes.Codes.Gram, Line.QuarantineExDocLine.QL_GrossMetricWeightUnit);
		}

		public void TestQuarantineExDocHeaderCreation()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var jobComInvoiceHeader = JobDec.Invoices.AddNew();
			var jobComInvoiceLine = jobComInvoiceHeader.JobComInvoiceLines.AddNew();
			var quarantineExDocLine = jobComInvoiceLine.QuarantineExDocLine;
			Factory.Save();
			AssertEquals("Is registered editable child", true, jobComInvoiceLine.IsRegisteredEditableChildObject(quarantineExDocLine));
			AssertEquals("Quarantine Exdoc Line is in database", true, quarantineExDocLine.IsInDatabase);
			var newFactory = new BusinessObjectFactory();
			var jobComInvoiceLineReload = newFactory.Load<JobComInvoiceLine>(jobComInvoiceLine.PK);
			AssertEquals("Same QurantineExdocLine is loaded as created", quarantineExDocLine.PK, jobComInvoiceLineReload.QuarantineExDocLine.PK);
		}

		public void TestEXDOCAMLCPerformanceNumber()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = JobDec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("The EXDOC AMLC Performance Number should be empty as there is no supplier", true, invoiceLine.EXDOCAMLCPerformanceNumber.IsEmpty);
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			JobDec.JE_OH_Supplier = supplier.PK;
			AssertEquals("The EXDOC AMLC Performance Number should be empty as the supplier entered doesn't have one", true, invoiceLine.EXDOCAMLCPerformanceNumber.IsEmpty);
			var exportNum = supplier.CustomsCodes.AddNew();
			exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber;
			exportNum.OK_RN_NKCodeCountry = "AU";
			exportNum.OK_OH = supplier.PK;
			exportNum.OK_CustomsRegNo = "12345";
			AssertEquals("The EXDOC AMLC Performance Number has been entered", "12345", invoiceLine.EXDOCAMLCPerformanceNumber);
		}

		public void TestDefaultAQSDatafromProducttoInvoiceLine()
		{
			var organisations = Factory.Load<OrgHeader>(new ZQuery() { MaximumRows = 2 });

			var exportClass = Factory.New<Classification>();
			exportClass.CC_ClassificationType = JobDeclaration.ClassificationType.EXP;
			exportClass.CC_LookupCode = "XXX";
			exportClass.CC_TariffNum = "1111.11.11";

			var part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "XXX111";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = organisations[0].PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_CC = exportClass.PK;
			pivot.AddInfo.ZA_AQISProduceType_Hidden = "MEA";
			pivot.AddInfo.ZA_AQISProduct_Hidden = "A";
			pivot.AddInfo.ZA_AQISSupplementaryCode_Hidden = "BB";
			pivot.AddInfo.ZA_AQISPackType_Hidden = "BP";
			pivot.AddInfo.ZA_AQISPreservation_Hidden = "C";
			pivot.AddInfo.ZA_AQISCutCode_Hidden = "1000";
			pivot.AddInfo.ZA_AQISCategoryCode_Hidden = "CAT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.JE_OH_Importer = organisations[0].PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(invoiceLine.Pivot);
			AssertEquals("Product", "A", invoiceLine.QuarantineExDocLine.QL_ProductType);
			AssertEquals("Product", "BB", invoiceLine.QuarantineExDocLine.QL_SupplimentaryCode);
			AssertEquals("Product", "BP", invoiceLine.QuarantineExDocLine.QL_PackType);
			AssertEquals("Product", "C", invoiceLine.QuarantineExDocLine.QL_PreservationType);
			AssertEquals("Product", "1000", invoiceLine.QuarantineExDocLine.QL_CutCode);
			AssertEquals("Category", "CAT", invoiceLine.QuarantineExDocLine.QL_Category);
		}

		public void TestDefaultDescriptionNotUsedWhenJobIsQuarantine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND."
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, exportCustomsUQ);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Line.JI_Tariff = ZString.Empty;
				JobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				Line.JI_Tariff = ExportTariffNum;
				AssertEquals("Description is defaulted", "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND.", Line.JI_Description);

				Line.JI_Tariff = ZString.Empty;
				JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				Line.JI_Description = ZString.Empty;
				Line.JI_Tariff = ExportTariffNum;
				AssertEquals("Description should still be empty", true, Line.JI_Description.IsEmpty);

				Line.JI_Tariff = ZString.Empty;
				JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				Line.JI_Description = "DESCRIPTION SET BY SCOTT FOR TESTING";
				Line.JI_Tariff = ExportTariffNum;
				AssertEquals("Description is the same", "DESCRIPTION SET BY SCOTT FOR TESTING", Line.JI_Description);
			}
		}

		public void TestDefaultDescriptionNotUsedWhenJobIsQuarantine_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AHECC.UA_LongDescription = "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND.";

				Line.JI_Tariff = ZString.Empty;
				JobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				Line.JI_Tariff = ExportTariffNum;
				AssertEquals("Description is defaulted", "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND.", Line.JI_Description);

				Line.JI_Tariff = ZString.Empty;
				JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				Line.JI_Description = ZString.Empty;
				Line.JI_Tariff = ExportTariffNum;
				AssertEquals("Description should still be empty", true, Line.JI_Description.IsEmpty);

				Line.JI_Tariff = ZString.Empty;
				JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				Line.JI_Description = "DESCRIPTION SET BY SCOTT FOR TESTING";
				Line.JI_Tariff = ExportTariffNum;
				AssertEquals("Description is the same", "DESCRIPTION SET BY SCOTT FOR TESTING", Line.JI_Description);
			}
		}

		public void TestCopyDescriptionToQuarantineLine()
		{
			JobDec.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			Line.JI_Description = "Testing 1234567890123456789012345678901234567890123456789012345678901234567890";
			Assert("QL_MeatInspectionDescription.IsEmpty", Line.QuarantineExDocLine.QL_MeatInspectionDescription.IsEmpty);

			Line.CopyDescriptionToQuarantineLine();
			AssertEquals("Copies JI_Description to QL_MeatInspectionDescription", "Testing 12345678901234567890123456789012345678901234567890123456789012", Line.QuarantineExDocLine.QL_MeatInspectionDescription);

			JobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Line.JI_Description = "Testing Again";
			Line.CopyDescriptionToQuarantineLine();
			AssertEquals("Does NOT Copy JI_Description to QL_MeatInspectionDescription", "Testing 12345678901234567890123456789012345678901234567890123456789012", Line.QuarantineExDocLine.QL_MeatInspectionDescription);
		}

		public void TestCreateContainerFromAddInfo_PersistentDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line1.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304711";
			line1.AddInfo.ZA_AQISTempContainerSeal_Hidden = "SEAL001";
			var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line2.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304711";
			line2.AddInfo.ZA_AQISTempContainerSeal_Hidden = "SEAL001";
			var line3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line3.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304712";
			var line4 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line4.AddInfo.ZA_AQISTempContainerNumber_Hidden = "";
			Factory.Save();

			line1.CreateContainerFromAddInfo();
			line2.CreateContainerFromAddInfo();
			line3.CreateContainerFromAddInfo();
			line4.CreateContainerFromAddInfo();

			AssertEquals("Created 2 containers", 2, declaration.AUCusContainers.Count);

			var container1 = declaration.AUCusContainers.Find("MAEU9304711");
			AssertNotNull("container MAEU9304711", container1);
			AssertEquals("SEAL001", container1.SealNumberForBinding);
			var container2 = declaration.AUCusContainers.Find("MAEU9304712");
			AssertNotNull("container MAEU9304712", container2);
			AssertEquals("", container2.SealNumberForBinding);

			AssertEquals("Container1 linked to line 1", true, line1.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 not linked to line 1", false, line1.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			AssertEquals("Container1 linked to line 2", true, line2.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 not linked to line 2", false, line2.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			AssertEquals("Container1 not linked to line 3", false, line3.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 linked to line 3", true, line3.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			AssertEquals("Container1 not linked to line 4", false, line4.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 not linked to line 4", false, line4.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			Assert(declaration.HasChanges);
			Assert(declaration.AUCusContainers.HasChanges);
			Assert(container1.HasChanges);
			Assert(container2.HasChanges);
			Assert(line1.ContainersPivot.HasChanges);
			Assert(line1.ContainersPivot[0].HasChanges);
			Assert(line2.ContainersPivot.HasChanges);
			Assert(line2.ContainersPivot[0].HasChanges);
			Assert(line3.ContainersPivot.HasChanges);
			Assert(line3.ContainersPivot[0].HasChanges);
			Assert(!line4.ContainersPivot.HasChanges);
			Assert(!line4.ContainersPivot.Any());

			Factory.Save();

			AssertEquals("", line1.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("", line1.AddInfo.ZA_AQISTempContainerSeal_Hidden);
			AssertEquals("", line2.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("", line2.AddInfo.ZA_AQISTempContainerSeal_Hidden);
			AssertEquals("", line3.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("", line3.AddInfo.ZA_AQISTempContainerSeal_Hidden);
			AssertEquals("", line4.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("", line4.AddInfo.ZA_AQISTempContainerSeal_Hidden);
		}

		public void TestCreateContainerFromAddInfo_NonPersistentDeclaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var fakeDeclaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			fakeDeclaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line1.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304711";
			line1.AddInfo.ZA_AQISTempContainerSeal_Hidden = "SEAL001";
			var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line2.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304711";
			line2.AddInfo.ZA_AQISTempContainerSeal_Hidden = "SEAL001";
			var line3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line3.AddInfo.ZA_AQISTempContainerNumber_Hidden = "MAEU9304712";
			var line4 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line4.AddInfo.ZA_AQISTempContainerNumber_Hidden = "";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedInvoice = factory2.Load<JobComInvoiceHeader>(invoice.PK);
			var otherFakeDeclaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(reloadedInvoice).HeaderData;
			otherFakeDeclaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			var reloadedLine1 = (JobComInvoiceLine)reloadedInvoice.InvoiceLines.Find(new ZQuery(JobComInvoiceLineSchema.JI_LineNo, (short)1)).FirstOrDefault();
			var reloadedLine2 = (JobComInvoiceLine)reloadedInvoice.InvoiceLines.Find(new ZQuery(JobComInvoiceLineSchema.JI_LineNo, (short)2)).FirstOrDefault();
			var reloadedLine3 = (JobComInvoiceLine)reloadedInvoice.InvoiceLines.Find(new ZQuery(JobComInvoiceLineSchema.JI_LineNo, (short)3)).FirstOrDefault();
			var reloadedLine4 = (JobComInvoiceLine)reloadedInvoice.InvoiceLines.Find(new ZQuery(JobComInvoiceLineSchema.JI_LineNo, (short)4)).FirstOrDefault();
			Assert(!reloadedLine1.ContainersPivot.HasChanges);
			Assert(!reloadedLine2.ContainersPivot.HasChanges);
			Assert(!reloadedLine3.ContainersPivot.HasChanges);
			Assert(!reloadedLine4.ContainersPivot.HasChanges);
			reloadedLine1.CreateContainerFromAddInfo();
			reloadedLine2.CreateContainerFromAddInfo();
			reloadedLine3.CreateContainerFromAddInfo();
			reloadedLine4.CreateContainerFromAddInfo();
			Assert(!reloadedLine1.ContainersPivot.HasChanges);
			Assert(!reloadedLine1.ContainersPivot[0].HasChanges);
			Assert(!reloadedLine2.ContainersPivot.HasChanges);
			Assert(!reloadedLine2.ContainersPivot[0].HasChanges);
			Assert(!reloadedLine3.ContainersPivot.HasChanges);
			Assert(!reloadedLine3.ContainersPivot[0].HasChanges);
			Assert(!reloadedLine4.ContainersPivot.HasChanges);
			Assert(!reloadedLine4.ContainersPivot.Any());

			AssertEquals("Created 2 containers", 2, otherFakeDeclaration.AUCusContainers.Count);

			var container1 = otherFakeDeclaration.AUCusContainers.Find("MAEU9304711");
			AssertNotNull("container MAEU9304711", container1);
			AssertEquals("SEAL001", container1.SealNumberForBinding);
			var container2 = otherFakeDeclaration.AUCusContainers.Find("MAEU9304712");
			AssertNotNull("container MAEU9304712", container2);
			AssertEquals("", container2.SealNumberForBinding);

			AssertEquals("Container1 linked to line 1", true, reloadedLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 not linked to line 1", false, reloadedLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			AssertEquals("Container1 linked to line 2", true, reloadedLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 not linked to line 2", false, reloadedLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			AssertEquals("Container1 not linked to line 3", false, reloadedLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 linked to line 3", true, reloadedLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			AssertEquals("Container1 not linked to line 4", false, reloadedLine4.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 not linked to line 4", false, reloadedLine4.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine);

			factory2.Save();

			AssertEquals("MAEU9304711", reloadedLine1.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("SEAL001", reloadedLine1.AddInfo.ZA_AQISTempContainerSeal_Hidden);
			AssertEquals("MAEU9304711", reloadedLine2.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("SEAL001", reloadedLine2.AddInfo.ZA_AQISTempContainerSeal_Hidden);
			AssertEquals("MAEU9304712", reloadedLine3.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("", reloadedLine3.AddInfo.ZA_AQISTempContainerSeal_Hidden);
			AssertEquals("", reloadedLine4.AddInfo.ZA_AQISTempContainerNumber_Hidden);
			AssertEquals("", reloadedLine4.AddInfo.ZA_AQISTempContainerSeal_Hidden);
		}
	}
}
