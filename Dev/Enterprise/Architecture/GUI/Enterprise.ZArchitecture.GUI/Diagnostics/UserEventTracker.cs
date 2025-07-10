using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class UserEventTracker : IUserEventTracker
	{
		#region Construction

		public static UserEventTracker Instance
		{
			get
			{
				if (instance.Value == null)
				{
					instance.Value = new UserEventTracker();
				}

				return instance.Value;
			}
		}

		static readonly ThreadLocalOverridable<UserEventTracker> instance = new ThreadLocalOverridable<UserEventTracker>();

		UserEventTracker()
		{
		}

		#endregion

		#region Enabled

		public bool IsEnabled
		{
			get { return fIsEnabled; }
			private set
			{
				fIsEnabled = value;
				((IEnableStateControl)SqlEventTracker.Instance).SetState(value);
				((IEnableStateControl)DatabaseConnectionEventTracker.Instance).SetState(value);
			}
		}

		bool fIsEnabled = true;

		public IDisposable TemporarilyDisable() => new TemporaryEnableStateChanger(false);

		public class TemporaryEnableStateChanger : IDisposable
		{
			readonly bool originalValue;

			public TemporaryEnableStateChanger(bool tempStateValue)
			{
				originalValue = UserEventTracker.Instance.IsEnabled;
				UserEventTracker.Instance.IsEnabled = tempStateValue;
			}

			void IDisposable.Dispose()
			{
				UserEventTracker.Instance.IsEnabled = originalValue;
			}
		}

		#endregion

		#region Clear

		public void ClearAll()
		{
			SqlEventTracker.Instance.Clear();
			DatabaseConnectionEventTracker.Instance.Clear();

			lock (UserEventList)
			{
				UserEventList.Clear();
			}
		}

		#endregion

		#region User Event Tracking

		#region User Statistics

		WeakReferencedKeyDictionary<Form, FormUserStatistics> FormUserStatisticsList
		{
			get
			{
				if (formUserStatisticsList == null)
				{
					formUserStatisticsList = new WeakReferencedKeyDictionary<Form, FormUserStatistics>();
				}
				return formUserStatisticsList;
			}
		}

		WeakReferencedKeyDictionary<Form, FormUserStatistics> formUserStatisticsList;

		public FormUserStatistics GetFormStatsForForm(Form form)
		{
			if (form != null)
			{
				FormUserStatistics formUserStats;
				if (FormUserStatisticsList.ContainsKey(form))
				{
					formUserStats = FormUserStatisticsList[form];
				}
				else
				{
					formUserStats = new FormUserStatistics();
					FormUserStatisticsList[form] = formUserStats;
				}
				return formUserStats;
			}
			return null;
		}

		#endregion

		public void AddUserEvent(Control control, string eventName, string eventData)
		{
			if (IsEnabled)
			{
				UserEvent userEvent = new UserEvent();
				userEvent.ControlName = ControlNameWithHierarchy(control);
				userEvent.EventData = eventData;
				userEvent.EventName = eventName;
				userEvent.EventReference = GetReference(control);

				lock (UserEventList)
				{
					UserEventList.Add(userEvent);
				}

				RemoveOldUserEvents();
			}
		}

		static string GetReference(Control control)
		{
			for (Control current = control; current != null; current = current.Parent)
			{
				string result = UserEventDiagnosticReferenceAttribute.Render(current);

				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public string UserEventDescription
		{
			get
			{
				StringWriter strWriter = new StringWriter();
				XmlTextWriter xtw = new XmlTextWriter(strWriter);

				xtw.WriteStartElement("UserEvents");
				WriteUserEvents(xtw);
				xtw.WriteEndElement();

				return strWriter.GetStringBuilder().ToString();
			}
		}

		public void AddUserEventToControl(Control control)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
				control.MouseDown += new MouseEventHandler(Control_MouseDown);
				control.GotFocus += new EventHandler(Control_GotFocus);
			}
		}

		public string SqlEventDescription
		{
			get { return SqlEventTracker.Instance.SqlEventDescription; }
		}

		public string SqlFailedEventDescription
		{
			get { return SqlEventTracker.Instance.SqlFailedEventDescription; }
		}

		public string LastSqlQuery
		{
			get { return SqlEventTracker.Instance.LastSqlQuery; }
		}

		public string DatabaseConnectionEventDescription
		{
			get { return DatabaseConnectionEventTracker.Instance.ConnectionEventDescription; }
		}

		#region Implementation

		const int MaxUserEvents = 30;
		internal static List<UserEvent> UserEventList
		{
			get { return userEventList ?? (userEventList = new List<UserEvent>()); }
		}
		[ThreadStatic]
		static List<UserEvent> userEventList;

		internal class UserEvent
		{
			public DateTime Occurrence = DateTime.Now; // This is only used for diagnostic purposes.
			public string ControlName = string.Empty;
			public string EventName = string.Empty;
			public string EventData = string.Empty;
			public string EventReference = string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localising diagnostic information is counterproductive.")]
		void Control_KeyPress(object sender, KeyPressEventArgs e)
		{
			string data;

			var textBox = sender as TextBox;
			if (textBox != null && textBox.PasswordChar != '\0')
			{
				data = textBox.PasswordChar.ToString();
			}
			else if (e.KeyChar == '\t')
			{
				data = "Tab";
			}
			else if (char.GetUnicodeCategory(e.KeyChar) == System.Globalization.UnicodeCategory.Control)
			{
				data = "Char 0x" + Convert.ToInt32(e.KeyChar).ToString("X");
			}
			else if (char.GetUnicodeCategory(e.KeyChar) == System.Globalization.UnicodeCategory.SpaceSeparator)
			{
				data = "Space";
			}
			else
			{
				data = e.KeyChar.ToString();
			}

			AddUserEvent((Control)sender, "KeyPress", data);

			Control senderAsControl = sender as Control;
			if (senderAsControl != null)
			{
				FormUserStatistics formUserStats = GetFormStatsForForm(senderAsControl.FindForm());
				if (formUserStats != null)
				{
					formUserStats.KeyPresses++;
				}
			}
		}

		void Control_MouseDown(object sender, MouseEventArgs e)
		{
			Control ctrl = (Control)sender;
			AddUserEvent(ctrl, "MouseDown", e.Button.ToString());

			FormUserStatistics formUserStats = GetFormStatsForForm(ctrl.FindForm());
			if (formUserStats != null)
			{
				formUserStats.MouseClicks++;
			}
		}

		void Control_GotFocus(object sender, EventArgs e)
		{
			FormUserStatistics formUserStats = GetFormStatsForForm(((Control)sender).FindForm());
			if (formUserStats != null)
			{
				formUserStats.ControlFocusChanges++;
			}
		}

		string ControlNameWithHierarchy(Control control)
		{
			string controlName = control.Name;
			Control parentControl = control.Parent;

			for (int i = 0; i < 2 && parentControl != null; i++)
			{
				controlName = controlName.Insert(0, parentControl.Name + '.');
				parentControl = parentControl.Parent;
			}

			return controlName;
		}

		void RemoveOldUserEvents()
		{
			lock (UserEventList)
			{
				while (UserEventList.Count > MaxUserEvents)
				{
					UserEventList.RemoveAt(0);
				}
			}
		}

		void WriteUserEvents(XmlTextWriter xtw)
		{
			lock (UserEventList)
			{
				xtw.WriteElementString("Count", UserEventList.Count.ToString());

				foreach (UserEvent userEvent in UserEventList)
				{
					xtw.WriteStartElement("Event"); // Localising diagnostic information is counterproductive.

					xtw.WriteString(userEvent.EventName);
					xtw.WriteElementString("Occurrence", userEvent.Occurrence.ToString("yyyy-MM-dd HH:mm:ss"));
					xtw.WriteElementString("Data", userEvent.EventData);
					xtw.WriteElementString("Control", userEvent.ControlName);
					xtw.WriteElementString("Reference", userEvent.EventReference);

					xtw.WriteEndElement();
				}
			}
		}

		#endregion

		#endregion
	}
}
