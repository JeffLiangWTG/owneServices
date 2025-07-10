using System;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE
{
	public class UPENumberFountains
	{
		protected UPENumberFountains()
		{
		}

		public static UPENumberFountains Instance
		{
			get { return instance ?? (instance = new UPENumberFountains()); }
		}
		[ThreadStatic]
		static UPENumberFountains instance;

		public INumberFountainProxy CalloutJobHeaderNumberFountain
		{
			get
			{
				if (fCalloutJobHeaderNumberFountain == null)
				{
					NonFormattedNumberFountainFactory factory = new NonFormattedNumberFountainFactory("UPECalloutJobHeaderNumberFountain");
					fCalloutJobHeaderNumberFountain = factory.New();
				}
				return fCalloutJobHeaderNumberFountain;
			}
		}
		INumberFountainProxy fCalloutJobHeaderNumberFountain;

		public INumberFountainProxy PartPaymentControlNumberFountain
		{
			get
			{
				if (fPartPaymentControlNumberFountain == null)
				{
					NonFormattedNumberFountainFactory factory = new NonFormattedNumberFountainFactory("UPEPartPaymentControlNumberFountain");
					fPartPaymentControlNumberFountain = factory.New();
				}
				return fPartPaymentControlNumberFountain;
			}
		}
		INumberFountainProxy fPartPaymentControlNumberFountain;

		public INumberFountainProxy RefundControlNumberFountain
		{
			get
			{
				if (fRefundControlNumberFountain == null)
				{
					NonFormattedNumberFountainFactory factory = new NonFormattedNumberFountainFactory("UPERefundControlNumberFountain");
					fRefundControlNumberFountain = factory.New();
				}
				return fRefundControlNumberFountain;
			}
		}
		INumberFountainProxy fRefundControlNumberFountain;

		public INumberFountainProxy BISIEntryPrintBatchNumber
		{
			get
			{
				if (fBISIEntryPrintBatchNumber == null)
				{
					NonFormattedNumberFountainFactory factory = new NonFormattedNumberFountainFactory("BISIEntryPrintBatchNumber");
					fBISIEntryPrintBatchNumber = factory.New();
				}
				return fBISIEntryPrintBatchNumber;
			}
		}
		INumberFountainProxy fBISIEntryPrintBatchNumber;
	}
}
