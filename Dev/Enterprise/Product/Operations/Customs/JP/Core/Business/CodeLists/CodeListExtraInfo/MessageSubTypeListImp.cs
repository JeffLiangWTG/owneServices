using System.Collections.Immutable;

namespace Enterprise.Customs.JP.Business
{
	public partial class JPImportDeclarationTypeList
	{
		public static bool IsNACCSCodeNullable(string decType) => GoodsAttributeNullableItemValue.Contains(decType);

		static readonly ImmutableHashSet<string> GoodsAttributeNullableItemValue = ImmutableHashSet.Create(new[]
		{
			Codes.H,
			Codes.N,
			Codes.Y
		});
	}
}
