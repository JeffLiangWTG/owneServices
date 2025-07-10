using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public abstract class UPEDataLine : DataLine
	{
		#region Constants

		public static class Constants
		{
			public const string RefLocoSystemUsage = UPEOtherLocoMapSystemUsageList.Codes.Ups;
			public const int MinRecordLength = 50;
		}

		#endregion

		public UPEDataLine(UPEManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		public override void Process()
		{
			PopulateHeaderDetails();
			DoProcessing();
		}

		public virtual ZBool ValidateLine()
		{
			if (line.Length < Constants.MinRecordLength)
			{
				return ZBool.False;
			}
			else
			{
				return ZBool.True;
			}
		}

		public virtual void PopulateHeaderDetails()
		{
			SetManifestType();
			SetTransportMode();
			ZString destCountry = SafeSubstring(line, 6, 2);
			SetPortOfDeparture(SafeSubstring(line, 0, 2), SafeSubstring(line, 2, 4));
			SetPortOfDestination(destCountry, SafeSubstring(line, 8, 4));
			SetCountryOfDestination(destCountry);
			SetAirWayBill(SafeSubstring(line, 18, header.ED_AirWayBillInfo.MaxLength));
		}

		#region Properties

		public ZString ED_RL_NKPortOfDeparture
		{
			get
			{
				return header.ED_RL_NKPortOfDeparture;
			}
		}

		public ZString ED_RL_NKPortOfDestination
		{
			get
			{
				return header.ED_RL_NKPortOfDestination;
			}
		}

		public ZString ED_RN_NKCountryOfDestination
		{
			get
			{
				return header.ED_RN_NKCountryOfDestination;
			}
		}

		public ZString ED_AirWayBill
		{
			get
			{
				return header.ED_AirWayBill;
			}
		}

		#endregion

		#region Implementation

		internal protected void SetManifestType()
		{
			if (header.ED_ManifestType.IsEmpty)
			{
				header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			}
		}

		internal protected void SetTransportMode()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
		}

		internal protected void SetPortOfDeparture(ZString originCountry, ZString originPort)
		{
			if (header.ED_RL_NKPortOfDeparture.IsEmpty)
			{
				ZGuid countryGuid = SetCountryGuid(originCountry);
				if (!countryGuid.IsEmpty)
				{
					RefLocoConverter converter = new RefLocoConverter(header.Factory);
					header.ED_RL_NKPortOfDeparture = converter.GetUNLOCOString(Constants.RefLocoSystemUsage, originPort, countryGuid);
				}
			}
		}

		internal protected void SetPortOfDestination(ZString destCountry, ZString destPort)
		{
			if (header.ED_RL_NKPortOfDestination.IsEmpty)
			{
				ZGuid countryGuid = SetCountryGuid(destCountry);
				if (!countryGuid.IsEmpty)
				{
					RefLocoConverter converter = new RefLocoConverter(header.Factory);
					header.ED_RL_NKPortOfDestination = converter.GetUNLOCOString(Constants.RefLocoSystemUsage, destPort, countryGuid);
				}
			}
		}

		internal protected void SetCountryOfDestination(ZString destCountry)
		{
			if (header.ED_RN_NKCountryOfDestination.IsEmpty)
			{
				header.ED_RN_NKCountryOfDestination = destCountry;
			}
		}

		internal protected void SetAirWayBill(ZString airWayBill)
		{
			if (header.ED_AirWayBill.IsEmpty && airWayBill.Length == header.ED_AirWayBillInfo.MaxLength)
			{
				header.ED_AirWayBill = airWayBill;
			}
		}

		#region Helper

		protected ZGuid SetCountryGuid(ZString countryCode)
		{
			ZGuid countryGuid;
			RefCountry country = (RefCountry)header.Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, countryCode);
			if (country != null)
			{
				countryGuid = country.PK;
			}
			else
			{
				countryGuid = ZGuid.Empty;
			}

			return countryGuid;
		}

		#endregion
		#endregion
		#region Implementation
		#endregion
		internal string internalLine
		{
			get { return line; }
			set { line = value; }
		}
	}
}
