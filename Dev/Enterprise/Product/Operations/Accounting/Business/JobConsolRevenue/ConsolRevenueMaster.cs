using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	public class ConsolRevenueMaster : NonPersistentBusinessObject, IApportionedChargesHeaderList, IObsoleteValidation, ISecurityOverrideProviderSource, IDisposable
	{
		public ConsolRevenueMaster(IJobCostingPlugIn consol, BusinessObjectFactory factory)
			: base(factory)
		{
			fConsol = consol;
			JobsWithMutexes = new List<Job>();
		}

		internal IJobCostingPlugIn Consol
		{
			get { return fConsol; }
		}

		readonly IJobCostingPlugIn fConsol;

		public ConsolRevenueCollection Revenues
		{
			get
			{
				if (fRevenues == null)
				{
					fRevenues = new ConsolRevenueCollection(this, Factory);
					RegisterEditableChildObject(fRevenues);
				}
				return fRevenues;
			}
		}
		ConsolRevenueCollection fRevenues;

		#region IApportionedChargesHeaderList members

		IApportionedChargesHeader[] IApportionedChargesHeaderList.Headers
		{
			get { return Revenues.Cast<IApportionedChargesHeader>().ToArray(); }
		}

		#endregion

		#region ISecurityOverrideProviderSource Members

		ISecurityOverrideProvider ISecurityOverrideProviderSource.Provider
		{
			get
			{
				if (fProvider == null)
				{
					fProvider = new DefaultAccessSecurityProvider();
				}
				return fProvider;
			}
			set
			{
				fProvider = value;
			}
		}
		ISecurityOverrideProvider fProvider;

		#endregion

		public event EventHandler<UserMessageEventArgs> JobCreationExceptionEvent;
		readonly List<Job> JobsWithMutexes;

		public void ReleaseMutexesAndRaiseJobCreationExceptionEvent(string message)
		{
			ReleaseMutexes();
			JobCreationExceptionEvent?.Invoke(this, new UserMessageEventArgs(message));
		}

		public void OnJobWithMutexCreated(Job job)
		{
			JobsWithMutexes.Add(job);
		}

		public void ReleaseMutexes()
		{
			JobsWithMutexes.ForEach((job) => job.Dispose());
			if (JobsWithMutexes.Any())
			{
				Factory.SetContext(BusinessContext.ShouldSkipConsolRevenueApportionFormClosing);
			}
			JobsWithMutexes.Clear();
		}

		public void Dispose()
		{
			ReleaseMutexes();
		}
	}
}
