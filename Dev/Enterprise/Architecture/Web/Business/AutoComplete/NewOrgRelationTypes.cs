namespace Enterprise.ZArchitecture.Web.Business
{
	public enum NewOrgRelationTypes
	{
		Unknown,
		/// <summary>
		/// Considers new organization as buyer when creating buyer-supplier link.
		/// </summary>
		Buyer,
		/// <summary>
		/// Considers new organization as suppplier when creating buyer-supplier link.
		/// </summary>
		Supplier,
	}
}
