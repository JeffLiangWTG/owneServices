using System.Collections.Specialized;
using System.IO;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;

namespace Enterprise.eHubMessaging.Business.Extensions
{
	public static class Extensions
	{
		internal static void MergeStringCollection(this StringCollection target, StringCollection source)
		{
			foreach (var item in source)
			{
				target.Add(item);
			}
		}

		public static bool ReadUntilMatch(this XmlReader reader, string name, string namespaceName = "")
		{
			if (reader.LocalName != name || reader.NamespaceURI != namespaceName)
			{
				if (!reader.ReadToFollowing(name, namespaceName))
				{
					return false;
				}
			}

			return true;
		}

		public static string GetElementAsString(this XmlReader reader, string name, string namespaceName = "")
		{
			if (reader.LocalName != name || reader.NamespaceURI != namespaceName)
			{
				if (!reader.ReadToFollowing(name, namespaceName))
				{
					return string.Empty;
				}
			}

			return reader.ReadElementContentAsString();
		}

		public static Stream GetElementAsStream(this XmlReader reader, string name, string namespaceName = "")
		{
			var result = new VirtualMemoryStream();

			if (reader.LocalName != name || reader.NamespaceURI != namespaceName)
			{
				reader.ReadToFollowing(name, namespaceName);
			}

			if (!reader.EOF)
			{
				reader.Read();
				reader.WriteToStream(result);
			}

			return result;
		}

		public static string GetFirstCharsAsString(this Stream stream, int numberOfChars)
		{
			stream.Position = 0;
			var reader = new StreamReader(stream);
			int bufferLength = numberOfChars;
			if (stream.Length < numberOfChars)
			{
				bufferLength = (int)stream.Length;
			}

			var buffer = new char[bufferLength];
			reader.Read(buffer, 0, bufferLength);
			stream.Position = 0;
			return new string(buffer);
		}
	}
}
