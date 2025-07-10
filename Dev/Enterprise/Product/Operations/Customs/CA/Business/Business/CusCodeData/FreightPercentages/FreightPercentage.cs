using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class FreightPercentage : CusCodeData, Integration.Customs.CA.IFreightPercentage
	{
		public FreightPercentage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : CusCodeData.Schema
		{
			public const string DefaultFreightPercentage = "DefaultFreightPercentage";
		}

		#endregion

		public new FreightPercentageLookups Lookups
		{
			get { return (FreightPercentageLookups)base.Lookups; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new FreightPercentageLookups(this);
		}

		public new FreightPercentageValidation Validation
		{
			get { return (FreightPercentageValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new FreightPercentageValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.FreightPercentage;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(OrgHeader)); }
		}

		[List(nameof(Lookups) + "." + nameof(FreightPercentageLookups.TransportTypeList))]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set { base.CY_Code = value; }
		}

		public ZDecimal DefaultFreightPercentage
		{
			get { return ZDecimal.ParseSafe(base.CY_Data, ZDecimal.Zero); }
			set { base.CY_Data = value.ToString(); }
		}

		public ZPropertyInfo DefaultFreightPercentageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DefaultFreightPercentage, x => CY_DataInfo); }
		}
	}
}
