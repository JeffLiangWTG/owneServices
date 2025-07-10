using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceSelectableJobCollection : NonPersistentBusinessObjectCollection<PeriodicInvoiceSelectableJob>, IIncludeInThePeriodicInvoiceChangedObserver
	{
		public PeriodicInvoiceSelectableJobCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Events

		public event EventHandler IncludeInThePeriodicInvoiceChanged;

		#endregion

		#region Base Override

		public override void Add(BusinessObject businessObject)
		{
			if (businessObject is Job job)
			{
				Add(job);
			}
			else
			{
				base.Add(businessObject);
			}
		}

		public PeriodicInvoiceSelectableJob Add(Job job)
		{
			var selectableJob = new PeriodicInvoiceSelectableJob(Factory, this, job);

			base.Add(selectableJob);

			return selectableJob;
		}

		public new bool Contains(BusinessObject businessObject)
		{
			if (businessObject is Job job)
			{
				return FindJob(job) != null;
			}

			return base.Contains(businessObject);
		}

		public PeriodicInvoiceSelectableJob FindJob(Job job)
		{
			return this.Cast<PeriodicInvoiceSelectableJob>().FirstOrDefault(sj => sj.Parent.PK == job.PK);
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			PeriodicInvoiceSelectableJob jobAdded = (PeriodicInvoiceSelectableJob)bizOAdded;
			var writableProperties = new string[] { jobAdded.IncludeInThePeriodicInvoiceInfo.Name };
			jobAdded.AddWritableProperties(writableProperties);
			if (useDefaultingLogic)
			{
				jobAdded.SetDefaultForIncludeInThePeriodicInvoice();
				jobAdded.SetCurrentValidationMode(currentValidationMode);
			}
			else
			{
				jobAdded.SetCurrentValidationMode(PeriodicInvoiceSelectableJob.ValidationMode.NoValidation);
				jobAdded.IncludeInThePeriodicInvoice = true;
			}
		}

		PeriodicInvoiceSelectableJob.ValidationMode currentValidationMode;
		bool includeInThePeriodicInvoice;
		internal void OnParentIncludeInThePeriodicInvoiceChanged(bool value)
		{
			if (useDefaultingLogic && includeInThePeriodicInvoice != value)
			{
				includeInThePeriodicInvoice = value;
				currentValidationMode = value ? PeriodicInvoiceSelectableJob.ValidationMode.FullValidation : PeriodicInvoiceSelectableJob.ValidationMode.EmptyValidation;
				foreach (var job in this)
				{
					((PeriodicInvoiceSelectableJob)job).SetCurrentValidationMode(currentValidationMode);
				}
			}
		}

		internal void UseDefaultingLogic()
		{
			useDefaultingLogic = true;
			currentValidationMode = PeriodicInvoiceSelectableJob.ValidationMode.WarningsOnly;
		}

		bool useDefaultingLogic;

		public override void Load()
		{
			throw new NotSupportedException();
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			Load(alternativeAdditionalFilter, string.Empty);
		}

		public void Load(ZQuery alternativeAdditionalFilter, string serviceDirection)
		{
			var jobs = Factory.Load<Job>(alternativeAdditionalFilter);

			PeriodicInvoiceSelectableJob[] sjobs = null;
			if (!string.IsNullOrEmpty(serviceDirection) && string.Compare(serviceDirection, "ALL", StringComparison.OrdinalIgnoreCase) != 0)
			{
				sjobs = jobs.Where(job => job.ServiceDirection == serviceDirection).Select(job => new PeriodicInvoiceSelectableJob(Factory, this, job)).ToArray();
			}
			else
			{
				sjobs = jobs.Select(job => new PeriodicInvoiceSelectableJob(Factory, this, job)).ToArray();
			}

			this.AddRange(sjobs);
		}

		#endregion

		public void Reload()
		{
			foreach (PeriodicInvoiceSelectableJob job in this)
			{
				job.Parent.Reload();
			}
		}

		public FunctionalitySuspender IncludeInThePeriodicInvoiceChangedSuspender
		{
			get { return includeInThePeriodicInvoiceChangedSuspender ?? (includeInThePeriodicInvoiceChangedSuspender = new FunctionalitySuspender(() => ((IIncludeInThePeriodicInvoiceChangedObserver)this).Notify(null), true)); }
		}
		FunctionalitySuspender includeInThePeriodicInvoiceChangedSuspender;

		void IIncludeInThePeriodicInvoiceChangedObserver.Notify(object sender)
		{
			if (IncludeInThePeriodicInvoiceChanged != null && !IncludeInThePeriodicInvoiceChangedSuspender.IsSuspended)
			{
				IncludeInThePeriodicInvoiceChanged(sender, EventArgs.Empty);
			}
		}
	}
}
