using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class ExceptionKeyBuilder
	{
		#region Build Key

		/// <summary>
		/// Build the identifying key. It will have no leading or trailing whitespace.
		/// </summary>
		public string BuildKey(ExceptionKeyFields fields)
		{
			ZString key = fields.Key.IsEmpty ? fields.CallStack : fields.Key;

			if (key.IsEmpty)
			{
				key = fields.Source + " - " + string.Join(" - ", fields.Messages);
			}

			key = key.Trim();
			key = MassageSpecialKeys(key, fields);
			key = ReplaceKeyIfMatchedRegex(key);
			key = RemoveRegexMatches(key);
			key = AddHResultToKey(key, fields.Message);
			key = key.Trim();

			if (key.Length <= 3)
			{
				key = fields.CallStack.Trim();
			}

			// key should be trimmed at this point
			return key;
		}

		#endregion

		#region Part Key Replacement

		string ReplaceKeyIfMatchedRegex(string key)
		{
			if (key.Length > 0)
			{
				foreach (string regex in ExceptionKeyMatchingRegexes)
				{
					if (Regex.IsMatch(key, regex))
					{
						return regex;
					}
				}
			}
			return key;
		}

		string RemoveRegexMatches(string key)
		{
			if (key.Length > 0)
			{
				foreach (var pattern in ExceptionKeyRegexes)
				{
					key = Regex.Replace(key, pattern, "");
				}
			}
			return key;
		}

		ZString[] ExceptionKeyMatchingRegexes
		{
			get { return exceptionKeyMatchingRegexes ?? (exceptionKeyMatchingRegexes = EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Cast<ExceptionKeyRegex>().Select(e => e.Regex).ToArray()); }
		}
		ZString[] exceptionKeyMatchingRegexes;

		ZString[] ExceptionKeyRegexes
		{
			get { return exceptionKeyRegexes ?? (exceptionKeyRegexes = EDIDataRegistry.Instance.ExceptionKeyRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Cast<ExceptionKeyRegex>().Select(e => e.Regex).ToArray()); }
		}
		ZString[] exceptionKeyRegexes;

		#endregion

		#region HResult

		string AddHResultToKey(string key, string message)
		{
			// Extract HResult from Exception Message and append to Key (Interim solution until correctly handled by error handling code).

			string hResult = GetHResultAsString(message);
			if (hResult != null)
			{
				key = string.Concat(key, " (HResult: ", hResult, ")");
			}

			return key;
		}

		string GetHResultAsString(string message)
		{
			if (message != null && message.Length > 0)
			{
				int startIndex = message.IndexOf("HRESULT: 0X", System.StringComparison.OrdinalIgnoreCase);
				if (startIndex >= 0)
				{
					int endIndex = message.IndexOfAny(new char[2] { ' ', ')' }, startIndex + 10);
					if (endIndex >= 0)
					{
						return message.Substring(startIndex + 9, endIndex - (startIndex + 9));
					}
				}
			}

			return null;
		}

		#endregion

		#region Massage Special Keys

		string MassageSpecialKeys(string key, ExceptionKeyFields fields)
		{
			if (!string.IsNullOrEmpty(key))
			{
				if (!TryMassageConcurrencyErrorKey(key, fields, out key))
				{
					key = MassageSqlExceptions(key, fields);
				}
			}

			return key;
		}

		bool TryMassageConcurrencyErrorKey(string key, ExceptionKeyFields fields, out string result)
		{
			if (TryMassageConcurrencySaveErrorKey(key, fields, out result))
			{
				return true;
			}
			else
			{
				foreach (ZString message in fields.Messages)
				{
					if (TryMassageConcurrencySaveErrorKey(message, fields, out result))
					{
						return true;
					}
				}
			}

			result = key;
			return false;
		}

		bool TryMassageConcurrencySaveErrorKey(string key, ExceptionKeyFields fields, out string result)
		{
			// Keys of the nature below are stripped from the PK:, extra asterisks and spaces are removed and the call stack is appended
			// Newlines are stripped so the Tablename appears in the first line of the key (for easy viewing in Issue Manager

			// **CONCURRENCY Error Saving Record **    Tablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Business......
			// ** CONCURRENCY Error Saving Record **    Tablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Business......
			// *** CONCURRENCY Error Saving Record **    Tablename: JobCharge  PK: a9075d04-9cc2-47b7-9545-119e74c1bd52  RowState: Modified  Business object around row = Enterprise.Accounting.Business......

			// Result = 
			// **CONCURRENCY Error Saving Record **  Tablename: JobCharge + CallStack

			int startIndex = key.IndexOf("CONCURRENCY Error Saving Record **", StringComparison.OrdinalIgnoreCase);

			if (startIndex >= 0)
			{
				int pkStartIndex = key.IndexOf("PK: ", startIndex, System.StringComparison.OrdinalIgnoreCase);

				if (pkStartIndex >= 0 && (IsPK(key, pkStartIndex + 4) || key.IndexOf("RowState", pkStartIndex + 4, StringComparison.Ordinal) > startIndex))
				{
					result = "**" + key.Substring(startIndex, pkStartIndex - startIndex);
					result = result.Replace("\r\n", " ");
					result += "\r\n" + fields.CallStack;
					return true;
				}
			}

			result = key;
			return false;
		}

		bool IsPK(string s, int startIndex)
		{
			if (string.IsNullOrEmpty(s))
			{
				return false;
			}

			if (startIndex + 36 > s.Length)
			{
				return false;
			}

			ZGuid guid = ZGuid.Empty;
			return ZGuid.TryParse(s.Substring(startIndex, 36), out guid);
		}

		string MassageSqlExceptions(string key, ExceptionKeyFields fields)
		{
			if (fields.Type.Contains("SqlException", StringComparison.Ordinal))
			{
				return fields.Message + System.Environment.NewLine + key;
			}
			return key;
		}

		#endregion
	}
}

