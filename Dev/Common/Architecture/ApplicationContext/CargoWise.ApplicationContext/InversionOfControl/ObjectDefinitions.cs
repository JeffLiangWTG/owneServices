using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Defines a collection of object definitions.
	/// </summary>
	[XmlRoot("objects", Namespace = "http://www.springframework.net")]
	[XmlSerializerAssembly("CargoWise.ApplicationContext.XmlSerializers")]
	public sealed class ObjectDefinitions
	{
		#region Constructors & Factory Methods

		public static ObjectDefinitions CreateUsingFile(string assemblyFilePath, string resourceName)
		{
			Argument.NotNullOrEmpty(assemblyFilePath, nameof(assemblyFilePath));
			Argument.NotNullOrEmpty(resourceName, nameof(resourceName));
			using var configurationResourceFileStream = AssemblyAccessor.GetManifestResourceStreamFromAssemblyFile(assemblyFilePath, resourceName);
			return configurationResourceFileStream == null
				? throw new InvalidOperationException($"Cannot find resource '{resourceName}' in {assemblyFilePath}.")
				: Create(configurationResourceFileStream);
		}

		public static ObjectDefinitions Create(string configurationResourceFileUri)
		{
			Argument.NotNullOrEmpty(configurationResourceFileUri, nameof(configurationResourceFileUri));
			var configurationResourceFileInfo = ParseConfigurationResourceFileUri(configurationResourceFileUri)
												?? throw new ArgumentException(string.Format("The URI ({0}) is of invalid format.", configurationResourceFileUri), nameof(configurationResourceFileUri));
			var ownerAssembly = Assembly.Load(configurationResourceFileInfo.AssemblyName) ?? throw new InvalidOperationException("The assembly name ({0}) specified in the configurationResourceFileUri is invalid.");
			using var configurationResourceFileStream = ownerAssembly.GetManifestResourceStream(configurationResourceFileInfo.Namespace + "." + configurationResourceFileInfo.FileName);
			return configurationResourceFileStream == null
				? throw new InvalidOperationException("The namespace or file name specified in the configurationResourceFileUri is invalid.")
				: Create(configurationResourceFileStream);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		static ConfigurationResourceFileInfo ParseConfigurationResourceFileUri(string configurationResourceFileUri)
		{
			Argument.NotNullOrEmpty(configurationResourceFileUri, nameof(configurationResourceFileUri));
			const string configurationResourceFileUriFormat = @"^assembly://(?<assembly>.+)/(?<namespace>.+)/(?<fileName>.+)$";
			var match = Regex.Match(configurationResourceFileUri.Trim(), configurationResourceFileUriFormat);
			if (match.Success)
			{
				return new ConfigurationResourceFileInfo { AssemblyName = match.Groups["assembly"].Value, Namespace = match.Groups["namespace"].Value, FileName = match.Groups["fileName"].Value };
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Creates an instance from an XML input stream.
		/// </summary>
		/// <param name="inputStream">The XML input stream.</param>
		/// <returns>The result instance.</returns>
		static ObjectDefinitions Create(Stream inputStream)
		{
			Argument.NotNull(inputStream, nameof(inputStream));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObjectDefinitions));
			try
			{
				if (!(xmlSerializer.Deserialize(inputStream) is ObjectDefinitions defintions))
				{
					throw new InvalidOperationException("Deserialization returned null");
				}
				return defintions;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("The configuration resource file is of invalid format.", ex);
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Contains all object definitions.
		/// </summary>
		[XmlElement("object")]
		public ObjectDefinition[] Definitions { get; set; }
		#endregion
	}
}
