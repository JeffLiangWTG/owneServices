using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ExceptionExtenderTest : TestCase
	{
		public void TestIsDiskFull()
		{
			IOException ex1 = new IOException("", -2147024784);
			Assert(ex1.IsDiskFull());

			IOException ex2 = new IOException("", -214702478);
			Assert(!ex2.IsDiskFull());
		}

		public void TestIsFileAlreadyExists()
		{
			IOException ex1 = new IOException("", -2146232060);
			Assert(ex1.IsFileAlreadyExists());

			IOException ex2 = new IOException("", -214623206);
			Assert(!ex2.IsFileAlreadyExists());
		}

		public void TestIsPaginFileTooSmall()
		{
			FileLoadException ex1 = new FileLoadException("", new IOException("", -2147023441));
			Assert(ex1.IsPagingFileTooSmall());

			FileLoadException ex2 = new FileLoadException("", new IOException("", -2147023414));
			Assert(!ex2.IsPagingFileTooSmall());
		}

		public void TestIsHResultIncludingInner()
		{
			Assert(!new IOException("", 1001).IsHResultIncludingInner(1000));
			Assert(new IOException("", 1000).IsHResultIncludingInner(1000));
			Assert(!new Exception("", new Exception()).IsHResultIncludingInner(1000));
			Assert(new Exception("", new IOException("", 1000)).IsHResultIncludingInner(1000));
		}

		public void TestGetInnermostException()
		{
			var innerEx = new Exception("I am an inner exception");
			var ex = new Exception("I am an exception", innerEx);
			AssertEquals(innerEx, ex.GetInnermostException());
			AssertEquals(innerEx, innerEx.GetInnermostException());
		}

		public void TestGetFirstOccurenceOfInnermostException()
		{
			var innerInnerEx = new InvalidOperationException("I am an inner inner exception");
			var innerEx = new InvalidOperationException("I am an inner exception", innerInnerEx);
			var ex = new Exception("I am an exception", innerEx);

			AssertEquals(innerEx, ex.GetFirstOccurrenceOfException<InvalidOperationException>());
			AssertEquals(innerInnerEx, ex.GetFirstOccurrenceOfException<InvalidOperationException>("I am an inner inner"));
			AssertEquals(innerInnerEx, innerInnerEx.GetFirstOccurrenceOfException<InvalidOperationException>());
			AssertEquals(innerInnerEx, innerInnerEx.GetFirstOccurrenceOfException<InvalidOperationException>("I am an inner inner"));
		}

		public void TestIsInnermostExceptionPresent()
		{
			var innerInnerEx = new InvalidOperationException("I am an inner inner exception");
			var innerEx = new InvalidOperationException("I am an inner exception", innerInnerEx);
			var ex = new Exception("I am an exception", innerEx);

			Assert(ex.IsExceptionPresentIncludingInner<InvalidOperationException>());
			Assert(ex.IsExceptionPresentIncludingInner<InvalidOperationException>("I am an inner inner"));
			Assert(innerInnerEx.IsExceptionPresentIncludingInner<InvalidOperationException>());
			Assert(innerInnerEx.IsExceptionPresentIncludingInner<InvalidOperationException>("I am an inner inner"));
			Assert(!ex.IsExceptionPresentIncludingInner<NullReferenceException>());
			Assert(!ex.IsExceptionPresentIncludingInner<InvalidOperationException>("blah"));
		}

		public void TestCheckAllInnerExceptions()
		{
			var innerInnerEx = new InvalidOperationException("I am an inner inner exception");
			var innerEx = new InvalidOperationException("I am an inner exception", innerInnerEx);
			var ex = new Exception("I am an exception", innerEx);

			Assert(ex.IsExceptionPresentIncludingInner<InvalidOperationException>(s => s.Message == "I am an inner inner exception"));
		}

		public void TestDatabaseUpgradedExceptionIsCritical()
		{
			var ex = new DatabaseUpgradedException();
			Assert(ex.IsCriticalException());
		}
	}
}
