namespace Enterprise.Client.WCB
{
	static class Constants
	{
		internal const int PartNumber = 12;
		internal const int Origin = 20;
		internal const int UnitPrice = 21;
		internal const int OrderNumber = 9;
		internal const int InvoiceNumber = 10;
		internal const int Quantity = 31;

		internal const int DaimlerInvoiceNumber = 0;
		internal const int DaimlerQty = 14;
		internal const int DaimlerUnitPrice = 22;

		internal const int RecylePeriod = 4;

		#region DaimlerChrysler

		internal static class DaimlerChrysler
		{
			internal static class Import
			{
				#region RecordType

				internal static class RecordType
				{
					public const int Length = 2;
				}

				#endregion
			}
		}

		#endregion
	}
}
