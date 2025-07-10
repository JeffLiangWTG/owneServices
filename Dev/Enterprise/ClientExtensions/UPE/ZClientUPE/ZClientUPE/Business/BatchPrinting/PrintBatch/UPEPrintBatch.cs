using System;
using System.Collections;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPrintBatch : AutoClientPrintBatch
	{
		public UPEPrintBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: this(factory, typeof(UPEPrintBatch))
			{
			}

			protected Loader(BusinessObjectFactory factory, Type typeOfPrintBatch)
				: base(factory)
			{
				this.TypeOfPrintBatch = typeOfPrintBatch;
			}

			#region CreateOrLoadLatestBatch

			public UPEPrintBatch CreateOrLoadLatestBatch(ZString batchType)
			{
				UPEPrintBatch result;
				try
				{
					result = TryCreateOrLoadLatestBatch(batchType);
				}
				catch (ZSaveException ex)
				{
					if (ex.IndexNameIfUniqueIndexViolation == "NR_UX__T7_BatchType_T7_BatchNumber")
					{
						Factory.ClearQueryCache(ClientPrintBatchSchema.Constants.TableName);
						result = TryCreateOrLoadLatestBatch(batchType);
					}
					else
					{
						throw;
					}
				}
				return result;
			}

			UPEPrintBatch TryCreateOrLoadLatestBatch(ZString batchType)
			{
				ZQuery filter = GetLatestBatchFilter(batchType);
				UPEPrintBatch result = (UPEPrintBatch)Factory.LoadTop1(TypeOfPrintBatch, filter);

				int nextBatchNumber = (result == null) ? 1 : (int)result.T7_BatchNumber + 1;
				if (result == null || result.NumberOfPrintItems >= PrintBatchMaxCount || result.IsPrinted)
				{
					result = CreateNewSavedPrintBatch(batchType, nextBatchNumber);
				}
				return result;
			}

			protected UPEPrintBatch TryCreateOrLoadLatestBatchForTest(ZString batchType)
			{
				return TryCreateOrLoadLatestBatch(batchType);
			}

			protected virtual int PrintBatchMaxCount
			{
				get { return UPEDataRegistry.Instance.PrintBatchMaxCount.Value; }
			}

			ZQuery GetLatestBatchFilter(ZString batchType)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(ClientPrintBatchSchema.T7_BatchType, batchType);
				filter.OrderBy = ClientPrintBatchSchema.T7_BatchNumber.Name + " DESC";
				return filter;
			}

			UPEPrintBatch CreateNewSavedPrintBatch(ZString batchType, int batchNumber)
			{
				IDbConnected dbConnected = Factory;
				BusinessObjectFactory newFactory = new BusinessObjectFactory(dbConnected.Connection);
				UPEPrintBatch newPrintBatch = (UPEPrintBatch)newFactory.New(TypeOfPrintBatch);
				newPrintBatch.T7_BatchType = batchType;
				newPrintBatch.T7_BatchNumber = batchNumber;

				newFactory.Save();
				UPEPrintBatch result = (UPEPrintBatch)Factory.Load(TypeOfPrintBatch, newPrintBatch.PK);
				return result;
			}

			#endregion

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(UPEPrintBatch);
			}

			readonly Type TypeOfPrintBatch;
		}

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			PrintItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region New Properties

		#region IsPrinted

		public ZBool IsPrinted
		{
			get { return T7_LastPrintedDate.IsValid; }
		}

		public ZPropertyInfo IsPrintedInfo
		{
			get { return GetZPropertyInfo(nameof(IsPrinted)); }
		}

		#endregion

		#region NumberOfPrintItems

		public ZInt NumberOfPrintItems
		{
			get { return (fPrintItems != null) ? PrintItems.Count : (int)NumberOfPrintItemsFromDatabaseCached; }
		}

		public ZPropertyInfo NumberOfPrintItemsInfo
		{
			get { return GetZPropertyInfo(nameof(NumberOfPrintItems)); }
		}

		ZInt NumberOfPrintItemsFromDatabaseCached
		{
			get
			{
				if (fNumberOfPrintItemsFromDatabaseCached == -1)
				{
					ZQuery filter = new ZQuery(ClientPrintBatchItemSchema.T6_T7, PK);
					fNumberOfPrintItemsFromDatabaseCached = Factory.GetDatabaseCount(typeof(UPEPrintBatchItem), filter);
				}
				return fNumberOfPrintItemsFromDatabaseCached;
			}
		}
		int fNumberOfPrintItemsFromDatabaseCached = -1;

		#endregion

		#endregion

		#region Related Business Objects

		[ChildEditable(true)]
		public UPEPrintBatchItemCollection PrintItems
		{
			get
			{
				if (fPrintItems == null)
				{
					fPrintItems = new UPEPrintBatchItemCollection(this);
					fPrintItems.Load();

					IComparer sortOrder = UPEPrintBatchItemPrintOrderComparer.New(T7_BatchType);
					if (sortOrder != null)
					{
						fPrintItems.Sort(sortOrder);
					}
					RegisterEditableChildObject(fPrintItems);
				}
				return fPrintItems;
			}
		}
		UPEPrintBatchItemCollection fPrintItems;

		#endregion

		#region QueueForBatchPrintAndSave

		public UPEPrintBatchItem QueueForBatchPrintAndSave(IUPEDocumentSupportable documentSupportable, ZGuid menuItemPK)
		{
			return QueueForBatchPrintAndSave(documentSupportable, menuItemPK, true);
		}

		public UPEPrintBatchItem QueueForBatchPrintAndSave(IUPEDocumentSupportable documentSupportable, ZGuid menuItemPK, bool notifyUser)
		{
			UPEPrintBatchItem existingItem = new UPEPrintBatchItem.Loader(Factory).LoadPrintBatchItem(T7_BatchType, documentSupportable.Identifier, menuItemPK);
			if (existingItem != null)
			{
				existingItem.Delete();
			}

			UPEPrintBatchItem printItem = PrintItems.AddNew(menuItemPK, documentSupportable);
			Factory.Save();

			documentSupportable.OnPrintBatchItemQueued(new PrintBatchItemQueuedEventArgs(printItem, notifyUser));
			return printItem;
		}

		#endregion

		#region Print

		public virtual void Print()
		{
			using (PrintTask printTask = new PrintTask())
			{
				foreach (UPEPrintBatchItem printItem in PrintItems)
				{
					printTask.Add(printItem.CreateDocumentPack());
				}
				RunPrintTask(printTask);
			}

			T7_LastPrintedDate = ZDateTime.Now;
			T7_PrintCount++;
		}

		void RunPrintTask(PrintTask printTask)
		{
			DeliveryInstructions instructions = GetHardCopyDeliveryInstructions();
			printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, instructions, Env.Security.None);
		}

		protected virtual DeliveryInstructions GetHardCopyDeliveryInstructions()
		{
			return new DeliveryInstructions();
		}

		#endregion
	}
}
