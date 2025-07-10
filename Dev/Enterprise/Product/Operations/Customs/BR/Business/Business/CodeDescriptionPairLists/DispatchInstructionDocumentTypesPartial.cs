namespace Enterprise.Customs.BR.Business
{
	public partial class DispatchInstructionDocumentTypes
	{
		public static string MapToCustomsCode(string code)
		{
			switch (code)
			{
				case DispatchInstructionDocumentTypes.Codes._01:
					return "49";
				case DispatchInstructionDocumentTypes.Codes._28:
					return "30";
				default:
					return string.Empty;
			}
		}
	}
}
