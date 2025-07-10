using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class MockCustomsJobProvider : DummyBusinessObject, ICustomsJobInfo, IBusiness, ICustomsJobInfoProvider, IJobInvoicingPlugInAdditionalJobs, IAccIntegrationDataProvider, IRatingSupporterWithAdapter
	{
		public MockCustomsJobProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fBranch = GlbBranch.CurrentBranch;
			((MockCustomsJobProviderInvoicingSupporter)InvoicingSupporter).TransportMode = Constants.TransportModes.Sea;
			((MockCustomsJobProviderInvoicingSupporter)InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			fTopLevelObjectForJobToReference = this;
			isImport = false;
		}

		public IJobInvoicingPlugIn fTopLevelObjectForJobToReference;
		public IJobInvoicingPlugIn TopLevelObjectForJobToReference
		{
			get { return fTopLevelObjectForJobToReference; }
		}

		public ZString IncoTermExposed;

		#region IAutoRatingAndJobInvoicing Members

		public GlbBranch fBranch;
		public GlbBranch Branch
		{
			get
			{
				return fBranch;
			}
		}

		#endregion

		#region IJobHeaderParent

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		public void SetJobNumberFieldOnSaving()
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		MockCustomsJobProviderInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new MockCustomsJobProviderInvoicingSupporter(this)); }
		}

		#endregion

		#region ICustomsJobInfo Members

		public AutoPostingNotification AutoPostingNotification
		{
			get;
			set;
		}

		public EntryInfoCollection Entries
		{
			get { return null; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber { get; set; }

		public ZString[] ValidAPInvoiceNumsToMatch;
		ZString[] ICustomsJobInfo.GetValidAPInvoiceNumsToMatchAndValidateAgainst()
		{
			return ValidAPInvoiceNumsToMatch ?? Array.Empty<ZString>();
		}

		ZGuid ICustomsJobInfo.CreditorPK
		{
			get { return RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value; }
		}

		#endregion

		#region IBusiness Members

		bool IBusiness.CanContinueWithSave
		{
			get { return true; }
		}

		IBusiness[] IBusiness.Children
		{
			get { return null; }
		}

		void IBusiness.Delete()
		{
		}

		ZString IBusiness.HumanReadableName
		{
			get { return ZString.Empty; }
		}

		bool IBusiness.IsValidationSuspended
		{
			get { return false; }
		}

		void IBusiness.MarkAsNeedingValidationIncludingChildren()
		{
		}

		void IBusiness.NotifyRegisteredChildEditable()
		{
		}

		void IBusiness.ResumeValidation()
		{
		}

		void IBusiness.RunPreSaveValidation()
		{
		}

		void IBusiness.RunPreSaveValidationFetch(bool executeHints)
		{
		}

		void IBusiness.SuspendValidation()
		{
		}

		string IBusiness.TableName
		{
			get { return TableName; }
		}

		void IBusiness.ValidateIfQuickAndImprovesPreSaveValidationPerformance()
		{
		}

		#endregion

		#region IBusinessObjectState Members

		void IBusinessObjectState.ClearHasChangesIncludingChildren()
		{
		}

		void IBusinessObjectState.DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
		}

		bool IBusinessObjectState.HasChanges
		{
			get { return false; }
			set { }
		}

		event EventHandler<HasChangesChangedEventArgs> IBusinessObjectState.HasChangesChanged
		{
			add { }
			remove { }
		}

		bool IBusinessObjectState.HasChangesNotIncludingChildren
		{
			get { return false; }
		}

		void IBusinessObjectState.IncrementReadOnlyIncludingChildren()
		{
		}

		bool IBusinessObjectState.IsInDatabaseIncludingChildren
		{
			get { return false; }
		}

		uint IBusinessObjectState.LastChangeNumber
		{
			get { return 0; }
		}

		event EventHandler<NotificationsChangedEventArgs> IBusinessObjectState.NotificationsChanged
		{
			add { }
			remove { }
		}

		void IBusinessObjectState.RefreshBindingIncludingChildren()
		{
		}

		event EventHandler IBusinessObjectState.UpdatedByDataRefreshIncludingChildren
		{
			add { }
			remove { }
		}

		#endregion

		#region IBindingList Members

		void System.ComponentModel.IBindingList.AddIndex(System.ComponentModel.PropertyDescriptor property)
		{
		}

		object System.ComponentModel.IBindingList.AddNew()
		{
			return null;
		}

		bool System.ComponentModel.IBindingList.AllowEdit
		{
			get { return true; }
		}

		bool System.ComponentModel.IBindingList.AllowNew
		{
			get { return true; }
		}

		bool System.ComponentModel.IBindingList.AllowRemove
		{
			get { return true; }
		}

		void System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction)
		{
		}

		int System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor property, object key)
		{
			return 0;
		}

		bool System.ComponentModel.IBindingList.IsSorted
		{
			get { return false; }
		}

		event System.ComponentModel.ListChangedEventHandler System.ComponentModel.IBindingList.ListChanged
		{
			add { }
			remove { }
		}

		void System.ComponentModel.IBindingList.RemoveIndex(System.ComponentModel.PropertyDescriptor property)
		{
		}

		void System.ComponentModel.IBindingList.RemoveSort()
		{
		}

		System.ComponentModel.ListSortDirection System.ComponentModel.IBindingList.SortDirection
		{
			get { return System.ComponentModel.ListSortDirection.Ascending; }
		}

		System.ComponentModel.PropertyDescriptor System.ComponentModel.IBindingList.SortProperty
		{
			get { return null; }
		}

		bool System.ComponentModel.IBindingList.SupportsChangeNotification
		{
			get { return false; }
		}

		bool System.ComponentModel.IBindingList.SupportsSearching
		{
			get { return false; }
		}

		bool System.ComponentModel.IBindingList.SupportsSorting
		{
			get { return false; }
		}

		#endregion

		#region IList Members

		int System.Collections.IList.Add(object value)
		{
			return 0;
		}

		void System.Collections.IList.Clear()
		{
		}

		bool System.Collections.IList.Contains(object value)
		{
			return false;
		}

		int System.Collections.IList.IndexOf(object value)
		{
			return 0;
		}

		void System.Collections.IList.Insert(int index, object value)
		{
		}

		bool System.Collections.IList.IsFixedSize
		{
			get { return false; }
		}

		bool System.Collections.IList.IsReadOnly
		{
			get { return false; }
		}

		void System.Collections.IList.Remove(object value)
		{
		}

		void System.Collections.IList.RemoveAt(int index)
		{
		}

		object System.Collections.IList.this[int index]
		{
			get { return null; }
			set { }
		}

		#endregion

		#region ICollection Members

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
		}

		int System.Collections.ICollection.Count
		{
			get { return 0; }
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get { return false; }
		}

		object System.Collections.ICollection.SyncRoot
		{
			get { return null; }
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return null;
		}

		#endregion

		#region INotificationProvider Members

		CargoWise.ComponentModel.INotificationType CargoWise.ComponentModel.INotificationProvider.GetHighestSeverityNotificationType()
		{
			return null;
		}

		bool CargoWise.ComponentModel.INotificationProvider.HasNotifications(CargoWise.ComponentModel.INotificationType type)
		{
			return false;
		}

		bool CargoWise.ComponentModel.INotificationProvider.HasNotifications()
		{
			return false;
		}

		IEnumerable<CargoWise.ComponentModel.INotification> CargoWise.ComponentModel.INotificationProvider.Notifications
		{
			get { return null; }
		}

		#endregion

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return PK; }
		}

		#endregion

		#region ICustomsJobInfoProvider Members

		ICustomsJobInfo ICustomsJobInfoProvider.GetCustomsJobInfo(ZGuid companyPK)
		{
			return this;
		}

		#endregion

		#region IJobInvoicingPlugInAdditionalJobs Members

		public IJobInvoicingPlugIn[] AdditionalJobsToShowChargesFor
		{
			get
			{
				if (additionalJobsToShowChargesFor == null)
				{
					additionalJobsToShowChargesFor = Array.Empty<IJobInvoicingPlugIn>();
				}
				return additionalJobsToShowChargesFor;
			}
			set
			{
				additionalJobsToShowChargesFor = value;
			}
		}
		IJobInvoicingPlugIn[] additionalJobsToShowChargesFor;

		#endregion

		#region IAccIntegrationDataProvider Members

		ChargePosterBehaviours IAccIntegrationDataProvider.Action
		{
			get { return ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail; }
		}

		Action IAccIntegrationDataProvider.OnIntegrated
		{
			get { return OnIntegratedWithAccountingSuccessfully; }
		}

		void OnIntegratedWithAccountingSuccessfully()
		{
		}

		ZGuid[] IAccIntegrationDataProvider.DisbursementChargeCodes
		{
			get { return Array.Empty<ZGuid>(); }
		}

		ZGuid IAccIntegrationDataProvider.AutoPostingEmailRecipient
		{
			get { return ZGuid.Empty; }
		}

		ZString IAccIntegrationDataProvider.ReferenceID
		{
			get { return ZString.Empty; }
		}

		ZString IAccIntegrationDataProvider.JobType
		{
			get { return "Declaration"; }
		}

		bool IAccIntegrationDataProvider.SupportIntegration
		{
			get { return true; }
		}

		void IAccIntegrationDataProvider.LogPostingResult(string message)
		{
		}

		public IEnumerable<IAccInvoiceDataProvider> InvDataProviderCandidates
		{
			get { yield return ChargeProvider; }
		}

		IAccInvoiceDataProvider[] IAccIntegrationDataProvider.InvDataProviders
		{
			get
			{
				var result = new List<IAccInvoiceDataProvider>();
				if (DataProviders.Count > 0)
				{
					result.AddRange(DataProviders);
				}

				if (result.Count == 0)
				{
					result.Add(ChargeProvider);
				}
				return result.ToArray();
			}
		}

		internal List<IAccInvoiceDataProvider> DataProviders
		{
			get { return dataProviders; }
			set { dataProviders = value; }
		}
		List<IAccInvoiceDataProvider> dataProviders = new List<IAccInvoiceDataProvider>();

		MockCustomsChargesProvider ChargeProvider
		{
			get
			{
				if (chargeProvider == null)
				{
					chargeProvider = new MockCustomsChargesProvider();
					chargeProvider.CustomsCharges = Array.Empty<ICustomsCharges>();//CusEntryHeader returns an empty array if withdrawn
					chargeProvider.InvoiceNumber = "ABC123DEF";
					chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
					chargeProvider.CustomsJob = this;
					chargeProvider.Factory = Factory;

					var recipients = new ZGuid[]
									{ AutoPostingNotification != null && AutoPostingNotification.EmailRecipients.Length > 0 ?
									 AutoPostingNotification.EmailRecipients[0] : GlbStaff.CurrentUser.PK };
					chargeProvider.AutoPostingNotification = new AutoPostingNotification(recipients, false);
					chargeProvider.HasBeenWithdrawn = true;
					chargeProvider.DisbursementChargeCodes = new ZGuid[] { this.PK };
					Env.OutgoingMailManager.EmailsCreated.Clear();
				}
				return chargeProvider;
			}
		}
		MockCustomsChargesProvider chargeProvider;

		Guid ZArchitecture.Modules.IControllerIDProvider.BusinessObjectPK
		{
			get { return this.PK.ToGuid(); }
		}

		ZArchitecture.Modules.ControllerID ZArchitecture.Modules.IControllerIDProvider.ControllerID
		{
			get { return ZArchitecture.Modules.Testing.DummyControllerIDs.Dummy; }
		}

		public GlbCompany Company
		{
			get { return company ?? GlbCompany.CurrentCompany; }
			set { company = value; }
		}
		GlbCompany company;
		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new MockCustomsJobProviderRatingAdapterProvider(this); }
		}

		#endregion

		#region IRatingSupporterWithAdapter Members

		public IAutoRating RatingAdapter
		{
			get { return new MockCustomsJobProviderRatingAdapter<MockCustomsJobProvider>(this); }
		}

		#endregion

		public OrgHeader Consignee
		{
			get;
			set;
		}

		public OrgHeader Consignor
		{
			get;
			set;
		}

		public Directions JobDirection
		{
			get
			{
				if (isImport)
				{
					return Directions.Import;
				}

				if (isExport)
				{
					return Directions.Export;
				}

				if (isDomestic)
				{
					return Directions.Domestic;
				}

				return Directions.Unknown;
			}
		}

		public ZBool isImport;
		public ZBool isExport;
		public ZBool isDomestic;
	}

	public class MockCustomsJobProviderRatingAdapterProvider : RatingAdaptersProvider<MockCustomsJobProvider>
	{
		public MockCustomsJobProviderRatingAdapterProvider(MockCustomsJobProvider parent) : base(parent)
		{
		}

		protected override List<IAutoRating> GetAdapters(MockCustomsJobProvider parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating>()
			{
				new MockCustomsJobProviderRatingAdapter<MockCustomsJobProvider>(parent)
			};
		}
	}

	public class MockCustomsJobProviderRatingAdapter<T> : RatingAdapter<T>, IAutoRatingCustomsInfo
		where T : MockCustomsJobProvider
	{
		public MockCustomsJobProviderRatingAdapter(T parent)
			: base(parent)
		{
		}

		#region IAutoRating Members

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return new RateType(); }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return ((MockCustomsJobProviderInvoicingSupporter)Parent.InvoicingSupporter).ConsumerType; }
		}

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent.InvoicingSupporter; }
		}

		public override ILocation Destination
		{
			get { return Parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"); }
		}

		public override ILocation Origin
		{
			get { return Parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX"); }
		}

		public override IDocAddress DeliveryAddress
		{
			get { return Parent.Consignee.MainAddress; }
		}

		public override IDocAddress PickupAddress
		{
			get { return Parent.Consignor.MainAddress; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				result[RatingDebtorOrgTypes.CNE] = Parent.Consignee;
				result[RatingDebtorOrgTypes.CNR] = Parent.Consignor;

				return result;
			}
		}

		#region IAutoRatingCustomsInfo Members

		public ZString MessageType
		{
			get { return ZString.Empty; }
		}

		public ZString MessageSubType
		{
			get { return ZString.Empty; }
		}

		public EntryInfoCollection Entries
		{
			get { return Parent.Entries; }
		}

		public InvoiceInfoCollection Invoices
		{
			get { return null; }
		}

		public InvoiceInfoCollection TariffsPerInvoice
		{
			get { return new InvoiceInfoCollection(); }
		}

		public InvoiceInfoCollection TariffsPerShipment
		{
			get { return new InvoiceInfoCollection(); }
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get { return 0; }
		}
		#endregion

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				var infos = new PaymentTermInfos();
				if (!string.IsNullOrWhiteSpace(Parent.IncoTermExposed))
				{
					infos.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, Parent.IncoTermExposed));
				}

				return infos;
			}
		}

		public override bool IsApplicableToPaymentTermFiltering(string chargeCode, CostSell costOrSell)
		{
			return true;
		}

		public ZDateTime JobArrivalDate
		{
			get { return new ZDateTime(); }
		}

		public ZDateTime JobDepartureDate
		{
			get { return new ZDateTime(); }
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		#endregion
	}

	public class MockCustomsJobProviderInvoicingSupporter : DummyJobHeaderParentJobInvoicingSupporter
	{
		public MockCustomsJobProviderInvoicingSupporter(MockCustomsJobProvider parent)
		{
			this.parent = parent;
			ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			base.Origin = this.parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			base.Destination = this.parent.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			ContainerMode = Constants.ContainerModes.FCL;
		}

		readonly MockCustomsJobProvider parent;

		public override bool IsExport
		{
			get
			{
				return parent.IsExport();
			}
		}

		public override bool IsImport
		{
			get
			{
				return parent.IsImport();
			}
		}

		public override OrgHeader Consignee
		{
			get
			{
				return parent.Consignee;
			}
		}

		public override OrgHeader Consignor
		{
			get
			{
				return parent.Consignor;
			}
		}

		public OrgHeader DefaultDebtor { get; set; }

		public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			return DefaultDebtor ?? job.LocalCharges;
		}
	}

	public class MockCustomsChargesProvider : IAccInvoiceDataProvider
	{
		public MockCustomsChargesProvider()
		{
			IsBillable = true;
		}

		public bool HasBeenWithdrawn
		{
			get;
			set;
		}

		public ICustomsCharges[] CustomsCharges
		{
			get;
			set;
		}

		public ZDateTime InvoiceDate
		{
			get;
			set;
		}

		public ZDateTime APDueDate
		{
			get;
			set;
		}

		public ICustomsJobInfo CustomsJob
		{
			get { return customsJob; }
			set
			{
				customsJob = value;
				if (Factory == null)
				{
					Factory = customsJob?.Factory;
				}
			}
		}
		ICustomsJobInfo customsJob;

		public bool IsBillable
		{
			get;
			set;
		}

		public string ReasonForUnbillability
		{
			get;
			set;
		}

		public ZString UniqueNumber
		{
			get { return InvoiceNumber; }
		}

		public ZString PreviousUniqueNumber
		{
			get { return PreviousInvoiceNumber; }
		}

		public ZString InvoiceNumber { get; set; }
		public ZString PreviousInvoiceNumber { get; set; }

		public BusinessObjectFactory Factory { get; set; }

		public AutoPostingNotification AutoPostingNotification { get; set; }

		public ZGuid[] DisbursementChargeCodes { get; set; }

		public string EntryWithdrawnStatusTerm
		{
			get { return "withdrawn"; }
		}

		public bool IsEligibleForIntegration
		{
			get { return true; }
		}

		public bool IsAutoBillingDueDateFromPaymentTerms { get; set; }

		public bool APInvoiceNumberAlwaysIncludeChargeCode { get; set; }

		public void MarkAccoutingIntegrated()
		{
		}

		public bool MatchCustomsChargesToClear(ZString apInvoiceNumber, ZString description) => MatchCustomsChargesToClearFunc?.Invoke(apInvoiceNumber, description) ?? true;
		public Func<ZString, ZString, bool> MatchCustomsChargesToClearFunc { get; set; } = (inv, desc) => true;
	}

	public class MockCustomCharge : ICustomsCharges
	{
		#region ICustomsCharges Members

		public CustomsCharge[] fCustomsCharges;

		public CustomsCharge[] GetCustomsCharges(ILogger logger)
		{
			return fCustomsCharges;
		}

		public ZBool fIsActive;

		public ZBool IsActive
		{
			get { return fIsActive; }
		}

		#endregion

		public void AddCustomsCharge(CustomsCharge customsCharge)
		{
			List<CustomsCharge> customsChargesList = new List<CustomsCharge>(fCustomsCharges);
			customsChargesList.Add(customsCharge);
			fCustomsCharges = customsChargesList.ToArray();
		}
	}
}
