using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using WTG.StaticAnalysis.Annotation;
using PropertyList = System.Collections.Generic.ICollection<(System.Reflection.PropertyInfo PropertyInfo, Enterprise.Customs.IE.Messaging.XlsxFieldAttribute XlsxField)>;

namespace Enterprise.Customs.IE.Business;

public static class XlsConverter
{
	public static Stream ToXlsxStream(IXlsxProvider provider, string description, out TFileFormats fileFormat)
	{
		var xlsFile = new XlsFile();
		xlsFile.NewFile(1, TExcelFileFormat.v2010);
		int row = 1, column = 1;
		WriteObject(xlsFile, ref row, ref column, provider);

		for (var col = 1; col < xlsFile.ColCount; col++)
		{
			xlsFile.AutofitCol(col, ignoreStrings: false, adjustment: 1.1);
		}
		xlsFile.SheetName = description;

		var result = new MemoryStream();
		xlsFile.Save(result, fileFormat = TFileFormats.Xlsx);
		result.Position = 0;

		return result;
	}

	static void WriteObject(this XlsFile xlsFile, ref int row, ref int column, IXlsxProvider messageProvider, int indexInCollection = -1, int collectionCount = -1)
	{
		var columnStart = column;

		var type = messageProvider.GetType();
		var xlsFields = type.GetXlsFields();

		var simpleProperties = xlsFields.Where(p => p.PropertyInfo.IsSimple()).OrderBy(p => p.XlsxField.Order).ToArray();
		var collectionProperties = xlsFields.Except(simpleProperties).OrderBy(p => p.XlsxField.Order)
			.Select(collectionProperty => new
			{
				collectionProperty.PropertyInfo,
				collectionProperty.XlsxField,
				Values = (collectionProperty.PropertyInfo.GetValue(messageProvider) as IEnumerable).Cast<object>().ToArray()
			}).Where(v => v.Values.Length > 0).ToArray();

		for (var simplePropertyIndex = 0; simplePropertyIndex < simpleProperties.Length; simplePropertyIndex++)
		{
			var simpleProperty = simpleProperties[simplePropertyIndex];
			if (indexInCollection < 1)
			{
				xlsFile.SetHeaderFormat(row, column);
				xlsFile.SetCellValue(row, column, simpleProperty.XlsxField.DisplayName);
			}
			row++;
			xlsFile.SetValueCellFormat(row, column, isPrimaryValue: simplePropertyIndex == 0, isLastInList: indexInCollection == collectionCount - 1);
			xlsFile.SetCellValue(row, column, simpleProperty.PropertyInfo.GetValue(messageProvider));
			column++;
			if (simplePropertyIndex < simpleProperties.Length - 1)
			{
				row--;
			}
		}

		column = columnStart;

		var collectionHeadersNeeded = simpleProperties.Length > 0 ? collectionProperties.Length > 0 : collectionProperties.Length > 1;
		for (var collectionPropertyIndex = 0; collectionPropertyIndex < collectionProperties.Length; collectionPropertyIndex++)
		{
			var collectionProperty = collectionProperties[collectionPropertyIndex];

			if (collectionHeadersNeeded && collectionProperty.Values.Length > 0)
			{
				row += 2;
				xlsFile.SetCellValue(row, column, collectionProperty.XlsxField.DisplayName);
			}
			row += 2;
			for (var collectionValueIndex = 0; collectionValueIndex < collectionProperty.Values.Length; collectionValueIndex++)
			{
				xlsFile.WriteObject(ref row, ref column, (IXlsxProvider)collectionProperty.Values[collectionValueIndex], indexInCollection: collectionValueIndex, collectionCount: collectionProperty.Values.Length);
			}
			column = columnStart;
		}
	}

	#region Message Object properties

	static bool IsSimple(this PropertyInfo propertyInfo)
	{
		var propertyType = propertyInfo.PropertyType;
		if (typeof(IZType).IsAssignableFrom(propertyType) || propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(decimal) || propertyType == typeof(DateTime))
		{
			return true;
		}
		else if (propertyType.IsGenericType && typeof(IReadOnlyCollection<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()))
		{
			return false;
		}
		else
		{
			throw new InvalidOperationException($"Xlsx Field: {propertyInfo.Name}: type need to be either one of Primitive, string, decimal, DateTime or IReadOnlyCollection. Actual: {propertyType.FullName}");
		}
	}

