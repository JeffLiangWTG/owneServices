using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class KeyedCodePairValue : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<KeyedCodePairValue({provider},{field},{index},{Key})>",
	ResString.GetMultilingualString("ebd1664c-626f-4cde-8402-894ecc76ad3c",
	@"Returns the field value(s) (code or description) from specified value pair provider's element with specified index.When {0} equals '{1}', returns comma-separated list of all values. {0} is 1-based or string Code.",
	"{index}", "All"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<KeyedCodePairValue(dynamicorderheaderstatus, Code, 1, <Key>)>", "INC"),
					((NoResString)"<KeyedCodePairValue(dynamicorderheaderstatus, Description, INC, <Key>)>", (NoResString)"Incomplete") });
		}

		static string EmptyResult
		{
			get { return Res.GetString("22968329-9a32-4fe5-98c6-f8b60f31f3e6", "(empty)"); }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;

			var match = Regex.Match(macro);
			var provider = match.Groups["CodeValuePairProviderName"].Value;
			var field = match.Groups["FieldName"].Value;
			var index = match.Groups["ElementNumber"].Value;
			var key = match.Groups["Key"].Value;

			ReadOnlyCodeDescriptionPairList codeDescriptionPairList;

			ZGuid keyZGuid;
			if (ZGuid.TryParse(key, out keyZGuid) && keyZGuid.IsValid)
			{
				codeDescriptionPairList = CodeDescriptionPairListProviderFactory.GetDynamicGUIDKeyedCodeDescriptionPairList(provider).GetCodeDescriptionPairList(keyZGuid);
			}
			else
			{
				var message = Res.GetString("886ba622-037f-4331-a9f5-34a4cac63629", "[{0}] Is not a valid GUID and cannot be use with Keyed Code Pair provider - [{1}]", key, provider);
				ReportMacroError(report, message);
				return string.Empty;
			}

			try
			{
				if (index.Equals("ALL", StringComparison.InvariantCultureIgnoreCase))
				{
					var propertyValues = new List<string>();

					foreach (ICodeDescription codeDescriptionPair in codeDescriptionPairList)
					{
						propertyValues.Add(GetPropertyValueByField(codeDescriptionPair, field));
					}

					result = string.Join(", ", propertyValues.ToArray());
				}
				else
				{
					int position;

					if (Int32.TryParse(index, out position))
					{
						position--;
					}
					else
					{
						position = codeDescriptionPairList.IndexOfCode(index);
					}

					if (position >= 0 && position < codeDescriptionPairList.Count)
					{
						result = GetPropertyValueByField(codeDescriptionPairList[position], field);
					}
					else
					{
						result = EmptyResult;
					}
				}
			}
			catch (InvalidFieldException ex)
			{
				ReportMacroError(report, ex.Message);
				return string.Empty;
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)KeyedCodePairValue(?:[\s]*)\((?:[\s]*)(?<CodeValuePairProviderName>[^\s]+)(?:[\s]*)\,(?:[\s]*)(?<FieldName>[^\s]+)(?:[\s]*)\,(?:[\s]*)(?<ElementNumber>[^\s,]*)(?:[\s]*)\,(?:[\s]*)(?<Key>[^\s,]+)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected string GetPropertyValueByField(ICodeDescription codeDescriptionPair, string field)
		{
			switch (field.ToUpperInvariant())
			{
				case "CODE":
					return codeDescriptionPair.Code;

				case "DESCRIPTION":
					return codeDescriptionPair.Description;

				default:
					throw new InvalidFieldException(field);
			}
		}

		[Serializable]
		class InvalidFieldException : DocumentEngineException
		{
			public InvalidFieldException(string field)
				: base(Res.GetString("088b1ff2-999c-4d6e-893f-af24c57e33fe", "Invalid field [{0}]. Should be CODE or DESCRIPTION.", field))
			{
				this.Field = field;
			}

#if NETFRAMEWORK
			protected InvalidFieldException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public readonly string Field;
		}
	}
}
