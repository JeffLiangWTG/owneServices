using CargoWise.Integration;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryHeaderTypeListProvider
		{
			ICodeDescriptionPairList GetEntryTypes();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface ICACusEntryHeaderTypeListProvider : ICusEntryHeaderTypeListProvider
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
		public interface IUSCusEntryHeaderTypeListProvider : ICusEntryHeaderTypeListProvider
		{
		}
	}
}
