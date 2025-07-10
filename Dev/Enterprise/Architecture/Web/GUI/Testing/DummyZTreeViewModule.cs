#if DEBUG
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class DummyZTreeViewModule : ZTreeViewModule
	{
		public DummyZTreeViewModule(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.DummyZTreeView; }
		}

		public override string FormatCode(string unformattedCode)
		{
			return ZString.Empty;
		}

		public override string GetCode(IFamilyMember node)
		{
			return ZString.Empty;
		}

		public override string GetDescription(IFamilyMember node)
		{
			return ZString.Empty;
		}

		public override string GetDescriptionForCode(string code, bool matchExact)
		{
			return ZString.Empty;
		}

		public override bool IsSelectable(IFamilyMember node)
		{
			return false;
		}

		public override string SelectedCode
		{
			get { return fSelectedCode; }
			set { fSelectedCode = value; }
		}
		string fSelectedCode;

		public override IFamilyMember SelectedItem
		{
			get { return null; }
		}

		public override string GetQuantity(IFamilyMember node)
		{
			return ZString.Empty;
		}

		public override TreeNodeCollection TreeCollection
		{
			get
			{
				return new TreeNodeCollection();
			}
		}
	}
}
#endif
