using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EventReference;
using CargoWise.Workflow;
using static Enterprise.ZArchitecture.Business.StmALog;

namespace Enterprise.ZArchitecture.Business
{
	static class EventReferenceProcessor
	{
		public static bool IsValidReferenceForTest(string reference)
		{
			return ParseAndValidateEventReference(reference).InvalidCodesForTest.Count == 0;
		}

		public static (ObservableDictionary<string, string> Parameters, List<string> InvalidCodesForTest) ParseAndValidateEventReference(string eventReference, ParseReferenceError throwOnDuplicates = ParseReferenceError.None)
		{
			var invalidCodesForTest = new List<string>();
			var parameters = new ObservableDictionary<string, string>();

			EventLogReferenceBuilder.New().ParseReference(eventReference, (text) => { }, (key, value) =>
			{
				if (!parameters.ContainsKey(key))
				{
					parameters.Add(key, value);

					#if DEBUG
					if (!IsValidCodeForTest(key))
					{
						invalidCodesForTest.Add(key);
					}
					#endif
				}
				else
				{
					var oldValue = parameters[key];

					if (oldValue != value)
					{
						switch (throwOnDuplicates)
						{
							case ParseReferenceError.Exception:
								var message = LogMessages.DuplicatedKeyError(key, value, oldValue);
								throw new ArgumentException(message, nameof(eventReference));
							case ParseReferenceError.None:
								break;
						}

						parameters[key] = value;
					}
				}
			});

			return (parameters, invalidCodesForTest);
		}

		static bool IsValidCodeForTest(string key)
		{
			CheckCodeSet();
			return (codeSet.Contains(key));
		}

		static void CheckCodeSet()
		{
			if (codeSet == null)
			{
				codeSet = new HashSet<string>();
				var fields =
					typeof(Constants.EventReferenceParameters.Codes)
					.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
					.Where(fieldInf => fieldInf.IsLiteral && !fieldInf.IsInitOnly && fieldInf.FieldType == typeof(string))
					.Select(s => (string)s.GetRawConstantValue()).ToArray();
				foreach (var field in fields)
				{
					codeSet.Add(field);
				}
			}
		}

		[ThreadStatic]
		static HashSet<string> codeSet;
	}
}
