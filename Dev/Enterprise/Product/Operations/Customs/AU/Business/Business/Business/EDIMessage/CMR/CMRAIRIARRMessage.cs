using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAIRIARRMessage : CMRCUSRESMessage
	{
		public CMRAIRIARRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);

			result = new CusMAWBBase.Loader(Factory).FindFirstMatchingMAWB(reference);
			if (result == null)
			{
				result = base.GetWrappedObject();
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.AIRIAR;
		}

		#region Report

		protected override ZString AdditionalInfoForHeaderSectionOfReport()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(base.AdditionalInfoForHeaderSectionOfReport());

			if (Voyage != null)
			{
				builder.Append("\tFlight Number: " + Voyage.JV_VoyageFlight + "\r\n");
				builder.Append("\tFlight Date: " + Voyage.JV_FlightDate + "\r\n");

				foreach (VoyageOrigin origin in Voyage.Origins)
				{
					builder.Append("\tPort of Loading: " + origin.JA_RL_NKPortOfLoading + "\r\n");
				}
				foreach (VoyageDestination dest in Voyage.Destinations)
				{
					builder.Append("\tPort of Discharge: " + dest.JB_RL_NKPortOfDischarge + "\r\n");
				}
			}

			return builder.ToString();
		}

		#endregion

		#region Related Business Objects

		public JobVoyage Voyage
		{
			get { return EM_LinkedObject as JobVoyage; }
		}

		#endregion
	}
}
