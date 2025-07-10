using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgCodeOrgType : RegistryBusinessObjectTemplate, IOrgCodeOrgType
	{
		ZString description;
		ZBool selected;

		public OrgCodeOrgType()
		{
		}

		public OrgCodeOrgType(ZString description)
		{
			this.description = description;
		}

		public ZString Description
		{
			get { return description; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public ZBool Selected
		{
			get { return selected; }
			set { SetNonPersistentPropertyValue<ZBool>(SelectedInfo, ref selected, value); }
		}

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(Schema.Selected); }
		}

		protected bool Selected_ReadOnly
		{
			get { return ParentCollections.Count > 0 && ((OrgCodeOrgTypeCollection)ParentCollections.First()).DefaultAlgorithm; }
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((OrgCodeOrgType)clone).description = description;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgCodeOrgType();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			description = reader.ReadElementString(Schema.Description);
			selected = new ZBool(reader.ReadElementString(Schema.Selected));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.Selected, Selected.ToString());
		}

		#region IOrgCodeOrgType Members

		string IOrgCodeOrgType.Description
		{
			get { return Description; }
		}

		bool IOrgCodeOrgType.Selected
		{
			get { return Selected; }
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string Description = "Description";
			public const string Selected = "Selected";
		}

		#endregion
	}
}
