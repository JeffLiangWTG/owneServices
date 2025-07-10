using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Column to display bound text with edit functionality.
	/// </summary>
	public class ZDocAddressColumn : ZTemplateColumn
	{
		#region Constructors

		public ZDocAddressColumn()
			: this(false)
		{
		}

		public new ZDataGrid Owner
		{
			get
			{
				return (ZDataGrid)base.Owner;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public ZDocAddressColumn(bool govermentRegNoVisible)
			: base("Address", "")
		{
			this.govermentRegNoVisible = govermentRegNoVisible;
		}

		#endregion

		#region Properties

		public bool GovermentRegNoVisible
		{
			get
			{
				return govermentRegNoVisible;
			}
		}
		readonly bool govermentRegNoVisible;

		#endregion

		#region Overrides

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZDocAddressColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZDocAddressColumnEditItemTemplate(this);
		}

		#endregion

	}
}
