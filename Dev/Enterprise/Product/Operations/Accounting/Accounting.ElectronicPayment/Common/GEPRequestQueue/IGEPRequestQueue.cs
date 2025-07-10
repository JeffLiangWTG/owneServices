namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal interface IGEPRequestQueue
	{
		string MessageTypeDescription { get; }
		IGEPRequestMessage GetTopOneQueuedRequest();
	}
}
