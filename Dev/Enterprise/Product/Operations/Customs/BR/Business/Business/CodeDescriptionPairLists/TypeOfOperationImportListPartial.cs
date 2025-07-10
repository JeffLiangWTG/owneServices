namespace Enterprise.Customs.BR.Business
{
	public partial class TypeOfOperationImportList
	{
		public static string MapToCustomsCode(string type)
		{
			switch (type)
			{
				case Codes.OnItsOwn:
					return "IMPORTACAO_DIRETA";
				case Codes.AccountAndOrder:
					return "IMPORTACAO_POR_CONTA_E_ORDEM";
				default:
					return null;
			}
		}
	}
}
