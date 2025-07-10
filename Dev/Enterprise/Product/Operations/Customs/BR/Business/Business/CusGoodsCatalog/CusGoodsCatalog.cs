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
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.BR.Business
{
	public class CusGoodsCatalog : BaseCusGoodsCatalog
		, Integration.Customs.BR.ICusGoodsCatalog
		, ICusCodeDataTypeSupporter
		, IMessageAttachee
		, IMessageManageableBizObj
		, IBackDoorSavingSupportableBizObj
		, IAttributeCusCodeDataParent
	{
		public CusGoodsCatalog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusGoodsCatalog.Schema
		{
			public const string ComplementaryDescription = nameof(CusGoodsCatalog.ComplementaryDescription);
			public const string LocalPartNumbersConcatenated = nameof(CusGoodsCatalog.LocalPartNumbersConcatenated);
			public const int AuthorityIdentifierMaxLength = 10;
			public const int AuthorityStatusMaxLength = 1;
			public const int TariffMaxLength = 10;
		}

		protected override Type GetProductionInfoTypeCore(ZString type)
		{
			switch (type)
			{
				case CusGoodsCatalogProductionInfoTypeList.Codes.FOR:
					return typeof(ForeignOperator);
				case CusGoodsCatalogProductionInfoTypeList.Codes.LPN:
					return typeof(LocalPartNumber);
				default:
					return base.GetProductionInfoTypeCore(type);
			}
		}

		public bool IsImport => CGC_Type == GoodsCatalogTypeList.Codes.Import;

		public bool IsExport => CGC_Type == GoodsCatalogTypeList.Codes.Export;

		ZGuid IMessageAttachee.BranchPK => Company?.FirstActiveBranch?.PK ?? ZGuid.Empty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CGC_CustomsStatus = CustomsPostedStatusList.Codes.Active;
		}

		[MaxLength(3700)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusGoodsCatalog|ComplementaryDescription", ShortCaption = "Comp. Descr.", Caption = "Complementary Description")]
		public ZString ComplementaryDescription
		{
			get => ComplementaryDescriptionNote.Text;
			set => ComplementaryDescriptionNote.SetNoteText(this, ComplementaryDescriptionInfo, value);
		}

		HiddenTextNote ComplementaryDescriptionNote => complementaryDescriptionNote ??= new HiddenTextNote(this, PredefinedNoteTypes.Instance.BRComplementaryDescription.Description);
		HiddenTextNote complementaryDescriptionNote;

		public ZPropertyInfo ComplementaryDescriptionInfo => GetZPropertyInfo(Schema.ComplementaryDescription);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			this.DeleteHiddenNotes();
			Attributes.RemoveAndDeleteAll();
			base.Delete();
		}

		public override MultilingualString ReasonForNotAbleToDelete => HasAuthorityIdentifier
					? ResString.GetMultilingualString("C163F4D7-D3D0-44BC-898A-FA04D7B4F7EF", "This Catalog cannot be deleted due to the following reason(s): It has an Authority Identifier.")
				: base.ReasonForNotAbleToDelete;

		public override bool CanDelete => base.CanDelete && !HasAuthorityIdentifier;

		[MaxLength(Schema.AuthorityIdentifierMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusGoodsCatalog|CGC_AuthorityIdentifier", Caption = "Authority Identifier", FullDescription = "The Goods Catalog Identifier on Customs side.")]
		[ReadOnly(true)]
		public override ZString CGC_AuthorityIdentifier { get => base.CGC_AuthorityIdentifier; set => base.CGC_AuthorityIdentifier = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.CusGoodsCatalog|CGC_AuthorityVersion", Caption = "Version", FullDescription = "The Goods Catalog Version on Customs side.")]
		[ReadOnly(true)]
		public override ZString CGC_AuthorityVersion { get => base.CGC_AuthorityVersion; set => base.CGC_AuthorityVersion = value; }

		[ReadOnly(true)]
		[MaxLength(Schema.AuthorityStatusMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusGoodsCatalogLookups.StatusTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusGoodsCatalog|CGC_AuthorityStatus", Caption = "Customs Status", FullDescription = "The Goods Catalog Status on Customs side.")]
		public override ZString CGC_AuthorityStatus { get => base.CGC_AuthorityStatus; set => base.CGC_AuthorityStatus = value; }

		[ReadOnlyMember(nameof(HasAuthorityIdentifier))]
		public override ZString CGC_Type
		{
			get => base.CGC_Type;
			set
			{
				var oldValue = CGC_Type;
				base.CGC_Type = value;
				if (!IsCopying && oldValue != CGC_Type)
				{
					Attributes.Rebuild();
				}
			}
		}

		[BusinessObjectMaxLengthTestExclude]
		[MaxLength(Schema.TariffMaxLength)]
		[ReadOnlyMember(nameof(HasAuthorityIdentifier))]
		public override ZString CGC_Tariff
		{
			get => base.CGC_Tariff;
			set
			{
				var oldValue = CGC_Tariff;
				base.CGC_Tariff = value;
				if (!IsCopying && oldValue != CGC_Tariff)
				{
					Attributes.Rebuild();
				}
			}
		}

		protected override ZString FormatTariffForSaving(ZString unformattedTariff) => unformattedTariff.KeepNumericCharacters();

		[ReadOnlyMember(nameof(HasAuthorityIdentifier))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CusGoodsCatalog|CGC_OH_Owner", Caption = "Owner", FullDescription = "The Goods Catalog Owner on Customs side.")]
		public override ZGuid CGC_OH_Owner
		{
			get => base.CGC_OH_Owner;
			set
			{
				base.CGC_OH_Owner = value;
				ForeignOperators.MarkAsNeedingValidation();
			}
		}

		[ReadOnly(true)]
		public override ZString CGC_CatalogCode { get => base.CGC_CatalogCode; set => base.CGC_CatalogCode = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusGoodsCatalogLookups.MessageStatusList))]
		public override ZString CGC_MessageStatus { get => base.CGC_MessageStatus; set => base.CGC_MessageStatus = value; }

		public override ZGuid CGC_GC_Company
		{
			get => base.CGC_GC_Company;
			set
			{
				base.CGC_GC_Company = value;
				Attributes.MarkAsNeedingValidation();
			}
		}

		public ZDateTime EffectiveAssessmentDate => ZDateTime.Today;

		[ChildEditable(true)]
		public LocalPartNumberCollection LocalPartNumbers
		{
			get
			{
				if (fLocalPartNumbers == null)
				{
					fLocalPartNumbers = new LocalPartNumberCollection(this);
					RegisterEditableChildObject(fLocalPartNumbers);
				}
				return fLocalPartNumbers;
			}
		}
		LocalPartNumberCollection fLocalPartNumbers;

		[ChildEditable(true)]
		public AttributeCusCodeDataCollection Attributes
		{
			get
			{
				if (fAttributes == null)
				{
					fAttributes = new AttributeCusCodeDataCollection(this);
					fAttributes.Load();
					fAttributes.Rebuild();
					RegisterEditableChildObject(fAttributes);
				}
				return fAttributes;
			}
		}
		AttributeCusCodeDataCollection fAttributes;

		public AttributeCusCodeDataCollection GetAttributes(string type) => type == CusCodeDataTypeList.Codes.Attribute ? Attributes : null;

		[ChildEditable(true)]
		public ForeignOperatorCollection ForeignOperators
		{
			get
			{
				if (fForeignOperators == null)
				{
					fForeignOperators = new ForeignOperatorCollection(this);
					RegisterEditableChildObject(fForeignOperators);
				}
				return fForeignOperators;
			}
		}
		ForeignOperatorCollection fForeignOperators;

		public ZString LocalPartNumbersConcatenated
		{
			get => string.Join(";", LocalPartNumbers.Select(x => x.CGI_Reference).Where(x => !x.IsEmpty));
		}

		public bool HasAuthorityIdentifier => !CGC_AuthorityIdentifier.IsEmpty;

		public bool IsMessageSent => CGC_MessageStatus != BRMessageStatusList.Codes.NotSent;

		public bool IsMessageRejected => CGC_MessageStatus == BRMessageStatusList.Codes.Rejected;

		public bool IsMessageAwaitingResponse => CGC_MessageStatus == BRMessageStatusList.Codes.AwaitingResponse;

		public bool IsMessageAccepted => CGC_MessageStatus == BRMessageStatusList.Codes.Accepted;

		public new CusGoodsCatalogLookups Lookups => (CusGoodsCatalogLookups)base.Lookups;

		protected override Customs.Business.CusGoodsCatalogLookups GetNewLookups() => new CusGoodsCatalogLookups(this);

		public new CusGoodsCatalogValidation Validation => (CusGoodsCatalogValidation)base.Validation;

		protected override Customs.Business.CusGoodsCatalogValidation GetNewValidation() => new CusGoodsCatalogValidation(this);

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (CusGoodsCatalog)base.CloneInternal(args);

			result.ComplementaryDescription = ComplementaryDescription;
			result.ForeignOperators.CloneFrom(ForeignOperators);
			result.Attributes.Rebuild();
			result.Attributes.CopyDataFrom(Attributes);
			return result;
		}

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}

		EDIMessageCollection fMessages;

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(CGC_CatalogCodeInfo, GetNewCatalogCode);
			UpdateCustomsStatusIfAnyChangesAffectMessage();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				CGC_CatalogCode = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		ZString GetNewCatalogCode(BusinessObjectFactory factory)
		{
			var target = new CatalogCodeGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.BRCatalogCode,
				FountainGetter = Env.NumberFountains.GetBRCatalogCodeGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new GoodCatalogValueSource(this));
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		protected override bool SupportsWorkflowCore => true;

		public bool IsInAStatusAmendmentSendable => IsMessageAwaitingResponse || HasAuthorityIdentifier;

		public StmALog AddCustomsUpdateLog(ZDateTimeOffset date, string description = null, string product = null, string reason = null)
		{
			var parameters = new Dictionary<string, string>();

			if (!description.IsNullOrEmpty())
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, description);
			}
			if (!product.IsNullOrEmpty())
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Product, product);
			}
			if (!reason.IsNullOrEmpty())
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason);
			}

			return Logs.AddNew(Events.CustomsUpdate, date, parameters.ToArray());
		}

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.Attribute, typeof(AttributeCusCodeData) },
			};
		}

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new GoodsCatalogMultiMessageManager(new GoodsCatalogMessageSendingObject(this));
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return ContinueWithDetection.Yes;
		}

		#endregion

		#region Reset Message Status & Customs Status

		public void ResetStatuses()
		{
			if (!IsResetStatusesSuspended && IsInDatabase)
			{
				if (CGC_MessageStatus != BRMessageStatusList.Codes.NotSent && !CGC_MessageStatusInfo.HasChanges)
				{
					CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;
				}

				if (CGC_CustomsStatus != CustomsPostedStatusList.Codes.Active && !CGC_CustomsStatusInfo.HasChanges)
				{
					CGC_CustomsStatus = CustomsPostedStatusList.Codes.Active;
				}
			}
		}

		public bool IsResetStatusesSuspended => resetStatusesSuspenderIndex > 0;
		int resetStatusesSuspenderIndex;

		public IDisposable SuspendResetStatuses() => new DisposableAction(() => resetStatusesSuspenderIndex++, () => resetStatusesSuspenderIndex--);

		public void SuspendResetStatusesUntilSaved()
		{
			var disposable = SuspendResetStatuses();
			Factory.Saved += OnFactorySaved;

			void OnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				factory.Saved -= OnFactorySaved;

				disposable?.Dispose();
				disposable = null;
			}
		}

		#endregion

		#region Update Customs Status on Saving

		public bool IsUpdateCustomStatusOnSavingSuspended => customStatusSuspenderIndex > 0;
		int customStatusSuspenderIndex;

		public IDisposable SuspendUpdateCustomStatusOnSaving() => new DisposableAction(() => customStatusSuspenderIndex++, () => customStatusSuspenderIndex--);

		void UpdateCustomsStatusIfAnyChangesAffectMessage()
		{
			if (!IsUpdateCustomStatusOnSavingSuspended && IsInDatabase && CGC_CustomsStatus.IsAccepted() && !CGC_CustomsStatusInfo.HasChanges)
			{
				var catalogInDatabase = new BusinessObjectFactory().Load<CusGoodsCatalog>(PK);
				if (catalogInDatabase != null)
				{
					var messageForCatalogInDB = new GoodsCatalogMessageSendingObject(catalogInDatabase).GetMessageText();
					var messageForCatalog = new GoodsCatalogMessageSendingObject(this).GetMessageText();

					if (messageForCatalogInDB != messageForCatalog)
					{
						CGC_CustomsStatus = CustomsPostedStatusList.Codes.UpdatePending;
					}
				}
			}
		}

		public void SuspendUpdateCustomStatusOnSavingUntilSaved()
		{
			var disposableCustomsStatus = SuspendUpdateCustomStatusOnSaving();

			Factory.Saved += OnFactorySaved;

			void OnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				factory.Saved -= OnFactorySaved;
				disposableCustomsStatus?.Dispose();
				disposableCustomsStatus = null;
			}
		}

		#endregion

		#region IBackDoorSavingSupportableBizObj

		bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected => true;

		AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReason();
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusGoodsCatalog);
			}

			public CusGoodsCatalog GetUniqueGoodsCatalogsByLocalPartNumber(ZString localPartNumber, ZGuid ownerPK, ZString type)
			{
				var goodsCatalogs = Factory.Load<CusGoodsCatalog>(GetGoodsCatalogByLocalPartNumberQuery(localPartNumber, ownerPK, type, 2));
				return goodsCatalogs.Length != 1 ? null : goodsCatalogs[0];
			}

			public CusGoodsCatalog GetGoodsCatalogByAuthorityIdentifierAndOwner(ZString identifier, ZGuid owner) => Factory.LoadTop1<CusGoodsCatalog>(GetGoodsCatalogByAuthorityIdentifierQuery(identifier, owner));

			public CusGoodsCatalog GetGoodsCatalogByAuthorityIdentifierAndOwner(ZString identifier, ZString rootCnpj)
			{
				var owner = Factory.FindOrganizationByRootCNPJ(rootCnpj);
				return owner == null ? null : GetGoodsCatalogByAuthorityIdentifierAndOwner(identifier, owner.PK);
			}

			ZQuery GetGoodsCatalogByLocalPartNumberQuery(ZString localPartNumber, ZGuid ownerPK, ZString type, int maximumRows)
			{
				if (localPartNumber.IsEmpty || !ownerPK.IsValid)
				{
					return ZQuery.NoResultQuery;
				}
				else
				{
					var partNumberQuery = new ZDBOnlySubQuery(typeof(LocalPartNumber), CusGoodsCatalogProductionInfoSchema.CGI_CGC_Catalog);
					partNumberQuery.AddToFilter(CusGoodsCatalogProductionInfoSchema.CGI_Type, CusGoodsCatalogProductionInfoTypeList.Codes.LPN);
					partNumberQuery.AddToFilter(CusGoodsCatalogProductionInfoSchema.CGI_Reference, localPartNumber);
					var catalogQuery = new ZDBOnlyQuery(typeof(CusGoodsCatalog));
					catalogQuery.AddToFilter(CusGoodsCatalogSchema.CGC_OH_Owner, ownerPK);
					catalogQuery.AddToFilter(CusGoodsCatalogSchema.CGC_Type, type);
					catalogQuery.AddSubQuery(partNumberQuery, JoinCondition.And);
					catalogQuery.MaximumRows = maximumRows;
					return catalogQuery;
				}
			}

			ZQuery GetGoodsCatalogByAuthorityIdentifierQuery(ZString identifier, ZGuid ownerPK)
			{
				if (identifier.IsEmpty)
				{
					return ZQuery.NoResultQuery;
				}
				else
				{
					var query = new ZQuery(CusGoodsCatalogSchema.CGC_AuthorityIdentifier, identifier);
					if (ownerPK.IsValid)
					{
						query.AddToFilter(CusGoodsCatalogSchema.CGC_OH_Owner, ownerPK);
					}
					query.OrderBy = CusGoodsCatalogSchema.CGC_SystemCreateTimeUtc.Name + " DESC";
					return query;
				}
			}
		}

		#endregion
	}
}
