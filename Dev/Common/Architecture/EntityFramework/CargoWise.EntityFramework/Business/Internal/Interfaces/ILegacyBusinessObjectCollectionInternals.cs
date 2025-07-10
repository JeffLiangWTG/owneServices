namespace CargoWise.EntityFramework
{
	public interface ILegacyBusinessObjectCollectionInternals
	{
		void RemoveAllButLeaveRelationshipsIntact();
		void SetCollectionRelationships(BusinessObject child);
		BusinessObject CreateNewBusinessObject();
		void SetOverriddenAdditionalFilter(ZQuery filter);

		ZQuery AdditionalFilter { get; }
		ZQuery AdditionalRelationshipFilter { get; }
		ZQuery RelationshipFilter { get; }
		ZQuery CompleteFilter { get; }
	}
}
