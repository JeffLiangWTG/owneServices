using System;

using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DataMapping
{
	public abstract class ImportExportMappingCollection<TMapping, TWizard> : NonPersistentBusinessObjectCollection<TMapping>
		where TMapping : ImportExportMapping<TMapping, TWizard>
		where TWizard : ImportExportWizard
	{
		public ImportExportMappingCollection(TWizard wizard)
			: base(wizard.Factory)
		{
			Parent = wizard;
		}

		public override void Load()
		{
			foreach (IImportPropertyInfo property in Parent.CollectionInfoProperties)
			{
				TMapping mapping = (TMapping)Activator.CreateInstance(typeof(TMapping), property, this);
				Add(mapping);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Creating of new elements is not allowed.");
		}

		public TWizard Parent { get; private set; }

		public new TMapping this[int index]
		{
			get { return (TMapping)Elements[index]; }
		}
	}
}
