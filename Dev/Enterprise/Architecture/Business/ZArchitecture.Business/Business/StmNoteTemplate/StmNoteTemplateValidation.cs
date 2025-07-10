//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmNoteTemplateValidation
//
//    This class should be used for overriding validation in AutoStmNoteTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteTemplateValidation : AutoStmNoteTemplateValidation
	{
		public StmNoteTemplateValidation(AutoStmNoteTemplate parent) : base(parent)
		{
		}

		protected override void CheckS8_Description()
		{
			base.CheckS8_Description();
			MandatoryValidation.CheckEntered(Parent.S8_DescriptionInfo);
			CheckDescriptionIsUnique();
		}

		protected void CheckDescriptionIsUnique()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmNoteTemplateSchema.S8_ContextID, Parent.S8_ContextID);
			query.AddToFilter(StmNoteTemplateSchema.S8_Description, Parent.S8_Description);
			query.AddToFilter(StmNoteTemplateSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			ZQuery securityMatchFilter = new ZQuery();
			securityMatchFilter.AddToFilter(StmNoteTemplateSchema.S8_GS_NKStaff, EnvProxy.Instance.CurrentUser.Initials);
			if (Parent.IsAllCompanies)
			{
				securityMatchFilter.AddToFilter(JoinCondition.Or, StmNoteTemplateSchema.S8_GS_NKStaff, ZString.Empty);
			}
			else
			{
				ZQuery publishedFilter = new ZQuery();
				publishedFilter.AddToFilter(StmNoteTemplateSchema.S8_GS_NKStaff, ZString.Empty);
				publishedFilter.AddToFilter(JoinCondition.And, StmNoteTemplateSchema.S8_GC, EnvProxy.Instance.CurrentCompany.PK);
				securityMatchFilter.AddToFilter(publishedFilter, JoinCondition.Or);
				ZQuery allCompaniesFilter = new ZQuery();
				allCompaniesFilter.AddToFilter(StmNoteTemplateSchema.S8_GS_NKStaff, ZString.Empty);
				allCompaniesFilter.AddToFilter(JoinCondition.And, StmNoteTemplateSchema.S8_GC, ZGuid.Empty);
				securityMatchFilter.AddToFilter(allCompaniesFilter, JoinCondition.Or);
			}
			query.AddToFilter(securityMatchFilter);
			StmNoteTemplate match = Parent.Factory.LoadTop1<StmNoteTemplate>(query);
			if (match != null)
			{
				if (match.IsPublished)
				{
					Parent.S8_DescriptionInfo.AddError(Res.GetString("8b54d6ba-fda7-4d82-8b7e-53ede35c18f0", "A template with the same name already exists in this context."));
				}
				else
				{
					Parent.S8_DescriptionInfo.AddError(Res.GetString("2a2280ef-7f4a-465a-9fdb-1a474cf83c68", "A published template with the same name already exists in this context."));
				}
			}
		}

		protected override void CheckS8_TemplateText()
		{
			base.CheckS8_TemplateText();
			MandatoryValidation.CheckEntered(Parent.S8_TemplateTextInfo);
		}

		public void ValidateIsPublished()
		{
			ValidateCalculatedProperty(Parent.IsPublishedInfo);
		}

		protected void CheckIsPublished()
		{
			if (Parent.IsPublished && !Parent.SecurityProvider.CurrentUserCanPublish)
			{
				Parent.IsPublishedInfo.AddError(StmNoteTemplateSecurityProvider.SecurityCheckpointForPublish.ErrorMessageForNotAllowed);
			}
		}

		public void ValidateIsAllCompanies()
		{
			ValidateCalculatedProperty(Parent.IsAllCompaniesInfo);
		}

		protected void CheckIsAllCompanies()
		{
			if (Parent.IsAllCompanies && !Parent.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies)
			{
				Parent.IsAllCompaniesInfo.AddError(Res.GetString("4794a3ed-1966-4552-ae9b-797c01496670", "{0} - for all companies", StmNoteTemplateSecurityProvider.SecurityCheckpointForPublish.ErrorMessageForNotAllowed));
			}
		}

		protected new StmNoteTemplate Parent
		{
			get { return (StmNoteTemplate)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsPublished();
			ValidateIsAllCompanies();
		}
	}
}
