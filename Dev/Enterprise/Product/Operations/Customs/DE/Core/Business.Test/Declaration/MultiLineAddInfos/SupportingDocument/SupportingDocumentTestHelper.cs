using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class SupportingDocumentTestHelper
	{
		public SupportingDocumentTestHelper(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var countryCode = Core.Constants.CountryCodes.Germany;
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Germany", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			const string measurementUnitCodeType = "UOMEU";
			helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Document Type (EU Box 44 Imports)");
			helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Document Type (EU Box 44 Exports)");
			helper.CreateNewOrGetExistingCusCodeType(measurementUnitCodeType, "Measurement Unit Code Type");

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Division, "Desc.", importCodeType, countryCode, importCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Level, "Desc.", importCodeType, countryCode, importCodeType);
			var cusCode1 = helper.CreateCusCodeList(countryCode, importCodeType, "9001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode1.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.Certificates);
			var cusCode2 = helper.CreateCusCodeList(countryCode, importCodeType, "9002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode2.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.ImportLegalPapers);
			cusCode2.Attributes.AddNew(RefCusCodeListAttributes.Name.Level, RefCusCodeListAttributes.Value.Item);
			var cusCode2AAA = helper.CreateCusCodeList(countryCode, importCodeType, "2AAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode2AAA.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.ImportLegalPapers);
			var cusCode3 = helper.CreateCusCodeList(countryCode, importCodeType, "9003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode3.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.ProofOfPreferentialStatus);
			var cusCode4 = helper.CreateCusCodeList(countryCode, importCodeType, "9004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode4.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.MiscellaneousDocument);
			cusCode4.Attributes.AddNew(RefCusCodeListAttributes.Name.Level, RefCusCodeListAttributes.Value.Header);
			var cusCode5 = helper.CreateCusCodeList(countryCode, importCodeType, "9005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode5.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.ExemptionsExplanations);
			var cusCode6 = helper.CreateCusCodeList(countryCode, importCodeType, "9006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode6.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.ProofOfCommunityStatusOrStatusOfGoodsInFreeCirculation);
			var cusCode8 = helper.CreateCusCodeList(countryCode, importCodeType, SupportingDocumentTypes.N990, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode8.Attributes.AddNew(RefCusCodeListAttributes.Name.Division, RefCusCodeListAttributes.Value.ImportLegalPapers);

			helper.CreateCusCodeListWithAttribute(countryCode, exportCodeType, "3LLA232", "3LLA232 Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.Level, RefCusCodeListAttributes.Value.Header);
			foreach (var cusCode in new ZString[] { SupportingDocumentTypes._9ZZX, SupportingDocumentTypes._9ZZY, SupportingDocumentTypes.C612, SupportingDocumentTypes.N820, SupportingDocumentTypes.N821, SupportingDocumentTypes.N822, SupportingDocumentTypes.N952 })
			{
				helper.CreateCusCodeListWithAttribute(countryCode, exportCodeType, cusCode, $"{cusCode} Description (should be filtered)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.Level, RefCusCodeListAttributes.Value.Header);
			}

			var code = helper.CreateNewOrGetExistingCusCodeList(countryCode, exportCodeType, "3LLA231", "3LLA231 ExpDescription", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListWithAttribute(countryCode, exportCodeType, SupportingDocumentTypes._9ZZY, "9ZZY Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.Level, RefCusCodeListAttributes.Value.Item);
			helper.CreateCusCodeListWithAttribute(countryCode, exportCodeType, SupportingDocumentTypes._9ZZX, "9ZZX Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.Level, RefCusCodeListAttributes.Value.Item);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, measurementUnitCodeType, "KLT", "1000 Liter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, measurementUnitCodeType, "DTN", "100 Kilogramm", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, measurementUnitCodeType, "NAR", "Anzahl Stück", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Reference, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Complement, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Detail, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.IssuingDate, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.ValidityDate, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Value, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Unit, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.ComplementaryUnit, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.MeasurementUnit, "Desc.", exportCodeType, countryCode, measurementUnitCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Authority, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.ItemNumber, "Desc.", exportCodeType, countryCode, exportCodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.Level, "Desc.", exportCodeType, countryCode, exportCodeType);

			FullType_3LLA231_Attribute_Reference = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.Reference, RefCusCodeListAttributes.Value.Yes);
			FullType_3LLA231_Attribute_Detail = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.Detail, RefCusCodeListAttributes.Value.No);
			FullType_3LLA231_Attribute_IssuingDate = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.IssuingDate, RefCusCodeListAttributes.Value.Yes);
			FullType_3LLA231_Attribute_ValidityDate = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.ValidityDate, RefCusCodeListAttributes.Value.Yes);
			FullType_3LLA231_Attribute_ComplementaryUnit = helper.CreateCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.ComplementaryUnit, RefCusCodeListAttributes.Value.Yes);
			FullType_3LLA231_Attribute_MeasurementUnit = helper.CreateCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.MeasurementUnit, RefCusCodeListAttributes.Value.No, false);
			FullType_3LLA231_Attribute_Authority = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.Authority, RefCusCodeListAttributes.Value.No);
			FullType_3LLA231_Attribute_ItemNumber = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.ItemNumber, RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributes.Name.Level, RefCusCodeListAttributes.Value.Item);

			factory.Save();
		}

		public static void CreateRefCusCodeWithAttributeToEnsurePropertyEditable(BusinessObjectFactory factory, string attributeName, string code, string codeType)
			=> CreateRefCusCodeWithAttributeToEnsurePropertyEditable(new UniversalReferenceTestDataHelper(factory), attributeName, code, codeType);

		public static void CreateRefCusCodeWithAttributeToEnsurePropertyEditable(UniversalReferenceTestDataHelper helper, string attributeName, string code, string codeType)
		{
			helper.CreateNewOrGetExistingCusCodeType(codeType, $"{codeType} Desc.");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, $"{attributeName} Desc.", codeType, Core.Constants.CountryCodes.Germany);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, code, $"Has attribute {attributeName}", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName, RefCusCodeListAttributes.Value.No);
			code1.Factory.Save();
		}

		public RefCusCodeListAttribute FullType_3LLA231_Attribute_Reference { get; }
		public RefCusCodeListAttribute FullType_3LLA231_Attribute_Detail { get; }
		public RefCusCodeListAttribute FullType_3LLA231_Attribute_IssuingDate { get; }
		public RefCusCodeListAttribute FullType_3LLA231_Attribute_ValidityDate { get; }
		public RefCusCodeListAttribute FullType_3LLA231_Attribute_ComplementaryUnit { get; }
		public RefCusCodeListAttribute FullType_3LLA231_Attribute_MeasurementUnit { get; }
		public RefCusCodeListAttribute FullType_3LLA231_Attribute_Authority { get; }
		public RefCusCodeListAttribute FullType_3LLA231_Attribute_ItemNumber { get; }
	}
}
