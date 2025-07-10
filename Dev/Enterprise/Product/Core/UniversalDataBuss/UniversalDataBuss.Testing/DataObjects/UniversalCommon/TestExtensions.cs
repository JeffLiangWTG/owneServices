using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	public static class TestExtensions
	{
		public static void AssertEquals(this ICodeDescriptionDataObject actual, ICodeDescriptionDataObject expected)
		{
			if (expected == null)
			{
				TestCase.AssertNull(actual);
			}
			else
			{
				TestCase.AssertEquals("Code", expected.Code, actual.Code);
				TestCase.AssertEquals("Description", expected.Description, actual.Description);
			}
		}

		public static CustomizedField AssertCustomFieldWasExported(this List<CustomizedField> customizedFieldsDataObjects, DataType expectedDataType, ZString? expectedKey, ZString? expectedValue)
		{
			var customizedField = customizedFieldsDataObjects.Find(field => field.DataType == expectedDataType && field.Key == expectedKey && field.Value == expectedValue);
			TestCase.AssertNotNull(string.Format("Customized Field of type: '{0}' with name: '{1}' and with value: '{2}' was not found.", expectedDataType.ToString(), expectedKey, expectedValue), customizedField);
			return customizedField;
		}

		public static Date AssertDateExists(this List<Date> dateCollection, DateType dateType, ZBool isEstimate, UXmlDateTime expectedValue)
		{
			var date = dateCollection.Find(findDate => { return findDate.Type == dateType && findDate.IsEstimate == isEstimate; });
			TestCase.AssertNotNull("Cound not find Date with Type: [" + dateType.ToString() + "], IsEstimate: [" + isEstimate.ToString() + "].", date);
			if (date != null)
			{
				dateCollection.Remove(date);
				TestCase.AssertEquals("Value from Date with Type: [" + dateType.ToString() + "], IsEstimate: [" + isEstimate.ToString() + "].", expectedValue, date.Value);
			}
			return date;
		}

		public static string FormatAndOrderImportAttempts(this IEnumerable<IImportResult> attemptedImports)
		{
			return string.Join("\r\n-----<<<<NEXT>>>>-----\r\n", attemptedImports.Select(a => $"{a.WasSuccessful}|{a.ToString()}|{FormatDataSource(a)}").OrderBy(o => o).ToArray());
		}

		static string FormatDataSource(IImportResult importResult)
		{
			var result = string.Empty;
			try
			{
				result = importResult.DataContextType.ToString();
			}
			catch (InvalidOperationException)
			{
				result = "NULL";
			}

			var key = importResult.DataContextKey;
			if (!string.IsNullOrEmpty(key))
			{
				result = string.Format("{0}-{1}", result, key);
			}

			return result;
		}

		public static IColumnIndexer Row(this IBusinessObjectInternals bizO)
		{
			return bizO.Row as IColumnIndexer;
		}
	}
}
