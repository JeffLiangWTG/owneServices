using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	public sealed class LPCOViewCollection : NonPersistentBusinessObjectCollection<LPCOView>
	{
		public LPCOViewCollection(CusCALPCOCollection collection, CusCALPCOCollection headerCollection, IPGAHeader pgaHeader)
			: base(collection.Factory)
		{
			Argument.NotNull(collection, nameof(collection));
			this.collection = collection;
			this.agencyID = pgaHeader?.GovAgencyIDCode ?? ZString.Empty;
			this.headerCollection = headerCollection;
			this.pgaHeader = pgaHeader;
			Load();
		}

		readonly CusCALPCOCollection collection;
		readonly ZString agencyID;
		readonly CusCALPCOCollection headerCollection;
		readonly IPGAHeader pgaHeader;
		ZBool IsOnDecLevel => pgaHeader == null;

		public override void Load()
		{
			if (!agencyID.IsEmpty && headerCollection != null)
			{
				foreach (CusCALPCO lpco in headerCollection)
				{
					if (agencyID.EqualsIgnoringCase(lpco.AgencyIDCode))
					{
						var view = new LPCOView(lpco, pgaHeader)
						{
							ReadOnly = true
						};
						view.Validation.ValidateAll();
						Add(view);
					}
				}
			}
			foreach (CusCALPCO lpco in collection)
			{
				Add(new LPCOView(lpco));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LPCOView(collection.AddNew());
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			if (collection != null)
			{
				var lpcoView = (LPCOView)bizO;
				var agencyIDCode = lpcoView.LPCO.AgencyIDCode;
				if (!lpcoView.ReadOnly)
				{
					lpcoView.LPCO.Delete();
				}
				if (IsOnDecLevel)
				{
					RaiseEvents(agencyIDCode);
				}
			}
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (IsOnDecLevel && e.ItemAdded)
			{
				var lpcoView = (LPCOView)e.BizObject;
				if (lpcoView != null && lpcoView.LPCO != null)
				{
					lpcoView.LPCO.CLP_TypeInfo.ValueChanged -= RaiseTypeChangedEvent;
					lpcoView.LPCO.CLP_TypeInfo.ValueChanged += RaiseTypeChangedEvent;
					RaiseEvents(lpcoView.LPCO.AgencyIDCode);
				}
			}
		}

		void RaiseTypeChangedEvent(object sender, EventArgs e)
		{
			var valueChangedEventArgs = (ValueChangedEventArgs)e;
			if (valueChangedEventArgs != null)
			{
				var date = ZDateTime.UtcToday.Date;
				var oldAgency = GetPGACode((ZString)valueChangedEventArgs.OldValue, date);
				var newAgency = GetPGACode((ZString)valueChangedEventArgs.NewValue, date);
				if (oldAgency.IsEmpty)
				{
					oldAgency = RegistrationNumberHelper.LoadCFIALPCOType(Factory, (ZString)valueChangedEventArgs.OldValue) != null ? (ZString)(PGACodes.Codes.CFIA) : ZString.Empty;
				}
				if (newAgency.IsEmpty)
				{
					newAgency = RegistrationNumberHelper.LoadCFIALPCOType(Factory, (ZString)valueChangedEventArgs.NewValue) != null ? (ZString)(PGACodes.Codes.CFIA) : ZString.Empty;
				}
				RaiseEvents(oldAgency);
				RaiseEvents(newAgency);
			}
		}

		ZString GetPGACode(ZString code, ZDateTime date)
		{
			return Factory.GetCachedValue(string.Format("CADocumentTypePGAType_{0}_{1}", code, date.ToShortDateString()), () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, code, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, date)?.GetAttribute(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType) ?? ZString.Empty;
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void RaiseEvents(ZString agencyIDCode)
		{
			switch (agencyIDCode)
			{
				case PGACodes.Codes.CFIA:
					CFIACountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.CNSC:
					CNSCCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.DFO:
					DFOCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.ECCC:
					ECCCCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.GAC:
					GACCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.HC:
					HCCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.NRCan:
					NRCanCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.PHAC:
					PHACCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				case PGACodes.Codes.TC:
					TCCountChanged?.Invoke(this, EventArgs.Empty);
					break;
				default:
					break;
			}
		}
		public event EventHandler CFIACountChanged;
		public event EventHandler CNSCCountChanged;
		public event EventHandler DFOCountChanged;
		public event EventHandler ECCCCountChanged;
		public event EventHandler GACCountChanged;
		public event EventHandler HCCountChanged;
		public event EventHandler NRCanCountChanged;
		public event EventHandler PHACCountChanged;
		public event EventHandler TCCountChanged;

		public void AddDefaultLPCOs()
		{
			var pgaHeader = collection.Master as IPGAProgramRequirementProvider;
			if (pgaHeader != null)
			{
				var defaultTypes = pgaHeader.GetDefaultLPCOTypesForAllEnalbedPrograms();
				if (defaultTypes != null)
				{
					foreach (var lpcoView in this.Cast<LPCOView>().ToArray())
					{
						var lpco = lpcoView.LPCO;
						if (collection.Contains(lpco) && IsEmptyLPCO(lpco))
						{
							RemoveAndDelete(lpcoView);
						}
					}

					var groupedMandatoryTypes = defaultTypes.Where(x => x.RequiredType == DocumentTypeRequieredType.Mandatory).GroupBy(x => x.Category).ToArray();
					foreach (var mandatoryTypes in groupedMandatoryTypes.Where(x => x.Key.RequieredType == DocumentTypeRequieredType.Mandatory))
					{
						foreach (var type in mandatoryTypes)
						{
							if (type.RequiredType == DocumentTypeRequieredType.Mandatory && collection.Cast<CusCALPCO>().All(x => x.CLP_Type != type.Code))
							{
								var newLPCO = AddNew();
								newLPCO.CLP_Type = type.Code;
							}
						}
					}
				}
			}
		}

		bool IsEmptyLPCO(CusCALPCO lpco)
		{
			return (lpco.CLP_RefNo.IsEmpty || lpco.CLP_RefNo == CusCALPCO.DefaultRefNo)
					&& lpco.CLP_SecondaryRefNo.IsEmpty
					&& lpco.CLP_DIFRefNumberOrLocation.IsEmpty
					&& lpco.CLP_HolderType.IsEmpty
					&& lpco.CLP_StartDate.IsEmpty
					&& lpco.CLP_EndDate.IsEmpty
					&& lpco.CLP_HolderName.IsEmpty
					&& lpco.CLP_OA_Holder.IsEmpty
					&& lpco.CLP_RN_NKOriginCountryCode.IsEmpty
					&& lpco.CLP_CommodityTypeCode.IsEmpty
					&& lpco.CLP_AlternativeQuotaQuantity == 0
					&& lpco.CLP_AlternativeQuotaUQ.IsEmpty
					&& lpco.CLP_HolderContactEmail.IsEmpty
					&& lpco.CLP_HolderContactName.IsEmpty
					&& lpco.CLP_HolderContactPhone.IsEmpty
					&& lpco.CLP_ApplicantType.IsEmpty
					&& lpco.CLP_ApplicantName.IsEmpty
					&& lpco.CLP_OA_Applicant.IsEmpty
					&& lpco.CLP_ApplicantContactEmail.IsEmpty
					&& lpco.CLP_ApplicantContactName.IsEmpty
					&& lpco.CLP_ApplicantContactPhone.IsEmpty;
		}

		internal void CopyValueFrom(LPCOViewCollection lPCOViews)
		{
			this.ToList().ForEach(x =>
			{
				if (!x.ReadOnly)
				{
					RemoveAndDelete(x);
				}
			});

			foreach (LPCOView lpcoV in lPCOViews.ToList())
			{
				if (!lpcoV.HasPGAHeader && !lpcoV.LPCO.IsDeleted)
				{
					var sourceLPCO = lpcoV.LPCO;
					var clonedLPCOV = this.AddNew();
					clonedLPCOV.LPCO.CopyPersistentValuesFrom(sourceLPCO, new BusinessObjectCloneArgs(Array.Empty<string>(), true));
				}
			}
		}

		public LPCOView AddNewIfNotExist(ZString type)
		{
			var lpco = this.Cast<LPCOView>().FirstOrDefault(x => x.CLP_Type == type);
			if (lpco == null)
			{
				lpco = this.AddNew();
				lpco.CLP_Type = type;
			}
			return lpco;
		}
	}
}
