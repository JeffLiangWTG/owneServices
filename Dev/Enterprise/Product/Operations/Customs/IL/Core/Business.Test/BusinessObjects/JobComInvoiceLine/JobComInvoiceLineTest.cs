using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Israel, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			var importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			var customsTemplate_Company = Factory.New<GlbCompany>();
			customsTemplate_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			var customsTemplate_Branch = customsTemplate_Company.Branches.AddNew();
			customsTemplate_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Israel)).RL_Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = customsTemplate_Branch.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertType<OrgSupplierPart>("Product type gets changed depending on who is requesting", invoiceLine.Part);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertType<OrgSupplierPart>("product type still the type", declarationLoaded.InvoiceLines[0].Part);
		}

		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<JobComInvoiceLine>("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)));
		}

		public void TestPermitCollection()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			AssertType<PermitCollection>(invoiceLine.Permits);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)invoiceLine).GetCusSupportingInfoTypes();
			AssertEquals(typeof(Permit), actualTypes["PRM"]);
			AssertEquals(typeof(PreviousDocument), actualTypes["PRE"]);
		}

		public void TestGetFetchStrategies()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var expectedTypes = new[] { typeof(Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy) };
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)invoiceLine).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.CustomsEntryInstructions?.Load();

			AssertType<CusEntryInstruction>(invoiceLine.EntryInstruction);
		}

		public void TestBaseType()
		{
			AssertEquals(typeof(AutoILJobComInvoiceLine), typeof(JobComInvoiceLine).BaseType);
		}

		public void TestValidation()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			AssertType<JobComInvoiceLineValidation>(invoiceLine.Validation);

			AssertEquals(typeof(AutoILJobComInvoiceLineValidation), typeof(JobComInvoiceLineValidation).BaseType);
		}

		public void TestLookups()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			AssertType<JobComInvoiceLineLookups>(invoiceLine.Lookups);
		}

		public void TestJI_PreferenceDocNumber()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			AssertNotNull(invoiceLine.JI_PreferenceDocNumberInfo);
			AssertEquals(35, invoiceLine.JI_PreferenceDocNumberInfo.MaxLength);
		}

		public void TestCaptions()
		{
			AssertCaptions("JI_LineNo", "Seq.No");
			AssertCaptions("JI_CustomsSecondQuantity", "Statistical Qty");
			AssertCaptions("JI_CustomsThirdQuantity", "Additional Qty");
			AssertCaptions("JI_BondedWhsQuantity", "Countable Qty");
			AssertCaptions("JI_ZZF_NKTaxType", "VAT Code");
			AssertCaptions("JI_PreferenceDocNumber", "Preference Doc.#");
		}

		public void TestDecimalPlaces()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			AssertDecimalPlaces(invoiceLine, "JI_CustomsQuantity");
			AssertDecimalPlaces(invoiceLine, "JI_CustomsSecondQuantity");
			AssertDecimalPlaces(invoiceLine, "JI_CustomsThirdQuantity");
		}

		public void TestUniversalTariffType()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			AssertEquals("Tariff Type should be IMP", "IMP", invoiceLine.UniversalTariffType);
		}

		public void TestPreviousDocuments()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			AssertType<PreviousDocumentCollection>(invoiceLine.PreviousDocuments);
		}

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);

		void AssertCaptions(string propertyName, string caption)
		{
			AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), propertyName).Caption);
		}

		static void AssertDecimalPlaces(JobComInvoiceLine invoiceLine, string propertyName)
		{
			CombineAssertions($"Check decimal places for {propertyName}", () =>
			{
				var decimalPlacesAttribute = invoiceLine.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(DecimalPlacesAttribute), false).FirstOrDefault() as DecimalPlacesAttribute;
				AssertNotNull("DecimalPlaces attribute should be defined", decimalPlacesAttribute);
				AssertEquals(3, decimalPlacesAttribute.DecimalPlaces);
			});
		}
	}
}
