namespace Enterprise.DbHealth.Check
{
	class VeryDummyWarning : DbHealthWarning
	{
		public VeryDummyWarning(string source, string warningType, string description, string action)
			: base(source, warningType, description, action)
		{
		}

		public override string SourceType
		{
			get { return StaticSourceType; }
		}
		public const string StaticSourceType = "I'm very dummy";
	}
}
