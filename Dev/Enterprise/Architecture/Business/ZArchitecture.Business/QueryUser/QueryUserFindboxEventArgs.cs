using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	public class QueryUserFindboxEventArgs :
		NonPersistentBusinessObject,
		IQueryUserEventArgs,
		IObsoleteValidation
	{
		public QueryUserFindboxEventArgs(ModuleIdentifier moduleID, IBusinessObjectCollection list)
		{
			this.ModuleID = moduleID;
			this.List = list;
		}

		public readonly ModuleIdentifier ModuleID;

		[BusinessObjectTestExclude]
		public IBusinessObjectCollection List { get; set; }

		[List("List")]
		public ZGuid SelectedItemPK
		{
			get { return fSelectedItemPK; }
			set { SetNonPersistentPropertyValue(SelectedItemPKInfo, ref fSelectedItemPK, value); }
		}

		public ZPropertyInfo SelectedItemPKInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedItemPK)); }
		}

		ZGuid fSelectedItemPK;
	}
}
