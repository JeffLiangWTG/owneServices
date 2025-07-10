using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants.DocumentEngine.EmailParsing;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	#region abstract class DocumentParser<T>

	public abstract class DocumentParser<T> where T : BusinessObject
	{
		protected DocumentParser(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		protected readonly BusinessObjectFactory Factory;

		protected abstract Type TypeOfWrapper { get; }

		protected bool throwErrorOnParsing;

		protected BusinessObject[] DocBuilderParsingRoots { get; set; }

		#region Parsing

		/// <summary>
		/// Searches supplied text for 'tags' (place holders for information found on a business object).
		/// Replaces tags with the equivalent property on the document wrapper.
		/// </summary>
		/// <param name="objectToWrap">The object that is being wrapped</param>
		/// <param name="documentText">The text being parsed </param>
		/// <param name="TypeOfWrapper">Wrapper type from Document Wrappers solution </param>
		/// <returns>The parsed document text</returns>
		public ZString Parse(T objectToWrap, ZString documentText)
		{
			return ParseCore(objectToWrap, documentText);
		}

		protected virtual ZString ParseCore(T objectToWrap, ZString documentText)
		{
			DocumentWrapper docWrapper;
			try
			{
				docWrapper = GetDocWrapper(objectToWrap, Factory);
			}
			catch (Exception)
			{
				throw new DeveloperNotificationException(string.Format(NoDocWrapperForBusinessObjectMessage, objectToWrap.GetType(), TypeOfWrapper));
			}
			return ParseCore(docWrapper, documentText);
		}

		ZString ParseCore(DocumentWrapper docWrapper, ZString documentText)
		{
			ZString result;

			documentText = StripHTMLSpanFromMacros(documentText);

			if (!IsDocumentTextWellFormedBySimpleCheck(documentText))
			{
				throw new DocumentParsingFailedException(DocumentIsMalformedMessage);
			}

			result = documentText;
			int parsingIndex = 0;
			int indexOfNextStartTag = 0;
			int indexOfNextEndTag = 0;

			Stack<int> startTagStack = new Stack<int>(1);
			DocumentFieldDefinitionCollection emailTagDescriptionPairList = EmailTagFinder.FindProperties(TypeOfWrapper);

			do
			{
				indexOfNextStartTag = result.IndexOf(StartTag, parsingIndex, StringComparison.CurrentCulture);
				indexOfNextEndTag = result.IndexOf(EndTag, parsingIndex, StringComparison.CurrentCulture);

				if (indexOfNextStartTag != -1 || indexOfNextEndTag != -1)
				{
					if (indexOfNextStartTag != -1 && indexOfNextStartTag < indexOfNextEndTag)
					{
						startTagStack.Push(indexOfNextStartTag);
						parsingIndex = indexOfNextStartTag + StartTag.Length;
					}
					else
					{
						if (startTagStack.Count == 0)
						{
							throw new DocumentParsingFailedException(DocumentIsMalformedMessage);
						}

						int indexOfStartTag = startTagStack.Pop();
						int indexOfEndTag = indexOfNextEndTag;

						string tagWithoutDeliminters = result.Substring(indexOfStartTag + StartTag.Length, indexOfNextEndTag - (indexOfStartTag + StartTag.Length));
						int indexOfEndFieldName = tagWithoutDeliminters.IndexOfAny(new char[] { '(', '.' });
						string fieldNameOnly = indexOfEndFieldName > 0 ? tagWithoutDeliminters.Substring(0, indexOfEndFieldName) : tagWithoutDeliminters;
						var fieldDefinition = emailTagDescriptionPairList.GetFieldDefinition(fieldNameOnly);
						if (fieldDefinition != null)
						{
							string parsedValue = "";

							if (fieldDefinition.FieldType == DocumentFieldDefinition.FieldTypes.Property)
							{
								string propertyNameWithCorrectCasing = emailTagDescriptionPairList.GetCorrectCasedFieldName(tagWithoutDeliminters);
								parsedValue = GetPropertyValue(docWrapper, propertyNameWithCorrectCasing);
							}
							else if (fieldDefinition.FieldType == DocumentFieldDefinition.FieldTypes.Method)
							{
								string methodNameWithCorrectCasing = emailTagDescriptionPairList.GetCorrectCasedFieldName(fieldNameOnly);
								parsedValue = GetMethodReturnValue(docWrapper, tagWithoutDeliminters, methodNameWithCorrectCasing);
							}
							else if (fieldDefinition.FieldType == DocumentFieldDefinition.FieldTypes.RelatedDocumentWrapper)
							{
								parsedValue = GetRelatedDocumentWrapperValue(docWrapper, tagWithoutDeliminters, fieldNameOnly);
							}
							else if (fieldDefinition.FieldType == DocumentFieldDefinition.FieldTypes.RelatedBusinessObject)
							{
								parsedValue = GetRelatedBusinessObjectValue(docWrapper, tagWithoutDeliminters, fieldNameOnly);
							}

							result = result.Substring(0, indexOfStartTag) + parsedValue + result.Substring(indexOfEndTag + EndTag.Length);

							parsingIndex = indexOfStartTag + parsedValue.Length;
						}
						else
						{
							parsingIndex = indexOfEndTag + EndTag.Length;
						}
					}
				}
			}
			while (indexOfNextStartTag != -1 || indexOfNextEndTag != -1);

			if (DocBuilderParsingRoots != null && DocBuilderParsingRoots.Length > 0)
			{
				result = result.Replace(MacroConstants.DefaultMacroOpeningBracket, TempOpeningBraket);
				result = result.Replace(StartTag, MacroConstants.DefaultMacroOpeningBracket);
				result = result.Replace(MacroConstants.DefaultMacroClosingBracket, TempClosingBraket);
				result = result.Replace(EndTag, MacroConstants.DefaultMacroClosingBracket);

				result = ObjectFactory.Get<ITextMacroProcessor>().Replace(result, DocBuilderParsingRoots, throwErrorOnParsing);

				result = result.Replace(TempOpeningBraket, MacroConstants.DefaultMacroOpeningBracket);
				result = result.Replace(TempClosingBraket, MacroConstants.DefaultMacroClosingBracket);
			}

			return result;
		}

		protected virtual string GetPropertyValue(BusinessObject docWrapper, string propertyNameWithCorrectCasing)
		{
			return docWrapper[propertyNameWithCorrectCasing].ToString();
		}

		string GetMethodReturnValue(BusinessObject docWrapper, string tagWithoutDeliminters, string methodNameWithCorrectCasing)
		{
			object methodReturnValue = null;

			try
			{
				MethodInfo methodInfo = TypeOfWrapper.GetMethod(methodNameWithCorrectCasing, BindingFlags.Instance | BindingFlags.Public);
				if (methodInfo == null)
				{
					return string.Empty;
				}

				ParameterInfo[] paramInfos = methodInfo.GetParameters();
				if (paramInfos.Length > 0)
				{
					string[] paramValuesAsText = ParseParameters(tagWithoutDeliminters);
					if (paramValuesAsText.Length == paramInfos.Length)
					{
						methodReturnValue = InvokeMethod(docWrapper, methodInfo, paramValuesAsText, paramInfos);
					}
				}
				else
				{
					methodReturnValue = methodInfo.Invoke(docWrapper, null);
				}
			}
			catch (AmbiguousMatchException)
			{
				string[] paramValuesAsText = ParseParameters(tagWithoutDeliminters);
				foreach (var info in TypeOfWrapper.GetMethods(BindingFlags.Instance | BindingFlags.Public))
				{
					if (info.Name == methodNameWithCorrectCasing)
					{
						ParameterInfo[] paramInfos = info.GetParameters();
						if (paramValuesAsText.Length == paramInfos.Length)
						{
							methodReturnValue = InvokeMethod(docWrapper, info, paramValuesAsText, paramInfos);
							break;
						}
					}
				}
			}
			catch { }

			int indexOfCloseBracket = tagWithoutDeliminters.IndexOf(')');
			if (indexOfCloseBracket < tagWithoutDeliminters.Length - 2
				&& methodReturnValue != null)
			{
				string textToParse = tagWithoutDeliminters.Substring(indexOfCloseBracket + 2);
				if (typeof(DocumentWrapper).IsAssignableFrom(methodReturnValue.GetType()))
				{
					var returnDocumentWrapper = methodReturnValue as DocumentWrapper;
					var returnValueParser = DocumentParser.New(returnDocumentWrapper.GetType(), Factory);
					if (DocBuilderParsingRoots != null && DocBuilderParsingRoots.Length > 0)
					{
						returnValueParser.DocBuilderParsingRoots = new BusinessObject[] { returnDocumentWrapper };
					}
					methodReturnValue = returnValueParser.ParseCore(returnDocumentWrapper, StartTag + textToParse + EndTag);
				}
				else if (typeof(BusinessObject).IsAssignableFrom(methodReturnValue.GetType()))
				{
					var returnBusinessObject = methodReturnValue as BusinessObject;
					methodReturnValue = ObjectFactory.Get<ITextMacroProcessor>().Replace(MacroConstants.DefaultMacroOpeningBracket + textToParse + MacroConstants.DefaultMacroClosingBracket, new BusinessObject[] { returnBusinessObject });
				}
			}

			return methodReturnValue != null ? methodReturnValue.ToString() : string.Empty;
		}

		static string[] ParseParameters(string tagWithoutDeliminters)
		{
			int indexOfCloseBracket = tagWithoutDeliminters.LastIndexOf(')');
			int indexOfOpenBracket = tagWithoutDeliminters.IndexOf('(');
			string paramsAsText = tagWithoutDeliminters.Substring(indexOfOpenBracket + 1, indexOfCloseBracket - indexOfOpenBracket - 1);
			return new OCsvLine(paramsAsText).FieldValues;
		}

		static object InvokeMethod(BusinessObject docWrapper, MethodInfo methodInfo, string[] paramValuesAsText, ParameterInfo[] paramInfos)
		{
			try
			{
				var parameters = new object[paramInfos.Length];

				for (int i = 0; i < paramValuesAsText.Length; i++)
				{
					parameters[i] = GetMethodParam(paramInfos[i].ParameterType, paramValuesAsText[i]);
				}
				return methodInfo.Invoke(docWrapper, parameters);
			}
			catch
			{
				return null;
			}
		}

		static object GetMethodParam(Type paramType, string paramAsText)
		{
			if (paramType == typeof(ZString))
			{
				ZString textParam = paramAsText;
				return textParam;
			}

			if (paramType == typeof(ZBool))
			{
				ZBool boolParam = ZBool.False;
				if (string.Compare(paramAsText, (NoResString)"true", true) == 0)
				{
					boolParam = ZBool.True;
				}
				return boolParam;
			}

			if (paramType == typeof(ZInt))
			{
				ZInt intParam;
				ZInt.TryParse(paramAsText, out intParam);
				return intParam;
			}

			if (paramType == typeof(ZShort))
			{
				ZShort shortParam;
				ZShort.TryParse(paramAsText, out shortParam);
				return shortParam;
			}

			if (paramType == typeof(ZDecimal))
			{
				ZDecimal decimalParam;
				ZDecimal.TryParse(paramAsText, out decimalParam);
				return decimalParam;
			}

			if (paramType == typeof(ZDateTime))
			{
				ZDateTime datetimeParam;
				ZDateTime.TryParseExact(paramAsText, out datetimeParam, ZDateTime.LongTimeFormat);
				return datetimeParam;
			}

			return null;
		}

		string GetRelatedDocumentWrapperValue(BusinessObject docWrapper, string tagWithoutDeliminters, string relatedDocumentWrapperName)
		{
			string result = string.Empty;

			PropertyInfo info = TypeOfWrapper.GetProperty(relatedDocumentWrapperName);
			var relatedDocumentWrapper = info.GetGetMethod().Invoke(docWrapper, null) as DocumentWrapper;

			if (relatedDocumentWrapper != null && tagWithoutDeliminters.Contains("."))
			{
				string propertyOrMethod = tagWithoutDeliminters.Substring(tagWithoutDeliminters.IndexOf('.') + 1);
				var relatedParser = DocumentParser.New(relatedDocumentWrapper.GetType(), Factory);
				if (DocBuilderParsingRoots != null && DocBuilderParsingRoots.Length > 0)
				{
					relatedParser.DocBuilderParsingRoots = new BusinessObject[] { relatedDocumentWrapper };
				}
				result = relatedParser.ParseCore(relatedDocumentWrapper, StartTag + propertyOrMethod + EndTag);
			}

			return result;
		}

		string GetRelatedBusinessObjectValue(BusinessObject docWrapper, string tagWithoutDeliminters, string relatedDocumentWrapperName)
		{
			string result = string.Empty;

			PropertyInfo info = TypeOfWrapper.GetProperty(relatedDocumentWrapperName);
			var relatedBusinessObject = info.GetGetMethod().Invoke(docWrapper, null) as BusinessObject;

			if (relatedBusinessObject != null && tagWithoutDeliminters.Contains("."))
			{
				string propertyOrMethod = tagWithoutDeliminters.Substring(tagWithoutDeliminters.IndexOf('.') + 1);
				result = ObjectFactory.Get<ITextMacroProcessor>().Replace(MacroConstants.DefaultMacroOpeningBracket + propertyOrMethod + MacroConstants.DefaultMacroClosingBracket, new BusinessObject[] { relatedBusinessObject });
			}

			return result;
		}

		string StripHTMLSpanFromMacros(string input)
		{
			return Regex.Replace(input, @"(?<=\(\*)<span.*?>(.*?)</span>", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		#endregion

		#region Implementation

		public static bool IsDocumentTextWellFormedBySimpleCheck(string documentText)
		{
			var startTagCount = Regex.Matches(documentText, Regex.Escape(StartTag)).Count;
			var endTagCount = Regex.Matches(documentText, Regex.Escape(EndTag)).Count;
			return startTagCount == endTagCount;
		}

		public static string DocumentIsMalformedMessage => Res.GetString("DFBE18F2-6D3E-476E-9E3C-23A035B68863", "The attached document is malformed, a start tag {0} should always be followed by an end tag {1}", StartTag, EndTag);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string NoDocWrapperForBusinessObjectMessage = "Business Object of type {0} does not have a document wrapper of type {1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string TempOpeningBraket = "||&lt;||";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string TempClosingBraket = "||&gt;||";

		protected DocumentWrapper GetDocWrapper(BusinessObject objectToBeWrapped, BusinessObjectFactory factory)
		{
			BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod;
			object[] invokeParams = new object[] { objectToBeWrapped, objectToBeWrapped.Factory };
			return (DocumentWrapper)TypeOfWrapper.InvokeMember("New", bindingFlags, null, null, invokeParams);
		}

		DocumentFieldAttributeFinder EmailTagFinder
		{
			get
			{
				if (fEmailTagFinder == null)
				{
					fEmailTagFinder = new DocumentFieldAttributeFinder();
				}
				return fEmailTagFinder;
			}
		}
		DocumentFieldAttributeFinder fEmailTagFinder;

		#endregion
	}

	#endregion

	#region class DocumentParser<TBusinessObject, TDocumentWrapper>

	public class DocumentParser<TBusinessObject, TDocumentWrapper> : DocumentParser<TBusinessObject>
		where TBusinessObject : BusinessObject
		where TDocumentWrapper : DocumentWrapper
	{
		public DocumentParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return typeof(TDocumentWrapper); }
		}
	}

	#endregion

	#region class DocumentParser

	public class DocumentParser : DocumentParser<BusinessObject>
	{
		protected DocumentParser(Type sourceDocWrapperType, BusinessObjectFactory factory)
			: base(factory)
		{
			SourceDocWrapperType = sourceDocWrapperType;
		}

		protected override Type TypeOfWrapper
		{
			get { return SourceDocWrapperType; }
		}

		readonly Type SourceDocWrapperType;

		#region New

		public static DocumentParser New(Type sourceDocWrapperType, BusinessObjectFactory factory)
		{
			DocumentParser result;

			if (NewDocumentParser != null)
			{
				result = NewDocumentParser(sourceDocWrapperType, factory);
			}
			else
			{
				result = new DocumentParser(sourceDocWrapperType, factory);
			}

			return result;
		}

		protected delegate DocumentParser GetNewDocumentParser(Type sourceDocWrapperType, BusinessObjectFactory factory);

		[SuppressThreadStaticFieldMessage]
		protected static GetNewDocumentParser NewDocumentParser;

		#endregion
	}

	#endregion
}
