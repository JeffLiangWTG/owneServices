using System.Globalization;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	public static class DataVersionExtensions
	{
		const string dataMajorVersionAttibute = "DataMajorVersion";
		const string dataMinorVersionAttibute = "DataMinorVersion";

		public static XDocument AddDataVersion(this XDocument xml)
		{
			AddVersionNumber(xml, dataMajorVersionAttibute, VisualizerDocumentDataVersion.DocumentData.Major);
			AddVersionNumber(xml, dataMinorVersionAttibute, VisualizerDocumentDataVersion.DocumentData.Minor);

			return xml;
		}

		static void AddVersionNumber(XDocument xml, string versionAttribute, int versionNumber)
		{
			if (xml == null || xml.Root == null)
			{
				return;
			}

			var attrib = xml.Root.Attribute(versionAttribute);

			if (attrib == null)
			{
				xml.Root.Add(new XAttribute(versionAttribute, versionNumber));
			}
			else
			{
				attrib.Value = versionNumber.ToString(CultureInfo.InvariantCulture);
			}
		}

		public static VersionLabel GetDataVersion(this XDocument xml)
		{
			var majorVersion = GetVersion(xml, dataMajorVersionAttibute);
			var minorVersion = GetVersion(xml, dataMinorVersionAttibute);

			return majorVersion != null && minorVersion != null ? new VersionLabel(majorVersion.Value, minorVersion.Value) : new VersionLabel(0, 0);
		}

		static int? GetVersion(XDocument xml, string versionAttribute)
		{
			if (xml == null || xml.Root == null)
			{
				return null;
			}

			var attrib = xml.Root.Attribute(versionAttribute);

			if (attrib == null)
			{
				return null;
			}

			var version = 0;
			return int.TryParse(attrib.Value, out version) ? version : null;
		}

		public static bool RequiresTransformation(this XDocument xml)
		{
			Argument.NotNull(xml, nameof(xml));

			var xmlVersion = xml.GetDataVersion();
			var currentVersion = VisualizerDocumentDataVersion.DocumentData;

			return xmlVersion.CompareTo(currentVersion) < 0;
		}

		public static Try<XDocument> RunTransformation(this XDocument xml, string dataStoreName, INotificationsHandler notificationsHandler)
		{
			Argument.NotNull(xml, nameof(xml));
			Argument.NotNullOrEmpty(dataStoreName, nameof(dataStoreName));
			Argument.NotNull(notificationsHandler, nameof(notificationsHandler));

			var xmlVersion = xml.GetDataVersion();

			var director = new DataTransformationDirector(xmlVersion, dataStoreName, notificationsHandler);

			return director.Transform(xml);
		}
	}
}
