using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Please do not add more DataContexts here, they all should be defined on per functional area e.g. Forwarding, Customs, eTail etc
	/// </summary>
	public static class DataContext
	{
		// Generic
		public const string UXML = "UXML";

		// Forwarding
		public const string HouseBill = Constants.HouseBillTemplateDataContext;
		public const string ShippingInstruction = "ShippingInstruction";
		public const string ShippingOrder = "ShippingOrder";
		public const string BookingRequest = "BookingRequest";
		public const string CargoDues = "CargoDues";
		public const string CargoDuesBrokerage = "CargoDuesBrokerage";

		// Customs
		public const string ATRCertificate = "ATRCertificate";
		public const string JobDeclaration = "JobDeclaration";
		public const string EURMEDCertificate = "EURMEDCertificate";
		public const string DV1Certificate = "DV1Certificate";
		public const string CMRWayBill = "CMRWayBill";
		public const string USATF6A = "USATF6A";

		//eCommerce
		public const string HVLVConsignment = "HVLVConsignment";
	}

	#endregion
}
