using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TIP
{
	[XmlSerializerAssembly("ZClientTIP.XmlSerializers")]
	public class OrgPartRelationRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		#region Schema

		static class Schema
		{
			public const string OrgHeaderPK = "OrgHeaderPK";
			public const string OrgName = "OrgName";
			public const string RelationshipType = "RelationshipType";
		}

		#endregion

		public OrgPartRelationRegistryBusinessObject()
		{
		}

		public OrgPartRelationRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			RelationshipType = OrgPartRelation.RelationshipTypes.Owner;
		}

		#region OrgHeaderPK

		public ZGuid OrgHeaderPK
		{
			get { return orgHeaderPK; }
			set
			{
				SetNonPersistentPropertyValue(OrgHeaderPKInfo, ref orgHeaderPK, value);
				if (!IsValidationSuspended)
				{
					ValidateOrgHeaderPK();
				}
			}
		}

		public void ValidateOrgHeaderPK()
		{
			OrgHeaderPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrgHeaderPKInfo, "Organisation");
			if (!OrgHeaderPKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(OrgHeaderPKInfo, Organisations);
				if (!OrgHeaderPKInfo.HasErrors())
				{
					ValidatePairInList();
				}
			}
		}

		void ValidatePairInList()
		{
			OrgPartRelationRegistryBusinessObjectCollection parentCollection = ParentCollection;
			if (parentCollection != null && parentCollection.GetRelationshipCountInList(OrgHeaderPK, relationshipType) > 1)
			{
				OrgHeaderPKInfo.AddError("This organisation-product relationship pair already exists.");
			}
		}

		OrgPartRelationRegistryBusinessObjectCollection ParentCollection
		{
			get { return (OrgPartRelationRegistryBusinessObjectCollection)base.GetParentCollection(this, typeof(OrgPartRelationRegistryBusinessObjectCollection)); }
		}

		public ZPropertyInfo OrgHeaderPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrgHeaderPK); }
		}

		ZGuid orgHeaderPK;

		public OrgHeader Organisation
		{
			get { return CurrentFactory.Load<OrgHeader>(OrgHeaderPK); }
		}

		public ZString OrgName
		{
			get { return Organisation != null ? Organisation.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo OrgNameInfo
		{
			get { return GetZPropertyInfo(Schema.OrgName); }
		}

		#endregion

		#region RelationshipType

		[MaxLength(3)]
		public ZString RelationshipType
		{
			get { return relationshipType; }
			set
			{
				CheckMaximumLength(RelationshipTypeInfo, value);
				SetNonPersistentPropertyValue(RelationshipTypeInfo, ref relationshipType, value);
				if (!IsValidationSuspended)
				{
					ValidateRelationshipType();
				}
			}
		}

		public void ValidateRelationshipType()
		{
			RelationshipTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RelationshipTypeInfo);
			if (!RelationshipTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(RelationshipTypeInfo, RelationshipTypeList);
				ValidatePairInList();
			}
		}

		public ZPropertyInfo RelationshipTypeInfo
		{
			get { return GetZPropertyInfo(Schema.RelationshipType); }
		}

		ZString relationshipType;

		#endregion

		#region Lookups

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(CurrentFactory); }
		}

		public CodeDescriptionPairList RelationshipTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(OrgPartRelation.RelationshipTypes.Owner, "Owner");
				result.AddPair(OrgPartRelation.RelationshipTypes.Supplier, "Supplier");
				return result;
			}
		}

		#endregion

		#region Method overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgPartRelationRegistryBusinessObject(factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrgHeaderPK();
			ValidateRelationshipType();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OrgHeaderPK = new ZGuid(reader.ReadElementString(Schema.OrgHeaderPK));
			RelationshipType = reader.ReadElementString(Schema.RelationshipType);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OrgHeaderPK, OrgHeaderPK.ToString());
			writer.WriteElementString(Schema.RelationshipType, RelationshipType);
		}

		public override bool Equals(object obj)
		{
			var compareTo = obj as OrgPartRelationRegistryBusinessObject;
			return compareTo != null && ZGuid.Equals(compareTo.PK, PK);
		}

		public override int GetHashCode()
		{
			return PK.GetHashCode();
		}

		#endregion
	}
}
