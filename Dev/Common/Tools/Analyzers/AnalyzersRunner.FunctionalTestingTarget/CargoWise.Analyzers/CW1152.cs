namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1152
	{
		// CW1152 Do not declare public virtual properties or methods. Create a public non-virtual property/method instead, and a protected virtual property/method with the same name + Core appended to the end.
		public virtual string GetName0(string name)
		{
			return name;
		}

		public virtual string GetName1(string name) => name;

		readonly string name;

		public virtual string Name0 => name;

		public virtual string Name1 { get; set; }
	}
}
