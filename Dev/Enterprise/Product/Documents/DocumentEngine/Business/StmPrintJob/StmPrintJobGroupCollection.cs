using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	/// <summary>
	/// A collection of StmPrintJobMergedCollections
	/// </summary>
	public class StmPrintJobGroupCollection : CollectionBase
	{
		/// <summary>
		/// Groups the collection of print jobs into StmPrintJobMergedCollections. Each Merged Collection goes to one uniquie recipient (e.g. bob@bob.com or fax# 9025 1199)
		/// </summary>
		public static StmPrintJobGroupCollection MergePrintJobsByRecipient(IEnumerable<StmPrintJob> printJobs)
		{
			if (printJobs == null)
			{ throw new ArgumentNullException(nameof(printJobs), "Argument printJobs must not be null."); }

			StmPrintJobGroupCollection result = new StmPrintJobGroupCollection();

			foreach (StmPrintJob printJob in printJobs)
			{
				if (printJob == null)
				{ throw new NullReferenceException("The printJob must not be null."); }
				if (printJob.Factory == null)
				{ throw new NullReferenceException("The factory of the printJob must not be null."); }

				// for now, create a new print group if they don't have one (backwards compatibility)
				if (printJob.SP_SB_DeliveryGroup.IsEmpty)
				{
					StmDeliveryGroup deliveryGroup = printJob.Factory.New<StmDeliveryGroup>() ?? throw new NullReferenceException("The factory of the printJob should not return a null deliveryGroup.");
					printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
				}

				StmPrintJobMergedCollection mergedPrintJobs = result.GetMergedPrintCollection(printJob);
				if (mergedPrintJobs != null)
				{
					mergedPrintJobs.Add(printJob);
				}
				else
				{
					result.AddNewMergedPrintCollection(printJob);
				}
			}

			return result;
		}

		public StmPrintJobGroupCollection()
		{
		}

		public StmPrintJobMergedCollection this[int index]
		{
			get { return ((StmPrintJobMergedCollection)List[index]); }
		}

		/// <summary>
		/// Will return the appropriate collection to merge if it exists.
		/// If it doesn't exist, a new one will be created
		/// </summary>
		/// <param name="printJob"></param>
		/// <returns></returns>
		public StmPrintJobMergedCollection GetMergedPrintCollection(StmPrintJob printJob)
		{
			foreach (StmPrintJobMergedCollection collection in List)
			{
				if (collection.Count > 0 && collection[0].IsInSameMergedJob(printJob))
				{
					return collection;
				}
			}
			return null;
		}

		/// <summary>
		/// Creates a new StmPrintJobMergedCollection to add to this this GroupCollection.
		/// The new StmPrintJobMergedCollection contains the print job passed in. 
		/// MAKE SURE NO OTHER SUITABLE MERGED COLLECTIONS EXIST BEFORE CALLING THIS METHOD.
		/// </summary>
		public StmPrintJobMergedCollection AddNewMergedPrintCollection(StmPrintJob printJob)
		{
			StmPrintJobMergedCollection returnCollection = new StmPrintJobMergedCollection(printJob.Factory);
			returnCollection.Add(printJob);
			Add(returnCollection);
			return returnCollection;
		}

		public int Add(StmPrintJobMergedCollection value)
		{
			return List.Add(value);
		}

		public int IndexOf(StmPrintJobMergedCollection value)
		{
			return List.IndexOf(value);
		}

		public void Remove(StmPrintJobMergedCollection value)
		{
			List.Remove(value);
		}

		public bool Contains(StmPrintJobMergedCollection value)
		{
			return List.Contains(value);
		}

		public StmPrintJobMergedCollection GetCollectionToFit(ZInt sizeToAdd, long sizeLimit)
		{
			foreach (StmPrintJobMergedCollection collection in List)
			{
				if ((long)collection.SizeInKB + sizeToAdd < sizeLimit)
				{
					return collection;
				}
			}
			return null;
		}
	}
}
