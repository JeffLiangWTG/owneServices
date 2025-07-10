using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(SupplementaryCode))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Analyzer suggests BaseSupplementaryCode.Loader, which is less readible")]
	class SupplementaryCodeTest : Customs.Business.Testing.CusCodeDataWithOrderAbstractTest<SupplementaryCode>
	{
		[ExpectNoExceptions]
		public void TestCY_CodeMaxLength()
		{
			var code = Loader.LoadOrCreate<SupplementaryCode, CusClassification>(classification, 1);
			NUnit.Framework.Assert.That(code.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(15));
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreate()
		{
			var loader = Loader;
			var code = loader.LoadOrCreate<SupplementaryCode, CusClassification>(classification, 1);
			NUnit.Framework.Assert.That(code, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.EU.Business.SupplementaryCode)), "Code must be created - should not be [null]");
			code.CY_Code = "SUP";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var classificationViaNewFactory = newFactory.Load<CusClassification>(classification.PK);
			var codeViaNewFactory = loader.LoadOrCreate<SupplementaryCode, CusClassification>(classificationViaNewFactory, 1);
			NUnit.Framework.Assert.That(codeViaNewFactory.PK, NUnit.Framework.Is.EqualTo(code.PK), "Code must be loaded");
		}

		[ExpectNoExceptions]
		public void TestAutoDeleteWhenEmpty()
		{
			var code = Loader.LoadOrCreate<SupplementaryCode, CusClassification>(classification, 1);
			code.CY_Code = "SUP";
			code.CY_Data = "HOME";
			code.CY_Date = new CargoWise.Types.ZDateTime(2017, 9, 1);
			Factory.Save();
			NUnit.Framework.Assert.That(code.IsDeleted, NUnit.Framework.Is.EqualTo(false));

			code.CY_Code = CargoWise.Types.ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(code.IsDeleted, NUnit.Framework.Is.EqualTo(false));

			code.CY_Data = CargoWise.Types.ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(code.IsDeleted, NUnit.Framework.Is.EqualTo(false));

			code.CY_Date = CargoWise.Types.ZDateTime.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(code.IsDeleted, NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var code = Loader.LoadOrCreate<SupplementaryCode, CusClassification>(classification, 1);
			NUnit.Framework.Assert.That(code.Validation, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Business.BaseSupplementaryCodeValidation)), "Validation should never be null - should not be [null]");

			var supplementaryCodeWithNoParents = Factory.New<SupplementaryCode>();
			NUnit.Framework.Assert.That(supplementaryCodeWithNoParents.Validation, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Business.BaseSupplementaryCodeValidation)), "Validation should never be null - should not be [null]");
		}

		[ExpectNoExceptions]
		public void TestProvider()
		{
			var code = Loader.LoadOrCreate<SupplementaryCode, CusClassification>(classification, 1);
			NUnit.Framework.Assert.That(code.Provider, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Business.BaseSupplementaryCodeProvider)), "Provider - should not be [null]");

			var codeWithNoParent = Factory.New<SupplementaryCode>();
			NUnit.Framework.Assert.That(codeWithNoParent.Provider, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Business.BaseSupplementaryCodeProvider)), "Even though SupplementaryCode has not parent, Provider should never be null - should not be [null]");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
		}

		CusClassification classification;

		protected override IEnumerable<SupplementaryCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode1 = "AAAA";
			var codeLoader = new SupplementaryCode.Loader(factory);
			yield return codeLoader.LoadOrCreate<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 1);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var code = factory.NewWithValidTestData<SupplementaryCode>();
			code.CY_Code = SupplementaryCode.CodeDataType;
			return code;
		}

		protected override string ExpectedCusCodeDataType => CusCodeDataTypeList.Codes.SupplementaryCode;

		#endregion

		SupplementaryCode.Loader Loader => loader ?? new SupplementaryCode.Loader(Factory);
		readonly SupplementaryCode.Loader loader;
	}
}
