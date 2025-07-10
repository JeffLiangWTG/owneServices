using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class CMREntryCreationStrategyTest : TestCaseWithFactory
	{
		class TwoWarehouseExWarehouseEntryCreator : MergedDeclarationCreator2Line
		{
			public TwoWarehouseExWarehouseEntryCreator(BusinessObjectFactory factory, ZDateTime? systemCreateDate = null)
				: base(factory)
			{
				this.systemCreateDate = systemCreateDate;
			}
			readonly ZDateTime? systemCreateDate;

			protected override void SetupDeclarationPreMerge(JobDeclaration declaration)
			{
				base.SetupDeclarationPreMerge(declaration);
				fDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				if (systemCreateDate.HasValue)
				{
					declaration.JE_SystemCreateTimeUtc = systemCreateDate.Value;
				}

				InvoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
				InvoiceLine1.AddInfo.WarehouseAddress.LocalControlledPremisesID = "ccp1";
				fDeclaration.WarehouseDocAddress.E2_OA_Address = InvoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden;
				InvoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
				InvoiceLine2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "ccp2";

				declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
				InvoiceLine3.AddInfo.ZA_OA_WarehouseAddress_Hidden = InvoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden;

				declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			}

			public JobComInvoiceLine InvoiceLine3
			{
				get { return Declaration.InvoiceLines[2]; }
			}

			public JobComInvoiceLine InvoiceLine4
			{
				get { return Declaration.InvoiceLines[3]; }
			}

			public CusEntryHeader Entry2
			{
				get { return Declaration.CustomsEntryHeaders[1]; }
			}

			public new CusEntryLine EntryLine2
			{
				get { return Declaration.CustomsEntryHeaders[1].MergedLines[0]; }
			}

			public CusEntryLine EntryLine3
			{
				get { return Declaration.CustomsEntryHeaders[1].MergedLines[1]; }
			}

			public CusEntryLine EntryLine4
			{
				get { return Declaration.CustomsEntryHeaders[0].MergedLines[1]; }
			}
		}

		public void TestLineWithDifferentProductDoNotMergeForAutomation()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			org.OH_IsConsignee = true;
			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = "IMP";
			classification.CC_TariffNum = "0000000011";
			var part1 = Factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "PART324";
			part1.AddNewImportPivotWithClassification(classification.PK);
			part1.RelatedOrganisations.AddOwner(org);
			var part2 = Factory.New<AUOrgSupplierPart>();
			part2.OP_PartNum = "PART324";
			part2.AddNewImportPivotWithClassification(classification.PK);
			part2.RelatedOrganisations.AddOwner(org);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice1Line1A = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1A.JI_PartNo = "PART324";
			invoice1Line1A.JI_OP = part1.PK;
			invoice1Line1A.JI_InvoiceQuantity = 100m;
			invoice1Line1A.JI_CustomsQuantity = 100m;
			invoice1Line1A.JI_IsPackToBondForLine = true;
			var invoice1Line2A = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2A.JI_PartNo = "PART324";
			invoice1Line2A.JI_OP = part2.PK;
			invoice1Line2A.JI_InvoiceQuantity = 200m;
			invoice1Line2A.JI_CustomsQuantity = 200m;
			invoice1Line2A.JI_IsPackToBondForLine = true;
			var invoice2Line1A = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1A.JI_PartNo = "PART324";
			invoice2Line1A.JI_OP = part2.PK;
			invoice2Line1A.JI_InvoiceQuantity = 400m;
			invoice2Line1A.JI_CustomsQuantity = 400m;
			invoice2Line1A.JI_IsPackToBondForLine = true;
			var invoice2Line2A = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2A.JI_PartNo = "PART324";
			invoice2Line2A.JI_OP = ZGuid.Empty;
			invoice2Line2A.JI_InvoiceQuantity = 800m;
			invoice2Line2A.JI_CustomsQuantity = 800m;
			invoice2Line2A.JI_IsPackToBondForLine = true;
			var invoice1Line1B = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1B.JI_PartNo = "PART324";
			invoice1Line1B.JI_OP = part1.PK;
			invoice1Line1B.JI_InvoiceQuantity = 150m;
			invoice1Line1B.JI_CustomsQuantity = 150m;
			invoice1Line1B.JI_IsPackToBondForLine = false;
			var invoice1Line2B = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2B.JI_PartNo = "PART324";
			invoice1Line2B.JI_OP = part2.PK;
			invoice1Line2B.JI_InvoiceQuantity = 250m;
			invoice1Line2B.JI_CustomsQuantity = 250m;
			invoice1Line2B.JI_IsPackToBondForLine = false;
			var invoice2Line1B = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1B.JI_PartNo = "PART324";
			invoice2Line1B.JI_OP = part2.PK;
			invoice2Line1B.JI_InvoiceQuantity = 450m;
			invoice2Line1B.JI_CustomsQuantity = 450m;
			invoice2Line1B.JI_IsPackToBondForLine = false;
			var invoice2Line2B = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2B.JI_PartNo = "PART324";
			invoice2Line2B.JI_OP = ZGuid.Empty;
			invoice2Line2B.JI_InvoiceQuantity = 850m;
			invoice2Line2B.JI_CustomsQuantity = 850m;
			invoice2Line2B.JI_IsPackToBondForLine = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(4, entry.MergedLines.Count);
			var entryLine1 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 100m);
			var entryLine2 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 600m);
			var entryLine3 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 800m);
			var entryLine4 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 1700m);
			AssertEquals(entryLine1, invoice1Line1A.CusEntryLine);
			AssertEquals(entryLine2, invoice2Line1A.CusEntryLine);
			AssertEquals(entryLine2, invoice1Line2A.CusEntryLine);
			AssertEquals(entryLine3, invoice2Line2A.CusEntryLine);
			AssertEquals(entryLine4, invoice1Line1B.CusEntryLine);
			AssertEquals(entryLine4, invoice2Line1B.CusEntryLine);
			AssertEquals(entryLine4, invoice1Line2B.CusEntryLine);
			AssertEquals(entryLine4, invoice2Line2B.CusEntryLine);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(6, entry.MergedLines.Count);
			entryLine1 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 100m);
			entryLine2 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 600m);
			entryLine3 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 800m);
			entryLine4 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 150m);
			var entryLine5 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 700m);
			var entryLine6 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CustomsQuantity == 850m);
			AssertEquals(entryLine1, invoice1Line1A.CusEntryLine);
			AssertEquals(entryLine2, invoice2Line1A.CusEntryLine);
			AssertEquals(entryLine2, invoice1Line2A.CusEntryLine);
			AssertEquals(entryLine3, invoice2Line2A.CusEntryLine);
			AssertEquals(entryLine4, invoice1Line1B.CusEntryLine);
			AssertEquals(entryLine5, invoice2Line1B.CusEntryLine);
			AssertEquals(entryLine5, invoice1Line2B.CusEntryLine);
			AssertEquals(entryLine6, invoice2Line2B.CusEntryLine);
		}

		public void TestLineWithDifferentInvoiceQtyToCustomsQtyRatioShouldBeMergedBasedOnRatio()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			org.OH_IsConsignee = true;
			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = "IMP";
			classification.CC_TariffNum = "10101010";
			var part1 = Factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "PART324";
			part1.AddNewImportPivotWithClassification(classification.PK);
			part1.RelatedOrganisations.AddOwner(org);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_PartNo = "PART324";
			invoice1Line1.JI_OP = part1.PK;
			invoice1Line1.JI_Tariff = "10101010";
			invoice1Line1.JI_InvoiceQuantity = 100m;
			invoice1Line1.JI_CustomsQuantity = 200m;
			invoice1Line1.AddInfo.ZA_IsPackToBondForLine_Hidden = "";
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_PartNo = "PART324";
			invoice1Line2.JI_OP = part1.PK;
			invoice1Line2.JI_Tariff = "10101010";
			invoice1Line2.JI_InvoiceQuantity = 100m;
			invoice1Line2.JI_CustomsQuantity = 300m;
			invoice1Line2.AddInfo.ZA_IsPackToBondForLine_Hidden = "";
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_PartNo = "PART324";
			invoice2Line1.JI_OP = part1.PK;
			invoice2Line1.JI_Tariff = "10101010";
			invoice2Line1.JI_InvoiceQuantity = 200m;
			invoice2Line1.JI_CustomsQuantity = 400m;
			invoice2Line1.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			var invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_PartNo = "PART324";
			invoice2Line2.JI_OP = part1.PK;
			invoice2Line2.JI_Tariff = "10101010";
			invoice2Line2.JI_InvoiceQuantity = 200m;
			invoice2Line2.JI_CustomsQuantity = 600m;
			invoice2Line2.AddInfo.ZA_IsPackToBondForLine_Hidden = "N";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
			var entryLine1 = entry.MergedLines[0];
			AssertEquals(1500m, entryLine1.CustomsQuantity);
			AssertEquals(entryLine1, invoice1Line1.CusEntryLine);
			AssertEquals(entryLine1, invoice2Line1.CusEntryLine);
			AssertEquals(entryLine1, invoice1Line2.CusEntryLine);
			AssertEquals(entryLine1, invoice2Line2.CusEntryLine);

			invoice1Line1.JI_IsPackToBondForLine = ZBool.True;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			if (entryLine1.CustomsQuantity == 1300m)
			{
				entryLine1 = entry.MergedLines[1];
				entryLine2 = entry.MergedLines[0];
			}
			AssertEquals(200m, entryLine1.CustomsQuantity);
			AssertEquals(1300m, entryLine2.CustomsQuantity);
			AssertEquals(entryLine1, invoice1Line1.CusEntryLine);
			AssertEquals(entryLine2, invoice2Line1.CusEntryLine);
			AssertEquals(entryLine2, invoice1Line2.CusEntryLine);
			AssertEquals(entryLine2, invoice2Line2.CusEntryLine);

			invoice1Line2.JI_IsPackToBondForLine = ZBool.True;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(3, entry.MergedLines.Count);
			entryLine1 = invoice1Line1.CusEntryLine;
			entryLine2 = invoice2Line1.CusEntryLine;
			AssertEquals(entryLine2, invoice2Line2.CusEntryLine);
			var entryLine3 = invoice1Line2.CusEntryLine;
			AssertNotEquals(entryLine1, entryLine2);
			AssertNotEquals(entryLine1, entryLine3);
			AssertNotEquals(entryLine3, entryLine2);
			AssertCollectionContains(entryLine1, entry.MergedLines);
			AssertCollectionContains(entryLine2, entry.MergedLines);
			AssertCollectionContains(entryLine3, entry.MergedLines);
			AssertEquals(200m, entryLine1.CustomsQuantity);
			AssertEquals(1000m, entryLine2.CustomsQuantity);
			AssertEquals(300m, entryLine3.CustomsQuantity);

			invoice2Line1.JI_IsPackToBondForLine = ZBool.True;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(3, entry.MergedLines.Count);
			entryLine1 = invoice1Line1.CusEntryLine;
			entryLine2 = invoice1Line2.CusEntryLine;
			AssertEquals(entryLine1, invoice2Line1.CusEntryLine);
			entryLine3 = invoice2Line2.CusEntryLine;
			AssertNotEquals(entryLine1, entryLine2);
			AssertNotEquals(entryLine1, entryLine3);
			AssertNotEquals(entryLine3, entryLine2);
			AssertCollectionContains(entryLine1, entry.MergedLines);
			AssertCollectionContains(entryLine2, entry.MergedLines);
			AssertCollectionContains(entryLine3, entry.MergedLines);
			AssertEquals(600m, entryLine1.CustomsQuantity);
			AssertEquals(300m, entryLine2.CustomsQuantity);
			AssertEquals(600m, entryLine3.CustomsQuantity);

			invoice2Line2.JI_IsPackToBondForLine = ZBool.True;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			var existingLines = new List<CusEntryLine>(new[] { entryLine1, entryLine2, entryLine3 });
			entryLine1 = invoice1Line1.CusEntryLine;
			entryLine2 = invoice1Line2.CusEntryLine;
			if (entryLine1.CustomsQuantity == 900m)
			{
				entryLine1 = entry.MergedLines[1];
				entryLine2 = entry.MergedLines[0];
			}
			existingLines.Remove(entryLine1);
			existingLines.Remove(entryLine2);
			AssertEquals(600m, entryLine1.CustomsQuantity);
			AssertEquals(900m, entryLine2.CustomsQuantity);
			AssertEquals(entryLine1, invoice1Line1.CusEntryLine);
			AssertEquals(entryLine1, invoice2Line1.CusEntryLine);
			AssertEquals(entryLine2, invoice1Line2.CusEntryLine);
			AssertEquals(entryLine2, invoice2Line2.CusEntryLine);
			AssertEquals(1, existingLines.Count);
			AssertEquals(true, existingLines[0].IsDeleted);

			invoice1Line1.JI_IsPackToBondForLine = ZBool.False;
			invoice2Line1.JI_IsPackToBondForLine = ZBool.False;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			entryLine1 = invoice1Line1.CusEntryLine;
			entryLine2 = invoice1Line2.CusEntryLine;
			if (entryLine1.CustomsQuantity == 900m)
			{
				entryLine1 = entry.MergedLines[1];
				entryLine2 = entry.MergedLines[0];
			}
			existingLines.Remove(entryLine1);
			existingLines.Remove(entryLine2);
			AssertEquals(600m, entryLine1.CustomsQuantity);
			AssertEquals(900m, entryLine2.CustomsQuantity);
			AssertEquals(entryLine1, invoice1Line1.CusEntryLine);
			AssertEquals(entryLine1, invoice2Line1.CusEntryLine);
			AssertEquals(entryLine2, invoice1Line2.CusEntryLine);
			AssertEquals(entryLine2, invoice2Line2.CusEntryLine);
		}

		public void TestInvoiceLinesWithSameClassButDifferentDefaultAnswersNotMergedTogether()
		{
			var dutyDate = new ZDateTime(2010, 1, 1);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			var profile1 = CMRCommunityProtectionProfile.New(Factory);
			profile1.CP_TariffClassificationNumberfield = "00000000";
			profile1.CP_CommunityProtectionRiskIdentifier = 400;
			var risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dutyDate;
			risk1.CK_LodgementQuestionIdentifier = 4001;
			var question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 4001;
			question1.CQ_LodgementQuestionStartDate = dutyDate;

			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			classification.CC_LookupCode = "LookupCode";
			classification.CC_TariffNum = "0000.00.00";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.GenerateQuestion();
			AssertEquals("1 question generated", 1, classification.Questions.Count);

			var part1 = Factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var pivot1 = part1.AddNewImportPivotWithClassification(classification.PK);
			pivot1.GenerateQuestion();
			AssertEquals("1 question generated", 1, pivot1.Questions.Count);
			pivot1.Questions[0].ON_AnswerCode = "Y";

			var part2 = Factory.New<AUOrgSupplierPart>();
			part2.OP_PartNum = "PART2";
			var pivot2 = part2.AddNewImportPivotWithClassification(classification.PK);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot2.GenerateQuestion();
			AssertEquals("1 question generated", 1, pivot2.Questions.Count);
			pivot2.Questions[0].ON_AnswerCode = "N";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART2";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals("Should not merge", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
		}

		public void TestInvoiceLinesWithSameClassAndSameDefaultAnswersDoMergedTogether()
		{
			var dutyDate = new ZDateTime(2010, 1, 1);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			var profile1 = CMRCommunityProtectionProfile.New(Factory);
			profile1.CP_TariffClassificationNumberfield = "00000000";
			profile1.CP_CommunityProtectionRiskIdentifier = 400;
			var risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dutyDate;
			risk1.CK_LodgementQuestionIdentifier = 4001;
			var question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 4001;
			question1.CQ_LodgementQuestionStartDate = dutyDate;

			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			classification.CC_LookupCode = "LookupCode";
			classification.CC_TariffNum = "0000.00.00";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.GenerateQuestion();
			AssertEquals("1 question generated", 1, classification.Questions.Count);

			//AUOrgSupplierPart part1 = Factory.New<AUOrgSupplierPart>();
			//part1.OP_CI_ImportCC = classification.PK;
			//part1.OP_PartNum = "PART1";
			//part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			//part1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			//part1.GenerateQuestion();
			//AssertEquals("1 question generated", 1, part1.Questions.Count);
			//part1.Questions[0].ON_AnswerCode = "Y";

			//AUOrgSupplierPart part2 = Factory.New<AUOrgSupplierPart>();
			//part2.OP_CI_ImportCC = classification.PK;
			//part2.OP_PartNum = "PART2";
			//part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			//part2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			//part2.GenerateQuestion();
			//AssertEquals("1 question generated", 1, part2.Questions.Count);
			//part2.Questions[0].ON_AnswerCode = "Y";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART2";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should merge", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
		}

		public void TestInvoiceLinesWithAmbiguousAnswersDoNotMergedTogether()
		{
			var dutyDate = new ZDateTime(2010, 1, 1);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			var profile1 = CMRCommunityProtectionProfile.New(Factory);
			profile1.CP_TariffClassificationNumberfield = "00000000";
			profile1.CP_CommunityProtectionRiskIdentifier = 400;
			var risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dutyDate;
			risk1.CK_LodgementQuestionIdentifier = 4001;
			var question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 4001;
			question1.CQ_LodgementQuestionStartDate = dutyDate;

			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			classification.CC_LookupCode = "LookupCode";
			classification.CC_TariffNum = "0000.00.00";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.GenerateQuestion();
			classification.Questions[0].ON_AnswerCode = "N";
			AssertEquals("1 question generated", 1, classification.Questions.Count);

			var part1 = Factory.New<AUOrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			var pivot1 = part1.AddNewImportPivotWithClassification(classification.PK);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot1.GenerateQuestion();
			AssertEquals("1 question generated", 1, pivot1.Questions.Count);
			pivot1.Questions[0].ON_AnswerCode = "Y";

			var part2 = Factory.New<AUOrgSupplierPart>();
			part2.OP_PartNum = "PART2";
			var pivot2 = part2.AddNewImportPivotWithClassification(classification.PK);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot2.GenerateQuestion();
			AssertEquals("1 question generated", 1, pivot2.Questions.Count);
			pivot2.Questions[0].ON_AnswerCode = "Y";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART2";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals("Should not merge", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
		}

		public void TestInvoiceLinesWithTILVAndWithoutTILVNotMergedTogether()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_TILV = "0.00AUD";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_TILV = "30.00AUD";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0000.00.00 00";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("TILV amount does not matter for merging", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals("But existence matters and should not merge together", invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);
		}

		public void TestWithDifferentWarehouseAddressOnLinesOfN30()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			var creator = new TwoWarehouseExWarehouseEntryCreator(Factory, ZDateTime.Now);
			AssertEquals("Customs entry header count", 2, creator.Declaration.CustomsEntryHeaders.Count);
			var entryA = creator.Entry1.WarehouseCCP == "CCP1" ? creator.Entry1 : creator.Entry2;
			var entryB = creator.Entry1.WarehouseCCP == "CCP2" ? creator.Entry1 : creator.Entry2;
			AssertEquals("CCP1", entryA.WarehouseCCP.ToUpper());
			AssertEquals("CCP2", entryB.WarehouseCCP.ToUpper());

			var lineA = creator.InvoiceLine1.CusEntryLine;
			var lineB = creator.InvoiceLine2.CusEntryLine;
			var lineC = creator.InvoiceLine3.CusEntryLine;
			var lineD = creator.InvoiceLine4.CusEntryLine;

			AssertEquals(lineA.Header, entryA);
			AssertEquals(lineB.Header, entryB);
			AssertEquals(lineC.Header, entryB);
			AssertEquals(lineD.Header, entryA);
		}

		class TwoWarehouseN20Creator : TwoWarehouseExWarehouseEntryCreator
		{
			public TwoWarehouseN20Creator(BusinessObjectFactory factory, ZDateTime? systemCreateDate = null)
				: base(factory, systemCreateDate)
			{ }

			protected override void SetupDeclarationPreMerge(JobDeclaration declaration)
			{
				base.SetupDeclarationPreMerge(declaration);
				fDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				InvoiceLine1.JI_IsPackToBondForLine = true;
				InvoiceLine2.JI_IsPackToBondForLine = true;
				InvoiceLine3.JI_IsPackToBondForLine = true;
				InvoiceLine4.JI_IsPackToBondForLine = false;
			}
		}

		public void TestWithDifferentWarehouseAddressOnLinesAndMixOfN10AndN20()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			var creator = new TwoWarehouseN20Creator(Factory, ZDateTime.Now);
			AssertEquals("Customs entry header count", 2, creator.Declaration.CustomsEntryHeaders.Count);
			var entryA = creator.Entry1.WarehouseCCP == "CCP1" ? creator.Entry1 : creator.Entry2;
			var entryB = creator.Entry1.WarehouseCCP == "CCP2" ? creator.Entry1 : creator.Entry2;
			AssertEquals("CCP1", entryA.WarehouseCCP.ToUpper());
			AssertEquals("CCP2", entryB.WarehouseCCP.ToUpper());

			var lineA = creator.InvoiceLine1.CusEntryLine;
			var lineB = creator.InvoiceLine2.CusEntryLine;
			var lineC = creator.InvoiceLine3.CusEntryLine;
			var lineD = creator.InvoiceLine4.CusEntryLine;

			AssertEquals(lineA.Header, entryA);
			AssertEquals(lineB.Header, entryB);
			AssertEquals(lineC.Header, entryB);
			Assert(lineD.Header == entryA || lineD.Header == entryB);
		}

		public void TestMergeWithEmptyDTYButWithSendZeroTicked()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_DTY = 0m;
			invoiceLine1.AddInfo.ZA_SendZeroDutyOverride_Hidden = true;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_DTY = 0m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Key with Invoice1 should have DTY", true, keyWithInvoice1.Contains((ZString)"DTY"));
			AssertEquals("Key with Invoice2 should have DTY", false, keyWithInvoice2.Contains((ZString)"DTY"));

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("Two merged lines", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			invoiceLine2.AddInfo.ZA_SendZeroDutyOverride_Hidden = true;
			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeWithZA_IsNonAQISAEPLineTicked()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_IsNonAQISAEPLine_Hidden = true;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";

			var merger = new LineMerger(declaration);
			var strategy = new CMREntryCreationStrategy(merger);

			merger.DoMerge();
			AssertEquals("Entry lines count", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertNotEquals("IsNonAQISAEPLine should be different in the entry lines",
				declaration.CustomsEntryHeaders[0].AllEntryLines[0].IsNonAQISAEPLine,
				declaration.CustomsEntryHeaders[0].AllEntryLines[1].IsNonAQISAEPLine);

			invoiceLine2.AddInfo.ZA_IsNonAQISAEPLine_Hidden = true;
			merger.DoMerge();
			AssertEquals("Entry lines count", 1, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("Invoice Lines count in the entry line", 2, declaration.CustomsEntryHeaders[0].AllEntryLines[0].InvoiceLines.Count);
			AssertEquals("Shoud be merged by ZA_IsNonAQISAEPLine", true, declaration.CustomsEntryHeaders[0].AllEntryLines[0].RandomLine.AddInfo.ZA_IsNonAQISAEPLine_Hidden);
		}

		public void TestDontMergeInvoiceLinesWithDTYAndWithoutDty()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_DTY = 200m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_DTY = 0m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Key with Invoice1 should have DTY", true, keyWithInvoice1.Contains((ZString)"DTY"));
			AssertEquals("Key with Invoice2 should have DTY", false, keyWithInvoice2.Contains((ZString)"DTY"));

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("Two merged lines", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestDontMergeInvoiceLinesWithSTDAndWithoutDty()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_STD = 200m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_STD = 0m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Key with Invoice1 should have STD", true, keyWithInvoice1.Contains((ZString)"STD"));
			AssertEquals("Key with Invoice2 should have STD", false, keyWithInvoice2.Contains((ZString)"STD"));

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("Two merged lines", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestCanMergeInvoiceLinesWithDTYsWithDifferentAmounts()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_DTY = 200m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_DTY = 100m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForHeader(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForHeader(invoiceLine2);

			AssertEquals("Two keys are the same", true, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestCanMergeInvoiceLinesWithSTDsWithDifferentAmounts()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_STD = 200m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_STD = 100m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForHeader(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForHeader(invoiceLine2);

			AssertEquals("Two keys are the same", true, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestWithTwoInvoicesHavingDifferentDateOfValuation()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 15);

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20000m;

			invoice1.AddInfo.ZA_EFD = "050314";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForHeader(line1);
			var keyWithInvoice2 = strategy.GetKeyForHeader(line2);
			AssertEquals("Two keys are different", false, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Two entry headers created", 2, testDec.CustomsEntryHeaders.Count);
		}

		public void TestWithTwoInvoicesHavingDifferentValuationAdviceNumber()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 15);

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20000m;

			invoice1.AddInfo.ZA_VAN = "123456";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForHeader(line1);
			var keyWithInvoice2 = strategy.GetKeyForHeader(line2);
			AssertEquals("Two keys are different", false, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Two entry headers created", 2, testDec.CustomsEntryHeaders.Count);
		}

		public void TestInvoiceWithChargesCanBeMergedAndNormalised()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 150m, "AUD");

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice2.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 150m, "AUD");

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForHeader1 = strategy.GetKeyForHeader(line1);
			var keyForHeader2 = strategy.GetKeyForHeader(line2);

			AssertEquals("Invoice 1 and Invoice 2 can be merged into one", keyForHeader1, keyForHeader2);
		}

		public void TestInvoicesShouldNotGetMergedIfEffectiveDutyDateAreDifferentForCMR()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.AddInfo.ZA_EFD = "050305";

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.AddInfo.ZA_EFD = "040305";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForHeader1 = strategy.GetKeyForHeader(line1);
			var keyForHeader2 = strategy.GetKeyForHeader(line2);

			Assert("Key for header 1 != key for header2", keyForHeader1 != keyForHeader2);
			AssertEquals("Key1 has Supplier1 Code", true, keyForHeader1.IndexOf(invoice1.AddInfo.ZA_EFD) >= 0);
			AssertEquals("Key2 has Supplier2 Code", true, keyForHeader2.IndexOf(invoice2.AddInfo.ZA_EFD) >= 0);
		}

		public void TestInvoicesShouldNotGetMergedIfDifferentValuationAdviceNumberAreDifferentForCMR()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.AddInfo.ZA_VAN = "X1";

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.AddInfo.ZA_VAN = "X2";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForHeader1 = strategy.GetKeyForHeader(line1);
			var keyForHeader2 = strategy.GetKeyForHeader(line2);

			Assert("Key for header 1 != key for header2", keyForHeader1 != keyForHeader2);
			AssertEquals("Key1 has Supplier1 Code", true, keyForHeader1.IndexOf(invoice1.AddInfo.ZA_VAN) >= 0);
			AssertEquals("Key2 has Supplier2 Code", true, keyForHeader2.IndexOf(invoice2.AddInfo.ZA_VAN) >= 0);
		}

		public void TestLinesShouldNotGetMergedIfSuppliersAreDifferent()
		{
			var supplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var supplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier1.PK));

			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_OH_Supplier = supplier2.PK;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);

			Assert("KeyForLine1 and Line2 should be different", keyForLine1 != keyForLine2);
			AssertEquals("Key for Line1 has Supplier1 Code", true, keyForLine1.IndexOf(supplier1.OH_Code) >= 0);
			AssertEquals("Key for Line2 has Supplier2 Code", true, keyForLine2.IndexOf(supplier2.OH_Code) >= 0);
		}

		public void TestMergingWithDifferentZA_WARAndICSPermits()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.AddInfo.ZA_WAR = "KK234";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfo.ZA_WAR = "KK234";
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.AddInfo.ZA_WAR = "KK234";
			var permit1 = invoiceLine3.ICSPermits.AddNew();
			permit1.CY_Data = "PER1";
			var permit2 = invoiceLine3.ICSPermits.AddNew();
			permit2.CY_Data = "PER2";
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.AddInfo.ZA_WAR = "KK234";
			permit1 = invoiceLine4.ICSPermits.AddNew();
			permit1.CY_Data = "PER2";
			permit2 = invoiceLine4.ICSPermits.AddNew();
			permit2.CY_Data = "PER1";
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			permit1 = invoiceLine5.ICSPermits.AddNew();
			permit1.CY_Data = "PER2";
			permit2 = invoiceLine5.ICSPermits.AddNew();
			permit2.CY_Data = "PER1";
			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			permit1 = invoiceLine6.ICSPermits.AddNew();
			permit1.CY_Data = "PER2";
			permit2 = invoiceLine6.ICSPermits.AddNew();
			permit2.CY_Data = "PER1";
			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			permit1 = invoiceLine7.ICSPermits.AddNew();
			permit1.CY_Data = "PER2";
			permit2 = invoiceLine7.ICSPermits.AddNew();
			permit2.CY_Data = "PER1";
			var permit3 = invoiceLine7.ICSPermits.AddNew();
			permit3.CY_Data = "PER3";
			var invoiceLine8 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine8.AddInfo.ZA_WAR = "KK234";
			invoiceLine8.JI_IsPackToBondForLine = true;
			var invoiceLine9 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine9.AddInfo.ZA_WAR = "KSD33";
			invoiceLine9.JI_IsPackToBondForLine = true;
			var invoiceLine10 = invoice.JobComInvoiceLines.AddNew();
			permit1 = invoiceLine10.ICSPermits.AddNew();
			permit1.CY_Data = "PER2";
			permit2 = invoiceLine10.ICSPermits.AddNew();
			permit2.CY_Data = "PER1";
			invoiceLine10.JI_IsPackToBondForLine = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			var entryLinePKs = ((CusEntryLineCollection)entry.MergedLines).GetPKs();
			AssertEquals(5, entryLinePKs.Count);
			entryLinePKs.Remove(invoiceLine1.JI_CL);
			AssertEquals(4, entryLinePKs.Count);
			AssertEquals(invoiceLine1.JI_CL, invoiceLine2.JI_CL);
			entryLinePKs.Remove(invoiceLine3.JI_CL);
			AssertEquals(3, entryLinePKs.Count);
			AssertEquals(invoiceLine3.JI_CL, invoiceLine4.JI_CL);
			entryLinePKs.Remove(invoiceLine5.JI_CL);
			AssertEquals(2, entryLinePKs.Count);
			AssertEquals(invoiceLine5.JI_CL, invoiceLine6.JI_CL);
			entryLinePKs.Remove(invoiceLine7.JI_CL);
			AssertEquals(1, entryLinePKs.Count);
			AssertEquals(entryLinePKs[0], invoiceLine8.JI_CL);
			AssertEquals(invoiceLine8.JI_CL, invoiceLine9.JI_CL);
			AssertEquals(invoiceLine10.JI_CL, invoiceLine9.JI_CL);
		}

		public void TestLinesShouldNotGetMergedIfSuppliersAreDifferentOnLine_ExWarehouse()
		{
			var supplierQuery = new ZQuery();
			var supplier1 = Factory.LoadTop1<OrgHeader>(supplierQuery);
			supplierQuery.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier1.PK);
			var supplier2 = Factory.LoadTop1<OrgHeader>(supplierQuery);
			supplierQuery.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier2.PK);
			var supplier3 = Factory.LoadTop1<OrgHeader>(supplierQuery);
			supplier3.CustomsClientID = "ABC123";
			var cusCode = supplier3.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);

				var testDec = JobDeclaration.New(Factory);
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;

				var invoice1 = testDec.Invoices.AddNew();
				invoice1.JZ_OH_Supplier = supplier1.PK;
				invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoice1.JZ_InvoiceAmount = 3000m;

				var line1 = invoice1.JobComInvoiceLines.AddNew();
				line1.JI_Tariff = "0206.29.00 26";
				line1.JI_LinePrice = 1000m;

				var line2 = invoice1.JobComInvoiceLines.AddNew();
				line2.JI_Tariff = "0206.29.00 26";
				line2.JI_LinePrice = 2000m;

				var merger = new LineMerger(testDec);
				var strategy = new CMREntryCreationStrategy(merger);
				var keyForLine1 = strategy.GetKeyForLine(line1);
				var keyForLine2 = strategy.GetKeyForLine(line2);

			AssertEquals("KeyForLine1 and Line2 should be same", keyForLine1, keyForLine2);
			AssertEquals("Key for Line1 has Supplier1 Code (from header)", true, keyForLine1.IndexOf(supplier1.OH_Code) >= 0);

			line1.JI_OH_Supplier = supplier1.PK;
			line2.JI_OH_Supplier = supplier2.PK;
			keyForLine1 = strategy.GetKeyForLine(line1);
			keyForLine2 = strategy.GetKeyForLine(line2);

			AssertEquals("KeyForLine1 and Line2 should be same", keyForLine1, keyForLine2);
			AssertEquals("Key for Line1 has Supplier1 Code", true, keyForLine1.IndexOf(supplier1.OH_Code) >= 0);

			line1.JI_OH_Supplier = supplier2.PK;
			line2.JI_OH_Supplier = supplier3.PK;
			keyForLine1 = strategy.GetKeyForLine(line1);
			keyForLine2 = strategy.GetKeyForLine(line2);

			AssertNotEquals("KeyForLine1 and Line2 should be different", keyForLine1, keyForLine2);
			AssertEquals("Key for Line1 missing Supplier3 CustomsID", false, keyForLine1.IndexOf(supplier3.CustomsClientID) >= 0);
			AssertEquals("Key for Line2 has Supplier3 CustomsID", true, keyForLine2.IndexOf(supplier3.CustomsClientID) >= 0);

			supplier2.CustomsClientID = "XYZ789";
			cusCode = supplier2.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			keyForLine1 = strategy.GetKeyForLine(line1);
			keyForLine2 = strategy.GetKeyForLine(line2);

			AssertNotEquals("KeyForLine1 and Line2 should be different", keyForLine1, keyForLine2);
			AssertEquals("Key for Line1 has Supplier2 CustomsID", true, keyForLine1.IndexOf(supplier2.CustomsClientID) >= 0);
			AssertEquals("Key for Line2 has Supplier3 CustomsID", true, keyForLine2.IndexOf(supplier3.CustomsClientID) >= 0);

			supplier2.CustomsClientID = "ABC123";
			keyForLine1 = strategy.GetKeyForLine(line1);
			keyForLine2 = strategy.GetKeyForLine(line2);

			AssertEquals("KeyForLine1 and Line2 should be same", keyForLine1, keyForLine2);
			AssertEquals("Key for Line1 has Supplier3 CustomsID", true, keyForLine1.IndexOf(supplier3.CustomsClientID) >= 0);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			keyForLine1 = strategy.GetKeyForLine(line1);
			keyForLine2 = strategy.GetKeyForLine(line2);
			AssertNotEquals("KeyForLine1 and Line2 should be different", keyForLine1, keyForLine2);
		}

		public void TestExportDeclarationMergeWithDifferentAssayCode()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_AssayAG_Hidden = 200m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_AssayAG_Hidden = 100m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are not same", false, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertNotEquals("Two merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestExportDeclarationMergeWithSameAssayCode()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_AssayAG_Hidden = 200m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_AssayAG_Hidden = 200m;

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are same", true, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestExportDeclarationMergeWithDifferentStateCode()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.JI_AUState = "VIC";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.JI_AUState = "MULT";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are not same", false, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertNotEquals("Two merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestExportDeclarationMergeWithSameStateCode()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.JI_AUState = "VIC";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.JI_AUState = "VIC";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are same", true, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestExportDeclarationMergeWithDifferentPermitNumbers()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_PermitNumbers_Hidden = "1";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_PermitNumbers_Hidden = "2";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are not same", false, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertNotEquals("Two merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestExportDeclarationMergeWithSamePermitNumbers()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.AddInfo.ZA_PermitNumbers_Hidden = "1";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.AddInfo.ZA_PermitNumbers_Hidden = "1";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are same", true, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestExportDeclarationMergeWithDifferentTempImportNum()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.JI_TempImportNum = "1";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.JI_TempImportNum = "2";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are not same", false, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertNotEquals("Two merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestExportDeclarationMergeWithSameTempImportNum()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			invoiceLine1.JI_TempImportNum = "1";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			invoiceLine2.JI_TempImportNum = "1";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyWithInvoice1 = strategy.GetKeyForLine(invoiceLine1);
			var keyWithInvoice2 = strategy.GetKeyForLine(invoiceLine2);

			AssertEquals("Two keys are same", true, keyWithInvoice1 == keyWithInvoice2);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		#region GST Exemption

		public void TestMergeGSTEWithDifferentHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_GSTE = "GST1";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_GSTE = "GST2";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeGSTEWithSameHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_GSTE = "GSTE";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_GSTE = "GSTE";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry lines", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeGSTEWithSameHeaderAndDifferentLineValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_GSTE = "GSTE";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_GSTE = "GSTE";
			var line2 = header2.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_GSTE = "GST1";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeGSTEWithDifferentLineValues()
		{
			var header1 = declaration.Invoices.AddNew();
			line1 = header1.JobComInvoiceLines.AddNew();
			line1.AddInfo.ZA_GSTE = "GST1";

			var header2 = declaration.Invoices.AddNew();
			var line2 = header2.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_GSTE = "GST2";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		#endregion

		#region Luxury Car Tax Exemption

		public void TestMergeLuxuryCarTaxWithoutIndicator()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "12345";

			line1 = header1.JobComInvoiceLines.AddNew();
			var line2 = header1.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			AssertEquals("KeyForLine1 and KeyForLine2 should be the same", true, keyForLine1 == keyForLine2);

			AssertEquals("KeyForLine1 has no invoice no", false, keyForLine1.Contains((ZString)"InvNo=12345"));
			AssertEquals("KeyForLine1 has no line no", false, keyForLine1.Contains((ZString)"LineNo=1"));

			AssertEquals("KeyForLine2 has no invoice no", false, keyForLine2.Contains((ZString)"InvNo=12345"));
			AssertEquals("KeyForLine2 has no line no", false, keyForLine2.Contains((ZString)"LineNo=2"));
		}

		public void TestMergeLuxuryCarTaxWithIndicatorOnOneLine()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "12345";

			line1 = header1.JobComInvoiceLines.AddNew();
			line1.AddInfo.ZA_LCTE = "Y";

			var line2 = header1.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			AssertEquals("KeyForLine1 and KeyForLine2 should be different", false, keyForLine1 == keyForLine2);

			AssertEquals("KeyForLine1 has no invoice no", true, keyForLine1.Contains((ZString)"InvNo=12345LineNo=1"));
			AssertEquals("KeyForLine1 has no line no", true, keyForLine1.Contains((ZString)""));

			AssertEquals("KeyForLine2 has no invoice no", false, keyForLine2.Contains((ZString)"InvNo=12345LineNo=2"));
		}

		public void TestMergeLuxuryCarTaxWithIndicatorOnBothLines()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "12345";

			line1 = header1.JobComInvoiceLines.AddNew();
			line1.AddInfo.ZA_LCTE = "Y";

			var line2 = header1.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_LCTE = "Y";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			AssertEquals("KeyForLine1 and KeyForLine2 should be different", false, keyForLine1 == keyForLine2);

			AssertEquals("KeyForLine1 has invoice no", true, keyForLine1.Contains((ZString)"InvNo=12345LineNo=1"));

			AssertEquals("KeyForLine2 has invoice no", true, keyForLine2.Contains((ZString)"InvNo=12345LineNo=2"));
		}

		#endregion

		#region Valuation Basis

		public void TestMergeValuationBasisWithDifferentHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_VALB_Hidden = "TV";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_VALB_Hidden = "IG";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeValuationBasisWithSameHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_VALB_Hidden = "TV";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_VALB_Hidden = "TV";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry lines", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeValuationBasisWithDifferentLineValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_VALB_Hidden = "TV";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_VALB_Hidden = "TV";
			var line2 = header2.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_VALB_Hidden = "IG";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		#endregion

		#region Preference Origin

		public void TestMergePreferenceOriginsWithDifferentHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_POC = "AU";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_POC = "IT";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergePreferenceOriginsWithSameHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_POC = "AU";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_POC = "AU";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry lines", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergePreferenceOriginsWithDifferentLineValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_POC = "AU";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_POC = "AU";
			var line2 = header2.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_POC = "IT";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		#endregion

		#region Preference Rule Type

		public void TestMergePreferenceRuleTypeWithDifferentHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_PRT = "DDD";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_PRT = "AAA";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergePreferenceRuleTypeWithSameHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_PRT = "DDD";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_PRT = "DDD";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry lines", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergePreferenceRuleTypeWithDifferentLineValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_PRT = "DDD";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_PRT = "DDD";
			var line2 = header2.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_PRT = "AAA";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		#endregion

		#region Preference Scheme Type

		public void TestMergePreferenceSchemeTypeWithDifferentHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_PST = "DDD";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_PST = "AAA";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergePreferenceSchemeTypeWithSameHeaderValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_PST = "DDD";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_PST = "DDD";
			var line2 = header2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One entry lines", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergePreferenceSchemeTypeWithDifferentLineValues()
		{
			var header1 = declaration.Invoices.AddNew();
			header1.AddInfo.ZA_PST = "DDD";
			line1 = header1.JobComInvoiceLines.AddNew();

			var header2 = declaration.Invoices.AddNew();
			header2.AddInfo.ZA_PST = "DDD";
			var line2 = header2.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_PST = "AAA";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("One entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		#endregion

		#region AQIS Line Container Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISContainersSelectedAreDifferent()
		{
			var testDec = GetDeclarationWithInvoiceLinesAttachedToContainers();
			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			var keyForLine3 = strategy.GetKeyForLine(line3);
			var keyForLine4 = strategy.GetKeyForLine(line4);

			AssertEquals("KeyForLine1 and KeyForLine2 should be different", false, keyForLine1 == keyForLine2);
			AssertEquals("KeyForLine1 and KeyForLine4 should be different", false, keyForLine1 == keyForLine4);
			AssertEquals("KeyForLine1 and KeyForLine2 should be the same", true, keyForLine1 == keyForLine3);
			AssertEquals("KeyForLine2 and KeyForLine3 should be different", false, keyForLine2 == keyForLine3);
			AssertEquals("KeyForLine2 and KeyForLine4 should be different", false, keyForLine2 == keyForLine4);
			AssertEquals("KeyForLine3 and KeyForLine4 should be different", false, keyForLine3 == keyForLine4);

			AssertEquals("Key for Line1", true, keyForLine1.Contains((ZString)"AAAA1234566 CCCC1234566"));
			AssertEquals("Key for Line2", true, keyForLine2.Contains((ZString)"BBBB1234566"));
			AssertEquals("Key for Line3", true, keyForLine3.Contains((ZString)"AAAA1234566 CCCC1234566"));

			merger.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			var entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Two entry lines", 3, entryHeader.MergedLines.Count);
		}

		JobDeclaration GetDeclarationWithInvoiceLinesAttachedToContainers()
		{
			var testDec = GetBasicDeclarationWithLines();
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CCCC1234566";

			var container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "AAAA1234566";

			var container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "BBBB1234566";

			AddContainerToInvoiceLine(line1, "AAAA1234566");
			AddContainerToInvoiceLine(line1, "CCCC1234566");

			AddContainerToInvoiceLine(line2, "BBBB1234566");

			AddContainerToInvoiceLine(line3, "AAAA1234566");
			AddContainerToInvoiceLine(line3, "CCCC1234566");

			return testDec;
		}

		void AddContainerToInvoiceLine(JobComInvoiceLine invoiceLine, ZString containerNumber)
		{
			foreach (var currentContainer in invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<Customs.Business.NonPersistentCusContainer>())
			{
				if (currentContainer.ContainerNumber == containerNumber)
				{
					currentContainer.IsForInvoiceLine = ZBool.True;
				}
			}
		}

		#endregion

		#region AQIS Package Type Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISPackageTypesAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();
			AddAQISPackagesToLines();

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			var keyForLine3 = strategy.GetKeyForLine(line3);
			var keyForLine4 = strategy.GetKeyForLine(line4);

			AssertEquals("KeyForLine1 and KeyForLine2 should be different", false, keyForLine1 == keyForLine2);
			AssertEquals("KeyForLine1 and KeyForLine3 should be different", false, keyForLine1 == keyForLine3);
			AssertEquals("KeyForLine1 and KeyForLine4 should be the same", true, keyForLine1 == keyForLine4);
			AssertEquals("KeyForLine2 and KeyForLine3 should be different", false, keyForLine2 == keyForLine3);
			AssertEquals("KeyForLine2 and KeyForLine4 should be different", false, keyForLine2 == keyForLine4);
			AssertEquals("KeyForLine3 and KeyForLine4 should be different", false, keyForLine3 == keyForLine4);

			AssertEquals("Key for Line1", true, keyForLine1.Contains((ZString)"BAG KG"));
			AssertEquals("Key for Line2", true, keyForLine2.Contains((ZString)"BAG"));
			AssertEquals("Key for Line3", true, keyForLine3.Contains((ZString)"KG"));
			AssertEquals("Key for Line4", true, keyForLine4.Contains((ZString)"BAG KG"));

			merger.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			var entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Three entry lines", 3, entryHeader.MergedLines.Count);
		}

		void AddAQISPackagesToLines()
		{
			var package1Line1 = line1.AQISPackages.AddNew();
			package1Line1.Number = 123;
			package1Line1.Type = "KG";
			var package2Line1 = line1.AQISPackages.AddNew();
			package2Line1.Number = 456;
			package2Line1.Type = "BAG";

			var package1Line2 = line2.AQISPackages.AddNew();
			package1Line2.Number = 456;
			package1Line2.Type = "BAG";

			var package1Line3 = line3.AQISPackages.AddNew();
			package1Line3.Number = 123;
			package1Line3.Type = "KG";

			var package1Line4 = line4.AQISPackages.AddNew();
			package1Line4.Number = 456;
			package1Line4.Type = "BAG";

			var package2Line4 = line4.AQISPackages.AddNew();
			package2Line4.Number = 123;
			package2Line4.Type = "KG";
		}

		#endregion

		#region AQIS Document Type and Number Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISDocumentNumberAndTypeAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();
			AddAQISDocumentInfoToLines();

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			var keyForLine3 = strategy.GetKeyForLine(line3);
			var keyForLine4 = strategy.GetKeyForLine(line4);

			AssertEquals("KeyForLine1 and KeyForLine2 should be different", false, keyForLine1 == keyForLine2);
			AssertEquals("KeyForLine1 and KeyForLine3 should be different", false, keyForLine1 == keyForLine3);
			AssertEquals("KeyForLine1 and KeyForLine4 should be the same", true, keyForLine1 == keyForLine4);
			AssertEquals("KeyForLine2 and KeyForLine3 should be different", false, keyForLine2 == keyForLine3);
			AssertEquals("KeyForLine2 and KeyForLine4 should be different", false, keyForLine2 == keyForLine4);
			AssertEquals("KeyForLine3 and KeyForLine4 should be different", false, keyForLine3 == keyForLine4);

			AssertEquals("Key for Line1", true, keyForLine1.Contains((ZString)"AAA Number 1 DDD Number 2"));
			AssertEquals("Key for Line2", true, keyForLine2.Contains((ZString)"EEE Number 1"));
			AssertEquals("Key for Line3", true, keyForLine3.Contains((ZString)"AAA Number 2 DDD Number 1"));
			AssertEquals("Key for Line4", true, keyForLine4.Contains((ZString)"AAA Number 1 DDD Number 2"));

			merger.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			var entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Three entry lines", 3, entryHeader.MergedLines.Count);
		}

		void AddAQISDocumentInfoToLines()
		{
			var line1Document1 = line1.AQISDocuments.AddNew();
			line1Document1.Type = "AAA";
			line1Document1.Number = "Number 1";
			var line1Document2 = line1.AQISDocuments.AddNew();
			line1Document2.Type = "DDD";
			line1Document2.Number = "Number 2";

			var line2Document1 = line2.InvoiceHeader.AQISDocuments.AddNew();
			line2Document1.Type = "EEE";
			line2Document1.Number = "Number 1";

			var line3Document1 = line3.Declaration.AQISDocuments.AddNew();
			line3Document1.Type = "DDD";
			line3Document1.Number = "Number 1";
			var line3Document2 = line3.Declaration.AQISDocuments.AddNew();
			line3Document2.Type = "AAA";
			line3Document2.Number = "Number 2";

			var line4Document1 = line4.AQISDocuments.AddNew();
			line4Document1.Type = "DDD";
			line4Document1.Number = "Number 2";
			var line4Document2 = line4.AQISDocuments.AddNew();
			line4Document2.Type = "AAA";
			line4Document2.Number = "Number 1";
		}

		#endregion

		#region AQIS Commodity Merge Tests

		public void TestLinesShouldNotGetMergedIfAQISCommodityCodesAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();
			line1.AddInfo.ZA_AQISCommCodes_Hidden = "AAA,DDD";
			line2.InvoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden = "AAA";
			line3.Declaration.AddInfo.ZA_AQISCommCodes_Hidden = "DDD";
			line4.AddInfo.ZA_AQISCommCodes_Hidden = "DDD,AAA";
			AssertMergeKeysAndMergeLineCount(testDec);
		}

		public void TestLineMergeWithSimiliarCommodityCodes()
		{
			var testDec = GetBasicDeclarationWithLines();
			line1.AddInfo.ZA_AQISCommCodes_Hidden = "A";
			line2.AddInfo.ZA_AQISCommCodes_Hidden = "AA";
			line4.AddInfo.ZA_AQISCommCodes_Hidden = "AAA";

			var merger = new LineMerger(testDec);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			var keyForLine3 = strategy.GetKeyForLine(line3);
			var keyForLine4 = strategy.GetKeyForLine(line4);

			AssertEquals("KeyForLine1 and KeyForLine2 should be different", false, keyForLine1 == keyForLine2);
			AssertEquals("KeyForLine1 and KeyForLine3 should be different", false, keyForLine1 == keyForLine3);
			AssertEquals("KeyForLine1 and KeyForLine4 should be different", false, keyForLine1 == keyForLine4);
			AssertEquals("KeyForLine2 and KeyForLine3 should be different", false, keyForLine2 == keyForLine3);
			AssertEquals("KeyForLine2 and KeyForLine4 should be different", false, keyForLine2 == keyForLine4);
			AssertEquals("KeyForLine3 and KeyForLine4 should be different", false, keyForLine3 == keyForLine4);

			AssertEquals("Key for Line1", true, keyForLine1.Contains((ZString)"A"));
			AssertEquals("Key for Line2", true, keyForLine2.Contains((ZString)"AA"));
			AssertEquals("Key for Line4", true, keyForLine4.Contains((ZString)"AAA"));

			merger.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			var entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Three entry lines", 4, entryHeader.MergedLines.Count);
		}

		#endregion

		#region AQIS Entity Id Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISEntityIdsAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();
			line1.AddInfo.ZA_AQISEntityIds_Hidden = "AAA,DDD";
			var entityId = line2.InvoiceHeader.AQISEntityIds.AddNew();
			entityId.Code = "AAA";
			line3.Declaration.AddInfo.ZA_AQISEntityIds_Hidden = "DDD";
			line4.AddInfo.ZA_AQISEntityIds_Hidden = "DDD,AAA";
			AssertMergeKeysAndMergeLineCount(testDec);
		}

		#endregion

		#region AQIS Permit Number Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISPermitNumbersAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();
			line1.AddInfo.ZA_AQISPermitIds_Hidden = "AAA,DDD";
			var permitId = line2.InvoiceHeader.AQISPermitIds.AddNew();
			permitId.Code = "AAA";
			line3.Declaration.AddInfo.ZA_AQISPermitIds_Hidden = "DDD";
			line4.AddInfo.ZA_AQISPermitIds_Hidden = "DDD,AAA";
			AssertMergeKeysAndMergeLineCount(testDec);
		}

		#endregion

		#region AQIS Premises Id Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISPremisesIdsAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();

			var premisesId1 = line1.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId1.PremisesId = "AAA";
			var premisesId2 = line1.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId2.PremisesId = "DDD";
			var premisesId3 = line2.InvoiceHeader.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId3.PremisesId = "AAA";
			var premisesId4 = line3.Declaration.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId4.PremisesId = "DDD";
			var premisesId5 = line4.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId5.PremisesId = "DDD";
			var premisesId6 = line4.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesId6.PremisesId = "AAA";

			AssertMergeKeysAndMergeLineCount(testDec);
		}

		#endregion

		#region AQIS Processing Type Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISProcessingTypesAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();
			var processingType1 = line1.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType1.ProcessingType = "AAA";
			var processingType2 = line1.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType2.ProcessingType = "DDD";
			var processingType3 = line2.InvoiceHeader.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType3.ProcessingType = "AAA";
			var processingType4 = line3.Declaration.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType4.ProcessingType = "DDD";
			var processingType5 = line4.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType5.ProcessingType = "DDD";
			var processingType6 = line4.AQISPremisesIdAndProcessingTypes.AddNew();
			processingType6.ProcessingType = "AAA";

			AssertMergeKeysAndMergeLineCount(testDec);
		}

		#endregion

		#region AQIS Producer Code Merge Tests

		public void TestLinesShuoldNotGetMergedIfAQISProducerCOdeAreDifferent()
		{
			var testDec = GetBasicDeclarationWithLines();
			testDec.AddInfo.ZA_AQISProducerCodes_Hidden = "DDD";

			line1.AddInfo.ZA_AQISProducerCodes_Hidden = "AAA,DDD";

			var producerCode = line2.InvoiceHeader.AQISProducerCodes.AddNew();
			producerCode.Code = "AAA";

			line4.AddInfo.ZA_AQISProducerCodes_Hidden = "DDD,AAA";
			AssertMergeKeysAndMergeLineCount(testDec);
		}

		#endregion

		#region Implementation

		JobDeclaration GetBasicDeclarationWithLines()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoice1 = testDec.Invoices.AddNew();
			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			var invoice2 = testDec.Invoices.AddNew();
			line3 = invoice2.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0206.29.00 26";
			line3.JI_LinePrice = 10000m;

			line4 = invoice1.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "0206.29.00 26";
			line4.JI_LinePrice = 10000m;

			return testDec;
		}

		JobComInvoiceLine line1;
		JobComInvoiceLine line2;
		JobComInvoiceLine line3;
		JobComInvoiceLine line4;

		void AssertMergeKeysAndMergeLineCount(JobDeclaration declaration)
		{
			var merger = new LineMerger(declaration);
			var strategy = new CMREntryCreationStrategy(merger);
			var keyForLine1 = strategy.GetKeyForLine(line1);
			var keyForLine2 = strategy.GetKeyForLine(line2);
			var keyForLine3 = strategy.GetKeyForLine(line3);
			var keyForLine4 = strategy.GetKeyForLine(line4);

			AssertEquals("KeyForLine1 and KeyForLine2 should be different", false, keyForLine1 == keyForLine2);
			AssertEquals("KeyForLine1 and KeyForLine3 should be different", false, keyForLine1 == keyForLine3);
			AssertEquals("KeyForLine1 and KeyForLine4 should be the same", true, keyForLine1 == keyForLine4);
			AssertEquals("KeyForLine2 and KeyForLine3 should be different", false, keyForLine2 == keyForLine3);
			AssertEquals("KeyForLine2 and KeyForLine4 should be different", false, keyForLine2 == keyForLine4);
			AssertEquals("KeyForLine3 and KeyForLine4 should be different", false, keyForLine3 == keyForLine4);

			AssertEquals("Key for Line1", true, keyForLine1.Contains((ZString)"AAA DDD"));
			AssertEquals("Key for Line2", true, keyForLine2.Contains((ZString)"AAA"));
			AssertEquals("Key for Line3", true, keyForLine3.Contains((ZString)"DDD"));
			AssertEquals("Key for Line4", true, keyForLine4.Contains((ZString)"AAA DDD"));

			merger.DoMerge();
			AssertEquals("One Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("Three entry lines", 3, entryHeader.MergedLines.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AutoCMRAqisPremises.Schema.TableName);
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}
		JobDeclaration declaration;

		#endregion
	}
}
