namespace Enterprise.Integration.Schedule
{
	public interface ITransportLegCollectionReader
	{
		void ReadIntoCollection();
	}

	public interface ITransportLegCollectionReaderForTransit : ITransportLegCollectionReader
	{
	}
}
