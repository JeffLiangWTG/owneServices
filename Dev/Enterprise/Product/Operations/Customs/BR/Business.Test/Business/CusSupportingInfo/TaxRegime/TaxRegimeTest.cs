using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(TaxRegime))]
	class TaxRegimeTest : Customs.Business.Testing.CusSupportingInfoTest<TaxRegime>
	{
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<TaxRegime>();
			CombineAssertions(() =>
			{
				AssertEquals(CusSupportingInfoTypeList.Codes.TaxRegime, supporting.CSI_Type);
				AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
			});
		}

		public void TestDeleteWhenUsedFieldsAreEmtpy()
		{
			var taxRegime = GetNewBusinessObject() as TaxRegime;

			Factory.Save();
			AssertEquals("TaxRegime should not be saved when it is empty", false, taxRegime.IsInDatabase);

			taxRegime.CSI_Code = "1";
			Factory.Save();
			AssertEquals("TaxRegime should be saved when it is not empty", true, taxRegime.IsInDatabase);

			taxRegime.CSI_Code = "";
			taxRegime.CSI_Procedure = "1";
			Factory.Save();
			AssertEquals("TaxRegime should be saved when it is not empty", true, taxRegime.IsInDatabase);

			taxRegime.CSI_Procedure = "";
			Factory.Save();
			AssertEquals("TaxRegime should be delete when it is empty", true, taxRegime.IsDeleted);
		}

		protected override IEnumerable<TaxRegime> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var taxRegime = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().TaxRegimeCollection.AddNew();
			taxRegime.CSI_Code = "1";
			yield return taxRegime;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			return invoiceLine.TaxRegimeCollection.AddNew();
		}
	}
}
