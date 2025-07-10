using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

#if DEBUG
using CargoWise.IO;
#endif

namespace Enterprise.ZArchitecture.Web.GUI
{
	/// <summary>
	/// Resource definition
	/// </summary>
	public class ZWebResource
	{
		public ZWebResource(Type controlType, string resourceName, ZPage page)
		{
			this.ControlType = controlType;
			this.fName = resourceName;
			this.Page = page;

#if DEBUG
			if (Globals.IsTest && page != null && !string.IsNullOrEmpty(page.ServerMappedPathForTest))
			{
				SetServerMappedPathForTest(page.ServerMappedPathForTest);
			}
#endif
		}

		public ZWebResource(Type controlType, string resourceName, ZPage page, string alternativeLocation) : this(controlType, resourceName, page)
		{
			this.fAlternativeLocation = alternativeLocation;
		}

		public ZWebResource(Type controlType, string resourceName, ZPage page, string alternativeLocation, Assembly callerAssembly) : this(controlType, resourceName, page, alternativeLocation)
		{
			this.fCallerAssembly = callerAssembly;
		}

		/// <summary>
		/// Extract resource to a Server path
		/// </summary>
		/// <param name="ServerPath"></param>
		public void Extract()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				int index = DiskFileName.IndexOf(RuntimePrefix);
				string relativePath = DiskFileName.Substring(index, DiskFileName.Length - index);

				ExtractToFile(Path.Combine(ServerMappedPath, relativePath));
			}
			else
#endif
			{
				ExtractToFile(Path.Combine(ServerMappedPath, DiskFileName));
			}
		}

		/// <summary>
		/// Name of the resource file including path
		/// </summary>
		public string FileName
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					int index = DiskFileName.IndexOf(RuntimePrefix);
					string relativePath = DiskFileName.Substring(index, DiskFileName.Length - index);

					return VirtualPathUtility.Combine(ApplicationRoot, relativePath);
				}
				else
#endif
				{
					return VirtualPathUtility.Combine(ApplicationRoot, DiskFileName);
				}
			}
		}

		public event ZWebResourceExtractEventHandler OnExtract;

		protected string ApplicationRoot
		{
			get
			{
				return (Page != null) ? Page.AppInstance.ApplicationRoot : "/";
			}
		}

		protected string DiskFileName
		{
			get
			{
				return Path.Combine(RuntimeDirectory, fName);
			}
		}

		protected string ServerMappedPath
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return ServerMappedPathForTesting;
				}
				else
#endif
				{
					return Page.Server.MapPath(ApplicationRoot);
				}
			}
		}

#if DEBUG
		public void SetServerMappedPathForTest(string tempPath)
		{
			if (!Globals.IsTest)
			{
				throw new InvalidOperationException("Should only be used for testing");
			}
			fTempServerMappedPathForTesting = tempPath;
		}

		public ZString ServerMappedPathForTesting
		{
			get { return fTempServerMappedPathForTesting; }
			set { fTempServerMappedPathForTesting = value; }
		}
		ZString fTempServerMappedPathForTesting;
#endif

		protected string ResourceLocation
		{
			get
			{
				string prefix = (string.IsNullOrEmpty(fAlternativeLocation)) ? ControlType.FullName : fAlternativeLocation;
				return prefix + "." + fName;
			}
		}

		protected void ExtractToFile(string extractFileName)
		{
			if (!File.Exists(extractFileName))
			{
				using (Stream resourceStream = CallerAssembly.GetManifestResourceStream(ResourceLocation))
				{
					if (resourceStream == null)
					{
						throw new ApplicationException(String.Format("Resource {0} could not be found.", ResourceLocation));
					}

					bool hasBeenExtracted = false;
					lock (StaticLock)
					{
						extractFileName = extractFileName.Length >= 260 && !extractFileName.StartsWith(LONG_PATH_PREFIX) ? LONG_PATH_PREFIX + extractFileName : extractFileName; // Fix Win32 Maximum Path Length Limitation
						if (!File.Exists(extractFileName))
						{
							MakeSureFolderIsCreated(extractFileName);
							using (FileStream resourceFile = File.Create(extractFileName))
							{
								byte[] buffer = new byte[0x2000];
								int bytesRead = 0;
								while ((bytesRead = resourceStream.Read(buffer, 0, buffer.Length)) > 0)
								{
									resourceFile.Write(buffer, 0, bytesRead);
								}
								resourceFile.Flush();
							}
							hasBeenExtracted = true;
						}
					}
					if (hasBeenExtracted && OnExtract != null)
					{
						OnExtract(this, new ZWebResourceExtractEventArgs(extractFileName));
					}
				}
			}
		}

		protected void MakeSureFolderIsCreated(string fileName)
		{
			string folder = Path.GetDirectoryName(fileName);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
		}

		protected byte[] ReadFileToEnd(Stream stream)
		{
			byte[] data = new byte[stream.Length];
			int offset;
			int bytesRead;

			bytesRead = stream.Read(data, 0, data.Length);
			offset = bytesRead;
			while (offset < stream.Length)
			{
				bytesRead = stream.Read(data, offset, (int)stream.Length - offset);
				if (bytesRead < 1)
				{
					break;
				}

				offset += bytesRead;
			}

			return data;
		}

		protected Type ControlType;
		protected ZPage Page;
		protected internal string fName;
		protected string fAlternativeLocation;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "directory name should not be translated")]
		protected const string RuntimePrefix = "Runtime";
		readonly static object StaticLock = new object();
		const string LONG_PATH_PREFIX = @"\\?\";

		#region Caller Assembly

		protected Assembly CallerAssembly
		{
			get { return fCallerAssembly ?? ControlType.Assembly; }
		}
		protected Assembly fCallerAssembly;

		#endregion

		protected string RuntimeDirectory
		{
			get
			{
				AssemblyName assemblyName = CallerAssembly.GetName();

				StringBuilder dirStringBuilder = new StringBuilder();
				Type tempType = ControlType;
				do
				{
					dirStringBuilder.Insert(0, tempType.Name + @"\");
					tempType = tempType.BaseType;
				} while (tempType != null && typeof(IContainResources).IsAssignableFrom(tempType));

				if (!string.IsNullOrEmpty(AssemblyVersion))
				{
					dirStringBuilder.Insert(0, AssemblyVersion + @"\");
				}
				dirStringBuilder.Insert(0, assemblyName.Name + @"\");
				dirStringBuilder.Insert(0, @"\");
				dirStringBuilder.Insert(0, RuntimePrefix);

#if DEBUG
				if (Globals.IsTest)
				{
					string tempPath = Temp.TempPath;
					dirStringBuilder.Insert(0, tempPath);
				}
#endif

				return dirStringBuilder.ToString().Replace(".", "_");
			}
		}

		public string AssemblyVersion
		{
			get
			{
				if (string.IsNullOrEmpty(fAssemblyVersion))
				{
					AssemblyFileVersionAttribute assemblyVersionAttribute = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(CallerAssembly, typeof(AssemblyFileVersionAttribute));
					fAssemblyVersion = (assemblyVersionAttribute != null) ? assemblyVersionAttribute.Version : "";
				}
				return fAssemblyVersion;
			}
		}
		protected string fAssemblyVersion;
	}
}
