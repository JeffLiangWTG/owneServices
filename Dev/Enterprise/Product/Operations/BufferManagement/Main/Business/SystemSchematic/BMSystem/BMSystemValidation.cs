using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemValidation : AutoBMSystemValidation
	{
		public BMSystemValidation(AutoBMSystem parent)
			: base(parent)
		{
		}

		new BMSystem Parent
		{
			get { return (BMSystem)base.Parent; }
		}

		protected override void CheckFS_Name()
		{
			base.CheckFS_Name();
			MandatoryValidation.CheckEntered(Parent.FS_NameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FS_NameInfo, new BMSystemCollection(Parent.Factory));
		}

		protected override void CheckFS_Description()
		{
			base.CheckFS_Description();
			MandatoryValidation.CheckEntered(Parent.FS_DescriptionInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			CheckAllComponentLinks();
		}

		void CheckAllComponentLinks()
		{
			Parent.RemoveRowError(MinActiveComponentsError);
			var allComponents = Parent.Components.ToList();
			var allComponentsForValidation = allComponents.Where(c => c.FC_IsActive).ToList();
			allComponents.ForEach(c => c.ClearRowNotifications());

			if (allComponentsForValidation.Count < 2)
			{
				Parent.AddRowError(MinActiveComponentsError);
			}

			// Ensure ONLY ONE component has no links pointing to it ("start" component)
			var startComponents = allComponentsForValidation
				.Where(child => child.GetFromOthersToMeLinksWithinSystem(Parent).Count == 0).ToList();

			if (startComponents.Count != 1)
			{
				var componentsToAddErrors = startComponents.Any() ? startComponents : allComponents;
				componentsToAddErrors.ForEach(c => c.AddRowError(OneStartComponentError));
			}
		}

		static string MinActiveComponentsError
		{
			get { return Res.GetString("9b4c70ca-5136-44e1-a16d-b9488e3d3328", "There must be at least 2 active components in the system."); }
		}

		static string OneStartComponentError
		{
			get { return Res.GetString("7dfd9956-a756-4401-a59d-0d97e24d4d25", "There must be only one active 'start' component, without other components within the same system pointing to it."); }
		}
	}
}
