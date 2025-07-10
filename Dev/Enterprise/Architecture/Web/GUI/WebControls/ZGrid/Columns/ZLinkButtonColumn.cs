using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZLinkButtonColumn : ZTemplateColumn
	{
		public ZLinkButtonColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZLinkButtonColumnItemTemplate(this);
		}

		#region Command

		public string Command
		{
			get { return fCommand; }
			set { fCommand = value; }
		}
		string fCommand;

		#endregion

		#region ClientClickHandler

		public string ClientClickHandler
		{
			get { return fClientClickHandler; }
			set { fClientClickHandler = value; }
		}
		string fClientClickHandler;

		#endregion
	}
}
