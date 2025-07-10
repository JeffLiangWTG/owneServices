using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ComponentRelationshipValidation : BMComponentValidation
	{
		public ComponentRelationshipValidation(AutoBMComponent parent)
			: base(parent)
		{
		}

		new ComponentRelationship Parent
		{
			get { return (ComponentRelationship)base.Parent; }
		}

		protected override void VerifyFC_NameIsUnique()
		{
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FC_NameInfo, Parent.Factory.Load<BMComponent>(new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.ComponentRelationship)));
		}

		protected override void CheckFC_FS_System()
		{
			MandatoryValidation.CheckNotEntered(Parent.FC_FS_SystemInfo);
		}

		public void ValidateRelatedComponentLinks()
		{
			Parent.ToggleRowErrorSafe(Parent.RelatedComponentLinks.Count == 0, Res.GetString("9679d625-e2c9-442f-8c32-42591b77cf3f", "Component relationships cannot be empty. Please add a component to the relationship"));
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateRelatedComponentLinks();
		}
	}
}
