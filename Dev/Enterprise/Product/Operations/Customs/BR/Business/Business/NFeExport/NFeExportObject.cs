using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class NFeExportObject : NonPersistentBusinessObject
	{
		public NFeExportObject(JobDeclaration declaration) : base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		public readonly JobDeclaration Declaration;

		#region NFeEntryExportObjectCollection

		[ChildEditable(true)]
		public NFeEntryExportObjectCollection Entries
		{
			get
			{
				if (fNFeEntryExportObjects == null)
				{
					fNFeEntryExportObjects = new NFeEntryExportObjectCollection(Declaration);
					RegisterEditableChildObject(fNFeEntryExportObjects);
					fNFeEntryExportObjects.Load();
				}
				return fNFeEntryExportObjects;
			}
		}

		NFeEntryExportObjectCollection fNFeEntryExportObjects;

		#endregion

		public new void Refresh()
		{
			Entries.Load();
		}
	}
}
