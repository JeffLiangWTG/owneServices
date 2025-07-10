using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI.Testing
{
	sealed class EDIMessageUserControlTest : TestCaseWithFactory
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestBinding()
		{
			DummyBizoWithEDIMessageCollection dummy = Factory.New<DummyBizoWithEDIMessageCollection>();
			TestBind(dummy);
		}

		public void TestSetBindPrepend()
		{
			using (EDIMessageUserControl control = new EDIMessageUserControl())
			{
				control.SetBindPrepend("XXX.");
				Assert("OriginAddressControl.BindToAddress prepended", control.MessagesGrid.BindTo.StartsWith("XXX."));
				Assert("OriginAddressControl.BindToOrgList prepended", control.MessageTextTextBox.BindTo.StartsWith("XXX."));
			}
		}

		public void TestBindPrepend()
		{
			using (EDIMessageUserControl control = new EDIMessageUserControl())
			{
				ZString defaultGridBindTo = control.MessagesGrid.BindTo;
				ZString defaultTextBoxBindTo = control.MessageTextTextBox.BindTo;

				control.BindPrepend = "foo.";
				AssertEquals("foo." + defaultGridBindTo, control.MessagesGrid.BindTo);
				AssertEquals("foo." + defaultTextBoxBindTo, control.MessageTextTextBox.BindTo);

				control.BindPrepend = "bar.";
				AssertEquals("bar." + defaultGridBindTo, control.MessagesGrid.BindTo);
				AssertEquals("bar." + defaultTextBoxBindTo, control.MessageTextTextBox.BindTo);

				control.MessagesGrid.BindTo = "rebind";
				control.MessageTextTextBox.BindTo = "rebind";
				control.BindPrepend = "foo.";
				AssertEquals("foo.rebind", control.MessagesGrid.BindTo);
				AssertEquals("foo.rebind", control.MessageTextTextBox.BindTo);

				control.BindPrepend = "";
				AssertEquals("rebind", control.MessagesGrid.BindTo);
				AssertEquals("rebind", control.MessageTextTextBox.BindTo);
			}
		}

		public void TestBindPrependIsBrowsableProperty()
		{
			Type controlType = typeof(EDIMessageUserControl);
			PropertyInfo bindPrependInfo = controlType.GetProperty("BindPrepend", BindingFlags.Instance | BindingFlags.Public);
			AssertEquals(true, Attribute.IsDefined(bindPrependInfo, typeof(BrowsableAttribute)));
		}

		public void TestAllColumnsReadonly()
		{
			DummyBizoWithEDIMessageCollection dummy = Factory.New<DummyBizoWithEDIMessageCollection>();
			using (ZForm form = new ZForm(dummy))
			{
				EDIMessageUserControl control = new EDIMessageUserControl();
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(dummy, "");
				AssertEquals("MessagesGrid.ReadOnly", true, control.MessagesGrid.ReadOnly);
			}
		}
		
		public void TestMessageHistoryHeadingIsSetUsingMasterBizoHeading()
		{
			DummyBizoWithEDIMessageCollection dummy1 = Factory.New<DummyBizoWithEDIMessageCollection>();
			dummy1.Z0_VarCharMax = "Dummy1";
			EDIMessage message1 = dummy1.Messages.AddNew();
			AssertEquals("Messages count", 2, dummy1.Messages.Count);

			using (ZForm form = new ZForm(dummy1))
			{
				EDIMessageUserControl control = new EDIMessageUserControl();
				form.Controls.Add(control);
				control.MessagesGrid.BindTo = "Messages";

				AssertEquals("ShowChangingBlueMessageHeading", false, control.ShowChangingBlueMessageHeading);
				control.ShowChangingBlueMessageHeading = true;

				form.Show();
				control.SetDataBinding(dummy1, "");

				GroupBox historyGroupBox = GetControl<GroupBox>(control, "HistoryGroupBox");
				control.MessagesGrid.ListManager.Position = 1;
				AssertEquals("HistoryGroupBox Text", "Dummy Dummy1 Messages", historyGroupBox.Text);

				control.ShowChangingBlueMessageHeading = false;
				dummy1.Z0_VarCharMax = "Changed Dummy1";

				historyGroupBox.Text = "Messages";
				control.MessagesGrid.ListManager.Position = 0;
				AssertEquals("HistoryGroupBox Text", "Messages", historyGroupBox.Text);
			}
		}

		void TestBind(BusinessObject bizo)
		{
			using (ZForm form = new ZForm(bizo))
			{
				EDIMessageUserControl control = new EDIMessageUserControl();
				form.Controls.Add(control);

				control.SetDataBinding(bizo, "");
			}
		}

		#region Implementation

		T GetControl<T>(EDIMessageUserControl control, string name)
			where T : Control
		{
			return (T)typeof(EDIMessageUserControl).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control);
		}

		#region TestHelper

		class DummyBizoWithEDIMessageCollection : DummyBusinessObject, IEDIMessageCollectionProvider, IDetailsTabPageHeadingProvider
		{
			public DummyBizoWithEDIMessageCollection(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IEDIMessageCollectionProvider Members

			EDIMessageCollection messages;
			public EDIMessageCollection Messages
			{
				get
				{
					if (messages == null)
					{
						messages = new EDIMessageCollection(this, Factory);
						messages.Load();
						messages.AddNew();
					}
					return messages;
				}
			}

			#endregion

			string IDetailsTabPageHeadingProvider.Heading
			{
				get { return "Dummy " + this.Z0_VarCharMax; }
			}
		}

		#endregion

		#endregion
	}
}
