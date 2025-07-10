namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IDutyAndTaxCollection
			{
				IDutyAndTax AddNew();

				IDutyAndTax FirstOrDefault();
			}
		}
	}
}
