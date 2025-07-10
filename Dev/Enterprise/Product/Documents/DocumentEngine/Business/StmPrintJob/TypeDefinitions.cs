using System;

namespace Enterprise.DocumentEngine
{
	public enum PrintJobType
	{
		ALL,
		PRN,
		PRS,
		DDS,
		EML,
		FAX,
		FAA, // Fax awaiting acknowledgement
			 //SMS,
		FTP
	}

	public enum PrintType
	{
		PRN,
		PRS,
		DDS,
		EML,
		FAX,
		FAA, // Fax awaiting acknowledgement
		SMS,
		FTP
	}

	public static class PrintTypeName
	{
		static string Name(PrintType printType)
		{
			switch (printType)
			{
				case PrintType.EML:
					return Res.GetString("69462382-dceb-4c28-b145-cf8f14206195", "Email");
				case PrintType.FAX:
					return Res.GetString("b66b7938-d385-44a2-96f1-d1ba1ebfe2d0", "Fax");
				case PrintType.FAA:
					return Res.GetString("e306141c-5da4-4abe-bcfc-20a309c9c1db", "Fax awaiting acknowledgement");
				case PrintType.PRN:
					return Res.GetString("41332957-e0be-4ff4-91d0-d6e729422b39", "Print");
				case PrintType.PRS:
					return Res.GetString("4e22ba9e-6478-4884-ba57-352b9e61520b", "Print sent to remote printer - success");
				case PrintType.DDS:
					return Res.GetString("06E815D5-D68A-4667-8E8C-D1DFA1666268", "Document Delivery Success");
				case PrintType.SMS:
					return Res.GetString("67f71aad-0bb0-4fd9-8eae-847b5ac4e9fd", "SMS");
				case PrintType.FTP:
					return Res.GetString("b41b1898-8104-4e35-9ae2-e31ad516cb4a", "Upload to FTP folder");
				default:
					throw new ArgumentOutOfRangeException("PrintType", printType, "Supplied value is not part of PrintType enumeration");
			}
		}

		static string Name(PrintJobType type)
		{
			switch (type)
			{
				case PrintJobType.ALL:
					return Res.GetString("4dc256b3-fd5b-442c-8521-89d90f38247b", "All types");
				case PrintJobType.EML:
					return Res.GetString("69462382-dceb-4c28-b145-cf8f14206195", "Email");
				case PrintJobType.FAA:
					return Res.GetString("e306141c-5da4-4abe-bcfc-20a309c9c1db", "Fax awaiting acknowledgement");
				case PrintJobType.FAX:
					return Res.GetString("b66b7938-d385-44a2-96f1-d1ba1ebfe2d0", "Fax");
				case PrintJobType.PRN:
					return Res.GetString("41332957-e0be-4ff4-91d0-d6e729422b39", "Print");
				case PrintJobType.PRS:
					return Res.GetString("4e22ba9e-6478-4884-ba57-352b9e61520b", "Print sent to remote printer - success");
				case PrintJobType.DDS:
					return Res.GetString("06E815D5-D68A-4667-8E8C-D1DFA1666268", "Document Delivery Success");
				case PrintJobType.FTP:
					return Res.GetString("b41b1898-8104-4e35-9ae2-e31ad516cb4a", "Upload to FTP folder");
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, "Supplied value is not part of PrintJobType enumeration");
			}
		}

		public static string Name(string code)
		{
			if (Array.Exists<string>(Enum.GetNames(typeof(PrintType)), s => s == code))
			{
				return Name((PrintType)Enum.Parse(typeof(PrintType), code, true));
			}
			else
			{
				return Name((PrintJobType)Enum.Parse(typeof(PrintJobType), code, true));
			}
		}
	}

	public enum PrintJobBlobType
	{
		XLS,
		XLSX,
		TIF
	}
}
