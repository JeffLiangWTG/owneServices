using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CASSFileImportDefaultTaxIDRegistryDataType))]
	class CASSFileImportDefaultTaxIDRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CASSFileImportDefaultTaxIDRegistryDataType>
	{
		protected override CASSFileImportDefaultTaxIDRegistryDataType GetNewDataType()
		{
			return new CASSFileImportDefaultTaxIDRegistryDataType();
		}

		protected override string ExpectedEditorName => "CASSFileImportDefaultTaxIDRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();

			var companyPK = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.Equal, "SIN")).PK.ToGuid();
			CASSFileImportDefaultTaxID result = new CASSFileImportDefaultTaxID();
			result.StandardRatedTaxID = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainGSTTaxRegistryID, companyPK);
			result.ZeroRatedTaxID = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainFreeGSTTaxRegistryID, companyPK);

			var companyPK2 = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.Equal, "EDI")).PK.ToGuid();
			CASSFileImportDefaultTaxID result2 = new CASSFileImportDefaultTaxID();
			result2.StandardRatedTaxID = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainGSTTaxRegistryID, companyPK2);
			result2.ZeroRatedTaxID = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainFreeGSTTaxRegistryID, companyPK2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new CASSFileImportDefaultTaxIDRegistryDataType().Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new CASSFileImportDefaultTaxIDRegistryDataType().Serialise(result2))
			};
		}
	}
}
