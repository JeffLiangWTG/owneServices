using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.DHL.Business
{
	public class FlightBulkUpdateProcessor
	{
		public FlightBulkUpdateProcessor(FlightBulkUpdateBusinessObject flightBulkUpdateBizO)
		{
			this.flightBulkUpdateBizO = flightBulkUpdateBizO;
		}

		public int BulkUpdate()
		{
			var factoryProvider = new JobDeclarationBusinessObjectFactoryProvider(this);
			var reader = new FilteredBusinessObjectReader(factoryProvider, flightBulkUpdateBizO.StandaloneJobDecFilter(flightBulkUpdateBizO.ExistingMAWB), typeof(JobDeclaration))
			{
				BatchSize = 50
			};
			int count = reader.ApproximateCount;
			reader.SaveBeforeLoadNextEnabled = true;

			int committedCount = 0;
			try
			{
				var lastJobDecPK = ZGuid.Empty;
				var jobDeclarations = reader.Cast<JobDeclaration>();
				if (jobDeclarations.Any())
				{
					lastJobDecPK = jobDeclarations.Last().PK;
				}

				int counter = 0;
				foreach (JobDeclaration jobDec in reader)
				{
					factoryProvider.AddJobDecsToUpdate(jobDec);
					counter++;
					FlightBulkUpdateProcessor_OnProgress(this, new DHLProgressEventArgs(counter, count, "Updating delcaration flight details..."));
					if (counter % reader.BatchSize == 0 || lastJobDecPK == jobDec.PK)
					{
						SaveCurrentAndCreateNew(factoryProvider);
						committedCount = counter;
					}
				}
			}
			catch (ZSaveConcurrencyException ex)
			{
				ConcurrencyErrorMessage = string.Format("{0}{1}{1}{2} out of {3} records have been updated.", new JobDeclarationConcurrencyExceptionHandler(ex).UserFriendlyMessage, System.Environment.NewLine,
					committedCount, count);
			}

			return committedCount;
		}

		public string ConcurrencyErrorMessage { get; set; }

		protected virtual void SaveCurrentAndCreateNew(JobDeclarationBusinessObjectFactoryProvider factoryProvider)
		{
			factoryProvider.SaveCurrentAndCreateNew();
		}

		class JobDeclarationConcurrencyExceptionHandler : ConcurrencyExceptionHandler
		{
			public JobDeclarationConcurrencyExceptionHandler(Exception e)
				: base(e)
			{ }

			public new string UserFriendlyMessage
			{
				get
				{
					return base.UserFriendlyMessage;
				}
			}
		}

		public event DHLProgressEventHandler OnProgress;

		#region JobDeclarationBusinessObjectFactoryProvider class

		protected class JobDeclarationBusinessObjectFactoryProvider : BusinessObjectFactoryProvider
		{
			public JobDeclarationBusinessObjectFactoryProvider(FlightBulkUpdateProcessor owner)
			{
				this.owner = owner;
			}

			public void AddJobDecsToUpdate(JobDeclaration jobDec)
			{
				JobDecs.Add(jobDec);
			}

			protected override void SaveCurrent()
			{
				owner.UpdateJobDeclarations(JobDecs);
				jobDecs = null;
				base.SaveCurrent();
			}

			List<JobDeclaration> JobDecs
			{
				get { return jobDecs ?? (jobDecs = new List<JobDeclaration>()); }
			}
			List<JobDeclaration> jobDecs;

			readonly FlightBulkUpdateProcessor owner;
		}

		#endregion

		internal void UpdateJobDeclarations(IEnumerable<JobDeclaration> jobDecs)
		{
			foreach (JobDeclaration jobDec in jobDecs)
			{
				UpdateJobDeclaration(jobDec);
			}
		}

		void UpdateJobDeclaration(JobDeclaration jobDec)
		{
			if (!flightBulkUpdateBizO.MAWB.IsEmpty)
			{
				jobDec.JE_MasterBill = flightBulkUpdateBizO.MAWB;
			}

			if (!flightBulkUpdateBizO.FlightNo.IsEmpty)
			{
				jobDec.JE_VoyageFlightNo = flightBulkUpdateBizO.FlightNo;
			}

			if (!flightBulkUpdateBizO.DepartureDate.IsEmpty)
			{
				jobDec.JE_ExportDate = flightBulkUpdateBizO.DepartureDate;
			}

			if (!flightBulkUpdateBizO.ArrivalDate.IsEmpty)
			{
				jobDec.JE_DateOfArrival = flightBulkUpdateBizO.ArrivalDate;
			}

			if (!flightBulkUpdateBizO.EDITransmitDate.IsEmpty)
			{
				jobDec.JE_EDITransmitDate = flightBulkUpdateBizO.EDITransmitDate;
			}
		}

		void FlightBulkUpdateProcessor_OnProgress(object sender, DHLProgressEventArgs e)
		{
			if (OnProgress != null)
			{
				OnProgress(this, e);
			}
		}

		readonly FlightBulkUpdateBusinessObject flightBulkUpdateBizO;
	}
}
