namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	/// <summary>
	/// Rule: Static holder types should be Static or NotInheritable
	/// </summary>
	public class CA1052
	{
		// CA1052: Static holder types should be Static or NotInheritable
		public static int SomeProperty { get; set; }
		public static void SomeMethod() { }
	}

	class CA1052Internal
	{
		class CA1052Private
		{
			// CA1052: Static holder types should be Static or NotInheritable
			internal static int AnotherProperty { get; set; }
			internal static void AnotherMethod() { }
		}
	}
}
