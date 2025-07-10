using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	public class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		class CusClassPartPivotForTest : CusClassPartPivot
		{
			public CusClassPartPivotForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString DataGroupingCodeForAdditionalProcedures => "CDS";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Analyzer suggests BaseSupplementaryCode.Loader, which is less readible")]
		public void TestISupplementaryCodeSupporterProperties()
		{
			var supporter = pivot as ISupplementaryCodeSupporter;

			var additionalSupplementaryCode = pivot.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode.CY_Code = "1";
			var loader = new SupplementaryCode.Loader(Factory);
			var supplementaryCode1 = loader.LoadOrCreate<SupplementaryCode, CusClassPartPivot>(pivot, 1);

			CombineAssertions(() =>
			{
				AssertEquals("SupplementaryCodesFieldType", nameof(FieldType.Text), supporter.SupplementaryCodesFieldType);
				AssertArrayEqualsByElements("SupplementaryCodes", new ZGuid[] { additionalSupplementaryCode.PK, supplementaryCode1.PK }, supporter.SupplementaryCodes.Select(x => x.PK).ToArray());
				AssertNull("Tariff", supporter.Tariff);
				AssertNull("RateSelectionCriteria", supporter.RateSelectionCriteria);
				AssertEquals("GetCountryCodeForSupplementaryCodeProvider", Core.Constants.CountryCodes.Latvia, supporter.GetCountryCodeForCodeProvider());
				AssertNull("CachedListOfAdditionalCodeDescriptions", supporter.CachedListOfAdditionalCodeDescriptions);
				AssertNull("SupplementaryCodeCaption", supporter.SupplementaryCodeCaption);
			});
		}

		[TestDate(2015, 8, 22)]
		public void TestITaxAndDocsProviderValuationDate()
		{
			var ci = Factory.New<CusClassPartPivot>();
			var td = ci as ITaxAndDocsProvider;
			AssertEquals(new ZDateTime(2015, 8, 22), td.DateOfValuation);
		}

		public void TestNotificationsOnInvalidCI_ChildTypeValidation()
		{
			var pivot2 = product.PivotsForBinding.AddNew();
			AssertNoNotifications("Pivot1 shouldn't have any notifications.", pivot);
			AssertNoNotifications("Pivot2 shouldn't have any notifications.", pivot2.CI_ChildTypeInfo);

			pivot.CI_ChildType = "BTH";
			pivot2.CI_ChildType = "BTH";
			pivot.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertHasError(pivot.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");

			pivot.CI_ChildType = "BTH";
			pivot2.CI_ChildType = "IMP";
			pivot.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertHasError(pivot.CI_ChildTypeInfo, "One organization cannot have a Type of 'IMP' and 'BTH', consider adding a type of 'EXP'.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "One organization cannot have a Type of 'IMP' and 'BTH', consider adding a type of 'EXP'.");

			pivot.CI_ChildType = "IMP";
			pivot2.CI_ChildType = "IMP";
			pivot.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertHasError(pivot.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for IMP classifications. Duplicates are only allowed where Attributes are specified.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for IMP classifications. Duplicates are only allowed where Attributes are specified.");

			pivot.CI_ChildType = "EXP";
			pivot2.CI_ChildType = "EXP";
			pivot.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertHasError(pivot.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");
			AssertHasError(pivot2.CI_ChildTypeInfo, "The combination of Classification Type and Organization should be unique for EXP and BTH classifications.");

			pivot.CI_ChildType = "EXP";
			pivot2.CI_ChildType = "IMP";
			pivot.Validation.ValidateAll();
			pivot2.Validation.ValidateAll();
			AssertNoNotifications("Pivot1 shouldn't have any notifications.", pivot);
			AssertNoNotifications("Pivot2 shouldn't have any notifications.", pivot2);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("BTH", pivot.CI_ChildType);
		}

		public void TestLookups()
		{
			AssertType(typeof(CusClassPartPivotLookups), pivot.Lookups);
		}

		public void TestConfiguration()
		{
			AssertType(typeof(CusClassPartPivotConfiguration), pivot.Configuration);
		}

		public void TestIAdditionalProcedureParent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "A", "11", "11", "111", "One", "IMP", group: "CDS");
			helper.CreateRefCusProcedure("CDS", "A", "22", "22", "222", "Two", "IMP", group: "CDS");
			helper.CreateRefCusProcedure("CDS", "A", "33", "33", "333", "Three", "EXP", group: "CDS");

			var pivot = Factory.New<CusClassPartPivotForTest>();
			pivot.CI_CPC = "1111222";
			pivot.CI_ChildType = "IMP";
			IAdditionalProcedureParent cpcParent = pivot;

			var codes = cpcParent.AdditionalProcedureCodeList;
			AssertEquals(1, codes.Count);
			Assert(codes.ContainsCode("1111111"));

			AssertEquals("1111222", cpcParent.MainProcedure);
			AssertEquals("1111", cpcParent.MainProcedurePrefix);
			AssertEquals(98, cpcParent.MaxNumberOfAdditionalProcedureCode);
		}

		public void TestCI_ThirdQty()
		{
			AssertEquals(0m, pivot.CI_ThirdQty);
			pivot.CI_ThirdQty = 69.70m;
			AssertEquals(69.70m, pivot.CI_ThirdQty);
			Factory.Save();
			pivot.Reload();
			AssertEquals(69.70m, pivot.CI_ThirdQty);
		}

		public void TestCI_CPC()
		{
			AssertEquals("", pivot.CI_CPC);
			pivot.CI_CPC = "4000000";
			AssertEquals("4000000", pivot.CI_CPC);
			Factory.Save();
			pivot.Reload();
			AssertEquals("4000000", pivot.CI_CPC);
		}

		public void TestCI_Supplement1()
		{
			AssertEquals("", pivot.CI_Supplement1);
			pivot.CI_Supplement1 = "1111";
			AssertEquals("1111", pivot.CI_Supplement1);
			Factory.Save();
			pivot.Reload();
			AssertEquals("1111", pivot.CI_Supplement1);
			AssertEquals("", pivot.CI_Supplement2);
		}

		public void TestCI_Supplement2()
		{
			AssertEquals("", pivot.CI_Supplement2);
			pivot.CI_Supplement2 = "1111";
			AssertEquals("1111", pivot.CI_Supplement2);
			Factory.Save();
			pivot.Reload();
			AssertEquals("1111", pivot.CI_Supplement2);
			AssertEquals("", pivot.CI_Supplement1);
		}

		public void TestNumberOfSupplementaryCodesAllowed()
		{
			for (int i = 0; i < NumberOfAdditionalSupplementaryCodes; i++)
			{
				pivot.AdditionalSupplementaryCodes.AddNew();
			}
			Assert("Should not be able to add more supplementary codes", !pivot.AdditionalSupplementaryCodes.AllowNew);
		}

		protected virtual int NumberOfAdditionalSupplementaryCodes => 8;

		public void TestAdditionalSupplementaryCodesCalculatedField()
		{
			var collection = pivot.AdditionalSupplementaryCodes;
			collection.AddNew("ABCD");
			collection.AddNew("EFGH");
			AssertEquals("ABCD,EFGH", pivot.CI_AdditionalSupplements);
		}

		public void TestCI_RN_NKCountryOfOrigin()
		{
			AssertEquals(ZString.Empty, pivot.CI_RN_NKCountryOfOrigin);
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryCodes.Australia, pivot.CI_RN_NKCountryOfOrigin);
			Factory.Save();
			pivot.Reload();
			AssertEquals(Core.Constants.CountryCodes.Australia, pivot.CI_RN_NKCountryOfOrigin);
		}

		public void TestCI_ConcessionOrder()
		{
			AssertEquals(ZString.Empty, pivot.CI_ConcessionOrder);
			pivot.CI_ConcessionOrder = "111";
			AssertEquals("111", pivot.CI_ConcessionOrder);
			Factory.Save();
			pivot.Reload();
			AssertEquals("111", pivot.CI_ConcessionOrder);
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, pivot.SupportingDocuments.Count);
			var child = pivot.SupportingDocuments.AddNew();
			AssertEquals(1, pivot.SupportingDocuments.Count);
			child.CSI_Code = "X";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			pivot = factory2.Load<CusClassPartPivot>(pivot.PK);
			AssertEquals(1, pivot.SupportingDocuments.Count);
			AssertEquals("X", child.CSI_Code);
		}

		public void TestPreviousDocuments()
		{
			AssertEquals(0, pivot.PreviousDocuments.Count);
			var child = pivot.PreviousDocuments.AddNew();
			AssertEquals(1, pivot.PreviousDocuments.Count);
			child.CSI_SubType = "X";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			pivot = factory2.Load<CusClassPartPivot>(pivot.PK);
			AssertEquals(1, pivot.PreviousDocuments.Count);
			AssertEquals("X", child.CSI_SubType);
		}

		public void TestAdditionalInfos()
		{
			AssertEquals(0, pivot.AdditionalInfos.Count);
			var child = pivot.AdditionalInfos.AddNew();
			AssertEquals(1, pivot.AdditionalInfos.Count);
			child.CSI_Description = "X";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			pivot = factory2.Load<CusClassPartPivot>(pivot.PK);
			AssertEquals(1, pivot.AdditionalInfos.Count);
			AssertEquals("X", child.CSI_Description);
		}

		public void TestTaxes()
		{
			AssertEquals(0, pivot.Taxes.Count);
			var child = pivot.Taxes.AddNew();
			AssertEquals(1, pivot.Taxes.Count);
			child.Data.G4_Type = "A00";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			pivot = factory2.Load<CusClassPartPivot>(pivot.PK);
			AssertEquals(1, pivot.Taxes.Count);
			var child2 = pivot.Taxes[0];
			AssertEquals("A00", child2.Data.G4_Type);
		}

		public void TestPreferenceCodeMaxLength()
		{
			AssertEquals(3, pivot.PreferenceCodeInfo.MaxLength);
		}

		public void TestIsClassificationBoth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BTH", true, pivot.IsClassificationBoth);
				pivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("IMP", false, pivot.IsClassificationBoth);
				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("EXP", false, pivot.IsClassificationBoth);
				pivot.CI_ChildType = ZString.Empty;
				AssertEquals("Empty", false, pivot.IsClassificationBoth);
			});
		}

		public void TestClearAllDetailsUnrelatedToBoth_Confirmed()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "~~~";
			product.OP_PartNum = "ABC";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			pivot.CI_ChildType = ClassificationType.IMP;
			pivot.CI_TariffNum = "1234567890";

			pivot.CI_Supplement1 = "Supp1";
			pivot.CI_Supplement2 = "Supp2";
			pivot.AdditionalSupplementaryCodes.AddNew();
			pivot.AdditionalSupplementaryCodes.AddNew();

			pivot.CI_CPC = "CPC";
			pivot.AdditionalProcedureCodes.AddNew();
			pivot.AdditionalProcedureCodes.AddNew();

			pivot.CI_ConcessionOrder = "ConOrder";
			pivot.CI_ThirdQty = 3.33m;
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;
			pivot.PreferenceCode = "PRC";

			pivot.SupportingDocuments.AddNew();
			pivot.SupportingDocuments.AddNew();

			pivot.AdditionalInfos.AddNew();
			pivot.AdditionalInfos.AddNew();

			pivot.PreviousDocuments.AddNew();
			pivot.PreviousDocuments.AddNew();

			pivot.Taxes.AddNew();
			pivot.Taxes.AddNew();

			pivot.OnChildTypeToBeBothAndClearUnrelatedDetails += (sender, args) => { args.Cancel = false; };
			pivot.CI_ChildType = ClassificationType.Both;
			CombineAssertions(() =>
			{
				AssertEquals("ChildType should be set to 'BTH' and not stopped by the confirmation", ClassificationType.Both, pivot.CI_ChildType);
				AssertEquals("CI_TariffNum should not have changed", "1234567890", pivot.CI_TariffNum);

				AssertEquals("CI_Supplement1 should have been cleared", ZString.Empty, pivot.CI_Supplement1);
				AssertEquals("CI_Supplement2 should have been cleared", ZString.Empty, pivot.CI_Supplement2);
				AssertEquals("Additional Supplementary Codes should have been cleared", 0, pivot.AdditionalSupplementaryCodes.Count);

				AssertEquals("CI_CPC should have been cleared", ZString.Empty, pivot.CI_CPC);
				AssertEquals("Additional Procedure Codes should have been cleared", 0, pivot.AdditionalProcedureCodes.Count);

				AssertEquals("CI_ConcessionOrder should have been cleared", ZString.Empty, pivot.CI_ConcessionOrder);
				AssertEquals("CI_ThirdQty should have been cleared", ZDecimal.Zero, pivot.CI_ThirdQty);
				AssertEquals("CI_RN_NKCountryOfOrigin should not have changed", Core.Constants.CountryCodes.Latvia, pivot.CI_RN_NKCountryOfOrigin);
				AssertEquals("PreferenceCode should have been cleared", ZString.Empty, pivot.PreferenceCode);

				AssertEquals("SupportingDocuments should have been cleared", 0, pivot.SupportingDocuments.Count);
				AssertEquals("AdditionalInfos should have been cleared", 0, pivot.AdditionalInfos.Count);
				AssertEquals("PreviousDocuments should have been cleared", 0, pivot.PreviousDocuments.Count);
				Assert("Taxes should have been cleared", !pivot.Taxes.Any());
			});
		}

		public void TestClearAllDetailsUnrelatedToBoth_Cancelled()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "~~~";
			product.OP_PartNum = "ABC";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			pivot.CI_ChildType = ClassificationType.IMP;
			pivot.CI_TariffNum = "1234567890";

			pivot.CI_Supplement1 = "Supp1";
			pivot.CI_Supplement2 = "Supp2";
			pivot.AdditionalSupplementaryCodes.AddNew();
			pivot.AdditionalSupplementaryCodes.AddNew();

			pivot.CI_CPC = "CPC";
			pivot.AdditionalProcedureCodes.AddNew();
			pivot.AdditionalProcedureCodes.AddNew();

			pivot.CI_ConcessionOrder = "ConOrder";
			pivot.CI_ThirdQty = 3.33m;
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Latvia;
			pivot.PreferenceCode = "PRC";

			pivot.SupportingDocuments.AddNew();
			pivot.SupportingDocuments.AddNew();

			pivot.AdditionalInfos.AddNew();
			pivot.AdditionalInfos.AddNew();

			pivot.PreviousDocuments.AddNew();
			pivot.PreviousDocuments.AddNew();

			pivot.Taxes.AddNew();
			pivot.Taxes.AddNew();

			pivot.OnChildTypeToBeBothAndClearUnrelatedDetails += (sender, args) => { args.Cancel = true; };
			pivot.CI_ChildType = ClassificationType.Both;
			CombineAssertions(() =>
			{
				AssertEquals("ChildType keeps unchanged", ClassificationType.IMP, pivot.CI_ChildType);
				AssertEquals("CI_TariffNum should not have changed", "1234567890", pivot.CI_TariffNum);

				AssertEquals("CI_Supplement1 should not have changed", "Supp1", pivot.CI_Supplement1);
				AssertEquals("CI_Supplement2 should not have changed", "Supp2", pivot.CI_Supplement2);
				AssertNotEquals("Additional Supplementary Codes should not have changed", 0, pivot.AdditionalSupplementaryCodes.Count);

				AssertEquals("CI_CPC should not have changed", "CPC", pivot.CI_CPC);
				AssertNotEquals("Additional Procedure Codes should not have changed", 0, pivot.AdditionalProcedureCodes.Count);

				AssertEquals("CI_ConcessionOrder should not have changed", "ConOrder", pivot.CI_ConcessionOrder);
				AssertEquals("CI_ThirdQty should not have changed", 3.33m, pivot.CI_ThirdQty);
				AssertEquals("CI_RN_NKCountryOfOrigin should not have changed", Core.Constants.CountryCodes.Latvia, pivot.CI_RN_NKCountryOfOrigin);
				AssertEquals("PreferenceCode should not have changed", "PRC", pivot.PreferenceCode);

				AssertNotEquals("SupportingDocuments should not have changed", 0, pivot.SupportingDocuments.Count);
				AssertNotEquals("AdditionalInfos should not have changed", 0, pivot.AdditionalInfos.Count);
				AssertNotEquals("PreviousDocuments should not have changed", 0, pivot.PreviousDocuments.Count);
				Assert("Taxes should not have changed", pivot.Taxes.Any());
			});
		}

		public void TestCI_Supplement1_ReadOnly()
		{
			CombineAssertions(() =>
			{
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Both", true, pivot.CI_Supplement1Info.ReadOnly);
				pivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("IMP", false, pivot.CI_Supplement1Info.ReadOnly);
				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("EXP", false, pivot.CI_Supplement1Info.ReadOnly);
			});
		}

		public void TestCI_Supplement2_ReadOnly()
		{
			CombineAssertions(() =>
			{
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Both", true, pivot.CI_Supplement2Info.ReadOnly);
				pivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("IMP", false, pivot.CI_Supplement2Info.ReadOnly);
				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("EXP", false, pivot.CI_Supplement2Info.ReadOnly);
			});
		}

		public void TestCI_CPC_ReadOnly()
		{
			CombineAssertions(() =>
			{
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Both", true, pivot.CI_CPCInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("IMP", false, pivot.CI_CPCInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("EXP", false, pivot.CI_CPCInfo.ReadOnly);
			});
		}

		public void TestCI_ConcessionOrder_ReadOnly()
		{
			CombineAssertions(() =>
			{
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Both", true, pivot.CI_ConcessionOrderInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("IMP", false, pivot.CI_ConcessionOrderInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("EXP", false, pivot.CI_ConcessionOrderInfo.ReadOnly);
			});
		}

		public void TestCI_ThirdQty_ReadOnly()
		{
			CombineAssertions(() =>
			{
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Both", true, pivot.CI_ThirdQtyInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("IMP", false, pivot.CI_ThirdQtyInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("EXP", false, pivot.CI_ThirdQtyInfo.ReadOnly);
			});
		}

		public void TestPreferenceCode_ReadOnly()
		{
			CombineAssertions(() =>
			{
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Both", true, pivot.PreferenceCodeInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.IMP;
				AssertEquals("IMP", false, pivot.PreferenceCodeInfo.ReadOnly);
				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("EXP", false, pivot.PreferenceCodeInfo.ReadOnly);
			});
		}

		public void TestSetTariffEtcDataOnInvoiceWhenNoClassificationExists()
		{
			pivot.CI_ChildType = ClassificationType.IMP;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			// Box 44 stuff that already exist on invoice line
			var aiOnInvoiceLine = invoiceLine.AdditionalInfos.AddNew();
			aiOnInvoiceLine.CSI_Code = "GEN01";
			aiOnInvoiceLine.CSI_Description = "Statement";
			var pdOnInvoiceLine = invoiceLine.PreviousDocuments.AddNew();
			pdOnInvoiceLine.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			pdOnInvoiceLine.CSI_ReferenceNumber = "Reference";
			pdOnInvoiceLine.CSI_Code = "380";
			var sdOnInvoiceLine = invoiceLine.SupportingDocuments.AddNew();
			sdOnInvoiceLine.CSI_Code = "ABCD";
			sdOnInvoiceLine.CSI_ReferenceNumber = "12345";
			var taxOnInvoiceLine = invoiceLine.Taxes.AddNew();
			taxOnInvoiceLine.Data.G4_Type = "A00";
			taxOnInvoiceLine.Data.G4_Amount = "123";

			// Two lots of box 44 data on pivot, on that duplicates that already on the invoice line
			var aiOnPivot1 = pivot.AdditionalInfos.AddNew();
			aiOnPivot1.CSI_Code = "GEN01";
			aiOnPivot1.CSI_Description = "Statement";
			var aiOnPivot2 = pivot.AdditionalInfos.AddNew();
			aiOnPivot2.CSI_Code = "GEN02";
			aiOnPivot2.CSI_Description = "Statement";
			var pdOnPivot1 = pivot.PreviousDocuments.AddNew();
			pdOnPivot1.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			pdOnPivot1.CSI_ReferenceNumber = "Reference";
			pdOnPivot1.CSI_Code = "380";
			var pdOnPivot2 = pivot.PreviousDocuments.AddNew();
			pdOnPivot2.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			pdOnPivot2.CSI_ReferenceNumber = "Awb number";
			pdOnPivot2.CSI_Code = "741";
			var sdOnPivot1 = pivot.SupportingDocuments.AddNew();
			sdOnPivot1.CSI_Code = "ABCD";
			sdOnPivot1.CSI_ReferenceNumber = "12345";
			var sdOnPivot2 = pivot.SupportingDocuments.AddNew();
			sdOnPivot2.CSI_Code = "HJKL";
			sdOnPivot2.CSI_ReferenceNumber = "12345";
			var taxOnPivot2 = pivot.Taxes.AddNew();
			taxOnPivot2.Data.G4_Type = "B00";

			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			pivot.CI_CPC = "A234567";
			pivot.CI_ThirdQty = 69.70m;
			pivot.CI_TariffNum = "1234567890";
			pivot.CI_Supplement1 = "1111";
			pivot.CI_Supplement2 = "2222";

			var supplementaryCodeProvider = SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);
			var numberOfAdditionalSupplementaryCodes = supplementaryCodeProvider.NumberOfCodes;
			for (int i = 3; i <= numberOfAdditionalSupplementaryCodes + 2; i++)
			{
				pivot.AdditionalSupplementaryCodes.AddNew(ZString.Format("P{0}{0}{0}", i).Substring(0, 4));
			}

			invoiceLine.JI_PartNo = pivot.Part.OP_PartNum;

			AssertEquals(Core.Constants.CountryCodes.Australia, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("A234567", invoiceLine.JI_Procedure);
			AssertEquals("1111", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("2222", invoiceLine.JI_SupplementaryCode2);
			for (int i = 3; i <= numberOfAdditionalSupplementaryCodes + 2; i++)
			{
				AssertEquals(ZString.Format("P{0}{0}{0}", i).Substring(0, 4), invoiceLine.AdditionalSupplementaryCodes[i - 3].CY_Code);
			}
			AssertEquals("1234567890", invoiceLine.JI_Tariff);
			// Now check that we create one new document, not two new documents, on the invoice line, bringing total to two (of each type) (not three).
			AssertEquals(2, invoiceLine.PreviousDocuments.Count);
			AssertEquals(2, invoiceLine.AdditionalInfos.Count);
			AssertEquals(2, invoiceLine.SupportingDocuments.Count);
			AssertEquals("GEN01", invoiceLine.AdditionalInfos[0].CSI_Code);
			AssertEquals("GEN02", invoiceLine.AdditionalInfos[1].CSI_Code);
			AssertEquals("380", invoiceLine.PreviousDocuments[0].CSI_Code);
			AssertEquals("741", invoiceLine.PreviousDocuments[1].CSI_Code);
			AssertEquals("ABCD", invoiceLine.SupportingDocuments[0].CSI_Code);
			AssertEquals("HJKL", invoiceLine.SupportingDocuments[1].CSI_Code);
			AssertEquals("A00", invoiceLine.Taxes[0].Data.G4_Type);
			AssertEquals("B00", invoiceLine.Taxes[1].Data.G4_Type);

			invoiceLine.JI_Tariff = ZString.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = org.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			pivot.CI_ChildType = ClassificationType.EXP;
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_PartNo = pivot.Part.OP_PartNum;
			AssertEquals("12345678", invoiceLine.JI_Tariff);
		}

		public void TestIsImportAndIsExport()
		{
			// Behavior without a classification
			pivot.CI_ChildType = ClassificationType.IMP;
			AssertEquals(true, ((ICanBeImportOrExport)pivot).IsImport);
			AssertEquals(false, ((ICanBeImportOrExport)pivot).IsExport);
			pivot.CI_ChildType = ClassificationType.EXP;
			AssertEquals(false, ((ICanBeImportOrExport)pivot).IsImport);
			AssertEquals(true, ((ICanBeImportOrExport)pivot).IsExport);
			pivot.CI_ChildType = ClassificationType.Both;
			AssertEquals(true, ((ICanBeImportOrExport)pivot).IsImport);
			AssertEquals(true, ((ICanBeImportOrExport)pivot).IsExport);

			var classification = Factory.New<CusClassification>();
			pivot.CI_CC = classification.PK;
			pivot.Classification.CC_ClassificationType = ClassificationType.IMP;
			AssertEquals("Classification type trumps child type", true, ((ICanBeImportOrExport)pivot).IsImport);
			AssertEquals("Classification type trumps child type", false, ((ICanBeImportOrExport)pivot).IsExport);
			AssertEquals(Directions.Import, ((IImportExport)pivot).JobDirection);

			pivot.Classification.CC_ClassificationType = ClassificationType.EXP;
			AssertEquals("Classification type trumps child type", false, ((ICanBeImportOrExport)pivot).IsImport);
			AssertEquals("Classification type trumps child type", true, ((ICanBeImportOrExport)pivot).IsExport);
			AssertEquals(Directions.Export, ((IImportExport)pivot).JobDirection);

			pivot.Classification.CC_ClassificationType = ClassificationType.Both;
			AssertEquals("Classification type trumps child type", true, ((ICanBeImportOrExport)pivot).IsImport);
			AssertEquals("Classification type trumps child type", true, ((ICanBeImportOrExport)pivot).IsExport);
			AssertEquals(Directions.Import, ((IImportExport)pivot).JobDirection);

			var impl = pivot as ICanBeImportOrExport;
			AssertEquals("Level", "Item", impl.Level);
			AssertEquals("Country Code", pivot.CI_RN_NKCountry, impl.TrueCountryCode);
			AssertEquals("Data Grouping", pivot.CI_RN_NKCountry, impl.DataGroupingCode);
		}

		public void TestReadOnlyMembers()
		{
			AssertEquals(false, pivot.CI_TariffNumInfo.ReadOnly);
			AssertEquals(false, pivot.CI_FormattedTariffNumInfo.ReadOnly);
			AssertEquals(false, pivot.CI_CCInfo.ReadOnly);
			pivot.CI_FormattedTariffNum = "123";
			AssertEquals(false, pivot.CI_TariffNumInfo.ReadOnly);
			AssertEquals(false, pivot.CI_FormattedTariffNumInfo.ReadOnly);
			AssertEquals(true, pivot.CI_CCInfo.ReadOnly);

			pivot.CI_FormattedTariffNum = ZString.Empty;
			pivot.CI_CC = ZGuid.NewZGuid();
			AssertEquals(true, pivot.CI_TariffNumInfo.ReadOnly);
			AssertEquals(true, pivot.CI_FormattedTariffNumInfo.ReadOnly);
			AssertEquals(false, pivot.CI_CCInfo.ReadOnly);
		}

		public void TestITariffFormatProvider()
		{
			AssertType<TariffFormatterThirteen>("TariffFormatter", ((ITariffFormatProvider)pivot).TariffFormatter);
		}

		public void TestSupplementaryCodes()
		{
			AssertEquals("When NO Supplementary Codes are entered, SupplementaryCodes Count", 0, pivot.SupplementaryCodes.Count());

			pivot.CI_Supplement1 = "S001";
			AssertEquals("SupplementaryCodes Count", 1, pivot.SupplementaryCodes.Count());
			AssertContainsExactElementsInAnyOrder("", new ZString[] { "S001" }, pivot.SupplementaryCodes.Select(x => x.CY_Code).ToArray());

			pivot.CI_Supplement2 = "S002";

			var additionalSupplementaryCode3 = pivot.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode3.CY_Code = "S003";
			additionalSupplementaryCode3.CY_Order = 3;
			var additionalSupplementaryCode4 = pivot.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode4.CY_Code = "";
			additionalSupplementaryCode3.CY_Order = 4;

			AssertEquals("SupplementaryCodes Count", 4, pivot.SupplementaryCodes.Count());
			AssertContainsExactElementsInAnyOrder("", new ZString[] { "S001", "S002", "S003", "" }, pivot.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
		}

		public void TestGetCountryCodeFromAdditionalCode()
		{
			AssertNullOrEmpty(pivot.GetCountryCodeFromAdditionalCode("AD"), ZString.Empty);
		}

		public void TestCI_GoodsCategory()
		{
			AssertEquals(ZString.Empty, pivot.CI_GoodsCategory);
			pivot.CI_GoodsCategory = "1";
			AssertEquals("1", pivot.CI_GoodsCategory);
			Factory.Save();
			pivot.Reload();
			AssertEquals("1", pivot.CI_GoodsCategory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return pivot;
		}

		protected override void SetUp()
		{
			org = Factory.NewWithValidTestData<OrgHeader>();
			product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = ClassificationType.Both;
			relationship.OU_OH = org.PK;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.Both;
		}
		OrgSupplierPart product;
		OrgHeader org;
		CusClassPartPivot pivot;
	}
}
