
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusClassPartPivotAddInfo : AUAddInfo, IEXDOCRefCodeTypeProvider
	{
		public CusClassPartPivotAddInfo(CusClassPartPivot parent, ZPropertyInfo addInfoProperty)
			: base(parent, addInfoProperty)
		{
		}

		protected override AUAddInfoValidation GetNewValidation()
		{
			return new CusClassPartPivotAddInfoValidation(this);
		}

		public new CusClassPartPivotAddInfoLookups Lookups => (CusClassPartPivotAddInfoLookups)base.Lookups;

		protected override AUAddInfoLookups GetNewLookups()
		{
			return new CusClassPartPivotAddInfoLookups(this);
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotAddInfoLookups.ProduceTypeList))]
		[ResourceStringData("CusClassPartPivotAddInfo.ZA_AQISProduceType_Hidden", Caption = "Produce Type")]
		public override ZString ZA_AQISProduceType_Hidden
		{
			get
			{
				return base.ZA_AQISProduceType_Hidden;
			}
			set
			{
				base.ZA_AQISProduceType_Hidden = value;
				if (value.IsEmpty)
				{
					ClearQuarantineDetailFields();
				}
			}
		}

		void ClearQuarantineDetailFields()
		{
			ZA_AQISProduct_Hidden = "";
			ZA_AQISSupplementaryCode_Hidden = "";
			ZA_AQISPackType_Hidden = "";
			ZA_AQISPreservation_Hidden = "";
			ZA_AQISCutCode_Hidden = "";
			ZA_AQISCategoryCode_Hidden = "";
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotAddInfoLookups.ProductList))]
		[ResourceStringData("CusClassPartPivotAddInfo.ZA_AQISProduct_Hidden", Caption = "Product")]
		public override ZString ZA_AQISProduct_Hidden { get => base.ZA_AQISProduct_Hidden; set => base.ZA_AQISProduct_Hidden = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotAddInfoLookups.CategoryCodes))]
		[ResourceStringData("CusClassPartPivotAddInfo.ZA_AQISCategoryCode_Hidden", Caption = "Category Code")]
		public override ZString ZA_AQISCategoryCode_Hidden { get => base.ZA_AQISCategoryCode_Hidden; set => base.ZA_AQISCategoryCode_Hidden = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotAddInfoLookups.SupplementaryCodesList))]
		[ResourceStringData("CusClassPartPivotAddInfo.ZA_AQISSupplementaryCode_Hidden", Caption = "Supplementary Code")]
		public override ZString ZA_AQISSupplementaryCode_Hidden { get => base.ZA_AQISSupplementaryCode_Hidden; set => base.ZA_AQISSupplementaryCode_Hidden = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotAddInfoLookups.PackTypeList))]
		[ResourceStringData("CusClassPartPivotAddInfo.ZA_AQISPackType_Hidden", Caption = "Pack Type")]
		public override ZString ZA_AQISPackType_Hidden { get => base.ZA_AQISPackType_Hidden; set => base.ZA_AQISPackType_Hidden = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotAddInfoLookups.PreservationList))]
		[ResourceStringData("CusClassPartPivotAddInfo.ZA_AQISPreservation_Hidden", Caption = "Preservation")]
		public override ZString ZA_AQISPreservation_Hidden { get => base.ZA_AQISPreservation_Hidden; set => base.ZA_AQISPreservation_Hidden = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotAddInfoLookups.CutCodesList))]
		[ResourceStringData("CusClassPartPivotAddInfo.ZA_AQISCutCode_Hidden", Caption = "Cut Code")]
		public override ZString ZA_AQISCutCode_Hidden { get => base.ZA_AQISCutCode_Hidden; set => base.ZA_AQISCutCode_Hidden = value; }

		#region IEXDOCRefCodeTypeProvider Members

		ZString IEXDOCRefCodeTypeProvider.Type => ZA_AQISProduceType_Hidden;

		BusinessObjectFactory IEXDOCRefCodeTypeProvider.Factory => Factory;

		#endregion
	}
}
