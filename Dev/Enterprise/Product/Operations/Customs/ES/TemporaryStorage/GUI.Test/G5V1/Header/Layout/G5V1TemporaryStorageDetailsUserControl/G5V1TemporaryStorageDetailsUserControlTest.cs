using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	sealed class G5V1TemporaryStorageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestLRNTextBox()
		{
			var lrnTextBox = control.LRNTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", lrnTextBox);
				lrnTextBox.AssertThisControl(x => x.WithBindTo("LRN"));
			});
		}

		public void TestMRNTextBox()
		{
			var mrnTextBox = control.MRNTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", mrnTextBox);
				mrnTextBox.AssertThisControl(x => x.WithBindTo("MRN"));
			});
		}

		public void TestCircuitTextBox()
		{
			var circuitTextBox = control.CircuitTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", circuitTextBox);
				circuitTextBox.AssertThisControl(x => x.WithBindTo("EntryStatus"));
			});
		}

		public void TestClearanceNumberTextBox()
		{
			var clearanceNumberTextBox = control.ClearanceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", clearanceNumberTextBox);
				clearanceNumberTextBox.AssertThisControl(x => x.WithBindTo("ClearanceNumber"));
			});
		}

		public void TestDsdtMrnNumberTextBox()
		{
			var dsdtMrnNumberTextBox = control.DsdtMrnNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", dsdtMrnNumberTextBox);
				dsdtMrnNumberTextBox.AssertThisControl(x => x.WithBindTo("DsdtMrnNumber"));
			});
		}

		public void TestDsdtSdFormatNoUrlUserControl()
		{
			var dsdtSdFormatNoUrlUserControl = control.DsdtSdFormatNoUrlUserControl;
			CombineAssertions(() =>
			{
				AssertType<DsdtSdFormatUserControl>("Type", dsdtSdFormatNoUrlUserControl);
			});
		}

		public void TestDsdtSdFormatHasUrlUserControl()
		{
			var dsdtSdFormatHasUrlUserControl = control.DsdtSdFormatHasUrlUserControl;
			CombineAssertions(() =>
			{
				AssertType<DsdtSdFormatUserControl>("Type", dsdtSdFormatHasUrlUserControl);
			});
		}

		public void TestDsdtSdFormatWritableUserControl()
		{
			var dsdtSdFormatWritableUserControl = control.DsdtMrnBindingMemberUserControl;
			CombineAssertions(() =>
			{
				AssertType<DsdtSdFormatUserControl>("Type", dsdtSdFormatWritableUserControl);
			});
		}

		public void TestCustomsStatusDropEdit()
		{
			var customsStatusDropEdit = control.CustomsStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", customsStatusDropEdit);
				customsStatusDropEdit.AssertThisControl(x => x.WithBindTo("CustomsStatus"));
			});
		}

		public void TestMessageStatusDropEdit()
		{
			var messageStatusDropEdit = control.MessageStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", messageStatusDropEdit);
				messageStatusDropEdit.AssertThisControl(x => x.WithBindTo("AMA_MessageStatus"));
			});
		}

		public void TestAcceptanceDateDateEdit()
		{
			var acceptanceDateDateEdit = control.AcceptanceDateDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", acceptanceDateDateEdit);
				acceptanceDateDateEdit.AssertThisControl(x => x.WithBindTo("AcceptanceDate"));
				AssertEquals("acceptanceDateDateEdit date format", Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, acceptanceDateDateEdit.DateTimeFormat);
			});
		}

		public void TestLAMEEntryNumberTextBox()
		{
			var lameEntryNumberTextBox = control.LAMEEntryNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", lameEntryNumberTextBox);
				lameEntryNumberTextBox.AssertThisControl(x => x.WithBindTo("EntryNumber"));
			});
		}

		public void TestLAMEEntryDateDateEdit()
		{
			var lameEntryDateDateEdit = control.LAMEEntryDateDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", lameEntryDateDateEdit);
				lameEntryDateDateEdit.AssertThisControl(x => x.WithBindTo("EntryDate"));
				AssertEquals("lameEntryDateDateEdit date format", Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, lameEntryDateDateEdit.DateTimeFormat);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new G5V1TemporaryStorageDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		G5V1TemporaryStorageDetailsUserControl control;
	}
}
