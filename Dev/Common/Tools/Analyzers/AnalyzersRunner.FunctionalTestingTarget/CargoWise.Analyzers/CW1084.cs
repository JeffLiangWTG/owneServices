namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1084 : BaseClass
	{
		//CW1084:Virtual New AddNew
		public virtual new void Method()
		{ }
	}

	class BaseClass
	{
		public virtual void Method()
		{ }
	}
}
