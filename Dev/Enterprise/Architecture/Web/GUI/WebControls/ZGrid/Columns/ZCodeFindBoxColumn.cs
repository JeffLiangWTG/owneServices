using System;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCodeFindBoxColumn : ZFindBoxColumn, IExcelExportCustomValue
	{
		public ZCodeFindBoxColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
		}

		public ZCodeFindBoxColumn(string headerText, string bindTo, string bindToList) : base(headerText, bindTo, bindToList)
		{
		}

		public ZCodeFindBoxColumn(string headerText, string bindTo, string bindToList, Type bizOType) : base(headerText, bindTo, bindToList, bizOType)
		{
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZCodeFindBoxColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZCodeFindBoxColumnEditItemTemplate(this);
		}

		#region Implementation of IExcelExportCustomValue

		IZType IExcelExportCustomValue.GetCustomValue(BusinessObject bizO)
		{
			ZString value = bizO.ZPropertyInfoHash[BindTo].Value.ToString();
			ZString result = value;

			if (!string.IsNullOrEmpty(BindToList))
			{
				IFindBoxListProvider lookupList = (IFindBoxListProvider)ZPropertyAccessor.Get(bizO, BindToList);
				BusinessObject obj = lookupList.GetBusinessObjectFromCode(value);
				string code = obj != null ? value : ZString.Empty;

				switch (DisplayStyle)
				{
					case OComboBoxDropDownStyle.CodeOnly:
						result = code;
						break;
					case OComboBoxDropDownStyle.DescriptionOnly:
						result = lookupList.DescriptionFromCode(code);
						break;
					case OComboBoxDropDownStyle.CodeAndDescription:
						string desc = lookupList.DescriptionFromCode(code);
						result = string.IsNullOrEmpty(desc) && string.IsNullOrEmpty(code)
									? ZString.Empty
									: ZString.Format("{0} ({1})", desc, code);
						break;
				}
			}

			return result;
		}

		ZString IExcelExportCustomValue.GetValueFormat(IZType value)
		{
			return ZString.Empty;
		}

		ZString IExcelExportCustomValue.GetDescription()
		{
			return HeaderText;
		}

		#endregion
	}
}
