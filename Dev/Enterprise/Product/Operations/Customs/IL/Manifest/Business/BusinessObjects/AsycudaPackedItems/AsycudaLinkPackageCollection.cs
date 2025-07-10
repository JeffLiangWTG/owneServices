using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public interface IAsycudaLinkPackageCollection<out T> : IBusinessObjectCollection<T>
		where T : AsycudaLinkPackage
	{
		bool HasAtLeastOneLinkedRecord { get; }
		bool HasMoreThanOneLinkedRecord { get; }
	}

	public class AsycudaLinkPackageCollection<T> : NonPersistentBusinessObjectCollection<T>, IAsycudaLinkPackageCollection<T>
		where T : AsycudaLinkPackage
	{
		public AsycudaLinkPackageCollection(AsycudaPackedItem packedItem)
			: base(packedItem.Factory)
		{
			this.packedItem = packedItem;
			this.bill = Argument.NotNull(packedItem.Bill, nameof(bill));
		}

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		public override void Load()
		{
			RemoveAll();

			var packages = bill.Packs;
			packages.CountChanged -= SyncLinkPackages;

			foreach (AsycudaPack package in packages)
			{
				AddLinkPackage(package);
			}
			packages.CountChanged += SyncLinkPackages;
		}

		public bool HasAtLeastOneLinkedRecord => Factory.GetValue(ref hasAtLeastOneLinkedRecordCached, () => this.Cast<T>().Any(x => x.IsLinked));
		CachedProperty<bool> hasAtLeastOneLinkedRecordCached;

		public bool HasMoreThanOneLinkedRecord => Factory.GetValue(ref hasMoreThanOneLinkedRecordCached, () => this.Cast<T>().Count(x => x.IsLinked) > 1);
		CachedProperty<bool> hasMoreThanOneLinkedRecordCached;

		protected override BusinessObject CreateNonPersistentBusinessObject() => (T)Activator.CreateInstance(typeof(T), packedItem);

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		void SyncLinkPackages(object sender, EventArgs e)
		{
			Load();
			RefreshBinding();
		}

		void AddLinkPackage(AsycudaPack package)
		{
			var linkPackage = (T)CreateNonPersistentBusinessObject();
			linkPackage.Package = package;

			Add(linkPackage);
		}

		readonly AsycudaPackedItem packedItem;
		readonly AsycudaBill bill;
	}
}
