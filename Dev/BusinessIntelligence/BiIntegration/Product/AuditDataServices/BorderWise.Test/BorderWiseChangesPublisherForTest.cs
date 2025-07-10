using System;
using System.Collections.Generic;
using BorderWise.Sync;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	class BorderWiseChangesPublisherForTest : IBorderWiseChangesPublisher
	{
		public void Publish(string key, string message)
		{
			if (message.Contains("EXCEPTION!!!"))
			{
				throw new Exception("Exception was requested");
			}

			Entries.Add(new KeyValuePair<string, string>(key, message));
		}

		public List<KeyValuePair<string, string>> Entries { get; } = new List<KeyValuePair<string, string>>();

		public void Dispose()
		{
		}
	}
}
