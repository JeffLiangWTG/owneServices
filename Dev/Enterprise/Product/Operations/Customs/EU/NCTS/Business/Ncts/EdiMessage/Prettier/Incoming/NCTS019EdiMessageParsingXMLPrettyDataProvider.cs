using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using static System.FormattableString;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTS019EdiMessageParsingXMLPrettyDataProvider : NCTS182EdiMessageParsingXMLPrettyDataProvider
	{
		public NCTS019EdiMessageParsingXMLPrettyDataProvider(EDIMessage message) : base(message)
		{
		}

		public DateTime? DiscrepanciesNotificationDate => ZDateTime.TryParseExact(SelectSingleNodeInnerText(TransitOperationDiscrepanciesNotificationDateXPath), out var endorsementDate, "yyyy-MM-dd") ? endorsementDate.ToDateTime() : null;

		public ZString DiscrepanciesNotificationText => SelectSingleNodeInnerText(TransitOperationDiscrepanciesNotificationTextXPath);

		public ZString HolderOfTheTransitProcedure => SelectSingleNodeInnerText(HolderOfTheTransitProcedureXPath);

		public ZString GuarantorIdentificationNumber => SelectSingleNodeInnerText(GuarantorIdentificationNumberXPath);

		public ZString GuarantorName => SelectSingleNodeInnerText(GuarantorNameXPath);

		public INCTSAddressData GuarantorAddress => SelectNodes(GuarantorAddressXPath).Select(node => new NCTSPrettierIncidentLocationAddressData(
				GetNodeInnerText(node, AddressCountryNodeXPath),
				GetNodeInnerText(node, AddressPostCodeNodeXPath),
				GetNodeInnerText(node, AddressStreetAndNumberNodeXPath))).FirstOrDefault();

		protected virtual string TransitOperationDiscrepanciesNotificationDateXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='discrepanciesNotificationDate']");
		protected virtual string TransitOperationDiscrepanciesNotificationTextXPath => Invariant($"//*[local-name()='TransitOperation']/*[local-name()='discrepanciesNotificationText']");
		protected virtual string HolderOfTheTransitProcedureXPath => Invariant($"//*[local-name()='HolderOfTheTransitProcedure']/*[local-name()='identificationNumber']");
		protected virtual string GuarantorIdentificationNumberXPath => Invariant($"//*[local-name()='Guarantor']/*[local-name()='identificationNumber']");
		protected virtual string GuarantorNameXPath => Invariant($"//*[local-name()='Guarantor']/*[local-name()='name']");
		protected virtual string GuarantorAddressXPath => Invariant($"//*[local-name()='Guarantor']/*[local-name()='Address']");
		protected virtual string AddressStreetAndNumberNodeXPath => Invariant($"*[local-name()='streetAndNumber']");
		protected virtual string AddressPostCodeNodeXPath => Invariant($"*[local-name()='postcode']");
		protected virtual string AddressCountryNodeXPath => Invariant($"*[local-name()='country']");
	}
}
