namespace Enterprise.Integration
{
	public interface IUniversalCopiedIdentifier
	{
		bool CreatedByUniversalCopy { get; }

		void MarkAsCopiedByUniversalCopy();
	}
}
