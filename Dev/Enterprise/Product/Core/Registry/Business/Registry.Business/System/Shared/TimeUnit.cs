using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class TimeUnit
	{
		public static readonly ZGuid Week = new ZGuid("0a4b8846-125b-44e4-bee0-2321c6a31ba9");
		public static readonly ZGuid Month = new ZGuid("b0dbe79c-cc75-4d7f-83d2-aef8e227cbef");
		public static readonly ZGuid Year = new ZGuid("4c3c094a-9bf2-48c4-931e-69f2cc0cba9a");

		internal static readonly ResourceString WeekDescription = ResString.GetMultilingualString("bbd8a76d-95f3-4746-8e20-20416e9e83bf", "Week");
		internal static readonly ResourceString MonthDescription = ResString.GetMultilingualString("2709c682-e281-4a3e-8a7f-42683a0da4df", "Month");
		internal static readonly ResourceString YearDescription = ResString.GetMultilingualString("c244d162-c910-49ff-a627-f509115d7d95", "Year");
	}
}
