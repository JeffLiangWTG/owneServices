using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI.Test
{
	sealed class InterchangeEventUserControlTest : TestCaseWithFactory
	{
		public void TestAllControlsReadonly()
		{
			var interchange = Factory.New<EDIInterchange>();
			using (var form = new ZForm(interchange))
			{
				var control = new InterchangeEventUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(interchange, "");
				AssertEquals("EventsGrid.ReadOnly", true, control.FindSingleOrDefault<ZGrid>(c => c.Name == "EventsGrid").ReadOnly);
				AssertEquals("xtMsgIdTextBox.ReadOnly", true, control.FindSingleOrDefault<ZTextBox>(c => c.Name == "xtMsgIdTextBox").ReadOnly);
				AssertEquals("xtMsgStatusTextBox.ReadOnly", true, control.FindSingleOrDefault<ZTextBox>(c => c.Name == "xtMsgStatusTextBox").ReadOnly);
				AssertEquals("showAllxTEventsLogCheckBox.ReadOnly", false, control.FindSingleOrDefault<ZCheckBox>(c => c.Name == "showAllxTEventsLogCheckBox").ReadOnly);
				AssertEquals("showRelatedxTEventsLogCheckBox.ReadOnly", false, control.FindSingleOrDefault<ZCheckBox>(c => c.Name == "showRelatedxTEventsLogCheckBox").ReadOnly);
			}
		}

		public void TestAllColumnsOnEventsGrid()
		{
			var interchange = Factory.New<EDIInterchange>();
			using (var form = new ZForm(interchange))
			{
				var control = new InterchangeEventUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(interchange, "");
				var columns = control.FindSingleOrDefault<ZGrid>(c => c.Name == "EventsGrid").Columns;
				AssertEquals("Sequence", columns[0].ColumnName);
				AssertEquals("Sequence ReadOnly", true, columns[0].ColumnStyle.ReadOnly);
				AssertEquals("LogTime", columns[1].ColumnName);
				AssertEquals("LogTime ReadOnly", true, columns[1].ColumnStyle.ReadOnly);
				AssertEquals("LogEvent", columns[2].ColumnName);
				AssertEquals("LogEvent ReadOnly", true, columns[2].ColumnStyle.ReadOnly);
				AssertEquals("LogText", columns[3].ColumnName);
				AssertEquals("LogText ReadOnly", true, columns[3].ColumnStyle.ReadOnly);
				AssertEquals("XtMsgId", columns[4].ColumnName);
				AssertEquals("XtMsgId ReadOnly", true, columns[4].ColumnStyle.ReadOnly);
				AssertEquals("InterchangeId", columns[5].ColumnName);
				AssertEquals("InterchangeId ReadOnly", true, columns[5].ColumnStyle.ReadOnly);
			}
		}

		public void TestPopupRelatedInterchangeColumn()
		{
			var interchange1 = Factory.New<EDIInterchangeForTest>();
			interchange1.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange1.EI_XTInternalMsgID = 101;
			interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.KRCustoms;
			interchange1.EI_SessionGUID = ZGuid.NewZGuid();
			interchange1.EI_InterchangeNum = "111";
			var interchange2 = Factory.New<EDIInterchangeForTest>();
			interchange2.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange2.EI_XTInternalMsgID = 102;
			interchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.KRCustoms;
			interchange2.EI_SessionGUID = ZGuid.NewZGuid();
			interchange2.EI_InterchangeNum = "112";
			Factory.Save();

			AssertxTMsgIdColumn("No new form as current form is for the interchange", interchange1, interchange1);
			AssertxTMsgIdColumn("Open New EDIInterchangeForm for interchange1 on interchange2 Form", interchange2, interchange1);
		}

		void AssertxTMsgIdColumn(string message, EDIInterchangeForTest interchangeForCurrentForm, EDIInterchangeForTest interchangeForNewForm)
		{
			interchangeForCurrentForm.RelatedInterchange = interchangeForNewForm;
			using (var form = new ZForm(interchangeForCurrentForm))
			{
				var control = new InterchangeEventUserControl();
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(interchangeForCurrentForm, "");
				var eventsGrid = control.FindSingleOrDefault<ZGrid>(c => c.Name == "EventsGrid");
				control.EventsGrid_DoubleClick(control, null);

				if (interchangeForCurrentForm.PK != interchangeForNewForm.PK)
				{
					var openedForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.GetType() == typeof(EDIInterchangeForm));
					AssertEquals(message, "View Interchange : 111", openedForm.Text);
					openedForm.Close();
				}
				else
				{
					var openedForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.GetType() == typeof(EDIInterchangeForm));
					AssertEquals(message, null, openedForm);
				}
			}
		}

		class EDIInterchangeForTest : EDIInterchange
		{
			public EDIInterchangeForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			public EDIInterchange RelatedInterchange { get; set; }

			public new XtMessageEventsCollection XtMessageEvents
			{
				get
				{
					var eventData = new XtMessageEventData(1, (ulong)RelatedInterchange.EI_XTInternalMsgID, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"test logtext2\"}");
					var xtEvent = new XtMessageEvent(RelatedInterchange, eventData);
					var events = new XtMessageEventsCollection(RelatedInterchange);
					events.Add(xtEvent);
					return events;
				}
			}
		}
	}
}
