using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class FilterStripValidation : ZValidation
	{
		public FilterStripValidation(FilterStrip parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidateFilterDescription

		public void ValidateFilterDescription()
		{
			ValidateCalculatedProperty(Parent.FilterDescriptionInfo);
		}

		protected virtual void CheckFilterDescription()
		{
			if (!Parent.IsFilterDescriptionEmpty)
			{
				if (Parent.ModuleFilters != null && Parent.CurrentModuleFilter != null && Parent.CurrentModuleFilter.IsSingleInstanceOnly &&
					Parent.ModuleFilters.Where(m => m.IsActive && m.OriginalCode == Parent.CurrentModuleFilter.OriginalCode).Take(2).Count() > 1)
				{
					Parent.FilterDescriptionInfo.AddError(Res.GetString("3c79de04-7dc4-4f74-a9df-494a61487652", "Only one instance of this filter can be used"));
				}

				ListValidation.ErrorIfInvalidCode(Parent.FilterDescriptionInfo, Parent.FilterDescriptionList);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateFilterDescription();
		}

		public void AddGlowResultWarningMessage(string message)
		{
			((IValidationInternals)this).Validate(Parent.FilterDescriptionInfo, () =>
			{
				Parent.FilterDescriptionInfo.AddWarning(message);
			});
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		protected readonly FilterStrip Parent;
	}
}
