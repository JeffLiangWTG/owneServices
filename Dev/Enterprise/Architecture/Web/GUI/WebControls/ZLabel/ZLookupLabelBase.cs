using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Base class for labels with look up lists
	/// </summary>
	public abstract class ZLookupLabelBase : ZLabelBase, IBindToList, IFetchHintGenerator
	{
		#region Display style 

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(OComboBoxDropDownStyle.CodeOnly)]
		public OComboBoxDropDownStyle DisplayStyle
		{
			get { return fDisplayStyle; }
			set { fDisplayStyle = value; }
		}
		protected OComboBoxDropDownStyle fDisplayStyle;

		#endregion

		public abstract string BindToList { get; set; }

		protected override void SetTextProperty(object dataSource)
		{
			IZType propertyValue = GetPropertyValueObject(dataSource);
			object list = GetPropertyValueObjectList(dataSource);
			Text = GetText(list, propertyValue);
			ToolTip = GetToolTip(list, propertyValue);
		}

		string GetText(object list, IZType propertyValue)
		{
			return GetTextFromProperty(list, propertyValue, DisplayStyle);
		}

		string GetToolTip(object list, IZType propertyValue)
		{
			return GetTextFromProperty(list, propertyValue, OComboBoxDropDownStyle.DescriptionOnly);
		}

		object GetPropertyValueObjectList(object dataSource)
		{
			string listMember = MetadataHelper.GetListMember(this, dataSource);
			return ZPropertyAccessor.Get(dataSource, listMember);
		}

		string GetTextFromProperty(object list, IZType value, OComboBoxDropDownStyle style)
		{
			if (value.IsValid)
			{
				switch (style)
				{
					case OComboBoxDropDownStyle.CodeOnly: return GetCode(list, value);
					case OComboBoxDropDownStyle.DescriptionOnly: return GetDescription(list, value);
					case OComboBoxDropDownStyle.CodeAndDescription:
						{
							string desc = GetDescription(list, value);
							string code = GetCode(list, value);
							if (((ZString)desc != "") || ((ZString)code != ""))
							{
								return String.Format("{0} ({1})", GetDescription(list, value), GetCode(list, value));
							}
							else
							{
								return ZString.Empty;
							}
						}
				}
			}
			return "";
		}

		protected abstract string GetCode(object list, IZType value);
		protected abstract string GetDescription(object list, IZType value);

		#region IFetchHintGenerator Members

		void IFetchHintGenerator.AddFetchHint(object dataSource, string dataMember)
		{
			if (string.IsNullOrEmpty(dataMember) || dataMember == BindTo)
			{
				GetFetchHintHandler(this, dataSource).Add();
			}
		}

		protected virtual ZCodeFindBoxFetchHintHandler GetFetchHintHandler(IBindToList control, object dataSource)
		{
			return new WebCodeFindBoxFetchHintHandler(control, dataSource);
		}

		class WebCodeFindBoxFetchHintHandler : ZCodeFindBoxFetchHintHandler
		{
			public WebCodeFindBoxFetchHintHandler(IBindToList control, object dataSource)
				: base(control, dataSource)
			{
			}

			protected override string BindingMember
			{
				get { return ((IBindTo)Control).BindTo; }
			}
		}

		#endregion
	}
}
