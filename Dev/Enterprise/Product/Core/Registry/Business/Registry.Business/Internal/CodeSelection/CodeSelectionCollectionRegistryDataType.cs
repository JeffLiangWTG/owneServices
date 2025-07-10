using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.CodeSelectionCollectionRegistryItemEditor, Enterprise.Registry.GUI")]
	class CodeSelectionCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeSelectionCollection>
	{
		internal readonly CodeDescriptionPairListProvider codesProvider;

		public CodeSelectionCollectionRegistryDataType(CodeDescriptionPairListProvider codesProvider)
			: base(new CodeSelectionCollection(codesProvider))
		{
			this.codesProvider = codesProvider;
		}

		protected override CodeSelectionCollection DeserialiseCore(byte[] value)
		{
			CodeSelectionCollection result = base.DeserialiseCore(value);
			result.SetCodesProvider(codesProvider);
			return result;
		}
	}
}
