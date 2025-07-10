namespace Enterprise.ZArchitecture.Modules
{
	public interface ISecured
	{
		ISecurityCheckpoint SecurityCheckpoint { get; }
		ISecurityCheckpoint[] GetSecurityCheckpointForPopups();
	}
}
