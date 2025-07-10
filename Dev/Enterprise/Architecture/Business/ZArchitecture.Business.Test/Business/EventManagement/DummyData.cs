namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class DummyData : IReferenceVersion
	{
		public string Name { get; set; }

		public int Version { get; set; }

		public int GetVersion(string reference)
		{
			return 1;
		}
	}
}
