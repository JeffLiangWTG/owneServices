using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Enterprise.Customs.CN.Business
{
	public partial class CargoAttributeList : IGroupedCodeDescriptionPairList
	{
		static readonly ImmutableArray<string[]> CargoAttributesGroup = ImmutableArray.Create(
			new[] { Codes._11, Codes._12, Codes._13 },
			new[] { Codes._14, Codes._15 },
			new[] { Codes._16, Codes._17 },
			new[] { Codes._18, Codes._19, Codes._20, Codes._21, Codes._22 },
			new[] { Codes._23, Codes._24 },
			new[] { Codes._25, Codes._26, Codes._27, Codes._28, Codes._29 },
			new[] { Codes._30 },
			DangerousGoodsAttributes,
			new[] { Codes._34, Codes._35, Codes._36, Codes._37, Codes._38 },
			new[] { Codes._39, Codes._40 },
			new[] { Codes._41 },
			new[] { Codes._42, Codes._43, Codes._44 },
			new[] { Codes._46 }
		);

		IEnumerable<string> IGroupedCodeDescriptionPairList.GetMutuallyExclusiveCodes(string code)
		{
			return CargoAttributesGroup.FirstOrDefault(group => group.Contains(code))?.Where(x => x != code) ?? Enumerable.Empty<string>();
		}

		bool IGroupedCodeDescriptionPairList.AutoUnselectExclusiveCodes => true;

		public static string[] DangerousGoodsAttributes => new[] { Codes._31, Codes._32, Codes._33 };

		public static bool IsDangerousGoodsAttribute(string code) => DangerousGoodsAttributes.Contains(code);

		public static bool CanLinkToAttachment(string code) => code == Codes._31 || code == Codes._32;
	}
}
