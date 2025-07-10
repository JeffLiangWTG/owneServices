using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class CusBondDetail : MasterFiles.Business.CusBondDetail
	{
		public CusBondDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new OrgHeader Parent
		{
			get { return (OrgHeader)base.Parent; }
			set { base.Parent = value; }
		}

		public bool IsBondActive(ZDateTime date)
		{
			return (PW_BondEffectiveDate.IsEmpty || PW_BondEffectiveDate <= date) && (PW_BondExpiryDate >= date || PW_BondExpiryDate.IsEmpty);
		}

		public bool IsContinuousBond
		{
			get { return PW_BondType == BondTypeList.Codes.ContinuousBond; }
		}

		#region Implementation

		protected override MasterFiles.Business.CusBondDetailValidation GetNewValidation()
		{
			return new CusBondDetailValidation(this);
		}

		protected override MasterFiles.Business.CusBondDetailLookups GetNewLookups()
		{
			return new CusBondDetailLookups(this);
		}

		public new CusBondDetailLookups Lookups
		{
			get { return (CusBondDetailLookups)base.Lookups; }
		}

		public new CusBondDetailValidation Validation
		{
			get { return (CusBondDetailValidation)base.Validation; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
		}

		#endregion
	}
}
