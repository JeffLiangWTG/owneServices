using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase
{
	[CodeAlive("AddInfo will need to be parsed in the future?")]
	public class AddInfoParser
	{
		public AddInfoParser(string addInfo, string addInfoKey)
		{
			if (!string.IsNullOrEmpty(addInfo))
			{
				addInfo = "*" + addInfo;
			}

			string keyChunk = "*" + addInfoKey + "=";
			if (addInfo.IndexOf(keyChunk) == -1)
			{
				Value = "";
				AddInfoMinusKeyAndValuePair = addInfo;
			}
			else
			{
				int indexOfStartOfValue = addInfo.IndexOf(keyChunk) + keyChunk.Length;
				int indexOfEndOfValue = addInfo.IndexOf("*", indexOfStartOfValue);
				if (indexOfEndOfValue == -1)
				{
					indexOfEndOfValue = addInfo.Length;
				}

				Value = addInfo.Substring(indexOfStartOfValue, indexOfEndOfValue - indexOfStartOfValue);
				if (indexOfEndOfValue == addInfo.Length)
				{
					AddInfoMinusKeyAndValuePair = addInfo.Substring(0, addInfo.IndexOf(keyChunk));
				}
				else
				{
					AddInfoMinusKeyAndValuePair = addInfo.Substring(1, addInfo.IndexOf(keyChunk)) + addInfo.Substring(indexOfEndOfValue + 1);
				}
			}

			if (AddInfoMinusKeyAndValuePair.IndexOf("'") > -1)
			{
				AddInfoMinusKeyAndValuePair = CargoWise.Data.DataUtils.EscapeSingleQuotes(AddInfoMinusKeyAndValuePair);
			}

			AddInfoMinusKeyAndValuePair = AddInfoMinusKeyAndValuePair.Trim('*');
		}

		public readonly string Value;
		public readonly string AddInfoMinusKeyAndValuePair;
	}
}
