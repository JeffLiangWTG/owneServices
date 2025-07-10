namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.CSharp.CodeStyle
{
	class IDE0070
	{
		readonly int j;

		//IDE0070:Use 'System.HashCode'
		public override int GetHashCode()
		{
			// IDE0070: GetHashCode can be simplified.
			var hashCode = 339610899;
			hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + j.GetHashCode();
			return hashCode;
		}
	}
}
