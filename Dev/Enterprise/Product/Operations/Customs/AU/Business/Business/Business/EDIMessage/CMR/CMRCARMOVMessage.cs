using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCARMOVMessage : CMRCUSRESMessage
	{
		public CMRCARMOVMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.CARMOV;
		}

		protected override ZString AdditionalInfoForHeaderSectionOfReport()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("\tVessel Lloyds Number: " + CUSRES.TDT[0].TransportIdentification.TransportMeansIdentificationNameIdentifier + "\r\n");
			builder.Append("\tVoyage Number: " + CUSRES.TDT[0].ConveyanceReferenceNumber + "\r\n");
			builder.Append("\tCTO Establishment ID: " + CUSRES.LOC[0].LocationIdentification.LocationNameCode + "\r\n");

			ReferenceFunctionCodeQualifierList lineType = CUSRES.Group3[0].RFF[0].Reference.ReferenceFunctionCodeQualifier;
			bool isExemptionLine = lineType == ReferenceFunctionCodeQualifierList.TaxExemptionLicenceNumber.ToString();
			bool isCANLine = lineType == ReferenceFunctionCodeQualifierList.TransactionReferenceNumber.ToString();
			if (isExemptionLine)
			{
				builder.Append("\tExemption Code: " + CUSRES.Group3[0].RFF[0].Reference.ReferenceIdentifier + "\r\n");
			}
			else if (isCANLine)
			{
				builder.Append("\tCAN: " + CUSRES.Group3[0].RFF[0].Reference.ReferenceIdentifier + "\r\n");
			}

			lineType = CUSRES.Group6[0].RFF[0].Reference.ReferenceFunctionCodeQualifier;
			if (lineType == ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber.ToString())
			{
				builder.Append("\tContainer Number: " + CUSRES.Group6[0].RFF[0].Reference.ReferenceIdentifier + "\r\n");
			}
			else if (lineType == ReferenceFunctionCodeQualifierList.GeneralCargoConsignmentReferenceNumber.ToString())
			{
				builder.Append("\tNon Containerised ID: " + CUSRES.Group6[0].RFF[0].Reference.ReferenceIdentifier + "\r\n");
			}

			return builder.ToString();
		}
	}
}
