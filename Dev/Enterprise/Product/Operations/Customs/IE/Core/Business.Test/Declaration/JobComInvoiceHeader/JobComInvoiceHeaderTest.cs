using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestIncotermDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_ShipmentIncoTerm = "FOB";
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("UCC5 should default Incoterm from declaration", "FOB", invoiceHeader.JZ_IncoTerm);
		}

		public void TestHas51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction()
		{
			CombineAssertions("InvoiceHeader: Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction", () =>
			{
				AssertEquals("False by default.", false, InvoiceHeader.Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction);

				var instruction = Declaration.CustomsEntryInstructions.AddNew();

				var supportingDocumentC601 = instruction.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty);
				supportingDocumentC601.CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
				AssertEquals("False when only C601.", false, InvoiceHeader.Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction);

				instruction.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("False when C601 & 00100 INF on Instruction but not linked.", false, InvoiceHeader.Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction);

				var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				AssertEquals("False without 51 InvoiceLine.", false, InvoiceHeader.Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._51;
				AssertEquals("False without 51 InvoiceLine.", true, InvoiceHeader.Has51ProcedureAndC601SupportingDocumentAnd00100AdditionalInformationCusEntryInstruction);
			});
		}

		public void TestIsProcedureCode44()
		{
			AssertEquals("No Instruction", false, InvoiceHeader.IsProcedureCode44);

			var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();

			invoiceLine1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._44;
			AssertEquals("Instruction1 Procedure is 44, Instruction2 Procedure is not 44", false, InvoiceHeader.IsProcedureCode44);

			invoiceLine2.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._44;
			AssertEquals("Both Procedures are 44", true, InvoiceHeader.IsProcedureCode44);

			invoiceLine1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76;
			AssertEquals("Instruction1 Procedure is not 44, Instruction2 Procedure is 44", false, InvoiceHeader.IsProcedureCode44);
		}

		public void TestHasMutuallyExclusiveSupportingDocument()
		{
			AssertEquals("empty collection", false, InvoiceHeader.HasMutuallyExclusiveSupportingDocument);

			var supportingDoc = InvoiceHeader.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "666";
			AssertEquals("without target supportingDocument", false, InvoiceHeader.HasMutuallyExclusiveSupportingDocument);

			supportingDoc.CSI_Code = "U164";
			var supportingDoc2 = InvoiceHeader.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "U165";
			AssertEquals(true, InvoiceHeader.HasMutuallyExclusiveSupportingDocument);

			supportingDoc.CSI_Code = "U167";
			AssertEquals("Specific case", false, InvoiceHeader.HasMutuallyExclusiveSupportingDocument);
		}

		public void TestDefaultIncoTermFromSupplier()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "EXP";

			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			var orgMiscServ = supplier.MiscServ;
			orgMiscServ.OM_EXDefaultIncoTerm = "FOB";

			var invoice1 = declaration.Invoices.AddNew();
			AssertEquals("EXP will set default value", "FOB", invoice1.JZ_IncoTerm);

			declaration.JE_MessageType = "EXS";
			var invoice2 = declaration.Invoices.AddNew();
			Assert("EXS will not set default value", invoice2.JZ_IncoTerm.IsEmpty);

			declaration.JE_MessageType = "REX";
			var invoice3 = declaration.Invoices.AddNew();
			Assert("REX will not set default value", invoice3.JZ_IncoTerm.IsEmpty);
		}

		public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = Factory.New<JobComInvoiceHeader>();
				invoiceHeader.JZ_JE = declaration.PK;
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "CustomsChargeTypeList_IEIMPInvoice", invoiceHeader.GetCustomsChargeTypeListCacheKey(ChargeParentTypes.Invoice));

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "CustomsChargeTypeList_IEEXPInvoice", invoiceHeader.GetCustomsChargeTypeListCacheKey(ChargeParentTypes.Invoice));
			});
		}

		public void TestJZ_OH_Supplier_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(InvoiceHeader.JZ_OH_SupplierInfo, (string)null, "Supplier");
		}

		public override void TestChargeTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			Customs.Business.ICommonInvoice invoice = declaration.Invoices.AddNew();
			var chargeTypeList = invoice.ChargeTypeList;
			var chargeTypeList2 = invoice.ChargeTypeList;
			AssertSame("Cached", chargeTypeList, chargeTypeList2);
			AssertEquals("ADD, AFT, DED, INS, OFT, ONS", chargeTypeList.CodesAsString);
		}

		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			Assert(true);
		}

		public void TestJZ_ValuationCode_Cpation()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(InvoiceHeader.JZ_ValuationCodeInfo, JobDeclaration.CaptionKeyExportUCC6, "Nature of Transaction", shortCaption: "Nature of Trans.", fullDescription: "[99 05 001 000] Nature of Transaction");

			this.AssertDataBoundResourceStringsWithMultipleResourceKey(InvoiceHeader.JZ_ValuationCodeInfo, JobDeclaration.CaptionKeyImportUCC5, "[8/5] Nature of Transaction", shortCaption: "[8/5] Trans. Nature", mediumCaption: "[8/5] Trans. Nature");
		}

		public void TestJZ_UCR()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_UCRInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Commercial Ref.", captionResourceString.Caption);
		}

		public void TestZG_TransportChargesMethodOfPayment_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().ZG_TransportChargesMethodOfPaymentInfo, JobDeclaration.CaptionKeyExportUCC6, "Transport Charges MoP", shortCaption: "Charges MoP", mediumCaption: "Trans. Chrg. MoP", fullDescription: "[14 02 038 000] Transport Charges - Method of Payment");
		}

		public void TestZG_AgreedPlaceCode_Caption()
		{
			CombineAssertions("UCC6", () =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().ZG_AgreedPlaceCodeInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption", "Incoterm Place", captionResourceString.Caption);
				AssertEquals("FullDescription", "[14 01 036 000] UN/LOCODE or [14 01 020 000] Country code", captionResourceString.FullDescription);
			});

			CombineAssertions("UCC5", () =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().ZG_AgreedPlaceCodeInfo, JobDeclaration.CaptionKeyImportUCC5);
				AssertEquals("ShortCaption", "[4/1] Place Code", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[4/1] Inco. Place Code", captionResourceString.MediumCaption);
				AssertEquals("Caption", "[4/1] Incoterm Place Code", captionResourceString.Caption);
			});
		}

		public void TestJZ_AdditionalTerms_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_AdditionalTermsInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Delivery Terms", captionResourceString.Caption);
				AssertEquals("[14 01 000 000] Delivery Terms Text", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_InvoiceAmount_CaptionUCC5IMP()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, JobDeclaration.CaptionKeyImportUCC5);
				AssertEquals("[4/11] Inv. Amount", captionResourceString.ShortCaption);
				AssertEquals("[4/11] Inv. Amount", captionResourceString.MediumCaption);
				AssertEquals("[4/11 & 4/10] Invoice Amount & Currency", captionResourceString.Caption);
				AssertEquals("[4/11] Total Amount Invoiced & [4/10] Invoice Currency", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_IncoTerm_CaptionUCC5IMP()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_IncoTermInfo, JobDeclaration.CaptionKeyImportUCC5);
				AssertEquals("Caption", "[4/1] Incoterm", captionResourceString.Caption);
			});
		}

		public void TestJZ_IncoTermPlace_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_IncoTermPlaceInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Incoterm Location", captionResourceString.Caption);
				AssertEquals("[14 01 037 000] Delivery Terms Location", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_IncoTermPlace_MaxLength()
		{
			AssertEquals("MaxLength", 35, InvoiceHeader.JZ_IncoTermPlaceInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestGetCusSupportingInfoTypes_PreviousDocument()
		{
			AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceHeader).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		public void TestGetCusSupportingInfoTypes_AdditionalInfo()
		{
			AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceHeader).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestGetCusSupportingInfoTypes_SupportingDocument()
		{
			AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)InvoiceHeader).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(InvoiceHeader.PreviousDocuments);
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(InvoiceHeader.AdditionalInfos);
		}

		public void TestSupportingDocuments()
		{
			AssertType<SupportingDocumentCollection>(InvoiceHeader.SupportingDocuments);
		}

		public void TestImportIncoTermAndChargeFactory()
		{
			Declaration.JE_MessageType = "IMP";
			AssertType<ImportIncoTermAndCustomsChargeFactory>(InvoiceHeader.IncoTermAndChargeFactory);

			Declaration.JE_MessageType = "EXP";
			AssertType<ExportIncoTermAndCustomsChargeFactory>(InvoiceHeader.IncoTermAndChargeFactory);
		}

		public void TestLookups()
		{
			AssertType<JobComInvoiceHeaderLookups>(InvoiceHeader.Lookups);
		}

		public void TestValidation()
		{
			CombineAssertions(() =>
			{
				var invoice = Factory.New<JobComInvoiceHeader>();
				var list = new List<string>(invoice.Lookups.MessageTypes.GetAllCodes());
				list.Remove(IEJobMessageTypeList.Codes.Export);
				invoice.JZ_StandAloneInvoiceDirection = IEJobMessageTypeList.Codes.Export;
				AssertType<ExportJobComInvoiceHeaderValidation>("Standalone EXP", invoice.Validation);

				list.Remove(IEJobMessageTypeList.Codes.Import);
				invoice.JZ_StandAloneInvoiceDirection = IEJobMessageTypeList.Codes.Import;
				AssertType<ImportJobComInvoiceHeaderValidation>("IMPORT", invoice.Validation);

				foreach (var code in list)
				{
					invoice.JZ_StandAloneInvoiceDirection = code;
					AssertType<EU.Business.Declaration.JobComInvoiceHeaderValidation>("Standalone " + code, invoice.Validation);
				}
				invoice = InvoiceHeader;
				list = list = new List<string>(invoice.Lookups.MessageTypes.GetAllCodes());
				list.Remove(IEJobMessageTypeList.Codes.Export);
				Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertType<ExportJobComInvoiceHeaderValidation>("EXP", invoice.Validation);
				list.Remove(IEJobMessageTypeList.Codes.ReExport);
				Declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
				AssertType<ReExportJobComInvoiceHeaderValidation>("REX", invoice.Validation);
				list.Remove(IEJobMessageTypeList.Codes.ExitSummary);
				Declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
				AssertType<ExitSummaryJobComInvoiceHeaderValidation>("EXS", invoice.Validation);

				list.Remove(IEJobMessageTypeList.Codes.Import);
				Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<ImportJobComInvoiceHeaderValidation>("IMPORT", invoice.Validation);

				foreach (var code in list)
				{
					Declaration.JE_MessageType = code;
					AssertType<EU.Business.Declaration.JobComInvoiceHeaderValidation>(code, invoice.Validation);
				}
			});
		}

		public void TestHasSupportingDocumentForExportWithInvoiceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - No supporting Docs", false, InvoiceHeader.HasSupportingDocumentForExportWithInvoiceNumber);
				var supportingDoc1 = InvoiceHeader.SupportingDocuments.AddNew();
				supportingDoc1.CSI_Code = "XXXX";
				supportingDoc1.CSI_ReferenceNumber = string.Empty;
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 invalid supporting doc", false, InvoiceHeader.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc1.CSI_Code = Constants.SupportingDocumentCodes._N325;
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 invalid supporting doc", false, InvoiceHeader.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc1.CSI_ReferenceNumber = "REFNO1";
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 valid supporting doc", true, InvoiceHeader.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc1.CSI_Code = "XXXX";
				var supportingDoc2 = InvoiceHeader.SupportingDocuments.AddNew();
				supportingDoc2.CSI_Code = Constants.SupportingDocumentCodes._D005;
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 2 invalid supporting docs", false, InvoiceHeader.HasSupportingDocumentForExportWithInvoiceNumber);
				supportingDoc2.CSI_ReferenceNumber = "REFNO2";
				AssertEquals("HasSupportingDocumentForExportWithInvoiceNumber - 1 valid, 1 invalid supporting docs", true, InvoiceHeader.HasSupportingDocumentForExportWithInvoiceNumber);
			});
		}

		public void TestHasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - No supporting Docs", false, InvoiceHeader.HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber);
				var supportingDoc1 = instruction.SupportingDocuments.AddNew();
				supportingDoc1.CSI_Code = "XXXX";
				supportingDoc1.CSI_ReferenceNumber = string.Empty;
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 invalid supporting doc", false, InvoiceHeader.HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc1.CSI_Code = Constants.SupportingDocumentCodes._N325;
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 invalid supporting doc", false, InvoiceHeader.HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc1.CSI_ReferenceNumber = "REFNO1";
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 valid supporting doc", true, InvoiceHeader.HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc1.CSI_Code = "XXXX";
				var supportingDoc2 = instruction.SupportingDocuments.AddNew();
				supportingDoc2.CSI_Code = Constants.SupportingDocumentCodes._D005;
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 2 invalid supporting docs", false, InvoiceHeader.HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber);
				supportingDoc2.CSI_ReferenceNumber = "REFNO2";
				AssertEquals("HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber - 1 valid, 1 invalid supporting docs", true, InvoiceHeader.HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber);
			});
		}

		public void TestHasEstimatedTimeOfDepartureAdditionalInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - No additional infos", false, InvoiceHeader.HasEstimatedTimeOfDepartureAdditionalInfo);
				var additionalInfo1 = InvoiceHeader.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "XXXX";
				additionalInfo1.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 1 invalid additional infos", false, InvoiceHeader.HasEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 1 valid additional infos", true, InvoiceHeader.HasEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = "XXXX";
				var additionalInfo2 = InvoiceHeader.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "YYYY";
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 2 invalid additional infos", false, InvoiceHeader.HasEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo2.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				additionalInfo2.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEstimatedTimeOfDepartureAdditionalInfo - 1 valid, 1 invalid additional infos", true, InvoiceHeader.HasEstimatedTimeOfDepartureAdditionalInfo);
			});
		}

		public void TestHasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - No additional infos", false, InvoiceHeader.HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo);
				var additionalInfo1 = instruction.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "XXXX";
				additionalInfo1.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 1 invalid additional infos", false, InvoiceHeader.HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 1 valid additional infos", true, InvoiceHeader.HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo1.CSI_Code = "XXXX";
				var additionalInfo2 = instruction.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "YYYY";
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 2 invalid additional infos", false, InvoiceHeader.HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo);
				additionalInfo2.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
				additionalInfo2.CSI_ReferenceNumber = "202210101000";
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 1 valid, 1 invalid additional infos", true, InvoiceHeader.HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo);
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
				AssertEquals("HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo - 1 valid, 1 invalid additional infos, CEI_Substyle not applicable", false, InvoiceHeader.HasEntryInstructionNeedingEstimatedTimeOfDepartureAdditionalInfo);
			});
		}

		public void TestHasTransportDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("HasTransportDocument - does not have TransportDocument", false, InvoiceHeader.HasTransportDocument);

				var invHeaderAddInfo = InvoiceHeader.AdditionalInfos.AddNew();
				invHeaderAddInfo.CSI_SubType = "YYY";
				invHeaderAddInfo.CSI_Description = "YYY Description";

				AssertEquals("HasTransportDocument - does not have TransportDocument", false, InvoiceHeader.HasTransportDocument);

				invHeaderAddInfo = InvoiceHeader.AdditionalInfos.AddNew();
				invHeaderAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				invHeaderAddInfo.CSI_Description = "TRA Description";

				AssertEquals("HasTransportDocument - has TransportDocument", true, InvoiceHeader.HasTransportDocument);
			});
		}

		public void TestHasEntryInstructionWithTransportDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("HasEntryInstructionWithTransportDocument - CustomsEntryInstructions empty, does not have TransportDocument", false, InvoiceHeader.HasEntryInstructionWithTransportDocument);

				var instruction = Declaration.CustomsEntryInstructions.AddNew();
				var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;

				var addInfo1 = instruction.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = "YYY";
				addInfo1.CSI_Description = "YYY Description";

				AssertEquals("HasEntryInstructionWithTransportDocument - does not have TransportDocument", false, InvoiceHeader.HasEntryInstructionWithTransportDocument);

				var addInfo2 = instruction.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo2.CSI_Description = "TRA Description";

				AssertEquals("HasEntryInstructionWithTransportDocument - has TransportDocument", true, InvoiceHeader.HasEntryInstructionWithTransportDocument);
			});
		}

		public void TestAllRelatedEntryInstructionsStatisticalValueRequired()
		{
			AssertEquals("AllRelatedEntryInstructionsStatisticalValueRequired - CustomsEntryInstructions empty", false, InvoiceHeader.AllRelatedEntryInstructionsStatisticalValueRequired);
			var instruction1 = Declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			var instruction2 = Declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;
			var instruction3 = Declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine3 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction3.PK;

			instruction1.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			AssertEquals("AllRelatedEntryInstructionsStatisticalValueRequired - Not all CEI_Style set", false, InvoiceHeader.AllRelatedEntryInstructionsStatisticalValueRequired);
			instruction2.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			instruction3.CEI_Style = ExportDeclarationTypeList.Codes.C1;
			AssertEquals("AllRelatedEntryInstructionsStatisticalValueRequired - Not all Statistical Value Required", false, InvoiceHeader.AllRelatedEntryInstructionsStatisticalValueRequired);
			instruction3.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			AssertEquals("AllRelatedEntryInstructionsStatisticalValueRequired - All Statistical Value Required", true, InvoiceHeader.AllRelatedEntryInstructionsStatisticalValueRequired);
		}

		public void TestHasInvoiceLineWithPreviousProcedure21Or22()
		{
			var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "0011";
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "0011";
			AssertEquals("HasInvoiceLineWithPreviousProcedure21Or22", false, InvoiceHeader.HasInvoiceLineWithPreviousProcedure21Or22);
			invoiceLine2.JI_Procedure = "0021";
			AssertEquals("HasInvoiceLineWithPreviousProcedure21Or22", true, InvoiceHeader.HasInvoiceLineWithPreviousProcedure21Or22);
			invoiceLine2.JI_Procedure = "0011";
			invoiceLine1.JI_Procedure = "0022";
			AssertEquals("HasInvoiceLineWithPreviousProcedure21Or22", true, InvoiceHeader.HasInvoiceLineWithPreviousProcedure21Or22);
		}

		public void TestHasInvoiceLineWithPreviousProcedure0700()
		{
			var invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "0000";
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "0711";
			AssertEquals("HasInvoiceLineWithPreviousProcedure0700", false, InvoiceHeader.HasInvoiceLineWithPreviousProcedure0700);
			invoiceLine2.JI_Procedure = "0700";
			AssertEquals("HasInvoiceLineWithPreviousProcedure0700", true, InvoiceHeader.HasInvoiceLineWithPreviousProcedure0700);
		}

		public void TestHasN018SupportingDocument()
		{
			Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var supDoc = InvoiceHeader.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				supDoc.CSI_Code = "YYYY";
				AssertEquals(false, InvoiceHeader.HasN018SupportingDocument);

				supDoc.CSI_Code = Constants.SupportingDocumentCodes._N018;
				AssertEquals(true, InvoiceHeader.HasN018SupportingDocument);
			});
		}

		public void TestGetSupportingDocumentsAtAnyLevel()
		{
			var doc2 = InvoiceHeader.SupportingDocuments.AddNew();
			doc2.CSI_Code = "DOC2";

			var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
			var doc3 = invoiceLine.SupportingDocuments.AddNew();
			doc3.CSI_Code = "DOC3";

			var supportingDocs = InvoiceHeader.SupportingDocumentsAtAnyLevel;
			AssertEquals(true, supportingDocs.Any(doc => doc.CSI_Code == "DOC2"));
			AssertEquals(true, supportingDocs.Any(doc => doc.CSI_Code == "DOC3"));
		}

		public void TestGetAdditionalInfosAtAnyLevel()
		{
			var doc2 = InvoiceHeader.AdditionalInfos.AddNew();
			doc2.CSI_Code = "DOC2";

			var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
			var doc3 = invoiceLine.AdditionalInfos.AddNew();
			doc3.CSI_Code = "DOC3";

			var transportDocs = InvoiceHeader.AdditionalInfosAtAnyLevel;
			AssertEquals(true, transportDocs.Any(doc => doc.CSI_Code == "DOC2"));
			AssertEquals(true, transportDocs.Any(doc => doc.CSI_Code == "DOC3"));
		}

		public void TestC100SupportingDocReferences()
		{
			var supDoc1 = InvoiceHeader.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = SupportingDocumentCodes._C100;
			supDoc1.CSI_ReferenceNumber = "X200";

			var supDoc2 = InvoiceHeader.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = SupportingDocumentCodes._C100;
			supDoc2.CSI_ReferenceNumber = "X100";

			var supDoc3 = InvoiceHeader.SupportingDocuments.AddNew();
			supDoc3.CSI_Code = SupportingDocumentCodes._C502;
			supDoc3.CSI_ReferenceNumber = "X300";

			var invoiceHeader2 = Declaration.Invoices.AddNew();
			var supDoc4 = invoiceHeader2.SupportingDocuments.AddNew();
			supDoc4.CSI_Code = SupportingDocumentCodes._C100;
			supDoc4.CSI_ReferenceNumber = "X400";
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("InvoiceHeader", new[] { "X200", "X100" }, InvoiceHeader.C100SupportingDocReferences);
				AssertContainsExactElementsInExactOrder("invoiceHeader2", new[] { "X400" }, invoiceHeader2.C100SupportingDocReferences);
			});
		}

		public void TestIsRuleBR4010Active()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "V1";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = "H2";

			CombineAssertions(() =>
			{
				AssertEquals(false, invoice.IsRuleBR4010Active);

				instruction.CEI_Style = "H1";
				AssertEquals(true, invoice.IsRuleBR4010Active);
			});
		}

		public override void TestInvoiceLineChargeAffectsBalanceCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			declaration.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertEquals(0m, invoice.JZ_Calc_ChargesExcludedFromITOT);
				AssertNotEquals("Not balanced yet", 0m, invoice.JZ_Calc_Balance);

				var lineCharge = invoiceLine2.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 5000m, declaration.LocalCurrencyCode);
				lineCharge.J7_IsDutiable = false;
				lineCharge.J7_IsGSTApplicable = false;
				lineCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
				declaration.ResumeApportionment();
				Assert("Pre-Req - lineCharge is includedInInvoiceAmount", lineCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("line level Discount", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
				AssertEquals("Now balanced", 0m, invoice.JZ_Calc_Balance);

				var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 5000m, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("Invoice Level Discount. Line level Discount is disregarded", -5000m, invoice.JZ_Calc_ChargesExcludedFromITOT);
				AssertEquals("Still balanced", 0m, invoice.JZ_Calc_Balance);
			});
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Ireland;

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override void AssertDefaultDocumentWhenApplicationCodeChanges(EU.Business.Declaration.JobDeclaration declaration, EU.Business.Declaration.JobComInvoiceHeader invoice, string messageType, string defaultInvoiceDocumentCode)
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoice.JZ_InvoiceNumber = "6";
			invoice.JZ_InvoiceDate = DateTime.Today;
			AssertEquals($"{messageType}, In Ireland, {defaultInvoiceDocumentCode} document should be added for BLT", 1, invoice.SupportingDocuments.Count);
			invoice.SupportingDocuments.RemoveAndDeleteAll();

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			invoice.JZ_InvoiceNumber = "7";
			invoice.JZ_InvoiceDate = DateTime.Today;
			AssertEquals($"{messageType}, In Ireland, {defaultInvoiceDocumentCode} document should be added for UCC5", 1, invoice.SupportingDocuments.Count);
			invoice.SupportingDocuments.RemoveAndDeleteAll();

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			invoice.JZ_InvoiceNumber = "8";
			invoice.JZ_InvoiceDate = DateTime.Today;
			AssertEquals($"{messageType}, In Ireland, {defaultInvoiceDocumentCode} document should be added for UCC6", 1, invoice.SupportingDocuments.Count);
			invoice.SupportingDocuments.RemoveAndDeleteAll();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			invoice.JZ_InvoiceNumber = "9";
			invoice.JZ_InvoiceDate = DateTime.Today;
			AssertEquals($"{messageType}, In Ireland, {defaultInvoiceDocumentCode} document should be added for ITF", 1, invoice.SupportingDocuments.Count);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoice.JZ_InvoiceNumber = "10";
			invoice.JZ_InvoiceDate = DateTime.Today;
			AssertEquals($"{messageType}, In Ireland, {defaultInvoiceDocumentCode} document should be added for BLT", 1, invoice.SupportingDocuments.Count);
			invoice.SupportingDocuments.RemoveAndDeleteAll();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			invoice.JZ_InvoiceNumber = "11";
			invoice.JZ_InvoiceDate = DateTime.Today;
			AssertEquals($"{messageType}, In Ireland, {defaultInvoiceDocumentCode} document should be added for ITF", 1, invoice.SupportingDocuments.Count);
		}

		[TestDate(2024, 3, 1)]
		public void TestEffectiveValuationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals(new ZDate(2024, 3, 1), invoice.EffectiveValuationDate);

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "H1";
			entryInstruction1.CEI_SubStyle = "A";
			entryInstruction1.CEI_DateForDuty = new ZDate(2024, 3, 10);
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "H2";
			entryInstruction2.CEI_SubStyle = "D";
			entryInstruction2.CEI_DateForDuty = new ZDate(2024, 3, 14);

			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			AssertEquals(new ZDate(2024, 3, 10), invoice.EffectiveValuationDate);
		}

		protected override bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO) => base.ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO) || bO is OfficeCode;

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)invoiceHeader;
		JobDeclaration Declaration => (JobDeclaration)declaration;
	}
}
