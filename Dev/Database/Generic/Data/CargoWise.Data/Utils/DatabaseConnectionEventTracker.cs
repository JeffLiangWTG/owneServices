using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.Common;
using CargoWise.Common.MemoryManagement;

namespace CargoWise.Data
{
	public sealed class DatabaseConnectionEventTracker : IEnableStateControl
	{
		static DatabaseConnectionEventTracker()
		{
			MemoryManager.Register("Database Connection Event Tracker", FlushCallback.OnAnyThread, delegate (FlushAction action) // name for diagnostic tool
			{
				Instance.Clear();
				return FlushResult.Exhausted;
			});
		}

		DatabaseConnectionEventTracker()
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly DatabaseConnectionEventTracker instance = new();

		public static DatabaseConnectionEventTracker Instance
		{
			get { return instance; }
		}

		#region Connection Event Tracking

		public void Clear()
		{
			lock (connectionEventList)
			{
				connectionEventList.Clear();
			}
		}

		#region Enabled

		void IEnableStateControl.SetState(bool newValue)
		{
			isEnabled = newValue;
		}

		public bool IsEnabled
		{
			get { return isEnabled; }
		}

		bool isEnabled = true;

		#endregion

		public bool HasConnections
		{
			get
			{
				lock (connectionEventList)
				{
					return connectionEventList.Count > 0;
				}
			}
		}

		public void AddConnectionEvent(IDbConnection connection, string additionalInfo = null)
		{
			if (IsEnabled && connection != null)
			{
#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
				var timeNow = DateTime.UtcNow; // Cannot use Env.Time as it results in an infinite
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
				StringBuilder message = new StringBuilder();
				message.Append(string.Format("Status={0}{1}connection string={2}{1}", connection.State, Environment.NewLine, connection.ConnectionString)); // diagnostic message
				message.Append(string.Format("   [Time={0}] [Current thread ID={1}]{2}{3}", timeNow.ToLongTimeString(), Thread.CurrentThread.ManagedThreadId, additionalInfo, Environment.NewLine)); // diagnostic message
				message.Append(Environment.NewLine);

				AddToListAndRemoveOverMax(connectionEventList, message.ToString(), MaxEvents);
			}
		}

		void AddToListAndRemoveOverMax(List<string> list, string message, int max)
		{
			Argument.NotNull(list, nameof(list)); // Suggested By ReviewBot 
			Argument.NotNull(message, nameof(message));

			lock (list)
			{
				list.Add(message);
				while (list.Count > max)
				{
					list.RemoveAt(0);
				}
			}
		}

		public string ConnectionEventDescription
		{
			get
			{
				return GetConnectionEventDescription("ConnectionEvents", connectionEventList);
			}
		}

		string GetConnectionEventDescription(string startElement, List<string> list)
		{
			Argument.NotNull(list, nameof(list)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(startElement, nameof(startElement)); // Suggested By ReviewBot 

			StringWriter strWriter = new StringWriter();
			XmlTextWriter xtw = new XmlTextWriter(strWriter);

			xtw.WriteStartElement(startElement);
			lock (list)
			{
				xtw.WriteElementString("Count", list.Count.ToString());

				foreach (string message in list)
				{
					xtw.WriteElementString("Command", message);
				}
			}
			xtw.WriteEndElement();

			return strWriter.GetStringBuilder().ToString();
		}

		#endregion Connection Event Tracking

		#region Implementation

		const int maxEvents = 50;

		int MaxEvents
		{
			get { return maxEvents; }
		}

		readonly List<string> connectionEventList = new List<string>();

		#endregion Implementation
	}
}
