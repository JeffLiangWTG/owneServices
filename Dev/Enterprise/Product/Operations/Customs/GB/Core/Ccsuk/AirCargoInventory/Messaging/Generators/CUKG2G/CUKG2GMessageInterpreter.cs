using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	class CUKG2GMessageInterpreter
	{
		public CUKG2GMessageInterpreter(IG2gHeader iG2gHeader)
		{
			consolHeader = iG2gHeader;
		}

		internal ZString MakePretty()
		{
			var sb = new ZStringBuilder();
			sb.AppendLine(MessagePrettierCss.CSS);
			sb.AppendLine("<h2>Good 2 Go Message - " + consolHeader.MasterUcr + "</h2>");
			var table = new HtmlTableCreator(new string[] { "Field", "Value" });
			table.WriteRow("AWB", consolHeader.MawpAndMawn);
			table.WriteRow("Master CAR", consolHeader.CustomsAuthorisationReference);
			table.WriteRow("Master SOE", consolHeader.MasterSOE);
			table.WriteRow("Location", consolHeader.Airport + consolHeader.Shed);
			table.WriteRow("Agent badge", consolHeader.AgentBadge);
			table.WriteRow("Agent type", consolHeader.AgentType);
			sb.AppendLine(table.ToHtml());

			foreach (IG2gConsignment consignmentShipment in consolHeader.Consignments)
			{
				sb.AppendLine("<h3>" + consignmentShipment.CargoWiseNumber + "</h3>");
				table = new HtmlTableCreator(new string[] { "Field", "Value" });
				table.WriteRow("HAWB", consignmentShipment.HouseAWBNumber);
				table.WriteRow("Consignment CAR", consignmentShipment.CustomsAuthorisationReference);
				sb.AppendLine(table.ToHtml());

				foreach (IG2gDeclaration declaration in consignmentShipment.Declarations)
				{
					sb.AppendLine("<h4>" + declaration.CargoWiseNumber + "</h4>");
					table = new HtmlTableCreator(new string[] { "Field", "Value" });
					table.WriteRow("DUCR & Part", declaration.DeclarationUcr + " " + declaration.DeclarationUcrPart);
					table.WriteRow("Entry #", declaration.DeclarationEpu + declaration.DeclarationENo + " " + (declaration.DeclarationDoe.IsEmpty ? "" : declaration.DeclarationDoe.ToString("dd/MM/yyyy")));
					table.WriteRow("SOE", declaration.DeclarationSoe);
					table.WriteRow("Declaration CAR", declaration.CustomsAuthorisationReference);
					table.WriteRow("Location", declaration.AirportCode + declaration.ShedOpId);
					sb.AppendLine(table.ToHtml());
				}
			}

			return sb.ToString();
		}

		readonly IG2gHeader consolHeader;
	}
}
