using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.DocumentWrappers.Customs.NZ;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Freight.NZ
{
	/// <summary>
	/// For Outward Report
	/// </summary>
	public class DocForwardingConsol : DocumentWrappers.DocForwardingConsol
	{
		protected DocForwardingConsol(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
			: base(consol, factoryToWrap)
		{
		}

		public new static DocForwardingConsol New(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
		{
			DocForwardingConsol result = null;

			if (consol != null)
			{
				result = new DocForwardingConsol(consol, factoryToWrap);
			}

			return result;
		}

		public ZString RefNoWithMAWB
		{
			get
			{
				ZString result = ConsolNumber;
				if (Consol.IsAir)
				{
					result += "/" + Consol.JK_MasterBillNum.SubstringSafe(0, 3) + "-" + Consol.JK_MasterBillNum.SubstringSafe(3);
				}
				return result;
			}
		}

		public ZString ORN
		{
			get { return ORNEntryNumber == null ? ZString.Empty : ORNEntryNumber.CE_EntryNum; }
		}

		public ZString VesselAircraftOperator
		{
			get { return Consol.ShippingLine == null ? ZString.Empty : Consol.ShippingLine.OH_FullName; }
		}

		Transport GetInternationalExportLeg()
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			return (from Transport transport in Consol.Transports
					orderby transport.JW_LegOrder
					where transport.JW_RL_NKLoadPort.Left(2) == currentCountry && transport.JW_RL_NKDiscPort.Left(2) != currentCountry
					select transport).FirstOrDefault();
		}

		public ZString CraftFlightNo
		{
			get
			{
				var result = ZString.Empty;

				var internationalLeg = GetInternationalExportLeg();

				if (internationalLeg != null)
				{
					if (Consol.IsSea)
					{
						result = internationalLeg.JW_Vessel;
					}
					else if (Consol.IsAir)
					{
						result = internationalLeg.JW_VoyageFlight;
					}
				}
				return result;
			}
		}

		public ZString VoyageNo
		{
			get
			{
				var result = ZString.Empty;

				var internationalLeg = GetInternationalExportLeg();

				if (internationalLeg != null && Consol.IsSea)
				{
					result = internationalLeg.JW_VoyageFlight;
				}
				return result;
			}
		}

		public ZString DeparturePort
		{
			get
			{
				var internationalLeg = GetInternationalExportLeg();

				var result = ZString.Empty;
				if (internationalLeg != null)
				{
					result = internationalLeg.JW_RL_NKLoadPort;

					var port = internationalLeg.LoadPort;

					if (port != null)
					{
						result += ":" + port.RL_PortName;
					}
				}

				return result;
			}
		}

		public ZString DepartureDate
		{
			get
			{
				var internationalLeg = GetInternationalExportLeg();
				return internationalLeg != null ? internationalLeg.JW_ETD.ToShortDateString() : string.Empty;
			}
		}

		public ZString DischargePort
		{
			get
			{
				var internationalLeg = GetInternationalExportLeg();

				var result = ZString.Empty;
				if (internationalLeg != null)
				{
					result = internationalLeg.JW_RL_NKDiscPort;

					var port = internationalLeg.DiscPort;

					if (port != null)
					{
						result += ":" + port.RL_PortName;
					}
				}

				return result;
			}
		}

		public override ZString ContainerNumbers
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (CommonContainer container in Consol.Containers)
				{
					result.Append(container.JC_ContainerNum + ", ");
				}

				ZString final = result.ToString();
				return final.TrimEndIncludingWhiteSpace(',');
			}
		}

		DocShipmentCollection fOrderedShipments;
		public DocShipmentCollection OrderedShipments
		{
			get
			{
				if (fOrderedShipments == null)
				{
					fOrderedShipments = new DocShipmentCollection(Factory);
					fOrderedShipments.Contruct(Consol.Shipments);
				}
				return fOrderedShipments;
			}
		}

		#region MAF Cover Sheet DocWrapper

		public DocMAFCoverSheet MAFCS
		{
			get { return mafcs ?? (mafcs = new DocMAFCoverSheet(Factory, Consol.Factory.GetValue<NZDocsMAFCoverSheet>() ?? new NZDocsMAFCoverSheet(Consol))); }
		}
		DocMAFCoverSheet mafcs;

		#endregion

		#region Implementation

		CusEntryNumber fORNEntryNumber;
		protected CusEntryNumber ORNEntryNumber
		{
			get
			{
				if (fORNEntryNumber == null)
				{
					fORNEntryNumber = (CusEntryNumber)Consol.Factory.LoadTop1(typeof(CusEntryNumber), CusEntryNumber.GetEntryNumberFilter(Consol));
				}
				return fORNEntryNumber;
			}
		}

		#endregion
	}
}
