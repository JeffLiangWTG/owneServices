using CargoWise.Schema;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveSystemSetup
	{
		/// <summary>
		/// Add a relationship between two types of records to enable Archive Manager to load them as a record set to archive together.
		/// </summary>
		/// <param name="parentPKColumn">Parent</param>
		/// <param name="childFKColumn">Child</param>
		void AddRelationship(SchemaColumn parentPKColumn, SchemaColumn childFKColumn);

		/// <summary>
		/// Add a relationship between two types of records to enable Archive Manager to load them as a record set to archive together.
		/// </summary>
		/// <param name="parentPKColumn">Parent</param>
		/// <param name="childFKColumn">Child</param>
		/// <param name="isReversed">Is Reversed if the child table in the look up relationship is infact a parent in the business layer. e.g. We start looking from a Shipment PK to find the JobConShipLink to then find the JobConsol. The relationship from dbo.JobConShipLink to JobConsol would then be considered Reversed.</param>
		void AddRelationship(SchemaColumn parentPKColumn, SchemaColumn childFKColumn, bool isReversed);

		/// <summary>
		/// Add a relationship between two types of records to enable Archive Manager to load them as a record set to archive together.
		/// </summary>
		/// <param name="parentNameOverride">Override the parent name if you have multiple logical relationships on the same table.</param>
		/// <param name="parentPKColumn">Parent</param>
		/// <param name="childNameOverride">Override the child name if you have multiple logical relationships on the same table.</param>
		/// <param name="childFKColumn">Child</param>
		void AddRelationship(string parentNameOverride, SchemaColumn parentPKColumn, string childNameOverride, SchemaColumn childFKColumn);

		/// <summary>
		/// Add a relationship between two types of records to enable Archive Manager to load them as a record set to archive together.
		/// </summary>
		/// <param name="parentNameOverride">Override the parent name if you have multiple logical relationships on the same table.</param>
		/// <param name="parentPKColumn">Parent</param>
		/// <param name="childNameOverride">Override the child name if you have multiple logical relationships on the same table.</param>
		/// <param name="childFKColumn">Child</param>
		/// <param name="isReversed">Is Reversed if the child table in the look up relationship is infact a parent in the business layer. e.g. We start looking from a Shipment PK to find the JobConShipLink to then find the JobConsol. The relationship from dbo.JobConShipLink to JobConsol would then be considered Reversed.</param>
		void AddRelationship(string parentNameOverride, SchemaColumn parentPKColumn, string childNameOverride, SchemaColumn childFKColumn, bool isReversed);

		/// <summary>
		/// Use this to add tables that uses ParentID relationships, such as StmALog, as child to all tables already added to the system.
		/// </summary>
		void AddRelationshipToAllPKs(params SchemaColumn[] childFKColumns);
	}
}
