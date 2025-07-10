using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[DependentBusinessObject(typeof(NctsHeader), nameof(NctsHeader.ArrivalMovementHeader))]
	[SystemDefinedValues]
	[UserDefinedValues]
	public class NctsArrivalMovementHeader : NctsCommonMovementHeader
		, Integration.Customs.EU.NCTS.IArrivalMovementHeader
		, ICusGoodsLocationProviderWithValidationDecider
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, Integration.Customs.Shared.ICountryCodeProvider
		, IAdditionalBusinessObjectFetchStrategyProvider
		, EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport
		, INctsAdditionalInfoSequenceHeader
		, ICusGoodsLocationProviderWhichAllowsMixedCase
		, IWorkflowProvider
		, ICustomFieldProvider
		, IWorkflowAffectedPropertyProvider
	{
		public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!Header?.ReadOnly ?? false)
			{
				SetReadOnlyForUnloadingDifferencesData(UnloadingDifferenceDataReadOnly);
			}
		}

		public new class Schema : NctsCommonMovementHeader.Schema
		{
			public const string ArrivalStatusDescription = nameof(NctsArrivalMovementHeader.ArrivalStatusDescription);
			public const string AuthorizationCode = nameof(NctsArrivalMovementHeader.AuthorizationCode);
			public const string AuthorizationNumber = nameof(NctsArrivalMovementHeader.AuthorizationNumber);
			public const string AuthorizationOwner = nameof(NctsArrivalMovementHeader.AuthorizationOwner);
			public const string AuthorizationLocation = nameof(NctsArrivalMovementHeader.AuthorizationLocation);
			public const string BM_StateOfSealsBoolean = nameof(NctsArrivalMovementHeader.BM_StateOfSealsBoolean);
			public const string ExportFlag = nameof(NctsArrivalMovementHeader.ExportFlag);
		}

		public new static readonly NctsArrivalMovementHeaderTypeDecider TypeDecider = new NctsArrivalMovementHeaderTypeDecider();

		public new INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> GoodsItems => (INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>)base.GoodsItems;

		#region Properties

		public ZBool AutoPopulatedArrivalGoodsItems
		{
			get => this.GetSystemDefinedValue<ZBool>(nameof(AutoPopulatedArrivalGoodsItems));
			set
			{
				var oldValue = AutoPopulatedArrivalGoodsItems;
				this.SetSystemDefinedValue(nameof(AutoPopulatedArrivalGoodsItems), value);
				AutoPopulatedArrivalGoodsItemsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AutoPopulatedArrivalGoodsItemsInfo => GetZPropertyInfo(nameof(AutoPopulatedArrivalGoodsItems));

		[ResourceStringData("1CC996E3-66E2-4C4E-BDF1-C42ADA560FC5", Caption = "Arrival Date&Time", MediumCaption = "Arrival Date", ShortCaption = "Arr. D&T")]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public override ZDateTime BM_ArrivalDate { get => base.BM_ArrivalDate; set => base.BM_ArrivalDate = value; }

		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.CarnetTotalPagesList))]
		[ResourceStringData("47ADE538-C759-4B0C-BC13-A26C54F2784E", Caption = "Total Page Number", MediumCaption = "Page Number", ShortCaption = "Page No.")]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public override ZShort BM_CarnetTotalPages { get => base.BM_CarnetTotalPages; set => base.BM_CarnetTotalPages = value; }

		[ReadOnly(true)]
		[ResourceStringData("A1CF334F-3EEE-4F2C-8798-34C0AA7A1B3C", Caption = "Transport Mode (Inland)", ShortCaption = "Inland M.O.T.")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.ModeOfTransportList))]
		public override ZString BM_InlandTransportMode
		{
			get => base.BM_InlandTransportMode;
			set
			{
				var oldValue = BM_InlandTransportMode;
				base.BM_InlandTransportMode = value;
				if (!IsCopying && oldValue != BM_InlandTransportMode)
				{
					MovementDetails.MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.DischargeTypeList))]
		[ResourceStringData("2ED10B06-ADCB-4110-9978-E7B07DD8B0E1", Caption = "Discharge TIR", MediumCaption = "Discharge", ShortCaption = "Discharge")]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public override ZString BM_DischargeType
		{
			get => base.BM_DischargeType;
			set => base.BM_DischargeType = value;
		}

		[MaxLength(3)]
		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.NctsTransitStatusList))]
		[ResourceStringData("18E78935-A012-4867-8B64-ED6A367E3799", Caption = "Arrival Status")]
		public override ZString BM_CustomsStatus
		{
			get => base.BM_CustomsStatus;
			set
			{
				var oldValue = BM_CustomsStatus;
				base.BM_CustomsStatus = value;
				if (!IsCopying && oldValue != BM_CustomsStatus)
				{
					if (!IsMarkingAsNeedingValidationSuspended && Header is NctsHeader header)
					{
						header.MarkAsNeedingValidation();
						header.UnloadingMovementHeader?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("A6C35B80-E6BF-4AAD-AEE0-BDFF33AD49A1", Caption = "Arrival Status Description", ShortCaption = "Arr. St. Desc.")]
		public ZString ArrivalStatusDescription
		{
			get
			{
				var status = BM_CustomsStatus;
				var arrivalStatusDescription = ZString.Empty;
				if (!status.IsEmpty && status != NctsTransitStatusList.Codes.Unknown)
				{
					arrivalStatusDescription = Lookups.NctsTransitStatusList.GetDescriptionFromCode(status);
				}
				return arrivalStatusDescription;
			}
		}

		public ZPropertyInfo ArrivalStatusDescriptionInfo => GetZPropertyInfo(Schema.ArrivalStatusDescription);

		[ResourceStringData("07AEF98C-5951-4001-AD20-1F136663139D", Caption = "Simplified Arrival?", FullDescription = "Simplified Arrival Notification?")]
		public override ZBool IsSimplifiedNctsProcedure
		{
			get => base.IsSimplifiedNctsProcedure;
			set => base.IsSimplifiedNctsProcedure = value;
		}

		[MaxLength(17)]
		[ResourceStringData("36CBDA7E-0D0F-482D-A122-4E1778E4B31B", Caption = "Arrival Sub Place", FullDescription = "Arrival Customs Sub Place")]
		[ReadOnlyMember(nameof(ArrivalCustomsSubPlace_ReadOnly))]
		public override ZString BM_CustomsSubPlace
		{
			get => base.BM_CustomsSubPlace;
			set => base.BM_CustomsSubPlace = value;
		}

		[ResourceStringData("D0D597FA-1AE0-401A-9B16-19A0DC907064", Caption = "Acceptance Date", MediumCaption = "Accept. Date", ShortCaption = "Acc. Date")]
		public override ZDateTime BM_EntryDate
		{
			get => base.BM_EntryDate;
			set => base.BM_EntryDate = value;
		}

		[MaxLength(17)]
		[ResourceStringData("F7819D56-30FF-4609-AB74-C0E9D28100A1", Caption = "Arrival Goods Location", ShortCaption = "Arr. Loc. Code", FullDescription = "Arrival Location of goods (code)")]
		public override ZString BM_LocationOfGoodsCode
		{
			get => base.BM_LocationOfGoodsCode;
			set => base.BM_LocationOfGoodsCode = value;
		}

		[ResourceStringData("0F86F6C0-3AB8-4A89-A177-3C1AC4E1654F", Caption = "Arrived No. of Items", ShortCaption = "Arr. Items", FullDescription = "Arrival Total Number of Items")]
		public override ZInt TotalNumberOfItems => base.TotalNumberOfItems;

		[ResourceStringData("AA783D6F-922D-4D98-BAF9-98967BF3C320", Caption = "Arrived Total Packages", ShortCaption = "Arr. Packs", FullDescription = "Arrival Total Number of Packages")]
		public override ZLong TotalNumberOfPackages
		{
			get
			{
				if (IsPhase5)
				{
					return Header.Bills.Aggregate(ZLong.Zero, (current, bill) => current + bill.ArrivalGoodsItems.SelectMany(goodsItem => goodsItem.Packages.Cast<NctsPackage>())
						.Where(package => package.B5_B5_ParentPackage.IsEmpty && NctsHelper.IsUnloadedStateAccepted(package.B5_TypeOfDifference))
						.Sum(package => package.IsBulk && package.B5_UnitCount == 0 ? 1 : package.B5_UnitCount));
				}

				return base.TotalNumberOfPackages;
			}
		}

		[ResourceStringData("C697C583-42B7-455C-A834-3E913749FF8C", Caption = "Arrived Total Gross Weight (kg)", ShortCaption = "Arr. Gross Wgt.")]
		public override ZDecimal TotalGrossMassInKilograms => BM_GrossWeight;

		public virtual ZDecimal TotalUnloadedGrossMassInKilograms => Header.Bills.Aggregate(ZDecimal.Zero, (current, bill)
			=> current + bill.ArrivalGoodsItems.Where(goodsItem => goodsItem.BY_UnloadedState != NctsUnloadedStateList.Codes.MIS)
				.Cast<NctsArrivalCargoDesc>()
				.Sum(goodsItem => goodsItem.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF ? goodsItem.UnloadedGoodsItem.GrossMassInKilograms : goodsItem.GrossMassInKilograms));

		public ZPropertyInfo TotalUnloadedGrossMassInKilogramsInfo => GetZPropertyInfo(nameof(TotalUnloadedGrossMassInKilograms));

		public virtual ZLong TotalUnloadedNumberOfPackages => TotalUnloadedNumberOfPackagesCore;

		protected virtual ZLong TotalUnloadedNumberOfPackagesCore
		{
			get
			{
				var typeOfDifferenceFilter = new List<ZString> { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DEC };
				if (ShouldConsiderDIFUnloadedStateForPackageCount)
				{
					typeOfDifferenceFilter.Add(NctsUnloadedStateList.Codes.DIF);
				}

				return Header.Bills.Aggregate(ZLong.Zero, (current, bill)
						=> current + bill.ArrivalGoodsItems.Where(goodsItem => goodsItem.BY_UnloadedState != NctsUnloadedStateList.Codes.MIS)
						.SelectMany(goodsItem => goodsItem.Packages.Cast<NctsPackage>())
						.Where(package => package.B5_TypeOfDifference.In(typeOfDifferenceFilter))
						.Select(package => package.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF ? package.PackDifference : package)
						.Sum(package => package.IsBulk && package.B5_UnitCount == 0 ? 1 : package.B5_UnitCount));
			}
		}

		public ZPropertyInfo TotalUnloadedNumberOfPackagesInfo => GetZPropertyInfo(nameof(TotalUnloadedNumberOfPackages));

		protected virtual bool ArrivalCustomsSubPlace_ReadOnly => !IsSimplifiedNctsProcedure;

		protected virtual ZBool ShouldConsiderDIFUnloadedStateForPackageCount => false;

		[ChildEditable]
		public SealCollection Seals
		{
			get
			{
				if (seals == null)
				{
					seals = GetNewSealCollection();
					seals.Load();
					RegisterEditableChildObject(seals);
				}

				return seals;
			}
		}
		SealCollection seals;

		protected virtual SealCollection GetNewSealCollection() => new SealCollection<Seal>(this);
		#endregion

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>(this);

		public new NctsArrivalMovementHeaderLookups Lookups => (NctsArrivalMovementHeaderLookups)base.Lookups;

		protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsArrivalMovementHeaderLookups(this);

		public new NctsArrivalMovementHeaderValidation Validation => (NctsArrivalMovementHeaderValidation)base.Validation;

		protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsArrivalMovementHeaderValidation(this);

		[ChildEditable]
		public IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos
		{
			get
			{
				if (arrivalTransportInfos == null)
				{
					arrivalTransportInfos = GetNewArrivalTransportInfos();
					arrivalTransportInfos.Load();
					RegisterEditableChildObject(arrivalTransportInfos);
				}

				return arrivalTransportInfos;
			}
		}
		IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> arrivalTransportInfos;

		protected virtual IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> GetNewArrivalTransportInfos() => new ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);

		[ChildEditable(true)]
		public INctsGuaranteeCollection<NctsGuarantee> GuaranteesForArrival
		{
			get
			{
				if (guaranteesForArrival == null)
				{
					guaranteesForArrival = GetGuaranteesForArrivalCore();
					guaranteesForArrival.Load();
					RegisterEditableChildObject(guaranteesForArrival);
				}
				return guaranteesForArrival;
			}
		}
		INctsGuaranteeCollection<NctsGuarantee> guaranteesForArrival;

		protected virtual INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesForArrivalCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

		public NctsGuarantee SingleGuaranteeForArrival
		{
			get
			{
				if (GuaranteesForArrival.Count == 0)
				{
					GuaranteesForArrival.AddNew();
				}
				return GuaranteesForArrival[0];
			}
		}

		public ZString ExportFlag => Header.BH_ExportFlag;

		public ZPropertyInfo ExportFlagInfo => GetWrappedZPropertyInfo(nameof(ExportFlag), x => Header.BH_ExportFlagInfo);

		public ZBool ShouldGuaranteeForArrivalBeVisible => ShouldGuaranteeForArrivalBeVisibleCore;

		protected virtual ZBool ShouldGuaranteeForArrivalBeVisibleCore => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_SubApplicationCode = NctsMoveHeaderType.Codes.Arrival;
			BM_UnloadingDate = ZDateTimeOffset.Today;
			BM_UnloadingCompleted = true;
			BM_StateOfSealsBoolean = true;
			BM_NoChangesToReport = true;
		}

		public void SynchronizeArrivalAndUnloadingGoodsItems()
		{
			var nctsHeader = Header;

			if (!nctsHeader.Configuration.ReceiveIE043UnloadingPermissionDetailsMessage)
			{
				if (!AutoPopulatedArrivalGoodsItems)
				{
					var unloadingMovement = nctsHeader.UnloadingMovementHeader;

					DeleteApplicableGoodsItem(unloadingMovement);

					CopyToUnloadingGoodsItems(unloadingMovement);
				}
			}
		}

		void DeleteApplicableGoodsItem(NctsUnloadingMovementHeader unloadingMovement)
		{
			var goodsItemsToDelete = new List<NctsArrivalAndUnloadingCargoDesc>();
			unloadingMovement.GoodsItems.ForEach(x =>
			{
				var currentItem = x;
				if (!currentItem.IsNew && !GoodsItems.Any(y => y.BY_LineNo == currentItem.BY_LineNo))
				{
					goodsItemsToDelete.Add(currentItem);
				}
			});

			goodsItemsToDelete.ForEach(x => x.Delete());
		}

		void CopyToUnloadingGoodsItems(NctsUnloadingMovementHeader unloadingMovement)
		{
			var newGoodsItems = GoodsItems.Where(x => !unloadingMovement.GoodsItems.Any(y => y.BY_LineNo == x.BY_LineNo));

			foreach (var goodsItem in newGoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>())
			{
				var unloadingGoodsItem = unloadingMovement.GoodsItems.AddNew();
				var args = new BusinessObjectCloneArgs(new[] { CusInBondCargoDescSchema.Constants.BY_ParentTableCode, CusInBondCargoDescSchema.Constants.BY_ParentID });
				unloadingGoodsItem.CopyPersistentValuesFrom(goodsItem, args);
				unloadingGoodsItem.IsNew = false;
			}

			foreach (var arrivalGoodsItem in newGoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>())
			{
				var unloadingGoodsItem = unloadingMovement.GoodsItems.FirstOrDefault(x => x.BY_LineNo == arrivalGoodsItem.BY_LineNo);
				CopyContainerPackagesAndDocumentsToUnloadingGoodsItems(arrivalGoodsItem, unloadingGoodsItem);
			}
		}

		public void PopulateArrivalGoodsItemsFromDeparture(NctsDepartureMovementHeader departureMovementHeader)
		{
			AutoPopulatedArrivalGoodsItems = true;

			GoodsItems.DeleteAll();
			Header.UnloadingMovementHeader.GoodsItems.DeleteAll();

			foreach (var goodsItem in departureMovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>())
			{
				var arrivalGoodsItem = GoodsItems.AddNew();
				var args = new BusinessObjectCloneArgs(new[] { CusInBondCargoDescSchema.Constants.BY_ParentTableCode, CusInBondCargoDescSchema.Constants.BY_ParentID });
				arrivalGoodsItem.CopyPersistentValuesFrom(goodsItem, args);

				if (arrivalGoodsItem.BY_Description.IsEmpty)
				{
					arrivalGoodsItem.BY_Description = goodsItem.BY_HarmonisedTariff;
				}

				CopyContainerPackagesAndDocumentsToArrivalGoodsItems(goodsItem, arrivalGoodsItem);

				var unloadingGoodsItem = Header.UnloadingMovementHeader.GoodsItems.AddNew();
				unloadingGoodsItem.CopyPersistentValuesFrom(goodsItem, args);
				unloadingGoodsItem.IsNew = false;
			}

			GoodsItems.SetArrivalGoodsItemsReadOnly();
		}

		void CopyContainerPackagesAndDocumentsToArrivalGoodsItems(NctsDepartureCargoDesc goodsItem, NctsArrivalAndUnloadingCargoDesc arrivalGoodsItem)
		{
			goodsItem.ContainersPivots.Select(x => x.Container).ForEach(x => CopyContainer(x, arrivalGoodsItem));

			goodsItem.Packages.ForEach(x => CopyPackage(x, arrivalGoodsItem));

			CopyDocumentsToArrivalGoodsItems(goodsItem, arrivalGoodsItem);
		}

		protected virtual void CopyDocumentsToArrivalGoodsItems(NctsDepartureCargoDesc goodsItem, NctsArrivalAndUnloadingCargoDesc arrivalGoodsItem)
		{
			goodsItem.SupportingDocuments.ForEach(x => CopySupportingDocument(x, arrivalGoodsItem));
		}

		void CopyContainerPackagesAndDocumentsToUnloadingGoodsItems(NctsArrivalAndUnloadingCargoDesc arrivalGoodsItem, NctsArrivalAndUnloadingCargoDesc unloadingGoodsItem)
		{
			unloadingGoodsItem.Containers.DeleteAll();
			arrivalGoodsItem.Containers.ForEach(x => CopyContainer(x, unloadingGoodsItem));

			unloadingGoodsItem.Packages.DeleteAll();
			arrivalGoodsItem.Packages.ForEach(x => CopyPackage(x, unloadingGoodsItem));

			unloadingGoodsItem.SupportingDocuments.RemoveAndDeleteAll();
			arrivalGoodsItem.SupportingDocuments.ForEach(x => CopySupportingDocument(x, unloadingGoodsItem));
		}

		void CopyContainer(BusinessObject containerToCopy, NctsArrivalAndUnloadingCargoDesc goodsItem)
		{
			var newContainer = goodsItem.Containers.AddNew();
			var containerArgs = new BusinessObjectCloneArgs(new[] { CusInBondContainerSchema.Constants.BC_ParentID, CusInBondContainerSchema.Constants.BC_ParentTableCode });
			newContainer.CopyPersistentValuesFrom(containerToCopy, containerArgs);
		}

		void CopyPackage(BusinessObject packageToCopy, NctsArrivalAndUnloadingCargoDesc goodsItem)
		{
			var newPackage = goodsItem.Packages.AddNew();
			var packageArgs = new BusinessObjectCloneArgs(new[] { CusInvPackSchema.Constants.B5_ParentID, CusInvPackSchema.Constants.B5_ParentTableCode });
			newPackage.CopyPersistentValuesFrom(packageToCopy, packageArgs);
		}

		protected NctsSupportingDocument CopySupportingDocument(BusinessObject supportingDocToCopy, NctsArrivalAndUnloadingCargoDesc goodsItem)
		{
			var newDocument = goodsItem.SupportingDocuments.AddNew();
			var documentArgs = new BusinessObjectCloneArgs(new[] { CusSupportingInfoSchema.Constants.CSI_ParentID, CusSupportingInfoSchema.Constants.CSI_ParentTableCode });
			newDocument.CopyPersistentValuesFrom(supportingDocToCopy, documentArgs);
			return newDocument;
		}

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsArrivalAndUnloadingCargoDesc);

		[ChildEditable]
		public new CusInBondMoveDetailCollection MovementDetails => (CusInBondMoveDetailCollection)base.MovementDetails;

		protected override ICusInBondMoveDetailCollection CreateMovementDetails() => new CusInBondMoveDetailCollection(this);

		protected override Type MovementDetailTypeCore => Header.IsPhase5 ? typeof(CusInBondMoveDetail) : null;

		public bool IsCustomsStatusARTAndPhaseFRC => BM_CustomsStatus == NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival && BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.FullReleaseOfGoodsMovementClosed;

		internal CusAuthorizationUsage LoadAuthorization()
		{
			var header = Header;
			if (authorization == null || authorization.IsDeleted || authorization.AGC_ParentID != header.PK || authorization.AGC_ParentTableCode != header.TablePrefix)
			{
				authorization = header.CusAuthorizationUsages.OrderBy(x => x.AGC_SystemCreateTimeUtc).FirstOrDefault();
			}
			return authorization;
		}
		CusAuthorizationUsage authorization;

		CusAuthorizationUsage LoadOrCreateAuthorization()
		{
			LoadAuthorization();
			if (authorization == null)
			{
				authorization = Header.CusAuthorizationUsages.AddNew();
			}
			return authorization;
		}

		protected virtual void UpdateAfterAuthorizationDataChange()
		{
			if (!AuthorizationCode.IsEmpty && !AuthorizationNumber.IsEmpty && !AuthorizationOwner.IsEmpty)
			{
				IsSimplifiedNctsProcedure = true;
			}
			else
			{
				IsSimplifiedNctsProcedure = false;
			}

			if (AuthorizationCode.IsEmpty && AuthorizationNumber.IsEmpty && AuthorizationLocation.IsEmpty
				&& (AuthorizationOwner.IsEmpty || (IsPhase5Arrival && ShouldSyncDestinationTraderWithAuthorization)))
			{
				authorization?.Delete();
				authorization = null;
			}

			if (IsPhase5Arrival && ShouldSyncDestinationTraderWithAuthorization)
			{
				if (!AuthorizationOwner.IsEmpty)
				{
					Header.DestinationTrader.OrganisationPK = AuthorizationOwner;
				}
				else if (!AuthorizationCode.IsEmpty || !AuthorizationNumber.IsEmpty)
				{
					AuthorizationOwner = Header.DestinationTrader.OrganisationPK;
				}
			}
		}

		public ZBool ShouldSyncDestinationTraderWithAuthorization => ShouldSyncDestinationTraderWithAuthorizationCore;
		protected virtual ZBool ShouldSyncDestinationTraderWithAuthorizationCore => true;

		[ResourceStringData("9979CFE5-7345-4EA4-8568-C03836E0BD4C", Caption = "Authorization Code", MediumCaption = "Code")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.AuthorizationCodeList))]
		[MaxLength(CusAuthorizationUsage.Schema.AGC_CodeMaxLength)]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public virtual ZString AuthorizationCode
		{
			get => LoadAuthorization()?.AGC_Code ?? ZString.Empty;
			set
			{
				var oldValue = AuthorizationCode;
				LoadOrCreateAuthorization().AGC_Code = value;
				if (!IsCopying && oldValue != AuthorizationCode)
				{
					UpdateAfterAuthorizationDataChange();
					MarkAsNeedingValidation();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizationCode();
				}
				AuthorizationCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AuthorizationCodeInfo => GetZPropertyInfo(Schema.AuthorizationCode);

		[ResourceStringData("7441AAE3-1FA8-4686-8DF6-F1A9AC303AB5", Caption = "Authorization Number", MediumCaption = "Authorization No.")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.AuthorizationNumberList))]
		[MaxLength(CusAuthorizationUsage.Schema.AGC_NumberMaxLength)]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public virtual ZString AuthorizationNumber
		{
			get => LoadAuthorization()?.AGC_Number ?? ZString.Empty;
			set
			{
				var oldValue = AuthorizationNumber;
				CheckMaximumLength(AuthorizationNumberInfo, value);
				LoadOrCreateAuthorization().AGC_Number = value;
				if (!IsCopying && oldValue != AuthorizationNumber)
				{
					UpdateAfterAuthorizationDataChange();
					MarkAsNeedingValidation();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizationNumber();
				}
				AuthorizationNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AuthorizationNumberInfo => GetZPropertyInfo(Schema.AuthorizationNumber);

		[ResourceStringData("262ACF40-55F4-4579-9A6F-A0B1063DBF39", Caption = "Authorization Owner", MediumCaption = "Owner", ShortCaption = "Own.")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.Organisations))]
		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public virtual ZGuid AuthorizationOwner
		{
			get => LoadAuthorization()?.AGC_OH_Owner ?? ZGuid.Empty;
			set
			{
				var oldValue = AuthorizationOwner;
				LoadOrCreateAuthorization().AGC_OH_Owner = value;
				if (!IsCopying && oldValue != AuthorizationOwner)
				{
					UpdateAfterAuthorizationDataChange();
					MarkAsNeedingValidation();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizationOwner();
				}
				AuthorizationOwnerInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AuthorizationOwnerInfo => GetZPropertyInfo(Schema.AuthorizationOwner);

		[MaxLength(CusAuthorizationUsage.Schema.AGC_LocationMaxLength)]
		public virtual ZString AuthorizationLocation
		{
			get => LoadAuthorization()?.AGC_Location ?? ZString.Empty;
			set
			{
				var oldValue = AuthorizationLocation;
				CheckMaximumLength(AuthorizationLocationInfo, value);
				LoadOrCreateAuthorization().AGC_Location = value;
				if (!IsCopying && oldValue != AuthorizationLocation)
				{
					UpdateAfterAuthorizationDataChange();
				}
				AuthorizationLocationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AuthorizationLocationInfo => GetZPropertyInfo(Schema.AuthorizationLocation);

		public CusInBondMoveDetail DeclaredMovementDetail
		{
			get
			{
				if (declaredMovementDetail == null)
				{
					declaredMovementDetail = MovementDetails.SingleOrDefault(x => x.B9_B0.IsEmpty && x.B9_UnloadedState == NctsUnloadedStateList.Codes.DEC);
					declaredMovementDetail?.SetReadOnlyIncludingChildren(true);
				}
				return declaredMovementDetail;
			}
		}
		CusInBondMoveDetail declaredMovementDetail;

		public CusInBondMoveDetail UnloadedMovementDetail
		{
			get
			{
				if (unloadedMovementDetail == null)
				{
					unloadedMovementDetail = MovementDetails.SingleOrDefault(x => x.B9_B0.IsEmpty && x.B9_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF }));
				}
				return unloadedMovementDetail;
			}
		}
		CusInBondMoveDetail unloadedMovementDetail;

		[ChildEditable(true)]
		public INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = GetSupportingDocuments();
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
					supportingDocuments.SetReadOnlyIncludingChildren(ShouldSetSupportingDocumentsReadOnly);
				}
				return supportingDocuments;
			}
		}
		INctsSupportingDocumentCollection<NctsSupportingDocument> supportingDocuments;

		protected virtual ZBool ShouldSetSupportingDocumentsReadOnly => true;

		[ChildEditable(true)]
		public INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments
		{
			get
			{
				if (additionalDocuments == null)
				{
					additionalDocuments = GetAdditionalDocuments();
					additionalDocuments.Load();
					RegisterEditableChildObject(additionalDocuments);
					additionalDocuments.SetReadOnlyIncludingChildren(ShouldSetAdditionalDocumentsReadOnly);
				}
				return additionalDocuments;
			}
		}
		INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalDocuments;

		protected virtual ZBool ShouldSetAdditionalDocumentsReadOnly => true;

		protected virtual INctsSupportingDocumentCollection<NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected virtual INctsAdditionalInfoCollection<NctsAdditionalInfo> GetAdditionalDocuments() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		[ResourceStringData("52950436-C3AA-4101-825A-03F00A319BAA", Caption = "Transport ID")]
		[MaxLength(CusInBondMoveDetail.Schema.B9_TransportAtDepartureIDMaxLength)]
		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		public ZString UnloadedB9_TransportAtDepartureID
		{
			get => UnloadedMovementDetail?.B9_TransportAtDepartureID ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.B9_TransportAtDepartureID = value);
				UnloadedB9_TransportAtDepartureIDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedB9_TransportAtDepartureIDInfo => GetZPropertyInfo(nameof(UnloadedB9_TransportAtDepartureID));

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("6107CE8E-0D0C-4853-9D41-D2BCFA623F35", Caption = "Nationality")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.TransportAtDepartureNationalities))]
		[MaxLength(CusInBondMoveDetail.Schema.B9_RN_NKTransportAtDepartureIDNationalityMaxLength)]
		public ZString UnloadedB9_RN_NKTransportAtDepartureIDNationality
		{
			get => UnloadedMovementDetail?.B9_RN_NKTransportAtDepartureIDNationality ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.B9_RN_NKTransportAtDepartureIDNationality = value);
				UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedB9_RN_NKTransportAtDepartureIDNationalityInfo => GetZPropertyInfo(nameof(UnloadedB9_RN_NKTransportAtDepartureIDNationality));

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("F650D96D-2BA5-4E70-AD0F-36056B2F0739", Caption = "Trailer ID 1")]
		[MaxLength(CusInBondMoveDetail.Schema.B9_TransportAtDepartureTrailer1RegNoMaxLength)]
		public ZString UnloadedB9_TransportAtDepartureTrailer1RegNo
		{
			get => UnloadedMovementDetail?.B9_TransportAtDepartureTrailer1RegNo ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.B9_TransportAtDepartureTrailer1RegNo = value);
				UnloadedB9_TransportAtDepartureTrailer1RegNoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedB9_TransportAtDepartureTrailer1RegNoInfo => GetZPropertyInfo(nameof(UnloadedB9_TransportAtDepartureTrailer1RegNo));

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("B76DDBBB-93EF-453D-87BF-9F9F2E11C9F3", Caption = "Nationality")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.TransportAtDepartureNationalities))]
		[MaxLength(CusInBondMoveDetail.Schema.B9_RN_NKTransportAtDepartureTrailer1NationalityMaxLength)]
		public ZString UnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality
		{
			get => UnloadedMovementDetail?.B9_RN_NKTransportAtDepartureTrailer1Nationality ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.B9_RN_NKTransportAtDepartureTrailer1Nationality = value);
				UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedB9_RN_NKTransportAtDepartureTrailer1NationalityInfo => GetZPropertyInfo(nameof(UnloadedB9_RN_NKTransportAtDepartureTrailer1Nationality));

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("0A24171F-C64A-4E92-8259-2BBFBD3A13B7", Caption = "Trailer ID 2")]
		[MaxLength(CusInBondMoveDetail.Schema.B9_TransportAtDepartureTrailer2RegNoMaxLength)]
		public ZString UnloadedB9_TransportAtDepartureTrailer2RegNo
		{
			get => UnloadedMovementDetail?.B9_TransportAtDepartureTrailer2RegNo ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.B9_TransportAtDepartureTrailer2RegNo = value);
				UnloadedB9_TransportAtDepartureTrailer2RegNoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedB9_TransportAtDepartureTrailer2RegNoInfo => GetZPropertyInfo(nameof(UnloadedB9_TransportAtDepartureTrailer2RegNo));

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("4715E6E6-E9A9-4567-B26F-A49A44741D1F", Caption = "Nationality")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.TransportAtDepartureNationalities))]
		[MaxLength(CusInBondMoveDetail.Schema.B9_RN_NKTransportAtDepartureTrailer2NationalityMaxLength)]
		public ZString UnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality
		{
			get => UnloadedMovementDetail?.B9_RN_NKTransportAtDepartureTrailer2Nationality ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.B9_RN_NKTransportAtDepartureTrailer2Nationality = value);
				UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedB9_RN_NKTransportAtDepartureTrailer2NationalityInfo => GetZPropertyInfo(nameof(UnloadedB9_RN_NKTransportAtDepartureTrailer2Nationality));

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("42940619-017A-4EE9-BCC0-29BD63871954", Caption = "Aircraft ID")]
		[MaxLength(CusInBondMoveDetail.Schema.B9_AircraftIDAtDepartureMaxLength)]
		public ZString UnloadedB9_AircraftIDAtDeparture
		{
			get => UnloadedMovementDetail?.B9_AircraftIDAtDeparture ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.B9_AircraftIDAtDeparture = value);
				UnloadedB9_AircraftIDAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedB9_AircraftIDAtDepartureInfo => GetZPropertyInfo(nameof(UnloadedB9_AircraftIDAtDeparture));

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("1BCC4512-25B1-423A-BFDD-1EB58A2222CE", Caption = "Vessel")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.Vessels))]
		[MaxLength(CusInBondMoveDetail.Schema.B9_TransportAtDepartureIDMaxLength)]
		public ZString UnloadedVesselNameAtDeparture
		{
			get => UnloadedMovementDetail?.VesselNameAtDeparture ?? ZString.Empty;
			set
			{
				UpdatedUnloadedMovementDetailOrCreateIfNotExists(x => x.VesselNameAtDeparture = value);
				UnloadedVesselNameAtDepartureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnloadedVesselNameAtDepartureInfo => GetZPropertyInfo(nameof(UnloadedVesselNameAtDeparture));

		void UpdatedUnloadedMovementDetailOrCreateIfNotExists(Action<CusInBondMoveDetail> updateProperty)
		{
			if (UnloadedMovementDetail == null || UnloadedMovementDetail.IsDeleted)
			{
				unloadedMovementDetail = MovementDetails.AddNew();
				if (DeclaredMovementDetail is CusInBondMoveDetail declared)
				{
					unloadedMovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
					unloadedMovementDetail.B9_B9_InBondMoveDetail = declared.PK;
				}
				else
				{
					unloadedMovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				}
			}
			updateProperty.Invoke(UnloadedMovementDetail);
		}

		#region OtherThingsToReport

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("E3EB18F3-69F7-4C09-8518-DA03A88E3475", Caption = "Report here the things that happened outside the unloading but had/have an effect on the cargo unloaded", MediumCaption = "Other things to report", ShortCaption = "Other info")]
		public ZString OtherThingsToReport
		{
			get { return OtherThingsToReportNoteWriter.Value; }
			set
			{
				CheckMaximumLength(OtherThingsToReportInfo, value);
				if (OtherThingsToReportNoteWriter.UpdateValue(value))
				{
					OtherThingsToReportInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo OtherThingsToReportInfo => GetZPropertyInfo(nameof(OtherThingsToReport));

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.OtherThingsToReport);
				return noteTypes;
			}
		}

		PredefinedNoteWriter OtherThingsToReportNoteWriter
		{
			get { return otherThingsToReportNoteWriter ?? (otherThingsToReportNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.OtherThingsToReport)); }
		}
		PredefinedNoteWriter otherThingsToReportNoteWriter;

		#endregion

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("CCC7A2C3-F53B-4F3F-8350-54178C403A2F", Caption = "All seals present and undamaged", MediumCaption = "All seals are OK", ShortCaption = "Seals OK")]
		public ZBool BM_StateOfSealsBoolean
		{
			get
			{
				if (base.BM_StateOfSeals.IsEmpty)
				{
					return false;
				}
				return ZBool.ParseSafe(base.BM_StateOfSeals, false);
			}
			set
			{
				var oldValue = BM_StateOfSeals;
				base.BM_StateOfSeals = value.ToString();

				if (!IsCopying && oldValue != BM_StateOfSeals)
				{
					if (Header != null)
					{
						foreach (NctsArrivalHeaderContainer container in Header.ArrivalHeaderContainers)
						{
							container.Seals.SetReadOnlyIncludingChildren(value);
						}
					}
				}
			}
		}

		public ZWrappedPropertyInfo BM_StateOfSealsBooleanInfo => GetWrappedZPropertyInfo(nameof(BM_StateOfSealsBoolean), x => BM_StateOfSealsInfo);

		[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.YesNoList))]
		public override ZString BM_StateOfSeals
		{
			get => base.BM_StateOfSeals;
			set
			{
				var oldValue = BM_StateOfSeals;
				base.BM_StateOfSeals = value;

				if (!IsCopying && oldValue != BM_StateOfSeals)
				{
					if (Header != null)
					{
						Header.ArrivalHeaderContainers.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("83B2EEA7-5C75-4DA2-BD65-8DD431EA87E1", Caption = "Date of unloading", MediumCaption = "Unloading Date", ShortCaption = "Date")]
		public override ZDateTimeOffset BM_UnloadingDate { get => base.BM_UnloadingDate; set => base.BM_UnloadingDate = value; }

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("71696F4C-852F-42EB-AA7B-0010C82A9DE0", Caption = "Unloaded cargo conforms to declaration", MediumCaption = "Unloading conforms", ShortCaption = "Conforms")]
		public override ZBool BM_NoChangesToReport
		{
			get => base.BM_NoChangesToReport;
			set
			{
				var oldValue = BM_NoChangesToReport;
				base.BM_NoChangesToReport = value;
				if (!IsCopying && oldValue != BM_NoChangesToReport)
				{
					SetReadOnlyForUnloadingDifferencesData(value);
					if (!BM_NoChangesToReport)
					{
						if (BM_GrossWeightUnloaded == 0)
						{
							ResetUnloadedWeightToDeclared();
						}
					}
					else
					{
						BM_GrossWeightUnloaded = 0;
					}
				}
			}
		}

		void ResetUnloadedWeightToDeclared()
		{
			BM_GrossWeightUnloaded = BM_GrossWeight;
		}

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("9F5B6BCD-D2CA-409D-9E9C-A9ACA635113B", Caption = "Unloading complete", ShortCaption = "Completed")]
		public override ZBool BM_UnloadingCompleted { get => base.BM_UnloadingCompleted; set => base.BM_UnloadingCompleted = value; }

		[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
		[ResourceStringData("3256452D-2E7D-4D42-9B98-1C1323D22228", Caption = "Enter the remarks regarding the unloading", MediumCaption = "Unloading remarks", ShortCaption = "Remarks")]
		public override ZString BM_UnloadingRemarks { get => base.BM_UnloadingRemarks; set => base.BM_UnloadingRemarks = value; }

		public virtual bool AreUnloadingRemarksFullyAccepted => IsPhase5 && Header.CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks) && Header.IsLocked;

		public virtual bool IsArrivalDetailsReadOnly => Header.IsArrivalDetailsReadOnly;

		public virtual bool IsUnloadingRemarksReadOnly => Header != null && Header.IsPhase5 && (Header.MessageHasBeenSent || AreUnloadingRemarksFullyAccepted);

		protected override bool BM_PaperlessInbondNumReadOnly => IsPhase5 && !Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.Value;

		protected override bool ShouldGenerateLocalReferenceNumberOnSavingCore => true;

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		public bool IsArrivalNotificationDisabled => Header.ReadOnly || Header.IsArrivalNotificationDisabled || IsArrivalDetailsReadOnly;

		public bool UnloadingDifferenceDataReadOnly => BM_NoChangesToReport;

		#region ICusGoodsLocationProvider

		EU.Business.CusGoodsLocation ICusGoodsLocationProvider.GoodsLocation => GoodsLocation;

		[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
		public CusGoodsLocation GoodsLocation
		{
			get
			{
				if (goodsLocation == null)
				{
					goodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Arrival);
					RegisterEditableChildObject(goodsLocation);
				}
				goodsLocation.SetReadOnlyIncludingChildren(IsArrivalNotificationDisabled);
				return goodsLocation;
			}
		}
		CusGoodsLocation goodsLocation;

		[ResourceStringData("D3A32EA7-5BAA-4286-B219-4C65EDDE8A64", Caption = "Location of Goods", ShortCaption = "Location")]
		public ZString GoodsLocationDescription
		{
			get
			{
				if (goodsLocation == null)
				{
					goodsLocation = Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Arrival);
					if (goodsLocation != null)
					{
						RegisterEditableChildObject(goodsLocation);
					}
				}
				goodsLocation?.SetReadOnlyIncludingChildren(IsArrivalNotificationDisabled);
				return goodsLocation?.DisplayText ?? ZString.Empty;
			}
		}

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		public void ValidateGoodsLocationDescription()
		{
			Validation.ValidateGoodsLocationDescription();
		}

		ZString ICusGoodsLocationProvider.ProviderKey => DefaultDataGroupingCode + GoodsLocationProviderApplications.Codes.NCTSMovement;

		#endregion

		ICusGoodsLocationValidationDecider ICusGoodsLocationProviderWithValidationDecider.GoodsLocationValidationDecider => Header?.Configuration.MovementHeaderConfiguration.GetGoodsLocationValidationDecider(this);

		#region ICusSupportingInfoTypeSupporter Implementation

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.SupportingDocument, SupportingDocumentType },
			{ CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoType },
		};

		protected virtual Type AdditionalInfoType => NctsTypeDecider.GetNctsAdditionalInfoType(Factory, CountryCode);
		protected virtual Type SupportingDocumentType => typeof(NctsSupportingDocument);

		#endregion

		#region ICanBeImportOrExport Members

		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		ZBool EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.IsExport => false;

		ZBool EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.IsImport => false;

		void EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.TrueCountryCode => CountryCode;
		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.DataGroupingCode => CountryCode;

		#endregion

		#region ISequenceNumberHeader
		public IEnumerable<IShortSequenceNumberLine> AdditionalDocumentLines => AdditionalDocuments;

		public ShortSequenceNumberGenerator RefSequenceNumberGenerator
		{
			get
			{
				return refLineNumberGenerator ?? (refLineNumberGenerator = new ShortSequenceNumberGenerator(() => AdditionalDocumentLines, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference));
			}
		}
		ShortSequenceNumberGenerator refLineNumberGenerator;

		public ShortSequenceNumberGenerator InfSequenceNumberGenerator
		{
			get
			{
				return infLineNumberGenerator ?? (infLineNumberGenerator = new ShortSequenceNumberGenerator(() => AdditionalDocumentLines, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation));
			}
		}
		ShortSequenceNumberGenerator infLineNumberGenerator;

		public ShortSequenceNumberGenerator TraSequenceNumberGenerator
		{
			get
			{
				return traLineNumberGenerator ?? (traLineNumberGenerator = new ShortSequenceNumberGenerator(() => AdditionalDocumentLines, (x) => ((NctsAdditionalInfo)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument));
			}
		}
		ShortSequenceNumberGenerator traLineNumberGenerator;

		#endregion

		void SetReadOnlyForUnloadingDifferencesData(bool readOnly) => SetReadOnlyForUnloadingDifferencesDataCore(readOnly);

		protected virtual void SetReadOnlyForUnloadingDifferencesDataCore(bool readOnly)
		{
			if (Header is NctsHeader header)
			{
				var isInPhase5TransitionPeriod = header.IsInPhase5TransitionPeriod;
				foreach (var bill in header.Bills)
				{
					bill.SetReadOnlyIncludingChildren(readOnly);
					if (!BM_NoChangesToReport)
					{
						bill.ArrivalTransportInfos.SetReadOnlyIncludingChildren(isInPhase5TransitionPeriod);
					}
				}
				header.Bills.RefreshBinding();
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				GuaranteesForArrival.Delete();
				WorkflowItems.RemoveAndDeleteAll();
			}
			base.Delete();
		}
		protected override ICustomsOffice GetDestinationCustomsOffice() => DestinationCustomsOfficeForArrival;

		public override ZString DestinationCustomsOfficeCode => DestinationCustomsOfficeCodeForArrival;

		public override ZString DestinationCustomsOfficeCodeForArrival
		{
			get => base.DestinationCustomsOfficeCodeForArrival;
			set
			{
				if (!IsValidationSuspended)
				{
					base.DestinationCustomsOfficeCodeForArrival = value;
					Validation.ValidateDestinationCustomsOfficeCodeForArrival();
				}
				DestinationCustomsOfficeCodeForArrivalInfo.RefreshBinding();
			}
		}

		public override ZString BM_MessageStatus
		{
			get => base.BM_MessageStatus;
			set
			{
				base.BM_MessageStatus = value;
				GoodsItems?.MarkAsNeedingValidation();
				Header?.UnloadingMovementHeader?.MarkAsNeedingValidation();
			}
		}

		public override ZDateTime BM_ValuationDate
		{
			get => base.BM_ValuationDate;
			set
			{
				var oldValue = BM_ValuationDate;
				base.BM_ValuationDate = value;
				if (BM_ValuationDate != oldValue)
				{
					Header?.Bills?.ForEach(x => x.ArrivalGoodsItems.ForEach(y => y.UnloadedGoodsItem?.MarkAsNeedingValidation()));
				}
			}
		}

		[ReadOnlyMember(nameof(EffectiveGrossWeightUnloadedReadOnly))]
		public ZDecimal EffectiveGrossWeightUnloaded
		{
			get => BM_NoChangesToReport == false ? BM_GrossWeightUnloaded : TotalUnloadedGrossMassInKilograms;
			set
			{
				var oldValue = BM_GrossWeightUnloaded;
				if (!IsCopying && oldValue != value)
				{
					BM_GrossWeightUnloaded = value;
					EffectiveGrossWeightUnloadedInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EffectiveGrossWeightUnloadedInfo => GetWrappedZPropertyInfo(nameof(EffectiveGrossWeightUnloaded), x => BM_GrossWeightUnloadedInfo);

		public bool EffectiveGrossWeightUnloadedReadOnly => BM_NoChangesToReport || IsUnloadingRemarksReadOnly;

		public event EventHandler OnFactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms;

		public void FactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms(object sender, EventArgs args) => OnFactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms?.Invoke(sender, args);

		public void UpdateGrossWeightsUnloaded()
		{
			UpdateBM_GrossWeightUnloadedFromTotalGrossMassInKilograms();
			UpdateBillsGrossWeightUnloaded();

			void UpdateBillsGrossWeightUnloaded()
			{
				var bills = Header.Bills;
				var billsWithUnloadedStateDIF = bills.Where(x => x.MovementDetail is CusInBondMoveDetail movementDetail && movementDetail.B9_UnloadedState == NctsUnloadedStateList.Codes.DIF);
				billsWithUnloadedStateDIF.ForEach(x => x.B0_GrossWeightUnloaded = x.MovementDetail.DifferenceMoveDetail is CusInBondMoveDetail differenceMoveDetail ? differenceMoveDetail.DifferenceWeight : ZDecimal.Zero);
				bills.Except(billsWithUnloadedStateDIF).ForEach(x => x.B0_GrossWeightUnloaded = ZDecimal.Zero);
			}
		}

		public void UpdateBM_GrossWeightUnloadedFromTotalGrossMassInKilograms()
		{
			BM_GrossWeightUnloaded = BM_NoChangesToReport == false ? TotalUnloadedGrossMassInKilograms : ZDecimal.Zero;
			EffectiveGrossWeightUnloadedInfo.RefreshBinding();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (BM_GrossWeightUnloaded > TotalUnloadedGrossMassInKilograms)
			{
				FactorySavingAndGrossWeightUnloadedGreaterThanTotalGrossMassInKilograms(this, new EventArgs());
			}
			else
			{
				UpdateGrossWeightsUnloaded();
			}
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		public ZBool AllowMixedCaseAuthorisationNumbers => AllowMixedCaseAuthorisationNumbersCore;

		protected virtual ZBool AllowMixedCaseAuthorisationNumbersCore => Header?.Configuration.AllowMixedCaseAuthorisationNumbers ?? ZBool.False;

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			var header = Header;
			if (header != null)
			{
				if (!Header.DestinationTrader.OrganisationPK.IsEmpty)
				{
					result = AdjustClientPriority(header.GetJobRelatedTemplateSelectionCriteria());
				}
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, ExportFlag, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType2, AuthorizationCode, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType3, DestinationCustomsOfficeCodeForArrival.Right(ProcessTaskTemplateSchema.P0_SubType3.MaxLength), ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType4, (ZString)BM_NoChangesToReport.ToString(), ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_SubType5, (ZString)BM_StateOfSealsBoolean.ToString(), ZString.Empty);
			}
			result.Add(ProcessTaskTemplateSchema.P0_GB, header != null && header.BH_GB.IsValid ? header.BH_GB : GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			return result;
		}

		ColumnValueRanker AdjustClientPriority(ColumnValueRanker result)
		{
			var columnValues = ((IColumnValueRankerInternals)result).ColumnValues;

			result = new ColumnValueRanker();
			object[] values;

			foreach (var columnValuesPair in columnValues)
			{
				values = columnValuesPair.Values;

				if (columnValuesPair.Column != null
					&& columnValuesPair.Column == ProcessTaskTemplateSchema.P0_OH_Client)
				{
					object[] newValues = [Header.DestinationTrader.OrganisationPK];

					values = values != null
						? newValues.Union(values).ToArray()
						: newValues;
				}

				result.Add(columnValuesPair.Column, values);
			}
			return result;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection<NctsArrivalMovementHeaderProcessTask, NctsArrivalMovementHeader> workflowItems;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor; }
		}

		#endregion

		#region IWorkflowAffectedPropertyProvider Members

		ZPropertyInfo[] IWorkflowAffectedPropertyProvider.PropertyThatAffectWorkflowChanged
		{
			get
			{
				if (!IsDeleted)
				{
					if (Header is NctsHeader header)
					{
						return
						[
							header.DestinationTrader.OrganisationPKInfo,
							ExportFlagInfo,
							AuthorizationCodeInfo,
							DestinationCustomsOfficeCodeForArrivalInfo,
							BM_NoChangesToReportInfo,
							BM_StateOfSealsBooleanInfo
						];
					}
				}

				return null;
			}
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		[ChildEditable]
		protected CustomBusinessObject CustomBusinessObject
		{
			get
			{
				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
					customBusinessObject = new CustomBusinessObject(Factory, this, properties);
					RegisterEditableChildObject(customBusinessObject);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;

		#endregion
	}
}
