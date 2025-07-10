using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Tab Page  PlugIn that has auto sized enabled; used for the eDocs tab.
	/// The ZAutoSizedTabPagePlugIn is placed at the end of the TabPage. Do not misuse it.
	/// </summary>
	public class ZAutoSizedTabPagePlugIn : ZTabPagePlugIn
	{
		public ZAutoSizedTabPagePlugIn(ZPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected internal override bool IsAutoSized
		{
			get { return true; }
		}
	}
}
