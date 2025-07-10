using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitConsignmentItem : EU.ExitControl.Business.CusExitConsignmentItem
		, Integration.Customs.DEExitControl.ICusExitConsignmentItem
	{
		public CusExitConsignmentItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("2438F2D4-1848-48A8-A695-AFA35687859E", Caption = "Registration Number (ext.)", MediumCaption = "Rego. No. (ext.)", ShortCaption = "Rego. No.")]
		public override ZString CCI_ReferenceNumber
		{
			get => base.CCI_ReferenceNumber;
			set => base.CCI_ReferenceNumber = value;
		}

		public new ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentPivots => (ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>)base.CusExitConsignmentPivots;

		protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

		public new ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentPackagePivots => (ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>)base.CusExitConsignmentPackagePivots;

		protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPackagePivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Package);
	}
}
