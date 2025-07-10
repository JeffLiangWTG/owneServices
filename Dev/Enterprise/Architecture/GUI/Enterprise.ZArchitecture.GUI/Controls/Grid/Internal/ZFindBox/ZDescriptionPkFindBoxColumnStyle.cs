using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.GUI
{
	class ZDescriptionPkFindBoxColumnStyleInfo : ZDescriptionCodeFindBoxColumnStyleInfo
	{
		public ZDescriptionPkFindBoxColumnStyleInfo(PropertyDescriptor propertyDescriptor)
			: base(propertyDescriptor)
		{
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get
			{
				return typeof(ZDescriptionPkFindBoxColumnStyle);
			}
		}
	}

	class ZDescriptionPkFindBoxColumnStyle : ZDescriptionCodeFindBoxColumnStyle
	{
		#region Constructor
		public ZDescriptionPkFindBoxColumnStyle(ZDescriptionPkFindBoxColumnStyleInfo columnInfo)
			: base(columnInfo)
		{
		}
		#endregion

		#region Overrides

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
				if (value == ZGuid.Invalid.ToString())
				{
					value = FieldInvalidTextMemory.GetInvalidText(source, PropertyDescriptor.Name);
					if (string.IsNullOrEmpty(value))
					{
						value = Constants.FindBoxMessages.InvalidSelection;
					}
				}
				else if (value == ZGuid.Empty.ToString())
				{
					return string.Empty;
				}
				else
				{
					DescriptionGridFindBox.PullList(source, DataPropertyNameForFindBoxList);
					var bizo = FindBox.ListProvider.GetBusinessObjectFromCode(FindBox.ListProvider.CodeFromPrimaryKey(new ZGuid(value)));
					if (bizo != null)
					{
						value = bizo[((ZDescriptionPkFindBoxColumnStyleInfo)ColumnInfo).DescriptionColumnName].ToString();
					}
				}
			}

			return value;
		}

		protected override object EditValue
		{
			get
			{
				ZString code = FindBox.Code;
				var value = ZGuid.Empty;
				if (!string.IsNullOrEmpty(code))
				{
					var codeFromPopup = ((IFindBoxPopupWithAdvancedCodeStore)FindBox).CodeFromPopup;
					if (code == FindBox.ListProvider.DescriptionFromCode(codeFromPopup))
					{
						value = FindBox.ListProvider.PrimaryKeyFromCode(codeFromPopup);
					}
					else
					{
						var findBoxListProviderDescriptionEx = FindBoxListProviderDescriptionEx;
						if (findBoxListProviderDescriptionEx != null)
						{
							code = findBoxListProviderDescriptionEx.CodeFromDescription(code);
							if (!code.IsEmpty)
							{
								value = FindBox.ListProvider.PrimaryKeyFromCode(code);
							}
						}
					}
				}
				return value;
			}
		}
		#endregion
	}
}
