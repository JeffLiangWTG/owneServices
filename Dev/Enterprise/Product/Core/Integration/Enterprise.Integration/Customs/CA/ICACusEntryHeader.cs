using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICACusEntryHeader
			{
				BusinessObjectCollection AllEntryLines { get; }
			}
		}
	}
}
