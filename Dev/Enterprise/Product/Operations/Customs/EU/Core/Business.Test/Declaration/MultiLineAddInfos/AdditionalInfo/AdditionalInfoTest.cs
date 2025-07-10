using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	public class AdditionalInfoTest : ImportExportAwareSupportingInfoTest<AdditionalInfo>
	{
		public void TestCSI_SubType_Caption()
		{
			var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyImportUCC6);
			CombineAssertions("CSI_SubType Caption: ImportUCC6", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("FullDescription", "[12 03 002 000] Kind", data.FullDescription);
			});

			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyExportUCC6);
			CombineAssertions("CSI_SubType Caption: ExportUCC6", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("Full Description", string.Empty, data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_SubType Caption: UCC6ExportOrImport_InvoiceHeader + INF", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("FullDescription", "[12 02 001 000] Additional Information", data.FullDescription);
			});

			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyExportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_SubType Caption: UCC6ExportOrImport_InvoiceHeader + INF", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("FullDescription", "[12 02 000 000] Additional Information", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_SubType Caption: UCC6ExportOrImport_InvoiceHeader + REF", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("FullDescription", "[12 04 001 000] Additional Reference", data.FullDescription);
			});

			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyExportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_SubType Caption: UCC6ExportOrImport_InvoiceHeader + REF", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("FullDescription", "[12 04 000 000] Additional Reference", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_SubType Caption: UCC6ExportOrImport_InvoiceHeader + TRA", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("FullDescription", "[12 05 001 000] Transport Document", data.FullDescription);
			});

			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_SubTypeInfo, JobDeclaration.CaptionKeyExportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_SubType Caption: UCC6ExportOrImport_InvoiceHeader + TRA", () =>
			{
				AssertEquals("Caption", "Kind", data.Caption);
				AssertEquals("FullDescription", "[12 05 000 000] Transport Document", data.FullDescription);
			});
		}

		public void TestCSI_Code_Caption()
		{
			var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_CodeInfo, JobDeclaration.CaptionKeyImportUCC6);
			CombineAssertions("CSI_Code Caption: ImportUCC6", () =>
			{
				AssertEquals("Caption", "Full Type", data.Caption);
				AssertEquals("Full Description", "[12 03 001 000] Full Type", data.FullDescription);
			});

			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_CodeInfo, null);
			CombineAssertions("CSI_Code Caption: ExportUCC6", () =>
			{
				AssertEquals("Caption", "Code", data.Caption);
				AssertEquals("Full Description", string.Empty, data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_CodeInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_Code Caption: UCC6ExportOrImport_InvoiceHeader + INF", () =>
			{
				AssertEquals("Caption", "Full type", data.Caption);
				AssertEquals("FullDescription", "[12 02 001 000] Additional Information type", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_CodeInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_Code Caption: UCC6ExportOrImport_InvoiceHeader + REF", () =>
			{
				AssertEquals("Caption", "Full type", data.Caption);
				AssertEquals("FullDescription", "[12 04 001 000] Additional Reference type", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_CodeInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_Code Caption: UCC6ExportOrImport_InvoiceHeader + TRA", () =>
			{
				AssertEquals("Caption", "Full type", data.Caption);
				AssertEquals("FullDescription", "[12 05 001 000] Transport Document type", data.FullDescription);
			});
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_ReferenceNumberInfo, JobDeclaration.CaptionKeyImportUCC6);
			CombineAssertions("CSI_ReferenceNumber Caption: ImportUCC6", () =>
			{
				AssertEquals("Caption", "Reference", data.Caption);
				AssertEquals("FullDescription", "[12 04 002 000] Additional Reference number", data.FullDescription);
			});

			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_ReferenceNumberInfo, null);
			CombineAssertions("CSI_ReferenceNumber Caption: ExportUCC6", () =>
			{
				AssertEquals("Caption", "Reference", data.Caption);
				AssertEquals("Full Description", string.Empty, data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_ReferenceNumberInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_ReferenceNumber Caption: UCC6ExportOrImport_InvoiceHeader + INF", () =>
			{
				AssertEquals("Caption", "Reference", data.Caption);
				AssertEquals("FullDescription", "[12 04 002 000] Additional Reference number", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_ReferenceNumberInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_ReferenceNumber Caption: UCC6ExportOrImport_InvoiceHeader + REF", () =>
			{
				AssertEquals("Caption", "Reference", data.Caption);
				AssertEquals("FullDescription", "[12 04 002 000] Additional Reference number", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_ReferenceNumberInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_ReferenceNumber Caption: UCC6ExportOrImport_InvoiceHeader + TRA", () =>
			{
				AssertEquals("Caption", "Reference", data.Caption);
				AssertEquals("FullDescription", "[12 05 002 000] Transport Document Reference number", data.FullDescription);
			});
		}

		public void TestCSI_Description_Caption()
		{
			var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_DescriptionInfo, JobDeclaration.CaptionKeyImportUCC6);
			CombineAssertions("CSI_Description Caption: ImportUCC6", () =>
			{
				AssertEquals("Caption", "Description", data.Caption);
				AssertEquals("FullDescription", "[12 02 009 000] Additional Reference Description", data.FullDescription);
			});

			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_DescriptionInfo, null);
			CombineAssertions("CSI_Description Caption: ExportUCC6", () =>
			{
				AssertEquals("Caption", "Description", data.Caption);
				AssertEquals("Full Description", string.Empty, data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_DescriptionInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_Description Caption: UCC6ExportOrImport_InvoiceHeader + INF", () =>
			{
				AssertEquals("Caption", "Description", data.Caption);
				AssertEquals("FullDescription", "[12 02 009 000] Additional information Description", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_DescriptionInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_Description Caption: UCC6ExportOrImport_InvoiceHeader + REF", () =>
			{
				AssertEquals("Caption", "Description", data.Caption);
				AssertEquals("FullDescription", "[12 02 009 000] Additional Reference Description", data.FullDescription);
			});

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(additionalInfo.CSI_DescriptionInfo, JobDeclaration.CaptionKeyImportUCC6, new DataBoundBusinessObject(additionalInfo));
			CombineAssertions("CSI_Description Caption: UCC6ExportOrImport_InvoiceHeader + TRA", () =>
			{
				AssertEquals("Caption", "Description", data.Caption);
				AssertEquals("FullDescription", "Transport Document Description", data.FullDescription);
			});
		}

		public void TestCSI_Value_Caption()
		{
			AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(additionalInfo.CSI_ValueInfo).Caption);
		}

		public void TestCSI_Value_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(AdditionalInfo), nameof(AdditionalInfo.CSI_Value), false, x => x.DecimalPlaces == 2);
		}

		public void TestAdditionalInfoMaxLength()
		{
			AssertEquals("DescriptionMaxLength", 70, additionalInfo.CSI_DescriptionInfo.MaxLength);
		}

		public void TestAdditionalInfoLevel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var cusCode1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode1.Attributes.AddNew("Direction", "IMPORT");
			cusCode1.Attributes.AddNew("Direction", "EXPORT");
			cusCode1.Attributes.AddNew("Level", "ITEM");

			var cusCode2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode2.Attributes.AddNew("Direction", "IMPORT");
			cusCode2.Attributes.AddNew("Direction", "EXPORT");
			cusCode2.Attributes.AddNew("Level", "HEADER");

			var cusCode3 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode3.Attributes.AddNew("Direction", "IMPORT");
			cusCode3.Attributes.AddNew("Direction", "EXPORT");
			cusCode3.Attributes.AddNew("Level", "ITEM");
			cusCode3.Attributes.AddNew("Level", "HEADER");

			Factory.Save();

			var data = additionalInfo;
			data.CSI_Code = "9001";
			Assert(data.IsLine);
			Assert(!data.IsHeaderOnly);
			Assert(data.IsLineOnly);

			data.CSI_Code = "9002";
			Assert(!data.IsLine);
			Assert(data.IsHeaderOnly);
			Assert(!data.IsLineOnly);

			data.CSI_Code = "9003";
			Assert(data.IsLine);
			Assert(!data.IsHeaderOnly);
			Assert(!data.IsLineOnly);
		}

		public void TestKeyToDeterimeUniqueness()
		{
			additionalInfo.CSI_Code = "TYPE";
			additionalInfo.CSI_Description = "DESC";
			AssertEquals("TYPEDESC", additionalInfo.KeyToDeterimeUniqueness);
		}

		public void TestCSI_StatusDefaultValue()
		{
			AssertEquals($"CSI_Status should default to {AdditionalInfoIssuerList.Codes.Customs}.", AdditionalInfoIssuerList.Codes.Customs, additionalInfo.CSI_Status);
		}

		public void TestHumanReadableName()
		{
			AssertEquals(nameof(additionalInfo.HumanReadableName), "Additional Info", additionalInfo.HumanReadableName);
		}

		public void TestCSI_RX_NKCurrency_Caption()
		{
			AssertEquals("Currency", DataBoundResourceStrings.GetDataForProperty(additionalInfo.CSI_RX_NKCurrencyInfo).Caption);
		}

		public void TestCSI_ReferenceNumber2_Caption()
		{
			AssertEquals("Detail", DataBoundResourceStrings.GetDataForProperty(additionalInfo.CSI_ReferenceNumber2Info).Caption);
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.AdditionalInfos.AddNew();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.AdditionalInfos.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.AdditionalInfos.AddNew();
			var product = factory.New<MasterFiles.OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.Both;
			yield return pivot.AdditionalInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => additionalInfo;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			additionalInfo = invoiceHeader.AdditionalInfos.AddNew();
		}
		AdditionalInfo additionalInfo;
	}
}
