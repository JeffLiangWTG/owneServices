using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPivot : ExitControlBase.Business.CusExitConsignmentPivot
		, EUExitControl.ICusExitConsignmentPivot
	{
		public CusExitConsignmentPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusExitConsignmentPivotTypeDecider TypeDecider = new CusExitConsignmentPivotTypeDecider();

		public new CusExitConsignmentItem ConsignmentItem => Factory.Load<CusExitConsignmentItem>(CNP_CCI_ConsignmentItem);

		public new CusExitContainer Container => Factory.Load<CusExitContainer>(CNP_CXN_Container);

		public new CusExitConsignmentPackage Package => (CusExitConsignmentPackage)base.Package;

		protected override ExitControlBase.Business.CusExitConsignmentPackage GetPackageCore() => Factory.Load<CusExitConsignmentPackage>(CNP_CXP_Package);

		public new CusExitConsignmentPivotLookups Lookups => (CusExitConsignmentPivotLookups)base.Lookups;

		public new CusExitConsignmentPivotValidation Validation => (CusExitConsignmentPivotValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitConsignmentPivotLookups GetNewLookups() => new CusExitConsignmentPivotLookups(this);

		protected override ExitControlBase.Business.CusExitConsignmentPivotValidation GetNewValidation() => new CusExitConsignmentPivotValidation(this);

		[ResourceStringData("{B4A58C4B-74D1-4818-AE5A-C977AF2662BF}", Caption = "Container/Equipment")]
		[List(nameof(Lookups) + "." + nameof(CusExitConsignmentPivotLookups.CusExitContainers))]
		public override ZGuid CNP_CXN_Container
		{
			get => base.CNP_CXN_Container;
			set => base.CNP_CXN_Container = value;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("{981BFE6A-5716-49E5-9F8B-C351F306610A}", "Consignment Item Package ({0})", Package.CXP_Sequence);
	}
}
