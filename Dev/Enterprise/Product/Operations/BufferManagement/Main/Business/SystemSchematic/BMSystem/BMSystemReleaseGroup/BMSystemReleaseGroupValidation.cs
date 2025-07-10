using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemReleaseGroupValidation : AutoBMSystemReleaseGroupValidation
	{
		public BMSystemReleaseGroupValidation(AutoBMSystemReleaseGroup parent)
			: base(parent)
		{
		}

		new BMSystemReleaseGroup Parent => (BMSystemReleaseGroup)base.Parent;

		protected override void CheckFSG_GG_Group()
		{
			base.CheckFSG_GG_Group();

			MandatoryValidation.CheckEntered(Parent.FSG_GG_GroupInfo);
			ListValidation.ErrorIfInvalidPK(Parent.FSG_GG_GroupInfo);

			var system = Parent.BMSystem;

			if (system != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FSG_GG_GroupInfo, system.ReleaseGroups);
			}
		}
	}
}
