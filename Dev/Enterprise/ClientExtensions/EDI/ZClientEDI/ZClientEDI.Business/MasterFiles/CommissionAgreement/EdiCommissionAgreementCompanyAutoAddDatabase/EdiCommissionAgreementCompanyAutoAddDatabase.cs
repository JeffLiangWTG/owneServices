using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyAutoAddDatabase : AutoEdiCommissionAgreementCompanyAutoAddDatabase
	{
		public EdiCommissionAgreementCompanyAutoAddDatabase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region EPD_EZN

		[RelatedBusinessObject("CommissionAgreementCustomization")]
		public override ZGuid EPD_EZN
		{
			get { return base.EPD_EZN; }
			set { base.EPD_EZN = value; }
		}

		public virtual EdiCommissionAgreementCustomization CommissionAgreementCustomization
		{
			get { return Factory.Load<EdiCommissionAgreementCustomization>(EPD_EZN); }
		}

		#endregion

		#region EPD_LD

		[RelatedBusinessObject("LicenceDatabase")]
		public override ZGuid EPD_LD
		{
			get { return base.EPD_LD; }
			set { base.EPD_LD = value; }
		}

		public virtual LicenceDatabase LicenceDatabase
		{
			get { return Factory.Load<LicenceDatabase>(EPD_LD); }
		}

		#endregion

		#region Save

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || CommissionAgreementCustomization == null || CommissionAgreementCustomization.IsSavedByFactory); }
		}

		#endregion
	}
}

