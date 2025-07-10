using System;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZDropDownListColumn.
	/// </summary>
	public class ZDropDownListColumn : ZTemplateColumn, IDropDownListSupport, IExcelExportCustomValue, IBindToList
	{
		public ZDropDownListColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
			this.showHint = true;
			this.EditorWidth = -1;
			this.ShowEmptyItem = true;
		}

		public ZDropDownListColumn(string headerText, string bindTo, OComboBoxDropDownStyle displayStyle)
			: this(headerText, bindTo)
		{
			this.DisplayStyle = displayStyle;
		}

		public ZDropDownListColumn(string headerText, string bindTo, string bindToList) : this(headerText, bindTo)
		{
			this.BindToList = bindToList;
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZDropDownListColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZDropDownListColumnEditItemTemplate(this);
		}

		public string BindToList
		{
			get { return bindToList; }
			set { bindToList = value; }
		}
		protected string bindToList = "";

		public string ValueFieldName
		{
			get { return valueFieldName; }
			set { valueFieldName = value; }
		}
		protected string valueFieldName = "";

		public string TextFieldName
		{
			get { return textFieldName; }
			set { textFieldName = value; }
		}
		protected string textFieldName = "";

		public bool AutoPostBack
		{
			get { return autoPostBack; }
			set { autoPostBack = value; }
		}
		protected bool autoPostBack;

		public bool ShowEmptyItem
		{
			get { return showEmptyItem; }
			set { showEmptyItem = value; }
		}
		protected bool showEmptyItem;

		[DefaultValue(OComboBoxDropDownStyle.DescriptionOnly)]
		public OComboBoxDropDownStyle DisplayStyle
		{
			get { return displayStyle; }
			set { displayStyle = value; }
		}
		protected OComboBoxDropDownStyle displayStyle = OComboBoxDropDownStyle.DescriptionOnly;

		public bool ShowHint
		{
			get { return showHint; }
			set { showHint = value; }
		}
		bool showHint;

		#region IExcelExportCustomValue Members

		public IZType GetCustomValue(BusinessObject bizObj)
		{
			ZString result;
			ZString value;

			if (bizObj is IWrappedBizOProvider provider)
			{
				if (bizObj.ZPropertyInfoHash.ContainsKey(BindTo))
				{
					value = bizObj.ZPropertyInfoHash[BindTo].Value.ToString();
				}
				else
				{
					var bindingArray = BindTo.Split('.');
					var bindingRoot = bindingArray[bindingArray.Length - 1];
					var wrappedBizo = provider.GetWrappedBizO();
					value = wrappedBizo.ZPropertyInfoHash[bindingRoot].Value.ToString();
				}
			}
			else
			{
				value = bizObj.ZPropertyInfoHash[BindTo].Value.ToString();
			}

			result = value;

			if (string.IsNullOrEmpty(BindToList))
			{
				string listMember = MetadataHelper.GetListMember(this, bizObj);
				BindToList = listMember;
			}
			if (!string.IsNullOrEmpty(BindToList))
			{
				object lookupList = ZPropertyAccessor.Get(bizObj, BindToList);
				if (lookupList != null)
				{
					switch (DisplayStyle)
					{
						case OComboBoxDropDownStyle.CodeOnly:
							result = GetLookupCode(lookupList, value);
							break;
						case OComboBoxDropDownStyle.DescriptionOnly:
							result = GetLookupDescription(lookupList, value);
							break;
						case OComboBoxDropDownStyle.CodeAndDescription:
							string desc = GetLookupDescription(lookupList, value);
							string code = GetLookupCode(lookupList, value);
							if (((ZString)desc != "") || ((ZString)code != ""))
							{
								result = String.Format("{0} ({1})", desc, code);
							}
							else
							{
								result = "";
							}
							break;
					}
				}
			}

			return result;
		}

		protected string GetLookupDescription(object list, IZType value)
		{
			string result = string.Empty;
			if (list is ICodeDescriptionPairList)
			{
				result = ((ICodeDescriptionPairList)list).GetDescriptionFromCode(GetLookupCode(list, value));
			}
			if (string.IsNullOrEmpty(result))
			{
				result = GetDescriptionFromCode(list, GetLookupCode(list, value));
			}

			return result;
		}

		string GetDescriptionFromCode(object list, IZType value)
		{
			string bindTo = string.IsNullOrEmpty(ValueFieldName) ? BindTo : ValueFieldName;
			ZString result = ZString.Empty;
			if (list is IBusinessObjectCollection)
			{
				foreach (var obj in (IBusinessObjectCollection)list)
				{
					string curValue = ZPropertyAccessor.Get(obj, bindTo).ToString();
					if (value.ToString() == curValue)
					{
						result = ZPropertyAccessor.Get(obj, TextFieldName)?.ToString();
						break;
					}
				}
			}
			return result;
		}

		protected ZString GetLookupCode(object list, IZType value)
		{
			string bindTo = string.IsNullOrEmpty(ValueFieldName) ? BindTo : ValueFieldName;
			ZString result = ZString.Empty;
			if (list is ICodeDescriptionPairList && ((ICodeDescriptionPairList)list).ContainsCode((ZString)value))
			{
				result = (ZString)value;
			}
			if (list is IBusinessObjectCollection)
			{
				foreach (var obj in (IBusinessObjectCollection)list)
				{
					string curValue = ZPropertyAccessor.Get(obj, bindTo).ToString();
					if (value.ToString() == curValue)
					{
						result = value.ToString();
						break;
					}
				}
			}
			return result;
		}

		public ZString GetDescription()
		{
			return this.HeaderText;
		}

		public ZString GetValueFormat(IZType value)
		{
			return ZString.Empty;
		}

		#endregion
	}
}
