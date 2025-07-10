using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionImportCollectionInfoTest : TestCaseWithFactory
	{
		public void TestAddPropertiesForImport_ForGlobal()
		{
			var collection = new CommissionFlattenedCollection(Factory);
			var impl = new CommissionImportCollectionInfo(collection, false);
			var properties = impl.Cast<ImportPropertyInfoImpl<CommissionFlattened>>().ToArray();

			AssertArrayEqualsByElements(
				new[]
				{
					CommissionFlattened.Schema.CompanyCode,
					CommissionFlattened.Schema.GroupingSourceCode,
					CommissionFlattened.Schema.InvoiceType,
					CommissionFlattened.Schema.StaffCode,
					CommissionFlattened.Schema.PartyCode,
					CommissionFlattened.Schema.CH0_CommissionDate,
					CommissionFlattened.Schema.CL0_CommissionType,
					CommissionFlattened.Schema.CommissionCurrencyCode,
					CommissionFlattened.Schema.EntityCommissionAmount,
				},
				properties.Select(x => x.MappingName).ToArray());
		}

		public void TestAddPropertiesForImport_ForCurrentCompanyOnly()
		{
			var collection = new CommissionFlattenedCollection(Factory);
			var impl = new CommissionImportCollectionInfo(collection, true);
			var properties = impl.Cast<ImportPropertyInfoImpl<CommissionFlattened>>().ToArray();

			AssertArrayEqualsByElements(
				new[]
				{
					CommissionFlattened.Schema.GroupingSourceCode,
					CommissionFlattened.Schema.InvoiceType,
					CommissionFlattened.Schema.StaffCode,
					CommissionFlattened.Schema.PartyCode,
					CommissionFlattened.Schema.CH0_CommissionDate,
					CommissionFlattened.Schema.CL0_CommissionType,
					CommissionFlattened.Schema.CommissionCurrencyCode,
					CommissionFlattened.Schema.EntityCommissionAmount,
				},
				properties.Select(x => x.MappingName).ToArray());
		}
	}
}
