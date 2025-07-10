using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.Modules
{
	/// <summary>
	/// Base class for all tree-view specific modules
	/// </summary>
	public abstract class ZTreeViewModule : ZWebModule
	{
		public ZTreeViewModule(BusinessObjectFactory factory) : base(factory)
		{
		}

		public abstract TreeNodeCollection TreeCollection { get; }
		public abstract string SelectedCode { get; set; }
		public abstract IFamilyMember SelectedItem { get; }

		public abstract bool IsSelectable(IFamilyMember node);
		public abstract string GetCode(IFamilyMember node);
		public abstract string GetDescription(IFamilyMember node);
		public abstract string GetQuantity(IFamilyMember node);
		public abstract string GetDescriptionForCode(string code, bool matchExact);
		public abstract string FormatCode(string unformattedCode);
	}
}
