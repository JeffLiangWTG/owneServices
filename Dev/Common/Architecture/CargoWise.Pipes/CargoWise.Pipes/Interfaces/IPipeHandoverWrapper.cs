namespace CargoWise.Pipes
{
	public interface IPipeHandoverWrapper<TOutput> : IPipeHandoverWrapper, IPipeDataSource<TOutput>
	{
		void OnRelease(TOutput value);
		void OnClaim(TOutput value);
	}

	public interface IPipeHandoverWrapper : IPipeDataSource
	{
		void OnRelease(object value);
		void OnClaim(object value);
	}
}
