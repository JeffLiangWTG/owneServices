using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CustomsOffice : CusCodeData, Integration.Customs.CN.ICustomsOffice
	{
		public CustomsOffice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Overrides of CusCodeData

		public override bool SupportsNotes => false;

		public JobDeclaration Declaration => (JobDeclaration)Parent;

		protected override CusCodeDataValidation GetNewValidation() => new CustomsOfficeValidation(this);

		public new CustomsOfficeValidation Validation => (CustomsOfficeValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups() => new CustomsOfficeLookups(this);

		public new CustomsOfficeLookups Lookups => (CustomsOfficeLookups)base.Lookups;

		public override void OnSaving()
		{
			if (!IsDeleted && IsInDatabase && CY_Data.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public bool IsEmpty => IsDeleted || CY_Data.IsEmpty;

		public override bool IsSavedByFactory => IsInDatabase ? base.IsSavedByFactory : !IsEmpty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = Constants.CusCodeDataTypes.Codes.CustomsOffice;
			CY_Code = CustomsOfficeTypeList.Codes.DES;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		[MaxLength(3)]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(CustomsOfficeLookups.CustomsOfficeList))]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		#endregion
	}
}
