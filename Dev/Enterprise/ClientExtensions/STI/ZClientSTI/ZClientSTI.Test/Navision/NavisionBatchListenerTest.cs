using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class NavisionBatchListenerTest : NavisionBatchListenerTestCase
	{
		public void TestAdditionalMatching()
		{
			NavisionBatchListenerTestClass batchListener = new NavisionBatchListenerTestClass();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			StmALog log = org.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			AssertEquals("Additional Matching should be false", false, batchListener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("Additional Matching should be true", true, batchListener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();
			AssertEquals("Additional Matching should be true", true, batchListener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.DataExport);
			Factory.Save();
			AssertEquals("Additional Matching should be false, Data Export event created after edit and add events", false, batchListener.AdditionalMatching(log));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			log = org.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("Additional Matching should be true, Data export event is before the last edit event", true, batchListener.AdditionalMatching(log));
		}

		[TestDate(2006, 3, 24, 10, 59, 48)]
		public void TestFileName()
		{
			NavisionBatchListenerTestClass batchListener = new NavisionBatchListenerTestClass();
			AssertEquals("File name was not correctly set", Path.Combine(Env.TempPath, "TST20060324591048.csv"), batchListener.FileNameExposed);
		}

		[TestDate(2006, 6, 14, 15, 58, 28)]
		public void TestObjectsShouldNotBeCachedAndExportedMoreThanOnce()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			org1.OH_FullName = "Testing Organisation1";
			org1.MainAddress.OA_Address1 = "2 Doody Street";
			org1.MainWebURL.PU_URL = "http://www.testing.com.au";
			org1.OH_Code = "TSTORG1";
			OrgHeader org2 = NavisionTestHelper.OrgForTesting(Factory);
			string fileName = Path.Combine(Env.TempPath, FileNamePreFix + "20060614581528.csv");
			NavisionBatchListenerTestClass batchListener = new NavisionBatchListenerTestClass();
			try
			{
				((IBatchListenerTestClass)batchListener).Process(org1, null, new NotificationBuffer());
				string expectedString = "TSTORG1,Testing Organisation1,Testing Organisation1,,2 Doody Street,,,,,,,0.00,,,COD0,,FOB,,,,,TSTORG1,,,,,,,http://www.testing.com.au,No GST,,,\r\n";
				AssertFileSameAsString(fileName, expectedString);
				((IBatchListenerTestClass)batchListener).Process(org2, null, new NotificationBuffer());
				expectedString += "EAGDATSYD,Eagle Datamation International,Eagle Datamation International,,Level 3,184 Bourke Road,Alexandria,,+61290251100,BNE,,100000.00,ASC,,INV30,,CIF,,AU,,,NYKLIN,,61 2 9025 1199,,2015,NSW,support@cargowise.com,http://www.cargowise.com,GST,,41 065 894 724,\r\n";
				AssertFileSameAsString(fileName, expectedString);
			}
			finally
			{
				DeleteIfExists(fileName);
			}
		}
		#region IBatchListenerTestClass
		public interface IBatchListenerTestClass
		{
			void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications);
			NavisionFlatFileExporter ExporterExposed { get; }
			Type BusinessObjectCollectionTypeExposed { get; }
		}
		#endregion

		#region Overrides
		protected override Type BusinessObjectCollectionType
		{
			get
			{
				return typeof(OrgHeaderCollection);
			}
		}

		protected override string BusinessObjectTableName
		{
			get
			{
				return OrgHeaderSchema.Constants.TableName;
			}
		}

		protected override Type BusinessObjectType
		{
			get
			{
				return typeof(OrgHeader);
			}
		}

		protected override string ExportDirectory
		{
			get
			{
				return Env.TempPath;
			}

			set
			{
				_ = value;
			}
		}

		protected override Type ExporterType
		{
			get
			{
				return typeof(NavisionFlatFileExporterTestClass);
			}
		}

		protected override string FileNamePreFix
		{
			get
			{
				return "TST";
			}
		}

		protected override NavisionBatchListener BatchListener
		{
			get
			{
				return new NavisionBatchListenerTestClass();
			}
		}

		protected override BusinessObject BusinessObjectForTesting()
		{
			return NavisionTestHelper.OrgForTesting(Factory);
		}

		#endregion
		#region NavisionBatchListenerTestClass
		class NavisionBatchListenerTestClass : NavisionBatchListener, IBatchListenerTestClass
		{
			public NavisionBatchListenerTestClass() : base(ZDateTime.Now)
			{
			}

			protected override Type BusinessObjectCollectionType
			{
				get
				{
					return typeof(OrgHeaderCollection);
				}
			}

			public Type BusinessObjectCollectionTypeExposed
			{
				get
				{
					return BusinessObjectCollectionType;
				}
			}

			protected override NavisionFlatFileExporter Exporter
			{
				get
				{
					if (fExporter == null)
					{
						fExporter = new NavisionFlatFileExporterTestClass(Factory, Instructions, FileName);
					}

					return fExporter;
				}
			}

			NavisionFlatFileExporter fExporter;
			public NavisionFlatFileExporter ExporterExposed
			{
				get
				{
					return Exporter;
				}
			}

			public override string BusinessObjectTableName
			{
				get
				{
					return OrgHeaderSchema.Constants.TableName;
				}
			}

			public override Type BusinessObjectType
			{
				get
				{
					return typeof(OrgHeader);
				}
			}

			public new bool AdditionalMatching(StmALog log)
			{
				return base.AdditionalMatching(log);
			}

			public new void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
			{
				base.Process(matchingBusinessObject, log, notifications);
			}

			protected override ZString ExportDirectory
			{
				get
				{
					return Env.TempPath;
				}
			}

			protected override ZString FileNamePreFix
			{
				get
				{
					return "TST";
				}
			}

			public ZString FileNameExposed
			{
				get
				{
					return base.FileName;
				}
			}
		}
		#endregion
	}
}
