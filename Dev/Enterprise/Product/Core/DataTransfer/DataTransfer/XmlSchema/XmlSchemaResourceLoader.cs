using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using CargoWise.IO;

namespace Enterprise.DataTransfer.Xml
{
	public class XmlSchemaResourceLoader
	{
		public XmlSchema ReadSchema(Assembly ass, string baseResourceName, string relativeResourceName)
		{
			return ReadSchema(ass, baseResourceName, relativeResourceName, new ArrayList());
		}

		#region Implementation

		XmlSchema ReadSchema(Assembly ass, string baseResourceName, string relativeResourceName, ArrayList dontIncludeThese)
		{
			dontIncludeThese.Add(relativeResourceName);

			string fullResourceName = GetFullResourceNameFromBaseAndRelativeName(baseResourceName, relativeResourceName);
			Stream xsdStreamWithIncludes = GetXmlSchemaStreamFromResource(ass, fullResourceName);

			string[] includeLocations;
			Stream xsdStream = RemoveIncludes(xsdStreamWithIncludes, out includeLocations);
			XmlSchema schema = fBuilder.Read(xsdStream);

			foreach (string includeLocation in includeLocations)
			{
				if (!ContainsStringCaseInsensitive(dontIncludeThese, includeLocation))
				{
					string baseResourceNameForIncludePath = GetBaseResourceNameFromFullResourceName(fullResourceName);
					XmlSchema includedSchema = ReadSchema(ass, baseResourceNameForIncludePath, includeLocation, dontIncludeThese);
					XmlSchemaInclude include = new XmlSchemaInclude();
					include.Schema = includedSchema;
					schema.Includes.Add(include);
				}
			}
			return schema;
		}

		bool ContainsStringCaseInsensitive(ICollection collection, string str)
		{
			bool result = false;
			foreach (string value in collection)
			{
				if (value.ToLower() == str.ToLower())
				{
					result = true;
					break;
				}
			}
			return result;
		}

		string GetFullResourceNameFromBaseAndRelativeName(string baseResourceName, string relativeResourceName)
		{
			string result = baseResourceName;
			relativeResourceName = relativeResourceName.Trim();
			if (relativeResourceName.StartsWith("\\") || relativeResourceName.StartsWith("/"))
			{
				result = "";
			}
			while (relativeResourceName.StartsWith("..\\") || relativeResourceName.StartsWith("../"))
			{
				relativeResourceName = relativeResourceName.Substring(3);
				int lastDotIndex = result.LastIndexOf(".");
				result = (lastDotIndex == -1) ? "" : result.Substring(0, lastDotIndex);
			}
			relativeResourceName = relativeResourceName.Replace("\\", ".").Replace("/", ".");

			result = result.Trim('.') + "." + relativeResourceName.Trim('.');
			return result.Trim('.');
		}

		string GetBaseResourceNameFromFullResourceName(string fullResourceName)
		{
			string result = fullResourceName;
			if (result.EndsWith(".xsd"))
			{
				result = result.Substring(0, result.Length - 4);
			}
			int lastDotIndex = result.LastIndexOf(".");
			result = (lastDotIndex == -1) ? "" : result.Substring(0, lastDotIndex);
			return result;
		}

		Stream GetXmlSchemaStreamFromResource(Assembly ass, string fullResourceName)
		{
			return GetXmlSchemaStreamFromResource(ass, fullResourceName, true, new System.Collections.Generic.LinkedList<string>());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		Stream GetXmlSchemaStreamFromResource(Assembly ass, string fullResourceName, bool throwIfNotFound, System.Collections.Generic.LinkedList<string> backTrack)
		{
			backTrack.AddLast(ass.GetName().FullName);
			Stream result = GetManifestResourceStreamCaseInsensitive(ass, fullResourceName);

			foreach (AssemblyName referencedAssemblyName in ass.GetReferencedAssemblies())
			{
				if (result != null)
				{
					break;
				}

				if (!referencedAssemblyName.Name.StartsWith("ms") && !referencedAssemblyName.Name.StartsWith("System.") && !referencedAssemblyName.Name.StartsWith("Microsoft."))
				{
					if (!backTrack.Contains(referencedAssemblyName.FullName))
					{
						try
						{
							Assembly referencedAssembly = Assembly.Load(referencedAssemblyName);
							result = GetXmlSchemaStreamFromResource(referencedAssembly, fullResourceName, false, backTrack);
						}
						catch (FileNotFoundException) // FileNotFoundException is ignored since not all referenced assemblies are required, such as LiteDB.dll, review in case any unit tests fail
						{
						}
					}
				}
			}

			if (throwIfNotFound && result == null)
			{
				throw new InvalidOperationException("Could not find resource " + fullResourceName + " from this assembly or any referenced assemblies");
			}

			return result;
		}

		Stream GetManifestResourceStreamCaseInsensitive(Assembly ass, string fullResourceName)
		{
			Stream result = null;
			foreach (string nextResourceName in ass.GetManifestResourceNames())
			{
				if (nextResourceName.ToLower() == fullResourceName.ToLower())
				{
					result = ass.GetManifestResourceStream(nextResourceName);
					break;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		Stream RemoveIncludes(Stream xsdStream, out string[] includeLocations)
		{
			includeLocations = Array.Empty<string>();
			StringBuilder xsd = new StringBuilder();
			ArrayList includeLocationsList = new ArrayList();

			using (TextReader reader = new StreamReader(xsdStream))
			{
				Regex regex = new Regex("^(?<begin>.*)<xs:include schemaLocation=\"(.+xsd)\"\\s*/>(?<end>.*)$", RegexOptions.Singleline | RegexOptions.IgnoreCase);

				string line;
				while ((line = reader.ReadLine()) != null)
				{
					Match match = regex.Match(line);
					if (match.Success)
					{
						line = match.Result("${begin}") + match.Result("${end}");
						includeLocationsList.Add(match.Groups[1].Value);
					}
					xsd.Append(line);
				}
			}

			includeLocations = (string[])includeLocationsList.ToArray(typeof(string));
			return StreamConverter.StringToStream(xsd.ToString());
		}

		readonly XsdSchemaBuilder fBuilder = new XsdSchemaBuilder();

		#endregion
	}
}
