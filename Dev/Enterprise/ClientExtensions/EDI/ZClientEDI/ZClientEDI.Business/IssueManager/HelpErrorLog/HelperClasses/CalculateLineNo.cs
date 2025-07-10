using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	[Serializable]
	public class CalcLineNoException : Exception
	{
		public CalcLineNoException(string message)
			: base(message)
		{ }

#if NETFRAMEWORK
		protected CalcLineNoException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	public static class CalculateLineNo
	{
		public const int HIDDEN_LINE_NO = 16707566;

		public static bool XmlDataHasCalculatedSourceLineInfo(ZString occurrenceHoXmlData)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(occurrenceHoXmlData);

			var callNodes = xmlDoc.SelectNodes("ExceptionDetails/StackTrace/Call");
			if (callNodes == null || callNodes.Count == 0)
			{
				return false;
			}

			var hasCalculated = false;
			foreach (var node in callNodes)
			{
				var xmlNode = (XmlNode)node;
				if (xmlNode["Source"] != null && xmlNode["Line"] != null && xmlNode["ApproxLine"] != null)
				{
					hasCalculated = true;
					break;
				}
			}

			return hasCalculated;
		}

		public static void Calculate(HelpErrorLogOccurrence occurrence)
		{
			occurrence = new BusinessObjectFactory().Load<HelpErrorLogOccurrence>(occurrence.PK);

			if (occurrence == null)
			{
				throw new CalcLineNoException("Issue occurrence not found");
			}

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(occurrence.HO_XMLData);

			var xmlVersionNumber = xmlDoc.DocumentElement["VersionNumber"];

			if (xmlVersionNumber == null || xmlVersionNumber.InnerText == null || xmlVersionNumber.InnerText.Length == 0)
			{
				throw new CalcLineNoException("Version number is not correct");
			}

			string strVersion = xmlVersionNumber.InnerText;
			VersionNumber versionNumber = new VersionNumber(strVersion);

			if (versionNumber.IsEmpty)
			{
				throw new CalcLineNoException("Version number is empty");
			}

			var exceptionDetailsNode = xmlDoc.DocumentElement["ExceptionDetails"];
			List<string> extractFileList = new List<string>();
			GetExtractFileList(exceptionDetailsNode, extractFileList);

			if (extractFileList.Count == 0)
			{
				throw new CalcLineNoException("File list extracted from the occurrence is empty");
			}

			bool addedLineNo = false;

			try
			{
				ReleaseBuild.DeleteTempAssemblyFolder(strVersion);
				ReleaseBuild.GetAssemblyAndPdbFiles(strVersion, extractFileList);

				addedLineNo = FillInLineNo(strVersion, xmlDoc, exceptionDetailsNode);
			}
			finally
			{
				ReleaseBuild.DeleteTempAssemblyFolder(strVersion);
			}

			if (!addedLineNo)
			{
				throw new CalcLineNoException("No new information is added");
			}

			occurrence.HO_XMLData = xmlDoc.OuterXml;
			occurrence.HasChanges = true;
			occurrence.Factory.Save();
		}

		static void GetExtractFileList(XmlElement exceptionDetails, List<string> extractFileList)
		{
			if (exceptionDetails == null)
			{
				return;
			}

			var stackTraceNode = exceptionDetails["StackTrace"];

			if (stackTraceNode == null)
			{
				return;
			}

			foreach (XmlNode node in stackTraceNode)
			{
				if (node == null || node.Name != "Call" || node.InnerText == null || node.InnerText.Length == 0)
				{
					continue;
				}

				var attrs = node.Attributes;

				if (ExceptionReportRenderer.GetLineNoInfoState(attrs) == ExceptionReportRenderer.LineNoInfoState.NO_LINENO_INFO)
				{
					continue;
				}

				if (extractFileList.IndexOf(attrs["Assembly"].InnerText) == -1)
				{
					extractFileList.Add(attrs["Assembly"].InnerText);
					extractFileList.Add(Path.GetFileNameWithoutExtension(attrs["Assembly"].InnerText) + ".pdb");
				}
			}

			GetExtractFileList(exceptionDetails["InnerException"], extractFileList);

			return;
		}

		static bool FillInLineNo(string strVersion, XmlDocument xmlDoc, XmlElement exceptionDetails)
		{
			bool addedLineNo = false;

			if (exceptionDetails == null)
			{
				return false;
			}

			var stackTraceNode = exceptionDetails["StackTrace"];

			if (stackTraceNode == null)
			{
				return false;
			}

			foreach (XmlNode node in stackTraceNode)
			{
				if (node == null || node.Name != "Call" || node.InnerText == null || node.InnerText.Length == 0)
				{
					continue;
				}

				var attrs = node.Attributes;

				if (ExceptionReportRenderer.GetLineNoInfoState(attrs) == ExceptionReportRenderer.LineNoInfoState.NO_LINENO_INFO)
				{
					continue;
				}

				var assemblyName = attrs["Assembly"].InnerText;
				var assemblyFilePath = ReleaseBuild.GetAssemblyFilePath(strVersion, assemblyName);

				if (string.IsNullOrEmpty(assemblyFilePath))
				{
					continue;
				}

				string source = null;
				string line = null;
				string approxLine = null;

				if (!CalcSourceAndLineNo(assemblyFilePath, attrs, ref source, ref line, ref approxLine))
				{
					continue;
				}

				if (attrs["Source"] != null && attrs["Source"].Value == source
					&& attrs["Line"] != null && attrs["Line"].Value == line
					&& attrs["ApproxLine"] != null && attrs["ApproxLine"].Value == approxLine)
				{
					continue;
				}

				UpdateAttribute(xmlDoc, attrs, "Source", source);
				UpdateAttribute(xmlDoc, attrs, "Line", line);
				UpdateAttribute(xmlDoc, attrs, "ApproxLine", approxLine);

				var approxILAttrName = "ApproxIL";
				var advanceInfoAttrName = "AdvanceInfo";
				string advanceInfo = null;
				var version = xmlDoc.DocumentElement["VersionNumber"].InnerText;
				var className = attrs["Type"]?.InnerText ?? string.Empty;
				var methodName = attrs["Method"]?.InnerText ?? string.Empty;
				var iLOffset = attrs["ILOffset"]?.InnerText ?? string.Empty;
				var parameters = attrs["Parameters"]?.InnerText ?? string.Empty;
				var approxIL = attrs[approxILAttrName]?.InnerText ?? "false";
				if (approxIL == "true" || !CalcAdvanceInfo(version, assemblyName, className, methodName, iLOffset, parameters, ref advanceInfo, ref approxIL))
				{
					continue;
				}

				UpdateAttribute(xmlDoc, attrs, advanceInfoAttrName, Convert.ToBase64String(Encoding.UTF8.GetBytes(advanceInfo)));
				UpdateAttribute(xmlDoc, attrs, approxILAttrName, approxIL);

				addedLineNo = true;
			}

			if (FillInLineNo(strVersion, xmlDoc, exceptionDetails["InnerException"]))
			{
				addedLineNo = true;
			}

			return addedLineNo;
		}

		static bool CalcAdvanceInfo(string version, string assemblyName, string className, string methodName, string iLOffset, string parameters, ref string advanceInfo, ref string approxIL)
		{
			approxIL = "true";
			if (version.IsNullOrEmpty() || assemblyName.IsNullOrEmpty() || className.IsNullOrEmpty() || methodName.IsNullOrEmpty() || iLOffset.IsNullOrEmpty() || parameters.IsNullOrEmpty())
			{
				return false;
			}
			var result = GetInfoFromRunningSoftware(version, assemblyName, className, methodName, iLOffset, parameters);
			var versionNumber = new VersionNumber(version);
			if (versionNumber.IsEmpty)
			{
				advanceInfo = result;
				return true;
			}

			advanceInfo = result + GetMethodsInfo(version, assemblyName, className, methodName, iLOffset, parameters);

			return true;
		}

		static void UpdateAttribute(XmlDocument xmlDoc, XmlAttributeCollection attrs, string name, string value)
		{
			if (attrs[name] != null)
			{
				attrs.Remove(attrs[name]);
			}

			var newAttrSource = xmlDoc.CreateAttribute(name);
			newAttrSource.Value = value;
			attrs.Append(newAttrSource);
		}

		static bool CalcSourceAndLineNo(string assemblyName, XmlAttributeCollection attrs, ref string source, ref string line, ref string approxLine)
		{
			string className = attrs["Type"].InnerText;
			string methodName = attrs["Method"].InnerText;
			int iLOffset = Convert.ToInt32(attrs["ILOffset"].InnerText, CultureInfo.InvariantCulture);

			var readerParameters = new ReaderParameters { ReadSymbols = true };
			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyName, readerParameters))
			{
				var typeDefinition = FindTypeDefintion(assemblyDefinition.MainModule.Types, className);

				if (typeDefinition == null)
				{
					return false;
				}

				var methodDefinition = FindMethod(typeDefinition.Methods, methodName, attrs);

				if (methodDefinition == null)
				{
					return false;
				}

				var instruction = FindInstruction(methodDefinition, iLOffset, ref approxLine);

				while (instruction == null)
				{
					return false;
				}

				var sequencePoint = methodDefinition.DebugInformation.GetSequencePoint(instruction);
				source = sequencePoint.Document.Url;
				line = sequencePoint.StartLine.ToString(CultureInfo.InvariantCulture);
			}

			return true;
		}

		static TypeDefinition FindTypeDefintion(IEnumerable<TypeDefinition> originalTypes, string className)
		{
			var types = originalTypes;
			TypeDefinition typeDefinition = null;
			bool found = false;

			while (!found && types != null)
			{
				typeDefinition = types.FirstOrDefault(t => t.FullName.Replace("/", ".").Equals(className));

				if (typeDefinition == null)
				{
					types = FindNestedTypes(types, className);
				}
				else
				{
					found = true;
				}
			}

			return typeDefinition;
		}

		static IEnumerable<TypeDefinition> FindNestedTypes(IEnumerable<TypeDefinition> types, string className)
		{
			IEnumerable<TypeDefinition> nestedTypes = null;
			bool found = false;

			while (!found && !string.IsNullOrEmpty(className))
			{
				int dotIndex = className.LastIndexOf(".", StringComparison.Ordinal);

				if (dotIndex == -1)
				{
					className = "";
				}
				else
				{
					className = className.Substring(0, dotIndex);
					var typeDefinition = types.FirstOrDefault(t => t.FullName.Replace("/", ".").Equals(className));

					if (typeDefinition != null)
					{
						nestedTypes = typeDefinition.NestedTypes;
						found = true;
					}
				}
			}

			return nestedTypes;
		}

		static MethodDefinition FindMethod(IEnumerable<MethodDefinition> methods, string methodName, XmlAttributeCollection attrs)
		{
			var overloadMethods = FindOverloadedMethods(methods, methodName);

			if (overloadMethods == null || overloadMethods.Count == 0)
			{
				return null;
			}

			if (overloadMethods.Count == 1)
			{
				return overloadMethods[0];
			}

			var parameters = GetParameters(attrs);

			if (parameters == null)
			{
				return null;
			}

			var matchedMethods = FindMatchedMethods(overloadMethods, parameters);

			if (matchedMethods.Count != 1)
			{
				return null;
			}

			return matchedMethods[0];
		}

		static IList<MethodDefinition> FindOverloadedMethods(IEnumerable<MethodDefinition> methods, string methodName)
		{
			if (methods == null)
			{
				return null;
			}

			var overloadedMethods = new List<MethodDefinition>();

			foreach (var m in methods)
			{
				if (!m.HasBody)
				{
					continue;
				}

				if (m.Name.Equals(methodName))
				{
					overloadedMethods.Add(m);
				}
			}

			return overloadedMethods;
		}

		static IList<string> GetParameters(XmlAttributeCollection attrs)
		{
			if (attrs["Parameters"] == null)
			{
				return null;
			}

			return GetParametersFromString(attrs["Parameters"].InnerText);
		}

		static IList<string> GetParametersFromString(string parameters)
		{
			if (parameters == null)
			{
				return null;
			}

			if (parameters.Length == 0 || parameters == "NoParameters")
			{
				return new List<string>();
			}

			return parameters.Split(';');
		}

		static IList<MethodDefinition> FindMatchedMethods(IEnumerable<MethodDefinition> overloadMethods, IList<string> parameters)
		{
			var matchedMethods = new List<MethodDefinition>();

			foreach (var m in overloadMethods)
			{
				if (m.Parameters.Count != parameters.Count)
				{
					continue;
				}

				bool matched = true;

				for (int i = 0; i < parameters.Count; i++)
				{
					if (m.Parameters[i].ParameterType.ToString() != parameters[i])
					{
						matched = false;
						break;
					}
				}

				if (matched)
				{
					matchedMethods.Add(m);
				}
			}

			return matchedMethods;
		}

		static Instruction FindInstruction(MethodDefinition methodDefinition, int iLOffset, ref string approxLine)
		{
			// exact match
			approxLine = "false";
			var offsetInstruction = methodDefinition.Body.Instructions.FirstOrDefault(i => i.Offset == iLOffset);

			if (offsetInstruction == null)
			{
				return null;
			}

			var sequencePoint = methodDefinition.DebugInformation.GetSequencePoint(offsetInstruction);
			if (sequencePoint != null && sequencePoint.StartLine != HIDDEN_LINE_NO)
			{
				return offsetInstruction;
			}

			// find previous loosely matched instruction
			approxLine = "true";

			var preInstruction = offsetInstruction.Previous;

			while (preInstruction != null)
			{
				sequencePoint = methodDefinition.DebugInformation.GetSequencePoint(preInstruction);
				if (sequencePoint != null && sequencePoint.StartLine != HIDDEN_LINE_NO)
				{
					return preInstruction;
				}

				preInstruction = preInstruction.Previous;
			}

			// find next loosely matched instruction
			var nextInstruction = offsetInstruction.Next;

			while (nextInstruction != null)
			{
				sequencePoint = methodDefinition.DebugInformation.GetSequencePoint(nextInstruction);
				if (sequencePoint != null && sequencePoint.StartLine != HIDDEN_LINE_NO)
				{
					return nextInstruction;
				}

				nextInstruction = nextInstruction.Next;
			}

			// find any meaningful instruction
			foreach (var instruction in methodDefinition.Body.Instructions)
			{
				if (instruction != null && (sequencePoint = methodDefinition.DebugInformation.GetSequencePoint(instruction)) != null)
				{
					if (sequencePoint.StartLine == HIDDEN_LINE_NO)
					{
						approxLine = "false";
					}
					else
					{
						approxLine = "true";
					}

					return instruction;
				}
			}

			// no meaningful instruction found
			approxLine = "false";

			return null;
		}

		public static string GetInfoFromRunningSoftware(string strVersion, string assembly, string className, string methodName, string iLOffset, string parameters)
		{
			var result = new StringBuilder();
			result.Append("Information from running software\n");
			result.Append("---------------------------------\n");
			result.AppendFormat(CultureInfo.InvariantCulture, "{0, -20} {1}\n", "Version", strVersion);
			result.AppendFormat(CultureInfo.InvariantCulture, "{0, -20} {1}\n", "Assembly", assembly);
			result.AppendFormat(CultureInfo.InvariantCulture, "{0, -20} {1}\n", "Class", className);
			result.AppendFormat(CultureInfo.InvariantCulture, "{0, -20} {1}\n", "Method", methodName);
			result.AppendFormat(CultureInfo.InvariantCulture, "{0, -20} IL_{1}\n", "IL Offset", Convert.ToInt32(iLOffset, CultureInfo.InvariantCulture).ToString("x4", CultureInfo.InvariantCulture));

			if (parameters != "NoInfo")
			{
				result.AppendFormat(CultureInfo.InvariantCulture, "{0, -20} {1}\n", "Parameters", parameters);
			}

			return result.ToString();
		}

		static string GetMethodsInfo(string strVersion, string assembly, string className, string methodName, string iLOffset, string parameters)
		{
			var assemblyName = ReleaseBuild.GetAssemblyFilePath(strVersion, assembly);

			if (string.IsNullOrEmpty(assemblyName))
			{
				return "";
			}

			var readerParameters = new ReaderParameters { ReadSymbols = true };
			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyName, readerParameters))
			{
				var typeDefinition = FindTypeDefintion(assemblyDefinition.MainModule.Types, className);

				if (typeDefinition == null)
				{
					return "";
				}

				var methodDefinitions = FindAllMethods(typeDefinition.Methods, methodName, parameters);

				if (methodDefinitions == null || methodDefinitions.Count == 0)
				{
					return "";
				}

				var result = new StringBuilder();
				result.Append("\n\n");
				result.Append("Information from released software\n");
				result.Append("---------------------------------\n");

				var pdbfileName = Path.GetFileNameWithoutExtension(assembly) + ".pdb";

				result.Append("Assembly = " + assembly + "\n");
				result.Append("Pdb = " + pdbfileName + "\n");
				result.Append("Class = " + typeDefinition.ToString() + "\n");

				var methodCount = methodDefinitions.Count;
				int methodIndex = 0;
				var intIlOffset = Convert.ToInt32(iLOffset, CultureInfo.InvariantCulture);

				foreach (var m in methodDefinitions)
				{
					result.Append("\n");

					if (methodCount > 1)
					{
						methodIndex++;
						result.AppendFormat(CultureInfo.InvariantCulture, "Method {0}: ", methodIndex);
					}

					result.Append(m.ToString() + "\n");

					foreach (var i in m.Body.Instructions)
					{
						if (methodCount == 1 && i.Offset == intIlOffset)
						{
							result.Append("*** ");
						}
						else
						{
							result.Append("    ");
						}

						var sequencePoint = m.DebugInformation.GetSequencePoint(i);
						if (sequencePoint != null && sequencePoint.StartLine != HIDDEN_LINE_NO)
						{
							result.AppendFormat(CultureInfo.InvariantCulture, "Line_{0, -6}", sequencePoint.StartLine);
						}
						else
						{
							result.Append(new string(' ', 11));
						}

						result.Append(i.ToString() + "\n");
					}
				}

				return result.ToString();
			}
		}

		static IList<MethodDefinition> FindAllMethods(IEnumerable<MethodDefinition> methods, string methodName, string strParameters)
		{
			var overloadMethods = FindOverloadedMethods(methods, methodName);

			if (overloadMethods == null || overloadMethods.Count == 0 || overloadMethods.Count == 1 || strParameters == "NoInfo")
			{
				return overloadMethods;
			}

			var parameters = GetParametersFromString(strParameters);

			if (parameters == null)
			{
				return overloadMethods;
			}

			var matchedMethods = FindMatchedMethods(overloadMethods, parameters);

			if (matchedMethods.Count == 0)
			{
				return overloadMethods;
			}
			else
			{
				return matchedMethods;
			}
		}
	}
}
