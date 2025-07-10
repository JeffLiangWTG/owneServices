using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;

namespace Enterprise.LogWalker
{
	internal class SystemLogSubscribers : IEnumerable<LogSubscriber>
	{
		/// <summary>
		/// Add Log Walker Subscribers here
		/// </summary>
		public IEnumerator<LogSubscriber> GetEnumerator()
		{
			foreach (LogSubscriber logSubscriber in (IEnumerable)ObjectFactory.Get("SystemLogSubscribers"))
			{
				if (logSubscriber.IsRequired)
				{
					yield return logSubscriber;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
