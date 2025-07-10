using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPESimilarOrgMatchForApproval : SimilarOrgMatchForApproval
	{
		protected UPESimilarOrgMatchForApproval(OrgMatchApproval matchApproval, OrgPatternMatch patternMatch)
			: base(matchApproval, patternMatch)
		{
		}

		#region Constructor

		public new static SimilarOrgMatchForApproval New(OrgMatchApproval matchApproval, OrgPatternMatch patternMatch)
		{
			return new UPESimilarOrgMatchForApproval(matchApproval, patternMatch);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		internal static bool IsSubTypeRegistered
		{
			get { return OverridableNewDelegate.IsOverriden; }
		}

		#endregion

		protected override ZString OwnerCodeType
		{
			get { return UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber; }
		}

		internal ZString GetOwnerCodeTypeForTest => OwnerCodeType;
	}
}
