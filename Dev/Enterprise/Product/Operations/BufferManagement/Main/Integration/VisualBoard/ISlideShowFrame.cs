using System;
using System.Collections.Generic;

namespace Enterprise.BufferManagement.Integration
{
	public interface ISlideShowFrame
	{
		Guid BoardPK { get; }
		string BoardName { get; }
		short DisplayTimeSeconds { get; }
		IEnumerable<IBMBoardSection> Sections { get; }
	}
}
