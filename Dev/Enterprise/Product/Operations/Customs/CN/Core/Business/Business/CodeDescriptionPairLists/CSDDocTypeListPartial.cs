using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public partial class CSDDocTypeList
	{
		static readonly ImmutableArray<string> CSDDocTypeCodesImport = ImmutableArray.Create(
			Codes._00000001, Codes._00000002, Codes._00000003, Codes._00000004, Codes._00000008, Codes._00000009, Codes._00000010,
			Codes._00000011, Codes._00000012, Codes._00000013, Codes._00000014, Codes._00000015, Codes._00000016, Codes._10000001,
			Codes._10000002, Codes._10000003, Codes._10000004, Codes._20000011, Codes._20000012, Codes._20000013, Codes._20000014,
			Codes._20000015, Codes._20000016, Codes._20000017, Codes._20000018, Codes._20000019, Codes._20000020, Codes._20000021,
			Codes._20000022, Codes._20000023, Codes._20000025, Codes._20000026, Codes._20000028, Codes._20000029, Codes._20000030,
			Codes._50000001, Codes._50000002, Codes._50000003, Codes._50000004, Codes._50000005, Codes._50000006, Codes._50000007,
			Codes._50000008, Codes._50000009, Codes._50000010, Codes._50000011, Codes._60000001, Codes._60000007, Codes._60000008,
			Codes._60000009, Codes._60000011, Codes._80000001, Codes._80000002, Codes._80000003, Codes._80000004, Codes._00000017);

		static readonly ImmutableArray<string> CSDDocTypeCodesExport = ImmutableArray.Create(
			Codes._00000001, Codes._00000002, Codes._00000003, Codes._00000004, Codes._00000008, Codes._00000009, Codes._00000010,
			Codes._00000015, Codes._10000001, Codes._10000002, Codes._10000003, Codes._10000004, Codes._20000011, Codes._20000012,
			Codes._20000013, Codes._20000015, Codes._20000017, Codes._20000018, Codes._20000019, Codes._20000020, Codes._20000022,
			Codes._20000023, Codes._20000024, Codes._20000025, Codes._20000026, Codes._20000027, Codes._20000028, Codes._20000029,
			Codes._20000030, Codes._50000001, Codes._50000002, Codes._50000003, Codes._50000004, Codes._50000005, Codes._50000007,
			Codes._50000008, Codes._50000010, Codes._50000011, Codes._50000012, Codes._50000013, Codes._50000014, Codes._60000002,
			Codes._60000003, Codes._60000004, Codes._60000005, Codes._60000006, Codes._60000007, Codes._60000008, Codes._60000009,
			Codes._60000010, Codes._60000011);

		public static CodeDescriptionPairList GetCachedCSDDocTypeList(BusinessObjectFactory factory, bool isImport)
		{
			return factory.GetCachedValue("CN_CSDDocTypeList_" + isImport.ToString(), () =>
			{
				return factory.GetCachedValue<CSDDocTypeList>().FilterListByCodes(isImport ? CSDDocTypeCodesImport : CSDDocTypeCodesExport);
			});
		}

		public static bool CanLinkToInvoiceLine(string attachmentType) => DangerousGoodsSet.Value.Contains(attachmentType);

		static Lazy<HashSet<string>> DangerousGoodsSet => new Lazy<HashSet<string>>(() => new HashSet<string> { Codes._80000001, Codes._80000002, Codes._80000003, Codes._80000004 });

		public static IEnumerable<string> GetRequiredAttachmentTypesByCargoAttribute(string cargoAttribute) => cargoAttribute switch
		{
			CargoAttributeList.Codes._31 => new[] { Codes._80000001, Codes._80000004 },
			CargoAttributeList.Codes._32 => new[] { Codes._80000001, Codes._80000003, Codes._80000004 },
			CargoAttributeList.Codes._33 => Array.Empty<string>(),
			_ => Array.Empty<string>(),
		};
	}
}
