using CargoWise.Common;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	sealed class UnknownElementParser : IElementNavigator
	{
		internal UnknownElementParser(XmlReader reader, string name, string errorMessagePrefix, IElementParser parentParser)
		{
			this.reader = Argument.NotNull(reader, "XmlReader reader");
			this.name = Argument.NotNull(name, "string name");
			this.errorMessagePrefix = (errorMessagePrefix ?? " -") + " " + Res.GetString("b0a7b836-cda6-41aa-a3a2-bd08098f9d99", "Unrecognized element <{0}> found.", name);
			this.parentParser = parentParser;
		}

		readonly XmlReader reader;
		readonly string name;
		readonly string errorMessagePrefix;
		readonly IElementParser parentParser;

		#region IElementParser Implementation

		IElementNavigator IElementNavigator.ParentElement
		{
			get { return parentParser; }
		}

		string IElementNavigator.CurrentElementName
		{
			get { return name; }
		}

		#endregion

		public void ParseOutContent(ICurrentElement startElement)
		{
			int nestingLevel = 0;
			var startLine = startElement.OpenLineNumber;
			var isSimpleType = true;

			while (XmlRegexes.SelfEndingElement.IsMatch(startElement.Name))
			{
				reader.AddWarning(startLine, ElementPath.FullName, Res.GetString("ea049f31-d788-494f-9f6a-1988e82958c5", "{0} Element skipped.", errorMessagePrefix));
				return;
			}

			while (!reader.EndOfStream)
			{
				IXmlDomObject firstTag = null;
				IXmlDomObject secondTag = null;
				IXmlDomObject thirdTag = null;

				try
				{
					firstTag = reader.GetNextTag();
					var currentElement = firstTag as ICurrentElement;
					if (currentElement is null)
					{
						break;
					}

					if (XmlRegexes.SelfEndingElement.IsMatch(currentElement.Name))
					{
						isSimpleType = false;
						secondTag = reader.GetNextTag();
						currentElement = secondTag as ICurrentElement;
						if (currentElement is null)
						{
							break;
						}
					}

					var closingTagMatch = XmlRegexes.ValidClosingTag.Match(currentElement.Name);
					if (closingTagMatch.Success)
					{
						if (closingTagMatch.Groups["elementName"].Value == name)
						{
							if (nestingLevel == 0)
							{
								if (isSimpleType)
								{
									reader.AddWarning(startLine, ElementPath.FullName, Res.GetString("ea049f31-d788-494f-9f6a-1988e82958c5", "{0} Element skipped.", errorMessagePrefix));
								}
								else
								{
									reader.AddWarning(startLine, ElementPath.FullName, Res.GetString("56fdb319-2065-427d-800e-1f8825024dee", "{0} Element and all its children skipped.", errorMessagePrefix));
								}

								return;
							}
							else
							{
								nestingLevel--;
							}
						}
					}
					else
					{
						closingTagMatch = XmlRegexes.FullClosingTag.Match(currentElement.Name);
						if (closingTagMatch.Success)
						{
							reader.CurrentElementParser = this;
							reader.AbortWithUnexpectedClosingTagFoundException(currentElement.OpenLineNumber, startLine, name, closingTagMatch.Groups["tag"].Value);
						}
					}

					isSimpleType = false;

					thirdTag = reader.GetNextTag();
					if (reader.EndOfStream)
					{
						reader.CurrentElementParser = this;
						reader.AbortWithNoClosingTagFoundException(startLine);
					}

					currentElement = (ICurrentElement)thirdTag;

					var openingTagMatch = XmlRegexes.ValidOpeningTag.Match(currentElement.Name);
					if (openingTagMatch.Success && openingTagMatch.Groups["elementName"].Value == name)
					{
						nestingLevel++;
					}
				}
				finally
				{
					firstTag?.Dispose();
					secondTag?.Dispose();
					thirdTag?.Dispose();
				}
			}
		}
	}
}
