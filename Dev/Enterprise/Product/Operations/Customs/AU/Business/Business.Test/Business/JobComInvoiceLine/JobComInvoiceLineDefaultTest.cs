using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineDefaultTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestCustomsUnitDefaultingStrategy()
		{
			AssertType<TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(typeof(BaseJobComInvoiceLine).GetProperty("CustomsUnitDefaultingStrategy", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(InvoiceLine));
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Australia, partDetails.CustomsCountryCode);
				AssertEquals(typeof(AUOrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestICusStorageDocPivotTypeSupporter_ReloadCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			_ = invoiceLine.EDocPivotCollection;
			var provider = (ICusStorageDocPivotTypeSupporter)invoiceLine;
			var pivot = Factory.New<CusStorageDocPivot>();
			pivot.CSD_ParentID = invoiceLine.PK;
			pivot.CSD_ParentTableCode = invoiceLine.TablePrefix;
			provider.ReloadCollection();
			AssertEquals(1, invoiceLine.EDocPivotCollection.Count);
		}

		public void TestICusStorageDocPivotTypeSupporter_HumanReadableName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV 123";
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var provider = (ICusStorageDocPivotTypeSupporter)invoiceLine;
			AssertEquals("invoice INV 123 line 1", provider.HumanReadableName);
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

			var pivotParent = (ICusStorageDocPivotParent)invoiceLine;
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
		}

		public void TestQuestionsWithEffectiveDefaultAnswers()
		{
			ZDateTime dutyDate = new ZDateTime(2010, 1, 1);
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			CMRCommunityProtectionProfile profile1 = CMRCommunityProtectionProfile.New(Factory);
			profile1.CP_TariffClassificationNumberfield = "00000000";
			profile1.CP_CommunityProtectionRiskIdentifier = 400;
			CMRCommunityProtectionRisk risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dutyDate;
			risk1.CK_LodgementQuestionIdentifier = 4001;
			CMRLodgementQuestion question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 4001;
			question1.CQ_LodgementQuestionStartDate = dutyDate;

			CMRCommunityProtectionProfile profile2 = CMRCommunityProtectionProfile.New(Factory);
			profile2.CP_TariffClassificationNumberfield = "00000000";
			profile2.CP_CommunityProtectionRiskIdentifier = 402;
			CMRCommunityProtectionRisk risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 402;
			risk2.CK_StartDate = dutyDate;
			risk2.CK_LodgementQuestionIdentifier = 5001;
			CMRLodgementQuestion question2 = CMRLodgementQuestion.New(Factory);
			question2.CQ_LodgementQuestionIdentifier = 5001;
			question2.CQ_LodgementQuestionStartDate = dutyDate;

			CMRCommunityProtectionProfile profile3 = CMRCommunityProtectionProfile.New(Factory);
			profile3.CP_TariffClassificationNumberfield = "00000000";
			profile3.CP_CommunityProtectionRiskIdentifier = 403;
			CMRCommunityProtectionRisk risk3 = CMRCommunityProtectionRisk.New(Factory);
			risk3.CK_Identifier = 403;
			risk3.CK_StartDate = dutyDate;
			risk3.CK_LodgementQuestionIdentifier = 6001;
			CMRLodgementQuestion question3 = CMRLodgementQuestion.New(Factory);
			question3.CQ_LodgementQuestionIdentifier = 6001;
			question3.CQ_LodgementQuestionStartDate = dutyDate;

			Classification classification = Factory.New<Classification>();
			classification.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			classification.CC_LookupCode = "LookupCode";
			classification.CC_TariffNum = "0000.00.00";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.GenerateQuestion();
			AssertEquals("3 questions generated", 3, classification.Questions.Count);
			setQuestionAnswer(classification.Questions, 4001, "Y");
			setQuestionAnswer(classification.Questions, 5001, "N");

			AUOrgSupplierPart part1 = Factory.New<AUOrgSupplierPart>();
			var pivot1 = part1.AddNewImportPivotWithClassification(classification.PK);
			part1.OP_PartNum = "PART1";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot1.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot1.Questions.Count);
			setQuestionAnswer(pivot1.Questions, 4001, ZString.Empty);
			setQuestionAnswer(pivot1.Questions, 5001, ZString.Empty);
			setQuestionAnswer(pivot1.Questions, 6001, ZString.Empty);

			AUOrgSupplierPart part2 = Factory.New<AUOrgSupplierPart>();
			var pivot2 = part2.AddNewImportPivotWithClassification(classification.PK);
			part2.OP_PartNum = "PART2";
			part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot2.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot2.Questions.Count);
			setQuestionAnswer(pivot2.Questions, 4001, "Y");
			setQuestionAnswer(pivot2.Questions, 5001, "Y");
			setQuestionAnswer(pivot2.Questions, 6001, "Y");

			AUOrgSupplierPart part3 = Factory.New<AUOrgSupplierPart>();
			var pivot3 = part3.AddNewImportPivotWithClassification(classification.PK);
			part3.OP_PartNum = "PART3";
			part3.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part3.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot3.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot3.Questions.Count);
			setQuestionAnswer(pivot3.Questions, 4001, "Y");
			setQuestionAnswer(pivot3.Questions, 5001, "N");
			setQuestionAnswer(pivot3.Questions, 6001, ZString.Empty);

			AUOrgSupplierPart part4 = Factory.New<AUOrgSupplierPart>();
			var pivot4 = part4.AddNewImportPivotWithClassification(classification.PK);
			part4.OP_PartNum = "PART4";
			part4.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part4.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot4.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot4.Questions.Count);
			setQuestionAnswer(pivot4.Questions, 4001, ZString.Empty);
			setQuestionAnswer(pivot4.Questions, 5001, "N");
			setQuestionAnswer(pivot4.Questions, 6001, "N");

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART1";
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART2";
			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = "PART3";
			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = "PART4";
			JobComInvoiceLine invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CC = classification.PK;

			CMRCusEntryCPDec[] questionsWithEffectiveDefaultAnswers = invoiceLine1.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 1 has 2 defaulted questions", 2, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 4001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "N");

			pivot2.Questions.Sort(CMRCusEntryCPDec.Schema.ON_CPDecNum, ListSortDirection.Ascending);
			questionsWithEffectiveDefaultAnswers = invoiceLine2.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 2 has 2 defaulted questions", 2, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 4001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "?");

			questionsWithEffectiveDefaultAnswers = invoiceLine3.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 3 has 2 defaulted questions", 2, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 4001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "N");

			questionsWithEffectiveDefaultAnswers = invoiceLine4.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 4 has 3 defaulted questions", 3, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 4001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "N");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 6001, "N");

			questionsWithEffectiveDefaultAnswers = invoiceLine5.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 5 has 2 defaulted questions", 2, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 4001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "N");
		}

		public void TestQuestionsWithEffectiveDefaultAnswersWithoutLookupAnswerSet()
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

			var profile2 = CMRCommunityProtectionProfile.New(Factory);
			profile2.CP_TariffClassificationNumberfield = "00000000";
			profile2.CP_CommunityProtectionRiskIdentifier = 402;
			var risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 402;
			risk2.CK_StartDate = dutyDate;
			risk2.CK_LodgementQuestionIdentifier = 5001;
			var question2 = CMRLodgementQuestion.New(Factory);
			question2.CQ_LodgementQuestionIdentifier = 5001;
			question2.CQ_LodgementQuestionStartDate = dutyDate;

			var profile3 = CMRCommunityProtectionProfile.New(Factory);
			profile3.CP_TariffClassificationNumberfield = "00000000";
			profile3.CP_CommunityProtectionRiskIdentifier = 403;
			var risk3 = CMRCommunityProtectionRisk.New(Factory);
			risk3.CK_Identifier = 403;
			risk3.CK_StartDate = dutyDate;
			risk3.CK_LodgementQuestionIdentifier = 6001;
			var question3 = CMRLodgementQuestion.New(Factory);
			question3.CQ_LodgementQuestionIdentifier = 6001;
			question3.CQ_LodgementQuestionStartDate = dutyDate;

			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			classification.CC_LookupCode = "LookupCode";
			classification.CC_TariffNum = "0000.00.00";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("no questions generated", 0, classification.Questions.Count);

			AUOrgSupplierPart part1 = Factory.New<AUOrgSupplierPart>();
			var pivot1 = part1.AddNewImportPivotWithClassification(classification.PK);
			part1.OP_PartNum = "PART1";
			part1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot1.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot1.Questions.Count);
			setQuestionAnswer(pivot1.Questions, 4001, ZString.Empty);
			setQuestionAnswer(pivot1.Questions, 5001, ZString.Empty);
			setQuestionAnswer(pivot1.Questions, 6001, ZString.Empty);

			AUOrgSupplierPart part2 = Factory.New<AUOrgSupplierPart>();
			var pivot2 = part2.AddNewImportPivotWithClassification(classification.PK);
			part2.OP_PartNum = "PART2";
			part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot2.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot2.Questions.Count);
			setQuestionAnswer(pivot2.Questions, 4001, "Y");
			setQuestionAnswer(pivot2.Questions, 5001, "Y");
			setQuestionAnswer(pivot2.Questions, 6001, "Y");

			AUOrgSupplierPart part3 = Factory.New<AUOrgSupplierPart>();
			var pivot3 = part3.AddNewImportPivotWithClassification(classification.PK);
			part3.OP_PartNum = "PART3";
			part3.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part3.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot3.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot3.Questions.Count);
			setQuestionAnswer(pivot3.Questions, 4001, "Y");
			setQuestionAnswer(pivot3.Questions, 5001, "N");
			setQuestionAnswer(pivot3.Questions, 6001, ZString.Empty);

			AUOrgSupplierPart part4 = Factory.New<AUOrgSupplierPart>();
			var pivot4 = part4.AddNewImportPivotWithClassification(classification.PK);
			part4.OP_PartNum = "PART4";
			part4.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			part4.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			pivot4.GenerateQuestion();
			AssertEquals("3 questions generated", 3, pivot4.Questions.Count);
			setQuestionAnswer(pivot4.Questions, 4001, ZString.Empty);
			setQuestionAnswer(pivot4.Questions, 5001, "Y");
			setQuestionAnswer(pivot4.Questions, 6001, "N");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "PART1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "PART2";
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = "PART3";
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = "PART4";
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CC = classification.PK;

			var questionsWithEffectiveDefaultAnswers = invoiceLine1.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 1 has 0 defaulted questions", 0, questionsWithEffectiveDefaultAnswers.Length);

			questionsWithEffectiveDefaultAnswers = invoiceLine2.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 2 has 3 defaulted questions", 3, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 4001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 6001, "Y");

			questionsWithEffectiveDefaultAnswers = invoiceLine3.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 3 has 2 defaulted questions", 2, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 4001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "N");

			questionsWithEffectiveDefaultAnswers = invoiceLine4.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 4 has 2 defaulted questions", 2, questionsWithEffectiveDefaultAnswers.Length);
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 5001, "Y");
			checkQuestionAnswer(questionsWithEffectiveDefaultAnswers, 6001, "N");

			questionsWithEffectiveDefaultAnswers = invoiceLine5.QuestionsWithEffectiveDefaultAnswers;
			AssertEquals("Line 5 has 0 defaulted questions", 0, questionsWithEffectiveDefaultAnswers.Length);
		}

		static void setQuestionAnswer(CMRCusEntryCPDecCollection questions, int cPDecNum, ZString answer)
		{
			int index = 0;
			while (index < questions.Count && questions[index].ON_CPDecNum != cPDecNum)
			{
				index++;
			}
			Assert("CPDecNum not found in Questions", index < questions.Count);
			questions[index].ON_AnswerCode = answer;
		}

		static void checkQuestionAnswer(CMRCusEntryCPDec[] questions, int cPDecNum, ZString answer)
		{
			int index = 0;
			while (index < questions.Length && questions[index].ON_CPDecNum != cPDecNum)
			{
				index++;
			}
			Assert("CPDecNum not found in Questions", index < questions.Length);
			AssertEquals("Wrong answer", answer, questions[index].EffectiveDefaultAnswer);
		}

		public override void TestEffectiveCountryOfOrigin()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.AddInfo.ZA_ORG = "KR";
			invoiceLine.JI_CountryOfOrigin = "";
			AssertEquals("EffectiveCountryOfOrigin", "KR", invoiceLine.EffectiveCountryOfOrigin);

			invoiceLine.JI_CountryOfOrigin = "US";
			AssertEquals("EffectiveCountryOfOrigin", "US", invoiceLine.EffectiveCountryOfOrigin);
		}

		public void TestDrawbackCalculationMethodComment()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			AssertEquals("DrawbackCalculationMethodComment blank if no lines in collection", "", invoiceLine.DrawbackCalculationMethodComment);
			CusEntryLine collectionLine = invoiceLine.DrawbackCusEntryLineCollection.AddNew();
			AssertEquals("DrawbackCalculationMethodComment set lines in collection", "Method B Average Calculation used on this line.", invoiceLine.DrawbackCalculationMethodComment);
			invoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			AssertEquals("DrawbackCalculationMethodComment blank if not method B", "", invoiceLine.DrawbackCalculationMethodComment);
		}

		public void TestPartType()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			MasterFiles.Business.OrgSupplierPart product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.NZ.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			GlbCompany auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch auBranch = auCompany.Branches.AddNew();
			auBranch.GB_RL_NKHomePort = "AUSYD";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = auBranch.PK;
			declaration.JE_OH_Importer = importer.PK;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(AUOrgSupplierPart), invoiceLine.Part.GetType());

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("NZ");
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			JobDeclaration declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still AU type as it is hooked with AU invoice line", typeof(AUOrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestNatureLegOrCMR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Nature", CusEntryHeader.NatureTypesForImportCMR.Nature10, invoiceLine.NatureLegOrCMR);

			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("Nature", CusEntryHeader.NatureTypesForImportCMR.Nature20, invoiceLine.NatureLegOrCMR);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Nature", CusEntryHeader.NatureTypesForImportCMR.Nature30, invoiceLine.NatureLegOrCMR);

			declaration.JE_ApplicationCode = "LEG";
			AssertEquals("Nature", CusEntryHeader.NatureTypes.Nature30, invoiceLine.NatureLegOrCMR);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("Nature", CusEntryHeader.NatureTypes.Nature20, invoiceLine.NatureLegOrCMR);

			invoiceLine.JI_IsPackToBondForLine = false;
			AssertEquals("Nature", CusEntryHeader.NatureTypes.Nature10, invoiceLine.NatureLegOrCMR);
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

		public override void TestJI_FormattedTariff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ZString tariff = "1234567890";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.78", InvoiceLine.JI_FormattedTariff);
			tariff = "9876.54.32 10";
			InvoiceLine.JI_FormattedTariff = tariff;
			AssertEquals("JI_FormattedTariff", "9876.54.32", InvoiceLine.JI_FormattedTariff);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			tariff = "1234567890";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.78 90", InvoiceLine.JI_FormattedTariff);
			tariff = "9876.54.32 10";
			InvoiceLine.JI_FormattedTariff = tariff;
			AssertEquals("JI_FormattedTariff", "9876.54.32 10", InvoiceLine.JI_FormattedTariff);
		}

		public override void TestFetchStrategy()
		{
			AssertEquals("InvoiceLine.FetchStrategy type", typeof(JobComInvoiceLineFetchStrategy), InvoiceLine.FetchStrategy.GetType());
		}

		public void TestUseBondedWarehouseAutomationDefaulting()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.AddInfo.UseBondedWarehouseAutomation);

			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine2.AddInfo.UseBondedWarehouseAutomation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(true, invoiceLine3.AddInfo.UseBondedWarehouseAutomation);

			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(true, invoiceLine4.AddInfo.UseBondedWarehouseAutomation);

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			AssertEquals(false, invoice2.AddInfo.UseBondedWarehouseAutomation);
		}

		[ExpectNoExceptions]
		public void TestMergeKeyException()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceHeader header1 = declaration.Invoices.AddNew();
			JobComInvoiceHeader header2 = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "Tariff";
			JobComInvoiceLine line2 = header2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "Tariff1";
			declaration.DoMerge();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration reloadedDec = factory2.Load<JobDeclaration>(declaration.PK);
			JobComInvoiceLine reloadedLine2 = reloadedDec.Invoices[1].JobComInvoiceLines[0];
			reloadedLine2.JI_Tariff = "Change";
			reloadedDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			reloadedDec.DoMerge();
			factory2.Save();
		}

		public void TestAggregatedPOC()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "NZ";
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("POC for line", "", invoiceLine.AddInfo.ZA_POC);
			AssertEquals("Aggregated POC for line", "NZ", invoiceLine.AggregatedZA_POC);

			invoiceLine.AddInfo.ZA_POC = "US";
			AssertEquals("Aggregated POC for line", "US", invoiceLine.AggregatedZA_POC);
		}

		public void TestIsGenerateRateWithPreferentialRate()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 10, 25);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_POC = "NZ";
			invoice.AddInfo.ZA_PST = "NZ";
			invoice.AddInfo.ZA_PRT = "P50";

			CMRTariffRatePeriodSnapshot rateWithoutPreferentialRate = CMRTariffRatePeriodSnapshot.New(Factory);
			rateWithoutPreferentialRate.TT_PreferenceSchemeType = "GEN";
			rateWithoutPreferentialRate.TT_TariffClassificationNumber = "00000000";
			rateWithoutPreferentialRate.TT_StartDate = new ZDateTime(2005, 10, 25);

			CMRTariffRatePeriodSnapshot rateWithPreferentialRate1 = CMRTariffRatePeriodSnapshot.New(Factory);
			rateWithPreferentialRate1.TT_PreferenceSchemeType = "GEN";
			rateWithPreferentialRate1.TT_TariffClassificationNumber = "00000001";
			rateWithPreferentialRate1.TT_StartDate = new ZDateTime(2005, 10, 25);

			CMRTariffRatePeriodSnapshot rateWithPreferentialRate2 = CMRTariffRatePeriodSnapshot.New(Factory);
			rateWithPreferentialRate2.TT_PreferenceSchemeType = "US";
			rateWithPreferentialRate2.TT_TariffClassificationNumber = "00000001";
			rateWithPreferentialRate2.TT_StartDate = new ZDateTime(2005, 10, 25);

			CMRPreferenceSchemePeriodCountry schemeCountryUS = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountryUS.PC_PreferenceSchemePeriodSnapshotSchemeType = "US";
			schemeCountryUS.PC_CountryCode = "US";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("No tariff selected yet", false, invoiceLine.IsGeneralRate);
			invoiceLine.JI_Tariff = "00000000";
			AssertEquals("IsGeneral rate as there is no other preferential rate", true, invoiceLine.IsGeneralRate);

			invoiceLine.JI_Tariff = "00000001 00";
			invoiceLine.AddInfo.ZA_POC = "US";
			AssertEquals("IsGeneral rate", false, invoiceLine.IsGeneralRate);

			invoiceLine.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			AssertEquals("IsGeneral rate", true, invoiceLine.IsGeneralRate);
		}

		public void TestDefaultingOfRNOOnTariffChanges()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);

			CMRTariffRatePeriodSnapshot gENRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			gENRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TT_RateNumber = "001";
			gENRateNumber.TT_PreferenceSchemeType = "GEN";
			gENRateNumber.TT_CalculationType = "FREE";
			gENRateNumber.TT_TariffClassificationNumber = "49011000";

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00 01";
			AssertEquals("Rate number defaulted", "001", line.AddInfo.ZA_RNO);
		}

		public void TestIUltimateDistributeeWithAUSpecificFunction()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			invoiceLine.JI_Weight = 1;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			invoiceLine.JI_Volume = 200;
			invoiceLine.JI_VolumeUQ = Core.Constants.Volume.Litre;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryLine.CL_CustomsValue = 1000m;

			entryLine.CL_DutyPercent = 10;
			CusEntryLineFee gSTVAT = entryLine.Fees.AddNew();
			gSTVAT.CF_ChargeType = CusEntryChargeTypeList.Codes.GSTAmount;
			gSTVAT.CF_ChargeAmount = 100m;

			CusEntryLineFee duty = entryLine.Fees.AddNew();
			duty.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			duty.CF_ChargeAmount = 99m;
			duty = entryLine.Fees.AddNew();
			duty.CF_ChargeType = CusEntryChargeTypeList.Codes.DutyAmount;
			duty.CF_IsLandedCostOnly = true;
			duty.CF_ChargeAmount = 11m;

			CusEntryLineFee wET = entryLine.Fees.AddNew();
			wET.CF_ChargeType = CusEntryChargeTypeList.Codes.WetAmount;
			wET.CF_ChargeAmount = 200m;
			wET = entryLine.Fees.AddNew();
			wET.CF_ChargeType = CusEntryChargeTypeList.Codes.WetAmount;
			wET.CF_IsLandedCostOnly = true;
			wET.CF_ChargeAmount = 2m;

			CusEntryLineFee lCT = entryLine.Fees.AddNew();
			lCT.CF_ChargeType = CusEntryChargeTypeList.Codes.LCTAmount;
			lCT.CF_ChargeAmount = 300m;
			lCT = entryLine.Fees.AddNew();
			lCT.CF_ChargeType = CusEntryChargeTypeList.Codes.LCTAmount;
			lCT.CF_IsLandedCostOnly = true;
			lCT.CF_ChargeAmount = 3m;

			CusEntryLineFee flat = entryLine.Fees.AddNew();
			flat.CF_ChargeType = CusEntryChargeTypeList.Codes.FlatDutyPortion;
			flat.CF_ChargeAmount = 400m;

			CusEntryLineFee dumping = entryLine.Fees.AddNew();
			dumping.CF_ChargeType = CusEntryChargeTypeList.Codes.DumpingDuty;
			dumping.CF_ChargeAmount = 500m;

			var woodLevy = entryHeader.Charges.AddNew();
			woodLevy.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			woodLevy.C1_IsLandedCostOnly = true;
			woodLevy.C1_ChargeAmount = 5m;
			woodLevy = entryHeader.Charges.AddNew();
			woodLevy.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			woodLevy.C1_ChargeAmount = 500m;

			var entryFee = entryHeader.Charges.AddNew();
			entryFee.C1_ChargeAmount = 600m;
			entryFee.C1_ChargeType = CusEntryChargeTypeList.Codes.DeclarationProcessingCharge;

			var aQISService = entryHeader.Charges.AddNew();
			aQISService.C1_ChargeAmount = 700m;
			aQISService.C1_ChargeType = CusEntryChargeTypeList.Codes.AQISServicePaymentAmount;

			DutyTaxEntryFee lineDutyTaxEntryFee = ((IUltimateDistributee)invoiceLine).LineDutyTaxEntryFeeItems;

			AssertEquals("IUltimateDistributee.EntryFee", 600m, lineDutyTaxEntryFee["ENT"]);
			AssertEquals("IUltimateDistributee.AQISServicePaymentAmount", 700m, lineDutyTaxEntryFee["QUA"]);
			AssertEquals("IUltimateDistributee.DutyPercent", 10m, ((IUltimateDistributee)invoiceLine).DutyPercent);
			AssertEquals("IUltimateDistributee.Duty", 110m, lineDutyTaxEntryFee["TDT"]);
			AssertEquals("IUltimateDistributee.WET", 202m, lineDutyTaxEntryFee["ST1"]);
			AssertEquals("IUltimateDistributee.LCT", 303m, lineDutyTaxEntryFee["ST2"]);
			AssertEquals("IUltimateDistributee.WoodLevy", 505m, lineDutyTaxEntryFee["ST3"]);
			AssertEquals("IUltimateDistributee.OtherOrFlatDutyAmount", 900m, lineDutyTaxEntryFee["OTH"]);
			AssertEquals("IUltimateDistributee.Excise", 0m, lineDutyTaxEntryFee["EXC"]);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("IUltimateDistributee.Actual for Air", 1000m, ((IUltimateDistributee)invoiceLine).Actual);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IUltimateDistributee.Actual for SEA", 0.2m, ((IUltimateDistributee)invoiceLine).Actual);
		}

		public void TestWoodLevy()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 5000m;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_CL = entryLine.PK;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 4000m;
			invoiceLine2.JI_CL = entryLine.PK;

			var estimatedWoodLevyNotPayabe = entryHeader.Charges.AddNew();
			estimatedWoodLevyNotPayabe.C1_ChargeType = CusEntryChargeTypeList.Codes.Woodlevy;
			estimatedWoodLevyNotPayabe.C1_ChargeAmount = 5m;
			estimatedWoodLevyNotPayabe.C1_IsLandedCostOnly = true;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.Woodlevy].C1_ChargeAmount = 500m;

			AssertEquals("Wood levy calculated for InvoiceLine1", 100m, invoiceLine1.JI_Calc_WoodLevy);
			AssertEquals("Wood levy calculated for InvoiceLine2", 400m, invoiceLine2.JI_Calc_WoodLevy);
			AssertEquals("Wood levy calculated for InvoiceLine1", 101m, invoiceLine1.JI_Calc_WoodLevyIncludingWHEstimate);
			AssertEquals("Wood levy calculated for InvoiceLine2", 404m, invoiceLine2.JI_Calc_WoodLevyIncludingWHEstimate);
		}

		public void TestAllEntryFees()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 5000m;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_CL = entryLine.PK;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 4000m;
			invoiceLine2.JI_CL = entryLine.PK;

			entryHeader.Charges[CusEntryChargeTypeList.Codes.EntryFee].C1_ChargeAmount = 100m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.MessageFee].C1_ChargeAmount = 200m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.ScreenFree].C1_ChargeAmount = 300m;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.TradegateGST].C1_ChargeAmount = 400m;

			AssertEquals("All Entry Fee calculated for invoiceline1", 200m, invoiceLine1.JI_Calc_AllEntryFees);
			AssertEquals("All Entry Fee calculated for invoiceline2", 800m, invoiceLine2.JI_Calc_AllEntryFees);
		}

		public void TestTradegateGST()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 5000m;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_CL = entryLine.PK;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 4000m;
			invoiceLine2.JI_CL = entryLine.PK;

			entryHeader.Charges[CusEntryChargeTypeList.Codes.TradegateGST].C1_ChargeAmount = 500m;

			AssertEquals("All TradegateGST calculated for invoiceline1", 100m, invoiceLine1.JI_Calc_TradegateGST);
			AssertEquals("All TradegateGST calculated for invoiceline2", 400m, invoiceLine2.JI_Calc_TradegateGST);
		}

		public void TestDumpingDuty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 5000m;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_CL = entryLine.PK;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 4000m;
			invoiceLine2.JI_CL = entryLine.PK;

			entryLine.Fees.GetOrAddFeeByFeeType(CusEntryChargeTypeList.Codes.DumpingDuty).CF_ChargeAmount = 500m;

			AssertEquals("DumpingDuty for InvoiceLine1", 100m, invoiceLine1.JI_Calc_DumpingDuty);
			AssertEquals("DumpingDuty for InvoiceLine2", 400m, invoiceLine2.JI_Calc_DumpingDuty);
		}

		public void TestCountervailingDuty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 5000m;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_CL = entryLine.PK;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 4000m;
			invoiceLine2.JI_CL = entryLine.PK;

			entryLine.Fees.GetOrAddFeeByFeeType(CusEntryChargeTypeList.Codes.CountervailingDuty).CF_ChargeAmount = 500m;

			AssertEquals("CountervailingDuty for InvoiceLine1", 100m, invoiceLine1.JI_Calc_CountervailingDuty);
			AssertEquals("CountervailingDuty for InvoiceLine2", 400m, invoiceLine2.JI_Calc_CountervailingDuty);
		}

		public void TestCloneWithPartsWorks()
		{
			ZQuery orgFilter = new ZQuery();
			orgFilter.MaximumRows = 2;
			OrgHeader[] organisations = (OrgHeader[])Factory.Load(typeof(OrgHeader), orgFilter);

			AUOrgSupplierPart part = CreateAndSavePart(organisations[0], organisations[1], "12345", ZString.Empty, Classification.ClassificationType.IMP);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = organisations[0].PK;
			declaration.JE_OH_Supplier = organisations[1].PK;
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();

			line1.JI_PartNo = "12345";
			AssertEquals("Line1.Part", part, line1.Part);
			Factory.Save();

			JobDeclaration clonedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Number of Lines", 1, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);

			JobComInvoiceLine clonedLine1 = clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			AssertEquals("Cloned Line1.Part", part, clonedLine1.Part);
		}

		AUOrgSupplierPart CreateAndSavePart(OrgHeader owner, OrgHeader supplier, string partNum, ZString tariff, string classificationType)
		{
			AUOrgSupplierPart part = AUOrgSupplierPart.New(Factory);
			part.OP_PartNum = "12345";
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_OH = owner.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_OH = supplier.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			if (!tariff.IsEmpty)
			{
				Classification @class = Factory.New<Classification>();
				@class.CC_LookupCode = "TestLookup";
				@class.CC_ClassificationType = classificationType;
				@class.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				@class.CC_TariffNum = tariff;
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_CC = @class.PK;
				pivot.CI_OP = part.PK;
			}

			Factory.Save();
			return part;
		}

		public void TestCloneWithProductWithClassWithInvalidTarrifDoesntClearCustomsQty()
		{
			ZQuery orgFilter = new ZQuery();
			orgFilter.MaximumRows = 2;
			OrgHeader[] organisations = (OrgHeader[])Factory.Load(typeof(OrgHeader), orgFilter);

			AUOrgSupplierPart part = CreateAndSavePart(organisations[0], organisations[1], "12345", "12345678", Classification.ClassificationType.IMP);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = organisations[0].PK;
			declaration.JE_OH_Supplier = organisations[1].PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();

			line1.JI_PartNo = "12345";
			AssertEquals("Tariff", "1234.56.78", line1.JI_Tariff);

			AssertEquals("Line1.Part", part, line1.Part);

			//			AssertEquals("JI_CustomsQuantityInfo.ReadOnly", false, Line1.JI_CustomsQuantityInfo.ReadOnly);
			//			AssertEquals("JI_CustomsUnitQtyInfo.ReadOnly", false, Line1.JI_CustomsUnitQtyInfo.ReadOnly);

			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 100m;

			Factory.Save();

			JobDeclaration clonedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Number of Lines", 1, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);

			JobComInvoiceLine clonedLine1 = clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0];
			AssertEquals("JI_CustomsQuantity", 100m, clonedLine1.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", "KG", clonedLine1.JI_CustomsUnitQty);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			QuarantineExDocLine quarantineExDocLine = line.QuarantineExDocLine;
			return line;
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return declaration;
		}

		public void TestHeaderDefaultsDisplay()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "US";
			invoiceHeader.AddInfo.ZA_PST = "US";
			invoiceHeader.AddInfo.ZA_PRT = "WO";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101.10.00 25";
			invoiceLine.AddInfo.RunPreSaveValidation();
			AssertHasWarningContaining(invoiceLine.AddInfo.ZA_RNOInfo, "The system will IGNORE preference and origin data entered for this Invoice line, or defaulted from the Invoice header preference and origin fields as there are no preferential rates of duty applicable to the tariff item entered and the general rate of duty will be applied.");

			invoiceLine.JI_Tariff = "4801.00.10 01";
			AssertEquals("Header POC shown on line should be same as InvoiceHeader", "US", invoiceLine.HeaderPrefOrg);
			AssertEquals("Header PST shown on line should be same as InvoiceHeader", "US", invoiceLine.HeaderPrefScheme);
			AssertEquals("Header PRT shown on line should be same as InvoiceHeader", "WO", invoiceLine.HeaderPrefRule);

			invoiceHeader.AddInfo.ZA_ORG = "CA";
			invoiceHeader.AddInfo.ZA_VALB_Hidden = "TV";
			AssertEquals("Header Origin shown on line should be same as InvoiceHeader", "CA", invoiceLine.HeaderOrigin);
			AssertEquals("Header Value Base shown on line should be same as InvoiceHeader", "TV", invoiceLine.HeaderValuationBasis);
		}

		public void TestMultiplePSTApplicableMessageError()
		{
			var newTariffRate1 = Factory.New<CMRTariffRatePeriodSnapshot>();
			newTariffRate1.TT_TariffClassificationNumber = "01011000";
			newTariffRate1.TT_PreferenceSchemeType = "DCS";
			newTariffRate1.TT_StartDate = ZDateTime.Today.AddDays(-1);
			newTariffRate1.TT_EndDate = ZDateTime.Today.AddDays(1);
			newTariffRate1.TT_RateNumber = "001";
			newTariffRate1.TT_CustomsValueRate = 1m;//100 Duty
			newTariffRate1.TT_CalculationType = Constants.DutyCalcTypes.Calc;
			var newTariffRate2 = Factory.New<CMRTariffRatePeriodSnapshot>();
			newTariffRate2.TT_TariffClassificationNumber = "01019000";
			newTariffRate2.TT_PreferenceSchemeType = "DCT";
			newTariffRate2.TT_StartDate = ZDateTime.Today.AddDays(-1);
			newTariffRate2.TT_EndDate = ZDateTime.Today.AddDays(1);
			newTariffRate2.TT_RateNumber = "001";
			newTariffRate2.TT_CustomsValueRate = 1m;//100 Duty
			newTariffRate2.TT_CalculationType = Constants.DutyCalcTypes.Calc;
			Factory.Save();

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "SG";
			invoiceHeader.AddInfo.ZA_PST = "DCT";
			invoiceHeader.AddInfo.ZA_PRT = "P50";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0102.10.00 21";
			invoiceLine1.AddInfo.RunPreSaveValidation();
			AssertHasWarningContaining(invoiceLine1.AddInfo.ZA_RNOInfo, "The system will IGNORE preference and origin data entered for this Invoice line, or defaulted from the Invoice header preference and origin fields as there are no preferential rates of duty applicable to the tariff item entered and the general rate of duty will be applied.");
			AssertNoMessageErrorContaining(invoiceLine1.AddInfo.ZA_RNOInfo, "The system cannot default preference for this line because the Invoice header preference is not applicable, but there are other preferential rates that may be applicable, or no rate (including the general rate) is applicable.");

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0101.10.00 25";
			invoiceLine2.AddInfo.RunPreSaveValidation();
			AssertNoWarningContaining(invoiceLine2.AddInfo.ZA_RNOInfo, "The system will IGNORE preference and origin data entered for this Invoice line, or defaulted from the Invoice header preference and origin fields as there are no preferential rates of duty applicable to the tariff item entered and the general rate of duty will be applied.");
			AssertHasMessageErrorContaining(invoiceLine2.AddInfo.ZA_RNOInfo, "The system cannot default preference for this line because the Invoice header preference is not applicable, but there are other preferential rates that may be applicable, or no rate (including the general rate) is applicable.");

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0101.90.00 37";
			invoiceLine3.AddInfo.RunPreSaveValidation();
			AssertNoWarningContaining(invoiceLine3.AddInfo.ZA_RNOInfo, "The system will IGNORE preference and origin data entered for this Invoice line, or defaulted from the Invoice header preference and origin fields as there are no preferential rates of duty applicable to the tariff item entered and the general rate of duty will be applied.");
			AssertNoMessageErrorContaining(invoiceLine3.AddInfo.ZA_RNOInfo, "The system cannot default preference for this line because the Invoice header preference is not applicable, but there are other preferential rates that may be applicable, or no rate (including the general rate) is applicable.");
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Assert("different to base, tested in other test cases", true);
		}

		public void TestDrawbackImportEntryLine()
		{
			JobDeclaration importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			importDeclaration.JE_DeclarationReference = "B12345678";
			JobComInvoiceHeader importInvoice = importDeclaration.Invoices.AddNew();
			JobComInvoiceLine importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = importDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			entryLine.CL_LineNumber = 2;
			importInvoiceLine.JI_CL = entryLine.PK;
			entryHeader.EntryNumber = "AAABBBCCC";
			Factory.Save();

			JobDeclaration drawbackDeclaration = Factory.New<JobDeclaration>();
			drawbackDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoice = drawbackDeclaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_DDN_Hidden = "AAABBBCCC";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 1;
			AssertNull("DrawbackImportEntryLine", invoiceLine.DrawbackImportEntryLine);
			invoiceLine.AddInfo.ZA_DDL_Hidden = 2;
			AssertEquals("DrawbackImportEntryLine", entryLine, invoiceLine.DrawbackImportEntryLine);
		}

		public override void TestWipeNKTaxType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			Assert(!invoiceLine.ShouldWipeNKTaxType);
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);

			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		protected override bool UseUniversalTariff => false;

		protected override bool ShouldMatchHTBForTestPivot => false;

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionedCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
