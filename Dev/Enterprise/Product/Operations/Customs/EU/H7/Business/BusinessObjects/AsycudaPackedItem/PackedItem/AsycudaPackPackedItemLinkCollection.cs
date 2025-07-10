using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public interface IAsycudaPackPackedItemLinkCollection<out T> : IBusinessObjectCollection<T>
		where T : AsycudaPackPackedItemLink
	{
		bool HasAtLeastOneLinkedRecord { get; }
	}

	public class AsycudaPackPackedItemLinkCollection<T> : NonPersistentBusinessObjectCollection<T>, IAsycudaPackPackedItemLinkCollection<T>
		where T : AsycudaPackPackedItemLink
	{
		public AsycudaPackPackedItemLinkCollection(AsycudaPackedItem packedItem)
			: base(packedItem.Factory)
		{
			this.packedItem = packedItem;
			bill = Argument.NotNull(packedItem.Bill, nameof(bill));
		}
		readonly AsycudaPackedItem packedItem;
		readonly AsycudaBill bill;

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		public override void Load()
		{
			RemoveAll();

			var packages = (BusinessObjectCollection)bill.Packs;
			packages.CountChanged -= SyncLinkPackages;
			foreach (AsycudaPack package in packages)
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

		void AddLinkPackage(AsycudaPack package)
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
