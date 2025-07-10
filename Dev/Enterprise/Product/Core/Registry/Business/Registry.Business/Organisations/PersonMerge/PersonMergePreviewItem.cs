using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PersonMergePreviewItem : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string FriendlyName = "FriendlyName";
			public const string ColumnName = "ColumnName";
			public const string Visibility = "Visibility";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PersonMergePreviewItem();
		}

		#endregion

		#region Properties

		#region FriendlyName

		[MaxLength(50)]
		[ReadOnly(true)]
		public ZString FriendlyName
		{
			get { return friendlyName; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(FriendlyNameInfo, ref friendlyName, value);
			}
		}
		ZString friendlyName;

		public ZPropertyInfo FriendlyNameInfo
		{
			get { return GetZPropertyInfo(Schema.FriendlyName); }
		}

		#endregion

		#region ColumnName

		[MaxLength(50)]
		[ReadOnly(true)]
		public ZString ColumnName
		{
			get { return columnName; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ColumnNameInfo, ref columnName, value);
			}
		}
		ZString columnName;

		public ZPropertyInfo ColumnNameInfo
		{
			get { return GetZPropertyInfo(Schema.ColumnName); }
		}

		#endregion

		#region Visibility 

		[ReadOnly(false)]

		public ZBool Visibility
		{
			get { return visibility; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(VisibilityInfo, ref visibility, value);
			}
		}
		ZBool visibility;

		public ZPropertyInfo VisibilityInfo
		{
			get { return GetZPropertyInfo(Schema.Visibility); }
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.FriendlyName, FriendlyName);
			writer.WriteElementString(Schema.ColumnName, ColumnName);
			writer.WriteElementString(Schema.Visibility, Visibility.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FriendlyName = reader.ReadElementString(Schema.FriendlyName);
			ColumnName = reader.ReadElementString(Schema.ColumnName);
			Visibility = reader.ReadElementStringAsZBool(Schema.Visibility);
		}

		#endregion
	}
}
