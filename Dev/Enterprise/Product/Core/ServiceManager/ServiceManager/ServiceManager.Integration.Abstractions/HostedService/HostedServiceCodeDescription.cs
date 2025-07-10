namespace ServiceManager.Integration.Abstractions
{
	public struct HostedServiceCodeDescription
	{
		public HostedServiceCodeDescription(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public string Code { get; }
		public string Description { get; }
	}
}
