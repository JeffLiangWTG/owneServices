using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class HVLVArchivalFileFormats : CodeDescriptionPairList
	{
		public HVLVArchivalFileFormats()
		{
			AddPair(Constants.FileFormats.CSV, Descriptions.CSV);
			AddPair(Constants.FileFormats.XML, Descriptions.XML);
		}

		public static class Descriptions
		{
			public static string CSV => ResString.GetMultilingualString("190a2bec-ec74-4736-8a84-78a116f2c053", "Comma Separated Values");
			public static string XML => ResString.GetMultilingualString("172c68e4-dd35-4029-bb7e-16cafa272cc1", "Extensible Markup Language");
		}

		public CodeDescriptionPairListProvider GetCodeDescriptionPairListProvider()
		{
			return new CodeDescriptionPairListProvider(() => { return this; });
		}
	}
}
