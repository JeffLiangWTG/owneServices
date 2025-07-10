using System.IO;

namespace CargoWise.EntityFramework
{
	public interface IStreamSource
	{
		Stream GetStream();
	}
}
