using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class SupplementaryCode : BaseSupplementaryCode
	{
		public SupplementaryCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region schema

		public new class Schema : BaseSupplementaryCode.Schema
		{
			public new const int CY_CodeMaxLength = 15;
		}

		#endregion

		[MaxLength(Schema.CY_CodeMaxLength)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[LightValidationTestExempt]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set => base.CY_Order = value;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				return new TypeLoaderCollection(
					typeof(JobComInvoiceLine),
					typeof(CusClassification),
					typeof(CusClassPartPivot),
					typeof(TemporaryStoragePackedItem),
					ObjectFactory.GetType(typeof(Integration.Customs.EU.NCTS.ICommonCargoDesc)));
			}
		}

		public new BaseSupplementaryCodeProvider Provider
		{
			get
			{
				var supporter = this.Supporter;
				var countryCode = supporter?.GetCountryCodeForCodeProvider() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				if (provider == null || provider.CountryCode != countryCode)
				{
					provider = SupplementaryCodeProvider.GetByCountryCode(countryCode);
				}
				return provider;
			}
		}
		BaseSupplementaryCodeProvider provider;

		protected override CusCodeDataValidation GetNewValidation() => Provider.GetNewValidation(this) ?? new SupplementaryCodeValidation(this);
	}
}
