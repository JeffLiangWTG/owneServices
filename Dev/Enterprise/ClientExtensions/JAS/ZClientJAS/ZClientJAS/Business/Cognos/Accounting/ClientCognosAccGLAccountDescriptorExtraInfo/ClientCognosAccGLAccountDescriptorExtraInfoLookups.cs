//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientCognosAccGLAccountDescriptorExtraInfoLookups
//
//    This class should be used for overriding collections in AutoClientCognosAccGLAccountDescriptorExtraInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ClientCognosAccGLAccountDescriptorExtraInfoLookups : AutoClientCognosAccGLAccountDescriptorExtraInfoLookups
	{
		public ClientCognosAccGLAccountDescriptorExtraInfoLookups(AutoClientCognosAccGLAccountDescriptorExtraInfo parent)
			: base(parent)
		{
		}

		public AccGLAccountDescriptorCollection CognosAccounts
		{
			get
			{
				if (fCognosAccounts == null)
				{
					ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportCategory, SQLComparisonOperator.NotEqual, Core.Constants.AccountType.Header);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
					filter.AddToFilter(AccGLAccountDescriptorSchema.PK, SQLComparisonOperator.NotEqual, Parent.T9_AJ);
					fCognosAccounts = new AccGLAccountDescriptorCollection(Factory, filter);
				}
				return fCognosAccounts;
			}
		}

		public OrgDebtorGroupCollection DebtorGroups
		{
			get
			{
				if (fDebtorGroups == null)
				{
					fDebtorGroups = new OrgDebtorGroupCollection(Factory);
				}
				return fDebtorGroups;
			}
		}

		public OrgCreditorGroupCollection CreditorGroups
		{
			get
			{
				if (fCreditorGroups == null)
				{
					fCreditorGroups = new OrgCreditorGroupCollection(Factory);
				}
				return fCreditorGroups;
			}
		}

		public CodeDescriptionPairList AccountAgeList
		{
			get
			{
				if (fAccountAgeList == null)
				{
					fAccountAgeList = new CodeDescriptionPairList();
					fAccountAgeList.AddPair("30", "Account aged between 0 - 30 days");
					fAccountAgeList.AddPair("60", "Account aged between 31 - 60 days");
					fAccountAgeList.AddPair("90", "Account aged between 61 - 90 days");
					fAccountAgeList.AddPair("++", "Account aged 90+ days");
				}
				return fAccountAgeList;
			}
		}

		new CognosAccGLAccountDescriptorExtraInfo Parent
		{
			get { return (CognosAccGLAccountDescriptorExtraInfo)base.Parent; }
		}

		AccGLAccountDescriptorCollection fCognosAccounts;
		OrgDebtorGroupCollection fDebtorGroups;
		OrgCreditorGroupCollection fCreditorGroups;
		CodeDescriptionPairList fAccountAgeList;
	}
}

#region Implementation
#endregion
