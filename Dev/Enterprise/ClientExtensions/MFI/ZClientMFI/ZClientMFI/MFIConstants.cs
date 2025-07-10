
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.MFI
{
	public static class MFIConstants
	{
		public static class BillofLading
		{
			public static class BillType
			{
				public const string MNZ = "MNZ";
			}
		}

		public static class NZ
		{
			public const string CompanyCode = "AKL";

			public static ZBool ClientSpecificCondition
			{
				get
				{
					return (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.NewZealand && GlbCompany.CurrentCompany.GC_Code == MFIConstants.NZ.CompanyCode);
				}
			}
		}

		public static class AutoeDoc
		{
			public static class DocumentPrefix
			{
				public const string CommercialDocs_ORD = "ORD.";
				public const string Various_ORG = "ORG.";
				public const string OceanBills_OBL = "OBL.";
				public const string OceanBills_O = "O.";
				public const string CommercialDocs_BOO = "BOO.";
				public const string OceanBills_BO = "BO.";
				public const string HouseBills_BL = "BL.";
				public const string CommercialDocs_B = "B.";
				public const string CommercialDocs_COMOBL = "COMOBL.";
				public const string CommercialDocs_COM = "COM.";
				public const string AgentAccountNotes_C = "C.";
				public const string CommercialDocs_S = "S.";
				public const string AgentAccountNotes_AGI = "AGI.";
				public const string TransportConNote_TSP = "TSP.";
				public const string ExpressHouseBill_TLX = "TLX.";
			}

			public static class DocRefType
			{
				public const string Order = "ORD";
				public const string Organisation = "ORG";
				public const string Masterbill = "MST";
				public const string Container = "CNT";
				public const string Consol = "CON";
				public const string Booking = "BKG";
				public const string Declaration = "DEC";
				public const string Housebill = "HBL";
				public const string Shipment = "SHP";
				public const string ConNotePOD = "POD";
				public const string Unknown = "UKN";
			}

			public static class BusinessRefType
			{
				public const string Order = "ORD";
				public const string Organisation = "ORG";
				public const string Consol = "CON";
				public const string Booking = "BKG";
				public const string Declaration = "DEC";
				public const string Shipment = "SHP";
				public const string ConNote = "CNN";
			}
		}

		public static class ServiceTasks
		{
			public const string DocumentAllocation = "ZM1";
			public const string CaroTransExport = "ZM2";
		}
	}
}
