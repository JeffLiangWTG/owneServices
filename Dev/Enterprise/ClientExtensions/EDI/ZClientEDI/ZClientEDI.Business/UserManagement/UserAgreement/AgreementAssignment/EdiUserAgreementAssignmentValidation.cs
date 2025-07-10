//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiUserAgreementAssignmentValidation
//
//    This class should be used for overriding validation in AutoEdiUserAgreementAssignmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAssignmentValidation : AutoEdiUserAgreementAssignmentValidation
	{
		public EdiUserAgreementAssignmentValidation(AutoEdiUserAgreementAssignment parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly AutoEdiUserAgreementAssignment parent;

		protected override void CheckEAE_AgreementType()
		{
			base.CheckEAE_AgreementType();
			var query = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_AgreementType, parent.EAE_AgreementType);
			query.AddToFilter(EdiUserAgreementAssignmentSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
			query.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentID, parent.EAE_ParentID);
			query.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, parent.EAE_ParentTableCode);
			if (parent.Factory.Exists(typeof(EdiUserAgreementAssignment), query))
			{
				parent.EAE_AgreementTypeInfo.AddError(ResString.GetMultilingualString("c3c777e8-a64a-404a-b2e5-6ae80e881af5", "Duplicate agreement type"));
			}
		}

		protected override void CheckEAE_AllowOnlineAcceptance()
		{
			base.CheckEAE_AllowOnlineAcceptance();

			if (!parent.EAE_ParentTableCode.EqualsIgnoringCase(LicenceDatabaseSchema.Constants.Prefix) || !parent.EAE_AllowOnlineAcceptance || !(((EdiUserAgreementAssignment)parent).Parent is LicenceDatabase))
			{
				return;
			}

			var differentVariantDatabaseAssignmentsWithAllowAcceptanceQuery = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, LicenceDatabaseSchema.Constants.Prefix);
			differentVariantDatabaseAssignmentsWithAllowAcceptanceQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AgreementType, parent.EAE_AgreementType);
			differentVariantDatabaseAssignmentsWithAllowAcceptanceQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_VariantCode, SQLComparisonOperator.NotEqual, parent.EAE_VariantCode);
			differentVariantDatabaseAssignmentsWithAllowAcceptanceQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AllowOnlineAcceptance, true);

			if (parent.Factory.Exists(typeof(EdiUserAgreementAssignment), differentVariantDatabaseAssignmentsWithAllowAcceptanceQuery))
			{
				parent.EAE_AllowOnlineAcceptanceInfo.AddError(ResString.GetMultilingualString("36e93386-01df-4195-8ebf-77da6a18668f", "Only one Variant can have online acceptance enabled per Agreement Type on an Enterprise License."));
			}
		}

		protected override void CheckEAE_ParentTableCode()
		{
			base.CheckEAE_ParentTableCode();
			MandatoryValidation.CheckEntered(parent.EAE_ParentTableCodeInfo,  (IMultilingualString)ResString.GetMultilingualString("4c130926-84db-4cca-9748-32cb1383a6b3", "Parent must be entered on the Assignment."));

			if(!string.IsNullOrEmpty(parent.EAE_ParentTableCode) && !parent.Lookups.IsPermittedParentTableCode(parent.EAE_ParentTableCode))
			{
				parent.EAE_ParentTableCodeInfo.AddError(ResString.GetMultilingualString("c7ed4019-824d-47bf-88a5-8dcc1e165c29", "The current parent is not supported"));
			}
		}

		protected override void CheckEAE_ParentID()
		{
			base.CheckEAE_ParentID();

			if (parent.EAE_ParentTableCode.EqualsIgnoringCase(LicenceDatabaseSchema.Constants.Prefix))
			{
				ListValidation.ErrorIfInvalidPK(parent.EAE_ParentIDInfo);
			}

			if (parent.EAE_ParentID.IsEmpty || !parent.EAE_ParentID.IsValid)
			{
				parent.EAE_ParentIDInfo.AddError(ResString.GetMultilingualString("9f82db43-7b9a-4bb3-9cbf-ea37f39b8aef", "Parent must be entered on the Assignment."));
			}
			else
			{
				var duplicateAssignmentQuery = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_ParentID, parent.EAE_ParentID);
				duplicateAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AgreementType, parent.EAE_AgreementType);
				duplicateAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);

				if (parent.EAE_ParentTableCode.EqualsIgnoringCase(LicenceEnterpriseSchema.Constants.Prefix))
				{
					if (parent.Factory.Exists(typeof(EdiUserAgreementAssignment), duplicateAssignmentQuery))
					{
						parent.EAE_ParentIDInfo.AddError(ResString.GetMultilingualString("492606d4-b1a5-4780-ba6c-fd0df41f18cd", "There can only be one Assignment per Agreement Type for Enterprise Level Assignments."));
					}
				}
				else if (parent.EAE_ParentTableCode.EqualsIgnoringCase(LicenceDatabaseSchema.Constants.Prefix))
				{
					if (parent.Factory.Exists(typeof(EdiUserAgreementAssignment), duplicateAssignmentQuery))
					{
						parent.EAE_ParentIDInfo.AddError(ResString.GetMultilingualString("eae89fa8-27b7-4e2e-89f5-d46fb7c7475e", "There is already an Assignment against this Database for this Agreement Type."));
					}

					var databaseParent = ((EdiUserAgreementAssignment)parent).Parent as LicenceDatabase;

					if (databaseParent != null)
					{
						var enterpriseParentQuery = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_ParentID, databaseParent.LD_LE);
						enterpriseParentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AgreementType, parent.EAE_AgreementType);

						if (parent.Factory.Exists(typeof(EdiUserAgreementAssignment), enterpriseParentQuery))
						{
							parent.EAE_ParentIDInfo.AddError(ResString.GetMultilingualString("efb30cd8-7cb6-4787-afc7-35a402460020", "There is already an Enterprise level Assignment for this Agreement Type. It must be removed if you want to specify Database level Assignments."));
						}
					}
				}
			}
		}

		protected override void CheckEAE_OH_ClientAgreementOrg()
		{
			base.CheckEAE_OH_ClientAgreementOrg();

			if (parent.EAE_ParentTableCode.EqualsIgnoringCase(LicenceDatabaseSchema.Constants.Prefix) && parent.EAE_OH_ClientAgreementOrg.IsEmpty)
			{
				parent.EAE_OH_ClientAgreementOrgInfo.AddError(ResString.GetMultilingualString("88276d5b-874e-47d3-919f-da0acd4ff277", "Client Agreement Organization must be entered on Database Level Assignments."));
			}
			else if (parent.EAE_ParentTableCode.EqualsIgnoringCase(LicenceEnterpriseSchema.Constants.Prefix) && !parent.EAE_OH_ClientAgreementOrg.IsEmpty)
			{
				parent.EAE_OH_ClientAgreementOrgInfo.AddError(ResString.GetMultilingualString("69c1d7ab-d51e-4b81-9a48-43f437cccb21", "Enterprise Level Assignments cannot have Client Agreement Organization overrides."));
			}
		}
	}
}

