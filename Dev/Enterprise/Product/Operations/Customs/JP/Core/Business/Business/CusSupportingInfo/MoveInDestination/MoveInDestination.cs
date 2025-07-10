using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;

namespace Enterprise.Customs.JP.Business
{
	public class MoveInDestination : CusSupportingInfo
	{
		public MoveInDestination(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string SubType = "ITEM";

		public new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

		public MoveInDestinationCollection ParentCollection => Parent.MoveInDestinationInfos;

		[MaxLength(5)]
		[ResourceStringData("24E403F8-FDBA-4A26-AAF8-A3E0393406F0", Caption = "Destination", ShortCaption = "Dest.", FullDescription = "Expected Move-In Location Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code && !IsCopying)
				{
					Parent.MoveInDestinationInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("B5FEEB14-5396-4018-900F-1895C210890A", Caption = "Date", FullDescription = "Expected Move-In Date")]
		public override ZDateTime CSI_DateOfIssue
		{
			get => base.CSI_DateOfIssue;
			set
			{
				var oldValue = CSI_DateOfIssue;
				base.CSI_DateOfIssue = value;
				if (oldValue != CSI_DateOfIssue && !IsCopying)
				{
					Parent.MoveInDateInfo.RefreshBinding();
				}
			}
		}

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(MoveInDestinationLookups.ViaList))]
		[ResourceStringData("E57E5A94-7DD6-4854-A1DA-C0913E1F8107", Caption = "Via", FullDescription = "Enter the Vanning Location Code if the Vanning Location is different from the clearance location.")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("B7684655-E16D-4BFE-BCEE-40C39961CE57", Caption = "Quantity", ShortCaption = "Qty.", FullDescription = "Expected Move-In Quantity")]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set
			{
				var oldValue = CSI_Quantity;
				base.CSI_Quantity = value;
				if (oldValue != CSI_Quantity && !IsCopying)
				{
					Parent.MoveInQuantityInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("859EDC65-858B-41D5-B6A2-C69E34BE8EF8", Caption = "Weight", ShortCaption = "Wgt.", FullDescription = "Expected Move-In Weight")]
		public override ZDecimal CSI_Quantity2
		{
			get => base.CSI_Quantity2;
			set
			{
				var oldValue = CSI_Quantity2;
				base.CSI_Quantity2 = value;
				if (oldValue != CSI_Quantity2 && !IsCopying)
				{
					Parent.MoveInWeightInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("D2B8FEE5-794E-442E-A5BD-2DFD42ACD52A", Caption = "Volume", ShortCaption = "Vol.", FullDescription = "Expected Move-In Volume")]
		public override ZDecimal CSI_Quantity3 { get => base.CSI_Quantity3; set => base.CSI_Quantity3 = value; }

		[ResourceStringData("B15A527C-76E2-4F31-9EB4-ECFF93D5D290", Caption = "Marks & Numbers", ShortCaption = "Marks")]
		[MaxLength(140)]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		[ChildEditable]
		public InventoryInfoCollection InventoryNumbers
		{
			get
			{
				if (inventoryNumbers == null)
				{
					inventoryNumbers = new InventoryInfoCollection(Parent, PK);
					inventoryNumbers.Load();
					RegisterEditableChildObject(inventoryNumbers);
				}

				return inventoryNumbers;
			}
		}
		InventoryInfoCollection inventoryNumbers;

		#region Overrides

		public new MoveInDestinationValidation Validation => (MoveInDestinationValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new MoveInDestinationValidation(this);

		public new MoveInDestinationLookups Lookups => (MoveInDestinationLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new MoveInDestinationLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ExpectedMoveInDestination;
			CSI_SubType = SubType;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				InventoryNumbers.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		#endregion
	}
}
