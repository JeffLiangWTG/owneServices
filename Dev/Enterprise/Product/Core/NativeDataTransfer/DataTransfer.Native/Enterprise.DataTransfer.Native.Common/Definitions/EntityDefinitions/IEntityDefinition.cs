using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	public interface IEntityDefinition : IGraphNode<IEntityDefinition>
	{
		string TableName { get; }
		string Suffix { get; }
		string EntityName { get; }
		string FullName { get; }
		bool HasCustomColumns { get; }
		bool IsUpdateOrInsert { get; }
		bool IsExternal { get; }
		bool RequiresAdditionOfActionEqualsMerge { get; }

		string DateRangeStartField { get; }
		string DateRangeEndField { get; }
		string OptionalEntityCondition { get; }
		string Behaviour { get; }

		EntitySetDefinition EntitySetDefinition { get; }
		PropertyDefinitionCollection PropertyDefinitions { get; }

		bool UniqueCriteriaExclusionsContains(string propertyName);

		// TODO: Should be internal and remove from interface
		#region Table and Column

		Table Table { get; }
		string TablePrefix { get; }
		IPropertyDef Id { get; }

		#region Association
		IEntityDefinition MainAssociationMate { get; }
		AssociationDefinition MainAssociation { get; }
		AssociationCollection AssociationCollection { get; }

		#endregion

		#endregion
	}
}
