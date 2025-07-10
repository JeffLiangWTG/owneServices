using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(PreviousDocument))]
	sealed class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
	{
		public void TestValidation()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
				var previousDocument = invoiceLine.PreviousDocuments.AddNew();
				AssertType<PreviousDocumentValidation>("Default", previousDocument.Validation);
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertType<ImportPreviousDocumentValidation>("Import", previousDocument.Validation);
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertType<ExportPreviousDocumentValidation>("Export-PreviousDocument", previousDocument.Validation);
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				AssertType<ExportPreviousProcedureValidation>("Export-PreviousProcedure", previousDocument.Validation);
			});
		}

		public void TestValidation_CusClassPartPivot()
		{
			var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "POOPY";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.EXP;
			var previousDocumentForPivot = pivot.PreviousDocuments.AddNew();
			AssertType<ExportPreviousDocumentValidation>("PreviousDocumentsTab is only available for Export", previousDocumentForPivot.Validation);
		}

		public void TestCSI_ReferenceNumber2_ReadOnly()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			Assert(previousDocument.CSI_ReferenceNumber2Info.ReadOnly);

			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			Assert(!previousDocument.CSI_ReferenceNumber2Info.ReadOnly);

			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			Assert(!previousDocument.CSI_ReferenceNumber2Info.ReadOnly);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._POST;
			previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			Assert(!previousDocument.CSI_ReferenceNumber2Info.ReadOnly);
		}

		public void TestSaving()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.CSI_SubType = "123";
			previousDocument.CSI_LineNo = 1;

			Factory.Save();

			AssertEquals(ZString.Empty, previousDocument.CSI_SubType);
			AssertEquals(ZShort.Zero, previousDocument.CSI_LineNo);
		}

		public void TestAuthorizationNumber()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._A;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			AssertType<ZString>(previousDocument.AuthorizationNumber);
			AssertEquals(35, previousDocument.AuthorizationNumberInfo.MaxLength);
		}

		public void TestCSI_Quantity()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.UsualProcessingFlag = true;
			previousDocument.CSI_Quantity = 1;

			CombineAssertions(() =>
			{
				AssertEquals("Set", 1m, previousDocument.CSI_Quantity);
				AssertEquals("Not Readonly", false, previousDocument.CSI_QuantityInfo.ReadOnly);

				previousDocument.UsualProcessingFlag = false;
				AssertEquals("Cleared", ZDecimal.Zero, previousDocument.CSI_Quantity);
				AssertEquals("Readonly", true, previousDocument.CSI_QuantityInfo.ReadOnly);
			});
		}

		public void TestCSI_QuantityDecimals()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			CombineAssertions(() =>
			{
				var invoiceLinePrevDoc = invoiceLine.PreviousDocuments.AddNew();
				AssertEquals("Export, invoice line prev doc", 5, invoiceLinePrevDoc.CSI_QuantityDecimals);

				var (_, cusClassPartPivotPrevDoc) = CreatePreviousDocumentWithPivotParent(Factory);
				AssertEquals("Export, classPartPivot prev doc", 5, cusClassPartPivotPrevDoc.CSI_QuantityDecimals);

				var invoiceLinePrevProc = invoiceLine.PreviousProcedures.AddNew();
				AssertEquals("Export, invoice line prev proc", 3, invoiceLinePrevProc.CSI_QuantityDecimals);

				var invoiceHeaderPrevDoc = invoiceHeader.PreviousDocuments.AddNew();
				AssertEquals("Export, invoice header prev doc", 3, invoiceHeaderPrevDoc.CSI_QuantityDecimals);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import, invoice line prev doc", 3, invoiceLinePrevDoc.CSI_QuantityDecimals);
				AssertEquals("Import, invoice line prev proc, not ATNEU", 3, invoiceLinePrevProc.CSI_QuantityDecimals);

				invoiceLinePrevProc.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				AssertEquals("Import, invoice line prev proc, ATNEU", 0, invoiceLinePrevProc.CSI_QuantityDecimals);

				AssertEquals("Import, invoice header prev doc", 3, invoiceHeaderPrevDoc.CSI_QuantityDecimals);
			});
		}

		public void TestCSI_Quantity_ReadOnly_ExportInvoiceLine()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceLine.PreviousDocuments.AddNew(), (x) => x.CSI_QuantityInfo, attributeName);
		}

		public void TestCSI_Quantity_ReadOnly_ExportCusClassPartPivot()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			var (_, previousDocument) = CreatePreviousDocumentWithPivotParent(Factory);
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(previousDocument, (x) => x.CSI_QuantityInfo, attributeName);
		}

		public void TestCSI_Quantity_Export_InvoiceHeader_Editable()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceHeader.PreviousDocuments.AddNew(), (x) => x.CSI_QuantityInfo, attributeName, false);
		}

		public void TestCSI_UnitOfQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "NAR", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.UsualProcessingFlag = true;
			previousDocument.CSI_UnitOfQuantity = "NAR";

			CombineAssertions(() =>
			{
				AssertEquals("Set", "NAR", previousDocument.CSI_UnitOfQuantity);
				AssertEquals("Not Readonly", false, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);

				previousDocument.UsualProcessingFlag = false;
				AssertEquals("Cleared", ZString.Empty, previousDocument.CSI_UnitOfQuantity);
				AssertEquals("Readonly", true, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);
			});
		}

		public void TestCSI_UnitOfQuantity_ReadOnly_ExportInvoiceLine()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceLine.PreviousDocuments.AddNew(), (x) => x.CSI_UnitOfQuantityInfo, attributeName);
		}

		public void TestCSI_UnitOfQuantity_ReadOnly_ExportCusClassPartPivot()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			var (_, previousDocument) = CreatePreviousDocumentWithPivotParent(Factory);
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(previousDocument, (x) => x.CSI_UnitOfQuantityInfo, attributeName);
		}

		public void TestCSI_UnitOfQuantity_Export_InvoiceHeader_Editable()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceHeader.PreviousDocuments.AddNew(), (x) => x.CSI_UnitOfQuantityInfo, attributeName, false);
		}

		public void TestUsualProcessingFlag()
		{
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._A;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			AssertEquals(false, previousDocument.UsualProcessingFlagInfo.ReadOnly);
		}

		public void TestCSI_LineNo()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			previousDocument.CSI_LineNo = 1;
			AssertEquals(false, previousDocument.CSI_LineNoInfo.ReadOnly);

			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			AssertEquals(true, previousDocument.CSI_LineNoInfo.ReadOnly);
			AssertEquals(ZShort.Zero, previousDocument.CSI_LineNo);

			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			previousDocument.CSI_LineNo = 1;
			AssertEquals(false, previousDocument.CSI_LineNoInfo.ReadOnly);

			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;
			AssertEquals(true, previousDocument.CSI_LineNoInfo.ReadOnly);
			AssertEquals(ZShort.Zero, previousDocument.CSI_LineNo);
		}

		public void TestCSI_Description_Import_PreviousDocument()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._A;
				var previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("CSI_Procedure 'A'", 26, previousDocument.CSI_DescriptionInfo.MaxLength);

				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("CSI_Procedure 'ATZL'", 100, previousDocument.CSI_DescriptionInfo.MaxLength);

				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("CSI_Procedure 'ATAV'", 300, previousDocument.CSI_DescriptionInfo.MaxLength);
			});
		}

		public void TestCSI_Description_Export_PreviousProcedure()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				var previousProcedure = invoiceLine.PreviousProcedures.AddNew();
				AssertEquals("CSI_Procedure 'ATZL'", 100, previousProcedure.CSI_DescriptionInfo.MaxLength);

				invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				previousProcedure = invoiceLine.PreviousProcedures.AddNew();
				AssertEquals("CSI_Procedure 'ATAV'", 300, previousProcedure.CSI_DescriptionInfo.MaxLength);
			});
		}

		public void TestCSI_Description_MaxLength_ExportInvoiceLine_PreviousDocument()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertEquals(35, previousDocument.CSI_DescriptionInfo.MaxLength);
		}

		public void TestCSI_Description_MaxLength_ExportCusClassPartPivot_PreviousDocument()
		{
			var (_, previousDocument) = CreatePreviousDocumentWithPivotParent(Factory);
			AssertEquals(35, previousDocument.CSI_DescriptionInfo.MaxLength);
		}

		public void TestCSI_Description_Readonly_ExportInvoiceLine_PreviousDocument()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceLine.PreviousDocuments.AddNew(), (x) => x.CSI_DescriptionInfo, attributeName);
		}

		public void TestCSI_Description_Readonly_ExportCusClassPartPivot_PreviousDocument()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			var (_, previousDocument) = CreatePreviousDocumentWithPivotParent(Factory);
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(previousDocument, (x) => x.CSI_DescriptionInfo, attributeName);
		}

		public void TestSimplifiedGrantAuthorizationFlag()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.CSI_SubType = ZString.Empty;

			//Testing Setter
			previousDocument.SimplifiedGrantAuthorizationFlag = true;
			AssertEquals(SimplifiedGrantAuthorizationList.Codes.J, previousDocument.CSI_SubType);

			previousDocument.SimplifiedGrantAuthorizationFlag = false;
			AssertEquals(SimplifiedGrantAuthorizationList.Codes.N, previousDocument.CSI_SubType);

			//Testing Getter
			previousDocument.CSI_SubType = SimplifiedGrantAuthorizationList.Codes.J;
			AssertEquals(true, previousDocument.SimplifiedGrantAuthorizationFlag);

			previousDocument.CSI_SubType = SimplifiedGrantAuthorizationList.Codes.N;
			AssertEquals(false, previousDocument.SimplifiedGrantAuthorizationFlag);
		}

		public void TestCSI_ReferenceNumber2()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			AssertEquals(17, previousDocument.CSI_ReferenceNumber2Info.MaxLength);

			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument = instruction.GetOnlyPreviousDocument();
			AssertEquals(35, previousDocument.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals(false, previousDocument.CSI_ReferenceNumber2Info.ReadOnly);

			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			AssertEquals(true, previousDocument.CSI_ReferenceNumber2Info.ReadOnly);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var previousDocumentLine = invoiceLine.PreviousDocuments.AddNew();
			AssertEquals(26, previousDocumentLine.CSI_ReferenceNumber2Info.MaxLength);
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();

			CombineAssertions(() =>
			{
				previousDocument.CSI_Status = YesNoList.Codes.Yes;
				AssertEquals("ATZL, Status Yes", 21, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

				previousDocument.CSI_Status = YesNoList.Codes.No;
				AssertEquals("ATZL, Status No", 35, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				previousDocument.CSI_Status = YesNoList.Codes.Yes;
				AssertEquals("ATAV, Status Yes", 21, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				AssertEquals("ATNEU, type 'AWB'", 44, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;
				AssertEquals("ATNEU, type 'ULD'", 44, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertEquals("ATNEU, type 'REG'", 21, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

				foreach (var procedure in new[] { PreviousProcedureList.Codes._ATA,
					PreviousProcedureList.Codes._ESUMA,
					PreviousProcedureList.Codes._VO,
					PreviousProcedureList.Codes._TIR,
					PreviousProcedureList.Codes._PUEB,
					PreviousProcedureList.Codes._GB })
				{
					previousDocument.CSI_Procedure = procedure;
					AssertEquals(procedure, 28, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
				}
			});
		}

		public void TestSetStatusToYesResetsLongReferenceNumber()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			previousDocument.CSI_Status = YesNoList.Codes.No;
			previousDocument.CSI_ReferenceNumber = "012345678901234567890123456789";

			previousDocument.CSI_Status = YesNoList.Codes.Yes;
			AssertEquals(ZString.Empty, previousDocument.CSI_ReferenceNumber);
		}

		public void TestUpdateAuthorizationNumberWithCusAuthorizationCWA()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT1");
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();
			AssertEquals(PreviousProcedureList.Codes._ATZL, previousDocument.CSI_Procedure);
			AssertEquals("DEFAULT1", previousDocument.AuthorizationNumber);
		}

		public void TestUpdateAuthorizationNumberWithOrgCusCodeIPAForATAV()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT1");
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			CombineAssertions(() =>
			{
				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				var previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("Default for ATAV", "DEFAULT1", previousDocument.AuthorizationNumber);
				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("No default for others", ZString.Empty, previousDocument.AuthorizationNumber);
			});
		}

		public void TestFormattedTariff()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();

			CombineAssertions(() =>
			{
				previousDocument.FormattedTariff = "12345678901";
				AssertEquals("Set unformatted: FormattedTariff", "1234.56.78 901", previousDocument.FormattedTariff);
				AssertEquals("Set unformatted: CSI_Tariff", "12345678901", previousDocument.CSI_Tariff);

				previousDocument.FormattedTariff = "1234.56.78 901";
				AssertEquals("Set formatted: FormattedTariff", "1234.56.78 901", previousDocument.FormattedTariff);
				AssertEquals("Set formatted: CSI_Tariff", "12345678901", previousDocument.CSI_Tariff);
			});
		}

		public void TestITariffFormatProvider()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.GetOnlyPreviousDocument();

			CombineAssertions(() =>
			{
				var provider = (ITariffFormatProvider)previousDocument;
				var tariffFormatter = provider.TariffFormatter;
				AssertType<TariffFormatterEleven>("Type", provider.TariffFormatter);
				AssertSame("Cached", tariffFormatter, provider.TariffFormatter);
			});
		}

		public void TestIsProcedureATZL()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				var previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("ATZL", true, previousDocument.IsProcedureATZL);

				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
				previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("ATA", false, previousDocument.IsProcedureATZL);
			});
		}

		public void TestIsProcedureATAV()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				var previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("ATAV", true, previousDocument.IsProcedureATAV);

				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
				previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("ATA", false, previousDocument.IsProcedureATAV);
			});
		}

		public void TestIsProcedureATNEU()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				var previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("ATNEU", true, previousDocument.IsProcedureATNEU);

				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
				previousDocument = instruction.GetOnlyPreviousDocument();
				AssertEquals("ATA", false, previousDocument.IsProcedureATNEU);
			});
		}

		public void TestCSI_TariffPersistsAfterSaving_ForImportATZL()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var previousDocument = instruction.PreviousDocuments.AddNew();
			previousDocument.CSI_Tariff = "1234567890";
			Factory.Save();
			AssertEquals("1234567890", previousDocument.CSI_Tariff);
		}

		public void TestCSI_StatusPersistsAfterSaving_ForImportATZL() => AssertCSI_StatusPersistsAfterSaving(PreviousProcedureList.Codes._ATZL);

		public void TestCSI_StatusPersistsAfterSaving_ForImportATAV() => AssertCSI_StatusPersistsAfterSaving(PreviousProcedureList.Codes._ATAV);

		public void TestSimplifiedGrantAuthorizationFlagPersistsAfterSaving_ForImportATAV()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			CombineAssertions(() =>
			{
				instruction.PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag = true;
				Factory.Save();
				AssertEquals("True", true, instruction.PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag);

				instruction.PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag = false;
				Factory.Save();
				AssertEquals("False", false, instruction.PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag);
			});
		}
		public void TestCSI_FullType_Attributes()
		{
			var previousDocument = invoiceHeader.PreviousDocuments.AddNew();

			AssertEquals("List", "Lookups.DocumentCodeList", previousDocument.CSI_CodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestCSI_ReferenceNumber_Import_InvoiceHeader_MaxLength()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
			AssertEquals(35, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
		}

		public void TestCSI_ReferenceNumber_Export_MaxLength_InTransitionPeriod()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var previousDocumentHeader = invoiceHeader.PreviousDocuments.AddNew();
			var previousDocumentLine = invoiceLine.PreviousDocuments.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals(35, previousDocumentHeader.CSI_ReferenceNumberInfo.MaxLength);
					AssertEquals(35, previousDocumentLine.CSI_ReferenceNumberInfo.MaxLength);
				});
			}
		}

		public void TestCSI_ReferenceNumber_Export_MaxLength_AfterTransitionPeriod()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var previousDocumentHeader = invoiceHeader.PreviousDocuments.AddNew();
			var previousDocumentLine = invoiceLine.PreviousDocuments.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				CombineAssertions(() =>
				{
					AssertEquals(70, previousDocumentHeader.CSI_ReferenceNumberInfo.MaxLength);
					AssertEquals(70, previousDocumentLine.CSI_ReferenceNumberInfo.MaxLength);
				});
			}
		}

		public void TestCSI_ReferenceNumber_Export_InvoiceHeader_ReadOnly()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceHeader.PreviousDocuments.AddNew(), (x) => x.CSI_ReferenceNumberInfo, attributeName);
		}

		public void TestCSI_ReferenceNumber_Export_InvoiceLine_ReadOnly()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceLine.PreviousDocuments.AddNew(), (x) => x.CSI_ReferenceNumberInfo, attributeName);
		}

		public void TestCSI_ReferenceNumber_Export_PreviousProcedure_Editable()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			var previousProcedure = invoiceLine.PreviousProcedures.AddNew();

			CombineAssertions(() =>
			{
				previousProcedure.CSI_Procedure = string.Empty;
				AssertEquals("Procedure empty", true, previousProcedure.CSI_ReferenceNumberInfo.ReadOnly);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				AssertEquals("Procedure not empty", false, previousProcedure.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber_Import_Editable() => AssertRefCusCodeImportPropertyEditable(x => x.CSI_ReferenceNumberInfo);

		public void TestCSI_ReferenceNumber_UpperCase()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var previousProcedure = invoiceLine.PreviousProcedures.AddNew();
			previousProcedure.CSI_ReferenceNumber = "abc123456789012345678";
			previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousProcedure.CSI_SubType = "REG";
			AssertEquals("Not match,CSI_ReferenceNumber not change", "abc123456789012345678", previousProcedure.CSI_ReferenceNumber);

			previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			Factory.Save();
			AssertEquals("Match,CSI_ReferenceNumber changed", "ABC123456789012345678", previousProcedure.CSI_ReferenceNumber);
		}

		public void TestCSI_ItemNumber_Caption()
		{
			var doc = invoiceLine.PreviousDocuments.AddNew();
			AssertEquals("Item No.", DataBoundResourceStrings.GetDataForProperty(doc.CSI_ItemNumberInfo).Caption);
		}

		public void TestCSI_ItemNumber_Export_InvoiceLine_ReadOnly()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceLine.PreviousDocuments.AddNew(), (x) => x.CSI_ItemNumberInfo, attributeName);
		}

		public void TestCSI_ItemNumber_Export_InvoiceHeader_ReadOnly()
		{
			var attributeName = UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber;
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, attributeName, ZString.Empty, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(invoiceHeader.PreviousDocuments.AddNew(), (x) => x.CSI_ItemNumberInfo, attributeName);
		}

		public void TestCSI_ItemNumber_Import_Editable() => AssertRefCusCodeImportPropertyEditable(x => x.CSI_ItemNumberInfo);

		public void TestCusEntryInstructionParentStyle()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			var instructionPrevDoc = instruction.PreviousDocuments.AddNew();
			var invoiceLinePrevDoc = invoiceLine.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Parent is CusEntryInstruction", ImportDeclarationTypeList.Codes.LUZ, instructionPrevDoc.CusEntryInstructionParentStyle);
				AssertEquals("ParentIsInvoiceLine", ZString.Empty, invoiceLinePrevDoc.CusEntryInstructionParentStyle);
			});
		}

		public void TestCSI_ItemNumberString_Getter()
		{
			var prevDoc = instruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_ItemNumber not set", "0", prevDoc.CSI_ItemNumberString);

				prevDoc.CSI_ItemNumber = 21;
				AssertEquals("CSI_ItemNumber = 21", "21", prevDoc.CSI_ItemNumberString);
			});
		}

		public void TestCSI_ItemNumberString_Setter()
		{
			var prevDoc = instruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				prevDoc.CSI_ItemNumberString = "21";
				AssertEquals("CSI_ItemNumberString set to '21'", (ZShort)21, prevDoc.CSI_ItemNumber);

				prevDoc.CSI_ItemNumberString = "abc";
				AssertEquals("CSI_ItemNumberString set to 'abc'", ZShort.Zero, prevDoc.CSI_ItemNumber);
			});
		}

		public void TestHumanReadableName_PreviousDocument()
		{
			AssertEquals("Previous Document", Factory.New<PreviousDocument>().HumanReadableName);
		}

		public void TestHumanReadableName_PreviousProcedure()
		{
			var prevProcedure = Factory.New<PreviousDocument>();
			prevProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			AssertEquals("Previous Procedure", prevProcedure.HumanReadableName);
		}

		public void TestCSI_SubType_AWB_ATNEU()
		{
			TestHelper.CreateCL010CoutryList(Factory);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var depotOrganisation = Factory.GetOrgHeaderWithEori("DEPOT", "111122223333", Core.Constants.CountryCodes.Germany);
			var depotAddress = depotOrganisation.Addresses.AddNew();
			depotAddress.OA_Address1 = "Address 1";
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;

			var prevDoc = instruction.PreviousDocuments.AddNew();
			prevDoc.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;

			CombineAssertions(() =>
			{
				prevDoc.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertEquals("CSI_ReferenceNumber2 not changed", ZString.Empty, prevDoc.CSI_ReferenceNumber2);

				prevDoc.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				AssertEquals("CSI_ReferenceNumber2 filled changed as SubType 'AWB'", "DE111122223333", prevDoc.CSI_ReferenceNumber2);
			});
		}

		public void TestCSI_SubType_AWB_OtherProcedure()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var depotOrganisation = Factory.GetOrgHeaderWithEori("DEPOT", "111122223333", Core.Constants.CountryCodes.Germany);
			var depotAddress = depotOrganisation.Addresses.AddNew();
			depotAddress.OA_Address1 = "Address 1";
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;

			var prevDoc = instruction.PreviousDocuments.AddNew();
			prevDoc.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			CombineAssertions(() =>
			{
				prevDoc.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertEquals("CSI_ReferenceNumber2 not changed", ZString.Empty, prevDoc.CSI_ReferenceNumber2);

				prevDoc.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				AssertEquals("CSI_ReferenceNumber2 not changed as procedure is not 'ATNEU'", ZString.Empty, prevDoc.CSI_ReferenceNumber2);
			});
		}

		public void TestCSI_ReferenceNumber2_AWB_ATNEU_CTO()
		{
			TestHelper.CreateCL010CoutryList(Factory);

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var ctoOrganisation = Factory.GetOrgHeaderWithEori("CTO", "555566667777", Core.Constants.CountryCodes.Germany);
			var ctoAddress = ctoOrganisation.Addresses.AddNew();
			ctoAddress.OA_Address1 = "Address 2";
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctoAddress.PK;

			var prevDoc = instruction.PreviousDocuments.AddNew();
			prevDoc.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			prevDoc.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			AssertEquals("CSI_ReferenceNumber2 from cto organisation, as depot empty", "DE555566667777", prevDoc.CSI_ReferenceNumber2);
		}

		public void TestCSI_ReferenceNumber2_AWB_ATNEU_DepotAndCtoEmpty()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var prevDoc = instruction.PreviousDocuments.AddNew();
			prevDoc.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			prevDoc.CSI_ReferenceNumber2 = "XYZ";
			prevDoc.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			AssertEquals("CSI_ReferenceNumber2 not updated when both depot and cto are empty", "XYZ", prevDoc.CSI_ReferenceNumber2);
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			yield return instruction.PreviousDocuments.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			yield return invoiceLine.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
		CusEntryInstruction instruction;

		void AssertCSI_StatusPersistsAfterSaving(ZString procedure)
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction.PreviousDocumentMaster.CSI_Procedure = procedure;
			var previousDocument = instruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				previousDocument.CSI_Status = YesNoList.Codes.Yes;
				Factory.Save();
				AssertEquals("Yes", "Y", previousDocument.CSI_Status);

				previousDocument.CSI_Status = YesNoList.Codes.No;
				Factory.Save();
				AssertEquals("No", "N", previousDocument.CSI_Status);
			});
		}

		void AssertRefCusCodePropertyReadOnlyIfAttributeIsMissing(PreviousDocument previousDocument, Func<PreviousDocument, ZPropertyInfo> getPropertyInfo, string attributeName, bool expectedReadOnlyWithoutAttribute = true)
		{
			var propertyInfo = getPropertyInfo(previousDocument);
			var propertyName = propertyInfo.Name;

			CombineAssertions(() =>
			{
				AssertEquals($"{propertyName} empty", expectedReadOnlyWithoutAttribute, propertyInfo.ReadOnly);

				previousDocument.CSI_Code = "DE01";
				AssertEquals($"No attribute '{attributeName}'", expectedReadOnlyWithoutAttribute, propertyInfo.ReadOnly);

				previousDocument.CSI_Code = "DE02";
				AssertEquals($"Has attribute '{attributeName}'", false, propertyInfo.ReadOnly);
			});
		}

		void AssertRefCusCodeImportPropertyEditable(Func<PreviousDocument, ZPropertyInfo> getPropertyInfo)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "DC40I");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "IMP1", "IMP1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var supportingDocumentForHeader = invoiceHeader.PreviousDocuments.AddNew();
			var supportingDocumentForLine = invoiceLine.PreviousDocuments.AddNew();
			supportingDocumentForLine.CSI_Code = "IMP1";
			supportingDocumentForHeader.CSI_Code = "IMP1";
			var infoLine = getPropertyInfo(supportingDocumentForLine);
			var infoHeader = getPropertyInfo(supportingDocumentForHeader);
			var propertyName = infoLine.Name;

			CombineAssertions(() =>
			{
				AssertEquals($"{propertyName} editable - SupportingDocumentForLine", false, infoLine.ReadOnly);
				AssertEquals($"{propertyName} editable - SupportingDocumentForHeader", false, infoHeader.ReadOnly);
			});
		}

		internal static (CusClassPartPivot, PreviousDocument) CreatePreviousDocumentWithPivotParent(BusinessObjectFactory factory)
		{
			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTPART";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.EXP;

			var previousDocument = pivot.PreviousDocuments.AddNew();
			return (pivot, previousDocument);
		}
	}
}
