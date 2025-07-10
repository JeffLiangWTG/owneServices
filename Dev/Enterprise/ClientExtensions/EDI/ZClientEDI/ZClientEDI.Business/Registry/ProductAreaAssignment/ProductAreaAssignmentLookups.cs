using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ProductAreaAssignmentLookups : ZLookups
	{
		public ProductAreaAssignmentLookups(ProductAreaAssignment parent)
			: base(parent) { }

		#region Implementation

		protected new ProductAreaAssignment Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ProductAreaAssignment)base.Parent; }
		}

		#endregion

		protected override BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		public CodeDescriptionPairList ProductAreaList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(EDIDataRegistry.Instance.ProductAreas.Value);
				list.AddPair(BlankProductAreaCode, "Blank Product Area");
				return list;
			}
		}

		public const string BlankProductAreaCode = "BLK";

		public GlbStaffCollection StaffList
		{
			get { return staffList ?? (staffList = new GlbStaffCollection(Factory)); }
		}
		GlbStaffCollection staffList;
	}
}

