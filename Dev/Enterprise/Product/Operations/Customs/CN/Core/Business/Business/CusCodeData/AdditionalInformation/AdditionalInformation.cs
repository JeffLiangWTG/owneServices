using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class AdditionalInformation : CusCodeData
	{
		public AdditionalInformation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override

		public override bool SupportsNotes => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.CY_Type = Constants.CusCodeDataTypes.Codes.AdditionalInformation;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusClassPartPivot));

		protected override CusCodeDataValidation GetNewValidation() => new AdditionalInformationValidation(this);

		public new AdditionalInformationValidation Validation => (AdditionalInformationValidation)base.Validation;

		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;

		#endregion
	}
}
