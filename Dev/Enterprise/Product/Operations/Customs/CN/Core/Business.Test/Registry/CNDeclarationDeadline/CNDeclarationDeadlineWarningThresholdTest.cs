using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDeclarationDeadlineWarningThreshold))]
	class CNDeclarationDeadlineWarningThresholdTest : RegistryBusinessObjectTemplateTestCase<CNDeclarationDeadlineWarningThreshold>
	{
		public void TestTransportTypeListWithAll()
		{
			var collection1 = new CNDeclarationDeadlineWarningThresholdCollection();
			var threshold = collection1.AddNew();
			CombineAssertions(() =>
			{
				var transportTypeListWithAll = threshold.TransportTypeListWithAll;
				AssertSame("Cached", transportTypeListWithAll, threshold.TransportTypeListWithAll);
				AssertEquals("TransportTypeListWithAll", 8, transportTypeListWithAll.Count);
				Assert("TransportType have 'All' value", transportTypeListWithAll.ContainsCode(CNDeclarationDeadlineWarningThreshold.ALL));
			});
		}

		public void TestFirstLevelWarningColor()
		{
			var collection1 = new CNDeclarationDeadlineWarningThresholdCollection();
			var threshold = collection1.AddNew();
			threshold.FirstLevelWarningColor = "abc123897";
			AssertEquals("abc,123,897", threshold.FirstLevelWarningColor);
		}

		public void TestSecondLevelWarningColor()
		{
			var collection1 = new CNDeclarationDeadlineWarningThresholdCollection();
			var threshold = collection1.AddNew();
			threshold.SecondLevelWarningColor = "abc123897";
			AssertEquals("abc,123,897", threshold.SecondLevelWarningColor);

			threshold.ThirdLevelWarningColor = "abc123897";
			AssertEquals("abc,123,897", threshold.ThirdLevelWarningColor);

			threshold.DelayedWarningColor = "255255255";
			AssertEquals("255,255,255", threshold.DelayedWarningColor);
		}

		public void TestThirdLevelWarningColor()
		{
			var collection1 = new CNDeclarationDeadlineWarningThresholdCollection();
			var threshold = collection1.AddNew();
			threshold.ThirdLevelWarningColor = "abc123897";
			AssertEquals("abc,123,897", threshold.ThirdLevelWarningColor);
		}

		public void TestDelayedWarningColor()
		{
			var collection1 = new CNDeclarationDeadlineWarningThresholdCollection();
			var threshold = collection1.AddNew();
			threshold.DelayedWarningColor = "255255255";
			AssertEquals("255,255,255", threshold.DelayedWarningColor);
		}

		public void TestCorrectSerialization()
		{
			var item = new CNDeclarationDeadlineWarningThresholdForTest(Factory)
			{
				TransportMode = CNDeclarationDeadlineWarningThreshold.ALL,
				FirstLevelThreshold = 0,
				FirstLevelWarningColor = "255000000",
				SecondLevelThreshold = 3,
				SecondLevelWarningColor = "255160122",
				ThirdLevelThreshold = 7,
				ThirdLevelWarningColor = "255255224",
				DelayedWarningColor = "255255224"
			};

			AssertNoExceptionThrown("XML should parse on both import and export", () =>
			{
				using (var stream = new MemoryStream())
				{
					using (var writer = XmlWriter.Create(stream))
					{
						item.WriteElementsForTest(writer);
						writer.Flush();
						writer.Close();
					}

					stream.Position = 0;
					item.ReadElementsForTest(new XmlReaderWrapper(XmlReader.Create(stream)));
				}
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new CNDeclarationDeadlineWarningThresholdCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override CNDeclarationDeadlineWarningThreshold GetBusinessObjectToClone() => (CNDeclarationDeadlineWarningThreshold)GetNewBusinessObject();

		protected override CNDeclarationDeadlineWarningThreshold GetBusinessObjectToSerialise() => (CNDeclarationDeadlineWarningThreshold)GetNewBusinessObject();
	}
}
