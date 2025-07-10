using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	[TypeConverter(typeof(ColumnInfoConverter))]
	public abstract class ZGridColumnInfo : IBindingMemberForCompileTimeCheckProvider, IResCaptionedControl
	{
		public ZGridColumnInfo(string columnName, int width)
			: this(columnName, width, CharacterCasing.Upper, true, false, false, true)
		{
		}

		public ZGridColumnInfo(string columnName, int width, CharacterCasing characterCasing, bool visible, bool mandatory, bool readOnly, bool sortable)
		{
			this.ColumnName = columnName;
			ControlDpiScalingHelper.SetWidth(this, width, false);
			this.CharacterCasing = characterCasing;
			this.IsVisible = visible;
			this.IsMandatory = mandatory;
			this.IsReadOnly = readOnly;
			this.IsSortable = sortable;
		}

		public ZGridColumnInfo()
		{
		}

		public override string ToString()
		{
			return ColumnName + " (" + GetType().Name + ")";
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public abstract Type ColumnStyleType { get; }

		#region Caption

		[DefaultValue(null)]
		public string Caption
		{
			get;
			set;
		}

		[Browsable(true)]
		public ResourceStringData CaptionResourceString
		{
			get { return captionResourceString ?? ResourceStringData.Empty; }
			set
			{
#if DEBUG
				if (DesignModeFinder.IsDesigning && value == null)
				{
					return;
				}
#endif
				captionResourceString = value;
			}
		}
		ResourceStringData captionResourceString;

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

		#endregion

		#region ToolTip

		[DefaultValue("")]
		public string ToolTip
		{
			get { return toolTip; }
			set { toolTip = value; }
		}

		string toolTip = "";

		#endregion

		#region IsCustomColumn

		[DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsCustomColumn { get; set; }

		#endregion

		#region ColumnName

		[DefaultValue("")]
		public virtual string ColumnName
		{
			get { return columnName; }
			set
			{
				columnName = value;
				if (columnName != null)
				{
					columnName = columnName.Replace(".", "+");
				}
			}
		}

		string columnName = "";

		#endregion

		#region Width

		public int Width
		{
			get { return width; }
			set { width = value; }
		}

		int width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

		#endregion

		#region IsMandatory

		[DefaultValue(false)]
		public bool IsMandatory
		{
			get { return mandatory; }
			set { mandatory = value; }
		}

		bool mandatory;

		#endregion

		#region IsVisible

		[DefaultValue(true)]
		public bool IsVisible
		{
			get { return visible; }
			set { visible = value; }
		}

		bool visible = true;

		#endregion

		#region IsReadOnly

		[DefaultValue(false)]
		public bool IsReadOnly
		{
			get { return readOnly; }
			set { readOnly = value; }
		}

		bool readOnly;

		#endregion

		#region IsSortable

		[DefaultValue(true)]
		public bool IsSortable
		{
			get { return sortable && !IsSensitiveValue; }
			set { sortable = value; }
		}

		bool sortable = true;

		#endregion

		#region CharacterCasing

		public virtual CharacterCasing CharacterCasing
		{
			get { return characterCasing ?? default(CharacterCasing); }
			set { characterCasing = value; }
		}

		bool ShouldSerializeCharacterCasing()
		{
			return characterCasing.HasValue;
		}

		CharacterCasing? characterCasing;

		#endregion

		#region GroupName

		[Browsable(true)]
		public ResourceStringData GroupName
		{
			get { return groupName ?? ResourceStringData.Empty; }
			set
			{
#if DEBUG
				if (DesignModeFinder.IsDesigning && value == null)
				{
					return;
				}
#endif
				groupName = value;
			}
		}

		ResourceStringData groupName;

		bool ShouldSerializeGroupName()
		{
			return !GroupName.IsEmpty();
		}

		#endregion

		#region IsUnavailable

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsUnavailable
		{
			get { return unavailable; }
			set { unavailable = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ErrorMessageWhenUnavailable
		{
			get { return errorMessageWhenUnavailable; }
			set { errorMessageWhenUnavailable = value; }
		}

		bool unavailable;
		string errorMessageWhenUnavailable = "";

		#endregion

		#region ColumnComparer

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZGridColumn.CustomColumnComparer ColumnComparer { get; set; }

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
			{
				var attr = (ZColumnBindingMemberTypeAttribute)property.Attributes[typeof(ZColumnBindingMemberTypeAttribute)];
				if (attr != null)
				{
					var value = property.GetValue(this);
					var columnMember = value as string;
					if (!string.IsNullOrEmpty(columnMember))
					{
						result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, attr.Type, new KBindingMemberInfo(dataMember, columnMember).BindingMember));
					}
				}
			}
			return result;
		}

		#endregion

		#region ISubmissiveColumn

		internal bool IsSubmissive { get; set; }

		#endregion

		#region IsSensitiveValue
		public virtual bool IsSensitiveValue => false;

		#endregion

	}
}
