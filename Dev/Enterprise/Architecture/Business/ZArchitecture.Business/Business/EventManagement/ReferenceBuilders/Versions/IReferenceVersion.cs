namespace Enterprise.ZArchitecture.Business
{
	public interface IReferenceVersion
	{
		int Version { get; set; }

		int GetVersion(string reference);
	}
}
