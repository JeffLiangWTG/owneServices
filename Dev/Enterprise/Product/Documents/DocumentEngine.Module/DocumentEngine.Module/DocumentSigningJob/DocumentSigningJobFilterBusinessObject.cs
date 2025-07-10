using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Module
{
	internal class DocumentSigningJobFilterBusinessObject : PrintJobFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AmendSignByFilter(filters);
			return filters;
		}

		void AmendSignByFilter(ModuleFilterCollection filters)
		{
			var filter = filters[SignByFilterDescription] as ModuleTextFilter;
			if (filter != null)
			{
				filter.DefaultProperty = DocumentsSignBy.DOS;
				filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				if (Env.CurrentUser.IsController)
				{
					return filter;
				}

				filter.AddToFilter(StmPrintJobSchema.SP_GS_NKJobSubmittedBy, Env.CurrentUser.Initials);

				return filter;
			}
		}
	}
}
