using System.IO;

namespace Enterprise.DataTransfer.Common.Import
{
	public interface IStreamProvider
	{
		Stream Stream();
	}
}