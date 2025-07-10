using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ComponentRelationshipLinkLookups : BMComponentLinkLookups
	{
		public ComponentRelationshipLinkLookups(AutoBMComponentLink parent)
			: base(parent)
		{
		}

		#region ComponentFroms

		protected override BMComponentCollection ComponentFromsCore()
		{
			return new BMComponentCollection(Factory, new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.ComponentRelationship), excludeRelationships: false);
		}

		#endregion

		#region ComponentTos

		protected override BMComponentCollection ComponentTosCore()
		{
			ZQuery query = new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer);
			query.AddToFilter(JoinCondition.Or, BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Bucket);
			query.AddToFilter(BMComponentSchema.FC_FC_ParentComponent, null);

			var components = new BMComponentCollection(Factory, query);
			var parent = Parent as ComponentRelationshipLink;

			if (parent != null && parent.ComponentFrom != null)
			{
				var componentRelationship = (ComponentRelationship)parent.ComponentFrom;

				var otherTypesInRelationship = componentRelationship.RelatedComponentLinks
										.Where(link => link != parent && link.ComponentTo != null)
										.Select(link => link.ComponentTo.FC_Type)
										.Distinct().ToArray();

				if (otherTypesInRelationship.Length == 1)
				{
					components.AddComponentLookupFilterBusinessObjectDefaults(otherTypesInRelationship.Single());
				}
			}

			return components;
		}

		#endregion

	}
}
