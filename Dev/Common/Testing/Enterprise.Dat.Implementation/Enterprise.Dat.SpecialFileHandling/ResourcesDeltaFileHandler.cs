using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;
using Enterprise.Dat.SpecialFileHandling.Assets;

namespace Enterprise.Dat.SpecialFileHandling
{
	public sealed class ResourcesDeltaFileHandler : DeltaFileHandler
	{
		public ResourcesDeltaFileHandler()
			: this(AssetService.Create())
		{
		}

		public ResourcesDeltaFileHandler(IAssetService assetService)
		{
			this.assetService = assetService
				?? throw new ArgumentNullException(nameof(assetService));
		}

		const string ResourcesDeltaXmlFileName = "ResourcesDelta.xml";
		const string ResourcesXmlFileName = "Resources.xml";

		protected override bool IsDeltaFile(string serverPath)
		{
			return serverPath.EndsWith(ResourcesDeltaXmlFileName);
		}

		protected override bool IsMasterFile(string serverPath)
		{
			return serverPath.EndsWith(ResourcesXmlFileName);
		}

		protected override bool ShouldUnshelveMaster(string serverPath)
		{
			return false;
		}

		public override bool ShouldUnshelveForDATCheckin(IPendingChange change)
		{
			return base.ShouldUnshelveForDATCheckin(change) || (change.ChangeType & TfsChangeType.Add) == TfsChangeType.Add;
		}

		protected override string[] UpdateForDATCheckinOnDeltaFile(IWorkspaceAccess workspace, IPendingChange change, string localTempFile)
		{
			var resourcesFilePath = workspace.GetLocalItemForServerItem(change.ServerItem.Replace(ResourcesDeltaXmlFileName, ResourcesXmlFileName));
			workspace.PendEdit(resourcesFilePath);

			var resources = new XmlDocument();
			resources.Load(resourcesFilePath);

			var language = GetLanguage(resources);

			var resourcesByKey = new Dictionary<string, XmlElement>();
			foreach (XmlElement child in resources.DocumentElement.SelectNodes("Res"))
			{
				XmlNode childKey = child.SelectSingleNode("Key");
				if (childKey != null && !resourcesByKey.ContainsKey(childKey.InnerText))
				{
					resourcesByKey.Add(childKey.InnerText, child);
				}
			}

			var deltaXml = new XmlDocument();
			deltaXml.Load(localTempFile);

			foreach (XmlElement res in deltaXml.DocumentElement.SelectNodes("Res"))
			{
				XmlElement existing = null;
				var childKey = res.SelectSingleNode("Key")?.InnerText;

				if (childKey == null)
				{
					continue;
				}

				resourcesByKey.TryGetValue(childKey, out existing);

				if (ResourceStringElementHasNoDefinitionAndShouldBeDeleted(res))
				{
					if (existing != null)
					{
						resourcesByKey.Remove(childKey);
					}
				}
				else
				{
					if (existing == null)
					{
						resourcesByKey[childKey] = res;
					}
					else
					{
						existing.InnerXml = res.InnerXml;
					}
				}
			}

			var result = ConstructResultDocument(language, resourcesByKey);
			byte[] hash;

			using (var stream = File.Open(resourcesFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
			{
				using (var writer = XmlWriter.Create(stream, new XmlWriterSettings() { Encoding = Encoding.UTF8, Indent = true, CloseOutput = false }))
				{
					result.Save(writer);
				}

				stream.Position = 0;

				using var hasher = SHA256.Create();
				hash = hasher.ComputeHash(stream);
			}

			var packageName = GeneratePackageName(language, hash);
			var packageNameFilePath = Path.Combine(Path.GetDirectoryName(resourcesFilePath), "PackageName.txt");
			File.WriteAllText(packageNameFilePath, packageName);

			assetService.UploadAsync(packageName, resourcesFilePath).Wait();

			return new[] { resourcesFilePath, packageNameFilePath };
		}

		static string GetLanguage(XmlDocument resources) => resources.DocumentElement?.Attributes["Language"]?.Value;

		static XmlDocument ConstructResultDocument(string language, Dictionary<string, XmlElement> resourcesByKey)
		{
			var result = new XmlDocument();
			var docElement = result.CreateElement("EnterpriseResources");
			result.AppendChild(docElement);

			if (language != null)
			{
				docElement.SetAttribute("Language", language);
			}

			// Sort resources by key for deterministic output.
			foreach (var pair in resourcesByKey.OrderBy(x => x.Key))
			{
				docElement.AppendChild(CloneElementFromAnotherDocument(result, pair.Value));
			}

			return result;
		}

		static string GeneratePackageName(string language, byte[] hash)
		{
			const string Prefix = "ResourceStrings/content/Translations/ResourceStrings-";
			const string Alphabet = "0123456789abcdef";

#pragma warning disable CW1060 // ZDateTime is not appropriate in a special file handler.
			var builder = new StringBuilder(Prefix.Length + (language?.Length ?? 0) + 9 + hash.Length * 2);
			builder
				.Append(Prefix)
				.Append(language)
				.AppendFormat(CultureInfo.InvariantCulture, "-{0:yyyy-MM}-", DateTime.Now);
#pragma warning restore CW1060

			for (var i = 0; i < hash.Length; i++)
			{
				var b = hash[i];
				builder.Append(Alphabet[b >> 4]);
				builder.Append(Alphabet[b & 0xf]);
			}

			return builder.ToString();
		}

		static bool ResourceStringElementHasNoDefinitionAndShouldBeDeleted(XmlElement res)
		{
			return ChildNodeIsNullOrEmpty(res, "Caption")
				&& ChildNodeIsNullOrEmpty(res, "FullDescription")
				&& ChildNodeIsNullOrEmpty(res, "ShortCaption")
				&& ChildNodeIsNullOrEmpty(res, "MediumCaption");
		}

		static bool ChildNodeIsNullOrEmpty(XmlElement parent, string childNodeName)
		{
			XmlNode result = parent.SelectSingleNode(childNodeName);

			return result == null || string.IsNullOrEmpty(result.InnerText);
		}

		static XmlElement CloneElementFromAnotherDocument(XmlDocument document, XmlElement element)
		{
			XmlElement result = document.CreateElement(element.Name);

			result.InnerXml = element.InnerXml;

			foreach (XmlAttribute a in element.Attributes)
			{
				result.SetAttribute(a.Name, a.Value);
			}

			return result;
		}

		readonly IAssetService assetService;
	}
}
