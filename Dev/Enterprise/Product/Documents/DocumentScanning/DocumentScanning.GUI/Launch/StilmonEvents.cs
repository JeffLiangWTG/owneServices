using System;
using System.Collections;

namespace Enterprise.DocumentScanning.Launch
{
	public enum StilmonEventType
	{
		None, PaperLoaded, ScanPressed
	}

	public class StilmonEventNode
	{
		public StilmonEventNode(string eventString, StilmonEventType eventType)
		{
			this.EventString = eventString;
			this.EventType = eventType;
		}

		public string EventString;
		public StilmonEventType EventType;
	}

	public class StilmonEventList
	{
		readonly ArrayList fList = new ArrayList();

		protected void Register(string eventString, StilmonEventType eventType)
		{
			fList.Add(new StilmonEventNode(eventString, eventType));
		}

		protected StilmonEventNode this[int index]
		{
			get { return (StilmonEventNode)fList[index]; }
		}

		public StilmonEventType GetEventTypeByEventString(string eventString)
		{
			StilmonEventNode curNode = GetEventNodeForString(eventString);
			if (curNode != null)
			{
				return curNode.EventType;
			}
			else
			{
				return StilmonEventType.None;
			}
		}

		protected StilmonEventNode GetEventNodeForString(string eventString)
		{
			StilmonEventNode curNode = null;

			eventString = eventString.ToUpper();

			for (int i = 0; i < fList.Count; i++)
			{
				curNode = this[i];

				if (eventString.IndexOf(curNode.EventString) >= 0)
				{
					//MessageBox.Show("Match Found ! "); 
					return curNode;
				}
			}

			//MessageBox.Show("Match NOT Found ! "); 
			return null;
		}

		StilmonEventList() : base()
		{
			RegisterEvents();
		}

		void RegisterEvents()
		{
			// Fujitsu
			Register("F5D8E2A0-CCA4-11D2-B118-00A0C93EE7E0", StilmonEventType.ScanPressed);

			// From Microsoft's sti.h. MS standard Scan button Guid (?) 
			Register("A6C5A715-8C6E-11D2-977A-0000F87A926F", StilmonEventType.ScanPressed);

			// Fujitsu Paper Detected
			Register("F5D8E1A0-CCA4-11D2-B118-00A0C93EE7E0", StilmonEventType.PaperLoaded);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[ThreadStatic]
		static StilmonEventList fStilmonEvents = null;

		// make Singleton.
		public static StilmonEventList StilmonEvents
		{
			get
			{
				if (fStilmonEvents == null)
				{
					fStilmonEvents = new StilmonEventList();
				}
				return fStilmonEvents;
			}
		}

		public static StilmonEventType StilmonEventNameToType(string eventName)
		{
			return StilmonEvents.GetEventTypeByEventString(eventName);
		}
	}
}
