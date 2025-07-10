using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageDecCollection = Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDecCollection<Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDec, Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader>;
using SupportingDocument = Enterprise.Customs.FR.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageJobHeader : EU.Business.CusTempStorage.CusTempStorageJobHeader,
		Integration.Customs.FR.ICusTempStorageJobHeader, ISynchroniserReadOnlyMembersProvider, IGuaranteeJobParent, ICorrelationIDProvider, IBillGenerationSupport
	{
		public new class Schema : EU.Business.CusTempStorage.CusTempStorageJobHeader.Schema
		{
			public const string JobReferencePrefix = "FRJ";
			public const string DDTNumber = "DDTNumber";
			public const string CorrelationID = "CorrelationID";
			public const int CorrelationMaxLength = 10;
		}

		public CusTempStorageJobHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static CusTempStorageJobHeader New(BusinessObjectFactory factory, string appCode = FRConstants.TemporaryStorage.AppCodeIST)
		{
			var header = factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = appCode;
			header.CreateRelatedCusTempStorageDec();
			return header;
		}

		public void InitialisefromNPBO(TemporaryStorageWrapperHeader npbo)
		{
			this.SJH_OA_Presenter = npbo.PresenterPK;
			this.SJH_TransportMode = npbo.TransportMode;
			this.SJH_OH_Customer = npbo.CustomerPK;
			this.SJH_TransportRegNo = npbo.Vessel;
			this.SJH_RL_NKLoading = npbo.PlaceOfLoading;
			this.SJH_CustomsProfile = npbo.CustomsProfile;
			this.SJH_DepartureDate = npbo.DepartureDate.Date;
			this.SJH_ArrivalDate = npbo.ArrivalDate.Date;
			this.SJH_ContainerCount = npbo.ContainerCount;
			this.SJH_CustomsOffice = npbo.CustomsOfficeOfDestination;
			this.SJH_CustomsOfficeOfEntryIntoEU = npbo.CustomsOfficeOfEntry;
			this.SJH_PresentationDate = npbo.PresentationDate.Date;
			this.SJH_PreviousReferenceType = npbo.PreviousEntryType;
			this.SJH_PreviousReferenceNumber = npbo.PreviousEntryNumber;
			this.SJH_TransportMeansDescription = npbo.TransportMeansDescription;

			this.SJH_CPH_Guarantee = npbo.GuaranteePK;

			if (npbo.TemporaryStorageContainers != null)
			{
				var containers = this.CusTempStorageDec.CusTempStorageContainers;

				foreach (TemporaryStorageWrapperContainer npContainer in npbo.TemporaryStorageContainers)
				{
					var container = containers.AddNew();
					container.CY_Code = npContainer.ContainerType;
					container.CY_Data = npContainer.ContainerNumber;
				}
			}

			if (npbo.TemporaryStorageLines != null)
			{
				var lines = this.CusTempStorageDec.CusTempStorageLines;
				lines.DeleteAll();
				foreach (TemporaryStorageWrapperLine npLine in npbo.TemporaryStorageLines)
				{
					var line = lines.AddNew();

					line.TSL_OwnerReferenceType = npLine.OwnerReferenceType;
					line.TSL_OwnerReferenceNumber = npLine.OwnerReferenceNumber;
					line.TSL_LocationOfGoods = npLine.LocationOfGoods.Left(line.TSL_LocationOfGoodsInfo.MaxLength);
					line.TSL_GrossWeight = npLine.GrossWeight;
					line.TSL_GrossWeightUQ = npLine.GrossWeightUQ;
					line.TSL_PackageQty = npLine.PackageQty;
					line.TSL_GoodsDescription = npLine.GoodsDescription;
					line.TSL_PackageType = npLine.PackageType;

					var lineItems = line.CusTempStorageLineItems;
					if (npLine.TemporaryStorageFurtherDetails != null)
					{
						foreach (TemporaryStorageWrapperFurtherDetail npFurtherDetail in npLine.TemporaryStorageFurtherDetails)
						{
							var lineItem = lineItems.AddNew();
							lineItem.TSI_CommodityCode = npFurtherDetail.CommodityCode;
							lineItem.TSI_GoodsOrigin = npFurtherDetail.OriginCountry;
							lineItem.TSI_NetWeight = npFurtherDetail.NetMass;
							lineItem.TSI_NetWeightUQ = npFurtherDetail.NetMassUQ;
							lineItem.TSI_GoodsValue = npFurtherDetail.GoodsValue;
							lineItem.TSI_GuaranteedValue = npFurtherDetail.GuaranteedValue;
							lineItem.TSI_RX_NKCurrency = npFurtherDetail.Currency;
						}
					}
				}
			}

			var cloneArgs = new BusinessObjectCloneArgs(Factory, Array.Empty<string>(), typeof(SupportingDocument), false);
			npbo.SupportingDocuments.ForEach(doc =>
			{
				var cloneDoc = doc.Clone(cloneArgs);
				using (cloneDoc.SuspendSettingHasChanges())
				{
					CusTempStorageDec.SupportingDocuments.Add(cloneDoc);
				}
			});
		}
		public new CusTempStorageDec CusTempStorageDec => (CusTempStorageDec)base.CusTempStorageDec;

		#region App Code Type

		public ZBool IsFRC => SJH_AppCode == FRConstants.TemporaryStorage.AppCodeFRC;

		public ZBool IsIST => SJH_AppCode == FRConstants.TemporaryStorage.AppCodeIST;

		public ZBool IsLADT => SJH_AppCode == FRConstants.TemporaryStorage.AppCodeLAD;

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageJobHeaderValidation GetNewValidation() => new CusTempStorageJobHeaderValidation(this);

		public new CusTempStorageJobHeaderValidation Validation => (CusTempStorageJobHeaderValidation)base.Validation;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageJobHeaderLookups GetNewLookups() => new CusTempStorageJobHeaderLookups(this);

		public new CusTempStorageJobHeaderLookups Lookups => (CusTempStorageJobHeaderLookups)base.Lookups;

		#endregion

		#region CusTempStorageDecs

		public new CusTempStorageDecCollection CusTempStorageDecs => (CusTempStorageDecCollection)base.CusTempStorageDecs;

		protected override EU.Business.CusTempStorage.CusTempStorageDecCollection CreateNewCusTempStorageDecs() => new CusTempStorageDecCollection(this);

		public void CreateRelatedCusTempStorageDec()
		{
			if (IsFRC)
			{
				FRCCusTempStorageDec.New(this);
			}
			else if (IsIST)
			{
				var istStorageDec = ISTCusTempStorageDec.New(this);
				istStorageDec.CusTempStorageLines.AddNew();
			}
			else if (IsLADT)
			{
				var ladtStorageDec = LADTCusTempStorageDec.New(this);
				ladtStorageDec.CusTempStorageLines.AddNew();
			}
		}
		#endregion

		#region Overrides
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateFormattedNumberPropertyIfRequired(SJH_JobReferenceInfo, Env.NumberFountains.FRTempStorageJobReference);
			CorrelationIdGenerator.InitCorrelationID(IsUniqueCorrelationID);
		}

		protected override ZString HumanReadableNameCore => SJH_JobReference;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override DocumentSupporter GetDocumentSupporter() => new CusTempStorageJobHeaderDocumentSupporter(this);

		public bool HasInStoreEvent => Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InStoreCode)).Any();

		#endregion

		#region Properties

		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.GuaranteeList))]
		public override ZGuid SJH_CPH_Guarantee { get => base.SJH_CPH_Guarantee; set => base.SJH_CPH_Guarantee = value; }

		#region SJH_TempStorageEndDateUtc
		[ReadOnlyMember(nameof(TempStorageEndDateUtcReadOnly))]
		[ResourceStringData("FR.CusTempStorageJobHeader.SJH_TempStorageEndDateUtc", Caption = "Temporary Storage End Date", MediumCaption = "End Date", ShortCaption = "End")]
		public override ZDateTime SJH_TempStorageEndDateUtc { get => base.SJH_TempStorageEndDateUtc; set => base.SJH_TempStorageEndDateUtc = value; }

		ZBool TempStorageEndDateUtcReadOnly
		{
			get
			{
				if (!IsInDatabase)
				{
					return false;
				}
				return !SJH_TempStorageEndDateUtcInfo.OriginalValue.IsEmpty;
			}
		}

		public ZBool ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty => !TempStorageEndDateUtcReadOnly && !SJH_TempStorageEndDateUtc.IsEmpty;

		public virtual void SetTempStorageEndDateUtcDefaultValue()
		{
			var endDate = ZDateTime.Now;

			if (IsLADT)
			{
				var rules = CustomsProfile?.CusAuthorisationRules;
				endDate = ZDateTime.Now.AddDays(3);
				var storageLimit = rules?.FirstOrDefault(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.STO);
				if (storageLimit != null && ZInt.TryParse(storageLimit.CPR_ValueFrom, out var addDays))
				{
					addDays = addDays > 6 ? (ZInt)6 : addDays;
					endDate = ZDateTime.Now.AddDays(addDays);
				}
			}
			else
			{
				endDate = ZDateTime.Now.AddDays(90);
			}

			SJH_TempStorageEndDateUtc = endDate;
		}
		#endregion

		public override ZGuid SJH_OH_Customer
		{
			get => base.SJH_OH_Customer;
			set
			{
				var oldValue = SJH_OH_Customer;
				base.SJH_OH_Customer = value;
				if (!IsCopying && oldValue != SJH_OH_Customer)
				{
					SetCustomsProfileDefaultValue();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateSJH_CustomsProfile();
				}
			}
		}

		#region SJH_CustomsProfile
		[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.CustomsProfileList))]
		[ResourceStringData("FR.CusTempStorageJobHeader.SJH_CustomsProfile", Caption = "Customs Profile", ShortCaption = "Profile")]
		public override ZString SJH_CustomsProfile
		{
			get => base.SJH_CustomsProfile;
			set => base.SJH_CustomsProfile = value;
		}

		void SetCustomsProfileDefaultValue()
		{
			if ((IsLADT || IsIST) && Lookups.CustomsProfileList.Count == 1)
			{
				SJH_CustomsProfile = Lookups.CustomsProfileList[0].Code;
			}
		}

		public CusAuthorisationHeader CustomsProfile => Lookups.GetCusAuthorisationHeader(SJH_CustomsProfile);
		#endregion
		#endregion

		#region CorrelationID

		[ReadOnly(true)]
		[MaxLength(Schema.CorrelationMaxLength)]
		public ZString CorrelationID
		{
			get { return CorrelationIDEntryNumber.CE_EntryNum; }
			set { CorrelationIDEntryNumber.CE_EntryNum = value; }
		}

		public ZPropertyInfo CorrelationIDInfo { get { return GetWrappedZPropertyInfo(Schema.CorrelationID, x => CorrelationIDEntryNumber.CE_EntryNumInfo); } }

		public CusEntryNumber CorrelationIDEntryNumber
		{
			get
			{
				if (correlationIDEntryNumber == null)
				{
					correlationIDEntryNumber = new CachedProperty<CusEntryNumber>(Factory, delegate
					{
						var correlationEntryNumberInternal = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.CorrelationIdentifier, CountryCode);
						if (correlationEntryNumberInternal == null)
						{
							correlationEntryNumberInternal = CusEntryNumber.New(this, CusEntryNumberTypes.EU.CorrelationIdentifier, CountryCode);
							correlationEntryNumberInternal.CE_EntryIsSystemGenerated = true;
							correlationEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
						}
						return correlationEntryNumberInternal;
					});
				}
				return correlationIDEntryNumber.Value;
			}
		}
		CachedProperty<CusEntryNumber> correlationIDEntryNumber;

		public CorrelationIDGenerator CorrelationIdGenerator => correlationIdGenerator ?? (correlationIdGenerator = new CorrelationIDGenerator(this, this));
		CorrelationIDGenerator correlationIdGenerator;

		bool IsUniqueCorrelationID(ZString correlationID)
		{
			return NumberGeneratorHelper.IsUniqueEntryNumber(Factory, TableName, correlationID, CusEntryNumberTypes.EU.CorrelationIdentifier, CountryCode);
		}

		public ZString CorrelationIDPrefix => ZString.Empty;

		#endregion

		public BusinessObject RelatedBusinessObject
		{
			get
			{
				if (temporaryStorageHeaderPivot == null || temporaryStorageHeaderPivot.IsDeleted)
				{
					FindTemporaryStorageHeaderPivot();
				}
				return temporaryStorageHeaderPivot?.Relation2Object;
			}
			protected set
			{
				if (value == null && temporaryStorageHeaderPivot != null)
				{
					temporaryStorageHeaderPivot.Delete();
					temporaryStorageHeaderPivot = null;
				}
				else
				{
					if (temporaryStorageHeaderPivot == null || temporaryStorageHeaderPivot.IsDeleted)
					{
						CreateOrFindTemporaryStorageHeaderPivot();
					}
					temporaryStorageHeaderPivot.Relation2Object = value;
				}
			}
		}

		public void SetRelatedBusinessObject(BusinessObject bizObj, ZString type)
		{
			RelatedBusinessObject = bizObj;
			temporaryStorageHeaderPivot.XX_RelationType = type;
		}

		public GenPivot CreateOrFindTemporaryStorageHeaderPivot()
		{
			var genPivot = FindTemporaryStorageHeaderPivot();
			if (genPivot == null)
			{
				genPivot = Factory.New<GenPivot>();
				genPivot.XX_Relation1ID = PK;
				genPivot.XX_Relation1TableCode = CusTempStorageJobHeaderSchema.Constants.Prefix;
			}
			temporaryStorageHeaderPivot = genPivot;
			return genPivot;
		}

		public GenPivot FindTemporaryStorageHeaderPivot()
		{
			var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusTempStorageJobHeaderSchema.Constants.Prefix);
			var genPivot = Factory.LoadTop1<GenPivot>(pivotQuery);
			temporaryStorageHeaderPivot = genPivot;
			return genPivot;
		}

		GenPivot temporaryStorageHeaderPivot;

		#region synchroniser properties

		public ZPropertyInfo ReferenceNumberInfo
		{
			get { return GetWrappedZPropertyInfo(AutoCusTempStorageJobHeader.Schema.SJH_ReferenceNumber, x => this.SJH_ReferenceNumberInfo); }
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetWrappedZPropertyInfo(AutoCusTempStorageJobHeader.Schema.SJH_TransportMode, x => this.SJH_TransportModeInfo); }
		}

		public ZPropertyInfo TransportRegNoInfo
		{
			get { return GetWrappedZPropertyInfo(AutoCusTempStorageJobHeader.Schema.SJH_TransportRegNo, x => this.SJH_TransportRegNoInfo); }
		}

		public ZPropertyInfo NKLoadingInfo
		{
			get { return GetWrappedZPropertyInfo(AutoCusTempStorageJobHeader.Schema.SJH_RL_NKLoading, x => this.SJH_RL_NKLoadingInfo); }
		}
		#endregion

		#region Synching from Forwarding
		public CusTempStorageHeaderConsolSynchroniser ConsolSynchroniser
		{
			get
			{
				if (cusTempStorageHeaderConsolSynchroniser == null)
				{
					if (RelatedBusinessObject == null)
					{
						throw new NotSupportedException("Error : No related Business Object has been find, the synchronization cannot be done");
					}
					cusTempStorageHeaderConsolSynchroniser = GetNewConsolSynchroniser();
				}
				return cusTempStorageHeaderConsolSynchroniser;
			}
		}
		CusTempStorageHeaderConsolSynchroniser cusTempStorageHeaderConsolSynchroniser;

		protected CusTempStorageHeaderConsolSynchroniser GetNewConsolSynchroniser() => new CusTempStorageHeaderConsolSynchroniser(this);

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		public void SynchroniseWithParentIfNeeded()
		{
			if (ConsolSynchroniser != null)
			{
				try
				{
					using (SuspendSettingHasChanges())
					using (GetValidationSuspender())
					{
						ConsolSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						ConsolSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
					}
				}
				finally
				{
					ClearSynchronisationHasChanges();
				}
			}
		}

		void ClearSynchronisationHasChanges()
		{
			if (IsInDatabase)
			{
				((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
			}
		}
		#endregion

		public CusTempStorageRegHeader RegisterHeader
		{
			get
			{
				if (cachedRegisterHeader == null)
				{
					cachedRegisterHeader = new CachedProperty<CusTempStorageRegHeader>(Factory, () =>
					{
						CusTempStorageRegHeader registerHeader = null;
						if (!DDTNumber.IsEmpty)
						{
							registerHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, DDTNumber));
						}
						if (registerHeader == null && !SJH_JobReference.IsEmpty)
						{
							registerHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, SJH_JobReference));
						}
						return registerHeader;
					});
				}
				return cachedRegisterHeader.Value;
			}
		}
		CachedProperty<CusTempStorageRegHeader> cachedRegisterHeader;

		public CusTempStorageJobHeader PreviousISTHeader => SJH_PreviousReferenceType == FRConstants.TemporaryStorage.AppCodeIST ? ComplementaryJobISTFinder.FindFromReferenceNumber(Factory, SJH_PreviousReferenceNumber, CountryCode) : null;

		public ZInt PackageCount => CusTempStorageDec != null ? CusTempStorageDec.CusTempStorageLines.Cast<CusTempStorageLine>().Sum(x => x.TSL_PackageQty) : 0;

		public ZDecimal AmountToBeGuaranteedInDeclarationCurrency => CusTempStorageDec != null ? CusTempStorageDec.CusTempStorageLines.Cast<CusTempStorageLine>().SelectMany(x => x.CusTempStorageLineItems).Sum(x => x.TSI_GuaranteedValue) : 0;

		#region DDTNumber

		public bool AllocateDDTNumberOnSaving { get; set; }

		public void AllocateDDTNumber()
		{
			var errorHeader = Res.GetString("75170CB7-7148-4A72-92EE-D42E1BF790BE", "Cannot allocate DDT number");
			var invalidProfileMessage = Res.GetString("8500555A-30CB-4B65-BA46-19B942296616", "Please select one valid customs profile.");
			var noDDTNumberMessage = Res.GetString("F6CAB4A8-CEDD-4EF1-858C-D18A34B2C62A", "There is no valid DDT number ranges defined.\r\nPlease go to Authorization > Number Ranges to create one.");
			var noAvailableDDTNumberMessage = Res.GetString("A527E03A-5DE0-4F83-93DD-292E3EAE6EA2", "There is no more DDT numbers available.\r\nPlease go to Authorization > Number Ranges to allocate more numbers.");

			var authorisation = CustomsProfile
				?? throw new ZCannotSaveException(invalidProfileMessage, errorHeader);

			var provider = CustomsProfile.CustomsNumberProvider;
			var stmNums = provider.CustomsNumbers.OrderBy(x => x.SN_FountainName).FirstOrDefault(x => x.SN_Type == CusAuthorisationHeaderCustomsNumberRangeTypeList.Codes.TemporaryStorageInstallationDdtNumberFrance && x.SN_AvailableNumbers > 0)
				?? throw new ZCannotSaveException(noDDTNumberMessage, errorHeader);

			var numberFountain = stmNums.TryGetNumberFountain()
				?? throw new ZCannotSaveException(noDDTNumberMessage, errorHeader);

			try
			{
				var formattedDDTNumber = ZString.Empty;
				var duplicateNumberFound = true;
				while (duplicateNumberFound)
				{
					var nextNumber = (ZString)numberFountain.GetNextFormatted(Factory);
					formattedDDTNumber = stmNums.SN_FountainName + nextNumber.Right(6).PadLeft(6, '0');
					duplicateNumberFound = DDTNumberExist(formattedDDTNumber);
				}
				DDTNumber = formattedDDTNumber;
			}
			catch (NumberFountainMaximumValueReachedException)
			{
				throw new ZCannotSaveException(noAvailableDDTNumberMessage, errorHeader);
			}
		}

		bool DDTNumberExist(ZString ddtNumber)
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.France.DDT);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, CountryCode);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, ddtNumber);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, CusTempStorageJobHeaderSchema.Constants.TableName);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, this.PK);
			return new BusinessObjectFactory().Exists(typeof(CusEntryNumber), filter);
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString DDTNumber
		{
			get { return DDTEntryNumber.CE_EntryNum; }
			set { DDTEntryNumber.CE_EntryNum = value; }
		}

		public ZPropertyInfo DDTNumberInfo { get { return GetWrappedZPropertyInfo(Schema.DDTNumber, x => DDTEntryNumber.CE_EntryNumInfo); } }

		public CusEntryNumber DDTEntryNumber
		{
			get
			{
				if (ddtEntryNumber == null)
				{
					ddtEntryNumber = new CachedProperty<CusEntryNumber>(Factory, delegate
					{
						var ddtEntryNumberInternal = CusEntryNumber.Load(this, CusEntryNumberTypes.France.DDT, CountryCode);
						if (ddtEntryNumberInternal == null)
						{
							ddtEntryNumberInternal = CusEntryNumber.New(this, CusEntryNumberTypes.France.DDT, CountryCode);
							ddtEntryNumberInternal.CE_EntryIsSystemGenerated = true;
							ddtEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
						}
						return ddtEntryNumberInternal;
					});
				}
				return ddtEntryNumber.Value;
			}
		}
		CachedProperty<CusEntryNumber> ddtEntryNumber;

		#endregion

		#region  IBillGenerationSupport

		BusinessObjectFactory IBillGenerationSupport.Factory => Factory;

		OrgHeader IBillGenerationSupport.CarrierPrincipal => null;

		ZString IBillGenerationSupport.TransportMode => ZString.Empty;

		ZString IBillGenerationSupport.ServiceLevel => ZString.Empty;

		RefUNLOCO IBillGenerationSupport.Origin => null;

		RefUNLOCO IBillGenerationSupport.Destination => null;

		RefUNLOCO IBillGenerationSupport.Load => null;

		RefUNLOCO IBillGenerationSupport.Discharge => null;

		ZString IBillGenerationSupport.TranshipmentIndicator => ZString.Empty;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusTempStorageJobHeaderFetchStrategy(this);
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (AllocateDDTNumberOnSaving)
			{
				AllocateDDTNumber();
			}
		}
	}
}
