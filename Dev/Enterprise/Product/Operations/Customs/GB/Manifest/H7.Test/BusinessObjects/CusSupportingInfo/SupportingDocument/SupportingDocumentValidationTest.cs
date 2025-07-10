using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			supportingDocument = packedItem.SupportingDocuments.AddNew();
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>
			{
				{ "Level", ["ITEM"] }
			};
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
				[importCodeType, exportCodeType], "Z983", "Z983 Desc", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(supportingDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			supportingDocument.CSI_Code = "Z111";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(supportingDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			supportingDocument.CSI_Code = "Z983";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageErrors(supportingDocument.CSI_CodeInfo);
		}

		public void TestBlankActionsAndAvailabilityAllowed_WhenCSICodeIsC600OrC601()
		{
			supportingDocument = packedItem.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "C600";
			supportingDocument.CSI_Actions = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_ActionsInfo);

			supportingDocument.CSI_Code = "C600";
			supportingDocument.CSI_Availability = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_AvailabilityInfo);

			supportingDocument.CSI_Code = "C601";
			supportingDocument.CSI_Actions = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_ActionsInfo);

			supportingDocument.CSI_Code = "C601";
			supportingDocument.CSI_Availability = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_AvailabilityInfo);
		}

		public void TestCheckCSI_Actions()
		{
			supportingDocument = packedItem.SupportingDocuments.AddNew();
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "Action Description");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "Z", "Test Action", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			supportingDocument.Validation.ValidateCSI_Actions();
			AssertHasMessageErrorContaining(supportingDocument.CSI_ActionsInfo, "Please enter an Action");

			supportingDocument.CSI_Actions = "D";
			supportingDocument.Validation.ValidateCSI_Actions();
			AssertHasMessageErrorContaining(supportingDocument.CSI_ActionsInfo, ListValidation.InvalidCodeMessageError);

			supportingDocument.CSI_Actions = "Z";
			supportingDocument.Validation.ValidateCSI_Actions();
			AssertNoMessageError(supportingDocument.CSI_ActionsInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_Availability()
		{
			supportingDocument = packedItem.SupportingDocuments.AddNew();
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Availability Description");
			var availA = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Z", "Test Availability", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			supportingDocument.Validation.ValidateCSI_Availability();
			AssertHasMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, "Please enter an Availability");

			supportingDocument.CSI_Availability = "D";
			supportingDocument.Validation.ValidateCSI_Availability();
			AssertHasMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, ListValidation.InvalidCodeMessageError);

			supportingDocument.CSI_Availability = "Z";
			supportingDocument.Validation.ValidateCSI_Availability();
			AssertNoMessageError(supportingDocument.CSI_AvailabilityInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckReferenceNumberRequiredWhenAttributeY()
		{
			supportingDocument = packedItem.SupportingDocuments.AddNew();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom"));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DC44I");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DC44E");

			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "9001", "TEST", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList.PK, GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber, "Y");
			Factory.Save();

			supportingDocument.CSI_Code = "9001";
			supportingDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, "You have not entered a Reference Number");

			Factory.Save();
		}

		public void TestUniqueCSICodeAndReference()
		{
			var supportingDocumentA = packedItem.SupportingDocuments.AddNew();
			supportingDocumentA.CSI_ReferenceNumber = "123";
			supportingDocumentA.CSI_Code = "C600";

			var supportingDocumentB = packedItem.SupportingDocuments.AddNew();
			supportingDocumentB.CSI_ReferenceNumber = "123";
			supportingDocumentB.CSI_Code = "C600";

			AssertHasMessageErrorContaining(supportingDocumentB.CSI_CodeInfo, "A row with this document type and reference number already exists on this Bill.");
		}

		public void TestValidActionAndAvailability()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "Action Pumpo");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Sminky Pinky Bang Bang");
			var actionD = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "D", "Dog", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var availD = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "D", "Daniel", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AE", "XX", "DD", "QQ" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			supportingDocument = packedItem.SupportingDocuments.AddNew();

			supportingDocument.CSI_Code = "9001";
			supportingDocument.CSI_Availability = "A";
			supportingDocument.CSI_Actions = "E";
			supportingDocument.Validation.ValidateCSI_Availability();
			supportingDocument.Validation.ValidateCSI_Actions();
			AssertNoMessageError(supportingDocument.CSI_AvailabilityInfo, "This Availability cannot be used with the selected Action. Please choose a different combination. The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAV'.");
			AssertNoMessageError(supportingDocument.CSI_ActionsInfo, "This Action cannot be used with the selected Availability. Please choose a different combination. The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAV'.");

			supportingDocument.CSI_Actions = "X";
			supportingDocument.Validation.ValidateCSI_Availability();
			supportingDocument.Validation.ValidateCSI_Actions();
			AssertHasMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, "This Availability cannot be used with the selected Action. Please choose a different combination. The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAV'.");
			AssertHasMessageErrorContaining(supportingDocument.CSI_ActionsInfo, "This Action cannot be used with the selected Availability. Please choose a different combination. The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAV'.");
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();

			helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", uk);

			Factory.Save();
		}

		SupportingDocument supportingDocument;
		AsycudaPackedItem packedItem;
		UniversalReferenceTestDataHelper helper;
		#endregion
	}
}
