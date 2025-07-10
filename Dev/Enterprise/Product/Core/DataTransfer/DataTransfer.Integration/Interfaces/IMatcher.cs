using CargoWise.Types;

namespace Enterprise.DataTransfer.Integration
{
	public interface IMatcher<ResultType> where ResultType : IZType
	{
		bool Match();
		ResultType Result { get; }
	}
}
