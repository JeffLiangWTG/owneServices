using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;

namespace Enterprise.DataTransfer.Native.Common.Definitions
{
	public class AssociationInfo
	{
		public string ChildName { get; set; }
		public List<AssociationKeyInfo> ParentKeys { get; set; }
		public string ChildKey { get; set; }
		public string ParentName { get; set; }
		public string ThroughTable { get; set; }
		public string Cardinality { get; set; }
		public string WhereThisColumnIsNull { get; set; }
		public string AdditionalKey { get; set; }
		public string AdditionalKeyRef { get; set; }
		public bool LinkChild { get; set; }
		public bool IsExternalChild { get; set; }
		public bool IsExternalParent { get; set; }

		public bool IsMainAssociationFor(EntityDefinition definition)
		{
			if (definition.MainAssociation == null)
			{
				return false;
			}

			if (!definition.MainAssociation.From.EntityName.Equals(this.ChildName))
			{
				return false;
			}

			if (!definition.MainAssociation.To.EntityName.Equals(this.ParentName))
			{
				return false;
			}

			return true;
		}
	}
}
