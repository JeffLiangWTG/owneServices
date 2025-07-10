namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA12
{
	using System.Text.RegularExpressions;
	using CargoWise.Types;

	/// <summary>
	/// Object to parse and hold the top half of an RRA12 string.
	/// </summary>
	public class RRA12MessageHeader
	{
		public const string DateFormatMask = "dd/MM/yy HH:mm";

		public RRA12MessageHeader()
		{ }

		public RRA12MessageHeader(
			ZDateTime adviceDate,
			ZString uVI,
			ZString vessel,
			ZDateTime actualArrivalDate,
			ZString agentsReference,
			ZString entryNumber,
			ZDateTime entryDate,
			ZString badge)
		{
			AgentsReference = agentsReference;
			EntryNumber = entryNumber;
			UVI = uVI;
			Vessel = vessel;
			EntryDate = entryDate;
			ActualArrivalDate = actualArrivalDate;
			AdviceDate = adviceDate;
			Badge = badge;
		}

		public ZString AgentsReference { get; private set; }

		public ZString EntryNumber { get; private set; }

		public ZString UVI { get; private set; }

		public ZString Vessel { get; private set; }

		public ZDateTime EntryDate { get; private set; }

		public ZDateTime ActualArrivalDate { get; private set; }

		public ZDateTime AdviceDate { get; private set; }

		public ZString Badge { get; private set; }

		public static RRA12MessageHeader CreateFromHeaderBlockISL(ZString headerBlock)
		{
			string[] elements = Regex.Split(headerBlock, "~");

			ZDateTime adviceDate;
			ZDateTime.TryParseExact(elements[1], out adviceDate, "yyMMddHHmm");
			ZString badge = elements[2];
			ZString uVI = elements[3];
			ZDateTime actualArrivalDate;
			ZDateTime.TryParseExact(elements[4], out actualArrivalDate, "yyMMddHHmm");
			ZString nominatedAgent = elements[5];
			ZString agentsReference = elements[6];
			ZDateTime entryDate;
			ZString entryNumber;
			SetEntryNumberAndEntryDate(elements[7], out entryNumber, out entryDate);

			return new RRA12MessageHeader(adviceDate, uVI,
											"",  // vessel - not in ISL
											actualArrivalDate, agentsReference, entryNumber, entryDate, badge);
		}

		static void SetEntryNumberAndEntryDate(ZString input, out ZString entryNumber, out ZDateTime entryDate)
		{
			var isChiefEntry = input.Trim().Length <= 16;
			if (isChiefEntry)
			{
				string epu = input.SubstringSafe(0, 3);
				string eno = input.SubstringSafe(3, 7);
				string entryDateString = input.SubstringSafe(10, 6);
				entryNumber = string.Format("{0} {1}", epu, eno);  // Note, no hyphen!
				ZDateTime.TryParseExact(entryDateString, out entryDate, "ddMMyy");
			}
			else
			{
				entryNumber = input.Trim();
				entryDate = ZDateTime.Empty;
			}
		}
	}
}
