using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
	{
		public void TestCSI_Quantity_Export_InvoiceHeader()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(6, supportingDocumentForHeader.QuantityDecimalPlaces);
		}

		public void TestCSI_Quantity_Export_InvoiceLine()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(4, supportingDocumentForLine.QuantityDecimalPlaces);
		}

		public void TestCSI_Quantity_Import_InvoiceHeader()
		{
			AssertEquals(6, supportingDocumentForHeader.QuantityDecimalPlaces);
		}

		public void TestCSI_Quantity_Import_InvoiceLine()
		{
			AssertEquals(3, supportingDocumentForLine.QuantityDecimalPlaces);
		}

		public void TestCSI_Quantity_Export_InvoiceLineReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.ComplementaryUnit, supportingDocumentForLine, x => x.CSI_QuantityInfo);
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.MeasurementUnit, supportingDocumentForLine, x => x.CSI_QuantityInfo);
		}

		public void TestCSI_ReferenceNumber2_Export_InvoiceHeader()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_ReferenceNumber2 = new ZString('A', 13);
				supportingDocumentForHeader.CSI_FullType = ExportLicenseGroupList.Codes._3LLDNB;
				AssertEquals("Is editable for export invoice header", false, supportingDocumentForHeader.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("Is capped at 12 characters", new ZString('A', 12), supportingDocumentForHeader.CSI_ReferenceNumber2);
			});
		}

		public void TestCSI_ReferenceNumber2_Export_InvoiceLine()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_ReferenceNumber2 = "test1";
				supportingDocumentForLine.CSI_FullType = ExportLicenseGroupList.Codes._3LLDNB;
				AssertEquals("Is read-only for export invoice line when CSI_FullType = '3LLDNB'", true, supportingDocumentForLine.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("For export invoice line, setting CSI_FullType to '3LLDNB' updates CSI_ReferenceNumber2 to 'NULL'", "NULL", supportingDocumentForLine.CSI_ReferenceNumber2);

				supportingDocumentForLine.CSI_ReferenceNumber2 = new ZString('A', 13);
				supportingDocumentForLine.CSI_FullType = ExportLicenseGroupList.Codes.C052AF;
				AssertEquals("Is read-only for export invoice line when CSI_FullType <> '3LLDNB'", true, supportingDocumentForLine.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("Setting CSI_FullType to anything other than '3LLDNB' doesn't update CSI_ReferenceNumber2", new ZString('A', 12), supportingDocumentForLine.CSI_ReferenceNumber2);
			});
		}

		public void TestCSI_ReferenceNumber2_Export_InvoiceLineReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Detail, supportingDocumentForLine, x => x.CSI_ReferenceNumber2Info);
		}

		public void TestCSI_ReferenceNumber2_Import_InvoiceLine()
		{
			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_ReferenceNumber2 = new ZString('A', 13);
				supportingDocumentForLine.CSI_FullType = ExportLicenseGroupList.Codes._3LLDNB;
				AssertEquals("Is editable for import invoice line", false, supportingDocumentForLine.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("For import invoice line, setting CSI_FullType to '3LLDNB' doesn't update CSI_ReferenceNumber2", new ZString('A', 12), supportingDocumentForLine.CSI_ReferenceNumber2);
			});
		}

		public void TestCSI_ReferenceNumber2_Import_InvoiceHeader()
		{
			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_ReferenceNumber2 = new ZString('A', 13);
				supportingDocumentForHeader.CSI_FullType = ExportLicenseGroupList.Codes._3LLDNB;
				AssertEquals("Is editable for import invoice header", false, supportingDocumentForHeader.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("Is capped at 12 characters", new ZString('A', 12), supportingDocumentForHeader.CSI_ReferenceNumber2);
			});
		}

		public void TestDivisionValue()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_Code = "9004";
				AssertEquals("Division is the Division attribute value of RefCusCodeList", RefCusCodeListAttributes.Value.MiscellaneousDocument, supportingDocumentForHeader.Division);

				supportingDocumentForLine.CSI_Code = "9005";
				AssertEquals("Division is updated when CSI_Code changes", RefCusCodeListAttributes.Value.ExemptionsExplanations, supportingDocumentForLine.Division);
			});
		}

		public void TestParentIsHeaderOrParentIsLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals("supportingDocumentForHeader.ParentIsInvoiceHeader", true, supportingDocumentForHeader.ParentIsInvoiceHeader);
				AssertEquals("supportingDocumentForHeader.ParentIsInvoiceLine", false, supportingDocumentForHeader.ParentIsInvoiceLine);
				AssertEquals("supportingDocumentForLine.ParentIsInvoiceLine", true, supportingDocumentForLine.ParentIsInvoiceLine);
				AssertEquals("supportingDocumentForLine.ParentIsInvoiceHeader", false, supportingDocumentForLine.ParentIsInvoiceHeader);
			});
		}

		public void TestRefCusCode_Import()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_Code = "Test";
				AssertNull("RefCusCodeList DC44I with code 'Test' doesn't exist", supportingDocumentForLine.RefCusCode);
				supportingDocumentForLine.CSI_Code = "9004";
				AssertEquals("RefCusCodeList DC44I with code '9004' exists", "9004", supportingDocumentForLine.RefCusCode.ZZD_Code);
			});
		}

		public void TestRefCusCode_Export()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_FullType = "9005";
				AssertNull("RefCusCodeList DC44E with code '9005' doesn't exist", supportingDocumentForLine.RefCusCode);
				supportingDocumentForLine.CSI_FullType = "3LLA231";
				AssertEquals("RefCusCodeList DC44E with code '3LLA231' exists", "3LLA231", supportingDocumentForLine.RefCusCode.ZZD_Code);
			});
		}

		public void TestCSI_ReferenceNumber_Import_InvoiceLine_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code Empty", false, supportingDocumentForLine.CSI_ReferenceNumberInfo.ReadOnly);

				supportingDocumentForLine.CSI_Code = "9005";
				AssertEquals("CSI_Code not in ('C990', 'D019', 'N990')", false, supportingDocumentForLine.CSI_ReferenceNumberInfo.ReadOnly);

				foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
				{
					supportingDocumentForLine.CSI_Code = type;
					AssertEquals($"CSI_Code {type}", true, supportingDocumentForLine.CSI_ReferenceNumberInfo.ReadOnly);
				}
			});
		}

		public void TestCSI_ReferenceNumber_Import_InvoiceHeader_ReadOnly()
		{
			supportingDocumentForHeader.CSI_Code = SupportingDocumentTypes.N990;
			AssertEquals("CSI_Code is editable for import invoice header", false, supportingDocumentForHeader.CSI_ReferenceNumberInfo.ReadOnly);
		}

		public void TestCSI_ReferenceNumber_Export_InvoiceHeader_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Reference, supportingDocumentForHeader, x => x.CSI_ReferenceNumberInfo);
		}

		public void TestCSI_ReferenceNumber_Export_InvoiceLine_Editable()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Reference, supportingDocumentForLine, x => x.CSI_ReferenceNumberInfo, false);
		}

		public void TestCSI_ReferenceNumber_Import_InvoiceHeader_EusAuthorisation()
		{
			var orgHeader = dec.DeclarantAddress.Header;
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "DEEUS123");

			supportingDocumentForHeader.CSI_ReferenceNumber = "Test";
			supportingDocumentForHeader.CSI_Code = SupportingDocumentTypes.C990;
			AssertEquals("CSI_ReferenceNumber of import invoice header is as specified by user", "Test", supportingDocumentForHeader.CSI_ReferenceNumber);
		}

		public void TestCSI_ReferenceNumber_Import_InvoiceLine_EusAuthorisation()
		{
			var orgHeader = dec.DeclarantAddress.Header;
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "DEACE111");

			CombineAssertions(() =>
			{
				foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
				{
					supportingDocumentForLine.CSI_Code = type;
					AssertEquals($"CSI_Code {type}, No EUS Authorisation", ZString.Empty, supportingDocumentForLine.CSI_ReferenceNumber);
				}

				orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "DEEUS123");

				foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
				{
					supportingDocumentForLine.CSI_Code = type;
					AssertEquals($"CSI_Code {type}, Has EUS Authorisation", "DEEUS123", supportingDocumentForLine.CSI_ReferenceNumber);
				}

				supportingDocumentForLine.CSI_Code = "9004";
				AssertEquals("CSI_Code not in ('C990', 'D019', 'N990')", ZString.Empty, supportingDocumentForLine.CSI_ReferenceNumber);
			});
		}

		public void TestCSI_ReferenceNumber_Export_InvoiceLine_EusAuthorisation()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			var orgHeader = dec.DeclarantAddress.Header;
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "DEEUS123");

			supportingDocumentForLine.CSI_Code = SupportingDocumentTypes.N990;
			AssertEquals("CSI_Code N990", ZString.Empty, supportingDocumentForLine.CSI_ReferenceNumber);
		}

		public void TestCSI_ReferenceNumber_Export_MaxLength_InTransitionPeriod()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			supportingDocumentForLine.CSI_Code = EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals(35, supportingDocumentForLine.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_ReferenceNumber_Export_MaxLength_AfterTransitionPeriod()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			supportingDocumentForLine.CSI_Code = EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals(70, supportingDocumentForLine.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_ReferenceNumber_MaxLength_NoParentDeclaration_ExportOutsideTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var supDocOnExportPivot = CreateSupportingDocumentWithPivotParent(true);

				AssertEquals(70, supDocOnExportPivot.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_ReferenceNumber_MaxLength_NoParentDeclaration_ExportInsideTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var supDocOnExportPivot = CreateSupportingDocumentWithPivotParent(true);

				AssertEquals(35, supDocOnExportPivot.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_ReferenceNumber_MaxLength_NoParentDeclaration_ImportOutsideTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var supDocOnImportPivot = CreateSupportingDocumentWithPivotParent(false);

				AssertEquals(35, supDocOnImportPivot.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_ReferenceNumber_MaxLength_NoParentDeclaration_ImportInsideTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var supDocOnImportPivot = CreateSupportingDocumentWithPivotParent(false);

				AssertEquals(35, supDocOnImportPivot.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_FullType_Export_InvoiceHeader()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_FullType = "2AAF";
				AssertEquals("2AAF", supportingDocumentForHeader.CSI_Code);
				AssertEquals("", supportingDocumentForHeader.CSI_SubType);

				supportingDocumentForHeader.CSI_FullType = "3LLA10";
				AssertEquals("3LLA", supportingDocumentForHeader.CSI_Code);
				AssertEquals("10", supportingDocumentForHeader.CSI_SubType);

				supportingDocumentForHeader.CSI_FullType = "3LLA231";
				AssertEquals("3LLA", supportingDocumentForHeader.CSI_Code);
				AssertEquals("231", supportingDocumentForHeader.CSI_SubType);
			});
		}

		public void TestKeyToDetermineUniqueness_Export_InvoiceHeader()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			supportingDocumentForHeader.CSI_Code = "3LLA";
			supportingDocumentForHeader.CSI_SubType = "231";
			supportingDocumentForHeader.CSI_ReferenceNumber = "FOO";
			AssertEquals("3LLA231FOO", supportingDocumentForHeader.KeyToDeterimeUniqueness);
		}

		public void TestKeyToDetermineUniqueness_Export_InvoiceLine()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			supportingDocumentForLine.CSI_Code = "3LLA";
			supportingDocumentForLine.CSI_SubType = "231";
			supportingDocumentForLine.CSI_ReferenceNumber = "FOO";
			AssertEquals("3LLA231FOO", supportingDocumentForLine.KeyToDeterimeUniqueness);
		}

		public void TestKeyToDetermineUniqueness_Import_InvoiceHeader()
		{
			supportingDocumentForHeader.CSI_Code = "3LLA";
			supportingDocumentForHeader.CSI_SubType = "231";
			supportingDocumentForHeader.CSI_ReferenceNumber = "FOO";
			AssertEquals("3LLAFOO", supportingDocumentForHeader.KeyToDeterimeUniqueness);
		}

		public void TestKeyToDetermineUniqueness_Import_InvoiceLine()
		{
			supportingDocumentForLine.CSI_Code = "3LLA";
			supportingDocumentForLine.CSI_SubType = "231";
			supportingDocumentForLine.CSI_ReferenceNumber = "FOO";
			AssertEquals("3LLAFOO", supportingDocumentForLine.KeyToDeterimeUniqueness);
		}

		public void TestCSI_UnitOfQuantity_Export_InvoiceHeader_Get()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_UnitOfQuantity = "KG";
				AssertEquals("SupportsUnitOfQuantity is true on export invoice.", true, supportingDocumentForHeader.SupportsUnitOfQuantity);
				AssertEquals("When the supporting document supports unit of quantity, we return the actual value.", "KG", supportingDocumentForHeader.CSI_UnitOfQuantity);
			});
		}

		public void TestCSI_UnitOfQuantity_Import_InvoiceHeader_Get()
		{
			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_UnitOfQuantity = "KG";
				AssertEquals("SupportsUnitOfQuantity is false on import invoice.", false, supportingDocumentForHeader.SupportsUnitOfQuantity);
				AssertEquals("When the supporting document doesn't support unit of quantity, we return empty string.", "", supportingDocumentForHeader.CSI_UnitOfQuantity);
			});
		}

		public void TestCSI_UnitOfQuantity_Export_InvoiceHeader_Set()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTPART";
			var pivot = product.PivotsForBinding.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_OP = product.PK;
			AssertEquals(pivot, invoiceLine.Pivot);

			var supDocOnExportDeclaration = dec.SupportingDocuments.AddNew();
			var supDocOnExportPivot = pivot.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is supported on export declaration.", supDocOnExportDeclaration, true);
				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is NOT supported on export invoice.", supportingDocumentForHeader, true);
				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is supported on export invoice line.", supportingDocumentForLine, true);
				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is supported on export pivot.", supDocOnExportPivot, true);
			});
		}

		public void TestCSI_UnitOfQuantity_Import_InvoiceHeader_Set()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTPART";
			var pivot = product.PivotsForBinding.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_OP = product.PK;
			CombineAssertions(() =>
			{
				AssertEquals(pivot, invoiceLine.Pivot);

				var supDocOnImportDeclaration = dec.SupportingDocuments.AddNew();
				var supDocOnImportPivot = pivot.SupportingDocuments.AddNew();

				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is supported on import declaration.", supDocOnImportDeclaration, true);
				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is NOT supported on import invoice.", supportingDocumentForHeader, false);
				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is supported on import invoice line.", supportingDocumentForLine, true);
				AssertSupportsUnitOfQuantity("CSI_UnitOfQuantity is supported on import pivot.", supDocOnImportPivot, true);
			});
		}

		public void TestCSI_UnitOfQuantity_Export_InvoiceHeader_Editable()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.ComplementaryUnit, supportingDocumentForHeader, x => x.CSI_UnitOfQuantityInfo, false);
		}

		public void TestCSI_UnitOfQuantity_Export_InvoiceLine_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.ComplementaryUnit, supportingDocumentForLine, x => x.CSI_UnitOfQuantityInfo);
		}

		public void TestCSI_Description_Export_InvoiceLine_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Complement, supportingDocumentForLine, x => x.CSI_DescriptionInfo);
		}

		public void TestCSI_Description_Export_InvoiceHeader_Editable()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Complement, supportingDocumentForHeader, x => x.CSI_DescriptionInfo, false);
		}

		public void TestCSI_Value_Export_InvoiceHeader_Editable()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Value, supportingDocumentForHeader, x => x.CSI_ValueInfo, false);
		}

		public void TestCSI_Value_Export_InvoiceLine_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Value, supportingDocumentForLine, x => x.CSI_ValueInfo);
		}

		public void TestFormattedType()
		{
			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_Code = "3LLA";
				supportingDocumentForHeader.CSI_SubType = "231";
				AssertEquals("CSI_FullType should be made up of CSI_Code and CSI_SubType.", "3LLA231", supportingDocumentForHeader.CSI_FullType);
				AssertEquals("FormattedType should return CSI_FullType", supportingDocumentForHeader.CSI_FullType, supportingDocumentForHeader.FormattedType);
			});
		}

		public void TestUnitOfQuantityFieldType()
		{
			AssertEquals(nameof(FieldType.TextDropEdit), Factory.New<SupportingDocument>().UnitOfQuantityFieldType);
		}

		public void TestCSI_UnitOfQuantity2_Export_InvoiceHeader_Editable()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.MeasurementUnit, supportingDocumentForHeader, x => x.CSI_UnitOfQuantity2Info, false);
		}

		public void TestCSI_UnitOfQuantity2_Export_InvoiceLine_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.MeasurementUnit, supportingDocumentForLine, x => x.CSI_UnitOfQuantity2Info);
		}

		public void TestCSI_UnitOfQuantity2_Import_Editable()
		{
			AssertRefCusCodeImportPropertyEditable(x => x.CSI_UnitOfQuantity2Info);
		}

		public void TestCSI_UnitOfQuantity2_Caption()
		{
			AssertEquals("EU-Unit of Measure", DataBoundResourceStrings.GetDataForProperty(supportingDocumentForLine.CSI_UnitOfQuantity2Info).Caption);
		}

		public void TestCSI_RX_NKCurrency_Export_InvoiceLine_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Value, supportingDocumentForLine, x => x.CSI_RX_NKCurrencyInfo);
		}

		public void TestCSI_RX_NKCurrency_Export_InvoiceHeader_Editable()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Value, supportingDocumentForHeader, x => x.CSI_RX_NKCurrencyInfo, false);
		}

		public void TestCSI_RX_NKCurrency_Import_Editable()
		{
			AssertRefCusCodeImportPropertyEditable(x => x.CSI_RX_NKCurrencyInfo);
		}

		public void TestCSI_AdditionalDescription_Export_InvoiceLine_MaxLength()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", 70, supportingDocumentForLine.CSI_AdditionalDescriptionInfo.MaxLength);
		}

		public void TestCSI_AdditionalDescription_Import_InvoiceLine_MaxLength()
		{
			AssertEquals("Import", 300, supportingDocumentForLine.CSI_AdditionalDescriptionInfo.MaxLength);
		}

		public void TestCSI_AdditionalDescription_Caption()
		{
			AssertEquals("Issuing Authority", DataBoundResourceStrings.GetDataForProperty(supportingDocumentForLine.CSI_AdditionalDescriptionInfo).Caption);
		}

		public void TestCSI_AdditionalDescription_Export_InvoiceHeader_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Authority, supportingDocumentForHeader, x => x.CSI_AdditionalDescriptionInfo);
		}

		public void TestCSI_AdditionalDescription_Export_InvoiceLine_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.Authority, supportingDocumentForLine, x => x.CSI_AdditionalDescriptionInfo);
		}

		public void TestCSI_AdditionalDescription_Import_Editable()
		{
			AssertRefCusCodeImportPropertyEditable(x => x.CSI_AdditionalDescriptionInfo);
		}

		public void TestCSI_ItemNumber_Export_InvoiceHeader_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.ItemNumber, supportingDocumentForHeader, x => x.CSI_ItemNumberInfo);
		}

		public void TestCSI_ItemNumber_Export_InvoiceLine_ReadOnly()
		{
			AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.ItemNumber, supportingDocumentForLine, x => x.CSI_ItemNumberInfo);
		}

		public void TestCSI_ItemNumber_Import_Editable()
		{
			AssertRefCusCodeImportPropertyEditable(x => x.CSI_ItemNumberInfo);
		}

		public void TestCSI_ItemNumber_Caption()
		{
			AssertEquals("Doc. Line Item Number", DataBoundResourceStrings.GetDataForProperty(supportingDocumentForLine.CSI_ItemNumberInfo).Caption);
		}

		public void TestCSI_FullType_ClearReadOnlyProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "EXP1", "No attributes", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_FullType = "3LLA231";
			supportingDocumentForLine.CSI_UnitOfQuantity2 = "UQ2";
			supportingDocumentForLine.CSI_RX_NKCurrency = "CUR";
			supportingDocumentForLine.CSI_AdditionalDescription = "ADD";
			supportingDocumentForLine.CSI_ItemNumber = 15;

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_FullType = "EXP1";
				AssertEquals("CSI_UnitOfQuantity readonly", true, supportingDocumentForLine.CSI_UnitOfQuantityInfo.ReadOnly);
				AssertEquals("CSI_UnitOfQuantity2 readonly", true, supportingDocumentForLine.CSI_UnitOfQuantity2Info.ReadOnly);
				AssertEquals("CSI_DateOfIssue readonly", true, supportingDocumentForLine.CSI_DateOfIssueInfo.ReadOnly);
				AssertEquals("CSI_DateOfExpiry readonly", true, supportingDocumentForLine.CSI_DateOfExpiryInfo.ReadOnly);
				AssertEquals("CSI_ReferenceNumber2 readonly", true, supportingDocumentForLine.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("CSI_RX_NKCurrency readonly", true, supportingDocumentForLine.CSI_RX_NKCurrencyInfo.ReadOnly);
				AssertEquals("CSI_AdditionalDescription readonly", true, supportingDocumentForLine.CSI_AdditionalDescriptionInfo.ReadOnly);
				AssertEquals("CSI_ItemNumber readonly", true, supportingDocumentForLine.CSI_ItemNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_DateOfExpiry_Export_ReadOnly()
		{
			foreach (var documentType in new[] { supportingDocumentForHeader, supportingDocumentForLine })
			{
				AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.ValidityDate, documentType, x => x.CSI_DateOfExpiryInfo);
			}
		}

		public void TestCSI_DateOfExpiry_Import_Editable()
		{
			AssertRefCusCodeImportPropertyEditable(x => x.CSI_DateOfExpiryInfo);
		}

		public void TestCSI_DateOfIssue_Export_ReadOnly()
		{
			foreach (var documentType in new[] { supportingDocumentForHeader, supportingDocumentForLine })
			{
				AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(RefCusCodeListAttributes.Name.IssuingDate, documentType, x => x.CSI_DateOfIssueInfo);
			}
		}

		public void TestCSI_DateOfIssue_Import_Editable()
		{
			AssertRefCusCodeImportPropertyEditable(x => x.CSI_DateOfIssueInfo);
		}

		public void TestCSI_LineNo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Parent is InvoiceHeader", (ZInt)1, supportingDocumentForHeader.CSI_LineNo);
				AssertEquals("Parent is InvoiceLine", (ZInt)1, supportingDocumentForLine.CSI_LineNo);

				var doc = dec.SupportingDocuments.AddNew();
				AssertEquals("CSI_LineNo not set when parent is JobDeclaration", ZInt.Zero, doc.CSI_LineNo);
			});
		}

		public void TestReadonlyResetWhenCSI_CodeChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(
				helper,
				RefCusCodeListAttributes.Name.MeasurementUnit,
				"EXP2",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "EXP1", "No attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_Code = "EXP1";

			CombineAssertions(() =>
			{
				AssertEquals(true, supportingDocumentForLine.CSI_QuantityInfo.ReadOnly);

				supportingDocumentForLine.CSI_Code = "EXP2";
				AssertEquals(false, supportingDocumentForLine.CSI_QuantityInfo.ReadOnly);
			});
		}

		public void TestReadonlyResetWhenCSI_SubTypeChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(
				helper,
				RefCusCodeListAttributes.Name.MeasurementUnit,
				"EXP12",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "EXP11", "No attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_Code = "EXP1";
			supportingDocumentForLine.CSI_SubType = "1";

			CombineAssertions(() =>
			{
				AssertEquals(true, supportingDocumentForLine.CSI_QuantityInfo.ReadOnly);

				supportingDocumentForLine.CSI_SubType = "2";
				AssertEquals(false, supportingDocumentForLine.CSI_QuantityInfo.ReadOnly);
			});
		}

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.SupportingDocuments.AddNew();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.SupportingDocuments.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.SupportingDocuments.AddNew();
			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = BaseCusClassification.ClassificationType.Both;
			yield return pivot.SupportingDocuments.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			dec = Factory.New<JobDeclaration>();
			invoiceHeader = dec.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			supportingDocumentForHeader = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocumentForLine = invoiceLine.SupportingDocuments.AddNew();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		}

		JobDeclaration dec;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		SupportingDocument supportingDocumentForHeader;
		SupportingDocument supportingDocumentForLine;

		void AssertRefCusCodeImportPropertyEditable(Func<SupportingDocument, ZPropertyInfo> getPropertyInfo)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DC44I");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "IMP1", "IMP1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
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

		void AssertRefCusCodeExportPropertyReadOnlyIfAttributeIsMissing(string attributeName, SupportingDocument supportingDocument, Func<SupportingDocument, ZPropertyInfo> getPropertyInfo, bool expectedReadOnlyWithoutAttribute = true)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(
				helper,
				attributeName,
				"EXP2",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "EXP1", "No attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocument.CSI_FullType = "EXP1";
			var propertyInfo = getPropertyInfo(supportingDocument);
			var propertyName = propertyInfo.Name;

			CombineAssertions(() =>
			{
				var expectedReadOnlyString = expectedReadOnlyWithoutAttribute ? "readonly" : "editable";
				AssertEquals($"RefCusCode doesn't have attribute '{attributeName}' -> {propertyName} {expectedReadOnlyString}", expectedReadOnlyWithoutAttribute, propertyInfo.ReadOnly);

				supportingDocument.CSI_FullType = "EXP2";
				AssertEquals($"RefCusCode has attribute '{attributeName}' -> {propertyName} editable", false, propertyInfo.ReadOnly);

				supportingDocument.CSI_FullType = ZString.Empty;
				AssertEquals($"RefCusCode null -> {propertyName} {expectedReadOnlyString}", expectedReadOnlyWithoutAttribute, propertyInfo.ReadOnly);
			});
		}

		void AssertSupportsUnitOfQuantity(string message, SupportingDocument supDoc, bool supportsUnitOfQuantity)
		{
			supDoc.CSI_UnitOfQuantity = "";
			supDoc.CSI_UnitOfQuantity = "KG";
			AssertEquals($"{message} -> SupportsUnitOfQuantity", supportsUnitOfQuantity, supDoc.SupportsUnitOfQuantity);
			AssertEquals($"{message} -> CSI_UnitOfQuantity", supportsUnitOfQuantity ? (ZString)"KG" : ZString.Empty, supDoc.CSI_UnitOfQuantity);
		}

		SupportingDocument CreateSupportingDocumentWithPivotParent(bool isExport)
		{
			dec.JE_MessageType = isExport ? MessageTypeList.Codes.Export : MessageTypeList.Codes.Import;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTPART";
			var pivot = product.PivotsForBinding.AddNew();

			pivot.CI_ChildType = isExport ? ClassificationType.EXP : ClassificationType.IMP;

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_OP = product.PK;

			var supDocOnPivot = pivot.SupportingDocuments.AddNew();

			return supDocOnPivot;
		}
	}
}
