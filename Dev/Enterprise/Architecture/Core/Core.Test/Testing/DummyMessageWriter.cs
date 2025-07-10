using System.Collections.Generic;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class DummyMessageWriter : IMessageWriter
	{
		public List<string> Messages { get; private set; }

		public DummyMessageWriter()
		{
			Messages = new List<string>();
		}

		public void WriteMessage(string message)
		{
			Messages.Add(message);
		}
	}
}
