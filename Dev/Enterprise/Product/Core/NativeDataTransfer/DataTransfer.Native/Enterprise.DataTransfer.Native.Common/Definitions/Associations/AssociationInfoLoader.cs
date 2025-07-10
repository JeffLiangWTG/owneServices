using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	class AssociationInfoLoader
	{
		internal AssociationInfoLoader(XElement element)
		{
			this.element = element;
			parser = new AssociationElementParser();
		}
		readonly XElement element;
		readonly AssociationElementParser parser;

		internal IEnumerable<AssociationInfo> GetAssociationInfos()
		{
			var associations = new HashSet<AssociationInfo>();
			var associationsElement = element.Element(TagName.Associations);

			if (associationsElement == null)
			{
				return associations;
			}

			var associationElements = associationsElement.Elements(TagName.Association);
			foreach (var associationElement in associationElements)
			{
				var associationKeys = new List<AssociationKeyInfo>();
				var association = GetAssociationInfo(associationElement);
				var associationKey = GetAssociationKeyInfo(associationElement);
				if (associationKey == null)
				{
					var associationKeyElements = associationElement.Elements(TagName.AssociationKey);
					foreach (var associationKeyElement in associationKeyElements)
					{
						associationKey = GetAssociationKeyInfo(associationKeyElement);
						if (associationKey != null)
						{
							associationKeys.Add(associationKey);
						}
					}
				}
				else
				{
					associationKeys.Add(associationKey);
				}
				if (associationKeys.Count == 0)
				{
					throw new NativeXMLUserVisibleException(string.Format(CultureInfo.InvariantCulture, "Could not found any Key related with {0} Association", association.ChildName));
				}

				association.ParentKeys = associationKeys;
				associations.Add(association);
			}
			return associations;
		}

		AssociationInfo GetAssociationInfo(XElement associationElement)
		{
			var association = new AssociationInfo();
			association.ChildName = parser.BuildOwnerEntity(associationElement);
			association.ChildKey = parser.BuildChildKey(associationElement);
			association.ParentName = parser.BuildReferEntity(associationElement);
			association.ThroughTable = parser.BuildThroughTable(associationElement);
			association.Cardinality = parser.BuildCardinality(associationElement);
			association.WhereThisColumnIsNull = parser.BuildWhereThisColumnIsNull(associationElement);
			association.AdditionalKey = parser.BuildAdditionalKey(associationElement);
			association.AdditionalKeyRef = parser.BuildAdditionalKeyRef(associationElement);
			association.LinkChild = parser.BuildLinkChild(associationElement);
			association.IsExternalChild = parser.BuildIsExternalChild(associationElement);
			association.IsExternalParent = parser.BuildIsExternalParent(associationElement);
			return association;
		}

		AssociationKeyInfo GetAssociationKeyInfo(XElement associationElement)
		{
			var associationKey = new AssociationKeyInfo();
			associationKey.ParentKey = parser.BuildParentKey(associationElement);
			associationKey.RefKey = parser.BuildRefKey(associationElement);
			return string.IsNullOrEmpty(associationKey.ParentKey) ? null : associationKey;
		}
	}
}
