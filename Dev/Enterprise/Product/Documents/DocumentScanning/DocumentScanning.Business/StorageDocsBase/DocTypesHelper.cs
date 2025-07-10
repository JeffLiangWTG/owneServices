using System.Collections.Immutable;

namespace Enterprise.DocumentScanning.Business
{
	public static class DocTypesHelper
	{
		public static bool IsCustomisableDocType(string docType) => CustomisableDocTypes.Contains(docType);

		static readonly ImmutableArray<string> CustomisableDocTypes = ImmutableArray.Create(
			Core.Constants.RefDocTypes.MiscellaneousDocument,
			Core.Constants.RefDocTypes.Invoice);
	}
}
