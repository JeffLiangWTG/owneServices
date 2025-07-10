using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAIRAARRMessage : CMRCUSRESMessage
	{
		public CMRAIRAARRMessage(BusinessObjectFactory factory, DataRow row)
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
			EM_MessageType = CMRMessageTypes.AIRAAR;
		}

		#region Report

		protected override ZString AdditionalInfoForHeaderSectionOfReport()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(base.AdditionalInfoForHeaderSectionOfReport());

			if (VoyageDest != null)
			{
				builder.Append("\tFlight Number: " + VoyageDest.Voyage.JV_VoyageFlight + "\r\n");
				builder.Append("\tFlight Date: " + VoyageDest.Voyage.JV_FlightDate + "\r\n");
				builder.Append("\tPort of Discharge: " + VoyageDest.JB_RL_NKPortOfDischarge + "\r\n");
			}

			return builder.ToString();
		}

		#endregion

		#region Related Business Objects

		public VoyageDestination VoyageDest
		{
			get { return EM_LinkedObject as VoyageDestination; }
		}

		#endregion
	}
}
