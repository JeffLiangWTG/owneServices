using System;
using CargoWise.Types;
using Enterprise.Client.DFD.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DFD
{
	sealed class DFDDataRegistry : RegistryItemSet
	{
		DFDDataRegistry()
		{
		}

		#region Instance

		public static DFDDataRegistry Instance
		{
			get { return instance ?? (instance = new DFDDataRegistry()); }
		}
		[ThreadStatic]
		static DFDDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "DFD Extensions";
		const string Export = Category + "/Export";
		const string ARTransactions = Export + "/AR Transactions";

		#region ARTransactions Export

		public TransactionsTypesToExportRegistryItem ARTransactionsTypesToExport
		{
			get
			{
				return GetItem("ARTransactionsTypesToExport", delegate
				{
					return new TransactionsTypesToExportRegistryItem(
						"ARTransactionsTypesToExport",
						ARTransactions,
						"Transactions Types To Include",
						"Determine what types of AR transaction will be exported",
						RegistryStorageFlags.System);
				});
			}
		}

		public AdditionalSettingsRegistryItem ARTransactionsExportItem
		{
			get
			{
				return GetItem("ARTransactionsExportItem", delegate
				{
					return new AdditionalSettingsRegistryItem(
						"ARTransactionsExportItem",
						ARTransactions,
						"Automatic Export Settings",
						"Please fill in all the fields provided below.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public ZString ARTransactionsExportDirectory
		{
			get { return ARTransactionsExportItem.Value.Directory; }
		}

		public ZGuid ARTransactionsExportNotifyGroupPK
		{
			get { return ARTransactionsExportItem.Value.GroupPK; }
		}

		#endregion
	}
}
