using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForDeclaration : CommonGuarantee
	{
		public GuaranteeForDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusBondDetail.Schema
		{
			public const string EntryInstructionID = "EntryInstructionID";
		}

		public JobDeclaration Declaration
		{
			get => declaration ?? (declaration = Factory.Load<JobDeclaration>(PW_ParentID));
			set => declaration = value;
		}
		JobDeclaration declaration;

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(GuaranteeForDeclarationLookups.HolderIdentificationList))]
		public override ZString PW_HolderIdentification { get => base.PW_HolderIdentification; set => base.PW_HolderIdentification = value; }

		public override ZDecimal PW_BondAmount
		{
			get => base.PW_BondAmount;

			set
			{
				base.PW_BondAmount = value;
				Declaration?.MarkAsNeedingValidation();
			}
		}

		public override ZString PW_RX_NKCurrency
		{
			get => base.PW_RX_NKCurrency;

			set
			{
				base.PW_RX_NKCurrency = value;
				Declaration?.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region EntryInstruction

		[List(nameof(Lookups) + "." + nameof(GuaranteeForDeclarationLookups.EntryInstructions))]
		[RelatedBusinessObject("EntryInstruction")]
		public ZGuid EntryInstructionID
		{
			get
			{
				if (entryInstructionPivot == null || entryInstructionPivot.IsDeleted)
				{
					EnsurePivotExists();
				}
				return entryInstructionPivot.XX_Relation2ID;
			}
			set
			{
				var oldValue = EntryInstructionID;
				if (value.IsEmpty && entryInstructionPivot != null)
				{
					entryInstructionPivot.Delete();
					entryInstructionPivot = null;
				}
				else
				{
					if (entryInstructionPivot == null || entryInstructionPivot.IsDeleted)
					{
						EnsurePivotExists();
					}
					entryInstructionPivot.XX_Relation2ID = value;

					if (!value.IsEmpty && oldValue != value)
					{
						OnEntryInstructionIDChanged();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateEntryInstructionID();
					}
				}
				EntryInstructionIDInfo.RefreshBinding(oldValue);
			}
		}

		protected virtual void OnEntryInstructionIDChanged()
		{
		}

		public ZPropertyInfo EntryInstructionIDInfo => GetZPropertyInfo(Schema.EntryInstructionID);

		internal void EnsurePivotExists()
		{
			if (entryInstructionPivot == null || entryInstructionPivot.IsDeleted)
			{
				var query = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.CusBondDetailRelatedEntryInstructionPivot);
				query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusBondDetailSchema.Constants.Prefix);
				query.AddToFilter(GenPivotSchema.XX_Relation1ID, PK);
				query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusEntryInstructionSchema.Constants.Prefix);
				query.OrderBy = GenPivotSchema.Constants.PK;
				entryInstructionPivot = Factory.LoadTop1<GenPivot>(query);

				if (entryInstructionPivot == null)
				{
					entryInstructionPivot = Factory.New<GenPivot>();
					entryInstructionPivot.XX_RelationType = GenPivotTypeDecider.Types.CusBondDetailRelatedEntryInstructionPivot;
					entryInstructionPivot.XX_Relation1ID = PK;
					entryInstructionPivot.XX_Relation1TableCode = CusBondDetailSchema.Constants.Prefix;
					entryInstructionPivot.XX_Relation2TableCode = CusEntryInstructionSchema.Constants.Prefix;
				}
			}
		}
		GenPivot entryInstructionPivot;

		public CusEntryInstruction EntryInstruction => Factory.Load<CusEntryInstruction>(EntryInstructionID);

		#endregion

		public override void Delete()
		{
			entryInstructionPivot?.Delete();
			base.Delete();
		}

		protected override TypeLoaderCollection ParentLoaders => new (typeof(JobDeclaration));

		#region TypeSafe

		protected override CusBondDetailLookups GetNewLookups() => new GuaranteeForDeclarationLookups(this);
		public new GuaranteeForDeclarationLookups Lookups => (GuaranteeForDeclarationLookups)base.Lookups;

		protected override CusBondDetailValidation GetNewValidation() => new GuaranteeForDeclarationValidation(this);
		public new GuaranteeForDeclarationValidation Validation => (GuaranteeForDeclarationValidation)base.Validation;

		#endregion
	}
}
