using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.Excel
{
	#region Excel Export Column

	public class ExcelExportColumn : ExcelExportColumnBase
	{
		#region Static Constructors

		public static ExcelExportColumn New(SchemaColumn column, IExcelExportCustomFunction gridBaseColumn = null)
		{
			var commentSupport = gridBaseColumn as IExcelExportCellComment;
			var colorSupport = gridBaseColumn as IExcelExportCellColor;

			var multiColumn = column as SchemaMultiColumn;
			if (multiColumn != null)
			{
				return new ExcelExportMultiColumn(multiColumn, commentSupport, colorSupport);
			}
			else
			{
				switch (column.ColumnType)
				{
					case SchemaColumnType.DateTimeOffset:
						return new ExcelExportDateTimeOffsetColumn((SchemaDateTimeOffsetColumn)column, commentSupport, colorSupport);

					case SchemaColumnType.DateTime:
						return new ExcelExportDateTimeColumn((SchemaDateTimeColumn)column, commentSupport, colorSupport);

					case SchemaColumnType.Date:
						return new ExcelExportDateColumn((SchemaDateColumn)column, commentSupport, colorSupport);

					case SchemaColumnType.Decimal:
						return new ExcelExportDecimalColumn((SchemaDecimalColumn)column, commentSupport, colorSupport);

					case SchemaColumnType.Guid:
						return new ExcelExportGuidColumn((SchemaGuidColumn)column, commentSupport, colorSupport);

					case SchemaColumnType.Time:
						return new ExcelExportTimeColumn((SchemaTimeColumn)column, commentSupport, colorSupport);

					default:
						return new ExcelExportColumn(column, commentSupport, colorSupport);
				}
			}
		}

		public static ExcelExportColumn New(SchemaColumn column, string description, IExcelExportCustomFunction gridBaseColumn = null)
		{
			var result = ExcelExportColumn.New(column, gridBaseColumn);

			result.Description = description;
			return result;
		}

		#endregion

		#region Protected Constructors

		protected ExcelExportColumn(SchemaColumn column, IExcelExportCellComment commentSupport = null, IExcelExportCellColor colorSupport = null)
			: base(commentSupport, colorSupport)
		{
			Argument.NotNull(column, "Column");

			this.SchemaColumn = column;
		}

		#endregion

		protected override string GetDescription()
		{
			return DataBoundResourceStrings.GetColumnDescriptiveName(SchemaColumn.TableName, SchemaColumn.Name);
		}

		public override CellFormat GetFormat(IZType value)
		{
			return Format;
		}

		#region Format

		public CellFormat Format
		{
			get
			{
				if (fFormat == null)
				{
					fFormat = new CellFormat();
				}
				fFormat.FormatPattern = FormatPattern;
				return fFormat;
			}
		}

		protected virtual ZString FormatPattern
		{
			get { return ""; }
		}

		CellFormat fFormat;

		#endregion

		bool? IsPassword { get; set; }

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			if (IsPassword == null)
			{
				var propertyDescriptor = bizObj.FindPropertyInfo(SchemaColumn.Name)?.PropertyDescriptor;
				IsPassword = propertyDescriptor != null && MetaData.GetPassword(bizObj, propertyDescriptor);
			}
			if (IsPassword == true) // this is a bool?
			{
				return new ZString("***"); // programmatic constant;
			}

			return (IZType)bizObj[SchemaColumn.Name];
		}

		public readonly SchemaColumn SchemaColumn;
	}

	#endregion

	#region Excel Export Column (Date)

	public class ExcelExportDateColumn : ExcelExportColumn
	{
		public ExcelExportDateColumn(SchemaDateColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var tempValue = base.GetValueForExportCore(bizObj);
			IZType result;

			if (tempValue == null)
			{
				result = ZString.Empty;
			}
			else if (!(tempValue is ZDate))
			{
				result = tempValue;
				ErrorReporter.ReportOnce("ExcelExportDateColumn - " + SchemaColumn.Name, GetUnableToFormatValueErrorMessage(tempValue));
			}
			else
			{
				var value = (ZDate)tempValue;
				if (value.IsValid)
				{
					result = tempValue;
				}
				else
				{
					result = ZString.Empty;
				}
			}

			return result;
		}

		protected override ZString FormatPattern
		{
			get { return ZDateTime.ShortDateFormat; }
		}
	}

	#endregion

	#region Excel Export Column (DateTime)

	public class ExcelExportDateTimeColumn : ExcelExportColumn
	{
		public ExcelExportDateTimeColumn(SchemaDateTimeColumn column)
			: this(column, ZDateTimePickerFormat.Long)
		{
		}

		public ExcelExportDateTimeColumn(SchemaDateTimeColumn column, ZDateTimePickerFormat format)
			: base(column)
		{
			this.DateTimeFormat = format;
		}

		public ExcelExportDateTimeColumn(SchemaDateTimeColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
			if (column.SqlDbType == SqlDbType.Time)
			{
				this.DateTimeFormat = ZDateTimePickerFormat.TimeIncludingSeconds;
			}
			else
			{
				this.DateTimeFormat = ZDateTimePickerFormat.Long;
			}
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var tempValue = base.GetValueForExportCore(bizObj);
			IZType result;

			if (tempValue == null)
			{
				result = ZString.Empty;
			}
			else if (!(tempValue is ZDateTime))
			{
				result = tempValue;
				ErrorReporter.ReportOnce("ExcelExportDateTimeColumn - " + SchemaColumn.Name, GetUnableToFormatValueErrorMessage(tempValue));
			}
			else
			{
				var value = (ZDateTime)tempValue;
				if (value.IsValid)
				{
					if (DateTimeFormat == ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes)
					{
						result = (ZString)ZTimeEditExHelper.GetTextFromTime(value, true);
					}
					else
					{
						result = tempValue;
					}
				}
				else
				{
					result = ZString.Empty;
				}
			}

			return result;
		}

		protected override ZString FormatPattern
		{
			get
			{
				ZString result = "";

				if (DateTimeFormat == ZDateTimePickerFormat.Short)
				{
					result = ZDateTime.ShortDateFormat;
				}
				else if (DateTimeFormat == ZDateTimePickerFormat.Time)
				{
					result = ZDateTime.ShortTimeFormat;
				}
				else if (DateTimeFormat == ZDateTimePickerFormat.TimeIncludingSeconds)
				{
					result = DateTimeFormatStrings.ShortTimeIncludingSecondsFormat;
				}
				else if (DateTimeFormat != ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes)
				{
					result = ZDateTime.LongTimeFormat;
				}

				return result;
			}
		}

		ZTimeEditExHelper ZTimeEditExHelper
		{
			get
			{
				if (fZTimeEditExHelper == null)
				{
					fZTimeEditExHelper = new ZTimeEditExHelper();
				}
				return fZTimeEditExHelper;
			}
		}

		ZTimeEditExHelper fZTimeEditExHelper;

		public ZDateTimePickerFormat DateTimeFormat;
	}

	#endregion

	#region Excel Export Column (DateTimeOffset)

	public class ExcelExportDateTimeOffsetColumn : ExcelExportColumn
	{
		public ExcelExportDateTimeOffsetColumn(SchemaDateTimeOffsetColumn column)
			: base(column)
		{
		}

		public ExcelExportDateTimeOffsetColumn(SchemaDateTimeOffsetColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var tempValue = base.GetValueForExportCore(bizObj);
			IZType result;

			if (tempValue == null)
			{
				result = ZString.Empty;
			}
			else if (!(tempValue is ZDateTimeOffset))
			{
				result = tempValue;
				ErrorReporter.ReportOnce("ExcelExportDateTimeOffsetColumn - " + SchemaColumn.Name, GetUnableToFormatValueErrorMessage(tempValue));
			}
			else
			{
				var value = (ZDateTimeOffset)tempValue;
				if (value.IsValid)
				{
					result = value.ToZDateTime();
				}
				else
				{
					result = ZString.Empty;
				}
			}

			return result;
		}

		protected override ZString FormatPattern
		{
			get
			{
				return ZDateTime.LongTimeFormat;
			}
		}
	}

	#endregion

	#region Excel Export Column (Time)

	public class ExcelExportTimeColumn : ExcelExportColumn
	{
		public ExcelExportTimeColumn(SchemaTimeColumn column)
			: base(column)
		{
		}

		public ExcelExportTimeColumn(SchemaTimeColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var tempValue = base.GetValueForExportCore(bizObj);
			IZType result;

			if (tempValue == null)
			{
				result = ZString.Empty;
			}
			else if (!(tempValue is ZTime))
			{
				result = tempValue;
				ErrorReporter.ReportOnce("ExcelExportTimeColumn - " + SchemaColumn.Name, GetUnableToFormatValueErrorMessage(tempValue));
			}
			else
			{
				var value = (ZTime)tempValue;
				if (value.IsValid)
				{
					result = tempValue;
				}
				else
				{
					result = ZString.Empty;
				}
			}

			return result;
		}

		protected override ZString FormatPattern
		{
			get
			{
				return ZTime.TimeFormat;
			}
		}

		public ZDateTimePickerFormat DateTimeFormat;
	}

	#endregion

	#region Excel Export Column (Geography)

	public class ExcelExportGeographyColumn : ExcelExportColumn
	{
		public ExcelExportGeographyColumn(SchemaGeographyColumn column)
			: base(column)
		{
		}

		public ExcelExportGeographyColumn(SchemaGeographyColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var tempValue = base.GetValueForExportCore(bizObj);
			IZType result;

			if (tempValue == null)
			{
				result = ZString.Empty;
			}
			else if (!(tempValue is ZGeography))
			{
				result = tempValue;
				ErrorReporter.ReportOnce("ExcelExporGeographyColumn - " + SchemaColumn.Name, GetUnableToFormatValueErrorMessage(tempValue));
			}
			else
			{
				var value = (ZGeography)tempValue;
				if (value.IsValid)
				{
					result = new ZString(value.ToString());
				}
				else
				{
					result = ZString.Empty;
				}
			}

			return result;
		}
	}

	#endregion

	#region Excel Export Column (Decimal)

	public class ExcelExportDecimalColumn : ExcelExportColumn
	{
		public ExcelExportDecimalColumn(SchemaDecimalColumn column)
			: this(column, ExcelExportDecimalColumn.DefaultDecimals)
		{
		}

		public ExcelExportDecimalColumn(SchemaDecimalColumn column, int decimals)
			: this(column, decimals, null, null)
		{
		}

		public ExcelExportDecimalColumn(SchemaDecimalColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: this(column, ExcelExportDecimalColumn.DefaultDecimals, commentSupport, colorSupport)
		{
		}

		public ExcelExportDecimalColumn(SchemaDecimalColumn column, int decimals, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
			if (decimals < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(decimals), decimals, "Decimals cannot be less than 0.");
			}
			this.Decimals = decimals;
		}

		protected override ZString FormatPattern
		{
			get { return (Decimals == 0) ? "#,#" : "#,##0." + new ZString('0', Decimals); }
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var value = base.GetValueForExportCore(bizObj);
			if (!(value is ZDecimal))
			{
				ErrorReporter.ReportOnce("ExcelExportDecimalColumn - " + SchemaColumn.Name, GetUnableToFormatValueErrorMessage(value));
			}

			return value;
		}

		public int Decimals;
		const int DefaultDecimals = 2;
	}

	#endregion

	#region Excel Export Column (Guid)

	public class ExcelExportGuidColumn : ExcelExportColumn
	{
		public ExcelExportGuidColumn(SchemaGuidColumn column)
			: base(column)
		{
		}

		public ExcelExportGuidColumn(SchemaGuidColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
		}

		protected ExcelExportGuidColumn(SchemaColumn column)
			: base(column)
		{
		}

		protected ExcelExportGuidColumn(SchemaColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var value = base.GetValueForExportCore(bizObj);
			if (value is ZGuid)
			{
				try
				{
					return GetValueFromPropertyAttribute(bizObj);
				}
				catch (InvalidOperationException)
				{
					//List property does not exist
					//Some related object in property path is null
					return null;
				}
			}
			else
			{
				ErrorReporter.ReportOnce("ExcelExportGuidColumn - " + SchemaColumn.Name, GetUnableToFormatValueErrorMessage(value));
				return value;
			}
		}

		IZType GetValueFromPropertyAttribute(BusinessObject bizObj)
		{
			IZType result = ZString.Empty;

			var info = bizObj.ZPropertyInfoHash[SchemaColumn.Name];

			if (HasRelatedBusinessObject(info))
			{
				switch (DisplayStyle)
				{
					case OComboBoxDropDownStyle.CodeOnly:
						result = RelatedBusinessObjectAttribute.GetCodeForGuid(info);
						break;
					case OComboBoxDropDownStyle.DescriptionOnly:
						result = RelatedBusinessObjectAttribute.GetDescriptionForGuid(info);
						break;
				}
			}

			if (result.Equals(ZString.Empty) && ColumnReferencesCodeDescriptionPairList(info))
			{
				var currentListItemPK = (ZGuid)info.PropertyDescriptor.GetValue(bizObj);
				result = GetValueFromCurrentCodeDescriptionPairListItem(currentListItemPK);
			}
			return result;
		}

		bool HasRelatedBusinessObject(ZPropertyInfo info)
		{
			return info.PropertyDescriptor.Attributes[typeof(RelatedBusinessObjectAttribute)] != null;
		}

		public OComboBoxDropDownStyle DisplayStyle
		{
			get { return fDisplayStyle; }
			set { fDisplayStyle = value; }
		}
		OComboBoxDropDownStyle fDisplayStyle = OComboBoxDropDownStyle.CodeOnly;

		bool ColumnReferencesCodeDescriptionPairList(ZPropertyInfo propertyInfo)
		{
			var listAttribute = propertyInfo.PropertyDescriptor.Attributes[typeof(ListAttribute)];
			if (listAttribute != null)
			{
				columnListDataSource = GetCodeDescriptionPairListFromListAttribute((ListAttribute)listAttribute, propertyInfo);
				return columnListDataSource != null;
			}
			return false;
		}

		CodeDescriptionPairList GetCodeDescriptionPairListFromListAttribute(ListAttribute listAttribute, ZPropertyInfo propertyInfo)
		{
			var listPropertyName = listAttribute.ListDataSourceMember;
			return GetCodeDescriptionPairList(listPropertyName, propertyInfo.BizObj);
		}

		CodeDescriptionPairList GetCodeDescriptionPairList(string listPropertyName, object instance)
		{
			var propertyArrary = listPropertyName.Split(new[] { '+', '.' });
			var result = instance;

			for (var i = 0; i < propertyArrary.Length; i++)
			{
				var propertyName = propertyArrary[i];
				result = GetPropertyValue(result, propertyName);

				if (result == null)
				{
					if (i == propertyArrary.Length - 1)
					{
						return null;
					}

					throw new InvalidOperationException(string.Format("Cannot get value of {0} on object of type {1} because {2} is null",
						listPropertyName,
						instance.GetType().Name,
						propertyName));
				}
			}

			return result as CodeDescriptionPairList;
		}

		object GetPropertyValue(object instance, string name)
		{
			var propertyInfo = instance.GetType().GetProperty(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ??
				instance.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
					.FirstOrDefault(property => property.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					?? throw new InvalidOperationException(string.Format("Property {0} not found in type {1}", name, instance.GetType()));

			return propertyInfo.GetValue(instance);
		}

		CodeDescriptionPairList columnListDataSource;

		IZType GetValueFromCurrentCodeDescriptionPairListItem(ZGuid currentItemPK)
		{
			switch (DisplayStyle)
			{
				case OComboBoxDropDownStyle.CodeOnly:
					return GetCodeForCurrentCodeDescriptionPairListItem(currentItemPK);
				case OComboBoxDropDownStyle.DescriptionOnly:
					return GetDescriptionForCurrentCodeDescriptionPairListItem(currentItemPK);
				default:
					return ZString.Empty;
			}
		}

		IZType GetCodeForCurrentCodeDescriptionPairListItem(ZGuid currentItemPK)
		{
			return (ZString)columnListDataSource[currentItemPK].Code;
		}

		IZType GetDescriptionForCurrentCodeDescriptionPairListItem(ZGuid currentItemPK)
		{
			return (ZString)columnListDataSource[currentItemPK].Description;
		}
	}

	#endregion

	#region Excel Export Column (Multi)

	public class ExcelExportMultiColumn : ExcelExportColumn
	{
		public ExcelExportMultiColumn(SchemaMultiColumn column)
			: base(column)
		{
		}

		public ExcelExportMultiColumn(SchemaMultiColumn column, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(column, commentSupport, colorSupport)
		{
		}

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var propInfo = bizObj.ZPropertyInfoHash[SchemaColumn.Name];
			IZType result;
			try
			{
				var valueAsGuid = new ZGuid(propInfo.Value.ToString());
				result = RelatedBusinessObjectAttribute.GetCodeForGuid(propInfo);
			}
			catch (FormatException)
			{
				result = (ZString)propInfo.Value.ToString();
			}

			return result;
		}
	}

	#endregion
}
