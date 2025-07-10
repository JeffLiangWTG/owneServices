namespace Enterprise.ZArchitecture.Core;

public class DotNetBuildVersionTargetTypeList : CodeDescriptionPairList
{
	public DotNetBuildVersionTargetTypeList()
	{
		AddPair(Codes.NetFramework48, Descriptions.NetFramework48);
		AddPair(Codes.NetCore8, Descriptions.NetCore8);
		DefaultCode = DefaultVersion;
	}

	internal const string DefaultVersion = Codes.NetFramework48;

	public static class Codes
	{
		public const string NetFramework48 = "NETFRAMEWORK4_8";
		public const string NetCore8 = "NETCORE8";
	}

	public static class Descriptions
	{
		public static MultilingualString NetFramework48 => SourceGenerated.ResString.GetMultilingualString("NetFrameworkBuildTargetTypeList|NetFramework48", ".NET Framework 4.8");
		public static MultilingualString NetCore8 => SourceGenerated.ResString.GetMultilingualString("NetFrameworkBuildTargetTypeList|NetCore8", ".NET Core 8");
	}
}
