using System;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using CargoWise.BuildTools;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Upgrades;
using WTG.DevTools.Definitions;

namespace Enterprise.ZArchitecture.Core
{
	#region SuppressResourceStringsCheckRegion
	public sealed class ReleaseInfo : IReleaseInfo
	{
		ReleaseInfo()
		{
		}

		public ReleaseInfo(string xmlFilePath)
		{
			if (xmlFilePath == null)
			{
				throw new ArgumentNullException(nameof(xmlFilePath));
			}
			this.xmlFilePath = xmlFilePath;
		}

		public static ReleaseInfo Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ReleaseInfo();
				}
				return instance;
			}
		}

		public VersionNumber VersionNumber
		{
			get
			{
				if (!versionNumber.HasValue)
				{
					versionNumber = new VersionNumber(GetInnerText("VersionNumber"));
				}
				return versionNumber.Value;
			}
		}

		public DateTime ExeDate
		{
			get
			{
				if (!exeDate.HasValue)
				{
					exeDate = DateTime.ParseExact(GetInnerText("ExeDate"), ExeDateFormat, Culture.Invariant);
				}
				return exeDate.Value;
			}
		}

		public string ReleaseRing
		{
			get { return GetInnerText("ReleaseRing"); }
		}

		public DateTime ReleaseDate => VersionNumber.GetReleaseDate();

		public string ReleaseDisplayText
		{
			get
			{
				if (releaseDisplayText == null)
				{
					releaseDisplayText = GetReleaseDisplayText(ReleaseRing, VersionNumber);
				}
				return releaseDisplayText;
			}
		}

		public static string GetReleaseDisplayText(string releaseRing, VersionNumber versionNumber)
		{
			var ring = ReleaseRings.Lookup(releaseRing);
			var result = ring != null ? ring.ShortDescription : string.Empty;

			if (result.Length > 0)
			{
				result += " ";
			}

			result += versionNumber.GetReleaseDate().ToString(ReleaseDateFormat);
			if (versionNumber.Patch != 0)
			{
				result += " patch " + versionNumber.Patch;
			}

			return result;
		}

		XPathNavigator Navigator
		{
			get
			{
				if (navigator == null)
				{
					Stream stream;
					XPathDocument document;
					if (xmlFilePath == null)
					{
						string mainExePath = GetMainExePath();
						stream = AssemblyAccessor.GetManifestResourceStreamFromAssemblyFile(mainExePath, "Enterprise." + XmlFileName);
					}
					else
					{
						stream = File.OpenRead(xmlFilePath);
					}
					using (stream)
					{
						document = new XPathDocument(stream);
					}
					navigator = document.CreateNavigator();
				}
				return navigator;
			}
		}

		string GetMainExePath()
		{
			string result = Path.Combine(AssemblyLoader.GetBinPath(), ExeFileNames.CargoWiseOneExeForVersionInfo);
			if (!File.Exists(result))
			{
				throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, "Failed to locate the {0} executable at '{1}' to obtain its Release Info", Constants.ProductName, result));
			}

			return result;
		}

		string GetInnerText(string element)
		{
			XPathNavigator node = Navigator.SelectSingleNode("//ReleaseInfo/" + element);
			return (node != null) ? node.Value : "";
		}

		VersionNumber? versionNumber;
		DateTime? exeDate;
		XPathNavigator navigator;
		string releaseDisplayText;
		readonly string xmlFilePath;
		static ReleaseInfo instance;
		const string ExeDateFormat = "yyyy-MM-dd HH:mm:ss";
		public const string ReleaseDateFormat = "yyyy MMM dd";
		public const string XmlFileName = "ReleaseInfo.xml";

		#region Test
#if DEBUG

		public static void ClearInstanceForTest()
		{
			instance = null;
		}

		public static IDisposable SetTemporaryInstanceForTesting(ReleaseInfo temporaryInstance)
		{
			var oldInstance = instance;
			instance = temporaryInstance;
			return new DisposableAction(() =>
			{
				instance = oldInstance;
			});
		}

		public static ReleaseInfo CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()
		{
			var currentTime = DateTime.Now;
			var currentVersionInfo = string.Format(CultureInfo.InvariantCulture, $"{currentTime.Year % 1000}.{currentTime.Month}.{currentTime.Day}.0");
			return CreateNewInstanceForTesting(currentVersionInfo, currentTime.ToString(ExeDateFormat), "ALP");
		}

		public static ReleaseInfo CreateNewInstanceForTesting(string versionNumber, DateTime exeDate, string releaseRing)
		{
			return CreateNewInstanceForTesting(versionNumber, exeDate.ToString(ExeDateFormat), releaseRing);
		}

		public static ReleaseInfo CreateNewInstanceForTesting(string versionNumber, string exeDate, string releaseRing)
		{
			ReleaseInfo result;
			using (TempFile tempFile = TempFile.New())
			{
				CreateNewFileForTesting(tempFile.Filename, versionNumber, exeDate, releaseRing);
				result = new ReleaseInfo(tempFile.Filename);
				XPathNavigator navigator = result.Navigator; // Invoke the lazy getter.
			}
			return result;
		}

		public static void CreateNewFileForTesting(string filePath, string versionNumber, DateTime exeDate, string releaseRing)
		{
			CreateNewFileForTesting(filePath, versionNumber, exeDate.ToString(ExeDateFormat), releaseRing);
		}

		public static void CreateNewFileForTesting(string filePath, string versionNumber, string exeDate, string releaseRing)
		{
			using (StreamWriter writer = File.CreateText(filePath))
			{
				writer.Write("<ReleaseInfo><VersionNumber>" + versionNumber + "</VersionNumber><ExeDate>" + exeDate + "</ExeDate><ReleaseRing>" + releaseRing + "</ReleaseRing></ReleaseInfo>");
			}
		}
#endif
		#endregion
	}
	#endregion
}
