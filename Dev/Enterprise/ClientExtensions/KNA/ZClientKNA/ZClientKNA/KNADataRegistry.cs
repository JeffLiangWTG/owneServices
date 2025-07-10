using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.KNA
{
	public sealed class KNADataRegistry : RegistryItemSet
	{
		#region Instance
		public static KNADataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new KNADataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static KNADataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "Kuehne & Nagel Australia Client Extensions";

		#region DirectoryToImportXml
		public ZString DirectoryToImportXml
		{
			get { return new ZString(DirectoryToImportXmlRaw.Value); }
			set { DirectoryToImportXmlRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}
		#endregion

		#region ProcessedDirectoryForXmlFiles

		public ZString ProcessedDirectoryForXmlFiles
		{
			get { return new ZString(ProcessedDirectoryForXmlFilesRaw.Value); }
			set { ProcessedDirectoryForXmlFilesRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion

		#region Implementation

		#region DirectoryToImportXmlRaw

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal StringRegistryItem DirectoryToImportXmlRaw
		{
			get
			{
				return GetItem("DirectoryToImportXmlFiles", delegate
				{
					StringRegistryItem result = new StringRegistryItem("DirectoryToImportXmlFiles", (NoResString)Category, (NoResString)"Directory To Import Xml Files", (NoResString)"Specify Directory to store xml files for CargoWise One to access.", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region ProcessedDirectoryForXmlFilesRaw

		internal StringRegistryItem	ProcessedDirectoryForXmlFilesRaw
		{
			get
			{
				return GetItem("ProcessedDirectoryForXmlFiles", delegate
				{
					StringRegistryItem result = new StringRegistryItem("ProcessedDirectoryForXmlFiles", (NoResString)Category, (NoResString)"Processed Directory For Xml Files", (NoResString)"Specify directory where xml files will be moved after they have been processed", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#endregion
	}
}
