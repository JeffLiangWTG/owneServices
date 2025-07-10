//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGlobalChargeCodeMapPivotLookups
//
//    This class should be used for overriding collections in AutoAccGlobalChargeCodeMapPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class AccGlobalChargeCodeMapPivotLookups : AutoAccGlobalChargeCodeMapPivotLookups
	{
		public AccGlobalChargeCodeMapPivotLookups(AutoAccGlobalChargeCodeMapPivot parent)
			: base(parent)
		{
		}

		#region LedgerTypes

		public CodeDescriptionPairList LedgerTypes
		{
			get
			{
				CodeDescriptionPairList fLedgerTypes = new CodeDescriptionPairList();
				fLedgerTypes.AddPair(ZArchitecture.Core.LedgerTypes.AccountsPayable, Res.GetString("DF2CA29A-33D7-4b9f-BACE-F656BD0FF69A", "Accounts Payable"));
				fLedgerTypes.AddPair(ZArchitecture.Core.LedgerTypes.AccountsReceivable, Res.GetString("CB9CC842-2998-4188-9E93-934B8B749453", "Accounts Receivable"));

				return fLedgerTypes;
			}
		}

		#endregion

		#region GlobalChargeCodeMaps

		public GlobalChargeCodeMapOrganizationCollection GlobalChargeCodeMapsOrganization
		{
			get { return new GlobalChargeCodeMapOrganizationCollection(Factory, ((GlobalChargeCodeMapPivotOrganization)Parent).ParentCollection.OrganisationPK); }
		}

		public GlobalChargeCodeMapIntercompanyCollection GlobalChargeCodeMapsIntercompany
		{
			get { return new GlobalChargeCodeMapIntercompanyCollection(Factory); }
		}

		#endregion
	}
}

