using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	[DependentBusinessObject(typeof(UPEPrintBatch), "PrintItems")]
	public class UPEPrintBatchItem : AutoClientPrintBatchItem
	{
		public UPEPrintBatchItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static void CascadeDeleteItems(BusinessObject parent)
		{
			ZQuery filter = new ZQuery(ClientPrintBatchItemSchema.T6_ParentID, parent.PK);
			UPEPrintBatchItem[] printItems = (UPEPrintBatchItem[])parent.Factory.Load(typeof(UPEPrintBatchItem), filter);
			foreach (UPEPrintBatchItem printItem in printItems)
			{
				printItem.Delete();
			}
		}

		public DocumentPack CreateDocumentPack()
		{
			DocumentCommand command = Factory.Load<DocumentCommand>(T6_SU);
			if (Parent == null)
			{
				ErrorReporter.ReportOnce("UPEPrintBatchItem.CreateDocumentPack", "Could not find business object for print batch item T6_ParentID='" + T6_ParentID + "' T6_SU='" + T6_SU);
			}
			return new DocumentPack(command, (IDocumentSupportable)Parent, null, null);
		}

		public ZString PrintItemQueuedMessage
		{
			get { return "The document has been queued for batch print (batch number " + PrintBatch.T7_BatchNumber + ")"; }
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public UPEPrintBatchItem LoadPrintBatchItem(ZString printBatchType, ZGuid parentPK, ZGuid documentCommandPK)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(ClientPrintBatchItemSchema.T6_ParentID, parentPK);
				filter.AddToFilter(ClientPrintBatchItemSchema.T6_SU, documentCommandPK);
				UPEPrintBatchItem[] printItems = (UPEPrintBatchItem[])Factory.Load(typeof(UPEPrintBatchItem), filter);
				return FindPrintBatchItemInMostRecentBatch(printBatchType, printItems);
			}

			UPEPrintBatchItem FindPrintBatchItemInMostRecentBatch(ZString printBatchType, UPEPrintBatchItem[] printItems)
			{
				int maxBatchNumber = 0;
				UPEPrintBatchItem result = null;

				foreach (UPEPrintBatchItem printItem in printItems)
				{
					if (printItem.PrintBatch.T7_BatchType == printBatchType &&
						printItem.PrintBatch.T7_BatchNumber > maxBatchNumber)
					{
						maxBatchNumber = printItem.PrintBatch.T7_BatchNumber;
						result = printItem;
					}
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(UPEPrintBatchItem);
			}
		}

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			T6_GS_QueuedBy = GlbStaff.CurrentUser.PK;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			T6_SU = new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK;
		}
#endif

		#endregion

		#region Related Business Objects

		public UPEPrintBatch PrintBatch
		{
			get { return (UPEPrintBatch)Factory.Load(typeof(UPEPrintBatch), T6_T7); }
		}

		public BusinessObject Parent
		{
			get
			{
				BusinessContext businessContext = (BusinessContext)Enum.Parse(typeof(BusinessContext), MenuItem.SU_BusinessContext);
				Type businessObjectType = GetParentBizObjType(businessContext, MenuItem);
				BusinessObject result = Factory.Load(businessObjectType, T6_ParentID);
				return result;
			}
		}

		#endregion

		#region Implementation

		Type GetParentBizObjType(BusinessContext businessContext, StmMenuItem menuItem)
		{
			Type result;
			if (businessContext == BusinessContext.Organisation)
			{
				result = typeof(OrgHeader);
			}
			else if (businessContext == BusinessContext.CusHAWB)
			{
				result = menuItem.SU_FilterList.Contains("UPECallout", StringComparison.OrdinalIgnoreCase) ? typeof(Callout) : typeof(UPECusHAWB);
			}
			else if (businessContext == BusinessContext.Customs)
			{
				result = typeof(UPEJobDeclaration);
			}
			else
			{
				throw new ArgumentException("Unable to handle BusinessContext " + businessContext);
			}
			return result;
		}

		#endregion
	}
}
