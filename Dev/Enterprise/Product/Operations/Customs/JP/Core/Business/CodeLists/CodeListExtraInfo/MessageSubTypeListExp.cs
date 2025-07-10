using System.Collections.Immutable;

namespace Enterprise.Customs.JP.Business
{
	public partial class JPExportDeclarationTypeList
	{
		public static bool IsNACCSCodeNullable(string decType, string hvlvType)
		{
			return (NACCSCodeNullableItemValue.Contains(decType) && hvlvType == ValueTypeList.Codes.S) || decType == Codes.G;
		}

		static readonly ImmutableHashSet<string> NACCSCodeNullableItemValue = ImmutableHashSet.Create(new[]
		{
			Codes.E,
			Codes.N,
			Codes.M,
			Codes.R,
			Codes.T
		});
	}
}
