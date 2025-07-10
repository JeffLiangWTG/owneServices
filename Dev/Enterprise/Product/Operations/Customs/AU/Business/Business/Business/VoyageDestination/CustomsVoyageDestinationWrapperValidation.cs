using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsVoyageDestinationWrapperValidation : ZValidation
	{
		public CustomsVoyageDestinationWrapperValidation(CustomsVoyageDestinationWrapper parent)
			: base(parent)
		{
			this.parent = parent;
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateActualArrivalDateTimeUTC();
			ValidateEstimatedDateTimeOfArrivalUTC();
			ValidatePortOfArrival();
			ValidateDischargeCTOEstablishmentID();
		}

		#endregion

		#region ActualArrivalDateTime

		public void ValidateActualArrivalDateTimeUTC()
		{
			ValidateCalculatedProperty(parent.ActualArrivalDateTimeUTCInfo);
		}

		protected virtual void CheckActualArrivalDateTimeUTC()
		{
		}

		#endregion

		#region EstimatedDateTimeOfArrival

		public void ValidateEstimatedDateTimeOfArrivalUTC()
		{
			ValidateCalculatedProperty(parent.EstimatedDateTimeOfArrivalUTCInfo);
		}

		protected virtual void CheckEstimatedDateTimeOfArrivalUTC()
		{
			MessageValidation.CheckEntered(parent.EstimatedDateTimeOfArrivalUTCInfo);
		}

		#endregion

		#region PortOfArrival

		public void ValidatePortOfArrival()
		{
			ValidateCalculatedProperty(parent.PortOfArrivalInfo);
		}

		protected virtual void CheckPortOfArrival()
		{
			if (parent.Destination.JB_RL_NKPortOfDischargeInfo.HasNotifications())
			{
				parent.PortOfArrivalInfo.AddMessageError("A valid port of arrival is required.");
			}
			else
			{
				MessageValidation.CheckEntered(parent.PortOfArrivalInfo);
			}
		}

		#endregion

		#region DischargeCTOEstablishmentID

		public void ValidateDischargeCTOEstablishmentID()
		{
			ValidateCalculatedProperty(parent.DischargeCTOEstablishmentIDInfo);
		}

		protected virtual void CheckDischargeCTOEstablishmentID()
		{
			MessageValidation.CheckEntered(parent.DischargeCTOEstablishmentIDInfo);
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(CustomsVoyageDestinationWrapperValidation); }
		}

		MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(parent);
				}
				return fMessageValidation;
			}
		}
		MessageValidation fMessageValidation;

		readonly CustomsVoyageDestinationWrapper parent;

		#endregion
	}
}
