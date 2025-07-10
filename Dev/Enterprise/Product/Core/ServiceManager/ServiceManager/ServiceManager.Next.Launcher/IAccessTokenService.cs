namespace CargoWise.ServiceManager.Next.Launcher;

public interface IAccessTokenService
{
	bool RotateToken(TimeSpan validity, TimeSpan overlap);
	bool CheckTokenValidity(string token);
}
