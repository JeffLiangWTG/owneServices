using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupplementaryDeclarant : CusCodeData
	{
		public SupplementaryDeclarant(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("C16CBC06-B18D-4CC7-8AEE-C02EE896CC5B", "Supplementary Declarant");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.EUICS2SupplementaryDeclarant;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaBill));

		public new SupplementaryDeclarantLookups Lookups => (SupplementaryDeclarantLookups)base.Lookups;
		protected override CusCodeDataLookups GetNewLookups() => new SupplementaryDeclarantLookups(this);

		public new SupplementaryDeclarantValidation Validation => (SupplementaryDeclarantValidation)base.Validation;
		protected override CusCodeDataValidation GetNewValidation() => new SupplementaryDeclarantValidation(this);

		#region Properties

		[MaxLength(17)]
		[ResourceStringData("EU.ICS2.SupplementaryDeclarant.IdentificationNumber", Caption = "Identification Number")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[MaxLength(3)]
		[ResourceStringData("EU.ICS2.SupplementaryDeclarant.FilingType", Caption = "Filing Type")]
		[List(nameof(Lookups) + "." + nameof(SupplementaryDeclarantLookups.SupplementaryDeclarantFilingTypesList))]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		#endregion
	}
}
