using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IClipboard
	{
		#if !WINZOR || DEBUG
		IDataObject GetDataObject();
		#endif

		bool SetDataObject(object data, bool copy = false);
	}
}
