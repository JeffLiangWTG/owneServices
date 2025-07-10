using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class NonPersistentPackagePivotCollection : NonPersistentBusinessObjectCollection<NonPersistentPackagePivot>
	{
		public NonPersistentPackagePivotCollection(EMCSJobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			this.invoiceLine = invoiceLine;
			if (invoiceLine != null && invoiceLine.Declaration != null && invoiceLine.Declaration.EMCSPackages != null)
			{
				var packages = invoiceLine.Declaration.EMCSPackages;
				packages.CountChanged += new CollectionCountChangedEventHandler(Packages_CountChanged);
				using (SuspendSettingHasChanges())
				{
					foreach (EMCSPackage package in packages)
					{
						AddNewNonPersistentPackage(package);
					}
				}
			}
		}

		readonly EMCSJobComInvoiceLine invoiceLine;

		public NonPersistentPackagePivot AddNewNonPersistentPackage(EMCSPackage package)
		{
			var npPackage = AddNew();
			npPackage.Package = package;
			return npPackage;
		}

		public void RemoveNonPersistentPackage(EMCSPackage package)
		{
			if (!package.IsDeleted)
			{
				foreach (NonPersistentPackagePivot npPackageForDelete in this)
				{
					if (npPackageForDelete.Package == package)
					{
						RemoveAndDelete(npPackageForDelete);
						return;
					}
				}
			}
		}

		protected virtual void Packages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				AddNewNonPersistentPackage((EMCSPackage)e.BizObject);
			}
			else // removed
			{
				RemoveNonPersistentPackage((EMCSPackage)e.BizObject);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NonPersistentPackagePivot(invoiceLine);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		internal void DeleteAll()
		{
			RemoveAll();
		}
	}
}
