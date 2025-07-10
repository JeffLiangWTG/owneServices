using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobDocsAndCartage : Freight.Forwarding.Business.ForwardingDocsAndCartage
	{
		public WoolworthsJobDocsAndCartage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override JobDocsAndCartageValidation GetNewValidation()
		{
			return new WoolworthsJobDocsAndCartageValidation(this);
		}

		[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
		public override ZDateTime JP_DeliveryCartageAdvised
		{
			get { return base.JP_DeliveryCartageAdvised; }
			set
			{
				if (base.JP_DeliveryCartageAdvised != value)
				{
					SetPropertyValue(JP_DeliveryCartageAdvisedInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJP_DeliveryCartageAdvised();
					}

					if (Parent is IStmALogParent parent)
					{
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.DeliveryCartageAdvised, EstimateActual.Actual, value.ToOffset());
						parent.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.BookingRequested, EstimateActual.Actual, value.ToOffset(), string.Empty,
							new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.DeliveryTransport));
					}
				}
			}
		}
	}
}
