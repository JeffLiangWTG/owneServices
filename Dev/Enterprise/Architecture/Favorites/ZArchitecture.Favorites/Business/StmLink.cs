using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Favorites
{
	public class StmLink : AutoStmLink, IStmLink
	{
		public StmLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool Equals(object obj)
		{
			var link = obj as StmLink;
			if (this.IsDeleted || link == null || link.IsDeleted)
			{
				return false;
			}

			return this.STL_ModuleID == link.STL_ModuleID && this.STL_ItemPK == link.STL_ItemPK;
		}

		public override int GetHashCode()
		{
			return this.IsDeleted ? base.GetHashCode() : STL_ModuleID.GetHashCode() ^ STL_ItemPK.GetHashCode();
		}

		public bool IsModule
		{
			get { return STL_ItemPK.IsEmpty; }
		}

		public string UniqueKey
		{
			get { return STL_ItemPK.IsEmpty ? STL_ModuleID.ToString() : string.Format("{0}{1}", STL_ModuleID, STL_ItemPK); }
		}
	}
}
