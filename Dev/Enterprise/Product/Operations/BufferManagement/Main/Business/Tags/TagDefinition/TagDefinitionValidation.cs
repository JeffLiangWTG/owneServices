using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TagDefinitionValidation : AutoTagDefinitionValidation
	{
		public TagDefinitionValidation(AutoTagDefinition parent)
			: base(parent)
		{
		}

		new TagDefinition Parent
		{
			get { return (TagDefinition)base.Parent; }
		}

		protected override void CheckTGD_Code()
		{
			base.CheckTGD_Code();
			MandatoryValidation.CheckEntered(Parent.TGD_CodeInfo);

			if (!Parent.TGD_IsSystem && !Parent.TGD_Code.IsEmpty)
			{
				var query = new ZQuery(TagDefinitionSchema.TGD_Code, Parent.TGD_Code);
				query.AddToFilter(TagDefinitionSchema.TGD_IsSystem, true);

				if (Parent.Factory.Exists(typeof(TagDefinition), query, mergeDbAndCacheResult: false))
				{
					Parent.TGD_CodeInfo.AddError(Res.GetString("678838e2-27ac-4e9c-bf1e-fe668e9c667e", "This code conflicts with a system-defined Tag Group."));
				}
			}

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.TGD_CodeInfo, new TagDefinitionCollection(Parent.Factory));
		}

		protected override void CheckTGD_Description()
		{
			base.CheckTGD_Description();
			MandatoryValidation.CheckEntered(Parent.TGD_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.TGD_DescriptionInfo);
		}

		protected override void CheckTGD_Scope()
		{
			base.CheckTGD_UsageScope();

			if (!Parent.ApplicableToWorkflows && !Parent.CanUserUseTags)
			{
				Parent.TGD_ScopeInfo.AddError(Res.GetString("25e9cd2c-ca6f-4672-adb4-d8df86564475", "Tag Rules can only apply tags to workflows", Parent.TGD_UsageScope));
			}
		}
	}
}
