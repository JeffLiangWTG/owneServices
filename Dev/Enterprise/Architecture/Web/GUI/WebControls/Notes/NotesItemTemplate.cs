using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for NotesItemTemplate.
	/// </summary>
	public class NotesItemTemplate : ITemplate
	{
		public NotesItemTemplate()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1147:DoNotUseWebControls", Justification = "To be removed")]
		public void InstantiateIn(Control container)
		{
			WebControl span = new WebControl(HtmlTextWriterTag.Span);
			span.CssClass = CssConstants.DetailsItem;
			container.Controls.Add(span);

			ZDateTimeLabel dateLabel = new ZDateTimeLabel();
			dateLabel.DateTimeFormat = ZDateTimePickerFormat.Long;
			dateLabel.BindTo = StmNote.Schema.ST_CreatedDateUtc;
			span.Controls.Add(dateLabel);

			span.Controls.Add(new LiteralControl("&nbsp;"));
			span.Controls.Add(new LiteralControl("-"));
			span.Controls.Add(new LiteralControl("&nbsp;"));

			ZTextLabel shortLabel = new ZTextLabel();
			shortLabel.BindTo = StmNote.Schema.ST_Description;
			span.Controls.Add(shortLabel);

			ZPreformattedText details = new ZPreformattedText();
			details.BindTo = StmNote.Schema.ST_NoteDataAsText;
			container.Controls.Add(details);

			container.DataBinding += new EventHandler(container_DataBinding);
		}

		void container_DataBinding(object sender, EventArgs e)
		{
			RepeaterItem item = sender as RepeaterItem;
			BindControls(item, item.DataItem);
		}

		void BindControls(Control ctrl, object dataSource )
		{
			if (ctrl is ISelfBindingWebControl)
			{
				((ISelfBindingWebControl)ctrl).Bind(dataSource);
			}
			else
			{
				foreach (Control child in ctrl.Controls)
				{
					BindControls(child, dataSource);
				}
			}
		}
	}
}
