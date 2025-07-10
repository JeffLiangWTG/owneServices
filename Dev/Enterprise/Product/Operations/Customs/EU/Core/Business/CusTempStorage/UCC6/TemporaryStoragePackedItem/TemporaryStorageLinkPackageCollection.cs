using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStorageLinkPackageCollection<out T> : IBusinessObjectCollection<T>
		where T : TemporaryStorageLinkPackage
	{
		bool HasAtLeastOneLinkedRecord { get; }
	}

	public class TemporaryStorageLinkPackageCollection<T> : NonPersistentBusinessObjectCollection<T>, ITemporaryStorageLinkPackageCollection<T>
		where T : TemporaryStorageLinkPackage
	{
		public TemporaryStorageLinkPackageCollection(TemporaryStoragePackedItem packedItem)
			: base(packedItem.Factory)
		{
			this.packedItem = packedItem;
			this.bill = Argument.NotNull(packedItem.Bill, nameof(bill));
		}
		readonly TemporaryStoragePackedItem packedItem;
		readonly TemporaryStorageBill bill;

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		public override void Load()
		{
			RemoveAll();

			var packages = bill.Packs;
			packages.CountChanged -= SyncLinkPackages;

			foreach (TemporaryStoragePack package in packages)
			{
				AddLinkPackage(package);
			}

			packages.CountChanged += SyncLinkPackages;
		}

		public bool HasAtLeastOneLinkedRecord => Factory.GetValue(ref hasAtLeastOneLinkedRecordCached, () => this.Cast<T>().Any(x => x.IsLinked));
		CachedProperty<bool> hasAtLeastOneLinkedRecordCached;

		void SyncLinkPackages(object sender, EventArgs e)
		{
			Load();
			RefreshBinding();
		}

		void AddLinkPackage(TemporaryStoragePack package)
		{
			var linkPackage = (T)CreateNonPersistentBusinessObject();
			linkPackage.Package = package;

			Add(linkPackage);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => (T)Activator.CreateInstance(typeof(T), packedItem);

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
