using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZNewRowColumn : ZTemplateColumn
	{
		public ZNewRowColumn(string bindTo)
			: base(String.Empty, bindTo)
		{
			base.Visible = false;
		}

		public new bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}
		bool visible = true;

		public bool Collapsable
		{
			get { return collapsable; }
			set { collapsable = value; }
		}
		bool collapsable;

		public override ITemplate ItemTemplate
		{
			get { return base.ItemTemplate; }
			set
			{
				if (value is INewRowColumnItemTemplate)
				{
					base.ItemTemplate = value;
				}
				else
				{
					innerItemTemplate = value;
				}
			}
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return null;
		}

		public ITemplate InnerItemTemplate
		{
			get { return innerItemTemplate; }
		}
		ITemplate innerItemTemplate;

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZNewRowColumnItemTemplate(this);
		}
	}

	public interface INewRowColumnItemTemplate : ITemplate
	{
	}

	internal class ZNewRowColumnItemTemplate : INewRowColumnItemTemplate
	{
		public ZNewRowColumnItemTemplate(ZNewRowColumn column)
			: base()
		{
			this.column = column;
		}

		public ZNewRowColumn Column
		{
			get { return column; }
		}
		readonly ZNewRowColumn column;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html id name should not be translated")]
		public void InstantiateIn(Control container)
		{
			if (Column.InnerItemTemplate != null)
			{
				Column.InnerItemTemplate.InstantiateIn(container);
			}
			if (Column.Collapsable)
			{
				ZExpandCollapseButton button = new ZExpandCollapseButton();
				button.ID = "Expand";
				container.Controls.AddAt(0, button);
			}
		}
	}
}