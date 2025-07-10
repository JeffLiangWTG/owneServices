//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuDocumentConfigItemValidation
//
//    This class should be used for overriding validation in AutoStmMenuDocumentConfigItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDocumentConfigItemValidation : AutoStmMenuDocumentConfigItemValidation
	{
		public StmMenuDocumentConfigItemValidation(AutoStmMenuDocumentConfigItem parent)
			: base(parent)
		{
		}

		protected override void CheckS4_SectionType()
		{
			base.CheckS4_SectionType();
			var configItem = Parent as StmMenuDocumentConfigItem;
			if (configItem != null)
			{
				if ((configItem.IsGenericSectionType && !configItem.Lookups.GenericSectionTypes.ContainsCode(configItem.S4_SectionType)) ||
					(!configItem.IsGenericSectionType && !configItem.Lookups.LegacySectionTypes.ContainsCode(configItem.S4_SectionType)))
				{
					Parent.S4_SectionTypeInfo.AddError(Res.GetString("446d33b9-1470-422a-910a-a63bbf631b60", "Invalid Section Type."));
				}
			}
		}
	}
}
