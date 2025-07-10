using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class ChangeTransactionDatesNotificationSubscriberGuiHelperTest : NotificationSubscriberGuiHelperTest
	{
		public void TestYesNoCancelMessage()
		{
			QueryUserYesNoCancelEventArgs args = new QueryUserYesNoCancelEventArgs("Message", true);
			ChangeTransactionDatesBusinessObject testBizo = new ChangeTransactionDatesBusinessObject(null, new BusinessObjectFactory());
			testBizo.InvoiceDate = ZDateTime.BrettsBirthday.AddDays(-10);
			testBizo.PostDate = ZDateTime.BrettsBirthday;
			TestNotificationHelper.SetChangeTransactionDateBusinessObject(testBizo);

			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-10), TestNotificationHelper.TransactionDate);
			AssertEquals(ZDateTime.BrettsBirthday, TestNotificationHelper.PostDate);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			TestNotificationHelper.QueryUser(args);

			ChangeTransactionDatesMessageBox lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(false, lastShownDialog.ShowYesNoAllButtons);

			ChangeTransactionDatesBusinessObject lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-10), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, lastShownBizo.PostDate);

			AssertEquals(false, args.Response);
			AssertEquals(true, args.Cancel);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestNotificationHelper.QueryUser(args);

			lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(false, lastShownDialog.ShowYesNoAllButtons);

			lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-10), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, lastShownBizo.PostDate);

			AssertEquals(true, args.Response);
			AssertEquals(false, args.Cancel);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			TestNotificationHelper.QueryUser(args);

			lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(false, lastShownDialog.ShowYesNoAllButtons);

			lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-10), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, lastShownBizo.PostDate);

			AssertEquals(false, args.Response);
			AssertEquals(false, args.Cancel);
		}

		public void TestYesNoAllMessage()
		{
			QueryUserYesNoYesAllNoAllEventArgs args = new QueryUserYesNoYesAllNoAllEventArgs();
			ChangeTransactionDatesBusinessObject testBizo = new ChangeTransactionDatesBusinessObject(null, new BusinessObjectFactory());
			testBizo.InvoiceDate = ZDateTime.BrettsBirthday.AddDays(-10);
			testBizo.PostDate = ZDateTime.BrettsBirthday;
			TestNotificationHelper.SetChangeTransactionDateBusinessObject(testBizo);

			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-10), TestNotificationHelper.TransactionDate);
			AssertEquals(ZDateTime.BrettsBirthday, TestNotificationHelper.PostDate);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			TestNotificationHelper.YesNoAllResultToReturnFromDialog = YesNoYesAllNoAllMessageBoxResult.Yes;
			TestNotificationHelper.QueryUser(args);

			ChangeTransactionDatesMessageBox lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(true, lastShownDialog.ShowYesNoAllButtons);

			ChangeTransactionDatesBusinessObject lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-10), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, lastShownBizo.PostDate);

			AssertEquals(true, args.Response);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			TestNotificationHelper.YesNoAllResultToReturnFromDialog = YesNoYesAllNoAllMessageBoxResult.No;
			TestNotificationHelper.QueryUser(args);

			lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(true, lastShownDialog.ShowYesNoAllButtons);

			lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-10), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, lastShownBizo.PostDate);

			AssertEquals(false, args.Response);
		}

		public void TestYesToAllPopulateBothDatesToAll()
		{
			QueryUserYesNoYesAllNoAllEventArgs args = new QueryUserYesNoYesAllNoAllEventArgs();
			TestChangeTransactionDatesBusinessObject testYesToAllBizo = new TestChangeTransactionDatesBusinessObject();

			testYesToAllBizo.InvoiceDate_ReadOnlyForTest = false;
			testYesToAllBizo.InvoiceDate = ZDateTime.BrettsBirthday.AddDays(-5);
			testYesToAllBizo.PostDate_ReadOnlyForTest = false;
			testYesToAllBizo.PostDate = ZDateTime.BrettsBirthday.AddDays(-3);
			AssertEquals(false, testYesToAllBizo.InvoiceDateInfo.ReadOnly);
			AssertEquals(false, testYesToAllBizo.PostDateInfo.ReadOnly);

			TestNotificationHelper.SetChangeTransactionDateBusinessObject(testYesToAllBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), TestNotificationHelper.TransactionDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-3), TestNotificationHelper.PostDate);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestNotificationHelper.YesNoAllResultToReturnFromDialog = YesNoYesAllNoAllMessageBoxResult.YesToAll;
			TestNotificationHelper.QueryUser(args);
			AssertEquals(true, args.Response);

			ChangeTransactionDatesMessageBox lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(true, lastShownDialog.ShowYesNoAllButtons);

			ChangeTransactionDatesBusinessObject lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-3), lastShownBizo.PostDate);

			AssertDatesAreTransaferred(true, true, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday);

			AssertDatesAreTransaferred(true, false, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday.AddDays(-3));

			AssertDatesAreTransaferred(false, true, ZDateTime.BrettsBirthday.AddDays(-5), ZDateTime.BrettsBirthday);

			AssertDatesAreTransaferred(false, false, ZDateTime.BrettsBirthday.AddDays(-5), ZDateTime.BrettsBirthday.AddDays(-3));
		}

		public void TestYesToAllPopulatePostDateToAll()
		{
			QueryUserYesNoYesAllNoAllEventArgs args = new QueryUserYesNoYesAllNoAllEventArgs();
			TestChangeTransactionDatesBusinessObject testYesToAllBizo = new TestChangeTransactionDatesBusinessObject();

			testYesToAllBizo.InvoiceDate_ReadOnlyForTest = true;
			testYesToAllBizo.InvoiceDate = ZDateTime.BrettsBirthday.AddDays(-5);
			testYesToAllBizo.PostDate_ReadOnlyForTest = false;
			testYesToAllBizo.PostDate = ZDateTime.BrettsBirthday.AddDays(-3);
			AssertEquals(true, testYesToAllBizo.InvoiceDateInfo.ReadOnly);
			AssertEquals(false, testYesToAllBizo.PostDateInfo.ReadOnly);

			TestNotificationHelper.SetChangeTransactionDateBusinessObject(testYesToAllBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), TestNotificationHelper.TransactionDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-3), TestNotificationHelper.PostDate);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestNotificationHelper.YesNoAllResultToReturnFromDialog = YesNoYesAllNoAllMessageBoxResult.YesToAll;
			TestNotificationHelper.QueryUser(args);
			AssertEquals(true, args.Response);

			ChangeTransactionDatesMessageBox lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(true, lastShownDialog.ShowYesNoAllButtons);

			ChangeTransactionDatesBusinessObject lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-3), lastShownBizo.PostDate);

			AssertDatesAreTransaferred(true, true, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday);

			AssertDatesAreTransaferred(true, false, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday.AddDays(-3));

			AssertDatesAreTransaferred(false, true, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday);

			AssertDatesAreTransaferred(false, false, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday.AddDays(-3));
		}

		public void TestYesToAllPopulateInvoiceDateToAll()
		{
			QueryUserYesNoYesAllNoAllEventArgs args = new QueryUserYesNoYesAllNoAllEventArgs();
			TestChangeTransactionDatesBusinessObject testYesToAllBizo = new TestChangeTransactionDatesBusinessObject();

			testYesToAllBizo.InvoiceDate_ReadOnlyForTest = false;
			testYesToAllBizo.InvoiceDate = ZDateTime.BrettsBirthday.AddDays(-5);
			testYesToAllBizo.PostDate_ReadOnlyForTest = true;
			testYesToAllBizo.PostDate = ZDateTime.BrettsBirthday.AddDays(-3);
			AssertEquals(false, testYesToAllBizo.InvoiceDateInfo.ReadOnly);
			AssertEquals(true, testYesToAllBizo.PostDateInfo.ReadOnly);

			TestNotificationHelper.SetChangeTransactionDateBusinessObject(testYesToAllBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), TestNotificationHelper.TransactionDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-3), TestNotificationHelper.PostDate);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestNotificationHelper.YesNoAllResultToReturnFromDialog = YesNoYesAllNoAllMessageBoxResult.YesToAll;
			TestNotificationHelper.QueryUser(args);
			AssertEquals(true, args.Response);

			ChangeTransactionDatesMessageBox lastShownDialog = ZFormModaliser.LastFormShownDialogForTest as ChangeTransactionDatesMessageBox;
			AssertNotNull(lastShownDialog);
			AssertEquals(true, lastShownDialog.ShowYesNoAllButtons);

			ChangeTransactionDatesBusinessObject lastShownBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastShownBizo);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-5), lastShownBizo.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-3), lastShownBizo.PostDate);

			AssertDatesAreTransaferred(true, true, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday);

			AssertDatesAreTransaferred(true, false, ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.BrettsBirthday);

			AssertDatesAreTransaferred(false, true, ZDateTime.BrettsBirthday.AddDays(-5), ZDateTime.BrettsBirthday);

			AssertDatesAreTransaferred(false, false, ZDateTime.BrettsBirthday.AddDays(-5), ZDateTime.BrettsBirthday);
		}

		void AssertDatesAreTransaferred(bool invoiceDateReadOnly, bool postDateReadOnly, ZDateTime expectedInvoiceDate, ZDateTime expectedPostDate)
		{
			QueryUserYesNoYesAllNoAllEventArgs args = new QueryUserYesNoYesAllNoAllEventArgs();
			TestChangeTransactionDatesBusinessObject testBizo = new TestChangeTransactionDatesBusinessObject();

			testBizo.InvoiceDate_ReadOnlyForTest = invoiceDateReadOnly;
			testBizo.InvoiceDate = ZDateTime.BrettsBirthday.AddDays(-1);
			testBizo.PostDate_ReadOnlyForTest = postDateReadOnly;
			testBizo.PostDate = ZDateTime.BrettsBirthday;

			AssertEquals(invoiceDateReadOnly, testBizo.InvoiceDateInfo.ReadOnly);
			AssertEquals(postDateReadOnly, testBizo.PostDateInfo.ReadOnly);
			TestNotificationHelper.SetChangeTransactionDateBusinessObject(testBizo);
			AssertEquals("testBizo.InvoiceDate", expectedInvoiceDate, testBizo.InvoiceDate);
			AssertEquals("testBizo.PostDate", expectedPostDate, testBizo.PostDate);

			TestNotificationHelper.QueryUser(args);
			AssertEquals("Response", true, args.Response);

			AssertEquals("testBizo.InvoiceDate", expectedInvoiceDate, testBizo.InvoiceDate);
			AssertEquals("testBizo.PostDate", expectedPostDate, testBizo.PostDate);
		}

		TestChangeTransactionDateNotificationSubscriberGuiHelper TestNotificationHelper
		{
			get { return TestNotificationHelper_internalValue ?? (TestNotificationHelper_internalValue = new TestChangeTransactionDateNotificationSubscriberGuiHelper()); }
		}
		TestChangeTransactionDateNotificationSubscriberGuiHelper TestNotificationHelper_internalValue;

		class TestChangeTransactionDateNotificationSubscriberGuiHelper : ChangeTransactionDatesNotificationSubscriberGuiHelper
		{
			public TestChangeTransactionDateNotificationSubscriberGuiHelper()
				: base()
			{
			}

			public YesNoYesAllNoAllMessageBoxResult YesNoAllResultToReturnFromDialog;

			protected override ChangeTransactionDatesMessageBox GetNewMessageBox()
			{
				ChangeTransactionDatesMessageBox messageBox = base.GetNewMessageBox();
				messageBox.YesNoAllResult = YesNoAllResultToReturnFromDialog;
				return messageBox;
			}
		}

		class TestChangeTransactionDatesBusinessObject : ChangeTransactionDatesBusinessObject
		{
			public TestChangeTransactionDatesBusinessObject()
				: base(null, new BusinessObjectFactory())
			{
			}

			protected override bool InvoiceDate_ReadOnly
			{
				get { return InvoiceDate_ReadOnlyForTest ?? base.InvoiceDate_ReadOnly; }
			}
			public bool? InvoiceDate_ReadOnlyForTest;

			protected override bool PostDate_ReadOnly
			{
				get { return PostDate_ReadOnlyForTest ?? base.PostDate_ReadOnly; }
			}
			public bool? PostDate_ReadOnlyForTest;
		}
	}
}
