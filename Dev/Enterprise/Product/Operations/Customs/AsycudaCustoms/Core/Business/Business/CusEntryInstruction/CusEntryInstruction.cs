using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	[SystemDefinedValues]
	public class CusEntryInstruction : Customs.Business.CusEntryInstruction, Integration.Customs.AsycudaCustoms.ICusEntryInstruction, Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusEntryInstruction.Schema
		{
			public const int ASY_LocalReferenceNumberMaxLength = 32;
			public const string ASY_LocalReferenceNumber = "ASY_LocalReferenceNumber";
			public const string ASY_PortOfExit = "ASY_PortOfExit";
			public const string EntryNumber = "EntryNumber";
		}

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

		protected override bool SupportsCloneCore() => true;

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		[ResourceStringData("4BA5DC87-ACDF-49D1-99A1-EBA4E2892981", Caption = "Entry Number")]
		public ZString EntryNumber => EntryHeader?.EntryNumber ?? ZString.Empty;

		protected override ZString HumanReadableNameCore => Res.GetString("318F3F12-C461-4353-B7B2-679C3AB7480A", "Entry Instruction {0}", CEI_Style);

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				ZGuid oldValue = CEI_JE;
				base.CEI_JE = value;
				if (!IsCopying && oldValue != CEI_JE)
				{
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("E531E803-84FE-482D-A32F-79EC211C0792", Caption = "Declaration Type")]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				ZString oldValue = CEI_Style;
				base.CEI_Style = value;
				if (!IsCopying && oldValue != CEI_Style)
				{
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("EFF1326A-8D90-4531-9FC3-4252D43A7092", Caption = "Date for Duty")]
		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set => base.CEI_DateForDuty = value;
		}

		[ResourceStringData("4CCEC104-A3FE-4096-88D9-66D742C77E3A", Caption = "From Warehouse")]
		public override ZGuid CEI_OA_Warehouse
		{
			get => base.CEI_OA_Warehouse;
			set
			{
				var oldValue = CEI_OA_Warehouse;
				base.CEI_OA_Warehouse = value;
				if (!IsCopying && oldValue != CEI_OA_Warehouse)
				{
					EntryHeader?.MarkAsNeedingValidation();
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("E2A0A645-F412-423C-B801-364FA82E7F71", Caption = "To Warehouse")]
		public override ZGuid CEI_OA_Warehouse2
		{
			get => base.CEI_OA_Warehouse2;
			set
			{
				var oldValue = CEI_OA_Warehouse2;
				base.CEI_OA_Warehouse2 = value;
				if (!IsCopying && oldValue != CEI_OA_Warehouse2)
				{
					EntryHeader?.MarkAsNeedingValidation();
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("5C9EEFF8-51DA-4DA0-A241-5D4B2F0917EE", Caption = "Bond Holder")]
		public override ZGuid CEI_OH_BondHolder
		{
			get => base.CEI_OH_BondHolder;
			set => base.CEI_OH_BondHolder = value;
		}

		protected override bool IsInventorySelectionEnabledCore() => JobDeclaration?.IsInventorySelectionEnabled ?? false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			_ = Guarantee;
		}

		public CusBondDetail Guarantee => GetGuarantee(true);

		public CusBondDetail GetGuarantee(bool createIfMissing)
		{
			if (guarantee == null || guarantee.IsDeleted)
			{
				if (!IsInDatabase)
				{
					guarantee = createIfMissing ? CusBondDetail.LoadOrCreate(this) : CusBondDetail.Load(this);
				}
				else
				{
					guarantee = CusBondDetail.Load(this);
					if (guarantee == null && createIfMissing && LockGuaranteeManagementMutex(false))
					{
						guarantee = CusBondDetail.Create(this);
					}
				}
				RegisterEditableChildObject(guarantee);
			}
			return guarantee;
		}
		CusBondDetail guarantee;

		[ChildEditable]
		public SecondCusBondDetailCollection ReleaseGuarantees
		{
			get
			{
				if (releaseGuarantees == null)
				{
					releaseGuarantees = new SecondCusBondDetailCollection(this);
					releaseGuarantees.CollectionCountChange += ReleaseGuarantees_CollectionCountChange;
					RegisterEditableChildObject(releaseGuarantees);
				}

				return releaseGuarantees;
			}
		}

		void ReleaseGuarantees_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			GetGuarantee(false)?.RemainingInfo.RefreshBinding();
		}

		SecondCusBondDetailCollection releaseGuarantees;

		public override void Delete()
		{
			this.DeleteChildren<CommonCusBondDetail>(CusBondDetailSchema.PW_ParentID);
			CusInBondPermitsHeaders.DeleteAll();
			base.Delete();
		}

		public override bool CanDelete => base.CanDelete && !HasLinkedGuarantee;

		public override MultilingualString ReasonForNotAbleToDelete => HasLinkedGuarantee
			? ResString.GetMultilingualString("5be4b7cf-f75e-4f3e-933a-d7166037bf3b",
				"Entry Instruction with {0} {1} has a linked guarantee and cannot be deleted.",
				CEI_StyleInfo.HumanReadableName,
				CEI_Style
			) : base.ReasonForNotAbleToDelete;

		bool HasLinkedGuarantee => Factory.GetValue(ref hasLinkedGuarantee, () => GetGuarantee(false) is CusBondDetail consuming && consuming.IsLinked && !consuming.PW_CPH_Guarantee.IsEmpty);
		CachedProperty<bool> hasLinkedGuarantee;

		public ZString LinkedGuaranteeNumber => Factory.GetValue(ref linkedGuaranteeNumberCached, () => GetGuarantee(false)?.CusGuarantee?.CPH_Number ?? ZString.Empty);
		CachedProperty<ZString> linkedGuaranteeNumberCached;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				UnlockGuaranteeManagementMutex();
				ReleaseGuaranteesAddingReversingTransaction.Clear();
				needAddGuaranteeTransactions = false;
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			AddGuaranteeTransactions();
		}

		internal void MarkNeedAddGuaranteeTransactions()
		{
			needAddGuaranteeTransactions = true;
		}
		bool needAddGuaranteeTransactions;

		void AddGuaranteeTransactions()
		{
			if (needAddGuaranteeTransactions && GetGuarantee(false) is CusBondDetail consumingGuarantee
				&& consumingGuarantee.IsContinuous
				&& consumingGuarantee.IsLinked
				&& consumingGuarantee.Remaining >= ZDecimal.Zero
				&& consumingGuarantee.CusGuarantee is CusGuaranteeHeader cusGuarantee)
			{
				cusGuarantee.DoActionWithMutexLock(() =>
					{
						var transactions = new List<SharedCusPermitLineTransaction>();
						try
						{
							var bondAmountInfo = consumingGuarantee.PW_BondAmountInfo;
							if (bondAmountInfo.HasChanges)
							{
								transactions.Add(consumingGuarantee.AddDifferenceTransaction());
							}

							foreach (var releaseGuarantee in ReleaseGuaranteesAddingReversingTransaction)
							{
								using (((IBusinessObjectInternals)releaseGuarantee).SuppressReportRowDeletedError())
								{
									if (!IsDeletedFromDatabase(releaseGuarantee))
									{
										transactions.Add(releaseGuarantee.AddReversingTransaction());
									}
								}
							}

							foreach (var releaseGuarantee in ReleaseGuarantees)
							{
								if (releaseGuarantee.HasEnteredMandatoryData)
								{
									if (!releaseGuarantee.IsInDatabase)
									{
										transactions.Add(releaseGuarantee.AddTransaction());
									}
									else if (releaseGuarantee.PW_BondAmountInfo.HasChanges && !IsDeletedFromDatabase(releaseGuarantee))
									{
										transactions.Add(releaseGuarantee.AddDifferenceTransaction());
									}
								}
							}
						}
						catch (Exception ex)
						{
							consumingGuarantee.PW_BondAmountInfo.AddWarningWithoutValidationCheck(ex.Message);
							transactions.WhereNotNull().DeleteAll();
							throw;
						}
					},
					message => throw new ZCannotSaveException(message, ConcurrencyWarningCaption));
			}
		}

		static bool IsDeletedFromDatabase(SecondCusBondDetail releaseGuarantee)
		{
			return releaseGuarantee != null && releaseGuarantee.IsInDatabase && !new BusinessObjectFactory().ExistsInDatabase(CusBondDetailSchema.Constants.TableName, new ZQuery(CusBondDetailSchema.PK, releaseGuarantee.PK));
		}

		public ZGlobalMutex GuaranteeManagementMutex => mutex ?? (mutex = new ZGlobalMutex(MutexIDs.GuaranteeManagement, PK.ToString()));
		ZGlobalMutex mutex;

		public string GuaranteeManagementMutexText => Res.GetString("0c6d74d7-088d-4449-9179-0e8c1ad17070", "The Guarantee is currently being edited by {0}. Please try later.", GuaranteeManagementMutex.GetMutexLockByInfo());

		public bool LockGuaranteeManagementMutex(bool warnMutexText = true)
		{
			bool result;
			if (IsInDatabase)
			{
				if (GuaranteeManagementMutex.IsLocked)
				{
					result = GuaranteeManagementMutex.HasLock;
				}
				else
				{
					result = GuaranteeManagementMutex.Lock();
					if (result)
					{
						ReloadGuaranteeManagementData();
					}
				}

				if (!result && warnMutexText)
				{
					WarnConcurrency(GuaranteeManagementMutexText);
				}
			}
			else
			{
				result = true;
			}
			return result;
		}

		void ReloadGuaranteeManagementData()
		{
			if (guarantee != null && !guarantee.IsDeleted)
			{
				guarantee.ReloadSafe();
				guarantee.RefreshBinding();
			}
			if (releaseGuarantees != null)
			{
				var existingReleaseGuarantees = releaseGuarantees.Where(x => x.IsInDatabase).ToArray();
				releaseGuarantees.RefreshFromDb();
				if (existingReleaseGuarantees.Length > 0)
				{
					HandleExistingReleaseGuaranteeDeletedByOthers(existingReleaseGuarantees);
				}
				releaseGuarantees.RefreshBinding();
			}
		}

		void HandleExistingReleaseGuaranteeDeletedByOthers(SecondCusBondDetail[] existingReleaseGuarantees)
		{
			var releaseGuaranteeQuery = new ZQuery(CusBondDetailSchema.PK, existingReleaseGuarantees.Select(x => x.PK));
			var sql = $"SELECT {CusBondDetailSchema.Constants.PK} FROM {CusBondDetailSchema.Constants.SqlSchemaName}.{CusBondDetailSchema.Constants.TableName} " + releaseGuaranteeQuery.GetAsWhereAndOrderByClause(false); // sql script
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, releaseGuaranteeQuery.Params);
			var dataInDB = collection.Select(x => new ZGuid(x[CusBondDetailSchema.Constants.PK])).ToHashSet();
			existingReleaseGuarantees.ForEach(x =>
			{
				if (!dataInDB.Contains(x.PK))
				{
					x.ReadOnly = true;
					x.AddRowError(Res.GetString("{F4D2547A-816F-43BF-A41E-2C91E7124A18}", "This row is no longer in the database; please reload the job to pick up the latest changes."));
				}
			});
		}

		static string ConcurrencyWarningCaption => Res.GetString("2fa66fe7-2b3b-4251-86b1-11d4ce39f009", "Concurrency Warning");

		internal void WarnConcurrency(ZString message)
		{
			Warn(message, ConcurrencyWarningCaption);
		}

		internal void Warn(ZString message, ZString? caption = null)
		{
			caption = caption.HasValue && !caption.Value.IsEmpty ? caption : (ZString)Res.GetString("ced889cd-da33-4d3b-be23-d74fa14880e7", "Warning");
			if (!message.IsEmpty && JobDeclaration is JobDeclaration declaration && declaration.HasMessageInitiator)
			{
				declaration.MessageInitiator.WarnUserAboutSomething(message, caption);
			}
		}

		public void UnlockGuaranteeManagementMutex()
		{
			if (mutex != null && mutex.IsLocked && mutex.HasLock)
			{
				mutex.Unlock();
			}
		}

		HashSet<SecondCusBondDetail> ReleaseGuaranteesAddingReversingTransaction => releaseGuaranteesAddingReversingTransaction ?? (releaseGuaranteesAddingReversingTransaction = new HashSet<SecondCusBondDetail>());
		HashSet<SecondCusBondDetail> releaseGuaranteesAddingReversingTransaction;

		internal void MarkNeedingReversingTransaction(SecondCusBondDetail releaseGuarantee)
		{
			MarkNeedAddGuaranteeTransactions();
			ReleaseGuaranteesAddingReversingTransaction.Add(releaseGuarantee);
		}

		[ChildEditable]
		public CusInBondMoveHeaderCollection CusInBondPermitsHeaders
		{
			get
			{
				if (cusInBondPermitsHeaders == null)
				{
					cusInBondPermitsHeaders = new CusInBondMoveHeaderCollection(this);
					cusInBondPermitsHeaders.Reload();
					RegisterEditableChildObject(cusInBondPermitsHeaders);
				}

				return cusInBondPermitsHeaders;
			}
		}
		CusInBondMoveHeaderCollection cusInBondPermitsHeaders;

		[ResourceStringData("928ECD14-DA48-4355-9848-521A56D897D3", Caption = "Customs Office of Destination/Exit")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.PortOfExitList))]
		[MaxLength(JobDeclaration.Schema.JE_CustomsOfficeMaxLength)]
		public ZString ASY_PortOfExit
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.ASY_PortOfExit);
			set
			{
				var oldValue = ASY_PortOfExit;
				CheckMaximumLength(ASY_PortOfExitInfo, value);
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ASY_PortOfExit, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateASY_PortOfExit();
				}
				ASY_PortOfExitInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ASY_PortOfExitInfo => GetZPropertyInfo(Schema.ASY_PortOfExit);

		[ResourceStringData("7EDF1637-E93B-4E00-84BE-9D19564263DB", Caption = "Local Reference Number")]
		[MaxLength(Schema.ASY_LocalReferenceNumberMaxLength)]
		public ZString ASY_LocalReferenceNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.ASY_LocalReferenceNumber);
			set
			{
				var oldValue = ASY_LocalReferenceNumber;
				CheckMaximumLength(ASY_LocalReferenceNumberInfo, value);
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ASY_LocalReferenceNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateASY_LocalReferenceNumber();
				}
				ASY_LocalReferenceNumberInfo.RefreshBinding(oldValue);

				if (!IsResettingASY_LocalReferenceNumber && !IsCopying && ASY_LocalReferenceNumber != oldValue && !ASY_LocalReferenceNumberInfo.HasNotifications() && !DoReMerge(ASY_LocalReferenceNumber))
				{
					using (ResetASY_LocalReferenceNumber)
					{
						ASY_LocalReferenceNumber = oldValue;
					}
				}
			}
		}
		int resetingASY_LocalReferenceNumber;

		bool IsResettingASY_LocalReferenceNumber => resetingASY_LocalReferenceNumber > 0;

		IDisposable ResetASY_LocalReferenceNumber => new DisposableAction(() => resetingASY_LocalReferenceNumber++, () => resetingASY_LocalReferenceNumber--);

		bool DoReMerge(ZString newReference)
		{
			var referenceNeedChange = true;
			var declaration = JobDeclaration;
			var entryHeader = EntryHeader;
			if (declaration != null && entryHeader != null)
			{
				if (newReference.IsEmpty)
				{
					OnLocalReferenceNumberChangedToEmpty?.Invoke(null, EventArgs.Empty);
					referenceNeedChange = false;
				}
				else if (entryHeader.HasWHSTransaction)
				{
					OnLocalReferenceNumberChangedWhenHasWarehouseTransaction?.Invoke(null, EventArgs.Empty);
					referenceNeedChange = false;
				}
				else if (!entryHeader.CH_BGMReference.EqualsIgnoringCase(newReference))
				{
					var args = new CancelEventArgs(false);
					OnLocalReferenceNumberChangedToReMerge?.Invoke(null, args);
					referenceNeedChange = !args.Cancel && declaration.DoMerge();
				}
			}

			return referenceNeedChange;
		}

		public event EventHandler<CancelEventArgs> OnLocalReferenceNumberChangedToReMerge;
		public event EventHandler OnLocalReferenceNumberChangedToEmpty;
		public event EventHandler OnLocalReferenceNumberChangedWhenHasWarehouseTransaction;

		public ZPropertyInfo ASY_LocalReferenceNumberInfo => GetZPropertyInfo(Schema.ASY_LocalReferenceNumber);

		[DecimalPlaces(2)]
		[ResourceStringData("44aed492-f039-47f2-81b5-c051fba7d4fb|CustomsValue", Caption = "Customs Value")]
		public ZDecimal CustomsValue => EntryHeader?.CustomsValue ?? ZDecimal.Zero;

		public ZPropertyInfo CustomsValueInfo => GetZPropertyInfo(nameof(CustomsValue));

		[DecimalPlaces(3)]
		[ResourceStringData("866b8564-efe3-410d-9224-3960383dd59a|NetWeightKilograms", Caption = "Net Weight")]
		public ZDecimal NetWeightKilograms => EntryHeader?.NetWeightKilograms ?? ZDecimal.Zero;

		public ZPropertyInfo NetWeightKilogramsInfo => GetZPropertyInfo(nameof(NetWeightKilograms));

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.WeightUQList))]
		public ZString NetWeightUQ => Core.Constants.Weight.Kilograms;

		[DecimalPlaces(5)]
		[ResourceStringData("bc1c7771-5fb0-45a5-8713-eddef1f8b225|CustomsQuantity", Caption = "Customs Quantity")]
		public ZDecimal CustomsQuantity => EntryHeader?.CustomsQuantity ?? ZDecimal.Zero;

		public ZPropertyInfo CustomsQuantityInfo => GetZPropertyInfo(nameof(CustomsQuantity));

		[DecimalPlaces(2)]
		[ResourceStringData("bf564d69-d43e-4b72-8b3a-98562033e3e3|CustomsValue", FullDescription = "Total Remaining Customs Value", Caption = "Customs Value")]
		public ZDecimal RemainingCustomsValue => Factory.GetValue(ref remainingCustomsValue, () => CustomsValue - RiskManagements.Cast<RiskManagement>().Sum(x => x.CSI_Value));
		CachedProperty<ZDecimal> remainingCustomsValue;

		public ZPropertyInfo RemainingCustomsValueInfo => GetZPropertyInfo(nameof(RemainingCustomsValue));

		[DecimalPlaces(3)]
		[ResourceStringData("a49bd197-fd2f-4e94-b65d-e1a877297ec5|RemainingNetWeightKilograms", FullDescription = "Total Remaining Net Weight", Caption = "Net Weight")]
		public ZDecimal RemainingNetWeightKilograms => Factory.GetValue(ref remainingNetWeightKilograms, () => NetWeightKilograms - RiskManagements.Cast<RiskManagement>().Sum(x => x.CSI_Quantity));
		CachedProperty<ZDecimal> remainingNetWeightKilograms;

		public ZPropertyInfo RemainingNetWeightKilogramsInfo => GetZPropertyInfo(nameof(RemainingNetWeightKilograms));

		[DecimalPlaces(5)]
		[ResourceStringData("7820f293-63b8-4fd5-92a1-8c67d5799bd9|RemainingCustomsQuantity", FullDescription = "Total Remaining Customs Quantity", Caption = "Customs Quantity")]
		public ZDecimal RemainingCustomsQuantity => Factory.GetValue(ref remainingCustomsQuantity, () => CustomsQuantity - RiskManagements.Cast<RiskManagement>().Sum(x => x.CSI_Quantity2));
		CachedProperty<ZDecimal> remainingCustomsQuantity;

		public ZPropertyInfo RemainingCustomsQuantityInfo => GetZPropertyInfo(nameof(RemainingCustomsQuantity));

		[ChildEditable(true)]
		public RiskManagementCollection RiskManagements
		{
			get
			{
				if (riskManagements == null)
				{
					riskManagements = new RiskManagementCollection(this);
					riskManagements.Load();
					riskManagements.CountChanged += RiskManagements_CountChanged;
					RegisterEditableChildObject(riskManagements);
				}

				return riskManagements;
			}
		}

		void RiskManagements_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			RemainingCustomsValueInfo.RefreshBinding();
			RemainingNetWeightKilogramsInfo.RefreshBinding();
			RemainingCustomsQuantityInfo.RefreshBinding();
		}

		RiskManagementCollection riskManagements;

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ Constants.CusSupportingInfoTypes.RiskManagement, typeof(RiskManagement) }
		};

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		public bool IsTransitPermitsTabPageVisible => Factory.GetValue(ref fIsTransitPermitsTabPageVisible, () => InvoiceLines.FirstOrDefault()?.CusProcedure?.IsTransit() ?? false);
		CachedProperty<bool> fIsTransitPermitsTabPageVisible;

		public bool IsRiskTabPageTabVisible
		{
			get
			{
				return Factory.GetValue(ref fIsRiskTabPageTabVisible, () => (JobDeclaration?.IsRiskManagementEnabled ?? false) && (RiskManagements.Count > 0 || IsProcedureIntoRegime()));

				bool IsProcedureIntoRegime()
				{
					var currentProcedure = InvoiceLines.FirstOrDefault()?.CusProcedure;
					return currentProcedure != null &&
						(currentProcedure.IsIntoWarehouse() ||
						currentProcedure.IsIntoInwardProcessing() ||
						currentProcedure.IsIntoOutwardProcessing() ||
						currentProcedure.IsIntoTemporaryImport() ||
						currentProcedure.IsIntoTemporaryExport() ||
						currentProcedure.IsTransit());
				}
			}
		}
		CachedProperty<bool> fIsRiskTabPageTabVisible;

		public bool HasRiskValue
			=> Factory.GetValue(
				ref hasRiskValue,
				() => RemainingCustomsValue > ZDecimal.Zero ||
					RemainingNetWeightKilograms > ZDecimal.Zero ||
					RemainingCustomsQuantity > ZDecimal.Zero);
		CachedProperty<bool> hasRiskValue;

		public bool AllInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure => Factory.GetValue(ref allInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure, () => AllInvoiceLinesHasValidProcedure(x => x.IsGuaranteeConsumed() && !x.IsGuaranteeReleased()));
		CachedProperty<bool> allInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure;

		public bool AllInvoiceLinesUseReleaseGuaranteeProcedure => Factory.GetValue(ref allInvoiceLineUseReleaseGuaranteeProcedure, () => AllInvoiceLinesHasValidProcedure(x => x.IsGuaranteeReleased()));
		CachedProperty<bool> allInvoiceLineUseReleaseGuaranteeProcedure;

		bool AllInvoiceLinesHasValidProcedure(Func<RefCusProcedure, bool> isValidProcedure)
		{
			return InvoiceLines.Any() && InvoiceLines.All(x =>
			{
				var procedure = x.CusProcedure;
				return procedure != null && isValidProcedure(procedure);
			});
		}
	}
}
