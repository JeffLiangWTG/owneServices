using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Client.DFD.Export;
using Enterprise.Client.DFD.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.DFD.GUI.Testing
{
	[TestedType(typeof(DFDFlatFileXmlExportGUIWrapper))]
	public class DFDFlatFileXmlExportGUIWrapperTest : NonPersistentBusinessObjectTestCase //FlatFileXmlExportGUIWrapperTest
	{
		[TestDate(2007, 1, 29, 1, 30, 0)]
		public void TestCreateFileAndExport()
		{
			ZDateTime currentDate = new ZDateTime(2007, 1, 29, 1, 30, 0);
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEBTORTEST";
			debtor.OH_IsDebtor = true;
			debtor.OH_RL_NKClosestPort = "HKHKG";
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = debtor.PK;
			invoice.AH_PostDate = currentDate.AddDays(-2).AddHours(2);
			invoice.AH_OSExTaxAmount = 10m;
			invoice.AH_ConsolidatedInvoiceRef = "S0000001";
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.AH_JH = jobHeader.PK;
			ARAdjustmentNote adjustmentNote = Factory.New<ARAdjustmentNote>();
			adjustmentNote.AH_OH = debtor.PK;
			adjustmentNote.AH_PostDate = currentDate.AddDays(-2).AddHours(3);
			Factory.Save();
			ZString expectedDirectory = Path.Combine(Env.TempPath, "DFD");
			DirectoryInfo directoryPath = new DirectoryInfo(expectedDirectory);
			TransactionsTypesToExportBusinessObject registryBusinessObject = new TransactionsTypesToExportBusinessObject();
			registryBusinessObject.ARAdjustmentNote = true;
			registryBusinessObject.ARCreditNote = true;
			registryBusinessObject.ARInvoice = true;
			registryBusinessObject.ARJobRelated = true;
			registryBusinessObject.ARNonJobRelated = true;
			DFDDataRegistry.Instance.ARTransactionsTypesToExport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryBusinessObject);
			try
			{
				Wrapper.CreateFileAndExport(Env.TempPath);
				Assert(Directory.Exists(expectedDirectory));
				directoryPath = new DirectoryInfo(Env.TempPath);
				AssertEquals(2, directoryPath.GetFiles().Length);
				AssertEquals("DEBTORTEST-HKHKG-ADJ-00001000-20070127-20070129-01-30.xml", directoryPath.GetFiles()[0].Name);
				AssertEquals("DEBTORTEST-HKHKG-INV-00001000-S0000001-20070127-20070129-01-30.xml", directoryPath.GetFiles()[1].Name);
				Assert("File should be exported", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Batch 0 was exported successfully."));
			}
			finally
			{
				foreach (FileInfo fileInfo in directoryPath.GetFiles())
				{
					DeleteIfExists(fileInfo.FullName);
				}

				Directory.Delete(expectedDirectory);
			}

			invoice = Factory.Load<ARInvoice>(invoice.PK);
			AssertEquals("DEX event with reference should be created", 1, invoice.Logs.Find(l => l.SL_SE_NKEvent == ZArchitecture.Business.Events.DataExport.Code && l.SL_Reference == "Transaction has been exported 29-Jan-07").Count());
		}

		public void TestIsOKToExport()
		{
			Assert(Wrapper.IsOKToExport());
		}

		public void TestDataExporter()
		{
			AssertEquals(typeof(DFDXmlARTransactionsExporter), Wrapper.DataExporter.GetType());
		}

		public void TestInitialDirectory()
		{
			AssertEquals("InitialDirectory", Wrapper.InitialDirectory, DFDDataRegistry.Instance.ARTransactionsExportDirectory);
		}

		#region Implementation
		readonly ZDateTime CurrentDate = new ZDateTime(2007, 1, 29, 1, 30, 0);
		AdditionalSettingsRegistryBusinessObject currentSettings;
		protected override void SetUp()
		{
			base.SetUp();
			currentSettings = new AdditionalSettingsRegistryBusinessObject(Factory);
			currentSettings.Directory = Env.TempPath;
			currentSettings.Interval = 1;
			currentSettings.LastRunDateTime = CurrentDate.AddDays(-2);
			currentSettings.NextRunDateTime = CurrentDate.AddDays(-1);
			currentSettings.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			currentSettings.ExportFileName = "ARTest";
			DFDDataRegistry.Instance.ARTransactionsExportItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentSettings);
		}

		DFDFlatFileXmlExportGUIWrapperForTest Wrapper
		{
			get
			{
				return wrapper ?? (wrapper = new DFDFlatFileXmlExportGUIWrapperForTest(Factory));
			}
		}

		DFDFlatFileXmlExportGUIWrapperForTest wrapper;
		class DFDFlatFileXmlExportGUIWrapperForTest : DFDFlatFileXmlExportGUIWrapper
		{
			public DFDFlatFileXmlExportGUIWrapperForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new void CreateFileAndExport(ZString selectedPath)
			{
				base.CreateFileAndExport(selectedPath);
			}

			public new bool IsOKToExport()
			{
				return base.IsOKToExport();
			}

			public new string InitialDirectory
			{
				get
				{
					return base.InitialDirectory;
				}
			}

			public new string FileName
			{
				get
				{
					return base.FileName;
				}
			}

			public new AccountingTransactionsDataExporter DataExporter
			{
				get
				{
					return base.DataExporter;
				}
			}
		}
		#endregion
	}
}
