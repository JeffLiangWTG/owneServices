#if NETFRAMEWORK
using System;
#endif
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class DragStatusHandlerTest : TestCase
	{
		public void TestHandlingOfDisposedForm()
		{
			// Arrange
			var testString = "test123";
			handler.SetUpForm(testString);

			// Act / Assert
#if NETFRAMEWORK
			AssertNoExceptionThrown(() => handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle(testString, (UIntPtr)123)));
#else
			AssertNoExceptionThrown(() => handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle(testString, 123)));
#endif
		}

		public void TestFormTitleWithSeparator()
		{
			// Arrange
			var caption = "AAA|||123";
			handler.SetUpForm(caption);

			// Act / Assert
#if NETFRAMEWORK
			AssertEquals(true, handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle(caption, (UIntPtr)123456)));
#else
			AssertEquals(true, handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle(caption, 123456)));
#endif
		}

		public void TestHandlingOfFormWithEmptyTitle()
		{
			// Arrange
			var originalCacheCount = handler.GetDragDropHelperInstance().associatedForms.Count;
			var testString = "test123";
			handler.SetUpForm(testString);

			// Act / Assert
#if NETFRAMEWORK
			AssertNoExceptionThrown(() => handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle("", (UIntPtr)123)));
#else
			AssertNoExceptionThrown(() => handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle("", 123)));
#endif
			AssertEquals("No form should be saved to cache when handling DragStatus", originalCacheCount, handler.GetDragDropHelperInstance().associatedForms.Count);
		}

		public void TestHandlingOfFormWithNonEmptyTitle()
		{
			// Arrange
			var originalCacheCount = handler.GetDragDropHelperInstance().associatedForms.Count;
			var testString = "test123";
			handler.SetUpForm(testString);

			// Act / Assert
#if NETFRAMEWORK
			AssertNoExceptionThrown(() => handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle(testString, (UIntPtr)123)));
#else
			AssertNoExceptionThrown(() => handler.RunHandleBody(WindowCaptionUtils.Base64EncodeCaptionAndHandle(testString, 123)));
#endif
			AssertEquals("Non-empty title form should be saved to cache when handling DragStatus", originalCacheCount + 1, handler.GetDragDropHelperInstance().associatedForms.Count);
		}

		public void TestHandlingOfFormWithNotExistTitle()
		{
			// Arrange
			var originalCacheCount = handler.GetDragDropHelperInstance().associatedForms.Count;
			var testString = "test123";
			var messageSuffix = "|||123";
			handler.SetUpForm(testString);

			// Act / Assert
			AssertNoExceptionThrown(() => handler.RunHandleBody($"NotExist{messageSuffix}"));
			AssertEquals("Non-empty title form that does not exist should be saved to cache when handling DragStatus", originalCacheCount + 1, handler.GetDragDropHelperInstance().associatedForms.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			handler = new DragStatusHandlerMock();
		}

		protected override void TearDown()
		{
			handler.DisposeForms();
			base.TearDown();
		}

		DragStatusHandlerMock handler;
	}

	class DragStatusHandlerMock : DragStatusHandler
	{
		//protected override bool DisposeTestForm()
		//{
		//	DisposeForms();
		//	throw new ObjectDisposedException("Testing disposal of form within the execution of a function");
		//}
		//protected override Form GetActiveForm()
		//{
		//	return null;
		//}

		public override DragDropHelper GetDragDropHelperInstance()
		{
			if (helperInstance == null)
			{
				helperInstance = new DragDropHelperForTest();
			}
			return helperInstance;
		}
		DragDropHelper helperInstance;

		public bool RunHandleBody(string title)
		{
			return HandleBody(title);
		}

		public void DisposeForms()
		{
			foreach (var i in forms.ToArray())
			{
				forms.Remove(i);
				i.Dispose();
			}
		}

		public void SetUpForm(string text)
		{
			var form = new Form
			{
				Text = text
			};
			form.Show();
			forms.Add(form);
		}

		readonly List<Form> forms = new List<Form>();
	}
}
