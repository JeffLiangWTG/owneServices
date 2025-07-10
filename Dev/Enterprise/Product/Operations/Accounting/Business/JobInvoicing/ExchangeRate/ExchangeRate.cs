using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class ExchangeRate : AutoJobExRate, IExchangeRate, IDataVersionLoggingSupported
	{
		public ExchangeRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ExchangeRateConstuctorCallStack, () => System.Environment.StackTrace);
		}

		#region Schema

		public new class Schema : AutoJobExRate.Schema
		{
			public const string JF_TodayRate = "JF_TodayRate";
		}

		#endregion

		internal static string[] PersistentFieldsToCopyAndInValidOrder
		{
			get
			{
				return new[]
				{
					JobExRateSchema.JF_RX_NKRateCurrency.Name,
					JobExRateSchema.JF_OH_Org.Name,
					JobExRateSchema.JF_OrgType.Name,
					JobExRateSchema.JF_BaseRate.Name,
					JobExRateSchema.JF_CFXPercent.Name,
					JobExRateSchema.JF_CFXMinimum.Name,
					JobExRateSchema.JF_IsTransformed.Name,
					JobExRateSchema.JF_InvoiceCurrencyType.Name,
				};
			}
		}

		public void CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(ExchangeRate sourceObject)
		{
			//the sequence is important to avoid rate overriding
			this.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(sourceObject, PersistentFieldsToCopyAndInValidOrder);
		}

		#region Related Business Objects

		public
#if DEBUG
			virtual
