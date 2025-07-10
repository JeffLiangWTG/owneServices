using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820.MN_NG_8240_CargoQuery_Message;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN820RequestMessagePrettier : ILEDIMessagePrettierBase<MnNg8240CargoQueryMessage>
	{
		public ILMAN820RequestMessagePrettier(MessageDataObject<MnNg8240CargoQueryMessage> messageDataObject) : base(messageDataObject)
		{
		}
		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData,
				builder => builder.WithRequestSection((cargoQuery, sb) =>
				{
					sb.Append(GetTable(cargoQuery));
				})
			)
			.Build();

		string GetTable(MnNg8240CargoQueryMessage cargoQuery)
		{
			var creator = new HtmlTableCreator();

			var columnWidthTitleCell = 240;
			var columnWidthValueCell = 260;

			creator.WriteRowWithFormatting(
				CreateCell(PrettiedCaptions.Common.Date, true, columnWidthTitleCell),
				CreateCell(cargoQuery.RequestContentHeader?.TransmitionDateTime.ToLongDateString(), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(PrettiedCaptions.ManifestQuery.CargoIdentifierType, true, columnWidthTitleCell),
				CreateCell(GetCargoIdentifierType(cargoQuery.CargoIdentifier?.CargoIdentifierType.ToString()), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(PrettiedCaptions.ManifestQuery.CargoIdentifierKey1, true, columnWidthTitleCell),
				CreateCell(cargoQuery.CargoIdentifier?.CargoIdentifierKey1, false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(PrettiedCaptions.ManifestQuery.CargoIdentifierKey2, true, columnWidthTitleCell),
				CreateCell(cargoQuery.CargoIdentifier?.CargoIdentifierKey2, false, columnWidthValueCell));

			return creator.ToHtml();
		}

		string GetCargoIdentifierType(string cargoIdentifierType)
		{
			return cargoIdentifierType switch
			{
				Constants.CargoIdentifierType.SeaDealImport => $"{TransportModes.Sea} - {cargoIdentifierType}",
				_ => string.Empty
			};
		}
	}
}
