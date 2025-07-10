using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ApplicableCustomisedLayoutCollection : NonPersistentBusinessObjectCollection<ApplicableCustomisedLayout>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ApplicableCustomisedLayoutCollection(BMComponentSectionConfiguration sectionConfig)
		{
			this.sectionConfig = sectionConfig;

			Build();
		}

		readonly BMComponentSectionConfiguration sectionConfig;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		internal void Reload()
		{
			RemoveAll();
			Build();
		}

		void Build()
		{
			foreach (var layout in BMControlCustomisation.GetAllPossibleCustomisedSummaryCardLinks(sectionConfig).Concat(BMControlCustomisation.GetAllPossibleCustomisedDetailedCardLinks(sectionConfig)))
			{
				Add(new ApplicableCustomisedLayout(layout));
			}
		}
	}
}