#endif
			Job ParentJob
		{
			get
			{
				return Factory.Load<Job>(JF_JH);
			}
		}

		#endregion

		#region Property overrides

		[ReadOnlyMember(nameof(IsNotManualOrTransformed))]
		public override ZString JF_RX_NKRateCurrency
		{
			get { return base.JF_RX_NKRateCurrency; }
			set
			{
				var originalValue = base.JF_RX_NKRateCurrency;
				base.JF_RX_NKRateCurrency = value;

				if (!value.IsEmpty && value != originalValue)
				{
					ZAccExchangeRate.RefetchExchangeRate();
					if (ParentJob?.PlugInData != null)
					{
						var invoiceCurrencyType = ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(EffectiveInvoiceCurrencyType);
						JF_BaseRate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(ParentJob.ExchangeRateConfigurationRateConsumer, RateCurrency, Org, OrgType.ToLedger(), invoiceCurrencyType);
					}
					JF_TodayRateInfo.RefreshBinding();
				}
				UpdateCfxAndRateWhenOrgOrOrgTypeChanged();
				OnChanged(nameof(JF_RX_NKRateCurrency));
			}
		}

		public virtual int ExchangeRateDecimals => Company.ExchangeRateDecimalPlaces;

		GlbCompany Company => Job?.Company ?? GlbCompany.CurrentCompany;

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal JF_BaseRate
		{
			get => base.JF_BaseRate;
			set
			{
				var roundedValue = Utilities.Round(value, ExchangeRateDecimals);
				if (JF_BaseRate != roundedValue)
				{
					base.JF_BaseRate = roundedValue;
				}
				if (ParentJob != null && RateCurrency != null)
				{
					OnChanged(nameof(JF_BaseRate));
				}
			}
		}

		public bool JF_BaseRate_ReadOnly => !SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideBaseExchangeRate);

		JobInvoicingSecurityHelper SecurityHelper
		{
			get
			{
				securityHelper ??= new JobInvoicingSecurityHelper(ParentJob?.PlugInData?.InvoicingSupporter?.JobInvoicingSecurity);
				return securityHelper;
			}
		}
		JobInvoicingSecurityHelper securityHelper;

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal JF_SellRate => ExchangeRateHelper.GetBaseRateAdjustedByCFX(JF_RX_NKRateCurrency, JF_BaseRate, JF_CFXPercent, Company);

		public ZPropertyInfo JF_SellRateInfo => GetZPropertyInfo(nameof(JF_SellRate));
		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal JF_TodayRate
		{
			get
			{
				ZDecimal result = 0M;

				if (ParentJob != null && ParentJob.PlugInData != null)
				{
					result = AccExchangeRateConfigurationRateFinder.GetTodaysExchangeRate(ParentJob.ExchangeRateConfigurationRateConsumer, RateCurrency, Org, OrgType.ToLedger(), ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(EffectiveInvoiceCurrencyType));
				}

				return result;
			}
		}

		public ZPropertyInfo JF_TodayRateInfo
		{
			get { return GetZPropertyInfo(Schema.JF_TodayRate); }
		}

		[List("Lookups.OrgTypesList")]
		[ReadOnlyMember(nameof(IsNotManualOrTransformed))]
		public override ZString JF_OrgType
		{
			get => base.JF_OrgType;
			set
			{
				if (JF_OrgType == value)
				{
					return;
				}

				base.JF_OrgType = value;

				if (!IsExchangeRateOrgTypeCompatibleWithInvoiceCurrencyType(ExchangeRateEnumsExtensions.GetOrgTypeFromCode(value)))
				{
					JF_InvoiceCurrencyType = Enterprise.Integration.Accounting.InvoiceCurrencyType.NotApplicable.ToCode();
				}

				if (JF_OrgTypeInfo.HasErrors())
				{
					return;
				}

				UpdateCfxAndRateWhenOrgOrOrgTypeChanged();
				OnChanged(nameof(JF_OrgType));
			}
		}

		[ReadOnlyMember(nameof(IsNotManualOrTransformed))]
		public override ZGuid JF_OH_Org
		{
			get => base.JF_OH_Org;
			set
			{
				if (JF_OH_Org == value)
				{
					return;
				}

				base.JF_OH_Org = value;

				Validation.ValidateJF_OrgType();
				UpdateCfxAndRateWhenOrgOrOrgTypeChanged();
				OnChanged(nameof(JF_OH_Org));
			}
		}

		[ReadOnlyMember(nameof(IsCFXReadOnly))]
		[DecimalPlaces(2)]
		public override ZDecimal JF_CFXPercent
		{
			get => base.JF_CFXPercent;
			set
			{
				if (JF_CFXPercent == value)
				{
					return;
				}
				base.JF_CFXPercent = value;
				OnChanged(nameof(JF_CFXPercent));
			}
		}

		[ReadOnlyMember(nameof(IsCFXReadOnly))]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JF_CFXMinimum
		{
			get => base.JF_CFXMinimum;
			set
			{
				if (JF_CFXMinimum == value)
				{
					return;
				}
				base.JF_CFXMinimum = value;
				OnChanged(nameof(JF_CFXMinimum));
			}
		}

		[ReadOnly(true)]
		public override ZBool JF_IsTransformed { get => base.JF_IsTransformed; set => base.JF_IsTransformed = value; }

		public ZBool IsGenericRate => OrgType == ExchangeRateOrgTypeEnum.None && JF_OH_Org.IsEmpty && JF_IsTransformed;

		public ZBool IsNotManualOrTransformed => !JF_IsTransformed;

		public ZBool IsCFXNotApplied => IsGenericRate || OrgType == ExchangeRateOrgTypeEnum.Creditor;

		public ZBool IsCFXReadOnly => IsCFXNotApplied || !(ParentJob?.InvoicingAllowOverrideofCFX ?? false);

		/// <summary>
		/// The invoice currency type is effective only when the registry is enabled
		/// </summary>
		public ZString EffectiveInvoiceCurrencyType
			=> IsInvoiceCurrencyTypeEnabled ? (string)JF_InvoiceCurrencyType : Enterprise.Integration.Accounting.InvoiceCurrencyType.NotApplicable.ToCode();

		/// <summary>
		/// JF_InvoiceCurrencyType property is used in UI and validation
		/// Otherwise you should use the EffectiveInvoiceCurrencyType property
		/// </summary>
		[List("Lookups.InvoiceCurrencyTypeList")]
		[MaxLength(3)]
		[ResourceStringData("2754c27f-b3b0-442a-bb2d-1d451c33ec90", Caption = "Invoice Currency Type",
			FullDescription = "The value in this column determines whether the configuration applies to foreign currency invoices, local currency invoices or to both when set as blank.")]
		[ReadOnlyMember(nameof(IsInvoiceCurrencyTypeReadOnly))]
		public override ZString JF_InvoiceCurrencyType
		{
			get => base.JF_InvoiceCurrencyType;
			set
			{
				if (JF_InvoiceCurrencyType == value)
				{
					return;
				}

				base.JF_InvoiceCurrencyType = value;

				if (!JF_InvoiceCurrencyTypeInfo.HasErrors()
					&& ParentJob != null
					&& IsInvoiceCurrencyTypeEnabled)
				{
					var invoiceCurrencyType = ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(JF_InvoiceCurrencyType);
					JF_BaseRate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(ParentJob.ExchangeRateConfigurationRateConsumer, RateCurrency, Org, OrgType.ToLedger(), invoiceCurrencyType);
				}
			}
		}

		public ZBool IsInvoiceCurrencyTypeReadOnly => !IsExchangeRateOrgTypeCompatibleWithInvoiceCurrencyType(OrgType) || IsNotManualOrTransformed;

		bool IsInvoiceCurrencyTypeEnabled => AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.GetFallBackValueAtAllLevels(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		bool IsExchangeRateOrgTypeCompatibleWithInvoiceCurrencyType(ExchangeRateOrgTypeEnum orgType) => orgType == ExchangeRateOrgTypeEnum.Debtor;

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!IsInDatabase && ShouldDeleteDuplicateExchangeRateBeforeSave)
			{
				var exRateAlreadyInDb = GetExchangeRateAlreadyInDatabase();
				if (exRateAlreadyInDb != null && exRateAlreadyInDb.PK != PK)
				{
					Delete();
					Factory.SetContext(BusinessContext.HasDeletedExchangeRate);
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				Factory.RemoveContext(BusinessContext.HasDeletedExchangeRate);
			}
			base.OnFactorySaved(saveSucceeded);
		}

		ExchangeRate GetExchangeRateAlreadyInDatabase()
		{
			var query = new ZQuery(JobExRateSchema.JF_OH_Org, JF_OH_Org);
			query.AddToFilter(JobExRateSchema.JF_JH, JF_JH);
			query.AddToFilter(JobExRateSchema.JF_OrgType, JF_OrgType);
			query.AddToFilter(JobExRateSchema.JF_RX_NKRateCurrency, JF_RX_NKRateCurrency);
			query.AddToFilter(JobExRateSchema.JF_InvoiceCurrencyType, EffectiveInvoiceCurrencyType);
			query.FetchOnlyFromLocalCache = false;
			return new BusinessObjectFactory().LoadTop1<ExchangeRate>(query);
		}

		internal bool ShouldDeleteDuplicateExchangeRateBeforeSave { get; set; }

		#region IDataVersionLoggingSupported
		
		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		protected override void OnUpdatedByDataRefresh()
		{
			using (new DisposableAction(() => Factory.SetContext(BusinessContext.IsUpdatedDueToChangesInDB), () => Factory.RemoveContext(BusinessContext.IsUpdatedDueToChangesInDB)))
			{
				base.OnUpdatedByDataRefresh();
				RaiseExchangeRateChangedEvent();
			}
		}

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);
			RaiseExchangeRateChangedEvent();
		}

		#region Delete

		public override void Delete()
		{
			try
			{
				if (this.HasContext(ExchangeRatesCollection.Context.AddingNewRate))
				{
					var message = BuildMessageAboutDeletedOrRemovedExchangeRate();
					ErrorReporter.ReportOnce("DeletingExRateInTheProcessOfAddingIt", message);
				}

				var parentCollections = ((IBusinessObjectInternals)this).ParentCollections;
				Factory.SetContext(BusinessContext.DeletingExchangeRate);
				base.Delete();

				deletedStack = System.Environment.StackTrace;

				if (!IsDeleting && IsDeleted && parentCollections.Any(x => !((IBusinessObjectCollectionInternals)x).MastersAreDeleted && x.Contains(PK)))
				{
					ErrorReporter.ReportOnce("Business_Object_Collections_With_Deleted_ExchangeRate", BuildMessageAboutDeletedExchangeRateAndItsParentCollections(parentCollections));
				}
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.DeletingExchangeRate);
			}
		}

		protected override void DeleteForDataRefresh()
		{
			var parentCollections = ((IBusinessObjectInternals)this).ParentCollections;

			base.DeleteForDataRefresh();

			deletedStack = System.Environment.StackTrace;

			if (!IsDeleting && IsDeleted && parentCollections.Any(x => !((IBusinessObjectCollectionInternals)x).MastersAreDeleted && x.Contains(PK)))
			{
				ErrorReporter.ReportOnce("Business_Object_Collections_With_DeletedByDataRefresh_ExchangeRate", BuildMessageAboutCollectionsDeletingByDataRefresh(parentCollections));
			}
		}

		string deletedStack;

		internal ZString BuildMessageAboutDeletedOrRemovedExchangeRate(string verb = "Deleted")
		{
			var stringBuilder = new ZStringBuilder();

			stringBuilder.Append(verb + (NoResString)" ExchangeRate:");
			stringBuilder.Append(this.GetBusinessObjectGenericInfo());
			stringBuilder.Append(this.GetContextInfo<ExchangeRatesCollection.Context>());
			stringBuilder.Append(this.GetExchangeRateOriginalInfo());
			stringBuilder.Append(CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PK, CriticalValidationInfoCollectorServiceKeyType.ExchangeRatesCollectionRemoveMethodInfo));

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		ZString BuildMessageAboutDeletedExchangeRateAndItsParentCollections(BusinessObjectCollection[] parentCollections)
		{
			var stringBuilder = new ZStringBuilder(BuildMessageAboutDeletedOrRemovedExchangeRate());

			stringBuilder.Append((NoResString)"Before Delete:");
			stringBuilder.Append(parentCollections.GetParentCollectionsInfo(this));
			stringBuilder.Append((NoResString)"After Delete:");
			stringBuilder.Append(this.GetParentCollectionsInfo());

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		ZString BuildMessageAboutCollectionsDeletingByDataRefresh(BusinessObjectCollection[] parentCollections)
		{
			var stringBuilder = new ZStringBuilder(BuildMessageAboutDeletedExchangeRateAndItsParentCollections(parentCollections));

			if (this.HasContext(ExchangeRatesCollection.Context.DeletedByDataRefresh))
			{
				stringBuilder.Append((NoResString)"Deleted via DataRefreshBus.");
			}

			var collectionsWithDataRefresh = parentCollections.Where(x => x.IsRefreshingByDataRefreshBus).ToArray();
			if (collectionsWithDataRefresh.Any())
			{
				stringBuilder.Append((NoResString)"Data Refresh on the following collection(s):");
				stringBuilder.Append(collectionsWithDataRefresh.GetParentCollectionsInfo(this));
			}

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		void RaiseExchangeRateChangedEvent()
		{
			changed?.Invoke(this, new EventArgs());
		}

		public ExchangeRateOrgTypeEnum OrgType
		{
			get => ExchangeRateEnumsExtensions.GetOrgTypeFromCode(JF_OrgType);
			set => JF_OrgType = value.ToCode();
		}

		public bool IsDuplicateOf(ExchangeRate exRate)
		{
			return JF_JH == exRate.JF_JH
				&& JF_RX_NKRateCurrency == exRate.JF_RX_NKRateCurrency
				&& JF_OrgType == exRate.JF_OrgType
				&& JF_OH_Org == exRate.JF_OH_Org
				&& EffectiveInvoiceCurrencyType == exRate.EffectiveInvoiceCurrencyType;
		}

		public void RefreshCFXMinimum()
		{
			UpdateCfx(true);
		}

		#region BusinessObject methods override

		public override bool IsSavedByFactory => !IsDeleted ? base.IsSavedByFactory && !JF_BaseRate.IsEmpty : base.IsSavedByFactory;

		public override bool HasChanges { get => !IsDeleted ? base.HasChanges && !JF_BaseRate.IsEmpty : base.HasChanges; set => base.HasChanges = value; }

		#region SuppressResourceStringsCheckRegion
		protected override ZString GetAdditionalInfoForZSaveExceptionCore()
		{
			var additionalInfo = base.GetAdditionalInfoForZSaveExceptionCore();
			if (!IsDeleted)
			{
				var service = CriticalValidationInfoCollectorService.GetService(Factory);
				if (service != null)
				{
					var builder = new ZStringBuilder();
					builder.AppendLine(Invariant($@"
PK = {PK}:{service.GetInfo(PK, CriticalValidationInfoCollectorServiceKeyType.ExchangeRateConstuctorCallStack)}"));
					builder.Append("Charges:");
					ParentJob?.Charges.ForEach(c => builder.Append(c.GetAllPropertyValues()));
					builder.Append("ExchangeRates:");
					ParentJob?.ExchangeRates.ForEach(x => builder.Append(x.GetAllPropertyValues()));
					additionalInfo = builder.ToStringWithNewLineBetweenAppends();
				}
			}
			return additionalInfo;
		}
		#endregion

		protected override StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message)
		{
			return base.BuildRowDeletedReport(columnName, ex, versionToUse, version, message)
				.Append((NoResString)"Delete Stack: Access property from a detached rate.").AppendLine(deletedStack ?? (NoResString)"Delete stack never collected");
		}

		#endregion

		#region Implementation

		int LocalCurrencyDecimals => Company.LocalCurrency.Decimals;

		void UpdateCfxAndRateWhenOrgOrOrgTypeChanged()
		{
			// When we copy an OOQ, CFX has to be calculated (coming from the Organisation), not copying from the original OOQ.
			// While the parent BusinessObject is Quote (not QuotedBooking), which doesn't implement IJobInvoicingPlugIn.
			// Therefore, we need to check the BusinessContext to let the CFX be updated during the copy.
			if (RowErrors.Any() || !this.HasAnyOfContexts(BusinessContext.InvoicingPlugInGUI, BusinessContext.CopyChargePersistentValues))
			{
				return;
			}

			UpdateCfx();

			if (ParentJob != null)
			{
				var invoiceCurrencyType = ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(EffectiveInvoiceCurrencyType);
				var rate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(ParentJob.ExchangeRateConfigurationRateConsumer, RateCurrency, Org, OrgType.ToLedger(), invoiceCurrencyType);
				JF_BaseRate = rate; //will raise OnChanged
			}
		}

		void UpdateCfx(bool skipPercent = false)
		{
			ZDecimal cfxPercent = 0m;
			ZDecimal cfxMin = 0m;

			if (OrgType.IsDebtor() && ParentJob != null)
			{
				var ledgerType = OrgType.ToLedger();
				var invoiceCurrencyType = ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(EffectiveInvoiceCurrencyType);
				var date = AccExchangeRateConfigurationRateFinder.GetPreferredExchangeRateDate(ParentJob.ExchangeRateConfigurationRateConsumer, Org, JF_RX_NKRateCurrency, ledgerType, invoiceCurrencyType);
				ParentJob.GetCFXPairFromOrganization(Org, JF_RX_NKRateCurrency, date, out cfxPercent, out cfxMin);
			}

			if (!skipPercent && JF_CFXPercent != cfxPercent)
			{
				JF_CFXPercent = cfxPercent;
			}
			JF_CFXMinimum = cfxMin;
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new ExchangeRateUniqueIndexFailureHandler(this); }
		}

		class ExchangeRateUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ExchangeRateUniqueIndexFailureHandler(ExchangeRate exchangeRate)
			{
				Argument.NotNull(exchangeRate, nameof(exchangeRate));
				this.exchangeRate = exchangeRate;
			}

			readonly ExchangeRate exchangeRate;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return JobExRateSchema.Constants.Indexes.FK_UX__JF_OH_Org_JF_JH_JF_OrgType_JF_RX_NKRateCurrency_JF_InvoiceCurrencyType; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier?.ReportError(GetNotificationMessage(), Res.GetString("51c068c6-4b0e-46b7-9b0d-c659361bfd50", "Duplicate Job Exchange Rate"));
			}

			string GetNotificationMessage()
			{
				return Res.GetString("8c051091-94e0-4699-98f6-b20281605514", @"While you were working, another user has created a duplicate Job Exchange Rate.
Job: {0}, currency: {1}, organization type: {2}, organization: {3}, invoice currency type: {4}.
Please cancel your changes and reload the form.", exchangeRate.ParentJob?.JH_JobNum, exchangeRate.JF_RX_NKRateCurrency, exchangeRate.JF_OrgType, exchangeRate.Org?.OH_Code.ToUpper(), exchangeRate.EffectiveInvoiceCurrencyType);
			}

			#endregion
		}

		#endregion

		#region ZExchangeRate

		ZAccExchangeRate ZAccExchangeRate => fZAccExchangeRate ?? (fZAccExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Buy, JF_BaseRateInfo, (ZPropertyInfoString)JF_RX_NKRateCurrencyInfo, null) { IsRateRequired = false });
		ZAccExchangeRate fZAccExchangeRate;

		#endregion

		#endregion

		#region IsBuyRateEqualsTodayRate

		public bool IsBuyRateEqualsTodayRate
		{
			get { return JF_BaseRate == JF_TodayRate; }
		}

		#endregion

		#region IExchangeRate

		event EventHandler changed;
		internal event EventHandler Changed
		{
			add
			{
				changed += value;
			}
			remove
			{
				changed -= value;
				if (!IsDeleted && CanDelete && !JF_IsTransformed)
				{
					Delete();
				}
			}
		}

		public override bool CanDelete => base.CanDelete && (changed == null);

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (changed != null)
				{
					return ResString.GetMultilingualString("ac05594a-8341-431b-bd06-b099c85f7730", "Cannot remove the selected Currency as it is being used by at least one of the charge line.");
				}
				return base.ReasonForNotAbleToDelete;
			}
		}

		ZString IExchangeRate.CurrencyCode => JF_RX_NKRateCurrency;

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		ZDecimal IExchangeRate.Rate => JF_BaseRate;

		void OnChanged(string propertyName)
		{
			if (RowErrors.Any() && this.HasContext(BusinessContext.InvoicingPlugInGUI))
			{
				return;
			}

			HookUpLinkedCharges();

			var propertiesRequiringRefresh = new[]
			{
				nameof(JF_RX_NKRateCurrency),
				nameof(JF_OH_Org),
				nameof(JF_OrgType)
			};

			if (propertiesRequiringRefresh.Contains(propertyName) && JF_IsTransformed)
			{
				ParentJob?.RefreshChargeLinesExchangeRateBinding();
			}
			else
			{
				changed?.Invoke(this, new EventArgs());
			}
		}

		void HookUpLinkedCharges()
		{
			var linker = ChargeToExRateLinker.Get(Factory);
			if (linker != null)
			{
				var chargePKs = linker.GetRelatedChargesPKs(this);
				if (chargePKs.Any())
				{
					var applicableRateKinds = new List<ExchangeRateKind>();
					if (JF_OrgType != ExchangeRateOrgTypeEnum.Creditor.ToCode())
					{
						applicableRateKinds.Add(ExchangeRateKind.SellRate);
						applicableRateKinds.Add(ExchangeRateKind.SellInvoiceRate);
					}
					if (JF_OrgType != ExchangeRateOrgTypeEnum.Debtor.ToCode())
					{
						applicableRateKinds.Add(ExchangeRateKind.CostRate);
					}

					var charges = Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, chargePKs) { FetchOnlyFromLocalCache = true });

					foreach (var charge in charges)
					{
						foreach (var rateKind in applicableRateKinds)
						{
							linker.RemoveLinks(charge, rateKind);   // Remove links to change IsInitialised state and avoid stack overflow

							switch (rateKind)
							{
								case ExchangeRateKind.CostRate:
									charge.UpdateCostExchangeRate();
									break;
								case ExchangeRateKind.SellRate:
									charge.UpdateRevenueExchangeRate();
									break;
								case ExchangeRateKind.SellInvoiceRate:
									charge.UpdateSellInvoiceExchangeRate();
									break;
							}
						}
					}
				}
			}
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			base.FillWithValidTestDataCore(kind, propertyPath);
			OrgType = ExchangeRateOrgTypeEnum.None;
			if (JF_BaseRate.IsEmpty)
			{
				JF_BaseRate = 1m;
			}
			base.JF_InvoiceCurrencyType = Enterprise.Integration.Accounting.InvoiceCurrencyType.NotApplicable.ToCode();
			JF_IsTransformed = true;
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
		}

#endif
	}
}
