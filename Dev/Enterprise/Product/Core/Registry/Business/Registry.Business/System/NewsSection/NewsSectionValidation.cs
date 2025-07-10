using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class NewsSectionValidation : ZValidation
	{
		public NewsSectionValidation(NewsSection parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly NewsSection parent;

		#region SectionID

		public void ValidateSectionID()
		{
			ValidateCalculatedProperty(parent.SectionIDInfo);
		}

		protected void CheckSectionID()
		{
			ListValidation.ErrorIfInvalidCode(parent.SectionIDInfo);
		}

		#endregion

		public override System.Type AutoValidationType
		{
			get { return typeof(NewsSectionValidation); }
		}

		public override void ValidateAll()
		{
			ValidateSectionID();
		}
	}
}
