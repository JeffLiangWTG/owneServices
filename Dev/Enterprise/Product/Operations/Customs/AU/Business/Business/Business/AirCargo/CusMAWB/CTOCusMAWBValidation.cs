using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusMAWBValidation : CusMAWBValidation
	{
		public CTOCusMAWBValidation(CTOCusMAWB parent)
			: base(parent)
		{
		}

		public new CTOCusMAWB Parent => (CTOCusMAWB)base.Parent;

		#region CM_RL_NKDischargePort

		protected override void CheckCM_RL_NKDischargePort()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(MAWB.CM_RL_NKDischargePortInfo);
			if (MAWB.DischargePort == null)
			{
				MAWB.CM_RL_NKDischargePortInfo.AddMessageError("Valid discharge port is required.");
			}
			else
			{
				ZString portWarning = MessageValidation.ValidatePortType(MAWB.CM_RL_NKDischargePort, true, false);
				if (!portWarning.IsEmpty)
				{
					MAWB.CM_RL_NKDischargePortInfo.AddWarning(portWarning);
				}
			}

			ValidateCM_RL_NKFirstArrivalPort();
		}

		#endregion

		#region CM_FlightNo

		protected override void CheckCM_FlightNo()
		{
			base.CheckCM_FlightNo();
			CheckPostedInvoiceChanges(Parent.CM_FlightNoInfo);
		}

		#endregion

		#region CM_RL_NKFirstArrivalPort

		protected override void CheckCM_RL_NKFirstArrivalPort()
		{
			base.CheckCM_RL_NKFirstArrivalPort();
			CheckPostedInvoiceChanges(Parent.CM_RL_NKFirstArrivalPortInfo);
		}

		#endregion

		#region CM_ArrivalDate

		protected override void CheckCM_ArrivalDate()
		{
			base.CheckCM_ArrivalDate();
			CheckPostedInvoiceChanges(Parent.CM_ArrivalDateInfo);
		}

		#endregion

		#region Posted Invoice Validation

		void CheckPostedInvoiceChanges(ZPropertyInfo info)
		{
			if (PostedInvoiceExists && info.HasChanges)
			{
				string originalValue = info.PropertyType == typeof(ZDateTime) ? ((ZDateTime)info.OriginalValue).ToShortDateString() : info.OriginalValue.ToString();
				info.AddError(string.Format("Posted invoices already exist. You cannot modify the {0} - please change it back to the original value of {1}.", info.Description, originalValue));
			}
		}

		bool PostedInvoiceExists
		{
			get
			{
				string oldJobNumberValue = Parent.GetFormattedJobNumber((ZString)Parent.CM_FlightNoInfo.OriginalValue,
					(ZString)Parent.CM_RL_NKFirstArrivalPortInfo.OriginalValue, (ZDateTime)Parent.CM_ArrivalDateInfo.OriginalValue);
				var filter = new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, oldJobNumberValue);

				return Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(ObjectFactory.GetType<Integration.Accounting.ITransactionHeader>()), filter);
			}
		}

		#endregion

		#region Implementation

		public new CTOCusMAWB MAWB
		{
			get { return Parent; }
		}

		#endregion
	}
}
