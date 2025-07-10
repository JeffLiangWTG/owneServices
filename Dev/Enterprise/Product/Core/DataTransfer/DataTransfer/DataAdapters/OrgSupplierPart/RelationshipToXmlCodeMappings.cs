using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class RelationshipToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		RelationshipToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OrgPartRelation.RelationshipTypes.Owner, nameof(Xsd.ClassificationRelatedOrgRelationship.OWN));
			yield return new Mapping(OrgPartRelation.RelationshipTypes.Supplier, nameof(Xsd.ClassificationRelatedOrgRelationship.SUP));
			yield return new Mapping(OrgPartRelation.RelationshipTypes.Both, nameof(Xsd.ClassificationRelatedOrgRelationship.BTH));
			yield return new Mapping(OrgPartRelation.RelationshipTypes.ClassificationOrganization, nameof(Xsd.ClassificationRelatedOrgRelationship.CLS));
		}

		public static readonly RelationshipToXmlCodeMappings Instance = new RelationshipToXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Relationship Type"; }
		}

		public new Xsd.ClassificationRelatedOrgRelationship GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ClassificationRelatedOrgRelationship.OWN, errorContext, notify);
		}
	}
}
