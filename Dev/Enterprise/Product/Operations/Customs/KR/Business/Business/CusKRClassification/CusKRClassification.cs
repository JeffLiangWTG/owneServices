using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.KR.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(CusClassPartPivot), "KRClassification")]
	public class CusKRClassification : AutoCusKRClassification
	{
		public CusKRClassification(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		public CusClassPartPivot Pivot => pivot ?? (pivot = Factory.Load<CusClassPartPivot>(CKR_CI));
		CusClassPartPivot pivot;

		[ResourceStringData("404C557D-B53C-47C9-85EF-B867290CF995", Caption = "Ingredient")]
		public override ZString CKR_Ingredient { get => base.CKR_Ingredient; set => base.CKR_Ingredient = value; }
		[List(nameof(Lookups) + "." + nameof(CusKRClassificationLookups.CountryOfOriginLabelLocationCodeList))]
		[ResourceStringData("46AAB172-EBEC-41C9-A334-B08B26E66B0F", Caption = "C/O Label Location")]
		public override ZString CKR_COOLabelLocation { get => base.CKR_COOLabelLocation; set => base.CKR_COOLabelLocation = value; }
	}
}
