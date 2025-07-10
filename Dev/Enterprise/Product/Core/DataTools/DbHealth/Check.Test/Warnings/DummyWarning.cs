namespace Enterprise.DbHealth.Check
{
	class DummyWarning : DbHealthWarning
	{
		public DummyWarning(string source, string warningType, string description, string action)
			: base(source, warningType, description, action)
		{
		}

		public override string SourceType
		{
			get { return StaticSourceType; }
		}

		public const string StaticSourceType = "I'm dummy";
	}
}
