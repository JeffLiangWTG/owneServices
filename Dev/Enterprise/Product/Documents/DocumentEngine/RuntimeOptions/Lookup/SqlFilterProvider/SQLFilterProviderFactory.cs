using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal static class SQLFilterProviderFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static SQLFilterProvider CreateSQLFilterProvider(FilterFieldValueSerialisable masterFilter, LookupFilterFieldBase detailFilter, string relationType)
		{
			SQLFilterProvider result = null;
			switch (relationType.Trim().ToLower())
			{
				case "supplierpartowner":
					result = new SupplierPartOwnerFilterProvider(masterFilter as LookupFilterFieldBase, detailFilter);
					break;

				case "whsareaowner":
					result = new WhsAreaFilterProvider(masterFilter as LookupFilterFieldBase, detailFilter);
					break;

				case "gllocalaccountcountry":
					result = new GLLocalAccCountryFilterProvider(masterFilter as LookupFilterFieldBase, detailFilter);
					break;
				default:
					throw new TemplateDefinitionException(string.Format("Relation type [{0}] is not defined in the system.", relationType), CellReference.UnKnown);
			}
			return result;
		}
	}
}
