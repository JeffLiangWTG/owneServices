using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Z version of standard Repeater Web Control
	/// </summary>
	[DefaultProperty("Text"),
	ToolboxData("<{0}:ZRepeater runat=server></{0}:ZRepeater>")]
	public class ZRepeater : Repeater, ISelfBindingWebControl
	{
		#region IBindTo Members

		/// <summary>
		/// Name of the property on the Business Object to bind the control to
		/// </summary>
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return (dataSource is ICollection) || !string.IsNullOrEmpty(BindTo);
		}

		/// <summary>
		/// Bind the object as a data source
		/// </summary>
		/// <param name="dataSource">The object to bind</param>
		public void Bind(object dataSource)
		{
			if (IsBindable(dataSource))
			{
				if (!string.IsNullOrEmpty(BindTo))
				{
					this.DataSource = ZPropertyAccessor.Get(dataSource, BindTo);
				}
				else if (dataSource is ICollection)
				{
					this.DataSource = dataSource;
				}
				DataBind();
			}
			IsBound = true;
		}
		bool IsBound;

		public void UnBind()
		{
			this.DataSource = null;
			this.DataBind();
		}

		#endregion

		#region Control Overrides

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (IsBound && Controls.Count == 0 && !HideStatusLabel)
			{
				StatusLabel.Text = Res.GetString("24770382-69f0-4188-994f-ce6f2da9919a", "No Data Found");
				StatusPanel.CssClass = CssConstants.DetailsTable;
				Controls.Add(StatusPanel);
			}
		}

		#endregion Control Overrides

		#region Status Panel/Label

		protected internal ZTextLabel StatusLabel
		{
			get
			{
				if (fStatusLabel == null)
				{
					fStatusLabel = new ZTextLabel();
				}
				return fStatusLabel;
			}
		}
		ZTextLabel fStatusLabel;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		protected internal Panel StatusPanel
		{
			get
			{
				if (fStatusPanel == null)
				{
					fStatusPanel = new Panel();
					fStatusPanel.ID = "Status";
					fStatusPanel.Controls.Add(StatusLabel);
				}
				return fStatusPanel;
			}
		}
		Panel fStatusPanel;

		public bool HideStatusLabel
		{
			get { return hideStatusLabel; }
			set { hideStatusLabel = value; }
		}
		bool hideStatusLabel;

		#endregion Status Panel/Label
	}
}
