namespace Enterprise.Customs.EU.Business
{
	public class EoriSuffixAndType
	{
		public EoriSuffixAndType(string number, string type)
		{
			this.Number = number;
			this.Type = type;
		}
		public string Number;
		public string Type;

		public const string EoriBranchStatementType = "BR";
		public const string EoriDeclarantStatementType = "AG";
	}
}
