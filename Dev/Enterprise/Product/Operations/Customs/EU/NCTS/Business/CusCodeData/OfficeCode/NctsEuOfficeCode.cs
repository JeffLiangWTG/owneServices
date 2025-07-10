using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCode : EuOfficeCode, IShortSequenceNumberLine
	{
		public NctsEuOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected sealed override CusCodeDataValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusCodeDataValidation GetNewPhase5Validation() => MovementHeader != null ? new NctsEuOfficeCodePhase5DepartureValidation(this) : new NctsEuOfficeCodePhase5Validation(this);

		protected virtual CusCodeDataValidation GetNewPhase4Validation() => new NctsEuOfficeCodeValidation(this);

		public INctsEuOfficeCodeValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsEuOfficeCodeValidationDecider> validationDeciderCached;

		INctsEuOfficeCodeValidationDecider GetValidationDecider()
		{
			var header = EffectiveHeader;
			return header?.Configuration.NctsEuOfficeCodeConfiguration.GetValidationDecider(header);
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups() => new NctsEuOfficeCodeLookups(this);

		public new NctsEuOfficeCodeLookups Lookups => (NctsEuOfficeCodeLookups)base.Lookups;

		public NctsHeader Header => Parent as NctsHeader;

		public NctsDepartureMovementHeader MovementHeader => Parent as NctsDepartureMovementHeader;

		public NctsArrivalMovementHeader ArrivalMovementHeader => Parent as NctsArrivalMovementHeader;

		public ZBool IsOfficeDeparture => CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;

		public bool IsInCL010CountryList => Factory.GetCached(ref isInCL010CountryListCachedProperty, GetIsInCL010CountryList);
		CachedProperty<bool> isInCL010CountryListCachedProperty;

		public bool IsInCL112CountryList => Factory.GetCached(ref isInCL112CountryListCachedProperty, GetIsInCL112CountryList);
		CachedProperty<bool> isInCL112CountryListCachedProperty;

		[ResourceStringData("Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode.CY_Code", Caption = "Purpose")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;
				var newValue = CY_Code;
				var parent = Parent;
				if (!IsCopying && oldValue != newValue)
				{
					if (AutomaticSequenceNumberEnabled)
					{
						RecalculateSequenceNumberOnChangeOfCode(parent, oldValue, newValue);
					}

					if (!IsMarkingAsNeedingValidationSuspended)
					{
						if (parent is NctsHeader header)
						{
							header.MovementHeader?.MarkAsNeedingValidation();
						}
						else if (parent is NctsDepartureMovementHeader departureMovementHeader)
						{
							departureMovementHeader.Header?.MarkAsNeedingValidation();
						}
						else if (parent is NctsArrivalMovementHeader arrivalMovementHeader)
						{
							arrivalMovementHeader.Header?.MarkAsNeedingValidation();
						}
						MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid CY_ParentID
		{
			get => base.CY_ParentID;
			set
			{
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				var parent = Parent;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					if (parent is NctsHeader { IsPhase5: true })
					{
						throw new DeveloperNotificationException("Trying to add EuOfficeCode on NctsHeader in Phase 5. For Phase 5 Office Codes should be added to MovementHeader");
					}
					DoMajorMarkAsNeedingValidation(parent);
				}

				SetOriginalData(parent);
			}
		}

		void DoMajorMarkAsNeedingValidation(BusinessObject parent)
		{
			if (IsMarkingAsNeedingValidationSuspended)
			{
				return;
			}

			switch (parent)
			{
				case NctsCommonMovementHeader movementHeader:
					movementHeader.MarkAsNeedingValidation();
					movementHeader.GoodsItems.MarkAsNeedingValidation();
					movementHeader.Header?.MarkAsNeedingValidation();
					movementHeader.Header?.Bills.ForEach(b => b.GoodsItems.MarkAsNeedingValidation());
					break;
				case NctsHeader header:
					header.MarkAsNeedingValidation();
					header.MovementHeader?.MarkAsNeedingValidation();
					header.MovementHeader?.GoodsItems.MarkAsNeedingValidation();
					header.ArrivalMovementHeader?.GoodsItems.MarkAsNeedingValidation();
					header.Bills.ForEach(b => b.GoodsItems.MarkAsNeedingValidation());
					break;
			}

			MarkAsNeedingValidation();
		}

		void SetOriginalData(BusinessObject parent)
		{
			if (parent is NctsHeader header)
			{
				OriginalHeader = header;
			}
			else if (parent is NctsDepartureMovementHeader departureMovementHeader)
			{
				OriginalMovementHeader = departureMovementHeader;
			}
			else if (parent is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				OriginalArrivalMovementHeader = arrivalMovementHeader;
			}
		}

		NctsHeader OriginalHeader { get; set; }

		NctsDepartureMovementHeader OriginalMovementHeader { get; set; }

		NctsArrivalMovementHeader OriginalArrivalMovementHeader { get; set; }

		public override ZString CY_ParentTableCode
		{
			get => base.CY_ParentTableCode;
			set
			{
				var oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				var parent = Parent;
				if (!IsCopying && oldValue != CY_ParentTableCode)
				{
					if (parent is NctsHeader { IsPhase5: true })
					{
						throw new DeveloperNotificationException("Trying to add EuOfficeCode on NctsHeader in Phase 5. For Phase 5 Office Codes should be added to MovementHeader");
					}

					DoMajorMarkAsNeedingValidation(parent);
				}
				SetOriginalData(parent);
			}
		}

		public override ZString CY_Type
		{
			get => base.CY_Type;
			set
			{
				base.CY_Type = value;
				var parent = Parent;
				if (parent is NctsHeader or NctsDepartureMovementHeader or NctsArrivalMovementHeader)
				{
					parent.MarkAsNeedingValidation();
				}
				MarkAsNeedingValidationIncludingChildren();
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode.CY_Data", Caption = "Office")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != value)
				{
					GetHeaderEquivalentPropertyInfo()?.RefreshBinding(oldValue);
				}
				MarkAsNeedingValidation();
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode.CY_Order", Caption = "Sequence Number", ShortCaption = "Seq. No.")]
		[ReadOnly(true)]
		public override ZShort CY_Order { get => base.CY_Order; set => base.CY_Order = value; }

		public override void Delete()
		{
			if (!IsDeleted && AutomaticSequenceNumberEnabled)
			{
				if (OriginalHeader is NctsHeader sequenceHeader)
				{
					sequenceHeader.RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this, CY_Code);
				}
				if (OriginalMovementHeader is NctsDepartureMovementHeader sequenceMovementHeader)
				{
					sequenceMovementHeader.RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this, CY_Code);
				}
				if (OriginalArrivalMovementHeader is NctsArrivalMovementHeader sequenceArrivalMovementHeader)
				{
					sequenceArrivalMovementHeader.RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this, CY_Code);
				}
			}
			base.Delete();
		}

		ZPropertyInfo GetHeaderEquivalentPropertyInfo()
		{
			var parent = Parent;
			var departureMovement = parent as NctsDepartureMovementHeader;
			var arrivalMovement = parent as NctsArrivalMovementHeader;
			var header = (parent as NctsHeader) ?? departureMovement?.Header ?? arrivalMovement?.Header;

			ZPropertyInfo info = null;
			if (header != null)
			{
				if (departureMovement == null && header.IsPhase5Departure
					|| arrivalMovement == null && header.IsPhase5Arrival)
				{
					ErrorReporter.ReportOnce("Incorrect Parent for Phase5", $"Application code is 'NC5', but customs office doesn't have Parent 'Departure/ArrivalMovementHeader'.");
				}

				switch (CY_Code)
				{
					case OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry:
						if (header.IsPhase5Departure)
						{
							info = (departureMovement ?? header.MovementHeader).EnquiryCustomsOfficeCodeInfo;
						}
						else
						{
							info = header.EnquiryCustomsOfficeCodeInfo;
						}
						break;

					case OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture:
						if (header.IsPhase5Departure)
						{
							info = (departureMovement ?? header.MovementHeader).DepartureCustomsOfficeCodeInfo;
						}
						else
						{
							info = header.DepartureCustomsOfficeCodeInfo;
						}
						break;

					case OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination:
						if (header.IsPhase5Departure)
						{
							info = (departureMovement ?? header.MovementHeader).DestinationCustomsOfficeCodeInfo;
						}
						else
						{
							info = header.DepartureCustomsOfficeCodeInfo;
						}
						break;

					case OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival:
						if (header.IsPhase5Arrival)
						{
							info = (arrivalMovement ?? header.ArrivalMovementHeader).DestinationCustomsOfficeCodeForArrivalInfo;
						}
						else
						{
							info = header.DestinationCustomsOfficeCodeForArrivalInfo;
						}
						break;
				}
			}
			return info;
		}

		public ZString OfficeCountryCode => Factory.GetCached(ref officeCountryCodeCachedProperty, () => CY_Data.Left(2));
		CachedProperty<ZString> officeCountryCodeCachedProperty;

		void RecalculateSequenceNumberOnChangeOfCode(BusinessObject parent, ZString oldValue, ZString newValue)
		{
			if (parent is NctsHeader header)
			{
				header.RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this, oldValue);
				header.RecalculateCustomsOfficeSequenceNoWhenAdded(this, newValue);
			}
			else if (parent is NctsDepartureMovementHeader movementHeader)
			{
				movementHeader.RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this, oldValue);
				movementHeader.RecalculateCustomsOfficeSequenceNoWhenAdded(this, newValue);
			}
			else if (parent is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				arrivalMovementHeader.RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this, oldValue);
				arrivalMovementHeader.RecalculateCustomsOfficeSequenceNoWhenAdded(this, newValue);
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode.CY_Date", Caption = "Time")]
		public override ZDateTime CY_Date { get => base.CY_Date; set => base.CY_Date = value; }

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsHeader), typeof(NctsCommonMovementHeader));

		public NctsHeader EffectiveHeader
		{
			get
			{
				var parent = Parent;
				return (parent as NctsHeader) ?? (parent as NctsDepartureMovementHeader)?.Header ?? (parent as NctsArrivalMovementHeader)?.Header;
			}
		}

		public bool IsPhase5 => EffectiveHeader?.IsPhase5 ?? false;

		protected override ZZRefCusCodeListCombined OfficeCore => CY_Data.IsEmpty ? null : ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Data, OfficeCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today);

		#region Type Decider

		public new static readonly NctsEuOfficeCodeTypeDecider TypeDecider = new NctsEuOfficeCodeTypeDecider();

		#endregion

		public ZBool IsOfficeOfTransit => CY_Code.EqualsIgnoringCase(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);

		public bool IsOfficeCountryConsideredInEuForSafetyAndSecurity => Factory.GetCached(ref isOfficeCountryConsideredInEuForSafetyAndSecurityCached, GetOfficeIsCountryConsideredInEuForSafetyAndSecurity);
		CachedProperty<bool> isOfficeCountryConsideredInEuForSafetyAndSecurityCached;

		#region ISequenceNumberLine

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => CY_Order;
			set => CY_Order = value;
		}

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		bool AutomaticSequenceNumberEnabled
		{
			get
			{
				return EffectiveHeader?.Configuration.NctsEuOfficeCodeConfiguration.AutomaticSequenceNumberEnabled ?? true;
			}
		}

		#endregion

		bool GetOfficeIsCountryConsideredInEuForSafetyAndSecurity()
		{
			var officeCountryCode = OfficeCountryCode;
			return !officeCountryCode.IsEmpty && Factory.IsCountryConsideredInEuForSafetyAndSecurity(officeCountryCode);
		}

		bool GetIsInCL010CountryList() => UniversalLookupsHelper.GetCL010CountryCodes(Factory).ContainsCode(OfficeCountryCode);

		bool GetIsInCL112CountryList() => Factory.GetCountryCodesCTC().ContainsCode(OfficeCountryCode);
	}
}
