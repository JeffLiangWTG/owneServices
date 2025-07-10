using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegLine : EU.TemporaryStorage.Business.CusTempStorageRegLine, Integration.Customs.FR.ICusTempStorageRegLine
	{
		public CusTempStorageRegLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZInt SRL_PackagesRemaining
		{
			get => base.SRL_PackagesRemaining;
			set
			{
				var oldValue = SRL_PackagesRemaining;
				base.SRL_PackagesRemaining = value;
				if (!IsCopying && oldValue != SRL_PackagesRemaining)
				{
					RegHeader?.UpdateStatus();
				}
			}
		}

		public new CusTempStorageRegHeader RegHeader => (CusTempStorageRegHeader)base.RegHeader;

		public new CusTempStorageRegLineLookups Lookups => (CusTempStorageRegLineLookups)base.Lookups;

		public new CusTempStorageRegLineTransactionCollection CusTempStorageRegLineTransactions => (CusTempStorageRegLineTransactionCollection)base.CusTempStorageRegLineTransactions;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups GetNewLookups() => new CusTempStorageRegLineLookups(this);

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionCollection CreateNewCusTempStorageRegLineTransactions()
		{
			var transactions = new CusTempStorageRegLineTransactionCollection(this);
			transactions.CountChanged += (sender, e) => UpdatePackagesRemaining();
			return transactions;
		}

		protected override Type GetStorageRegLineTransactionCore() => typeof(CusTempStorageRegLineTransaction);
	}
}
