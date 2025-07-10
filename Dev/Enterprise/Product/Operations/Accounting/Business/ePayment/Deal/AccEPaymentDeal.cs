using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccEPaymentDealLookups;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.Business
{
	[UniversalDataContext(DataContextType.AccEPaymentDeal)]
	public class AccEPaymentDeal : AutoAccEPaymentDeal, IEPaymentDeliveryContextValueProvider, IEPaymentLogParent
	{
		public AccEPaymentDeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.StatusCodeList")]
		public override ZString AED_Status { get => base.AED_Status; set => base.AED_Status = value; }

		[List("Lookups.ProviderCodeList")]
		public override ZString AED_ProviderCode { get => base.AED_ProviderCode; set => base.AED_ProviderCode = value; }

		[RelatedBusinessObject(nameof(Quote))]
		[List("Lookups.Quotes")]
		public override ZGuid AED_QU_Quote { get => base.AED_QU_Quote; set => base.AED_QU_Quote = value; }

		public virtual AccEPaymentQuote Quote
		{
			get { return Factory.Load<AccEPaymentQuote>(AED_QU_Quote); }
		}

		public GlbStaff CreatingUser => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, AED_SystemCreateUser);

		public bool HasProviderSentAResponse => new ZString[] { StatusCodes.Accepted, StatusCodes.InProgress, StatusCodes.Paid, StatusCodes.Failed, StatusCodes.Declined }.Contains(AED_Status);

		public bool HasProviderSentAReference => new ZString[] { StatusCodes.Accepted, StatusCodes.InProgress, StatusCodes.Paid, StatusCodes.Failed }.Contains(AED_Status);

		#endregion

		#region IEPaymentDeliveryContextValueProvider Memebers

		ZString IEPaymentDeliveryContextValueProvider.Purpose => $"E-Payment Deal {AED_InternalReference} submitted for processing to {AED_ProviderCode}";
		EntityInfo IEPaymentDeliveryContextValueProvider.EntityInfo => EntityInfo.New(this);

		#endregion

		#region IEPaymentLogParent Memebers

		PaymentApprovalBase IEPaymentLogParent.PaymentApprovalForLogging => (Quote as IEPaymentLogParent)?.PaymentApprovalForLogging;

		#endregion

		public void CancelEPayment()
		{
			AED_LastResponseReceivedUtc = ZDateTime.Empty;
			AED_ProviderReference = "";
			AED_Status = EPaymentStatusCodes.Deal.Cancelled;

			AED_LastResponseReceivedUtcInfo.RefreshBinding();
			AED_ProviderReferenceInfo.RefreshBinding();
			AED_StatusInfo.RefreshBinding();
		}

		public string ConvertOFXDealStatusToCW1(string ofxStatus)
		{
			switch (ofxStatus)
			{
				case OFXDealStatus.Booked:
					return StatusCodes.Accepted;
				case OFXDealStatus.ReceivedNotCleared:
				case OFXDealStatus.Received:
				case OFXDealStatus.ReadyForPayment:
					return StatusCodes.InProgress;
				case OFXDealStatus.Paid:
					return StatusCodes.Paid;
				case OFXDealStatus.Error:
					if (AED_Status == StatusCodes.Accepted)
					{
						return StatusCodes.Failed;
					}
					else if (AED_Status == StatusCodes.Requested)
					{
						return StatusCodes.Declined;
					}
					else
					{
						throw new InvalidOperationException(string.Format("Cannot convert ofx error status to CW1 when the previous status is {0}", AED_Status));
					}
				default:
					throw new InvalidOperationException(string.Format("Cannot convert ofx status '{0}' to CW1", ofxStatus));
			}
		}

		public ZString FindActiveDealAleadyInDatabase()
		{
			var result = ZString.Empty;
			if (StatusCodes.ActiveStatusCodes.Contains(AED_Status.ToString()))
			{
				var matchingDealQuery = new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, AED_QU_Quote);
				matchingDealQuery.AddToFilter(AccEPaymentDealSchema.AED_GC_Company, AED_GC_Company);
				matchingDealQuery.AddToFilter(AccEPaymentDealSchema.PK, SQLComparisonOperator.NotEqual, PK);

				var dealsAlreadyInDatabase = new BusinessObjectFactory().Load<AccEPaymentDeal>(matchingDealQuery);
				var validDeal = dealsAlreadyInDatabase.FirstOrDefault(x => StatusCodes.ActiveStatusCodes.Contains(x.AED_Status.ToString()));
				result = validDeal?.AED_Status ?? ZString.Empty;
			}
			return result;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AED_InternalReference = PK.ToString().Substring(0, 8);
			Quote.QU_InternalReference = Quote.PK.ToString().Substring(0, 8);
			AED_GC_Company = GlbCompany.CurrentCompany.PK;
			AED_ProviderCode = EPaymentProviderCodes.Codes.OFX;
		}
#endif
	}
}
