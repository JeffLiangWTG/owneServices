using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	public class MessagePartyPairList : CodeDescriptionPairList
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "This class is being inherited.")]
		public class Codes
		{
			public const string AATAppletonDock = "AATAD";
			public const string AATBrisbane = "AATFI";
			public const string AATPortKembla = "AATPK";
			public const string BrisbaneContainerTerminal = "HPAFI";
			public const string CSXAdelaide = "CXSXAD";
			public const string DPWorldBrisbane = "DPBNE";
			public const string DPWorldWestSwansonIntermodal = "WSIT";
			public const string LinxAucklandPoint = "LNXAP";
			public const string LinxEastArmWharfDarwin = "LNXDW";
			public const string LinxGeelongPort = "LNXGL";
			public const string LinxPortAlma = "LNXPA";
			public const string PandODarwin = "CONDW";
			public const string PandOFreemantle = "CONFR";
			public const string PandOMelbourneWestSwanson = "CONWS";
			public const string PandOSydneyPortBotany = "CTLPB";
			public const string PandOTasmaniaBellBay = "CONBE";
			public const string PatrickBrisbaneFishermanIsland7 = "PTFIT";
			public const string PatrickFreemantle = "ASLFR";
			public const string PatrickMelbourneEastSwanson = "ASES1";
			public const string PatrickSydneyPortBotany = "ASLPB";
			public const string SydneyInternationalContainerTerminal = "HPAPB";
			public const string PortOfNapier = "NPENZ";
			public const string PortOfHedland = "QBPHD";
			public const string VictoriaInternationalContainerTerminal = "VICTM";
			public const string QubePortsEsperance = "QBESP";
			public const string LinxEsperance = "LNXEP";
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "This class is being inherited.")]
		public class Descriptions
		{
			public static MultilingualString AATAppletonDock { get { return ResString.GetMultilingualString("MessagePartyPairList|AATAppletonDock", "AAT - Appleton Dock"); } }
			public static MultilingualString AATBrisbane { get { return ResString.GetMultilingualString("MessagePartyPairList|AATBrisbane", "AAT - Brisbane"); } }
			public static MultilingualString AATPortKembla { get { return ResString.GetMultilingualString("MessagePartyPairList|AATPortKembla", "AAT - Port Kembla"); } }
			public static MultilingualString BrisbaneContainerTerminal { get { return ResString.GetMultilingualString("MessagePartyPairList|BrisbaneContainerTerminal", "Brisbane Container Terminal - Fisherman Island"); } }
			public static MultilingualString CSXAdelaide { get { return ResString.GetMultilingualString("MessagePartyPairList|CSXAdelaide", "CSX - Adelaide"); } }
			public static MultilingualString DPWorldBrisbane { get { return ResString.GetMultilingualString("MessagePartyPairList|DPWorldBrisbane", "DP World Brisbane"); } }
			public static MultilingualString DPWorldWestSwansonIntermodal { get { return ResString.GetMultilingualString("MessagePartyPairList|DPWorldWestSwansonIntermodal", "DP World West Swanson Intermodal"); } }
			public static MultilingualString LinxAucklandPoint { get { return ResString.GetMultilingualString("MessagePartyPairList|LinxAucklandPoint", "Linx Auckland Point"); } }
			public static MultilingualString LinxEastArmWharfDarwin { get { return ResString.GetMultilingualString("MessagePartyPairList|LinxEastArmWharfDarwin", "Linx East Arm Wharf Darwin"); } }
			public static MultilingualString LinxGeelongPort { get { return ResString.GetMultilingualString("MessagePartyPairList|LinxGeelongPort", "Linx Geelong Port"); } }
			public static MultilingualString LinxPortAlma { get { return ResString.GetMultilingualString("MessagePartyPairList|LinxPortAlma", "Linx Port Alma"); } }
			public static MultilingualString PandODarwin { get { return ResString.GetMultilingualString("MessagePartyPairList|PandODarwin", "P&O Ports Darwin"); } }
			public static MultilingualString PandOFreemantle { get { return ResString.GetMultilingualString("MessagePartyPairList|PandOFreemantle", "P&O Ports Fremantle"); } }
			public static MultilingualString PandOMelbourneWestSwanson { get { return ResString.GetMultilingualString("MessagePartyPairList|PandOMelbourneWestSwanson", "P&O Ports Melbourne - West Swanson"); } }
			public static MultilingualString PandOSydneyPortBotany { get { return ResString.GetMultilingualString("MessagePartyPairList|PandOSydneyPortBotany", "P&O Ports Sydney - Port Botany"); } }
			public static MultilingualString PandOTasmaniaBellBay { get { return ResString.GetMultilingualString("MessagePartyPairList|PandOTasmaniaBellBay", "P&O Ports Tasmania - Bell Bay"); } }
			public static MultilingualString PatrickBrisbaneFishermanIsland7 { get { return ResString.GetMultilingualString("MessagePartyPairList|PatrickBrisbaneFishermanIsland7", "Patrick Brisbane - Fisherman Island 7"); } }
			public static MultilingualString PatrickFreemantle { get { return ResString.GetMultilingualString("MessagePartyPairList|PatrickFreemantle", "Patrick Fremantle"); } }
			public static MultilingualString PatrickMelbourneEastSwanson { get { return ResString.GetMultilingualString("MessagePartyPairList|PatrickMelbourneEastSwanson", "Patrick Melbourne - East Swanson"); } }
			public static MultilingualString PatrickSydneyPortBotany { get { return ResString.GetMultilingualString("MessagePartyPairList|PatrickSydneyPortBotany", "Patrick Sydney - Port Botany"); } }
			public static MultilingualString SydneyInternationalContainerTerminal { get { return ResString.GetMultilingualString("MessagePartyPairList|SydneyInternationalContainerTerminal", "Sydney International Container Terminal - Port Botany"); } }
			public static MultilingualString PortOfNapier { get { return ResString.GetMultilingualString("MessagePartyPairList|PortOfNapier", "Port of Napier"); } }
			public static MultilingualString PortOfHedland { get { return ResString.GetMultilingualString("MessagePartyPairList|PortOfHedland", "Qube Port Hedland"); } }
			public static MultilingualString VictoriaInternationalContainerTerminal { get { return ResString.GetMultilingualString("MessagePartyPairList|VictoriaInternationalContainerTerminal", "Victoria International Container Terminal"); } }
			public static MultilingualString QubePortsEsperance { get { return ResString.GetMultilingualString("MessagePartyPairList|QubePortsEsperance", "Qube Ports Esperance"); } }
			public static MultilingualString LinxEsperance { get { return ResString.GetMultilingualString("MessagePartyPairList|LinxEsperance", "Linx Esperance"); } }
		}

		public MessagePartyPairList()
		{
			AddPair(Codes.AATAppletonDock, Descriptions.AATAppletonDock);
			AddPair(Codes.AATBrisbane, Descriptions.AATBrisbane);
			AddPair(Codes.AATPortKembla, Descriptions.AATPortKembla);
			AddPair(Codes.BrisbaneContainerTerminal, Descriptions.BrisbaneContainerTerminal);
			AddPair(Codes.CSXAdelaide, Descriptions.CSXAdelaide);
			AddPair(Codes.DPWorldBrisbane, Descriptions.DPWorldBrisbane);
			AddPair(Codes.DPWorldWestSwansonIntermodal, Descriptions.DPWorldWestSwansonIntermodal);
			AddPair(Codes.LinxAucklandPoint, Descriptions.LinxAucklandPoint);
			AddPair(Codes.LinxEastArmWharfDarwin, Descriptions.LinxEastArmWharfDarwin);
			AddPair(Codes.LinxGeelongPort, Descriptions.LinxGeelongPort);
			AddPair(Codes.LinxPortAlma, Descriptions.LinxPortAlma);
			AddPair(Codes.PandODarwin, Descriptions.PandODarwin);
			AddPair(Codes.PandOFreemantle, Descriptions.PandOFreemantle);
			AddPair(Codes.PandOMelbourneWestSwanson, Descriptions.PandOMelbourneWestSwanson);
			AddPair(Codes.PandOSydneyPortBotany, Descriptions.PandOSydneyPortBotany);
			AddPair(Codes.PandOTasmaniaBellBay, Descriptions.PandOTasmaniaBellBay);
			AddPair(Codes.PatrickBrisbaneFishermanIsland7, Descriptions.PatrickBrisbaneFishermanIsland7);
			AddPair(Codes.PatrickFreemantle, Descriptions.PatrickFreemantle);
			AddPair(Codes.PatrickMelbourneEastSwanson, Descriptions.PatrickMelbourneEastSwanson);
			AddPair(Codes.PatrickSydneyPortBotany, Descriptions.PatrickSydneyPortBotany);
			AddPair(Codes.PortOfNapier, Descriptions.PortOfNapier);
			AddPair(Codes.PortOfHedland, Descriptions.PortOfHedland);
			AddPair(Codes.SydneyInternationalContainerTerminal, Descriptions.SydneyInternationalContainerTerminal);
			AddPair(Codes.VictoriaInternationalContainerTerminal, Descriptions.VictoriaInternationalContainerTerminal);
			AddPair(Codes.QubePortsEsperance, Descriptions.QubePortsEsperance);
			AddPair(Codes.LinxEsperance, Descriptions.LinxEsperance);
		}
	}
}


