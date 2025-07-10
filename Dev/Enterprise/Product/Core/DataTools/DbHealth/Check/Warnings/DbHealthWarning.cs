namespace Enterprise.DbHealth.Check
{
	public abstract class DbHealthWarning
	{
		protected DbHealthWarning(string source, string warningType, string description, string action)
		{
			this.source = source;
			this.warningType = warningType;
			this.description = description;
			this.action = action;
		}

		public abstract string SourceType { get; }

		public string Source
		{
			get { return source; }
		}

		readonly string source;

		public string WarningType
		{
			get { return warningType; }
		}

		readonly string warningType;

		public string Description
		{
			get { return description; }
		}

		readonly string description;

		public string Action
		{
			get { return action; }
		}

		readonly string action;
	}
}
