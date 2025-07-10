using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusFiscalReference))]
	class CusFiscalReferenceTest : CusFiscalReferenceAbstractTest<CusFiscalReference>
	{
		public void TestProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			cusFiscalReference.CFR_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			cusFiscalReference.CFR_ParentID = invoiceLine.PK;
			CombineAssertions(() =>
			{
				var provider = cusFiscalReference.Provider;
				AssertNotNull("Isn't null", provider);

				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
				declaration.JE_GC = company.PK;
				AssertEquals("Provider is changed when DataGroupingCode changes", false, ReferenceEquals(provider, cusFiscalReference.Provider));
			});
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusReferenceTypeList.Codes.FiscalReference, cusFiscalReference.CFR_Type);
		}

		public void TestLookups()
		{
			AssertType<CusFiscalReferenceLookups>(cusFiscalReference.Lookups);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
				AssertType<UCC6ImportCusFiscalReferenceLookups>("UCC6 - Import", cusFiscalReference.Lookups);
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertType<CusFiscalReferenceLookups>(cusFiscalReference.Provider.GetNewLookups(cusFiscalReference));
			}
		}

		public void TestValidation()
		{
			AssertType<CusFiscalReferenceValidation>(cusFiscalReference.Validation);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Fiscal Reference", cusFiscalReference.HumanReadableName);
		}

		public void TestInstruction()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("EntryInstruction on CusFiscalReference", entryInstruction.PK, cusFiscalReference.Instruction.PK);
				AssertEquals(CusEntryInstructionSchema.Constants.Prefix, cusFiscalReference.CFR_ParentTableCode);
			});
		}

		public void TestInvoiceLine()
		{
			var testDec = Factory.New<JobDeclaration>();
			var invoiceLine = testDec.Invoices.AddNew().InvoiceLines.AddNew();
			var cusFiscalReference = invoiceLine.FiscalReferences.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceLine on CusFiscalReference", invoiceLine.PK, cusFiscalReference.InvoiceLine.PK);
				AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, cusFiscalReference.CFR_ParentTableCode);
			});
		}

		public void TestCFR_Code_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var fiscalRef = instruction.FiscalReferences.AddNew();
			var info = fiscalRef.CFR_CodeInfo;

			CombineAssertions("ImportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, fiscalRef.MultipleKeysToUse, "Code", "Code", "[13 16 031 000] Additional Fiscal Reference < Role");
				}
			});
		}

		public void TestCFR_Reference_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var fiscalRef = instruction.FiscalReferences.AddNew();
			var info = fiscalRef.CFR_ReferenceInfo;

			CombineAssertions("ImportUCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertCaption(info, fiscalRef.MultipleKeysToUse, "Reference", "Reference", "[13 16 034 000] Additional Fiscal Reference < VAT Identification Number");
				}
			});
		}

		void AssertCaption(ZPropertyInfo info, IReadOnlyList<string> keys, string expectedHumanReadableName, string expectedCaption, string expectedFullDescription)
		{
			AssertEquals("HumanReadableName", expectedHumanReadableName, info.HumanReadableName);
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, keys);
			AssertEquals("Caption", expectedCaption, captionResourceString.Caption);
			AssertEquals("FullDescription", expectedFullDescription, captionResourceString.FullDescription);
		}

		protected override BusinessObject GetNewBusinessObject() => cusFiscalReference;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<CusFiscalReference>();

		protected override void SetUp()
		{
			base.SetUp();
			cusFiscalReference = Factory.NewWithValidTestData<CusFiscalReference>();
		}
		CusFiscalReference cusFiscalReference;
	}

	public abstract class CusFiscalReferenceAbstractTest<T> : CusReferenceAbstractTest<T> where T : CusFiscalReference
	{
		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			if (declaration.CustomsEntryInstructions.AddNew().FiscalReferences.AddNew() is T cusFiscalReference1)
			{
				yield return FillWithValidData(cusFiscalReference1);
			}
			if (declaration.Invoices.AddNew().InvoiceLines.AddNew().FiscalReferences.AddNew() is T cusFiscalReference2)
			{
				yield return FillWithValidData(cusFiscalReference2);
			}
		}

		protected T FillWithValidData(T reference)
		{
			reference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			reference.CFR_Reference = "111";
			return reference;
		}
	}
}
