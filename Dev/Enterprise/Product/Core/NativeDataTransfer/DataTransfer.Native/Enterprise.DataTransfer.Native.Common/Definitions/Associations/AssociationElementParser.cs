using System.Xml.Linq;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
	static class Tag
	{
		// Association Definition
		public const string ReferEntity = "To";
		public const string OwnerEntity = "From";
		public const string Key = "Key";
		public const string RefKey = "RefKey";
		public const string Through = "Through";
		public const string ChildFk = "ChildFK";
		public const string Cardinality = "Cardinality";
		public const string WhereThisColumnIsNull = "WhereThisColumnIsNull"; // Means "exclude rows where the following column is blank"
		public const string AdditionalKey = "AdditionalKey";
		public const string AdditionalKeyRef = "AdditionalKeyRef";
		public const string LinkChild = "LinkChild";
		public const string IsExternalChild = "ExternalChild";
		public const string IsExternalParent = "ExternalParent";
	}

	class AssociationElementParser
	{
		public string BuildParentKey(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.Key);
		}

		public string BuildRefKey(XElement associationElement)
		{
			var refKey = (string)associationElement.Attribute(Tag.RefKey);
			return refKey ?? string.Empty;
		}

		public string BuildChildKey(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.ChildFk);
		}

		public string BuildOwnerEntity(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.OwnerEntity);
		}

		public string BuildReferEntity(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.ReferEntity);
		}

		public string BuildThroughTable(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.Through);
		}

		public string BuildWhereThisColumnIsNull(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.WhereThisColumnIsNull);
		}

		public string BuildAdditionalKey(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.AdditionalKey);
		}

		public string BuildAdditionalKeyRef(XElement associationElement)
		{
			return (string)associationElement.Attribute(Tag.AdditionalKeyRef);
		}

		public bool BuildLinkChild(XElement associationElement)
		{
			bool linkChild;
			return !bool.TryParse((string)associationElement.Attribute(Tag.LinkChild), out linkChild) || linkChild;
		}

		public string BuildCardinality(XElement associationElement)
		{
			var cardinality = (string)associationElement.Attribute(Tag.Cardinality);
			return cardinality ?? string.Empty;
		}

		public bool BuildIsExternal(XElement associationElement)
		{
			bool isMain;
			bool.TryParse((string)associationElement.Attribute(TagName.IsExternal), out isMain);
			return isMain;
		}

		public bool BuildIsExternalChild(XElement associationElement)
		{
			bool isMain;
			if (!bool.TryParse((string)associationElement.Attribute(Tag.IsExternalChild), out isMain))
			{
				isMain = false;
			}
			return isMain;
		}

		public bool BuildIsExternalParent(XElement associationElement)
		{
			if (!bool.TryParse((string)associationElement.Attribute(Tag.IsExternalParent), out var isExternalParent))
			{
				isExternalParent = false;
			}
			return isExternalParent;
		}
	}
}
