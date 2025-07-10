using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public partial class PrintQueueReplaceBizo : NonPersistentBusinessObject<PrintQueueReplaceBizoValidation>, IObsoleteValidation
	{
		public readonly ZGuid PrintQueuePK;
		public readonly string DisplayName;

		public PrintQueueReplaceBizo(StmPrintQueue printQueue)
			: base(printQueue.Factory)
		{
			PrintQueuePK = printQueue.PK;
			DisplayName = printQueue.SQ_DisplayName;
		}

		[List("ReplacePrintQueue_List")]
		public ZGuid ReplacePrintQueuePK
		{
			get { return replacePrintQueuePK; }
			set
			{
				if (replacePrintQueuePK != value)
				{
					SetNonPersistentPropertyValue(ReplacePrintQueuePKInfo, ref replacePrintQueuePK, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateReplacePrintQueuePK();
					}
				}
			}
		}
		ZGuid replacePrintQueuePK;

		public ZPropertyInfo ReplacePrintQueuePKInfo
		{
			get { return this.GetZPropertyInfo(nameof(ReplacePrintQueuePK)); }
		}

		public StmPrintQueueCollection ReplacePrintQueue_List
		{
			get
			{
				if (replacePrintQueue_List == null)
				{
					var filter = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True);
					filter.AddToFilter(StmPrintQueueSchema.PK, SQLComparisonOperator.NotEqual, PrintQueuePK);
					replacePrintQueue_List = new StmPrintQueueCollection(Factory, filter);
					replacePrintQueue_List.Sort(new SortInfo(StmPrintQueueSchema.SQ_DisplayName.Name, ListSortDirection.Ascending));
				}
				return replacePrintQueue_List;
			}
		}
		StmPrintQueueCollection replacePrintQueue_List;

		#region Validation

		public override PrintQueueReplaceBizoValidation GetNewValidation()
		{
			return new PrintQueueReplaceBizoValidation(this);
		}

		#endregion
	}
}
