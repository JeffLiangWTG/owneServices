using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutNett7DayPayment : CalloutPaymentDetails
	{
		public CalloutNett7DayPayment(Callout callout)
			: base(callout)
		{
		}

		[BusinessObjectTestExclude]
		public ZBool IsResidential
		{
			get { return Nett7DayType == Residential; }
			set
			{
				Nett7DayType = Residential;
				IsResidentialInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsResidentialInfo
		{
			get { return GetZPropertyInfo(nameof(IsResidential)); }
		}

		[BusinessObjectTestExclude]
		public ZBool IsBusiness
		{
			get { return Nett7DayType == Business; }
			set
			{
				Nett7DayType = Business;
				IsBusinessInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsBusinessInfo
		{
			get { return GetZPropertyInfo(nameof(IsBusiness)); }
		}

		protected override string PaymentMethodAsText
		{
			get { return "Nett 7 Day"; }
		}

		protected override string Reference
		{
			get { return "Nett 7 Day Type: " + Nett7DayType; }
		}

		ZString Nett7DayType
		{
			get { return fNett7DayType; }
			set { fNett7DayType = value; }
		}

		ZString fNett7DayType;
		const string Residential = "Residential";
		const string Business = "Business";
	}
}
