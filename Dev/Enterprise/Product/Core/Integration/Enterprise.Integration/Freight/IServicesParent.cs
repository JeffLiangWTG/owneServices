namespace Enterprise.Integration.Freight
{
	public interface IServicesParent
	{
		bool NeedsServiceEvents { get; }

		bool NeedsReferenceNumber { get; }
	}
}
