using System;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZFindBoxColumn : ZTemplateColumn, IGuidBindToListSupport
	{
		public ZFindBoxColumn(string headerText, string bindTo)
			: base(headerText, bindTo)
		{
			fSortExpression = "";
		}

		public ZFindBoxColumn(string headerText, string bindTo, string bindToList)
			: this(headerText, bindTo)
		{
			this.BindToList = bindToList;
		}

		public ZFindBoxColumn(string headerText, string bindTo, string bindToList, Type bizOType)
			: this(headerText, bindTo, bindToList)
		{
			this.BizOType = bizOType;
		}
		readonly Type BizOType;

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZFindBoxColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZFindBoxColumnEditItemTemplate(this);
		}

		public string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		protected string fBindToList = "";

		public string ValueFieldName
		{
			get { return fValueFieldName; }
			set { fValueFieldName = value; }
		}
		protected string fValueFieldName = "";

		public string TextFieldName
		{
			get { return fTextFieldName; }
			set { fTextFieldName = value; }
		}
		protected string fTextFieldName = "";

		public bool AutoPostBack
		{
			get { return fAutoPostBack; }
			set { fAutoPostBack = value; }
		}
		protected bool fAutoPostBack;

		public WebModuleID ModuleID
		{
			get { return fModuleID; }
			set { fModuleID = value; }
		}
		protected WebModuleID fModuleID;

		[DefaultValue(OComboBoxDropDownStyle.CodeOnly)]
		public OComboBoxDropDownStyle DisplayStyle
		{
			get { return fDisplayStyle; }
			set { fDisplayStyle = value; }
		}
		protected OComboBoxDropDownStyle fDisplayStyle;

		public override string SortExpression
		{
			get
			{
				if (fSortExpression == null)
				{
					fSortExpression = "";
				}
				if (string.IsNullOrEmpty(fSortExpression) && BizOType != null)
				{
					PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(BizOType);
					PropertyDescriptor propDesc = properties.Find(BindTo, false);
					if (propDesc != null)
					{
						string relatedProperty = RelatedBusinessObjectAttribute.GetRelatedBizObjName(propDesc);
						if (relatedProperty != null)
						{
							PropertyDescriptor relatedBizOPropDesc = properties.Find(relatedProperty, false);

							if (relatedBizOPropDesc != null)
							{
								try
								{
									if (DisplayStyle == OComboBoxDropDownStyle.DescriptionOnly)
									{
										fSortExpression = relatedProperty + "." + DescriptionPropertyAttribute.DescriptionPropertyNameFromType(relatedBizOPropDesc.PropertyType);
									}
									else
									{
										fSortExpression = relatedProperty + "." + CodePropertyAttribute.CodePropertyNameFromType(relatedBizOPropDesc.PropertyType);
									}
								}
								catch (NoCodePropertyException)
								{
									ErrorReporter.ReportOnce("ZFindBoxColumnSort_" + BindTo, "NoCodePropertyException caught while determining SortExpression for column bound to " + BindTo + " using Type " + BizOType.FullName);
								}
							}
							else
							{
								ErrorReporter.ReportOnce("ZFindBoxColumnSort_NoRelatedProperty_" + BindTo, "RelatedBusinessObject is not set on BizOType " + BizOType.FullName + " for property " + BindTo);
							}
						}
					}
				}
				return fSortExpression;
			}
			set
			{
				fSortExpression = value;
			}
		}
		string fSortExpression;
	}
}
