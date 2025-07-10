using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class NonPersistentCusDV1DetailPivot : AutoNonPersistentCusDV1DetailPivot
	{
		public NonPersistentCusDV1DetailPivot(CusEntryInstruction entryInstruction)
			: base(entryInstruction?.Factory)
		{
			EntryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		}
		protected CusEntryInstruction EntryInstruction { get; }

		[ResourceStringData("NonPersistentCusDV1DetailPivot.IsForEntryInstruction", Caption = "Is for entry instruction?")]
		public override ZBool IsForEntryInstruction
		{
			get => HasPivot;
			set
			{
				var originalValue = IsForEntryInstruction;
				base.IsForEntryInstruction = value;
				if (originalValue != value)
				{
					if (DV1Detail != null)
					{
						if (value)
						{
							AddIsForEntryInstructionPivot();
						}
						else
						{
							DeleteIsForEntryInstructionPivot();
						}
					}
					IsForEntryInstructionInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("A297BDD6-2750-4ADA-ADED-B35A13D07C6D", Caption = "Sequence No.")]
		public override ZShort Sequence => DV1Detail?.Sequence ?? ZShort.Zero;

		[ResourceStringData("5CDF6D1E-A3EE-4278-9DD0-0EAB523C2C8A", Caption = "Relationship")]
		public override ZString Relationship => DV1Detail?.DV1_Relationship ?? ZString.Empty;

		[ResourceStringData("63166524-17FB-48C5-8140-C6F93A1A8FC6", Caption = "Price influenced?")]
		public override ZString PriceInfluence => DV1Detail?.DV1_PriceInfluence ?? ZString.Empty;

		[ResourceStringData("E05FB82C-143B-47A7-ABC9-9FE0595159A8", Caption = "Details")]
		public override ZString RelationDetails => DV1Detail?.DV1_RelationDetails ?? ZString.Empty;

		[ResourceStringData("D8AE7E34-DF0B-4538-86F9-B1FE39ABA42A", Caption = "Restrictions")]
		public override ZString Restrictions => DV1Detail?.DV1_Restrictions ?? ZString.Empty;

		[ResourceStringData("6E9E15FA-C9B2-4DC6-B1D2-3174C96CB85A", Caption = "Conditions")]
		public override ZString Consideration => DV1Detail?.DV1_Consideration ?? ZString.Empty;

		[ResourceStringData("2624EB9B-71A6-4668-A6DF-3C8321D962C7", Caption = "Details")]
		public override ZString RestrictionConsiderationDetails => DV1Detail?.DV1_RestrictionConsiderationDetails ?? ZString.Empty;

		[ResourceStringData("1130B556-0073-4353-A849-D70A5DB47026", Caption = "License Fees")]
		public override ZString RoyaltiesLicence => DV1Detail?.DV1_RoyaltiesLicence ?? ZString.Empty;

		[ResourceStringData("965BE1EA-58D3-4A6D-A37C-8BF923FEC013", Caption = "Details")]
		public override ZString RoyaltiesLicenceDetails => DV1Detail?.DV1_RoyaltiesLicenceDetails ?? ZString.Empty;

		[ResourceStringData("787138CF-A30E-4DFC-9542-1EC4AAAEF0C7", Caption = "Resale")]
		public override ZString Resale => DV1Detail?.DV1_Resale ?? ZString.Empty;

		[ResourceStringData("9F4ED4D4-74E7-4BEB-9311-45DAB1CCD64F", Caption = "Details")]
		public override ZString ResaleDetails => DV1Detail?.DV1_ResaleDetails ?? ZString.Empty;

		[ResourceStringData("6DE8AE8A-954F-4259-B685-87951C18704B", Caption = "Former Decisions")]
		public override ZString CustomsDecisionNumber => DV1Detail?.DV1_CustomsDecisionNumber ?? ZString.Empty;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				DeleteIsForEntryInstructionPivot();
			}
			base.Delete();
		}

		public CusDV1Detail DV1Detail
		{
			get
			{
				return (cusDV1Detail != null && !cusDV1Detail.IsDeleted) ? cusDV1Detail : null;
			}
			set
			{
				cusDV1Detail = value;
				base.IsForEntryInstruction = HasPivot;
			}
		}
		CusDV1Detail cusDV1Detail;

		protected virtual void AddIsForEntryInstructionPivot()
		{
			if (!HasPivot)
			{
				var genPivot = Factory.New<GenPivot>();
				genPivot.XX_RelationType = Core.Constants.GenPivotTypes.CusDV1Detail;
				genPivot.XX_Relation1ID = EntryInstruction.PK;
				genPivot.XX_Relation2ID = cusDV1Detail.PK;
				genPivot.XX_Relation1TableCode = CusEntryInstructionSchema.Constants.Prefix;
				genPivot.XX_Relation2TableCode = CusDV1DetailSchema.Constants.Prefix;
			}
		}

		protected void DeleteIsForEntryInstructionPivot()
		{
			IsForEntryInstructionPivot?.Delete();
		}

		protected ZBool HasPivot => IsForEntryInstructionPivot != null;

		GenPivot IsForEntryInstructionPivot
		{
			get
			{
				GenPivot result = null;
				if (cusDV1Detail != null)
				{
					var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.CusDV1Detail);
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, EntryInstruction.PK);
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, cusDV1Detail.PK);
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusEntryInstructionSchema.Constants.Prefix);
					pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusDV1DetailSchema.Constants.Prefix);
					result = EntryInstruction.Factory.LoadTop1<GenPivot>(pivotQuery);
				}
				return result;
			}
		}
	}
}
