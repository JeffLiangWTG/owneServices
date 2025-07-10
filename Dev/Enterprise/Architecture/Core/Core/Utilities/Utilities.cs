using System;
using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Jint;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public static class Utilities
	{
		#region Conversions, Comparisons and Validation

		public readonly static Guid InvalidSelectionGuid = new Guid("0000ffff-ffff-ffff-ffff-ffffffffffff");
		public readonly static Guid MissingRecordGuid = new Guid("1111ffff-ffff-ffff-ffff-ffffffffffff");

		public static object GetValidValue(object input, Type inputType)
		{
			object result;

			if (input is DBNull)
			{
				result = NullType.GetNullObject(inputType);
			}
			else
			{
				result = input;
			}

			return result;
		}

		public static Guid GetGuidFromObject(object value)
		{
			Guid convertedGuid;

			if (value == null || (value is string) && string.IsNullOrWhiteSpace(value.ToString()))
			{
				return Guid.Empty;
			}

			if (value.GetType() == typeof(string))
			{
				try
				{
					convertedGuid = new Guid(value.ToString());
					return convertedGuid;
				}
				catch (FormatException)
				{
					return Guid.Empty;
				}
			}
			else
			{
				return (value is Guid) ? (Guid)value : Guid.Empty;
			}
		}

		public static decimal ConvertToDecimal(object objToConvert)
		{
			decimal result = 0;

			if (objToConvert != null && !string.IsNullOrEmpty(objToConvert.ToString()))
			{
				try
				{
					result = Convert.ToDecimal(objToConvert, Culture.CurrentCompanyCountryCulture);
				}
				catch (FormatException)
				{
					try
					{
						result = Convert.ToDecimal(objToConvert, Culture.Default);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
				}
				catch (OverflowException)
				{
				}
			}

			return result;
		}

		public static DateTime ConvertToDateTime(object objToConvert)
		{
			return objToConvert == DBNull.Value || string.IsNullOrEmpty(objToConvert.ToString()) ? NullType.DateTime : Convert.ToDateTime(objToConvert);
		}

		/// <summary>
		/// System.Convert.Int32 wrapper that ignores exceptions due to null references, invalid formats, and overflows.
		/// </summary>
		/// <param name="objectToConvert">An object passed to Convert.ToInt32</param>
		/// <returns>Zero on error, the converted value on success</returns>
		public static int ConvertToInt32(object objectToConvert)
		{
			return ConvertToInt32(objectToConvert, 0);
		}

		/// <summary>
		/// System.Convert.Int32 wrapper that ignores exceptions due to null references, invalid formats, and overflows.
		/// </summary>
		/// <param name="objectToConvert">An object passed to Convert.ToInt32</param>
		/// <param name="valueToReturnOnError">The value to return when an error occurs</param>
		/// <returns>ValueToReturnOnError on error, the converted value on success</returns>
		public static int ConvertToInt32(object objectToConvert, int valueToReturnOnError)
		{
			int result = valueToReturnOnError;

			if (objectToConvert != null && !string.IsNullOrEmpty(objectToConvert.ToString()))
			{
				try
				{
					result = Convert.ToInt32(objectToConvert, Culture.CurrentCompanyCountryCulture);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					try
					{
						result = Convert.ToInt32(objectToConvert);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
				}
			}

			return result;
		}

		public static string GetStringFromObject(object obj)
		{
			return obj == null ? "" : obj.ToString();
		}

		public static bool IsObjectAValidDate(object testThisObj)
		{
			return testThisObj is DateTime && (DateTime)testThisObj != DateTime.MinValue;
		}

		public static bool IsValidGuid(object value)
		{
			bool result = false;

			if (value is Guid)
			{
				Guid guidValue = (Guid)value;
				result = guidValue != Utilities.InvalidSelectionGuid && guidValue != Guid.Empty && guidValue != Utilities.MissingRecordGuid;
			}

			return result;
		}

		public static bool IsByteArrayEqual(byte[] firstByteArray, byte[] secondByteArray)
		{
			if (firstByteArray == secondByteArray)
			{
				return true;
			}

			if (firstByteArray == null || secondByteArray == null)
			{
				return false;
			}

			if (firstByteArray.Length != secondByteArray.Length)
			{
				return false;
			}

			for (int x = 0; x < firstByteArray.Length; ++x)
			{
				if (firstByteArray[x] != secondByteArray[x])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Compares pixel by pixel if the images are valid
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is part of the tool that performs scaling")]
		public static bool IsImageEqual(Image firstImage, Image secondImage)
		{
			if (firstImage == secondImage)
			{
				return true;
			}

			if (!IsImageValidBitmap(firstImage) || !IsImageValidBitmap(secondImage))
			{
				return false;
			}

			Bitmap bm1 = new Bitmap(firstImage);
			Bitmap bm2 = new Bitmap(secondImage);

			int scaledWidth1 = bm1.Width;
			int scaledWidth2 = bm2.Width;

			if (scaledWidth1 != scaledWidth2)
			{
				return false;
			}

			int scaledHeight1 = bm1.Height;
			int scaledHeight2 = bm2.Height;

			if (scaledHeight1 != scaledHeight2)
			{
				return false;
			}

			PixelFormat lowestPixels = bm1.PixelFormat;
			if (bm2.PixelFormat < lowestPixels)
			{
				lowestPixels = bm2.PixelFormat;
			}

			Rectangle rectangle = new Rectangle(0, 0, scaledWidth1, scaledHeight1);
			BitmapData data1 = bm1.LockBits(rectangle, ImageLockMode.ReadWrite, lowestPixels);
			BitmapData data2 = bm2.LockBits(rectangle, ImageLockMode.ReadWrite, lowestPixels);

			int bytes1 = data1.Stride * bm1.Height;
			byte[] rgbValues1 = new byte[bytes1];

			int bytes2 = data2.Stride * bm2.Height;
			byte[] rgbValues2 = new byte[bytes2];

			try
			{
				for (long i = 0; i < rgbValues1.Length; i++)
				{
					if (rgbValues1[i] != rgbValues2[i])
					{
						return false;
					}
				}
				return true;
			}
			finally
			{
				bm1.UnlockBits(data1);
				bm2.UnlockBits(data2);
			}
		}

		public static int GetImageContentsHashCode(Image image)
		{
			var hashCode = 0;

			if (image != null)
			{
				using (var stream = new MemoryStream())
				{
					image.Save(stream, image.RawFormat.Equals(ImageFormat.MemoryBmp) ? ImageFormat.Bmp : image.RawFormat);
					hashCode = HashCodeHelper.GetCompositeHashCode(stream.ToArray());
				}
			}

			return hashCode;
		}

		static bool IsImageValidBitmap(Image imageToCheck)
		{
			return imageToCheck != null && imageToCheck.Height != 0 && imageToCheck.Width != 0;
		}

		public static bool IsConnectionAndPortChecked(IRegistryItem registryItem, IRegistryItem lookupRegistryItem)
		{
			var result = false;
			var defaultPort = (int)lookupRegistryItem.DefaultValue;
			var proposedRegistryItemValue = ((IRegistryItemInternals)registryItem).GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty);
			var proposedLookupRegistryItemValue = (int)((IRegistryItemInternals)lookupRegistryItem).GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, Guid.Empty);
			if (proposedRegistryItemValue != null && proposedRegistryItemValue.ToString() != SecureConnectionTypes.None && proposedLookupRegistryItemValue == defaultPort)
			{
				var message = Res.GetString("480f71c2-9c46-48c2-bd0f-a680945697ea", "You have selected to use a secure connection, please confirm that the port number selected is desired > {0}", lookupRegistryItem.Caption);
				Globals.Message.ShowWarning(message);
				result = true;
			}

			return result;
		}

		public static bool IsProtocolAndPortChecked(IRegistryItem registryItem, IRegistryItem lookupRegistryItem)
		{
			int[] commonlyUsedIMAPPorts = { 143, 993 };
			int[] commonlyUsedPOP3Ports = { 110, 995 };

			var result = false;
			var message = Res.GetString("46c3e3ee-8768-463a-9bed-a5861d8146ac", "Please confirm that this protocol matches the selected port > {0}", lookupRegistryItem.Caption);

			var proposedRegistryItemValue = ((IRegistryItemInternals)registryItem).GetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty);
			if (proposedRegistryItemValue != null)
			{
				var protocol = proposedRegistryItemValue.ToString();
				var lookupRegistryItemValue = (int)((IRegistryItemInternals)lookupRegistryItem).GetCurrentValueFromProposedValueAccessor(Guid.Empty, Guid.Empty, Guid.Empty);

				if (protocol == MailRetrievalProtocols.IMAP && !commonlyUsedIMAPPorts.Contains(lookupRegistryItemValue))
				{
					Globals.Message.ShowWarning(message);
					result = true;
				}
				else if (protocol == MailRetrievalProtocols.POP3 && !commonlyUsedPOP3Ports.Contains(lookupRegistryItemValue))
				{
					Globals.Message.ShowWarning(message);
					result = true;
				}
			}

			return result;
		}

		public static class ExpressionEvaluator
		{
			[ThreadSafe]
			static readonly ThreadLocal<(Engine engine, int globalCount)> jintEngine = new ThreadLocal<(Engine engine, int globalCount)>();

			public enum ErrorCode
			{
				None,
				EmptyExpression,
				NotTrueFalseExpression,
				JScriptEngineError,
				InvalidExpression
			}

			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is javascript")]
			static Engine Engine
			{
				get
				{
					if (!jintEngine.IsValueCreated || (jintEngine.Value.engine.Global.GetOwnProperties().Count() != jintEngine.Value.globalCount))
					{
						var engine = new Engine(o => o.LimitRecursion(100).MaxStatements(RawDataRegistry.Instance.JSEngineMaxStatements.Value));
						engine.Execute(
										@"String.prototype.Contains = function (str) { return !(this.indexOf(str) === -1)};
										String.prototype.Substring = String.prototype.substr;
										String.prototype.IndexOf = String.prototype.indexOf;
										String.prototype.StartsWith = function (str) { return this.indexOf(str) === 0 };
										String.prototype.EndsWith = function (str) { return this.lastIndexOf(str) === this.length - str.length };
										String.prototype.ToLower = String.prototype.toLowerCase;
										");
						var globalCount = engine.Global.GetOwnProperties().Count();

						jintEngine.Value = (engine, globalCount);
					}

					return jintEngine.Value.engine;
				}
			}

			/// <summary>
			/// ExpressionEvaluator.Evaluate a method to evaluate a Java Script expression bypassing macros
			/// </summary>
			/// <param name="expression">A Boolean Java Script expression to evaluate</param>
			/// <returns>Returns either result or an error code</returns>
			public static Either<ErrorCode, bool> EvaluateJS(string expression)
			{
				if (!string.IsNullOrEmpty(expression))
				{
					expression = expression
						.Replace("&gt;", ">")
						.Replace("&lt;", "<");
				}

				return EvaluateJSCore(expression, true);
			}

			static Either<ErrorCode, bool> EvaluateJSCore(string expression, bool tryToEscapeQuotes)
			{
				if (string.IsNullOrWhiteSpace(expression))
				{
					return ErrorCode.EmptyExpression;
				}

				var jsEngineEvaluatorResult = StringToObjectJS(expression);

				if (jsEngineEvaluatorResult.IsLeft)
				{
					if (tryToEscapeQuotes)
					{
						var escaped = EscapeDoubleQuotesAndBackslashesWithinExpressionValues(expression);

						if (string.CompareOrdinal(expression, escaped) != 0)
						{
							return EvaluateJSCore(escaped, false);
						}
					}

					return jsEngineEvaluatorResult.Left;
				}

				if (jsEngineEvaluatorResult.Right is bool res)
				{
					return res;
				}

				return ErrorCode.NotTrueFalseExpression;
			}

			#region Reintroduced legacy code removed in checkin 252669

			/// <summary>
			/// Escapes double quotes in an expression that will cause the expression evaluator to fail.
			/// eg:
			///		replaces	""A" Line Shipping\" == ""A" Line Shipping\"
			///		with		"\"A\" Line Shipping\\\" == "\"A\" Line Shipping\\\"
			///	</summary>

			public static string EscapeDoubleQuotesAndBackslashesWithinExpressionValues(string expression)
			{
				if (string.IsNullOrEmpty(expression))
				{
					return expression;
				}

				var replacedExpression = new StringBuilder();
				for (int i = 0; i < expression.Length; i++)
				{
					if (BackslashNextToQuoteFound(i, expression))
					{
						replacedExpression.Append(expression[i] + "\\");
					}
					else if (RogueDoubleQuoteFound(i, expression))
					{
						replacedExpression.Append("\\\"");
					}
					else
					{
						replacedExpression.Append(expression[i]);
					}
				}
				return replacedExpression.ToString();
			}

			static bool BackslashNextToQuoteFound(int index, string expression)
			{
				return expression[index] == '\\';
			}

			static bool RogueDoubleQuoteFound(int index, string expression)
			{
				return expression[index] == '"' // we found a quote
					&& !(IsFirstNonWhitespace(index, expression) ^ IsLastNonWhitespace(index, expression))      // it's not at the outer edge of the string
					&& !(OperatorPreceedsIndex(index, expression) ^ OperatorFollowsIndex(index, expression));   // it isn't surrounding an expression value
			}

			static bool IsFirstNonWhitespace(int index, string expression)
			{
				return index == 0 || ContainsWhitespaceAndSpecifiedCharsOnly(expression.Substring(0, index), OpenBrackets);
			}

			static bool IsLastNonWhitespace(int index, string expression)
			{
				return index == expression.Length - 1 || ContainsWhitespaceAndSpecifiedCharsOnly(expression.Substring(index + 1), CloseBrackets);
			}

			static bool ContainsWhitespaceAndSpecifiedCharsOnly(string substring, char[] allowedChars)
			{
				return substring.All(c => IsWhitespaceOrSpecifiedChars(c, allowedChars));
			}

			static bool IsWhitespaceOrSpecifiedChars(char c, char[] allowedChars)
			{
				return char.IsWhiteSpace(c) || allowedChars.Contains(c);
			}

			static bool OperatorPreceedsIndex(int index, string expression)
			{
				index--;

				while (index > 0 && IsWhitespaceOrSpecifiedChars(expression[index], OpenBrackets))
				{
					index--;
				}

				if (index >= 0 && index < expression.Length)
				{
					var targetChar = expression[index];

					foreach (var op in Operators.Where(op => op.Contains(targetChar)))
					{
						var indexOfOperatorStart = index - op.Length + 1;
						var isMatched = indexOfOperatorStart >= 0 && expression.Substring(indexOfOperatorStart, op.Length) == op;
						if (isMatched)
						{
							return true;
						}
					}
				}

				return false;
			}

			static bool OperatorFollowsIndex(int index, string expression)
			{
				index++;

				while (index < expression.Length - 1 && IsWhitespaceOrSpecifiedChars(expression[index], CloseBrackets))
				{
					index++;
				}

				if (index >= 0 && index < expression.Length)
				{
					var targetChar = expression[index];

					foreach (var op in Operators.Where(op => op.Contains(targetChar)))
					{
						var indexOfOperatorEnd = index + op.Length - 1;
						var isMatched = indexOfOperatorEnd < expression.Length && expression.Substring(index, op.Length) == op;
						if (isMatched)
						{
							return true;
						}
					}
				}

				return false;
			}

			static char[] OpenBrackets { get { return new[] { '[', '{', '(' }; } }
			static char[] CloseBrackets { get { return new[] { ']', '}', ')' }; } }

			static string[] Operators
			{
				get { return operators; }
			}

			[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It's readonly and ititializes right away.")]
			static readonly string[] operators = { greaterThan, lessThan, greaterThanOrEqualTo, lessThanOrEqualTo, isEqualTo, isNotEqualTo, and, or };

			const string greaterThan = ">";
			const string lessThan = "<";
			const string greaterThanOrEqualTo = ">=";
			const string lessThanOrEqualTo = "<=";
			const string isEqualTo = "==";
			const string isNotEqualTo = "!=";
			const string and = "&&";
			const string or = "||";

			#endregion

			static Either<ErrorCode, object> StringToObjectJS(string expression)
			{
				expression = expression.Replace("\n", "\\n");
				expression = expression.Replace("\r", "\\r");

				try
				{
					return Engine.Execute(expression).GetCompletionValue().AsBoolean();
				}
				catch (ArgumentException)
				{
					return ErrorCode.NotTrueFalseExpression;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return ErrorCode.InvalidExpression;
				}
			}
		}

		#endregion

		#region Formatting

		public static string FixNewLinesForEnvironment(string value)
		{
			return Regex.Replace(value, "(\r*\n)", System.Environment.NewLine);
		}

		public static string FormatNumber(ZDecimal number, int decimals)
		{
			decimal numberToFormatAsDecimal = number;
			return FormatNumberCore(numberToFormatAsDecimal, decimals, Culture.Default);
		}

		public static string FormatNumber(ZDecimal number, int decimals, CultureInfo culture)
		{
			decimal numberToFormatAsDecimal = number;
			return FormatNumberCore(numberToFormatAsDecimal, decimals, culture);
		}

		public static string FormatNumberNational(ZDecimal number, int decimals)
		{
			decimal numberToFormatAsDecimal = number;
			return FormatNumberCore(numberToFormatAsDecimal, decimals, Culture.CurrentCompanyCountryCulture);
		}

		public static string FormatNumberFromZInt(ZInt number, int decimals)
		{
			decimal numberToFormatAsDecimal = number;
			return FormatNumberCore(numberToFormatAsDecimal, decimals, Culture.Default);
		}

		public static string FormatNumberFromZInt(ZInt number, int decimals, CultureInfo culture)
		{
			decimal numberToFormatAsDecimal = number;
			return FormatNumberCore(numberToFormatAsDecimal, decimals, culture);
		}

		public static string FormatNumberFromZIntNational(ZInt number, int decimals)
		{
			decimal numberToFormatAsDecimal = number;
			return FormatNumberCore(numberToFormatAsDecimal, decimals, Culture.CurrentCompanyCountryCulture);
		}

		public static string FormatNumberFromZLongNational(ZLong number, int decimals)
		{
			return FormatNumberCore((decimal)number, decimals, Culture.CurrentCompanyCountryCulture);
		}

		public static string FormatNumber(object number, int decimals)
		{
			decimal numberToDisplay = GetNumberAsDecimal(number);
			return FormatNumberCore(numberToDisplay, decimals, Culture.Default);
		}

		public static string FormatNumber(object number, int decimals, CultureInfo culture)
		{
			decimal numberToDisplay = GetNumberAsDecimal(number);
			return FormatNumberCore(numberToDisplay, decimals, culture);
		}

		public static string FormatNumberNational(object number, int decimals)
		{
			decimal numberToDisplay = GetNumberAsDecimal(number);
			return FormatNumberCore(numberToDisplay, decimals, Culture.CurrentCompanyCountryCulture);
		}

		public static string FormatNumberWithGroupSeparators(object number, int decimals)
		{
			decimal numberToDisplay = GetNumberAsDecimal(number);
			return FormatNumberWithGroupSeparators(numberToDisplay, decimals, Culture.Default);
		}

		public static string FormatNumberWithGroupSeparators(object number, int decimals, CultureInfo culture)
		{
			decimal numberToDisplay = GetNumberAsDecimal(number);
			return FormatNumberWithGroupSeparators(numberToDisplay, decimals, culture);
		}

		public static string FormatNumberNationalWithGroupSeparators(object number, int decimals)
		{
			decimal numberToDisplay = GetNumberAsDecimal(number);
			return FormatNumberWithGroupSeparators(numberToDisplay, decimals, Culture.CurrentCompanyCountryCulture);
		}

		#region Implementation

		static string FormatNumberCore(decimal number, int decimals, CultureInfo culture)
		{
			string result;
			if (decimals == 0)
			{
				result = decimal.Round(number, 0).ToString();
			}
			else
			{
				result = string.Format(culture, GetFormatString(decimals), new object[] { number });
			}

			return result;
		}

		static string FormatNumberWithGroupSeparators(decimal number, int decimals, CultureInfo culture)
		{
			return number.ToString("n" + decimals, culture.NumberFormat);
		}

		static decimal GetNumberAsDecimal(object number)
		{
			decimal numberToDisplay = 0;

			if ((number != null) && (number != DBNull.Value))
			{
				try
				{
					numberToDisplay = Convert.ToDecimal(number);
				}
				catch (FormatException)
				{
				}
			}

			return numberToDisplay;
		}

		internal static string GetFormatString(int decimals)
		{
			string formatString = "0";
			if (decimals > 0)
			{
				formatString += ".";
			}
			for (int i = 0; i < decimals; i++)
			{
				formatString += "0";
			}
			return "{0,3:#" + formatString + "}";
		}

		#endregion

		#endregion

		#region Data Utilities

		#region Generic DB/ADO Utilities

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static object GetFieldFromTable(string fieldName, string tableName, SchemaColumn filterColumn, object filterValue)
		{
			DbCommand command = Db.Connection.Command("SELECT " + fieldName + " FROM " + tableName + " WHERE " + filterColumn.Name + " = @FilterValue");    // This is O so can't use Z
			command.AddParameterBasedOnDbColumn("@FilterValue", filterValue, filterColumn);
			return command.ExecuteScalar();
		}

		public static Guid GetGuidFromTopRowInTable(string fieldName, string table)
		{
			return (Guid)GetFieldFromTopRowInTable(fieldName, table);
		}

		static object GetFieldFromTopRowInTable(string fieldName, string table)
		{
			return GetFieldFromRowInTable(fieldName, table, "");
		}

		public static Guid GetGuidFromRandomRowInTable(string fieldName, string table)
		{
			return (Guid)GetFieldFromRandomRowInTable(fieldName, table);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Statement")]
		public static object GetFieldFromRandomRowInTable(string fieldName, string table)
		{
			return GetFieldFromRowInTable(fieldName, table, "ORDER BY newid()");
		}

		static object GetFieldFromRowInTable(string fieldName, string table, string orderClause)
		{
			string selectSQL = "SELECT TOP 1 " + fieldName + " FROM " + table + " " + orderClause;
			return Db.Connection.ExecuteScalar(selectSQL) // This is O so can't use Z
				?? throw new NullReferenceException("Table " + table + " Contains 0 Rows");
		}

		#region Get DataTable From Query

		public static DataTable GetDataTableFromQuery(string sqlText, params IStructuralEquatable[] sqlParams)
		{
			return GetDataTableFromQuery(Db.Connection, sqlText, sqlParams);
		}

		public static DataTable GetDataTableFromQuery(DbConnection conn, string sqlText, params IStructuralEquatable[] sqlParams)
		{
			return DataUtils.GetDataTableFromQuery(conn, sqlText, sqlParams);
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void RunResultingScriptsBasedOnSQLScriptGenerator(string generatorScript)
		{
			DbCommand command = Db.Connection.Command(generatorScript); // This is O so can't use Z
			ArrayList scripts = new ArrayList();

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					StringBuilder scriptStringBuilder = new StringBuilder(512);

					for (int i = 0; i < reader.FieldCount; i++)
					{
						scriptStringBuilder.Append(reader.GetString(i));
					}

					scripts.Add(scriptStringBuilder.ToString());
				}
			}

			StringBuilder scriptBatch = new StringBuilder();
			foreach (string script in scripts)
			{
				const string OneSingleQuote = "'";
				const string TwoSingleQuotes = "''";

				scriptBatch.Append("\n");
				scriptBatch.Append("EXEC('"); // SQL Statement
				scriptBatch.Append("\n");
				scriptBatch.Append(script.Replace(OneSingleQuote, TwoSingleQuotes));
				scriptBatch.Append("')");
				if (scriptBatch.Length > 5000)
				{
					AttemptScript(scriptBatch);
					scriptBatch = new StringBuilder();
				}
			}
			AttemptScript(scriptBatch);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void AttemptScript(StringBuilder scriptBatch)
		{
			if (scriptBatch.Length > 0)
			{
				DbCommand command = Db.Connection.Command(scriptBatch.ToString());  // This is O so can't use Z
				try
				{
					command.ExecuteNonQuery();
				}
				catch (System.Data.Common.DbException e)
				{
					throw new Exception("Error Running script:" + scriptBatch + System.Environment.NewLine + "Original Message is: " + e.Message, e);
				}
			}
		}

		#endregion

		#region UNLoco Utilities

		public static string GetCodeFromNameAndUNLOCO(string name, object uNLOCO, string oldCode)
		{
			string code;
			int iteration = 0;

			do
			{
				code = ShortenCompanyName(CleanCompanyName(name), iteration++) + CleanLocoCode(GetUNLOCOStringFromGuid(uNLOCO));
			}
			while (DoesCodeExist(code, oldCode));

			return code;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		static string GetUNLOCOStringFromGuid(object uNLOCO)
		{
			DbCommand command = Db.Connection.Command("select ISNULL((select RL_Code from dbo.RefUNLOCO where RL_PK = @pk), '')");  // This is O so can't use Z
			command.AddParameterBasedOnDbColumn("@pk", GetGuidFromObject(uNLOCO), RefUNLOCOSchema.PK);
			return command.ExecuteScalar().ToString();
		}

		static string ShortenCompanyName(string companyName, int iteration)
		{
			int lengthOfCode = 9;

			if (companyName.Length > lengthOfCode)
			{
				for (int i = companyName.Length - 1; i > 2 && companyName.Length > lengthOfCode; i--)
				{
					if ("AOEIU".IndexOf(companyName[i]) > -1)
					{
						companyName = companyName.Remove(i, 1);
					}
				}

				companyName = companyName.Substring(0, companyName.Length > lengthOfCode ? lengthOfCode : companyName.Length);
			}

			if (string.IsNullOrWhiteSpace(companyName))
			{
				companyName = "1";
			}

			if (iteration > 0)
			{
				while (companyName.Length < lengthOfCode)
				{
					companyName += " ";
				}

				companyName = companyName.Substring(0, lengthOfCode - iteration.ToString().Length) + iteration.ToString();
				companyName = companyName.Replace(" ", "");
			}

			return companyName;
		}

		static string CleanLocoCode(string lOCOCode)
		{
			if (lOCOCode.Length > 4)
			{
				lOCOCode = lOCOCode.Substring(2, 3);
			}

			return lOCOCode.ToUpper().Trim();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Company Name Suffixes")]
		static string CleanCompanyName(string companyName)
		{
			companyName = companyName.ToUpper().Trim();
			string[] exclusions = new string[] { "PTY", "LTD", "PROPRIETORY", "LIMITED", "INC", "INCORPORATED", "BB", "GMBH", "PTE LTD", "P/L", "CO" };

			foreach (string exclusion in exclusions)
			{
				companyName = companyName.Replace(" " + exclusion, "");
			}

			string newCompanyName = "";

			foreach (char c in companyName)
			{
				if (" ,';-()*[]\"{}!@#$%^&~`/\\.|`<>:+_".IndexOf(c) == -1)
				{
					newCompanyName += c.ToString();
				}
			}

			return newCompanyName;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static bool DoesCodeExist(string code, string oldCode)
		{
			bool result = false;

			if (code != oldCode)
			{
				DbCommand command = Db.Connection.Command("SELECT COUNT(1) FROM dbo.ORGHEADER WHERE OH_CODE = '" + code + "'"); // This is O so can't use Z
				result = (int)command.ExecuteScalar() > 0;
			}

			return result;
		}

		#endregion

		public static string TrimExpression(string expression)
		{
			return expression.Trim(' ', '\t', '<', '>');
		}

		#endregion

		#region Windows Utilities

		public static string GetLocalIPAddress()
		{
			string result;

			try
			{
				System.Net.IPAddress[] addresses = System.Net.Dns.GetHostEntry("").AddressList;
				result = GetFirstRoutableIpAddressOrEmpty(addresses);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// TODO: Find proper DNS exception to catch
				result = "";
			}

			return result;
		}

		internal static string GetFirstRoutableIpAddressOrEmpty(System.Net.IPAddress[] addresses)
		{
			var addr = string.Empty;
			foreach (var address in addresses)
			{
				if (address != null && !address.IsIPv6LinkLocal && IsRoutableIp(address.ToString()) && !IsLoopbackAddress(address.ToString()))
				{
					return addr = address.ToString();
				}
			}

			return addr;
		}

		internal static bool IsLoopbackAddress(string ipAddress)
		{
			return string.Equals(ipAddress, "::1") || string.Equals(ipAddress, "127.0.0.1");
		}

		internal static bool IsRoutableIp(string ipAddress)
		{
			return !ipAddress.StartsWith("169.254.");
		}

		#endregion

		#region Rounding

		public static class RoundingTypes
		{
			public const string NoRounding = "NOR";
			public const string Bankers = "BNK";
			public const string UpToHalf = "UPH";
			public const string UpTo1 = "UP1";
			public const string UpTo1IfLessThanOne = "U1L";
			public const string Custom = "CUS";
		}

		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Math.Round gets corrected here, so it should be invoked")]
		public static decimal Round(decimal amount, int roundingScale)
		{
			int originalMultipler = amount > 0 ? 1 : -1;
			decimal bankersRound = Math.Round(Math.Abs(amount), roundingScale); // Math.Round gets corrected here, so it should be invoked

			if (bankersRound - Math.Abs(amount) == -((decimal)Math.Pow(10, -roundingScale) / 2))
			{
				bankersRound += (decimal)Math.Pow(10, -roundingScale);
			}

			return bankersRound * originalMultipler;
		}

		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Math.Round does Bankers Rounding.")]
		public static ZDecimal Round(ZDecimal amount, ZString rounding, ZDecimal roundingFactor)
		{
			ZDecimal result;
			switch (rounding)
			{
				case RoundingTypes.Bankers:
					result = Math.Round(amount);
					break;

				case RoundingTypes.UpTo1:
					result = Math.Ceiling((double)amount);
					break;

				case RoundingTypes.UpTo1IfLessThanOne:
					result = Math.Max(1, amount);
					break;

				case RoundingTypes.UpToHalf:
					result = (ZDecimal)Math.Ceiling((double)(amount / 0.5m)) * 0.5m;
					break;

				case RoundingTypes.Custom:
					if (roundingFactor != 0)
					{
						result = (ZDecimal)Math.Ceiling((double)(amount / roundingFactor)) * roundingFactor;
					}
					else
					{
						result = amount;
					}
					break;

				default:
					result = amount;
					break;
			}

			return result;
		}

		public static Guid CurrencyUSD
		{
			get
			{
				SetUSDCurrencyIfNotCached();
				return fCurrencyUSD;
			}
			set { fCurrencyUSD = value; }
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void SetUSDCurrencyIfNotCached()
		{
			if (fCurrencyUSD == NullType.Guid)
			{
				DbCommand command = Db.Connection.Command("SELECT " + RefCurrencySchema.Constants.PK + " FROM " + RefCurrencySchema.Constants.SqlSchemaName + "." + RefCurrencySchema.Constants.TableName + " WHERE " + RefCurrencySchema.Constants.RX_Code + " = 'USD'");    // This is O so can't use Z
				object result = command.ExecuteScalar();
				if (result is Guid)
				{
					CurrencyUSD = (Guid)result;
				}
			}
		}

		[ThreadStatic]
		static Guid fCurrencyUSD;

		#endregion

		#region Host Server

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static string GetServerMachineName()
		{
			DbCommand command = Db.Connection.Command("SELECT ServerProperty('MachineName')");  // This is O so can't use Z SQL Staetment
			object result = command.ExecuteScalar();
			return result == null ? "" : result.ToString();
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static string GetCurrentHostMachineName()
		{
			DbCommand command = Db.Connection.Command("SELECT Host_Name()");    // This is O so can't use Z SQL Staetment
			object result = command.ExecuteScalar();
			return result == null ? "" : result.ToString();
		}

		#endregion

		#region Security

		#region GenerateSecurityToken

		public static string GenerateSecurityToken(string username, string password, string branchCode = "", string departmentCode = "")
		{
			string result = "";

			TwoWayEncoder encrypter = TwoWayEncoder.NewWithStandardInitialisationVector();
			DateTime now = DateTime.Now; // used to generate security token

			result = encrypter.Encrypt(username + password + branchCode + departmentCode +
				now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + now.Hour.ToString());

			return result;
		}

		#endregion

		#endregion

		#region ZDateTime

		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Specific functionality")]
		public static string ToFriendlyTimeString(this TimeSpan span, FriendlyMinutesDisplay minutesDisplay = FriendlyMinutesDisplay.Default, FriendlyMaxUnitRollup maxUnitRollup = FriendlyMaxUnitRollup.Year)
		{
			var sign = Math.Sign(span.TotalMinutes);
			var days = Math.Abs(span.Days);
			var hours = Math.Abs(span.TotalHours);
			var minutes = Math.Abs(span.TotalMinutes);

			var result = string.Empty;

			var years = days / 365; // It's approximate only
			if (maxUnitRollup >= FriendlyMaxUnitRollup.Year && years >= 1)
			{
				result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", years, years == 1 ? TimeConstants.TimeStrings.Year : TimeConstants.TimeStrings.Years);
			}
			else
			{
				var months = days / (365 / 12); // Again, just approximate
				if (maxUnitRollup >= FriendlyMaxUnitRollup.Month && months >= 1)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", months, months == 1 ? TimeConstants.TimeStrings.Month : TimeConstants.TimeStrings.Months);
				}
				else
				{
					var weeks = days / 7;
					if (maxUnitRollup >= FriendlyMaxUnitRollup.Week && weeks >= 1)
					{
						result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", weeks, weeks == 1 ? TimeConstants.TimeStrings.Week : TimeConstants.TimeStrings.Weeks);
					}
					else
					{
						if (maxUnitRollup >= FriendlyMaxUnitRollup.Day && days >= 1)
						{
							result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", days, days == 1 ? TimeConstants.TimeStrings.Day : TimeConstants.TimeStrings.Days);
						}
						else
						{
							if (maxUnitRollup >= FriendlyMaxUnitRollup.Hour && hours >= 1)
							{
								result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", hours.FormatWithNoMoreThanTwoDecimalPlaces(), hours == 1 ? TimeConstants.TimeStrings.Hour : TimeConstants.TimeStrings.Hours);
							}
							else
							{
								var showDecimalsInMinutes = minutesDisplay == FriendlyMinutesDisplay.ShowMinutesDecimalPlaces;
								var showSeconds = minutesDisplay == FriendlyMinutesDisplay.ShowSeconds;

								if (minutes >= 1)
								{
									result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", minutes.ToString(showDecimalsInMinutes && !showSeconds ? "0.##" : "0", CultureInfo.InvariantCulture),
										minutes == 1 || ((!showDecimalsInMinutes || showSeconds) && Math.Round(minutes) == 1) // Specific functionality
											? TimeConstants.TimeStrings.Minute
											: TimeConstants.TimeStrings.Minutes);

									if (showSeconds && span.Seconds > 0)
									{
										result += " " + Res.GetString("eeafe1e0-760a-4e96-b237-7a58afd95a25", "and") + " ";
									}
								}

								if (showSeconds && span.Seconds > 0)
								{
									result += span.Seconds + " " + (span.Seconds == 1 ? TimeConstants.TimeStrings.Second : TimeConstants.TimeStrings.Seconds);
								}
							}
						}
					}
				}
			}

			if (sign < 0)
			{
				result = "-" + result;
			}

			if (minutesDisplay.HasFlag(FriendlyMinutesDisplay.ShowZeroMinutes) && string.IsNullOrEmpty(result))
			{
				return Res.GetString("09ec3d10-0e37-4586-b175-a4930e46ebb7", "0 minutes");
			}
			else
			{
				return result;
			}
		}

		#endregion

		#region Decimal

		public static string FormatWithNoMoreThanTwoDecimalPlaces(this double value)
		{
			return ((decimal)value).FormatWithNoMoreThanTwoDecimalPlaces();
		}

		public static string FormatWithNoMoreThanTwoDecimalPlaces(this decimal value)
		{
			return value.ToString("0.##", CultureInfo.InvariantCulture);
		}

		#endregion

		public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
				{
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}
	}

	public interface IWindowPersister
	{
		string GetOpenFormUrls();
	}

	public enum FriendlyMinutesDisplay
	{
		Default,
		ShowMinutesDecimalPlaces,
		ShowSeconds,
		ShowZeroMinutes,
	}

	public enum FriendlyMaxUnitRollup
	{
		Hour,
		Day,
		Week,
		Month,
		Year
	}
}
