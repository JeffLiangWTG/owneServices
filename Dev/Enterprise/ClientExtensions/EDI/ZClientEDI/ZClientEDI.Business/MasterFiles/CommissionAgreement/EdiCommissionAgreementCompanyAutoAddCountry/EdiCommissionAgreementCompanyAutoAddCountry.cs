using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyAutoAddCountry : AutoEdiCommissionAgreementCompanyAutoAddCountry
	{
		public EdiCommissionAgreementCompanyAutoAddCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region EPC_EZN

		[RelatedBusinessObject("CommissionAgreementCustomization")]
		public override ZGuid EPC_EZN
		{
			get { return base.EPC_EZN; }
			set { base.EPC_EZN = value; }
		}

		public virtual EdiCommissionAgreementCustomization CommissionAgreementCustomization
		{
			get { return Factory.Load<EdiCommissionAgreementCustomization>(EPC_EZN); }
		}

		#endregion

		#region EPC_LD

		[RelatedBusinessObject("LicenceDatabase")]
		public override ZGuid EPC_LD
		{
			get { return base.EPC_LD; }
			set { base.EPC_LD = value; }
		}

		public virtual LicenceDatabase LicenceDatabase
		{
			get { return Factory.Load<LicenceDatabase>(EPC_LD); }
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

