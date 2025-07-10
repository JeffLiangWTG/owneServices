namespace Enterprise.UniversalDataBuss.Integration
{
	public enum ImportAction
	{
		/// <summary>
		/// Insert no match, Update on match (default)
		/// </summary>
		Merge,

		/// <summary>
		/// Fail no match, Link on match
		/// </summary>
		LinkOnly
	}
}
