using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core
{
	abstract class BaseFunctionExtractor
	{
		public abstract MethodInfoChainLink GetMethodInfoChainLink(Type typeBeingReflected);
		internal abstract Type AddMethodInfoChainLinks(Type typeToReflect, List<MethodInfoChainLink> chainLinks);

		protected static string ExtractFirstParameter(ref string formatParameters)
		{
			ZStringBuilder result = new ZStringBuilder();
			string remainingParameters = "";
			char[] characters = formatParameters.ToCharArray();
			bool lastCharacterWasEscape = false;
			bool amWithinQuotes = false;
			for (int count = 0; count < characters.Length; count++)
			{
				char character = characters[count];
				if (character == '\\')
				{
					lastCharacterWasEscape = true;
				}
				else
				{
					if (lastCharacterWasEscape)
					{
						result.Append(character.ToString());
						lastCharacterWasEscape = false;
					}
					else
					{
						if (character == '"')
						{
							if (!amWithinQuotes && result.IsEmpty)
							{
								amWithinQuotes = true;
							}
							else if (amWithinQuotes)
							{
								remainingParameters = formatParameters.Substring(count + 1);
								int positionOfComma = remainingParameters.IndexOf(',');
								remainingParameters = positionOfComma == -1 ? "" : remainingParameters.Substring(positionOfComma + 1);
								break;
							}
							else
							{
								result.Append(character.ToString());
							}
						}
						else if (character == ',' && !amWithinQuotes)
						{
							remainingParameters = formatParameters.Substring(count + 1);
							break;
						}
						else
						{
							result.Append(character.ToString());
						}
					}
				}
			}
			formatParameters = remainingParameters.TrimStart();
			return result.ToString();
		}

		protected static string ExtractMethodAndReturnParametersAsString(string lowerCasedFuntionName, ref string lowerCasedPropertyIdentifier, ref string unModifiedPropertyIdentifier)
		{
			string formatString = null;
			int posOfFormatFunction = lowerCasedPropertyIdentifier.IndexOf(lowerCasedFuntionName + "(");
			if (posOfFormatFunction != -1)
			{
				int posOfFormatString = posOfFormatFunction + lowerCasedFuntionName.Length + 1;
				int posOfFormatFunctionEnd = unModifiedPropertyIdentifier.LastIndexOf(")");
				if (posOfFormatFunctionEnd >= posOfFormatString)
				{
					int lengthOfFormatString = posOfFormatFunctionEnd - posOfFormatString;
					formatString = unModifiedPropertyIdentifier.Substring(posOfFormatString, lengthOfFormatString);
					lowerCasedPropertyIdentifier = lowerCasedPropertyIdentifier.Substring(0, posOfFormatFunction).TrimEnd('.');
					unModifiedPropertyIdentifier = unModifiedPropertyIdentifier.Substring(0, posOfFormatFunction).TrimEnd('.');
				}
			}
			return formatString;
		}
	}
}
