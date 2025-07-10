//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMailDBItemTemplateValidation
//
//    This class should be used for overriding validation in AutoMailDBItemTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Res = MailManager.Res;

namespace Enterprise.MailManager.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class MailDBItemTemplateValidation : AutoMailDBItemTemplateValidation
	{
		public MailDBItemTemplateValidation(AutoMailDBItemTemplate parent) : base(parent)
		{
		}

		public new MailItemTemplate Parent
		{
			get { return (MailItemTemplate)base.Parent; }
		}

		protected override void CheckMIT_Name()
		{
			base.CheckMIT_Name();
			MandatoryValidation.CheckEntered(Parent.MIT_NameInfo);
		}

		protected override void CheckMIT_Category()
		{
			base.CheckMIT_Category();
			MandatoryValidation.CheckEntered(Parent.MIT_CategoryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MIT_CategoryInfo);
		}

		public void ValidateTemplateID()
		{
			ValidateCalculatedProperty(Parent.TemplateIDInfo);
		}

		protected virtual void CheckTemplateID()
		{
			if (!Parent.TemplateID.IsEmpty)
			{
				var filter = new ZQuery(MailDBItemTemplateSchema.MIT_Name, Parent.MIT_Name);
				filter.AddToFilter(MailDBItemTemplateSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(MailDBItemTemplateSchema.MIT_Category, SQLComparisonOperator.Equal, Parent.MIT_Category);

				bool alreadyExists = Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(MailItemTemplate)), filter);
				if (alreadyExists)
				{
					Parent.TemplateIDInfo.AddError(Res.GetString("E2ED8842-78AC-4062-8108-D7D501F2B557", "This Name has already been used on another template in this Category. Please specify a different Name."));
				}
			}
		}

		protected override void CheckMIT_GB_Branch()
		{
			base.CheckMIT_GB_Branch();
			if (!Parent.MIT_GC_Company.IsEmpty && !Parent.MIT_GB_Branch.IsEmpty)
			{
				if (Parent.Branch != null && Parent.Branch.GB_GC != Parent.MIT_GC_Company)
				{
					Parent.MIT_GB_BranchInfo.AddError(Res.GetString("34E04F10-6B9C-44B5-A348-4A91EEAE2EE2", "The selected branch doesn't belong to the specified company."));
				}
			}
		}

		protected override void CheckMIT_Language()
		{
			base.CheckMIT_Language();
			MandatoryValidation.CheckEntered(Parent.MIT_LanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MIT_LanguageInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTemplateID();
			ValidateMIT_Category();
		}
	}
}
