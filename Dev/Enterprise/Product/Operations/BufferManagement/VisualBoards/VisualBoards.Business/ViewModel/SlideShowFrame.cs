using System;
using System.Collections.Generic;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business
{
	public class SlideShowFrame : ISlideShowFrame
	{
		public SlideShowFrame(Guid boardPK, string name, short displayTimeSeconds, IBMBoardSection[] sections)
		{
			BoardPK = boardPK;
			BoardName = name;
			DisplayTimeSeconds = displayTimeSeconds;
			Sections = sections;
		}

		public Guid BoardPK { get; private set; }
		public string BoardName { get; private set; }
		public short DisplayTimeSeconds { get; private set; }
		public IEnumerable<IBMBoardSection> Sections { get; private set; }
	}
}
