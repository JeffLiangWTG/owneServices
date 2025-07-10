using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Edifact;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.MCP
{
	public class McpTransmissionMessageGenerator : GbChiefEdifactTransmissionMessageGenerator
	{
		public McpTransmissionMessageGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction)
			: base(declarationMessageFunction)
		{
		}

		protected override ZString GetApplicationCode(Customs.Business.CusEntryHeader entry)
		{
			return ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly;
		}

		protected override ZString GetApplicationReference(CusEntryHeader entryHeader)
		{
			return "Destin8/" + entryHeader.Declaration.JE_CustomsProfile;
		}

		protected override UNCharacterSet CharSet
		{
			get => McpCharSet;
		}

		public static UNCharacterSet McpCharSet // static for CusResUndrerstanderer
		{
			get => new UkCharSet('\\', '#', '?', '{');
		}
	}
}
