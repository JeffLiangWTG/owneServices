using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public class DefaultClipboard : IClipboard
	{
		#if !WINZOR || DEBUG

		public IDataObject GetDataObject() => SafeClipboard.GetDataObject();

		#endif

		public bool SetDataObject(object data, bool copy = false) => SafeClipboard.SetDataObject(data, copy);
	}
}
