using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Control that displays notes for business objects
	/// </summary>
	[ToolboxData("<{0}:ZNotesControl runat=server></{0}:ZNotesControl>")]
	public class ZNotesControl : ZRepeater
	{
		public ZNotesControl()
		{
			this.ItemTemplate = new NotesItemTemplate();
		}
	}
}
