using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ClientInTemplateSelectionCriteriaOrgType : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string OrgTypeCode = "OrgTypeCode";
			public const string OrgTypeDescription = "OrgTypeDescription";
		}

		#endregion

		public ClientInTemplateSelectionCriteriaOrgType()
		{
		}

		public ClientInTemplateSelectionCriteriaOrgType(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public ClientInTemplateSelectionCriteriaOrgType(string orgTypeCode)
		{
			OrgTypeCode = orgTypeCode;
		}

		#region Bound Properties

		#region OrgTypeCode

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString OrgTypeCode
		{
			get { return orgTypeCode; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OrgTypeCodeInfo, ref orgTypeCode, value);
				orgTypeDescription = null;
			}
		}

		public ZPropertyInfo OrgTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.OrgTypeCode); }
		}

		ZString orgTypeCode;

		#endregion

		#region OrgTypeDescription

		public MultilingualString OrgTypeDescription
		{
			get
			{
				if (orgTypeDescription == null)
				{
					if (ParentCollection == null || ParentCollection.Parent == null)
					{
						orgTypeDescription = (NoResString)"";
					}
					else
					{
						orgTypeDescription = ClientInTemplateSelectionCriteriaCollectionRegistryItem.GetDefaultOrgTypesByCode(ParentCollection.Parent.ProcessTaskCode).GetCodeDescriptionPairList().GetMultilingualDescriptionFromCode(OrgTypeCode) ?? (NoResString)"";
					}
				}

				return orgTypeDescription;
			}
		}

		public ZPropertyInfo OrgTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.OrgTypeDescription); }
		}

		MultilingualString orgTypeDescription;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.OrgTypeCode, OrgTypeCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OrgTypeCode = reader.ReadElementString(Schema.OrgTypeCode);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ClientInTemplateSelectionCriteriaOrgType(fallbackLevel, factory);
		}

		ClientInTemplateSelectionCriteriaOrgTypesCollection ParentCollection
		{
			get { return (ClientInTemplateSelectionCriteriaOrgTypesCollection)GetParentCollection(this, typeof(ClientInTemplateSelectionCriteriaOrgTypesCollection)); }
		}

		#endregion
	}
}
