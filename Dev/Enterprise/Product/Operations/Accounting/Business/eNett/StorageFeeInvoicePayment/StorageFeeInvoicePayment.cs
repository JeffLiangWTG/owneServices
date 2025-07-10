using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.eNett;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.eNett_Integration
{
	public class StorageFeeInvoicePayment : AutoStorageFeeInvoicePayment, IStmALogParent, IApportionedChargesHeader, IDisposable
	{
		public StorageFeeInvoicePayment(IContainerStorageDataProvider container)
			: base(new BusinessObjectFactory())
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}
			this.Container = container;
		}

		Dictionary<ZGuid, Job> JobsCreatedOrLoaded;

		public void Initialise()
		{
			PickupDate = Container.PickupDate.IsValid ? Container.PickupDate : ZDateTime.Empty;
			ChargeCode = AccountingConfigurationRegistry.Instance.ENettStoragePaymentChargeCode.Value;
			ApportionmentMethod = "SHP";

			Invoice.SubmittedFromInvoicingForm = true;
			Invoice.IsInvoiceReceiptPayment = true;
			Invoice.ValidateExpectedInvoiceTotal = false;
			Invoice.AH_TransactionNum = Container.ContainerNumber;
			Invoice.AH_Desc = Res.GetString("6e9c76b1-86cc-48de-a3d1-3ac465d29e01", "{0} Container Storage Charges", Container.ContainerNumber);

			Invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			Invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			Invoice.ReceiptPaymentAH_ChequeOrReference = Container.ContainerNumber;
			Invoice.SetAH_OH_Readonly(true);
			Invoice.SetReceiptPaymentAH_ReceiptTypeReadOnly(true);
			Invoice.CanSetIsInvoiceReceiptPayment = false;
			Invoice.ReceiptPaymentAH_AB = GetDefaultBankAccount(AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.Value);

			JobsCreatedOrLoaded = new Dictionary<ZGuid, Job>();

			foreach (IJobInvoicingPlugIn operationsJob in Container.OperationsJobs)
			{
				var loader = new Job.Loader(Factory, operationsJob);
				var jobHeader = loader.TryLoadOrCreateWithMutex();
				var line = (APInvoiceLine)Invoice.Lines.AddNew();
				if (jobHeader == null)
				{
					var errorMessage = loader.GetJobCreationError().Message;
					line.SetCreatingJobHeaderError(errorMessage);
				}
				else
				{
					JobsCreatedOrLoaded.Add(jobHeader.PK, jobHeader);
					line.AL_JH = jobHeader.PK;
				}
			}
		}

		ZGuid GetDefaultBankAccount(ENettRegisteredBankAccountCollection accounts)
		{
			ZGuid result = ZGuid.Empty;

			if (accounts != null && accounts.Count > 0)
			{
				List<ZGuid> localCurrencyENettRegisteredBankAccounts = new List<ZGuid>();

				foreach (ENettRegisteredBankAccount eNettAccount in accounts)
				{
					if (eNettAccount.BankAccountCurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						localCurrencyENettRegisteredBankAccounts.Add(eNettAccount.BankAccountPK);
					}
				}

				if (localCurrencyENettRegisteredBankAccounts.Count == 1)
				{
					result = localCurrencyENettRegisteredBankAccounts[0];
				}
			}

			return result;
		}

		readonly IContainerStorageDataProvider Container;

		public APInvoice Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = Factory.New<APInvoice>();
					RegisterEditableChildObject(fInvoice);
				}

				return fInvoice;
			}
		}
		APInvoice fInvoice;

		public ComPayRegisteredOrganisationDataSource OrgSelectionDataSource
		{
			get
			{
				if (fOrgSelectionDataSource == null)
				{
					fOrgSelectionDataSource = new ComPayRegisteredOrganisationDataSource(new BusinessObjectFactory());
					fOrgSelectionDataSource.OnlyShowOrgsWithTerminalCode = true;
				}
				return fOrgSelectionDataSource;
			}
		}
		public ComPayRegisteredOrganisationDataSource fOrgSelectionDataSource;

		#region Web Service

		public eNettResponseWithMessages GetStorageFeeFromWebService()
		{
			eNettResponseWithMessages result;

			Validation.ValidatePortCode();
			Validation.ValidatePickupDate();

			if (!PortCodeInfo.HasErrors() && !PickupDateInfo.HasErrors())
			{
				IeNettWebServiceWrapper eNettWebServiceWrapper = ObjectFactory.Get<IeNettWebServiceWrapper>();
				result = eNettWebServiceWrapper.GetContainerStorageFee(this);
			}
			else
			{
				List<string> messages = new List<string>();

				if (PortCodeInfo.HasErrors())
				{
					messages.Add(Res.GetString("670b187e-b021-4226-8306-a3689d3faabe", "Please enter a {0}", PortCodeInfo.HumanReadableName));
				}

				if (PickupDateInfo.HasErrors())
				{
					messages.Add(Res.GetString("670b187e-b021-4226-8306-a3689d3faabe", "Please enter a {0}", PickupDateInfo.HumanReadableName));
				}

				result = new eNettResponseWithMessages(false, messages.ToArray());
			}

			return result;
		}

		#endregion

		#region Properties

		public AccChargeCode ChargeCodeBizObj
		{
			get { return Factory.Load<AccChargeCode>(ChargeCode); }
		}

		public override ZString ContainerNumber
		{
			get { return Container.ContainerNumber; }
		}

		[ReadOnly(true)]
		[List("Lookups.Currencies")]
		public override ZString LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		RefCurrency LocalCurrencyBizObj
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, LocalCurrency); }
		}

		[List("Lookups.ChargeCodes")]
		public override ZGuid ChargeCode
		{
			get { return base.ChargeCode; }
			set
			{
				base.ChargeCode = value;

				if (value.IsValid)
				{
					Invoice.DefaultChargeCodeForLines = value;

					foreach (InvoicingLineBase line in Invoice.Lines)
					{
						line.GenericCharge = value;
					}
				}
			}
		}

		[List("Lookups.ApportionmentMethods")]
		public override ZString ApportionmentMethod
		{
			get { return base.ApportionmentMethod; }
			set
			{
				var originalValue = ApportionmentMethod;
				base.ApportionmentMethod = value;

				if (value != originalValue)
				{
					ApportionToLines();
				}
			}
		}

		void ApportionToLines()
		{
			ApportionmentCreator.Apportion(this, "GSTInclusiveAmount", StorageCharges, ZString.Empty, ZDecimal.Zero);

			foreach (APInvoiceLine line in ((IApportionedChargesHeader)this).Charges)
			{
				line.SplitGSTInclusiveAmount();
			}

			Invoice.RefreshBindingIncludingChildren();
		}

		#region Storage Charges

		[ReadOnly(true)]
		public override ZDecimal StorageCharges
		{
			get { return base.StorageCharges; }
			set
			{
				bool valueHasChanged = StorageCharges != value;
				base.StorageCharges = value;
				Invoice.ExpectedInvoiceTotal = value;
				StorageChargesInfo.RefreshBinding();
				if (valueHasChanged && value != ZDecimal.Zero)
				{
					ApportionToLines();
				}
			}
		}

		public int StorageChargesDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		#endregion

		#endregion

		#region Lookups

		public StorageFeeInvoicePaymentLookups Lookups
		{
			get { return lookups ?? (lookups = new StorageFeeInvoicePaymentLookups(this)); }
		}
		StorageFeeInvoicePaymentLookups lookups;

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZString logMessage = string.Format((NoResString)"Paid Storage Charges {0}{1}", LocalCurrencyBizObj.RX_Symbol, StorageCharges.ToString(LocalCurrencyBizObj.Decimals));
			StmALog editLog = Container.CreateEditLog(logMessage);
			newFactory.ImportFromAnotherFactory(editLog);
			newFactory.Save();

			Invoice.ContainerDetailsForCompayExport = new APInvoice.ContainerDetailsForInvoiceExport(ContainerNumber, PortCode, PickupDate);

			foreach (APInvoiceLine line in Invoice.Lines)
			{
				if (JobsCreatedOrLoaded.ContainsKey(line.AL_JH))
				{
					JobsCreatedOrLoaded.Remove(line.AL_JH);
				}
			}

			foreach (Job job in JobsCreatedOrLoaded.Values.Where(x => !x.IsInDatabase))
			{
				job.Delete();
			}

			JobsCreatedOrLoaded.Clear();
		}

		#region IApportionedChargesHeader

		IApportionedCharge[] IApportionedChargesHeader.Charges
		{
			get
			{
				List<APInvoiceLine> lines = new List<APInvoiceLine>();

				foreach (APInvoiceLine line in Invoice.Lines)
				{
					if (line.Job != null)
					{
						lines.Add(line);
					}
				}

				return lines.ToArray();
			}
		}

		AccChargeCode IApportionedChargesHeader.ChargeCode
		{
			get { return ChargeCodeBizObj; }
		}

		RefCurrency IApportionedChargesHeader.Currency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency; }
		}

		bool IApportionedChargesHeader.IsChargeReadyToPost(IApportionedCharge charge)
		{
			return ((APInvoiceLine)charge).AL_LineAmount != 0M;
		}

		ZDecimal IApportionedChargesHeader.FreeSpace => ZDecimal.Zero;

		#endregion

		#region IStmALogParent Members

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return Invoice.BusinessObjectsWithRelatedEvents; }
		}

		Logs IStmALogProvider.Logs
		{
			get { return Invoice.Logs; }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Invoice.Factory; }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return Invoice.PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return Invoice.TableName; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		public static ZGlobalMutex GetCOMPayMutex(ZGuid paymentPK)
		{
			return new ZGlobalMutex(MutexIDs.COMPayDirectDebitOperation, paymentPK + "_COMPay");
		}

		public void Dispose()
		{
			if (JobsCreatedOrLoaded != null)
			{
				foreach (var job in JobsCreatedOrLoaded.Values)
				{
					job.Dispose();
				}
			}
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.eNett_Integration.Testing
{
	using Enterprise.ZArchitecture.Business;

	public class MockContainerStorageDataProvider : IContainerStorageDataProvider
	{
		#region IContainerStorageDataProvider Members

		public ZString ContainerNumber
		{
			get { return "ACBD1234567"; }
		}

		public IJobInvoicingPlugIn[] OperationsJobs
		{
			get { return Array.Empty<IJobInvoicingPlugIn>(); }
		}

		public ZString PortCode
		{
			get { return "AUSYD"; }
		}

		public ZDateTime PickupDate
		{
			get { return ZDateTime.Now.AddDays(3); }
		}

		StmALog IContainerStorageDataProvider.CreateEditLog(string reference)
		{
			return null;
		}

		#endregion
	}
}

#endif
#endregion
