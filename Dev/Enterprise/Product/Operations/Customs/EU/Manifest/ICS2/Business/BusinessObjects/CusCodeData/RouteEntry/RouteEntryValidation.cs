using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RouteEntryValidation : Customs.Business.CusCodeDataValidation
	{
		public RouteEntryValidation(RouteEntry parent) : base(parent)
		{
		}

		new RouteEntry Parent => (RouteEntry)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckRoutingCountries();
		}

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_DataInfo);
		}

		void CheckRoutingCountries()
		{
			var header = (AsycudaManifestHeader)Parent.Parent;

			if (header != null)
			{
				var specificCircumstanceIndicator = header.SpecificCircumstanceIndicator;

				var validSpecificCircumstances = new ZString[] {
					EUICS2SpecificCircumstanceList.Codes.F40,
					EUICS2SpecificCircumstanceList.Codes.F50 };

				if (specificCircumstanceIndicator.In(validSpecificCircumstances))
				{
					ValidateFirstRoutingCountry(header);
					ValidateLastRoutingCountry(header);
					ValidateFirstEuCountry(header);
				}
			}
		}

		void ValidateFirstRoutingCountry(AsycudaManifestHeader header)
		{
			var message = Res.GetString("7F9BB847-0F1B-4DD5-B93C-C0383405EB2B", "The first Country of Routing does not match the Port of Loading Country.");
			Parent.RemoveRowMessageError(message);

			var sequenceNumber = Parent.CY_Order;
			var countryCode = Parent.CY_Data;

			var loadPortCountryCode = header.PortOfLoading?.RL_RN_NKCountryCode;

			if (sequenceNumber == 1 && loadPortCountryCode.HasValue && countryCode != loadPortCountryCode.Value)
			{
				Parent.AddRowMessageError(message);
			}
		}

		void ValidateLastRoutingCountry(AsycudaManifestHeader header)
		{
			var message = Res.GetString("6CD562CB-5E2A-4BC6-9072-FB07B5172AC0", "The last Country of Routing does not match the Discharge Port Country.");
			Parent.RemoveRowMessageError(message);

			var routeEntryCollection = header.Itinerary.Cast<RouteEntry>();
			var sequenceNumber = Parent.CY_Order;
			var countryCode = Parent.CY_Data;

			var maxSequenceNumber = routeEntryCollection.Max(r => r.CY_Order);
			var dischargePortCountryCode = header.PortOfDischarge?.RL_RN_NKCountryCode;

			if (sequenceNumber == maxSequenceNumber && dischargePortCountryCode.HasValue && countryCode != dischargePortCountryCode.Value)
			{
				Parent.AddRowMessageError(message);
			}
		}

		void ValidateFirstEuCountry(AsycudaManifestHeader header)
		{
			var message = Res.GetString("E71ECFDF-6622-43CB-A509-4C8EEECC8398", "The first EU country of the routing does not match the Customs Office of First Entry.");
			Parent.RemoveRowMessageError(message);

			var routeEntryCollection = header.Itinerary.Cast<RouteEntry>();
			var sequenceNumber = Parent.CY_Order;

			var lowestOrderEuCountry = routeEntryCollection
				.Where(r => EuropeanUnionCountries.Contains(r.CY_Data))
				.OrderBy(r => r.CY_Order)
				.FirstOrDefault();

			var customsOfficeCountryCode = header.AMA_CustomsOffice.SubstringSafe(0, 2);

			if (lowestOrderEuCountry != null && lowestOrderEuCountry.CY_Order == sequenceNumber && lowestOrderEuCountry.CY_Data != customsOfficeCountryCode)
			{
				Parent.AddRowMessageError(message);
			}
		}

		List<string> EuropeanUnionCountries => europeanUnionCountries ??= Parent.Factory.GetEuropeanUnionForCustomsMembers().ToList();
		List<string> europeanUnionCountries;
	}
}
