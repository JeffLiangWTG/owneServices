namespace Enterprise.Edifact.CA.D99B.Messages.CUSDEC
{
	using Enterprise.Edifact.Auto;
	using Enterprise.Edifact.D99B.Segments;

	class CUSDECMessage : Edifact.D99B.Messages.CUSDEC.CUSDECMessage
	{
		public CUSDECMessage()
		{
			Group10 = new SegmentGroupMessageSection<SegmentGroup10>(9999);
		}

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[]
					{
						UNH, BGM, CST, LOC, DTM, GIS, FII, MEA, EQD, SEL, FTX, Group1, Group4, Group5, Group6,
						Group7, Group8, UNS1, Group10, Group30, UNS2, CNT, Group49, Group50, UNT
					};
		}

		public new readonly SegmentGroupMessageSection<SegmentGroup10> Group10;
	}

	class SegmentGroup10 : Edifact.D99B.Messages.CUSDEC.SegmentGroup10
	{
		public SegmentGroup10()
		{
			DTM = new DTMSegmentMessageSection(2);
			Group11 = new SegmentGroupMessageSection<SegmentGroup11>(9999);
		}

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { DMS, DTM, MEA, Group11, Group13, Group14, Group16, Group18, Group19, Group21 };
		}

		public new readonly DTMSegmentMessageSection DTM;
		public new readonly SegmentGroupMessageSection<SegmentGroup11> Group11;
	}

	class SegmentGroup11 : Edifact.D99B.Messages.CUSDEC.SegmentGroup11
	{
		public SegmentGroup11()
		{
			MOA = new MOASegmentMessageSection(7);
		}

		protected override MessageSectionBase[] GetMessageSections()
		{
			return new MessageSectionBase[] { MOA, Group12 };
		}

		public new readonly MOASegmentMessageSection MOA;
	}
}
