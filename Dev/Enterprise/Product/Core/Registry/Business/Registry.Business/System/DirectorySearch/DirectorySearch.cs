using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface IDirectorySearch
	{
		ZString DirectoryPath { get; }
		ZBool SearchSubdirectories { get; }
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DirectorySearch : RegistryBusinessObjectTemplate, IDirectorySearch
	{
		#region Schema

		public abstract class Schema
		{
			public const string DirectoryPath = "DirectoryPath";
			public const string SearchSubdirectories = "SearchSubdirectories";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DirectorySearch();
		}

		#region Bound Properties

		#region Directory Path

		public ZString DirectoryPath
		{
			get { return directoryPath; }
			set
			{
				CheckMaximumLength(DirectoryPathInfo, value);
				SetNonPersistentPropertyValue(DirectoryPathInfo, ref directoryPath, value);
			}
		}

		public ZPropertyInfo DirectoryPathInfo
		{
			get { return GetZPropertyInfo(Schema.DirectoryPath); }
		}

		ZString directoryPath;

		#endregion

		#region Search Subdirectories

		public ZBool SearchSubdirectories
		{
			get { return searchSubdirectories; }
			set { SetNonPersistentPropertyValue<ZBool>(SearchSubdirectoriesInfo, ref searchSubdirectories, value); }
		}

		public ZPropertyInfo SearchSubdirectoriesInfo
		{
			get { return GetZPropertyInfo(Schema.SearchSubdirectories); }
		}

		ZBool searchSubdirectories;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DirectoryPath, DirectoryPath);
			writer.WriteElementString(Schema.SearchSubdirectories, SearchSubdirectories.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DirectoryPath = reader.ReadElementString(Schema.DirectoryPath);
			SearchSubdirectories = new ZBool(reader.ReadElementString(Schema.SearchSubdirectories));
		}

		#endregion
	}
}
