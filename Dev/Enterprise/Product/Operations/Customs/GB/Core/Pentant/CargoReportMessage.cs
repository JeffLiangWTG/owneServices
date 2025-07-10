using CargoWise.Types;

namespace Enterprise.Customs.GB.Pentant
{
	public class CargoReportMessage
	{
		public const string BeginMessage = "BEGINMESSAGE";
		public string MsgRef { get; set; }//an..25
		public string Sender { get; set; } //Advised by Pentant
		public string Recipient { get; set; } //Advised by Pentant

		public string MsgHeader = "SHORTSEA";
		public string MsgFunction { get; set; } // A..1
		public string ImportExport { get; set; } // A..1
		public string Date { get; set; }// n..25
		public string Time { get; set; }// n..25
		public string Port { get; set; } // a..3
		public string Shed { get; set; } // an..3
		public string ACARef { get; set; } // 	3..A-an..6
		public string VehicleNoPlate { get; set; } // an..17
		public string TrailerNo { get; set; } // an..17
		public string NoPackages { get; set; } // n..9
		public string Weight { get; set; } //  n..12.2
		public string Marks { get; set; } // an..35
		public string TypePackages { get; set; } //  a..7
		public string Hazardous { get; set; } // a..1
		public string GoodsDescription { get; set; } //  an..70
		public string ModeOfTransport { get; set; } //  an..3
		public string PlaceArrivalExport { get; set; } //  an..3
		public string TransitShed { get; set; }// a..3
		public string OrginalFinalPOD { get; set; } //  a..2
		public string AgentsOwnRef { get; set; } // an..25
		public string EndMSG => "ENDMESSAGE";

		internal string Serialise()
		{
			var allFields = new ZString[] { BeginMessage, MsgRef, Sender, Recipient, MsgHeader, MsgFunction, ImportExport, Date, Time, Port, Shed, ACARef, VehicleNoPlate, TrailerNo, NoPackages, Weight, Marks, TypePackages,
													Hazardous, GoodsDescription, ModeOfTransport, PlaceArrivalExport, TransitShed, OrginalFinalPOD, AgentsOwnRef, EndMSG };

			for (int i = 0; i < allFields.Length; i++)
			{
				allFields[i] = allFields[i].Replace(PentantConstants.SeparatorChar.ToString(), "").Trim();
			}

			return string.Join(PentantConstants.SeparatorChar.ToString(), allFields);
		}
	}
}
