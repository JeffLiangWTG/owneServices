using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ExceptionExtensionsTest : NUnit.Framework.TestCase
	{
		public void TestIsGdiPlusException()
		{
			var ex = new TypeInitializationException("Gdip", new ExternalException("A generic error occurred in GDI+."));
			Assert(ex.IsGdiPlusException());
		}

		public void TestGetFullMessage()
		{
			var ex = new TypeInitializationException("Gdip", new ExternalException("A generic error occurred in GDI+."));
			var fullMessage = ex.GetFullMessage();
			AssertEquals("The type initializer for 'Gdip' threw an exception. --> A generic error occurred in GDI+.", fullMessage);
		}

		public void TestIsBadImageFormatExceptionExtension()
		{
			try
			{
				Image.FromFile("Some File That Does Not6 Exist.TIF");
				Fail("What??? No Exception!?!?");
			}
			catch (Exception exception)
			{
				AssertEquals("exception.IsBadImageFormatException() for Non Existant Image File", false, exception.IsBadImageFormatException());
			}

			try
			{
				using (MemoryStream bodgyImageFileStream = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }))
				{
					Image.FromStream(bodgyImageFileStream);
				}
				Fail("What??? No Exception!?!?");
			}
			catch (Exception exception)
			{
				AssertEquals("exception.IsBadImageFormatException() for Bodgy Image Stream", true, exception.IsBadImageFormatException());
			}

			AssertEquals("exception.IsBadImageFormatException() for ArgumentException outside of System.Drawing", false, new ArgumentException().IsBadImageFormatException());

			AssertEquals("exception.IsBadImageFormatException() for ImageFormatException. This method is used to wrap Image construction, not catch the resulting ImageFormatException.", false, new ImageFormatException().IsBadImageFormatException());
		}
	}
}
