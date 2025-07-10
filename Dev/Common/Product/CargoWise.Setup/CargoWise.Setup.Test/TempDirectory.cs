namespace CargoWise.Setup.Test;

public class TempDirectory : IDisposable
{
	public TempDirectory()
	{
		Path = Directory.CreateTempSubdirectory().FullName;
	}

	public string Path { get; }

	public void Dispose()
	{
		if (Directory.Exists(Path))
		{
			Directory.Delete(Path, true);
		}
	}
}
