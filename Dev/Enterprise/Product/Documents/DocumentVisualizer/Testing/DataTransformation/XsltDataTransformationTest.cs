using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DataTransformation;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestsSubclassesOf(typeof(XsltDataTransformation))]
	abstract class XsltDataTransformationTest : TestCase
	{
		const string TestXmlDataFolder = @"Enterprise\Product\Documents\DocumentVisualizer\Testing\TestData\";

		public void TestTransformation()
		{
			var source = Source;
			var expectedResult = ExpectedResult;
			var expectedNotifications = ExpectedNotifications;

			AssertNotNull("source", source);
			AssertNotNull("expected", expectedResult);

			var transformation = (XsltDataTransformation)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
			var notificationHandler = new NotificationsHandler();
			var result = transformation.Run(source, notificationHandler);

			AssertNotNull("result", result);

			AssertMultilineASCIIEquals("result",
				expectedResult.ToString(),
				result.ToString());

			AssertContainsExactElementsInAnyOrder("notifications",
				expectedNotifications,
				notificationHandler.Notifications.Select(n => n.Message));
		}

		protected abstract XDocument Source { get; }
		protected abstract XDocument ExpectedResult { get; }
		protected abstract IEnumerable<string> ExpectedNotifications { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected XDocument GetDocument(string xmlName)
		{
			var path = Path.Combine(BaseSourcePath, TestXmlDataFolder, xmlName);
			return XDocument.Load(path);
		}
	}
}
