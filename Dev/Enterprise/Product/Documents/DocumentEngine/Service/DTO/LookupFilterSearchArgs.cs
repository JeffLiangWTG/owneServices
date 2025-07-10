namespace Enterprise.DocumentEngine
{
	public class LookupFilterSearchArgs : ReportLookupSearchArgs
	{
		public string LookupType
		{
			get { return lookupType; }
			set { lookupType = value?.Trim(); }
		}

		string lookupType;
	}
}
