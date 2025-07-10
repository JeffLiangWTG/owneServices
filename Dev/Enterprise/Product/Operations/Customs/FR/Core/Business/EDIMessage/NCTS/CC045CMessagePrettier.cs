using CargoWise.Customs.FR.MessageDefinitions.TP5.CC045C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC045CMessagePrettier : NCTSMessagePrettier<Cc045CType>
	{
		public CC045CMessagePrettier(CC045CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc045CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", NCTS5DepartureCustomsStatusList.Descriptions.GoodsWrittenOffClosed ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Write-off Date", transitOperation?.WriteOffDate.ToString("dd/MM/yyyy") ?? ZString.Empty)
			});
		}
	}
}
