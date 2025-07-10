using System.Data;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSMessage : EDIMessage
	{
		public COLSMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static string SenderID
		{
			get { return EDIInterchange.InterchangePartyIDs.EXDOCSendersMailbox; }
		}

		public static string ReceiverID
		{
			get { return EDIInterchange.InterchangePartyIDs.EXDOCReceiversMailbox; }
		}

		public override ZString EM_FormattedMessageText
		{
			get
			{
				if (formattedMessageText.IsEmpty)
				{
					var messageText = EM_MessageText;
					try
					{
						formattedMessageText = GetFormattedMessageText(messageText);
					}
					catch
					{
						formattedMessageText = messageText;
					}
				}
				return formattedMessageText;
			}
		}
		ZString formattedMessageText = ZString.Empty;

		protected virtual ZString GetFormattedMessageText(ZString messageText)
		{
			var result = messageText;
			if (!string.IsNullOrWhiteSpace(messageText))
			{
				result = JsonSerializer.Serialize(JsonDocument.Parse(messageText),
					new JsonSerializerOptions { WriteIndented = true });
			}
			return result;
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation => EM_FormattedMessageText;

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ApplicationCode = ApplicationCodes.COLS;
			EM_IsTestMessage = Env.Registry.AQISMessagingTestMode;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", SenderID, ReceiverID).GetNextFormatted(Factory) + "00";
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;
	}
}
