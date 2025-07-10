namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA12
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.GB.MCP.ServiceTasks.RraProcessors.RRA12;

	/// <summary>
	///  Object to parse and hold the line items of an RRA12 string. 
	/// </summary>
	public class RRA12MessageLine
	{
		public RRA12MessageLine(
			ZString uCN,
			ZString bOL,
			ZString contNumber,
			ZString packages,
			ZString devanStataus,
			ZString removalNote)
		{
			UCN = uCN;
			BoL = bOL;
			ContNumber = contNumber;

			Packages = ZInt.ParseSafe(packages, 0);
			DeVanStataus = devanStataus;
			RemovalNote = removalNote.EqualsIgnoringCase("Y");
			Comments = new List<ZString>();
		}

		enum LineTypes
		{
			UCN = 1,
			BoL = 2,
			Container = 3,
			Packages = 4,
			DevanStatus = 5,
			RemovalNote = 6
		}

		public ZString UCN { get; private set; }

		public ZString BoL { get; internal set; }

		public ZString ContNumber { get; private set; }

		public ZInt Packages { get; private set; }

		public ZString DeVanStataus { get; private set; }

		public ZBool RemovalNote { get; private set; }

		public List<ZString> Comments { get; private set; }

		/// <summary>
		/// Reference to link this back to its parent, so we can access its header. The header is the RRA12MessageHeader. Its header is its bigger brother, both children of the RRA12Message)
		/// </summary>
		public RRA12MessageBase Parent { get; set; }

		public static RRA12MessageLine CreateFromSingleLineOfText(ZString line)
		{
			var elements = new Dictionary<LineTypes, ZString>();
			int[] indexes = new int[] { 0, 1, 17, 29, 43, 49, 69, 70 };  // these are the column positions within the file

			for (int columnBlockNumber = 0; columnBlockNumber < indexes.Length - 1; columnBlockNumber++)
			{
				ZString element = ZString.Empty;
				try
				{
					element = line.Substring(indexes[columnBlockNumber], indexes[columnBlockNumber + 1] - indexes[columnBlockNumber]);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce(
												"BGB-MCP-RRA12-ParserError-SingleLine",
												string.Format("Could not make RRA12MessageLine from single line [{0}]. Block number {1}, indexes.Length: {2}", line, columnBlockNumber, indexes.Length),
												ex
											);
				}
				elements.Add((LineTypes)columnBlockNumber, element.TrimEnd());
			}

			return new RRA12MessageLine(
											elements[LineTypes.UCN],
											elements[LineTypes.BoL],
											elements[LineTypes.Container],
											elements[LineTypes.Packages],
											elements[LineTypes.DevanStatus],
											elements[LineTypes.RemovalNote]);
		}

		public override string ToString()
		{
			string commentsAsOne = ZString.Join(", ", Comments.ToArray());
			string row = string.Format("{0}	{1}	{2}	{3}	{4}	{5}	{6}", UCN, BoL, ContNumber, Packages, DeVanStataus, RemovalNote, commentsAsOne);

			return row;
		}
	}
}
