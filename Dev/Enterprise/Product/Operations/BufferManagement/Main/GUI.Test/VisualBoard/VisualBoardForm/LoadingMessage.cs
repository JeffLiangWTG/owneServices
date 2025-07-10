using System;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class LoadingMessage : Tuple<string, string>
	{
		public LoadingMessage(string sectionName, string message)
			: base(sectionName, message)
		{
		}

		public string SectionName => Item1;
		public string Message => Item2;
	}
}