	[ThreadSafe]
	static readonly Lazy<Dictionary<Type, PropertyList>> xlsxFieldDictionaryLazy = new Lazy<Dictionary<Type, PropertyList>>(() => new Dictionary<Type, PropertyList>());
	static Dictionary<Type, PropertyList> XlsxFieldDictionary => xlsxFieldDictionaryLazy.Value;
	static PropertyList GetXlsFields(this Type messageProviderType) => typeof(IXlsxProvider).IsAssignableFrom(messageProviderType)
		? XlsxFieldDictionary.GetOrAdd(messageProviderType, messageProviderType.GetNewXlsFields)
		: null;

	static PropertyList GetNewXlsFields(this Type messageProviderType)
	{
		static (PropertyInfo PropertyInfo, XlsxFieldAttribute XlsxField) GetXlsField(PropertyInfo propertyInfo) => (propertyInfo, propertyInfo.GetCustomAttribute<XlsxFieldAttribute>());

		if (typeof(IXlsxProvider).IsAssignableFrom(messageProviderType))
		{
			var properties = messageProviderType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(GetXlsField);
			return IEnumerableExtensions.DistinctBy(properties.Where(p => p.XlsxField != null), p => p.PropertyInfo)
				.OrderBy(p => p.XlsxField.Order)
				.ToArray();
		}
		else
		{
			return null;
		}
	}

	#endregion

	#region Cell Formats

	static void SetHeaderFormat(this XlsFile file, int row, int column) => SetCellFormat(
			file, row, column,
			textColor: Color.White, backgroundColor: Color.FromArgb(255, 14, 40, 65),
			TFlxBorderStyle.Medium, TFlxBorderStyle.Medium, TFlxBorderStyle.Medium, TFlxBorderStyle.Thick
	);

	static void SetValueCellFormat(this XlsFile file, int row, int column, bool isPrimaryValue = false, bool isLastInList = false)
	{
		if (isPrimaryValue)
		{
			SetCellFormat(
				file, row, column,
				textColor: Color.Black, backgroundColor: Color.FromArgb(255, 183, 212, 239),
				 TFlxBorderStyle.Medium, TFlxBorderStyle.None, TFlxBorderStyle.Medium, TFlxBorderStyle.Thick
			);
		}
		else
		{
			SetCellFormat(
				file, row, column,
				textColor: Color.Black, backgroundColor: Color.White,
				TFlxBorderStyle.Medium, TFlxBorderStyle.None, TFlxBorderStyle.Medium, isLastInList ? TFlxBorderStyle.Medium : TFlxBorderStyle.Thick
			);
		}
	}

	static void SetCellFormat(XlsFile xlsFile, int row, int column, TExcelColor textColor, TExcelColor backgroundColor, TFlxBorderStyle? left = null, TFlxBorderStyle? top = null, TFlxBorderStyle? right = null, TFlxBorderStyle? bottom = null)
	{
		var format = xlsFile.GetCellVisibleFormatDef(row, column);
		format.FillPattern.Pattern = TFlxPatternStyle.Solid;
		format.FillPattern.FgColor = backgroundColor;
		format.Font.Color = textColor;
		format.Font.Size20 = 220;

		format.Borders.Left.Style = left ?? TFlxBorderStyle.Thin;
		format.Borders.Left.Color = TExcelColor.Automatic;
		format.Borders.Top.Style = top ?? TFlxBorderStyle.Thin;
		format.Borders.Top.Color = TExcelColor.Automatic;
		format.Borders.Right.Style = right ?? TFlxBorderStyle.Thin;
		format.Borders.Right.Color = TExcelColor.Automatic;
		format.Borders.Bottom.Style = bottom ?? TFlxBorderStyle.Thin;
		format.Borders.Bottom.Color = TExcelColor.Automatic;

		xlsFile.SetCellFormat(row, column, xlsFile.AddFormat(format));
	}

	#endregion
}
