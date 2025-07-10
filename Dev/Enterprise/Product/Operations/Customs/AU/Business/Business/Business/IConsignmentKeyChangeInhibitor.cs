namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Every business object that uses ConsignmentKeyInhibitor should implement this one
	/// </summary>
	public interface IConsignmentKeyChangeInhibitor
	{
		bool ShouldStopKeyFieldsChange { get; }
	}
}
