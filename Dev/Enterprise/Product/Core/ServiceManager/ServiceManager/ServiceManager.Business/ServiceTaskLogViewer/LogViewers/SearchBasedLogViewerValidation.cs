using System;
using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Business
{
	public class SearchBasedLogViewerValidation : ZValidation
	{
		public SearchBasedLogViewerValidation(SearchBasedLogViewer parent)
			: base(parent)
		{
			this.parent = parent;
			zValidationInternals = this;
			parentListInternals = parent;
		}

		public override void ValidateAll()
		{
			using (parentListInternals.SuspendListChanged())
			{
				ValidateServiceTaskCode();
			}
		}

		public override Type AutoValidationType
		{
			get
			{
				return typeof(SearchBasedLogViewerValidation);
			}
		}

		internal void ValidateServiceTaskCode()
		{
			zValidationInternals.Validate(
				parent.ServiceTaskCodeInfo,
				() =>
				{
					MandatoryValidation.CheckEntered(parent.ServiceTaskCodeInfo);
				});
		}

		readonly SearchBasedLogViewer parent;
		readonly ISingleElementListInternal parentListInternals;
		readonly IValidationInternals zValidationInternals;
	}
}

