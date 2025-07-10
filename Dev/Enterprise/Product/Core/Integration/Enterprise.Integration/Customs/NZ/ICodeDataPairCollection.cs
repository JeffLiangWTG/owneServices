using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICodeDataPairCollection
			{
				ICodeDataPair this[int index] { get; }

				ICodeDataPair AddNew();

				void AddOrUpdateExisting(ZString code, ZString data);
			}
		}
	}
}
