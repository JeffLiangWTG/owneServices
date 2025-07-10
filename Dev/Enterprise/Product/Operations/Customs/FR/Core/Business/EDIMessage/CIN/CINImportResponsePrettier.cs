using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CINImportResponsePrettier : FREDIMessagePrettier
	{
		public CINImportResponsePrettier(MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
		{
			return ToH1IfNotEmpty(Res.GetString("257a0f01-26a3-4cf8-84fe-74d7b0e2e228", "The content of the response is not defined as yet"));
		}
	}
}
