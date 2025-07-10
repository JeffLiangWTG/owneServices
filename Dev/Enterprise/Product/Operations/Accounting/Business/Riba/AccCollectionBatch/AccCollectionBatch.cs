using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Riba
{
	[CodeProperty(Schema.ACB_BatchNumber)]
	[DescriptionProperty("ACB_Description")]
	[UniversalDataContext(DataContextType.CollectionBatch)]
	[PreventDelete(false)]
	public class AccCollectionBatch : AutoAccCollectionBatch, IWorkflowProvider, IDocumentSupportable, IDocManagerSupport, IAccountingNumberFountainDataSource, IEDocsParsingSupport
	{
		public abstract new class Schema : AutoAccCollectionBatch.Schema
		{
			public const string ACB_RX_NKCurrency = "ACB_RX_NKCurrency";
		}

		public AccCollectionBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ACB_TotalAmount), ConcurrencyPolicy.Strict);
		}

		public ZString UniqueBatchID
		{
			get
			{
				return ACB_BatchNumber;
			}
		}

		AccCollectionOrderCollection fCollectionOrders;

		[ChildEditable(true)]
		public AccCollectionOrderCollection CollectionOrders
		{
			get
			{
				if (fCollectionOrders == null)
				{
					ZQuery query = new ZQuery(AccCollectionOrderSchema.ACO_ACB, PK);
					fCollectionOrders = new AccCollectionOrderCollection(Factory, query);

					foreach (var order in fCollectionOrders)
					{
						order.SetIncludeInBatchWithoutRecalculateBatchAmount(ZBool.True);
					}

					RegisterEditableChildObject(fCollectionOrders);
				}
				return fCollectionOrders;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public int Decimals => Currency != null ? Currency.Decimals : Company.GetLocalDecimals();

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public override ZDecimal ACB_TotalAmount
		{
			get { return base.ACB_TotalAmount; }
			set { base.ACB_TotalAmount = value; }
		}

		[ReadOnly(true)]
		public override ZString ACB_BatchNumber
		{
			get { return base.ACB_BatchNumber; }
			set { base.ACB_BatchNumber = value; }
		}

		public ZString ACB_Description
		{
			get { return Res.GetString("6379e007-6949-4bc2-9216-1932b9750535", "Collection Batch"); }	// should we add a new field for description?
		}

		[List("Lookups.ACB_CollectionFileFormat_List")]
		public override ZString ACB_CollectionFileFormat
		{
			get { return base.ACB_CollectionFileFormat; }
			set	{ base.ACB_CollectionFileFormat = value; }
		}

		[RelatedBusinessObject("BankAccount")]
		[List("Lookups.BankAccounts")]
		public override ZGuid ACB_AB
		{
			get { return base.ACB_AB; }
			set { base.ACB_AB = value; }
		}

		[RelatedBusinessObject("Company")]
		public override ZGuid ACB_GC
		{
			get { return base.ACB_GC; }
			set { base.ACB_GC = value; }
		}

		[ReadOnly(true)]
		[RelatedBusinessObject("Currency")]
		[List("Lookups.Currencies")]
		public ZString ACB_RX_NKCurrency
		{
			get
			{
				if (batchCurrency.IsEmpty && BankAccount != null)
				{
					batchCurrency = BankAccount.AB_RX_NKAccountCurrency;
				}
				return batchCurrency;
			}
			set
			{
				var hasChange = batchCurrency != value;
				batchCurrency = value;
				ACB_RX_NKCurrencyInfo.RefreshBinding();
				if (hasChange)
				{
					UpdateCollectionFormat();
				}
			}
		}
		ZString batchCurrency;

		public RefCurrency Currency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ACB_RX_NKCurrency); }
		}

		public ZPropertyInfo ACB_RX_NKCurrencyInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ACB_RX_NKCurrency); }
		}

		public ZInt ACB_Calc_RXDecimals
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, ACB_RX_NKCurrency);
				return currency != null ? currency.Decimals : 2;
			}
		}

		[List("Lookups.BatchTypes")]
		public override ZString ACB_Type
		{
			get { return base.ACB_Type; }
			set { base.ACB_Type = value; }
		}

		public ZPropertyInfo ACB_Calc_RXDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(ACB_Calc_RXDecimals)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			UpdateCollectionFormat();
		}

		void UpdateCollectionFormat()
		{
			if (GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion)
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Italy)
				{
					ACB_CollectionFileFormat = CollectionFileFormatList.Codes.ribaFormat;
				}
				else
				{
					UpdateCollectionFormatBasedOnCurrency();
				}
			}
			else
			{
				var countryCode = GlbCompany.CurrentCompany.Country.Code;
				switch (countryCode)
				{
					case Core.Constants.CountryCodes.Australia:
						ACB_CollectionFileFormat = CollectionFileFormatList.Codes.deFormat;
						break;
					case Core.Constants.CountryCodes.Brazil:
						ACB_CollectionFileFormat = CollectionFileFormatList.Codes.itauBank;
						break;
					case Core.Constants.CountryCodes.Iceland:
					case Core.Constants.CountryCodes.Liechtenstein:
					case Core.Constants.CountryCodes.Norway:
						{
							UpdateCollectionFormatBasedOnCurrency();
							break;
						}
					default:
						ACB_CollectionFileFormat = CollectionFileFormatList.Codes.unknown;
						break;
				}
			}
		}

		void UpdateCollectionFormatBasedOnCurrency()
		{
			if (ACB_RX_NKCurrency == Core.Constants.CurrencyCodes.EuropeanUnion)
			{
				ACB_CollectionFileFormat = CollectionFileFormatList.Codes.sepaFormat;
			}
			else
			{
				ACB_CollectionFileFormat = CollectionFileFormatList.Codes.unknown;
			}
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsCancelled && ACB_BatchNumber.IsEmpty)
			{
				AccountingNumberFountainWrapper wrapper = AccountingNumberFountainWrapperFactory.Instance.AccCollectionBatchNo;
				ACB_BatchNumber = wrapper.Generate(this);
			}
			int index = 1;
			foreach (var order in CollectionOrders)
			{
				if (order.IncludeInBatch)
				{
					if (order.ACO_OrderNumber.IsEmpty)
					{
					order.ACO_OrderNumber = index.ToString("D8");
					}
					index++;

					order.DeleteNotIncludedLines();
				}
				else
				{
					order.CollectionOrderLines.DeleteAll();
				}
			}
			foreach (var order in CollectionOrders.Where(order => !order.IncludeInBatch).ToArray())
			{
				order.Delete();
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				var fileGeneratorDataHelper = ObjectFactory.Get<IAccountingDependencyFactory>().GetCollectionBatchFileGeneratorProvider(this, Env.Instance)?.GetHelper();

				if (fileGeneratorDataHelper == null)
				{
					return;
				}

				((IDocManagerSupport)this).DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
				var fileData = fileGeneratorDataHelper.GetFileData();
				fileGeneratorDataHelper.AttachFileToEdoc(fileData);
			}
		}

		#region IAccountingNumberFountainDataSource members

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => ACB_SystemCreateTimeUtc;

		GlbBranch IAccountingNumberFountainDataSource.Branch => GlbBranch.CurrentBranch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => GlbDepartment.CurrentDepartment;

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool CreateReceiptsAndDepositBatch(out ZString errorMessage, ZDateTime backPostDate, ZDateTime backInvoiceDate, bool createDepositBatchPerOrder)
		{
			errorMessage = ZString.Empty;
			var newDepositBatchCreated = false;
			var needRollback = false;
			var listReceipts = new List<(ZGuid receiptPK, ZDateTime postDate, ZDateTime invoiceDate)>();
			var newFactory = new BusinessObjectFactory();

			using (var transactionManager = ((IDbConnected)newFactory).Connection.BeginTransactionWithManager())
			{
				try
				{
					foreach (var order in CollectionOrders)
					{
						if (!order.IsCancelled)
						{
							try
							{
								ARReceipt receipt;

								if (createDepositBatchPerOrder)
								{
									receipt = order.CreateReceiptsAndDepositBatch(newFactory, backPostDate, backInvoiceDate, false, true);
								}
								else
								{
									receipt = order.CreateReceiptsAndDepositBatch(newFactory, backPostDate, backInvoiceDate, true, true);
								}

								if (receipt != null)
								{
									listReceipts.Add((receipt.PK, receipt.AH_PostDate, receipt.AH_InvoiceDate));
								}
							}
							catch (OrderIsAlreadyCancelledOrPaidException)
							{
								continue;   // skip the current order and continue with the rest orders
							}
						}
					}
					if (listReceipts.Count > 0)
					{
						if (!createDepositBatchPerOrder)
						{
							var groupedReceipts = listReceipts.GroupBy(x => new { x.postDate, x.invoiceDate });
							foreach (var groupedReceipt in groupedReceipts)
							{
								var depositBatch = newFactory.New<DepositBatch>();
								depositBatch.AH_PostDate = groupedReceipt.Key.postDate;
								depositBatch.AH_InvoiceDate = groupedReceipt.Key.invoiceDate;
								depositBatch.AH_AB = ACB_AB;
								depositBatch.LoadTransactions(groupedReceipt.Select(x => x.receiptPK).ToList());  // load all receipts into the deposit batch
							}
						}
						newFactory.Save();
						newDepositBatchCreated = true;
						return true;
					}
					else
					{
						needRollback = true;
						errorMessage = Res.GetString("487910f3-682d-4c1e-9af4-22b595a4970b", "No receipt is created because no suitable orders can be found.");
						return false;
					}
				}
				catch (ReceiptMatchingProcessFailedException ex)
				{
					needRollback = true;    // if receipt matching failed, then need to roll back, include other receipts as well.
					errorMessage = ex.UserFriendlyMessage;
					return false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					needRollback = true;
					errorMessage = ex.Message;
					return false;
				}
				finally
				{
					if (!needRollback)
					{
						transactionManager.CommitTransaction();
						if (newDepositBatchCreated) // save logs in its own factory, if new deposit batch is created
						{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Receipt and Deposit Batch created for Batch Number {0}.", ACB_BatchNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							Logs.Factory.Save();
						}
					}
				}
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (ACB_IsCancelled)
			{
				ACB_TotalAmount = 0;
			}
			else
			{
				ACB_TotalAmount = 10m;
			}
		}

#endif

		public DocumentSupporter DocumentSupporter
		{
			get { return new AccCollectionBatchDocumentSupporter(this); }
		}

		#region IWorkflowSupporter

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new AccCollectionBatchProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.CollectionBatchCode; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CollectionBatch);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
	}
}

