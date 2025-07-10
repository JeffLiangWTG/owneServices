namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.CSharp.CodeStyle
{
	class IDE0052
	{
		//IDE0052:Remove unread private members
		readonly int UnreadField;

		public IDE0052()
		{
			UnreadField = 5;
		}
	}
}
