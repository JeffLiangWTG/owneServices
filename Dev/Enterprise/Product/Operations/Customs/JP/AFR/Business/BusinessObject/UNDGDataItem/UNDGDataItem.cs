using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class UNDGDataItem : MasterFiles.Business.UNDGDataItem
	{
		public UNDGDataItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JPAFRBills Bill => IsParentValidToSave ? Factory.Load<JPAFRBills>(DI_ParentID) : null;

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.UNDGDataItem|DI_DG", Caption = "UNDG")]
		public override ZGuid DI_DG { get => base.DI_DG; set => base.DI_DG = value; }

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.UNDGDataItem|DI_Description", Caption = "Description")]
		public ZString DI_Description => UNDGSubstance?.DG_PSN ?? ZString.Empty;

		public ZPropertyInfo DI_DescriptionInfo => GetZPropertyInfo(nameof(DI_Description));

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.UNDGDataItem|DI_IMOClass", Caption = "IMO Class")]
		public override ZString DI_IMOClass { get => base.DI_IMOClass; set => base.DI_IMOClass = value; }

		public new UNDGDataItemValidation Validation => (UNDGDataItemValidation)base.Validation;

		protected override MasterFiles.Business.UNDGDataItemValidation GetNewValidation()
		{
			return new UNDGDataItemValidation(this);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("34936082-a148-4def-b4b0-756cb38f92be", "UNDG Item");
	}
}
