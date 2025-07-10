using System.Collections;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestHeaderManifestHeaderWrapper : IManifestHeaderWrapper
	{
		public ExportCustomsManifestHeaderManifestHeaderWrapper(ExportCustomsManifestHeader header)
		{
			this.Header = header;
		}

		public ZString MessageFunctionCode
		{
			get { return MessageFunctionCodeList.Original.ToString(); }
		}

		public ZDateTime DateOfDeparture
		{
			get
			{
				return Header.ED_DepartureDate;
			}
		}

		public ZString FlightNumber
		{
			get
			{
				return Header.ED_FlightNumber.KeepChars("1234567890");
			}
		}

		public bool IsAir
		{
			get
			{
				return Header.ED_TransportMode == Enterprise.Core.Constants.TransportModes.Air;
			}
		}

		public bool IsSea
		{
			get
			{
				return Header.ED_TransportMode == Enterprise.Core.Constants.TransportModes.Sea;
			}
		}

		public ZString PortOfLoading
		{
			get
			{
				return Header.ED_RL_NKPortOfDeparture;
			}
		}

		public ZString CountryOfDischarge
		{
			get
			{
				return Header.ED_RN_NKCountryOfDestination;
			}
		}

		public ZString VesselID
		{
			get
			{
				return Header.ED_LloydsIMO;
			}
		}

		public ZString VoyageNumber
		{
			get
			{
				return Header.ED_VoyageNumber;
			}
		}

		public ZString AirlineCode
		{
			get
			{
				return Header.ED_FlightNumber.ToUpper().KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			}
		}

		public ZString DepotPremiseID
		{
			get
			{
				string result = string.Empty;
				if (Header.PackDepotAddress != null && Header.PackDepotAddress.Address != null)
				{
					result = Header.PackDepotAddress.Address.LocalControlledPremisesID;
				}
				return result;
			}
		}

		public ZString CAN
		{
			get
			{
				return Header.ED_CAN;
			}
		}

		public ZString CCAN
		{
			get
			{
				return Header.ED_CCAN;
			}
		}

		public int TotalPackageCount
		{
			get
			{
				return Header.ED_NoOfPacks;
			}
		}

		public int TotalContainerCount
		{
			get
			{
				return Header.ED_NoOfContainer;
			}
		}

		public int TotalEmptyContainerCount
		{
			get
			{
				return Header.ED_NoOfEmptyContainers;
			}
		}

		public IManifestLineWrapper[] Lines
		{
			get
			{
				ArrayList result = new ArrayList();
				bool cCANWriteOff = false;
				if (Header.ED_CAN.IsEmpty && !Header.ED_CCAN.IsEmpty)
				{
					result.Add(new ExportCustomsManifestHeaderManifestLineWrapper(Header));
					cCANWriteOff = true;
				}
				foreach (ExportCustomsManifestLines line in Header.Lines)
				{
					result.Add(new ExportCustomsManifestLinesManifestLineWrapper(line, (int)line.EL_LineNo + (cCANWriteOff ? 1 : 0)));
				}
				return (IManifestLineWrapper[])result.ToArray(typeof(IManifestLineWrapper));
			}
		}

		#region Implementation

		protected readonly ExportCustomsManifestHeader Header;
		#endregion
	}
}
