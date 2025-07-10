using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	[ToolboxData("<{0}:ZPreformattedText runat=server></{0}:ZPreformattedText>")]
	public class ZPreformattedText : WebControl, ISelfBindingWebControl, IHtmlEncodableLabelControl
	{
		public ZPreformattedText() : base(HtmlTextWriterTag.Pre)
		{
			EnableHtmlEncoding = true;
		}

		LiteralControl Contents
		{
			get
			{
				if (fContents == null)
				{
					fContents = new LiteralControl();
				}

				return fContents;
			}
		}
		LiteralControl fContents;

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Add(Contents);
		}

		/// <summary>
		/// Overriding default rendering because it renders things like:
		///			<pre>
		///				some text here
		///			</pre>
		///	which causes additional tabs being displayed in the first line of the note.
		///	
		///	What should be rendered is: <pre>some text here</pre>
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			writer.Write("<pre>");
			writer.Write(this.GetHtmlEncodableLabelContent(Contents.Text));
			writer.Write("</pre>");
		}

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			try
			{
				Contents.Text = new StringConverter().ConvertToString(ZPropertyAccessor.Get(dataSource, BindTo));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Contents.Text = "";
			}
		}

		public void UnBind()
		{
			Contents.Text = "";
		}

		#endregion

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion

		#region IHtmlEncodableLabelControl Members

		public bool EnableHtmlEncoding
		{
			get;
			set;
		}

		#endregion
	}

	#endregion
}
