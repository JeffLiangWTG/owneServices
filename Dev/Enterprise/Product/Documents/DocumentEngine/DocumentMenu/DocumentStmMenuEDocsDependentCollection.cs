using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine
{
	/// <summary>
	/// Summary description for DocumentStmMenuEDocsCollection.
	/// </summary>
	public class DocumentStmMenuEDocsDependentCollection : DependentBusinessObjectCollection<DocumentStmMenuEDocs, StmMenuItemBase>
	{
		public DocumentStmMenuEDocsDependentCollection(StmMenuItemBase parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public DocumentStmMenuEDocs this[string docType]
		{
			get
			{
				foreach (DocumentStmMenuEDocs menu in Elements)
				{
					if (menu.DocType != null && menu.DocType.RT_DocType == docType)
					{
						return menu;
					}
				}
				return null;
			}
		}
	}
}
