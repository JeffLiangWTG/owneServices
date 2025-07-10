using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsJobVoyageWrapperValidation : ZValidation
	{
		public CustomsJobVoyageWrapperValidation(CustomsJobVoyageWrapper parent)
			: base(parent)
		{
			this.parent = parent;
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateLastOverseasPortOfDeparture();
			ValidatePortOfFirstArrival();
			ValidateDateTimeOfDepartureUTC();
			ValidateFlightNo();
		}

		#endregion

		#region LastOverseasPortOfDeparture

		public void ValidateLastOverseasPortOfDeparture()
		{
			ValidateCalculatedProperty(parent.LastOverseasPortOfDepartureInfo);
		}

		protected virtual void CheckLastOverseasPortOfDeparture()
		{
			MessageValidation.CheckEntered(parent.LastOverseasPortOfDepartureInfo, "A last overseas port is required. Please go back and ensure that you have at least one overseas load port.");
		}

		#endregion

		#region PortOfFirstArrival

		public void ValidatePortOfFirstArrival()
		{
			ValidateCalculatedProperty(parent.PortOfFirstArrivalInfo);
		}

		protected virtual void CheckPortOfFirstArrival()
		{
			MessageValidation.CheckEntered(parent.PortOfFirstArrivalInfo, "A first local port is required. Please go back and ensure that you have at least one local discharge port.");
		}

		#endregion

		#region DateTimeOfDeparture

		public void ValidateDateTimeOfDepartureUTC()
		{
			ValidateCalculatedProperty(parent.DateTimeOfDepartureUTCInfo);
		}

		protected virtual void CheckDateTimeOfDepartureUTC()
		{
			MessageValidation.CheckEntered(parent.DateTimeOfDepartureUTCInfo, "An actual time of departure is required for the last overseas port.");
		}

		#endregion

		#region FlightNo

		public void ValidateFlightNo()
		{
			ValidateCalculatedProperty(parent.FlightNoInfo);
		}

		protected virtual void CheckFlightNo()
		{
			if (parent.Voyage.JV_VoyageFlightInfo.HasNotifications())
			{
				parent.FlightNoInfo.AddMessageError("A valid flight number is required.");
			}
			else
			{
				MessageValidation.CheckEntered(parent.FlightNoInfo, "A flight number is required.");
			}
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(CustomsJobVoyageWrapperValidation); }
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

		readonly CustomsJobVoyageWrapper parent;

		#endregion
	}
}
