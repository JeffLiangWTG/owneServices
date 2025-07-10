using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	public abstract class NavisionBatchListenerTestCase : TestCaseWithFactory
	{
		protected abstract NavisionBatchListener BatchListener { get; }

		protected abstract Type BusinessObjectCollectionType { get; }

		protected abstract Type BusinessObjectType { get; }

		protected abstract Type ExporterType { get; }

		protected abstract string BusinessObjectTableName { get; }

		protected abstract string FileNamePreFix { get; }

		protected abstract string ExportDirectory { get; set; }

		protected abstract BusinessObject BusinessObjectForTesting();
		public void TestListener()
		{
			NavisionBatchListener listener = BatchListener;
			NavisionFlatFileExporter exporter = ((NavisionBatchListenerTest.IBatchListenerTestClass)listener).ExporterExposed;
			if (exporter != null)
			{
				AssertSame("Exporter was not lazy loaded", exporter, ((NavisionBatchListenerTest.IBatchListenerTestClass)listener).ExporterExposed);
				AssertEquals("Exporter should be a " + ExporterType, ExporterType, exporter.GetType());
			}

			if (BusinessObjectCollectionIsSupported)
			{
				AssertEquals("Business Object Collection should be " + BusinessObjectCollectionType, BusinessObjectCollectionType, ((NavisionBatchListenerTest.IBatchListenerTestClass)listener).BusinessObjectCollectionTypeExposed);
			}

			AssertEquals("Business Object Type should be " + BusinessObjectType, BusinessObjectType, listener.BusinessObjectType);
			AssertEquals("Business Object Table should be " + BusinessObjectTableName, BusinessObjectTableName, listener.BusinessObjectTableName);
		}

		[TestDate(2006, 2, 16, 13, 39, 39)]
		public void TestExport()
		{
			NavisionBatchListener listener = BatchListener;
			ExportDirectory = Env.TempPath;
			string fileName = Path.Combine(ExportDirectory, FileNamePreFix + "20060216391339.csv");
			NotificationBuffer notify = new NotificationBuffer();
			BusinessObject objectToBeExported = BusinessObjectForTesting();
			Factory.Save();
			try
			{
				((NavisionBatchListenerTest.IBatchListenerTestClass)listener).Process(objectToBeExported, null, notify);
				Assert("File " + fileName + " should exist", File.Exists(fileName));
			}
			finally
			{
				DeleteIfExists(fileName);
			}
		}

		protected virtual bool BusinessObjectCollectionIsSupported
		{
			get
			{
				return true;
			}
		}
	}
}
