using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementDatabasePivot : AutoEdiCommissionAgreementDatabasePivot
	{
		public EdiCommissionAgreementDatabasePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region EZD_EZN

		[RelatedBusinessObject("CommissionAgreementCustomization")]
		public override ZGuid EZD_EZN
		{
			get { return base.EZD_EZN; }
			set { base.EZD_EZN = value; }
		}

		public virtual EdiCommissionAgreementCustomization CommissionAgreementCustomization
		{
			get { return Factory.Load<EdiCommissionAgreementCustomization>(EZD_EZN); }
		}

		#endregion

		#region EZD_LD

		[RelatedBusinessObject("LicenceDatabase")]
		public override ZGuid EZD_LD
		{
			get { return base.EZD_LD; }
			set { base.EZD_LD = value; }
		}

		public virtual LicenceDatabase LicenceDatabase
		{
			get { return Factory.Load<LicenceDatabase>(EZD_LD); }
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

