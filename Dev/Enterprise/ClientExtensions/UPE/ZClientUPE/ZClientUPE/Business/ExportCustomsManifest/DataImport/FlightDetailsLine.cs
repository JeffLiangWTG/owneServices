using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class FlightDetailsLine : UPEDataLine
	{
		public FlightDetailsLine(UPEManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
			fPortOfLoading = fPortOfDischarge = "-";
			fArrivalDate = ZDateTime.MaxSmallDateTime;
		}

		public override ZBool ValidateLine()
		{
			if (line.Length < 91)
			{
				return ZBool.False;
			}

			return ZBool.True;
		}

		#region Properties

		//
		//		Other details available in the imported file
		//
		//		MAWB TYPE
		//		ORIGIN AIRPORT CODE
		//		DESTINATION AIRPORT CODE
		//		COURIER NAME
		//		COURIER TICKET NUMBER
		//		IT7512 NUMBER
		//		CONTAINER NUMBER
		//		IOPS FLIGHT NUMBER
		//		FILLER
		//

		public ZString ED_FlightNumber
		{
			get
			{
				return header.ED_FlightNumber;
			}
		}

		public ZDateTime ED_DepartureDate
		{
			get
			{
				return header.ED_DepartureDate;
			}
		}

		public ZString PortOfLoading
		{
			get
			{
				if (fPortOfLoading == "-")
				{
					ZString countryCode = SafeSubstring(line, 51, 2);
					ZString localCode = SafeSubstring(line, 53, 4);
					RefCountry country = (RefCountry)header.Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, countryCode);
					if (country == null)
					{
						fPortOfLoading = ZString.Empty;
					}
					else
					{
						RefLocoConverter converter = new RefLocoConverter(header.Factory);
						fPortOfLoading = converter.GetUNLOCOString(Constants.RefLocoSystemUsage, localCode, country.PK);
					}
				}

				return fPortOfLoading;
			}
		}

		public ZString PortOfDischarge
		{
			get
			{
				if (fPortOfDischarge == "-")
				{
					ZString countryCode = SafeSubstring(line, 63, 2);
					ZString localCode = SafeSubstring(line, 65, 4);
					RefCountry country = (RefCountry)header.Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, countryCode);
					if (country == null)
					{
						fPortOfDischarge = ZString.Empty;
					}
					else
					{
						RefLocoConverter converter = new RefLocoConverter(header.Factory);
						fPortOfDischarge = converter.GetUNLOCOString(Constants.RefLocoSystemUsage, localCode, country.PK);
					}
				}

				return fPortOfDischarge;
			}
		}

		public ZDateTime ArrivalDate
		{
			get
			{
				if (fArrivalDate == ZDateTime.MaxSmallDateTime)
				{
					string formatString;
					string eTAString = SafeSubstring(line, 94, 10).Trim(' ');
					if (eTAString.Length == 6)
					{
						formatString = "yyMMdd";
					}
					else
					{
						formatString = "yyMMddhhmm";
					}

					try
					{
						fArrivalDate = DateTime.ParseExact(eTAString, formatString, System.Threading.Thread.CurrentThread.CurrentCulture);
					}
					catch (FormatException) { fArrivalDate = ZDateTime.Empty; }
				}

				return fArrivalDate;
			}
		}

		#endregion

		#region Implementation

		protected override void DoProcessing()
		{
			SetFlightNumber(SafeSubstring(line, 69, 9).Trim(' '));
			SetFlightDepartureTimeStamp(SafeSubstring(line, 81, 10).Trim(' '));
		}

		internal protected void SetFlightNumber(ZString flightNumber)
		{
			if (header.ED_FlightNumber.IsEmpty)
			{
				header.ED_FlightNumber = flightNumber;
			}
		}

		internal protected void SetFlightDepartureTimeStamp(ZString eTDString)
		{
			if (header.ED_DepartureDate.IsEmpty)
			{
				string formatString;
				if (eTDString.Length == 6)
				{
					formatString = "yyMMdd";
				}
				else
				{
					formatString = "yyMMddhhmm";
				}

				try
				{
					header.ED_DepartureDate = DateTime.ParseExact(eTDString, formatString, System.Threading.Thread.CurrentThread.CurrentCulture);
				}
				catch (FormatException) { }
			}
		}

		internal protected ZString fPortOfLoading;
		internal protected ZString fPortOfDischarge;
		protected ZDateTime fArrivalDate;

		#endregion
		#region Implementation
		#endregion
		internal string InternalLine
		{
			get { return line; }
			set { line = value; }
		}
	}
}
