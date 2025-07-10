using System.IO;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public interface IFileOutput
	{
		void Write(StreamWriter writer);
	}
}
