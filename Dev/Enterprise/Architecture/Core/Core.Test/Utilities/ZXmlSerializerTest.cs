using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Xml.ZXmlSerializer;

namespace Enterprise.ZArchitecture.Xml.Testing
{
	sealed class ZXmlSerializerTest : TestCase
	{
		public void TestSerializerIsCached()
		{
			ZXmlSerializer.Cache.Clear();

			AssertEquals("The same ZXmlSerializer should be returned.",
				ZXmlSerializer.New(typeof(string)).XmlSerializerInternal,
				ZXmlSerializer.New(typeof(string)).XmlSerializerInternal);
			AssertEquals("ZXmlSerializerHashtable.Count", 1, ZXmlSerializer.Cache.Count);

			Assert("A different ZXmlSerializer should be returned.",
				ZXmlSerializer.New(typeof(string)).XmlSerializerInternal !=
				ZXmlSerializer.New(typeof(int)).XmlSerializerInternal);
			AssertEquals("ZXmlSerializerHashtable.Count", 2, ZXmlSerializer.Cache.Count);

			ZXmlSerializer.Cache.Clear();
			ErrorReporter.Clear();
		}

#if NETFRAMEWORK
		[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
#endif
		public void TestCheckIsSgenAssembly()
		{
			ErrorReporter.Clear();
			ZXmlSerializer.Cache.Clear();

			AssertNotNull(ZXmlSerializer.New(Assembly.Load("Enterprise.DataTransfer").GetType("Enterprise.DataTransfer.Xml.XsdVersion1.XmlInterchange")).XmlSerializerInternal);
			AssertEquals("", ErrorReporter.LastMessageReported);

			AssertNotNull(ZXmlSerializer.New(typeof(TestTypeWithXmlRoot)).XmlSerializerInternal);
			AssertNotEquals("", ErrorReporter.LastMessageReported);
			AssertEquals("NoSgen.Enterprise.ZArchitecture.Xml.ZXmlSerializer+TestTypeWithXmlRoot", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestRetryNew_WithinRetryLimit()
		{
			Cache.Clear();
			Exception ex = new Exception();
			exceptionsToThrow = new Queue<Exception>();
			try
			{
				exceptionsToThrow.Enqueue(ex);
				exceptionsToThrow.Enqueue(ex);
				AssertNotNull("A ZXmlSerializer should be created.", ZXmlSerializer.New(typeof(string)));
			}
			finally
			{
				exceptionsToThrow = null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is just an error description - not a hard-coded path")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "This is just an error description - not a hard-coded path")]
		public void TestRetryNew_ExceedRetryLimit()
		{
			Cache.Clear();
			ExternalException ex = new ExternalException("Cannot execute a program. The command being executed was \"C:\\WINDOWS\\Microsoft.NET\\Framework\\v2.0.50727\\csc.exe\" /noconfig /fullpaths @\"C:\\Documents and Settings\\Owner\\Local Settings\\Temp\\cufuqb-a.cmdline\".", 5); // this is just an error description - not a hard-coded path
			exceptionsToThrow = new Queue<Exception>();
			try
			{
				exceptionsToThrow.Enqueue(ex);
				exceptionsToThrow.Enqueue(ex);
				exceptionsToThrow.Enqueue(ex);
				AssertExceptionThrown(typeof(ZXmlSerializerCompilationException), delegate
				{ CreateDotNetXmlSerializer(ZXmlSerializer.New(typeof(string))); });
			}
			finally
			{
				exceptionsToThrow = null;
			}

			if (ErrorReporter.LastMessageReported.Contains("Caught exception trying to execute delegate ") &&
				ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestRetryNewExceededRetryLimitOtherException()
		{
			Cache.Clear();
			OdysseyException ex = new OdysseyException("An error has occurred.");
			exceptionsToThrow = new Queue<Exception>();
			try
			{
				exceptionsToThrow.Enqueue(ex);
				exceptionsToThrow.Enqueue(ex);
				exceptionsToThrow.Enqueue(ex);
				AssertExceptionThrown(typeof(OdysseyException), delegate
				{ CreateDotNetXmlSerializer(ZXmlSerializer.New(typeof(string))); });
			}
			finally
			{
				exceptionsToThrow = null;
			}

			if (ErrorReporter.LastMessageReported.Contains("Caught exception trying to execute delegate ") &&
				ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		XmlSerializer CreateDotNetXmlSerializer(ZXmlSerializer xmlSerializer)
		{
			return xmlSerializer.XmlSerializerInternal;
		}
	}
}
