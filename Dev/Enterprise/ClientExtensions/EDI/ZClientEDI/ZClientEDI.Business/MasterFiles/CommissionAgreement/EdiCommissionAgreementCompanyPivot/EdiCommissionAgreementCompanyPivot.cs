using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyPivot : AutoEdiCommissionAgreementCompanyPivot
	{
		public EdiCommissionAgreementCompanyPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region EPY_EZN

		[RelatedBusinessObject("CommissionAgreementCustomization")]
		public override ZGuid EPY_EZN
		{
			get { return base.EPY_EZN; }
			set { base.EPY_EZN = value; }
		}

		public virtual EdiCommissionAgreementCustomization CommissionAgreementCustomization
		{
			get { return Factory.Load<EdiCommissionAgreementCustomization>(EPY_EZN); }
		}

		#endregion

		#region EPY_LCC

		[RelatedBusinessObject("ClientCompany")]
		public override ZGuid EPY_LCC
		{
			get { return base.EPY_LCC; }
			set { base.EPY_LCC = value; }
		}

		public virtual ClientCompany ClientCompany
		{
			get { return Factory.Load<ClientCompany>(EPY_LCC); }
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

