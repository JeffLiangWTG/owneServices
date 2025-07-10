using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class InventoryInfo : CusSupportingInfo
	{
		public InventoryInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public new const int CSI_CodeMaxLength = 10;
		}

		public new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

		public MoveInDestination ParentMoveInDestination => parentMoveInDestination ??= Parent.MoveInDestinationInfos.Cast<MoveInDestination>().FirstOrDefault(x => x.PK == CSI_CSI_SupportingInfo);
		MoveInDestination parentMoveInDestination;

		public InventoryInfoCollection ParentCollection => parentCollection ??= ParentMoveInDestination.InventoryNumbers;
		InventoryInfoCollection parentCollection;

		[MaxLength(Schema.CSI_CodeMaxLength)]
		[ResourceStringData("JP.Business.InventoryInfo|CSI_Code", Caption = "Inventory Management Number", MediumCaption = "Number")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[ResourceStringData("JP.Business.InventoryInfo|CSI_Quantity", Caption = "Move-In Quantity", MediumCaption = "Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.Inventory;
			CSI_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
		}

		public new InventoryInfoValidation Validation => (InventoryInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new InventoryInfoValidation(this);
	}
}
