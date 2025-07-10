
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class DeferredB3SendActionListOverride : DeferredB3SendActionList
	{
		public new class Codes : DeferredB3SendActionList.Codes
		{
			public const string RegistryDefault = "DEF";
		}

		public new class Descriptions : DeferredB3SendActionList.Descriptions
		{
			public static MultilingualString RegistryDefault { get { return ResString.GetMultilingualString("DeferredB3SendActionListOverride|RegistryDefault", "Use Registry Default"); } }
		}

		public DeferredB3SendActionListOverride()
			: base()
		{
			AddPair(Codes.RegistryDefault, Descriptions.RegistryDefault);
		}
	}
}
