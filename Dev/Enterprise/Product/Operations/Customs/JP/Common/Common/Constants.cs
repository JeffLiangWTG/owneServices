using System.Collections.Immutable;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public static class Constants
	{
		public static class UniversalReferenceConstants
		{
			public static class RefCusCodeList
			{
				public static class CodeTypes
				{
					public const string NACCSResultCode = "NRC";
				}
			}
		}

		public static class DocumentMessageCodes
		{
			public static class ShipmentTypes
			{
				public static readonly string Import = "I";
				public static readonly string Export = "E";
			}

			public static readonly ImmutableArray<string> ImportPermitMessageCodes = new ImportClearanceNotice().OutputInformationCodes.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_NA_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1AG2,
				JPOutputInformationCodeList.Codes.SAX1AG2,
				JPOutputInformationCodeList.Codes.SAD2AG2,
				JPOutputInformationCodeList.Codes.SAX2AG2,
				JPOutputInformationCodeList.Codes.SAD1GG2,
				JPOutputInformationCodeList.Codes.SAX1GG2,
				JPOutputInformationCodeList.Codes.SAD2GG2,
				JPOutputInformationCodeList.Codes.SAX2GG2,
				JPOutputInformationCodeList.Codes.SAD1KG2,
				JPOutputInformationCodeList.Codes.SAX1KG2,
				JPOutputInformationCodeList.Codes.SAD2KG2,
				JPOutputInformationCodeList.Codes.SAX2KG2,
				JPOutputInformationCodeList.Codes.SAD1NG2,
				JPOutputInformationCodeList.Codes.SAX1NG2,
				JPOutputInformationCodeList.Codes.SAD2NG2,
				JPOutputInformationCodeList.Codes.SAX2NG2,
				JPOutputInformationCodeList.Codes.AAD1AG2,
				JPOutputInformationCodeList.Codes.AAX1AG2,
				JPOutputInformationCodeList.Codes.AAD2AG2,
				JPOutputInformationCodeList.Codes.AAX2AG2,
				JPOutputInformationCodeList.Codes.AAD1GG2,
				JPOutputInformationCodeList.Codes.AAX1GG2,
				JPOutputInformationCodeList.Codes.AAD2GG2,
				JPOutputInformationCodeList.Codes.AAX2GG2,
				JPOutputInformationCodeList.Codes.AAD1KG2,
				JPOutputInformationCodeList.Codes.AAX1KG2,
				JPOutputInformationCodeList.Codes.AAD2KG2,
				JPOutputInformationCodeList.Codes.AAX2KG2,
				JPOutputInformationCodeList.Codes.AAD1NG2,
				JPOutputInformationCodeList.Codes.AAX1NG2,
				JPOutputInformationCodeList.Codes.AAD2NG2,
				JPOutputInformationCodeList.Codes.AAX2NG2,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_NB_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1BG2,
				JPOutputInformationCodeList.Codes.SAX1BG2,
				JPOutputInformationCodeList.Codes.SAD2BG2,
				JPOutputInformationCodeList.Codes.SAX2BG2,
				JPOutputInformationCodeList.Codes.SAD1HG2,
				JPOutputInformationCodeList.Codes.SAX1HG2,
				JPOutputInformationCodeList.Codes.SAD2HG2,
				JPOutputInformationCodeList.Codes.SAX2HG2,
				JPOutputInformationCodeList.Codes.SAD1LG2,
				JPOutputInformationCodeList.Codes.SAX1LG2,
				JPOutputInformationCodeList.Codes.SAD2LG2,
				JPOutputInformationCodeList.Codes.SAX2LG2,
				JPOutputInformationCodeList.Codes.SAD1PG2,
				JPOutputInformationCodeList.Codes.SAX1PG2,
				JPOutputInformationCodeList.Codes.SAD2PG2,
				JPOutputInformationCodeList.Codes.SAX2PG2,
				JPOutputInformationCodeList.Codes.AAD1BG2,
				JPOutputInformationCodeList.Codes.AAX1BG2,
				JPOutputInformationCodeList.Codes.AAD2BG2,
				JPOutputInformationCodeList.Codes.AAX2BG2,
				JPOutputInformationCodeList.Codes.AAD1HG2,
				JPOutputInformationCodeList.Codes.AAX1HG2,
				JPOutputInformationCodeList.Codes.AAD2HG2,
				JPOutputInformationCodeList.Codes.AAX2HG2,
				JPOutputInformationCodeList.Codes.AAD1LG2,
				JPOutputInformationCodeList.Codes.AAX1LG2,
				JPOutputInformationCodeList.Codes.AAD2LG2,
				JPOutputInformationCodeList.Codes.AAX2LG2,
				JPOutputInformationCodeList.Codes.AAD1PG2,
				JPOutputInformationCodeList.Codes.AAX1PG2,
				JPOutputInformationCodeList.Codes.AAD2PG2,
				JPOutputInformationCodeList.Codes.AAX2PG2,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_NC_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1CG2,
				JPOutputInformationCodeList.Codes.SAX1CG2,
				JPOutputInformationCodeList.Codes.SAD2CG2,
				JPOutputInformationCodeList.Codes.SAX2CG2,
				JPOutputInformationCodeList.Codes.SAD1JG2,
				JPOutputInformationCodeList.Codes.SAX1JG2,
				JPOutputInformationCodeList.Codes.SAD2JG2,
				JPOutputInformationCodeList.Codes.SAX2JG2,
				JPOutputInformationCodeList.Codes.SAD1MG2,
				JPOutputInformationCodeList.Codes.SAX1MG2,
				JPOutputInformationCodeList.Codes.SAD2MG2,
				JPOutputInformationCodeList.Codes.SAX2MG2,
				JPOutputInformationCodeList.Codes.SAD1QG2,
				JPOutputInformationCodeList.Codes.SAX1QG2,
				JPOutputInformationCodeList.Codes.SAD2QG2,
				JPOutputInformationCodeList.Codes.SAX2QG2,
				JPOutputInformationCodeList.Codes.AAD1CG2,
				JPOutputInformationCodeList.Codes.AAX1CG2,
				JPOutputInformationCodeList.Codes.AAD2CG2,
				JPOutputInformationCodeList.Codes.AAX2CG2,
				JPOutputInformationCodeList.Codes.AAD1JG2,
				JPOutputInformationCodeList.Codes.AAX1JG2,
				JPOutputInformationCodeList.Codes.AAD2JG2,
				JPOutputInformationCodeList.Codes.AAX2JG2,
				JPOutputInformationCodeList.Codes.AAD1MG2,
				JPOutputInformationCodeList.Codes.AAX1MG2,
				JPOutputInformationCodeList.Codes.AAD2MG2,
				JPOutputInformationCodeList.Codes.AAX2MG2,
				JPOutputInformationCodeList.Codes.AAD1QG2,
				JPOutputInformationCodeList.Codes.AAX1QG2,
				JPOutputInformationCodeList.Codes.AAD2QG2,
				JPOutputInformationCodeList.Codes.AAX2QG2,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_BPB_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1BJ1,
				JPOutputInformationCodeList.Codes.SAX1BJ1,
				JPOutputInformationCodeList.Codes.SAD2BJ1,
				JPOutputInformationCodeList.Codes.SAX2BJ1,
				JPOutputInformationCodeList.Codes.SAD1HJ1,
				JPOutputInformationCodeList.Codes.SAX1HJ1,
				JPOutputInformationCodeList.Codes.SAD2HJ1,
				JPOutputInformationCodeList.Codes.SAX2HJ1,
				JPOutputInformationCodeList.Codes.SAD1LJ1,
				JPOutputInformationCodeList.Codes.SAX1LJ1,
				JPOutputInformationCodeList.Codes.SAD2LJ1,
				JPOutputInformationCodeList.Codes.SAX2LJ1,
				JPOutputInformationCodeList.Codes.SAD1PJ1,
				JPOutputInformationCodeList.Codes.SAX1PJ1,
				JPOutputInformationCodeList.Codes.SAD2PJ1,
				JPOutputInformationCodeList.Codes.SAX2PJ1,
				JPOutputInformationCodeList.Codes.AAD1BJ1,
				JPOutputInformationCodeList.Codes.AAX1BJ1,
				JPOutputInformationCodeList.Codes.AAD2BJ1,
				JPOutputInformationCodeList.Codes.AAX2BJ1,
				JPOutputInformationCodeList.Codes.AAD1HJ1,
				JPOutputInformationCodeList.Codes.AAX1HJ1,
				JPOutputInformationCodeList.Codes.AAD2HJ1,
				JPOutputInformationCodeList.Codes.AAX2HJ1,
				JPOutputInformationCodeList.Codes.AAD1LJ1,
				JPOutputInformationCodeList.Codes.AAX1LJ1,
				JPOutputInformationCodeList.Codes.AAD2LJ1,
				JPOutputInformationCodeList.Codes.AAX2LJ1,
				JPOutputInformationCodeList.Codes.AAD1PJ1,
				JPOutputInformationCodeList.Codes.AAX1PJ1,
				JPOutputInformationCodeList.Codes.AAD2PJ1,
				JPOutputInformationCodeList.Codes.AAX2PJ1,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_BPC_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1CJ1,
				JPOutputInformationCodeList.Codes.SAX1CJ1,
				JPOutputInformationCodeList.Codes.SAD2CJ1,
				JPOutputInformationCodeList.Codes.SAX2CJ1,
				JPOutputInformationCodeList.Codes.SAD1JJ1,
				JPOutputInformationCodeList.Codes.SAX1JJ1,
				JPOutputInformationCodeList.Codes.SAD2JJ1,
				JPOutputInformationCodeList.Codes.SAX2JJ1,
				JPOutputInformationCodeList.Codes.SAD1MJ1,
				JPOutputInformationCodeList.Codes.SAX1MJ1,
				JPOutputInformationCodeList.Codes.SAD2MJ1,
				JPOutputInformationCodeList.Codes.SAX2MJ1,
				JPOutputInformationCodeList.Codes.SAD1QJ1,
				JPOutputInformationCodeList.Codes.SAX1QJ1,
				JPOutputInformationCodeList.Codes.SAD2QJ1,
				JPOutputInformationCodeList.Codes.SAX2QJ1,
				JPOutputInformationCodeList.Codes.AAD1CJ1,
				JPOutputInformationCodeList.Codes.AAX1CJ1,
				JPOutputInformationCodeList.Codes.AAD2CJ1,
				JPOutputInformationCodeList.Codes.AAX2CJ1,
				JPOutputInformationCodeList.Codes.AAD1JJ1,
				JPOutputInformationCodeList.Codes.AAX1JJ1,
				JPOutputInformationCodeList.Codes.AAD2JJ1,
				JPOutputInformationCodeList.Codes.AAX2JJ1,
				JPOutputInformationCodeList.Codes.AAD1MJ1,
				JPOutputInformationCodeList.Codes.AAX1MJ1,
				JPOutputInformationCodeList.Codes.AAD2MJ1,
				JPOutputInformationCodeList.Codes.AAX2MJ1,
				JPOutputInformationCodeList.Codes.AAD1QJ1,
				JPOutputInformationCodeList.Codes.AAX1QJ1,
				JPOutputInformationCodeList.Codes.AAD2QJ1,
				JPOutputInformationCodeList.Codes.AAX2QJ1,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_ISTA_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1RG1,
				JPOutputInformationCodeList.Codes.SAX1RG1,
				JPOutputInformationCodeList.Codes.SAD2RG1,
				JPOutputInformationCodeList.Codes.SAX2RG1,
				JPOutputInformationCodeList.Codes.SAD1RH1,
				JPOutputInformationCodeList.Codes.SAD2RH1,
				JPOutputInformationCodeList.Codes.SAD1RJ1,
				JPOutputInformationCodeList.Codes.SAX1RJ1,
				JPOutputInformationCodeList.Codes.SAD2RJ1,
				JPOutputInformationCodeList.Codes.SAX2RJ1,
				JPOutputInformationCodeList.Codes.SAD1UG1,
				JPOutputInformationCodeList.Codes.SAX1UG1,
				JPOutputInformationCodeList.Codes.SAD2UG1,
				JPOutputInformationCodeList.Codes.SAX2UG1,
				JPOutputInformationCodeList.Codes.SAD1UH1,
				JPOutputInformationCodeList.Codes.SAD2UH1,
				JPOutputInformationCodeList.Codes.SAD1UJ1,
				JPOutputInformationCodeList.Codes.SAX1UJ1,
				JPOutputInformationCodeList.Codes.SAD2UJ1,
				JPOutputInformationCodeList.Codes.SAX2UJ1,
				JPOutputInformationCodeList.Codes.SAD1XG1,
				JPOutputInformationCodeList.Codes.SAX1XG1,
				JPOutputInformationCodeList.Codes.SAD2XG1,
				JPOutputInformationCodeList.Codes.SAX2XG1,
				JPOutputInformationCodeList.Codes.SAD1XH1,
				JPOutputInformationCodeList.Codes.SAD2XH1,
				JPOutputInformationCodeList.Codes.SAD1XJ1,
				JPOutputInformationCodeList.Codes.SAX1XJ1,
				JPOutputInformationCodeList.Codes.SAD2XJ1,
				JPOutputInformationCodeList.Codes.SAX2XJ1,
				JPOutputInformationCodeList.Codes.SAD11G1,
				JPOutputInformationCodeList.Codes.SAX11G1,
				JPOutputInformationCodeList.Codes.SAD21G1,
				JPOutputInformationCodeList.Codes.SAX21G1,
				JPOutputInformationCodeList.Codes.SAD11H1,
				JPOutputInformationCodeList.Codes.SAD21H1,
				JPOutputInformationCodeList.Codes.SAD11J1,
				JPOutputInformationCodeList.Codes.SAX11J1,
				JPOutputInformationCodeList.Codes.SAD21J1,
				JPOutputInformationCodeList.Codes.SAX21J1,
				JPOutputInformationCodeList.Codes.AAD1RG1,
				JPOutputInformationCodeList.Codes.AAX1RG1,
				JPOutputInformationCodeList.Codes.AAD2RG1,
				JPOutputInformationCodeList.Codes.AAX2RG1,
				JPOutputInformationCodeList.Codes.AAD1RH1,
				JPOutputInformationCodeList.Codes.AAD2RH1,
				JPOutputInformationCodeList.Codes.AAD1RJ1,
				JPOutputInformationCodeList.Codes.AAX1RJ1,
				JPOutputInformationCodeList.Codes.AAD2RJ1,
				JPOutputInformationCodeList.Codes.AAX2RJ1,
				JPOutputInformationCodeList.Codes.AAD1UG1,
				JPOutputInformationCodeList.Codes.AAX1UG1,
				JPOutputInformationCodeList.Codes.AAD2UG1,
				JPOutputInformationCodeList.Codes.AAX2UG1,
				JPOutputInformationCodeList.Codes.AAD1UH1,
				JPOutputInformationCodeList.Codes.AAD2UH1,
				JPOutputInformationCodeList.Codes.AAD1UJ1,
				JPOutputInformationCodeList.Codes.AAX1UJ1,
				JPOutputInformationCodeList.Codes.AAD2UJ1,
				JPOutputInformationCodeList.Codes.AAX2UJ1,
				JPOutputInformationCodeList.Codes.AAD1XG1,
				JPOutputInformationCodeList.Codes.AAX1XG1,
				JPOutputInformationCodeList.Codes.AAD2XG1,
				JPOutputInformationCodeList.Codes.AAX2XG1,
				JPOutputInformationCodeList.Codes.AAD1XH1,
				JPOutputInformationCodeList.Codes.AAD2XH1,
				JPOutputInformationCodeList.Codes.AAD1XJ1,
				JPOutputInformationCodeList.Codes.AAX1XJ1,
				JPOutputInformationCodeList.Codes.AAD2XJ1,
				JPOutputInformationCodeList.Codes.AAX2XJ1,
				JPOutputInformationCodeList.Codes.AAD11G1,
				JPOutputInformationCodeList.Codes.AAX11G1,
				JPOutputInformationCodeList.Codes.AAD21G1,
				JPOutputInformationCodeList.Codes.AAX21G1,
				JPOutputInformationCodeList.Codes.AAD11H1,
				JPOutputInformationCodeList.Codes.AAD21H1,
				JPOutputInformationCodeList.Codes.AAD11J1,
				JPOutputInformationCodeList.Codes.AAX11J1,
				JPOutputInformationCodeList.Codes.AAD21J1,
				JPOutputInformationCodeList.Codes.AAX21J1,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_ISTB_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1SG1,
				JPOutputInformationCodeList.Codes.SAX1SG1,
				JPOutputInformationCodeList.Codes.SAD2SG1,
				JPOutputInformationCodeList.Codes.SAX2SG1,
				JPOutputInformationCodeList.Codes.SAD1SH1,
				JPOutputInformationCodeList.Codes.SAD2SH1,
				JPOutputInformationCodeList.Codes.SAD1SJ1,
				JPOutputInformationCodeList.Codes.SAX1SJ1,
				JPOutputInformationCodeList.Codes.SAD2SJ1,
				JPOutputInformationCodeList.Codes.SAX2SJ1,
				JPOutputInformationCodeList.Codes.SAD1VG1,
				JPOutputInformationCodeList.Codes.SAX1VG1,
				JPOutputInformationCodeList.Codes.SAD2VG1,
				JPOutputInformationCodeList.Codes.SAX2VG1,
				JPOutputInformationCodeList.Codes.SAD1VH1,
				JPOutputInformationCodeList.Codes.SAD2VH1,
				JPOutputInformationCodeList.Codes.SAD1VJ1,
				JPOutputInformationCodeList.Codes.SAX1VJ1,
				JPOutputInformationCodeList.Codes.SAD2VJ1,
				JPOutputInformationCodeList.Codes.SAX2VJ1,
				JPOutputInformationCodeList.Codes.SAD1YG1,
				JPOutputInformationCodeList.Codes.SAX1YG1,
				JPOutputInformationCodeList.Codes.SAD2YG1,
				JPOutputInformationCodeList.Codes.SAX2YG1,
				JPOutputInformationCodeList.Codes.SAD1YH1,
				JPOutputInformationCodeList.Codes.SAD2YH1,
				JPOutputInformationCodeList.Codes.SAD1YJ1,
				JPOutputInformationCodeList.Codes.SAX1YJ1,
				JPOutputInformationCodeList.Codes.SAD2YJ1,
				JPOutputInformationCodeList.Codes.SAX2YJ1,
				JPOutputInformationCodeList.Codes.SAD12G1,
				JPOutputInformationCodeList.Codes.SAX12G1,
				JPOutputInformationCodeList.Codes.SAD22G1,
				JPOutputInformationCodeList.Codes.SAX22G1,
				JPOutputInformationCodeList.Codes.SAD12H1,
				JPOutputInformationCodeList.Codes.SAD22H1,
				JPOutputInformationCodeList.Codes.SAD12J1,
				JPOutputInformationCodeList.Codes.SAX12J1,
				JPOutputInformationCodeList.Codes.SAD22J1,
				JPOutputInformationCodeList.Codes.SAX22J1,
				JPOutputInformationCodeList.Codes.AAD1SG1,
				JPOutputInformationCodeList.Codes.AAX1SG1,
				JPOutputInformationCodeList.Codes.AAD2SG1,
				JPOutputInformationCodeList.Codes.AAX2SG1,
				JPOutputInformationCodeList.Codes.AAD1SH1,
				JPOutputInformationCodeList.Codes.AAD2SH1,
				JPOutputInformationCodeList.Codes.AAD1SJ1,
				JPOutputInformationCodeList.Codes.AAX1SJ1,
				JPOutputInformationCodeList.Codes.AAD2SJ1,
				JPOutputInformationCodeList.Codes.AAX2SJ1,
				JPOutputInformationCodeList.Codes.AAD1VG1,
				JPOutputInformationCodeList.Codes.AAX1VG1,
				JPOutputInformationCodeList.Codes.AAD2VG1,
				JPOutputInformationCodeList.Codes.AAX2VG1,
				JPOutputInformationCodeList.Codes.AAD1VH1,
				JPOutputInformationCodeList.Codes.AAD2VH1,
				JPOutputInformationCodeList.Codes.AAD1VJ1,
				JPOutputInformationCodeList.Codes.AAX1VJ1,
				JPOutputInformationCodeList.Codes.AAD2VJ1,
				JPOutputInformationCodeList.Codes.AAX2VJ1,
				JPOutputInformationCodeList.Codes.AAD1YG1,
				JPOutputInformationCodeList.Codes.AAX1YG1,
				JPOutputInformationCodeList.Codes.AAD2YG1,
				JPOutputInformationCodeList.Codes.AAX2YG1,
				JPOutputInformationCodeList.Codes.AAD1YH1,
				JPOutputInformationCodeList.Codes.AAD2YH1,
				JPOutputInformationCodeList.Codes.AAD1YJ1,
				JPOutputInformationCodeList.Codes.AAX1YJ1,
				JPOutputInformationCodeList.Codes.AAD2YJ1,
				JPOutputInformationCodeList.Codes.AAX2YJ1,
				JPOutputInformationCodeList.Codes.AAD12G1,
				JPOutputInformationCodeList.Codes.AAX12G1,
				JPOutputInformationCodeList.Codes.AAD22G1,
				JPOutputInformationCodeList.Codes.AAX22G1,
				JPOutputInformationCodeList.Codes.AAD12H1,
				JPOutputInformationCodeList.Codes.AAD22H1,
				JPOutputInformationCodeList.Codes.AAD12J1,
				JPOutputInformationCodeList.Codes.AAX12J1,
				JPOutputInformationCodeList.Codes.AAD22J1,
				JPOutputInformationCodeList.Codes.AAX22J1,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_ISTC_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1TG1,
				JPOutputInformationCodeList.Codes.SAX1TG1,
				JPOutputInformationCodeList.Codes.SAD2TG1,
				JPOutputInformationCodeList.Codes.SAX2TG1,
				JPOutputInformationCodeList.Codes.SAD1TH1,
				JPOutputInformationCodeList.Codes.SAD2TH1,
				JPOutputInformationCodeList.Codes.SAD1TJ1,
				JPOutputInformationCodeList.Codes.SAX1TJ1,
				JPOutputInformationCodeList.Codes.SAD2TJ1,
				JPOutputInformationCodeList.Codes.SAX2TJ1,
				JPOutputInformationCodeList.Codes.SAD1WG1,
				JPOutputInformationCodeList.Codes.SAX1WG1,
				JPOutputInformationCodeList.Codes.SAD2WG1,
				JPOutputInformationCodeList.Codes.SAX2WG1,
				JPOutputInformationCodeList.Codes.SAD1WH1,
				JPOutputInformationCodeList.Codes.SAD2WH1,
				JPOutputInformationCodeList.Codes.SAD1WJ1,
				JPOutputInformationCodeList.Codes.SAX1WJ1,
				JPOutputInformationCodeList.Codes.SAD2WJ1,
				JPOutputInformationCodeList.Codes.SAX2WJ1,
				JPOutputInformationCodeList.Codes.SAD1ZG1,
				JPOutputInformationCodeList.Codes.SAX1ZG1,
				JPOutputInformationCodeList.Codes.SAD2ZG1,
				JPOutputInformationCodeList.Codes.SAX2ZG1,
				JPOutputInformationCodeList.Codes.SAD1ZH1,
				JPOutputInformationCodeList.Codes.SAD2ZH1,
				JPOutputInformationCodeList.Codes.SAD1ZJ1,
				JPOutputInformationCodeList.Codes.SAX1ZJ1,
				JPOutputInformationCodeList.Codes.SAD2ZJ1,
				JPOutputInformationCodeList.Codes.SAX2ZJ1,
				JPOutputInformationCodeList.Codes.SAD13G1,
				JPOutputInformationCodeList.Codes.SAX13G1,
				JPOutputInformationCodeList.Codes.SAD23G1,
				JPOutputInformationCodeList.Codes.SAX23G1,
				JPOutputInformationCodeList.Codes.SAD13H1,
				JPOutputInformationCodeList.Codes.SAD23H1,
				JPOutputInformationCodeList.Codes.SAD13J1,
				JPOutputInformationCodeList.Codes.SAX13J1,
				JPOutputInformationCodeList.Codes.SAD23J1,
				JPOutputInformationCodeList.Codes.SAX23J1,
				JPOutputInformationCodeList.Codes.AAD1TG1,
				JPOutputInformationCodeList.Codes.AAX1TG1,
				JPOutputInformationCodeList.Codes.AAD2TG1,
				JPOutputInformationCodeList.Codes.AAX2TG1,
				JPOutputInformationCodeList.Codes.AAD1TH1,
				JPOutputInformationCodeList.Codes.AAD2TH1,
				JPOutputInformationCodeList.Codes.AAD1TJ1,
				JPOutputInformationCodeList.Codes.AAX1TJ1,
				JPOutputInformationCodeList.Codes.AAD2TJ1,
				JPOutputInformationCodeList.Codes.AAX2TJ1,
				JPOutputInformationCodeList.Codes.AAD1WG1,
				JPOutputInformationCodeList.Codes.AAX1WG1,
				JPOutputInformationCodeList.Codes.AAD2WG1,
				JPOutputInformationCodeList.Codes.AAX2WG1,
				JPOutputInformationCodeList.Codes.AAD1WH1,
				JPOutputInformationCodeList.Codes.AAD2WH1,
				JPOutputInformationCodeList.Codes.AAD1WJ1,
				JPOutputInformationCodeList.Codes.AAX1WJ1,
				JPOutputInformationCodeList.Codes.AAD2WJ1,
				JPOutputInformationCodeList.Codes.AAX2WJ1,
				JPOutputInformationCodeList.Codes.AAD1ZG1,
				JPOutputInformationCodeList.Codes.AAX1ZG1,
				JPOutputInformationCodeList.Codes.AAD2ZG1,
				JPOutputInformationCodeList.Codes.AAX2ZG1,
				JPOutputInformationCodeList.Codes.AAD1ZH1,
				JPOutputInformationCodeList.Codes.AAD2ZH1,
				JPOutputInformationCodeList.Codes.AAD1ZJ1,
				JPOutputInformationCodeList.Codes.AAX1ZJ1,
				JPOutputInformationCodeList.Codes.AAD2ZJ1,
				JPOutputInformationCodeList.Codes.AAX2ZJ1,
				JPOutputInformationCodeList.Codes.AAD13G1,
				JPOutputInformationCodeList.Codes.AAX13G1,
				JPOutputInformationCodeList.Codes.AAD23G1,
				JPOutputInformationCodeList.Codes.AAX23G1,
				JPOutputInformationCodeList.Codes.AAD13H1,
				JPOutputInformationCodeList.Codes.AAD23H1,
				JPOutputInformationCodeList.Codes.AAD13J1,
				JPOutputInformationCodeList.Codes.AAX13J1,
				JPOutputInformationCodeList.Codes.AAD23J1,
				JPOutputInformationCodeList.Codes.AAX23J1,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_NH_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD1EG2,
				JPOutputInformationCodeList.Codes.SAX1EG2,
				JPOutputInformationCodeList.Codes.SAD2EG2,
				JPOutputInformationCodeList.Codes.SAX2EG2,
				JPOutputInformationCodeList.Codes.AAD1EG2,
				JPOutputInformationCodeList.Codes.AAX1EG2,
				JPOutputInformationCodeList.Codes.AAD2EG2,
				JPOutputInformationCodeList.Codes.AAX2EG2,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_ISTH_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.SAD15G1,
				JPOutputInformationCodeList.Codes.SAX15G1,
				JPOutputInformationCodeList.Codes.AAD15G1,
				JPOutputInformationCodeList.Codes.AAX15G1,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_NS_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.AAD1DG2,
				JPOutputInformationCodeList.Codes.AAX1DG2,
				JPOutputInformationCodeList.Codes.AAD2DG2,
				JPOutputInformationCodeList.Codes.AAX2DG2,
			}.ToImmutableArray();

			public static readonly ImmutableArray<ZString> ImportPermitMessage_BPS_Codes = new ZString[]
			{
				JPOutputInformationCodeList.Codes.AAD1DJ1,
				JPOutputInformationCodeList.Codes.AAX1DJ1,
				JPOutputInformationCodeList.Codes.AAD2DJ1,
				JPOutputInformationCodeList.Codes.AAX2DJ1,
			}.ToImmutableArray();

			public static class ImportPermitMessageGroupTypes
			{
				public const string GA = "GA";
				public const string GB = "GB";
				public const string GC = "GC";
				public const string GISTH = "GISTH";
				public const string GBPS = "GBPS";
			}

			public static class ImportPermitMessageSubGroupTypes
			{
				public const string GNABC = "GNABC";
				public const string GBPBC = "GBPBC";
				public const string GISTABC = "GISTABC";
			}
		}

		public static class RefSysConfigCodes
		{
			public const string LowerThresholdValueForJapanLargeValueDeclarations = "JPLVTHOLD";
		}

		public static class CountryCodes
		{
			public const string UnknownCountryCode = "ZY";
		}

		public static class HBLDeliveryModes
		{
			public const string CY = "CY";
			public const string CFS = "CFS";
			public const string DOOR = "DOOR";
			public const string PORT = "PORT";
		}

		public static class PortNames
		{
			public const string UnknownPortName = "ZZZ";
		}

		public const string NACCSProdMailboxKey = "NACCSMailP";

		public const string NACCSTestMailboxKey = "NACCSMailT";

		public const string WebPrintParty = "WebPrint";

		public static class DirectxT
		{
			public const string ReceiverAttribute = "std.receiver";
			public const string CompanyCodeAttribute = "custom.NACCS.CompanyCode";
			public const string ClientMailboxAttribute = "custom.NACCS.ClientMailbox";
			public const string DomainAttribute = "custom.NACCS.Domain";
			public const string ServerMailboxAttribute = "custom.NACCS.ServerMailbox";
			public const string MessageIdAttribute = "custom.NACCS.MessageId";
			public const string ProtocolTypeAttribute = "custom.NACCS.ProtocolType";
		}

		public static class ProtocolType
		{
			public const string SMTP = "SMTP";
			public const string POP3 = "POP3";
		}

		public static class DocumentMenuNames
		{
			public const string ExportPermit = "Export Permit";
		}

		public static readonly ImmutableArray<string> EACNoticeMessageCodes = new EACSucceedNotice().OutputInformationCodes.ToImmutableArray().AddRange(new EACFailNotice().OutputInformationCodes.ToImmutableArray());
	}
}
