using System.Drawing;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// used to present timelines in the grid
	/// </summary>
	public class ZDateTimeStatusColumn : ZDateTimeColumn, IExcelExportCellColor
	{
		public ZDateTimeStatusColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
		}

		public ZDateTimeStatusColumn(string headerText, string bindTo, string bindToStatus, ZDateTimePickerFormat dateFormat) : base(headerText, bindTo, dateFormat)
		{
			this.BindToStatus = bindToStatus;
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZDateTimeStatusColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZDateTimeStatusColumnEditItemTemplate(this);
		}

		public string BindToStatus
		{
			get { return fBindToStatus; }
			set { fBindToStatus = value; }
		}
		string fBindToStatus;

		#region IExcelExportCellColor Members

		public Color? GetCustomColor(BusinessObject bizObj)
		{
			ZDateTime actualValue = (ZDateTime)bizObj.ZPropertyInfoHash[this.BindTo].Value;
			ZString statusValue = !string.IsNullOrEmpty(BindToStatus) ? (ZString)bizObj.ZPropertyInfoHash[this.BindToStatus].Value : ZString.Empty;

			Color? result = Color.White;

			switch (statusValue.ToString())
			{
				case Constants.DateTimeStatus.Overdue: result = Color.FromArgb(255, 182, 193); break;
				case Constants.DateTimeStatus.Late: result = Color.FromArgb(255, 204, 153); break;
				case Constants.DateTimeStatus.OnTime: result = Color.FromArgb(152, 252, 142); break;
			}

			return result;
		}

		#endregion
	}
}
