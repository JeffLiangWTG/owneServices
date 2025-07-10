using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IL.Business.Message;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN170RequestMessagePrettier : ILEDIMessagePrettierBase<MnMsg1Manifest>
	{
		public ILMAN170RequestMessagePrettier(MessageDataObject<MnMsg1Manifest> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData.Declaration,
				builder =>
				builder.WithRequestSection((declaration, sb) =>
				{
					sb.Append(GetTable(declaration));
				})
			)
			.Build();

		string GetTable(Declaration declaration)
		{
			var declarationConsignmentTransportContractDocument = declaration?.Consignment?.SelectMany(c => c.TransportContractDocument.Where(t => t.TypeCode?.Value == TransportDocsTypeList.Codes.IL2)).FirstOrDefault();

			var creator = new HtmlTableCreator();

			var columnWidthTitleCell = 240;
			var columnWidthValueCell = 260;

			creator.WriteRowWithFormatting(
				CreateCell(PrettiedCaptions.Manifest.ManifestNumber, true, columnWidthTitleCell),
				CreateCell(declaration.Id?.Value, false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(PrettiedCaptions.Manifest.ParentDealNumber, true, columnWidthTitleCell),
				CreateCell(declarationConsignmentTransportContractDocument?.Id.Value, false, columnWidthValueCell));

			return creator.ToHtml();
		}
	}
}

