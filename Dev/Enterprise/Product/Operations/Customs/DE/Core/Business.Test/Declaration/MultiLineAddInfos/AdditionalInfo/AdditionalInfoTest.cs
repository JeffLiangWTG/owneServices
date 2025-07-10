using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseCusClassification;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestValidationType()
		{
			AssertType<AdditionalInfoValidation>(headerAddInfo.Validation);
		}

		public void TestLookups()
		{
			AssertType<AdditionalInfoLookups>(headerAddInfo.Lookups);
		}

		public void TestParentType()
		{
			var additionalInfoWithPivotParent = CreateAdditionalInfoWithPivotParent(Factory);
			CombineAssertions(() =>
			{
				AssertNotNull("headerAddInfo ParentAsInvoiceHeader", headerAddInfo.ParentAsInvoiceHeader);
				AssertNull("headerAddInfo ParentAsInvoiceLine", headerAddInfo.ParentAsInvoiceLine);
				AssertNull("headerAddInfo ParentAsCusClassPartPivot", headerAddInfo.ParentAsCusClassPartPivot);
				AssertEquals("headerAddInfo ParentIsExitHeader", false, headerAddInfo.ParentIsExitDetail);

				AssertNull("lineAddInfo ParentAsInvoiceHeader", lineAddInfo.ParentAsInvoiceHeader);
				AssertNotNull("lineAddInfo ParentAsInvoiceLine", lineAddInfo.ParentAsInvoiceLine);
				AssertNull("lineAddInfo ParentAsCusClassPartPivot", lineAddInfo.ParentAsCusClassPartPivot);
				AssertEquals("lineAddInfo ParentIsExitHeader", false, lineAddInfo.ParentIsExitDetail);

				AssertNull("exitDetailAddInfo ParentAsInvoiceHeader", exitDetailAddInfo.ParentAsInvoiceHeader);
				AssertNull("exitDetailAddInfo ParentAsInvoiceLine", exitDetailAddInfo.ParentAsInvoiceLine);
				AssertNull("exitDetailAddInfo ParentAsCusClassPartPivot", exitDetailAddInfo.ParentAsCusClassPartPivot);
				AssertEquals("exitDetailAddInfo ParentIsExitHeader", true, exitDetailAddInfo.ParentIsExitDetail);

				AssertNull("additionalInfoWithPivotParent ParentAsInvoiceHeader", additionalInfoWithPivotParent.ParentAsInvoiceHeader);
				AssertNull("additionalInfoWithPivotParent ParentAsInvoiceLine", additionalInfoWithPivotParent.ParentAsInvoiceLine);
				AssertNotNull("additionalInfoWithPivotParent ParentAsCusClassPartPivot", additionalInfoWithPivotParent.ParentAsCusClassPartPivot);
				AssertEquals("additionalInfoWithPivotParent ParentIsExitHeader", false, additionalInfoWithPivotParent.ParentIsExitDetail);
			});
		}

		public void TestCSI_Description_MaxLength_Import()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Import InvoiceLine", 100, lineAddInfo.CSI_DescriptionInfo.MaxLength);
		}

		public void TestCSI_Description_MaxLength_Export_InTransitionPeriod()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Export InvoiceHeader", 70, headerAddInfo.CSI_DescriptionInfo.MaxLength);
					AssertEquals("Export InvoiceLine", 70, lineAddInfo.CSI_DescriptionInfo.MaxLength);
				});
			}
		}

		public void TestCSI_Description_MaxLength_Export_AfterTransitionPeriod()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Export InvoiceHeader", 300, headerAddInfo.CSI_DescriptionInfo.MaxLength);
					AssertEquals("Export InvoiceLine", 300, lineAddInfo.CSI_DescriptionInfo.MaxLength);
				});
			}
		}

		public void TestCSI_Description_ReadOnly_InvoiceLine_Export()
		{
			CombineAssertions(() =>
			{
				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				AssertEquals("Export InvoiceLine, CSI_SubType 'REF'", true, lineAddInfo.CSI_DescriptionInfo.ReadOnly);

				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				AssertEquals("Export InvoiceLine, CSI_SubType 'INF'", false, lineAddInfo.CSI_DescriptionInfo.ReadOnly);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import InvoiceLine", false, lineAddInfo.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_Description_ReadOnly_InvoiceHeader_Export()
		{
			CombineAssertions(() =>
			{
				headerAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				AssertEquals("Export InvoiceHeader, CSI_SubType 'REF'", true, headerAddInfo.CSI_DescriptionInfo.ReadOnly);

				headerAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
				AssertEquals("Export InvoiceHeader, CSI_SubType 'TRA'", true, headerAddInfo.CSI_DescriptionInfo.ReadOnly);

				headerAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				AssertEquals("Export InvoiceHeader, CSI_SubType 'INF'", false, headerAddInfo.CSI_DescriptionInfo.ReadOnly);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import InvoiceHeader", false, headerAddInfo.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_Description_Editable_ExitDetail()
		{
			AssertPropertyEditableIfParentIsExitDetail(x => x.CSI_DescriptionInfo);
		}

		public void TestCSI_Description_Caption()
		{
			AssertEquals("Description", lineAddInfo.CSI_DescriptionInfo.Description);
		}

		public void TestCSI_SubType_Caption()
		{
			AssertEquals("Kind", lineAddInfo.CSI_SubTypeInfo.Description);
		}

		public void TestCSI_SubType_MaxLength()
		{
			AssertEquals(3, lineAddInfo.CSI_SubTypeInfo.MaxLength);
		}

		public void TestCSI_SubType_Readonly_ExitDetail()
		{
			AssertEquals(true, exitDetailAddInfo.CSI_SubTypeInfo.ReadOnly);
		}

		public void TestCSI_SubType_Editable_Export()
		{
			CombineAssertions(() =>
			{
				AssertEquals("lineAddInfo", false, lineAddInfo.CSI_SubTypeInfo.ReadOnly);
				AssertEquals("headerAddInfo", false, headerAddInfo.CSI_SubTypeInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_Caption()
		{
			AssertEquals("Full Type", lineAddInfo.CSI_CodeInfo.Description);
		}

		public void TestCSI_Code_MaxLength_ExitDetail()
		{
			AssertEquals(5, exitDetailAddInfo.CSI_CodeInfo.MaxLength);
		}

		public void TestCSI_Code_MaxLength_NotExitDetail()
		{
			CombineAssertions(() =>
			{
				AssertEquals("headerAddInfo", 17, headerAddInfo.CSI_CodeInfo.MaxLength);
				AssertEquals("lineAddInfo", 17, lineAddInfo.CSI_CodeInfo.MaxLength);
			});
		}

		public void TestFullTypeRefCusCode_InvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "AI44E");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, "AR44E");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, Core.Constants.CountryCodes.Germany);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "DE03", "DE03 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			Factory.Save();

			CombineAssertions(() =>
			{
				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				lineAddInfo.CSI_Code = "DE03";
				AssertNull("CSI_Code has invalid level", lineAddInfo.FullTypeRefCusCode);

				lineAddInfo.CSI_Code = "DE01";
				var refCusCode = lineAddInfo.FullTypeRefCusCode;
				AssertEquals("Valid CSI_Code and CSI_SubType", "DE01", refCusCode.ZZD_Code);
				AssertSame("Cached", refCusCode, lineAddInfo.FullTypeRefCusCode);

				lineAddInfo.CSI_Code = "DE02";
				AssertNull("CSI_Code is invalid for selected CSI_SubType", lineAddInfo.FullTypeRefCusCode);

				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				AssertEquals("Now CSI_Code is valid for selected CSI_SubType", "DE02", lineAddInfo.FullTypeRefCusCode.ZZD_Code);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertNull("Import invoice line", lineAddInfo.FullTypeRefCusCode);
			});
		}

		public void TestFullTypeRefCusCode_InvoiceHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E, "TD44E");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E, Core.Constants.CountryCodes.Germany);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			Factory.Save();

			CombineAssertions(() =>
			{
				headerAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
				headerAddInfo.CSI_Code = "DE02";
				AssertNull("CSI_Code has invalid level", headerAddInfo.FullTypeRefCusCode);

				headerAddInfo.CSI_Code = "DE01";
				var refCusCode = headerAddInfo.FullTypeRefCusCode;
				AssertEquals("Valid CSI_Code and CSI_SubType", "DE01", refCusCode.ZZD_Code);
				AssertSame("Cached", refCusCode, headerAddInfo.FullTypeRefCusCode);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertNull("Import invoice header", headerAddInfo.FullTypeRefCusCode);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength_Export_AUT()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			AssertEquals(35, lineAddInfo.CSI_ReferenceNumberInfo.MaxLength);
		}

		public void TestCSI_ReferenceNumber_MaxLength_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			AssertEquals(35, lineAddInfo.CSI_ReferenceNumberInfo.MaxLength);
		}

		public void TestCSI_ReferenceNumber_MaxLength_Export_InTransitionPeriod()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			lineAddInfo.CSI_Code = EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals(35, lineAddInfo.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_ReferenceNumber_MaxLength_Export_AfterTransitionPeriod()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			lineAddInfo.CSI_Code = EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals(70, lineAddInfo.CSI_ReferenceNumberInfo.MaxLength);
			}
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			AssertEquals("Reference", lineAddInfo.CSI_ReferenceNumberInfo.Description);
		}

		public void TestCSI_ReferenceNumber_ReadOnly_Export()
		{
			AssertPropertyReadOnlyIfAttributeDoesNotExist(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, x => x.CSI_ReferenceNumberInfo);
		}

		public void TestCSI_ReferenceNumber_Editable_ExitDetail()
		{
			AssertPropertyEditableIfParentIsExitDetail(x => x.CSI_ReferenceNumberInfo);
		}

		public void TestCSI_ReferenceNumber2_MaxLength()
		{
			AssertEquals(17, lineAddInfo.CSI_ReferenceNumber2Info.MaxLength);
		}

		public void TestCSI_ReferenceNumber2_Caption()
		{
			AssertEquals("Detail", lineAddInfo.CSI_ReferenceNumber2Info.Description);
		}

		public void TestCSI_ReferenceNumber2_ReadOnly_Export()
		{
			AssertPropertyReadOnlyIfAttributeDoesNotExist(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail, x => x.CSI_ReferenceNumber2Info);
		}

		public void TestCSI_Code_ClearReadOnlyProperties()
		{
			lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			lineAddInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626;
			lineAddInfo.CSI_RX_NKCurrency = "AUD";
			lineAddInfo.CSI_Value = 10m;
			lineAddInfo.CSI_ReferenceNumber = "REF";
			lineAddInfo.CSI_ReferenceNumber2 = "REF2";
			lineAddInfo.CSI_Description = "DESC";

			lineAddInfo.CSI_Code = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("CSI_RX_NKCurrency", ZString.Empty, lineAddInfo.CSI_RX_NKCurrency);
				AssertEquals("CSI_Value", ZDecimal.Zero, lineAddInfo.CSI_Value);
				AssertEquals("CSI_ReferenceNumber", ZString.Empty, lineAddInfo.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2", ZString.Empty, lineAddInfo.CSI_ReferenceNumber2);
				AssertEquals("CSI_Description", ZString.Empty, lineAddInfo.CSI_Description);
			});
		}

		public void TestCSI_SubType_ClearReadOnlyProperties()
		{
			lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			lineAddInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626;
			lineAddInfo.CSI_RX_NKCurrency = "AUD";
			lineAddInfo.CSI_Value = 10m;
			lineAddInfo.CSI_ReferenceNumber = "REF";
			lineAddInfo.CSI_ReferenceNumber2 = "REF2";
			lineAddInfo.CSI_Description = "DESC";

			lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			CombineAssertions(() =>
			{
				AssertEquals("CSI_RX_NKCurrency", ZString.Empty, lineAddInfo.CSI_RX_NKCurrency);
				AssertEquals("CSI_Value", ZDecimal.Zero, lineAddInfo.CSI_Value);
				AssertEquals("CSI_ReferenceNumber", ZString.Empty, lineAddInfo.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2", ZString.Empty, lineAddInfo.CSI_ReferenceNumber2);
				AssertEquals("CSI_Description", ZString.Empty, lineAddInfo.CSI_Description);
			});
		}

		public void TestCSI_ReferenceNumber2_ReadOnly_Export_AUT_C626()
		{
			CombineAssertions(() =>
			{
				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAddInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019;
				AssertEquals("Kind = 'AUT', Code = 'C019'", true, lineAddInfo.CSI_ReferenceNumber2Info.ReadOnly);

				lineAddInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626;
				AssertEquals("Kind = 'AUT', Code = 'C626'", false, lineAddInfo.CSI_ReferenceNumber2Info.ReadOnly);

				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				AssertEquals("Kind = 'REF', Code = 'C626'", true, lineAddInfo.CSI_ReferenceNumber2Info.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2_ReadOnly_Export_AUT_C627()
		{
			CombineAssertions(() =>
			{
				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAddInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019;
				AssertEquals("Kind = 'AUT', Code = 'C019'", true, lineAddInfo.CSI_ReferenceNumber2Info.ReadOnly);

				lineAddInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C627;
				AssertEquals("Kind = 'AUT', Code = 'C627'", false, lineAddInfo.CSI_ReferenceNumber2Info.ReadOnly);

				lineAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				AssertEquals("Kind = 'REF', Code = 'C627'", true, lineAddInfo.CSI_ReferenceNumber2Info.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2_Editable_ExitDetail()
		{
			AssertPropertyEditableIfParentIsExitDetail(x => x.CSI_ReferenceNumber2Info);
		}

		public void TestCSI_RX_NKCurrency_Caption()
		{
			AssertEquals("Currency", lineAddInfo.CSI_RX_NKCurrencyInfo.Description);
		}

		public void TestCSI_RX_NKCurrency_ReadOnly_Export()
		{
			AssertPropertyReadOnlyIfAttributeDoesNotExist(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value, x => x.CSI_RX_NKCurrencyInfo);
		}

		public void TestCSI_RX_NKCurrency_Editable_ExitDetail()
		{
			AssertPropertyEditableIfParentIsExitDetail(x => x.CSI_RX_NKCurrencyInfo);
		}

		public void TestCSI_Value_ReadOnly_Export()
		{
			AssertPropertyReadOnlyIfAttributeDoesNotExist(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value, x => x.CSI_ValueInfo);
		}

		public void TestCSI_Value_Editable_ExitDetail()
		{
			AssertPropertyEditableIfParentIsExitDetail(x => x.CSI_ValueInfo);
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			_ = dec.Invoices.AddNew();
			var line = dec.InvoiceLines.AddNew();
			var add = line.AdditionalInfos.AddNew();
			yield return add;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			_ = dec.Invoices.AddNew();
			var line = dec.InvoiceLines.AddNew();
			var add = line.AdditionalInfos.AddNew();
			return add;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			headerAddInfo = invoice.AdditionalInfos.AddNew();
			lineAddInfo = invoiceLine.AdditionalInfos.AddNew();
			exitDetailAddInfo = Factory.NewWithValidTestData<CusExitDetail>().AdditionalInfos.AddNew();
		}
		JobDeclaration declaration;
		AdditionalInfo headerAddInfo;
		AdditionalInfo lineAddInfo;
		AdditionalInfo exitDetailAddInfo;

		void AssertPropertyReadOnlyIfAttributeDoesNotExist(string attributeName, Func<AdditionalInfo, ZPropertyInfo> propertyInfoGetter)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "AI44E");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, Core.Constants.CountryCodes.Germany);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			var info = propertyInfoGetter.Invoke(lineAddInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code empty", true, info.ReadOnly);

				lineAddInfo.CSI_Code = "DE01";
				AssertEquals("CSI_Code doesn't have attribute 'Reference'", true, info.ReadOnly);

				lineAddInfo.CSI_Code = "DE02";
				AssertEquals("CSI_Code has attribute 'Reference'", false, info.ReadOnly);
			});
		}

		void AssertPropertyEditableIfParentIsExitDetail(Func<AdditionalInfo, ZPropertyInfo> propertyInfoGetter)
		{
			var propertyToTest = propertyInfoGetter(exitDetailAddInfo);
			AssertEquals(false, propertyToTest.ReadOnly);
		}

		internal static AdditionalInfo CreateAdditionalInfoWithPivotParent(BusinessObjectFactory factory)
		{
			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTPART";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.EXP;

			var additionalInfo = pivot.AdditionalInfos.AddNew();
			return additionalInfo;
		}
	}
}
