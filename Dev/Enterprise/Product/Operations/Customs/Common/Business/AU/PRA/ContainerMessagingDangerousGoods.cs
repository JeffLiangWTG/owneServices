using System.Text;
using CargoWise.Types;

namespace Enterprise.Customs.Common.AU
{
	public class ContainerMessagingDangerousGoods
	{
		public ZString ShipmentReference;
		public ZString IMDGClass; // Required
		public ZString IMDGCodePage; // Optional
		public ZString IMDGCodeVersion; // Optional
		public ZString UNDGNumber; // Required
		public ZString FlashpointTemperatureInCelcius; // Conditional
		public ZString PackingGroup; //(I, II, or III) Conditional
		public ZString TechnicalName; // (up to 5 lines of 70) Required
		public ZString ContactName; // Required
		public ZString ContactPhoneNumber; // Required
		public ZString ContactFaxNumber; // Optional
		public ZString ContactEmailAddress; // Optional
		public ZDecimal Weight; // Required

		public string GetErrors()
		{
			StringBuilder result = new StringBuilder();
			if (IMDGClass.IsEmpty)
			{
				result.Append(Res.GetString("7D160A6E-500D-43EA-B9D8-F12930750B1F", "    IMDG Class\r\n"));
			}

			if (UNDGNumber.IsEmpty)
			{
				result.Append(Res.GetString("3602276A-2C8A-4656-A3D6-C019EECD5895", "    UNDG Number\r\n"));
			}

			if (TechnicalName.IsEmpty)
			{
				result.Append(Res.GetString("7DA87E71-80F7-4789-9F9B-9CBE0DBAEE95", "    Hazard Technical Name\r\n"));
			}

			if (ContactName.IsEmpty)
			{
				result.Append(Res.GetString("DB9A7D3A-D395-4A07-85BB-61F186E22AB1", "    Emergency Contact Name\r\n"));
			}

			if (ContactPhoneNumber.IsEmpty)
			{
				result.Append(Res.GetString("06C16AFD-007F-4AE1-BEA8-4E9D7BD598A2", "    Emergency Contact Phone Number\r\n"));
			}

			return result.ToString();
		}
	}
}
