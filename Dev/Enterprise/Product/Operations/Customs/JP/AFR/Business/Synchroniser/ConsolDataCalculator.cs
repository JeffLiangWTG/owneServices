using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class ConsolDataCalculator : Customs.Business.ConsolDataCalculator
	{
		public ConsolDataCalculator(ForwardingConsol consol, JPAFRHeader header)
			: base(consol, Core.Constants.CountryCodes.Japan)
		{
			this.header = header;
		}
		readonly JPAFRHeader header;

		public static ZString GetSCAC(JobDocAddress docAddress)
		{
			var result = ZString.Empty;
			if (docAddress != null && docAddress.HasRealOrganisation)
			{
				result = GetSCAC(docAddress.Organisation);
			}
			return result;
		}

		public static ZString GetSCAC(OrgHeader org)
		{
			var result = ZString.Empty;
			if (org != null)
			{
				result = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Japan).Left(4);
			}
			return result;
		}

		public ZString OrgProxySCAC
		{
			get
			{
				var branch = header.Branch;
				var companyPK = branch == null ? GlbCompany.CurrentCompany.PK : branch.GB_GC;
				var company = Factory.Load<GlbCompany>(companyPK) ?? GlbCompany.CurrentCompany;
				return GetSCAC(Factory.Load<OrgHeader>(company.GC_OH_OrgProxy));
			}
		}

		public ZString CarrierCode
		{
			get { return GetSCAC(consol.ShippingLine); }
		}

		public RefUNLOCO LastForeignPortOfLoading
		{
			get
			{
				var lastForeignPortOfLoad = consol.LastForeignPort;
				var firstJPBoundTransportOrDepartureTransport = FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				if (firstJPBoundTransportOrDepartureTransport != null && (lastForeignPortOfLoad == null ||
					IsPortInTheCountry(lastForeignPortOfLoad) ||
					IsUSBoundTransportMoreValid(firstJPBoundTransportOrDepartureTransport)))
				{
					lastForeignPortOfLoad = firstJPBoundTransportOrDepartureTransport.LoadPort;
				}
				return lastForeignPortOfLoad;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingLastForeignPortOfLoading()
		{
			yield return consol.JK_RL_NKLastForeignPortInfo;
			yield return consol.JK_DateLastForeignPortInfo;

			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_ETD, Transport.Schema.JW_ATD))
			{
				yield return info;
			}
		}

		public Transport LoadTransportForJPBoundVessel
		{
			get
			{
				var allTransportsForUSBoundVessel = AllTransportsForJPBoundVessel;
				return allTransportsForUSBoundVessel.Length > 0 ? allTransportsForUSBoundVessel[0] : null;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingLoadTransportForJPBoundVessel()
		{
			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_Vessel))
			{
				yield return info;
			}
		}

		#region Implementation

		bool IsUSBoundTransportMoreValid(Transport firstUSBoundTransportOrDepartureTransport)
		{
			return firstUSBoundTransportOrDepartureTransport != null && ((firstUSBoundTransportOrDepartureTransport.JW_ATD.IsEmpty ? firstUSBoundTransportOrDepartureTransport.JW_ETD : firstUSBoundTransportOrDepartureTransport.JW_ATD) > consol.JK_DateLastForeignPort &&
						!IsPortInTheCountry(firstUSBoundTransportOrDepartureTransport.LoadPort));
		}

		Transport[] AllTransportsForJPBoundVessel
		{
			get
			{
				var list = new List<Transport>();
				var firstJPBoundTransportOrDepartureTransport = FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				if (firstJPBoundTransportOrDepartureTransport != null)
				{
					list.AddRange(GetAllTransportsForVessel(FirstCountryBoundTransportOrFirstTransportWithTransportMode.JW_Vessel));
				}
				return list.ToArray();
			}
		}

		Transport[] GetAllTransportsForVessel(ZString vessel)
		{
			var result = new List<Transport>();
			foreach (Transport transport in TransportsInLegOrder)
			{
				if (transport.JW_Vessel == vessel)
				{
					result.Add(transport);
				}
			}
			return result.ToArray();
		}

		#endregion

		protected override ZString GetConsolTransportMode()
		{
			return Core.Constants.TransportModes.Sea;
		}
	}
}
