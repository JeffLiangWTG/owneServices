using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ABLEntryNumRelatedPacksGenPivot : CustomsGenPivot, Integration.Customs.ASYCUDA.IABLEntryNumRelatedPacksGenPivot
	{
		public const string RelationType = GenPivotTypeDecider.Types.ABCEntryNumRelatedPacksGenPivot;

		public ABLEntryNumRelatedPacksGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.ABLEntryNumRelatedPacksGenPivot|XX_Relation2ID", Caption = "Pack")]
		[List(nameof(Packs))]
		public override ZGuid XX_Relation2ID
		{
			get { return base.XX_Relation2ID; }
			set { base.XX_Relation2ID = value; }
		}

		public new ABLEntryNum Relation1Object
		{
			get { return base.Relation1Object as ABLEntryNum; }
			set { base.Relation1Object = value; }
		}

		public new AsycudaPack Relation2Object
		{
			get { return base.Relation2Object as AsycudaPack; }
			set { base.Relation2Object = value; }
		}

		public new ABLEntryNumRelatedPacksGenPivotValidation Validation
		{
			get { return (ABLEntryNumRelatedPacksGenPivotValidation)base.Validation; }
		}

		protected override GenPivotValidation GetNewValidation()
		{
			return new ABLEntryNumRelatedPacksGenPivotValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = RelationType;
			XX_Relation1TableCode = CusEntryNumSchema.Constants.Prefix;
			XX_Relation2TableCode = AsycudaPackSchema.Constants.Prefix;
		}

		public IAsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => Relation1Object?.Bill?.Packs;
	}
}
