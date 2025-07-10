using System;
using CargoWise.Types;
using Enterprise.Client.DFD.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Testing
{
	[TestedType(typeof(DFDDataRegistry))]
	class DFDDataRegistryTest : RegistryItemSetTestCaseWithFactory<DFDDataRegistry>
	{
		#region AR Transactions Export

		public void TestARTransactionsTypesToExport()
		{
			AssertEquals("ARTransactionsTypesToExport", ItemSet.ARTransactionsTypesToExport.Name);
			AssertEquals("DFD Extensions/Export/AR Transactions", ItemSet.ARTransactionsTypesToExport.Category);
			AssertEquals("Transactions Types To Include", ItemSet.ARTransactionsTypesToExport.Caption);
			AssertEquals("Determine what types of AR transaction will be exported", ItemSet.ARTransactionsTypesToExport.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ARTransactionsTypesToExport.Storage);

			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARAdjustmentNote);
			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARCreditNote);
			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARInvoice);
			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARJobRelated);
			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARNonJobRelated);

			ItemSet.ARTransactionsTypesToExport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ARExportRegistryTypes);

			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARAdjustmentNote);
			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARCreditNote);
			Assert(ItemSet.ARTransactionsTypesToExport.Value.ARInvoice);
			Assert(ItemSet.ARTransactionsTypesToExport.Value.ARJobRelated);
			Assert(!ItemSet.ARTransactionsTypesToExport.Value.ARNonJobRelated);
		}

		#endregion

		#region Setup
		AdditionalSettingsRegistryBusinessObject ARExportRegistrySettings;
		TransactionsTypesToExportBusinessObject ARExportRegistryTypes;

		protected override void SetUp()
		{
			ARExportRegistrySettings = new AdditionalSettingsRegistryBusinessObject(Factory);
			ARExportRegistrySettings.Directory = Env.TempPath;
			ARExportRegistrySettings.Interval = 2;
			ARExportRegistrySettings.IntervalType = "DAYS";
			ARExportRegistrySettings.NextRunDateTime = new ZDateTime(2007, 01, 29, 01, 30, 00);
			ARExportRegistrySettings.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			ARExportRegistrySettings.ExportFileName = "ARTest";

			ARExportRegistryTypes = new TransactionsTypesToExportBusinessObject(Factory);
			ARExportRegistryTypes.ARInvoice = true;
			ARExportRegistryTypes.ARJobRelated = true;

			base.SetUp();
		}

		#endregion
	}
}
