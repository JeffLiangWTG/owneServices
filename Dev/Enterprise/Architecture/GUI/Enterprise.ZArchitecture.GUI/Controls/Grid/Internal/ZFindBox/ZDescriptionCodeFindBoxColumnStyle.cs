using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	internal class ZDescriptionCodeFindBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public ZDescriptionCodeFindBoxColumnStyleInfo(PropertyDescriptor propertyDescriptor)
		{
			CharacterCasing = CharacterCasing.Normal;
			((IOverridablePropertyDescriptor)this).PropertyDescriptor = propertyDescriptor;
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZDescriptionCodeFindBoxColumnStyle); }
		}

		public string DescriptionColumnName { get; set; }

		[DefaultValue(CharacterCasing.Normal)]
		public override CharacterCasing CharacterCasing
		{
			get { return base.CharacterCasing; }
			set { base.CharacterCasing = value; }
		}
	}

	internal class ZDescriptionCodeFindBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		#region Constructor

		public ZDescriptionCodeFindBoxColumnStyle(ZDescriptionCodeFindBoxColumnStyleInfo columnInfo)
			: this(() => new ZDescriptionGridFindBox(), columnInfo)
		{
			this.IsSubmissive = true;
		}

		protected ZDescriptionCodeFindBoxColumnStyle(Func<ZDescriptionGridFindBox> gridFindBox, ZDescriptionCodeFindBoxColumnStyleInfo columnInfo)
			: base(gridFindBox, columnInfo)
		{
			this.IsSubmissive = true;
		}

		#endregion

		#region Properties

		public string DescriptionColumnName
		{
			get { return ((ZDescriptionCodeFindBoxColumnStyleInfo)ColumnInfo).DescriptionColumnName; }
		}

		protected ZDescriptionGridFindBox DescriptionGridFindBox
		{
			get { return (ZDescriptionGridFindBox)FindBox; }
		}

		protected IFindBoxListProviderDescriptionEx FindBoxListProviderDescriptionEx
		{
			get { return DescriptionGridFindBox.FindBoxListProviderDescriptionEx; }
		}

		#endregion

		#region Overrides

		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			return initialised ? base.GetColumnValueAtRow(source, rowNum) : "";
		}

		protected override void OnGettingColumnTextAtRow(CurrencyManager source, int rowNum)
		{
			base.OnGettingColumnTextAtRow(source, rowNum);
			initialised = true;
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			var value = base.FormatValueObjectCore(source, propertyValue);
			if (!string.IsNullOrEmpty(value))
			{
				if (value == InvalidCode)
				{
					value = FieldInvalidTextMemory.GetInvalidText(source, PropertyDescriptor.Name);
					if (string.IsNullOrEmpty(value))
					{
						value = Constants.FindBoxMessages.InvalidSelection;
					}
				}
				else
				{
					DescriptionGridFindBox.PullList(source, DataPropertyNameForFindBoxList);
					var bizo = FindBox.ListProvider.GetBusinessObjectFromCode(value);
					if (bizo != null)
					{
						value = bizo[((ZDescriptionCodeFindBoxColumnStyleInfo)ColumnInfo).DescriptionColumnName].ToString();
					}
				}
			}

			return value;
		}

		protected override void UpdateUI(CurrencyManager source, int rowNum, string displayText)
		{
			TextBox.Text = ColumnTextAtRow(source, rowNum);
		}

		protected override void SetValueInFindBox(CurrencyManager source, int rowNum)
		{
			FindBox.Code = ColumnTextAtRow(source, rowNum);
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			initialised = false;
			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
		}

		protected override string DataPropertyNameForFindBoxList
		{
			get { return PropertyDescriptor.Name; }
		}

		protected override object EditValue
		{
			get
			{
				ZString value = FindBox.Code;

				if (!string.IsNullOrEmpty(value))
				{
					var codeFromPopup = ((IFindBoxPopupWithAdvancedCodeStore)FindBox).CodeFromPopup;
					if (value == FindBox.ListProvider.DescriptionFromCode(codeFromPopup))
					{
						value = codeFromPopup;
					}
					else
					{
						var findBoxListProviderDescriptionEx = FindBoxListProviderDescriptionEx;
						if (findBoxListProviderDescriptionEx != null)
						{
							value = findBoxListProviderDescriptionEx.CodeFromDescription(value) ?? InvalidCode;
						}
					}
				}

				return value;
			}
		}

		protected override bool ShouldSetValue(object currentEditValue, CurrencyManager source, int rowNum)
		{
			return
				base.ShouldSetValue(currentEditValue, source, rowNum) ||
				currentEditValue != null && currentEditValue.ToString() == InvalidCode && FieldInvalidTextMemory.GetInvalidText(source.List[rowNum], MappingName) != FindBox.Code;
		}

		protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
		{
			if (value != null && value.ToString() == InvalidCode)
			{
				FieldInvalidTextMemory.SetInvalidText(source.GetCurrent(), PropertyDescriptor.Name, FindBox.Code);
			}

			base.SetColumnValueAtRow(source, rowNum, value);
		}
#if DEBUG
		internal
#endif
		protected override int GetMaxLength(BusinessObject bizObj)
		{
			return ElementType != null ? GetDescriptionSchemaColumnFrom(ElementType).MaxLength : -1;
		}

		protected override bool IsCellReadOnlyCore(object current)
		{
			var currentAsBizo = current as IAccessBusinessObject;
			return base.IsCellReadOnlyCore(current) || (currentAsBizo != null && currentAsBizo.IsPropertyReadOnly(PropertyDescriptor.Name));
		}

		#endregion

		#region Implementation

		public const string ColumnNameSuffix = "_DescriptionVirtual";
		public const string InvalidCode = "?";

		protected bool initialised { get; set; }

		SchemaColumn GetDescriptionSchemaColumnFrom(Type elementType)
		{
			return descriptionSchemaColumn ?? (descriptionSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(
				GetDescriptionPropertyName(elementType), BusinessObjectFactory.GetTableNameFromType(elementType)));
		}
		SchemaColumn descriptionSchemaColumn;

		protected virtual string GetDescriptionPropertyName(Type typeOfElements)
		{
			return DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeOfElements);
		}

		Type ElementType
		{
			get
			{
				if (elementType == null && FindBox.ListProvider.List != null)
				{
					elementType = FindBox.ListProvider.List.TypeOfElements;
				}
				return elementType;
			}
		}
		Type elementType;

		#endregion

		#region Test Stuff
#if DEBUG

		internal bool Initialised4Test
		{
			get { return initialised; }
			set { initialised = value; }
		}

#endif
		#endregion
	}
}
