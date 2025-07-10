using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class JobHeaderViewCollection : NonPersistentBusinessObjectCollection<JobHeaderView>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		internal JobHeaderViewCollection(BusinessObject[] scheduledEntities, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var scheduledEntity in scheduledEntities.OfType<IWorkflowProvider>())
			{
				factory.AddFetchHint(ProcessHeaderSchema.Instance, ProcessJobHeader.GetJobHeaderQuery(scheduledEntity));
			}

			foreach (var scheduledEntity in scheduledEntities)
			{
				Add(new JobHeaderView(scheduledEntity, factory));
			}
		}

		#region BusinessObjectCollection Overrides

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
	}
}
