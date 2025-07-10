using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.CustomsEntryHeaders))]
	public class CusEntryHeader : EU.Business.Declaration.CusEntryHeader
		, Integration.Customs.DE.ICusEntryHeader
	{
		public new class Schema : EU.Business.Declaration.CusEntryHeader.Schema
		{
			public const string Style = nameof(CusEntryHeader.Style);
			public const string SubStyle = nameof(CusEntryHeader.SubStyle);
			public const string Description = nameof(CusEntryHeader.Description);
			public const string LocalReferenceNumber = nameof(CusEntryHeader.LocalReferenceNumber);
		}

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZGuid CH_JE
		{
			get => base.CH_JE;
			set
			{
				var oldValue = CH_JE;
				base.CH_JE = value;
				if (!IsCopying &&
					oldValue != value &&
					!IsInDatabase &&
					CH_WarehouseTransactionStatus.IsEmpty &&
					Declaration != null)
				{
					CH_WarehouseTransactionStatus = Declaration.WarehouseTransactionStatus;
				}
			}
		}

		public ZString Style => Factory.GetValue(ref styleCached, () => EntryInstruction?.CEI_Style ?? ZString.Empty);
		CachedProperty<ZString> styleCached;

		public ZString SubStyle => Factory.GetValue(ref subStyleCached, () => EntryInstruction?.CEI_SubStyle ?? ZString.Empty);
		CachedProperty<ZString> subStyleCached;

		public ZString Description => Factory.GetValue(ref descriptionCached, () => EntryInstruction?.CEI_Description ?? ZString.Empty);
		CachedProperty<ZString> descriptionCached;

		[MaxLength(nameof(LocalReferenceNumberMax_Length))]
		[ResourceStringData("823DE38C-E4CD-4B6E-A100-91821A1EF5FC", Caption = "LRN")]
		public ZString LocalReferenceNumber
		{
			get
			{
				return LRNCusEntryNumberWrapper.ExistsCusEntryNumber
					? LRNCusEntryNumberWrapper.EntryNumber
					: Declaration.TradersOwnReferenceFullForBox7;
			}
			set
			{
				LRNCusEntryNumberWrapper.SetEntryNumber(value, LocalReferenceNumberInfo);
			}
		}

		int LocalReferenceNumberMax_Length => 22;

		public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(Schema.LocalReferenceNumber);

		public IEnumerable<IDefermentAccount> GetDutyDefermentAccounts()
		{
			var result = new List<IDefermentAccount>(2);
			if (MethodOfPaymentHelper.RequireDeferralPaymentParty(Declaration.ZG_MethodOfPayment))
			{
				var paymentMethod = Declaration.JE_PaymentMethod;
				var defermentAccountNumber = Declaration.JE_DefermentAccountNumber;
				var vatDeferType = Declaration.ZG_VATDeferType;
				var vatDeferNumber = Declaration.ZG_VATDeferNumber;
				if (!paymentMethod.IsEmpty && !defermentAccountNumber.IsEmpty)
				{
					var dutyDefermentOrg = GetOrgAddressFromType(paymentMethod);
					if (dutyDefermentOrg != null)
					{
						AddProviderIfHasValidAccount(new DefermentAccount(dutyDefermentOrg, defermentAccountNumber));
					}
				}
				if (!vatDeferType.IsEmpty && !vatDeferNumber.IsEmpty)
				{
					var vatDefermentOrg = GetOrgAddressFromType(vatDeferType);
					if (vatDefermentOrg != null)
					{
						AddProviderIfHasValidAccount(new DefermentAccount(vatDefermentOrg, vatDeferNumber));
					}
				}
			}
			return result.ToArray();

			OrgHeader GetOrgAddressFromType(string type) => type switch
			{
				DeferralPaymentPartyList.Codes.Declarant => Declaration.Declarant?.Header,
				DeferralPaymentPartyList.Codes.Representative => Declaration.Representative?.Header,
				DeferralPaymentPartyList.Codes.RepresentedParty => Declaration.BuyingAgentAddress?.Header,
				DeferralPaymentPartyList.Codes.DefermentParty => Declaration.DefermentPartyDocAddress?.Organisation,
				_ => null,
			};

			void AddProviderIfHasValidAccount(DefermentAccount provider)
			{
				if (provider.AccountToUse != null)
				{
					result.Add(provider);
				}
			}
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public override ZString EntryTypeFriendlyName
		{
			get
			{
				ZString result;
				if (Declaration?.IsExport ?? false)
				{
					var procedureCodeWithoutConcession = ProcedureCodeWithoutConcession;
					result = string.Format(CultureInfo.CurrentCulture, "({0}{1}{2}) {3}", Declaration.JE_EntryStyle, EntryInstruction?.CEI_SubStyle,
						procedureCodeWithoutConcession.IsEmpty ? string.Empty : string.Format(CultureInfo.CurrentCulture, " / {0}", procedureCodeWithoutConcession), EntryInstruction?.CEI_Style);
				}
				else
				{
					result = base.EntryTypeFriendlyName;
				}
				return result;
			}
		}

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

		protected override bool IsOriginAndDestinationRequiredInItinerary => true;

		protected override ZString EntryNumberType => CusEntryNumberTypes.Standard.MovementReferenceNumber;

		protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

		protected override ICommonGoodsItemsIntegrator CommonGoodsItemsIntegratorCore => new DECommonGoodsItemsIntegrator(this);

		protected override bool ShouldBeIncludedInCusEntryNumberFilterCore()
		{
			var result = base.ShouldBeIncludedInCusEntryNumberFilterCore();

			var entryNum = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			if (result && IsExport && entryNum != null && canceledEntryStatusForExport.Contains(CH_EntryStatus))
			{
				result = false;
			}
			return result;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			FillInLocalReferenceNumber();
		}

		void FillInLocalReferenceNumber()
		{
			var declaration = Declaration;
			if ((declaration.IsImport || declaration.IsExport) && !IsInDatabase)
			{
				var defaultLocalReferenceNumber = LocalReferenceNumber;
				if (!defaultLocalReferenceNumber.IsEmpty)
				{
					var customsEntryHeaders = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().ToArray();

					if (customsEntryHeaders.Any(x => x.PK != PK && x.LocalReferenceNumber == defaultLocalReferenceNumber))
					{
						var existingSuffix = GetCH_BGMReferenceSuffix();
						var localReferenceNumberWithSuffix = defaultLocalReferenceNumber + existingSuffix;
						LocalReferenceNumber = localReferenceNumberWithSuffix;
					}
					else
					{
						LocalReferenceNumber = defaultLocalReferenceNumber;
					}
				}
			}
		}

		string GetCH_BGMReferenceSuffix()
		{
			var suffix = string.Empty;
			var bgmReference = CH_BGMReference;
			var suffixIndex = bgmReference.LastIndexOf(LocalReferenceNumberSlash, StringComparison.InvariantCultureIgnoreCase);
			if (suffixIndex != -1)
			{
				suffix = bgmReference.Substring(suffixIndex);
			}
			return suffix;
		}

		CusEntryNumberWrapper LRNCusEntryNumberWrapper => lrnCusEntryNumberWrapper ?? (lrnCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Standard.LocalReferenceNumber));
		CusEntryNumberWrapper lrnCusEntryNumberWrapper;

		const string LocalReferenceNumberSlash = "/";

		static readonly ImmutableHashSet<string> canceledEntryStatusForExport = ImmutableHashSet.Create(A0115DepartureStatusCodeList.Codes._191, A0115DepartureStatusCodeList.Codes._520);

		protected override ZString PreviousStatus => (ZString)CH_EntryStatusInfo.OriginalValue;
		protected override ZString CurrentStatus => CH_EntryStatus;

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
			var statusCodesForAutoBillingFromRegistry = ListOfStatusCodesForAutoBilling;

			bool result;
			if (statusCodesForAutoBillingFromRegistry.Count > 0)
			{
				result = !statusCodesForAutoBillingFromRegistry.Contains(originalStatus) && statusCodesForAutoBillingFromRegistry.Contains(newStatus);
			}
			else
			{
				var now = ZDateTime.Now;
				var dataGroupingCode = Declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty;
				result = !Customs.Universal.CustomsStatusAttributeHelper.ShouldExecuteAutoBilling(Factory, originalStatus, dataGroupingCode, now)
					&& Customs.Universal.CustomsStatusAttributeHelper.ShouldExecuteAutoBilling(Factory, newStatus, dataGroupingCode, now);
			}

			return result;
		}

		HashSet<ZString> ListOfStatusCodesForAutoBilling => Factory.GetValue(ref listOfStatusCodesForAutoBillingCached,
			getValueDelegate: () => GetListOfStatusCodesForAutoBillingFromRegistry(Declaration));
		CachedProperty<HashSet<ZString>> listOfStatusCodesForAutoBillingCached;

		static HashSet<ZString> GetListOfStatusCodesForAutoBillingFromRegistry(JobDeclaration declaration)
		{
			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetFallBackValueAtAllLevels(declaration?.Branch.GB_GC.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);
			return options.EUCustomsStatusCodes.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty).Distinct().ToHashSet();
		}
	}
}
