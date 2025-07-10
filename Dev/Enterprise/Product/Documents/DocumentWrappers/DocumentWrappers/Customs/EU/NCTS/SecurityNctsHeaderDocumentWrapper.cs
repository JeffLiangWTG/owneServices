using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.ZArchitecture.Core;
using CusAuthorizationUsage = Enterprise.Customs.EU.Business.CusAuthorizationUsage;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class SecurityNctsHeaderDocumentWrapper : NctsHeaderDocumentWrapper
	{
		protected SecurityNctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public static new SecurityNctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
		{
			return new SecurityNctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
		}

		#region Cached properties

		ITrader SecurityConsignee => CachedValueHelper.GetValue(ref securityConsignee, () => TraderWrapper.New(NctsHeader.SecurityConsignee, false, IsAddressExtended));
		CachedValue<ITrader> securityConsignee;

		ITrader SecurityConsignor => CachedValueHelper.GetValue(ref securityConsignor, () => TraderWrapper.New(NctsHeader.SecurityConsignor, false, IsAddressExtended));
		CachedValue<ITrader> securityConsignor;

		ITrader Representative => CachedValueHelper.GetValue(ref representative, () => TraderWrapper.New(MovementHeader.Representative, populateTIR: false, IsAddressExtended));
		CachedValue<ITrader> representative;

		ZString[] SelectedSeals => CachedValueHelper.GetValue(ref selectedSeals, () => GetSealsOfSelectedPackages());
		CachedValue<ZString[]> selectedSeals;

		#endregion

		#region Document Properties

		public ZBool BOXS00SECURITY => NctsHeader.IsSecurityDeclaration;

		public ZString BOXS32OTHERSCI
		{
			get { return Tools.ValueOrThreeDashes(MovementHeader.BM_BTAIndicator); }
		}

		public ZString BOXS12FIRSTARRIVALTIME
		{
			get { return "---"; } // The element was introduced in annex 30a CCIP and it is not part of the message IE15; when NOT used print "---"
		}

		public ZString BOXS29TRANSPORTCHARGESMOP => Tools.ValueOrThreeDashes(MovementHeader?.BM_MethodOfPayment ?? ZString.Empty);

		public ZString BOX7REFERENCENUMBERS => GetBox7ReferenceNumbers();

		public ZString BOX7UCR => MovementHeader.BM_UniqueConsignmentReference;

		public ZString BOX21BORDERTRANSPORTID => Tools.ValueOrThreeDashes(GetBox21BorderTransportId());

		public ZString BOX21BORDERTRANSPORTFLAG => Tools.ValueOrThreeDashes(MovementHeader?.BM_RN_NKTOLCarrierNationality ?? ZString.Empty);

		public ZString BOX25BORDERTRANSPORTMODE => Tools.ValueOrThreeDashes(MovementHeader?.BM_ExportTransportMode ?? ZString.Empty);

		public ZString BOX30LOCATIONOFGOODS => GetBox30LocationOfGoods();

		public ZString BOXS18PLACEOFUNLOADING => Tools.ValueOrThreeDashes(MovementHeader is NctsDepartureMovementHeader movementHeader
			? new ZString(new ZStringBuilder(movementHeader.BM_ForeignDestPortKCode).Append(movementHeader.BM_PlaceOfUnloading).ToStringWithDelimiterBetweenAppends(" "))
			: ZString.Empty);

		public ZString BOXS17PLACEOFLOADING => Tools.ValueOrThreeDashes(MovementHeader is NctsDepartureMovementHeader movementHeader
			? new ZString(new ZStringBuilder(movementHeader.BM_PortOfPresentationCode).Append(movementHeader.BM_PlaceOfLoading).ToStringWithDelimiterBetweenAppends(" "))
			: ZString.Empty);

		public ZString BOXS10CONVEYANCE => Tools.ValueOrThreeDashes(MovementHeader?.BM_ConveyanceNumber ?? ZString.Empty);

		public ZString BOXS13ROUTING => GetBoxS13Routing();

		public ZString BOXS6SECURITYCONSIGNEE => SecurityConsignee.GetWrappedAddress();

		public ZString BOXS6SECURITYCONSIGNEEEORI => SecurityConsignee?.TIN ?? ZString.Empty;

		public ZString BOXS4SECURITYCONSIGNOR => SecurityConsignor.GetWrappedAddress();

		public ZString BOXS4SECURITYCONSIGNOREORI => SecurityConsignor?.TIN ?? ZString.Empty;

		public ZString BOXS7CARRIER => GetCarrier()?.GetWrappedAddress() ?? ZString.Empty;

		public ZString BOXS7CARRIEREORI => GetCarrier()?.TIN ?? ZString.Empty;

		public ZString PRESENTATIONOFGOODSDATETIME => ZString.Empty;

		public ZString BOX44AUTHORISATIONS => GetAuthorisations();

		public ZString BOX50REPRESENTATIVE => Representative?.TIN ?? ZString.Empty;

		public ZString BOXS28SEALSNUMBER => GetBoxS28SealsNumber();

		#endregion

		#region Methods

		ITrader GetCarrier()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return GetCarrierForPhase5();
			}
			return GetCarrierForPhase4();
		}
		CachedValue<ITrader> carrier;

		ITrader GetCarrierForPhase5() => CachedValueHelper.GetValue(ref carrier, () => TraderWrapper.New(MovementHeader?.Carrier, false, IsAddressExtended));

		protected virtual ITrader GetCarrierForPhase4() => CachedValueHelper.GetValue(ref carrier, () => TraderWrapper.New(NctsHeader.CarrierOrgAddress, false, IsAddressExtended));

		protected virtual ZString GetBox7ReferenceNumbers() => NctsHeader.LocalReferenceNumber;

		protected virtual ZString GetBox21BorderTransportId() => MovementHeader?.BM_TOLCarrierID ?? ZString.Empty;

		ZString GetBox30LocationOfGoods()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return GetBox30LocationOfGoodsForPhase5();
			}
			return GetBox30LocationOfGoodsForPhase4();
		}

		ZString GetBox30LocationOfGoodsForPhase5()
		{
			var goodsLocation = MovementHeader?.GoodsLocation;
			if (goodsLocation is null)
			{
				return ZString.Empty;
			}

			var address = goodsLocation.Address;
			return goodsLocation.CGL_Qualifier.ToString() switch
			{
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber => $"{address.AuthorisationNumber}-{goodsLocation.AdditionalIdentifier}",
				CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier => goodsLocation.CGL_CustomsOffice,
				CusGoodsLocationQualifierList.Codes.Address => $"{address.E2_Address1AndE2_Address2}-{address.E2_City}",
				_ => ZString.Empty
			};
		}

		protected virtual ZString GetBox30LocationOfGoodsForPhase4()
		{
			return ((MovementHeader?.BM_LocationOfGoods ?? ZString.Empty) + " " + (MovementHeader?.BM_CustomsSubPlace ?? ZString.Empty)).Trim();
		}

		protected override ZString GetGuarantee() => Guarantees.ToStringCertainNumberGuarantees(4);

		ZString GetAuthorisations()
		{
			var sb = new ZStringBuilder();
			var cusAuthorizationUsages = NctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)NctsHeader.MovementHeader.CusAuthorizationUsages : NctsHeader.CusAuthorizationUsages;
			cusAuthorizationUsages?.ForEach(a => sb.Append($"{a.AGC_Code} - {a.AGC_Number}"));
			return sb.ToStringWithNewLineBetweenAppends();
		}

		ZString[] GetSealsOfSelectedPackages()
		{
			var sealList = new List<ZString>();

			foreach (var item in NctsHeader.GetGoodsItems() ?? Enumerable.Empty<NctsDepartureCargoDesc>())
			{
				foreach (var package in item.Packages)
				{
					foreach (var container in package.ContainersPivot.Containers.Cast<NctsDepartureHeaderContainer>())
					{
						sealList.AddRange(container.AllSeals);
					}
				}
			}
			return sealList.Where(s => !s.IsEmpty).Distinct().ToArray();
		}

		ZString GetBoxS13Routing()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return GetBoxS13RoutingForPhase5();
			}
			return GetBoxS13RoutingForPhase4();
		}

		ZString GetBoxS13RoutingForPhase5()
		{
			var sb = new ZStringBuilder();
			CountryCodesOfRouting?.ForEach(code => sb.AppendIfNotEmpty(code));
			return sb.ToStringWithDelimiterBetweenAppends(",").TrimEnd();
		}

		protected virtual ZString GetBoxS13RoutingForPhase4()
		{
			var sb = new ZStringBuilder();
			Itinerary?.ForEach(item => sb.AppendIfNotEmpty(item));
			return sb.ToStringWithDelimiterBetweenAppends(";").TrimEnd();
		}

		ZString	GetBoxS28SealsNumber()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return GetBoxS28SealsNumberForPhase5();
			}
			return GetBoxS28SealsNumberForPhase4();
		}

		protected virtual ZString GetBoxS28SealsNumberForPhase5() => ConvertNumberToString(new ZInt(SelectedSeals.Length));

		protected virtual ZString GetBoxS28SealsNumberForPhase4() => ConvertNumberToString(MovementHeader?.BM_SealQty ?? ZShort.Zero);

		protected sealed override ZString GetBoxDSealsAffixedNumber()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return ConvertNumberToString(new ZInt(SelectedSeals.Length));
			}
			return base.GetBoxDSealsAffixedNumber();
		}

		protected sealed override ZString GetBoxSealsIdentity()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return ZString.Join(", ", SelectedSeals);
			}
			return base.GetBoxSealsIdentity();
		}

		protected sealed override ZString GetBox15CountryOfOrigin()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return Tools.ValueOrThreeDashes(MovementHeader.BM_RN_NKCountryOfDispatch);
			}
			return base.GetBox15CountryOfOrigin();
		}

		protected sealed override ZString GetBox35GrossMass()
		{
			return NctsHeader.IsPhase5Departure
					? GetBox35GrossMassForPhase5()
					: GetBox35GrossMassForPhase4();
		}

		protected virtual ZString GetBox35GrossMassForPhase4() => base.GetBox35GrossMass();

		protected virtual ZString GetBox35GrossMassForPhase5()
		{
			var mass = NctsHeader.GetGoodsItems().Sum(item => item.GrossMassInKilograms);
			return ConvertNumberToString(new ZDecimal(Utilities.Round(mass, 6)));
		}

		protected sealed override ZString GetBox5Items()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return ConvertNumberToString(NctsHeader.TotalNumberOfItems);
			}
			return GetBox5ItemsForPhase4();
		}

		protected virtual ZString GetBox5ItemsForPhase4() => base.GetBox5Items();

		protected sealed override ZString GetBox6Packages()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return ConvertNumberToString(NctsHeader.TotalNumberOfPackages);
			}
			return GetBox6PackagesForPhase4();
		}

		protected virtual ZString GetBox6PackagesForPhase4() => base.GetBox6Packages();

		protected sealed override ZString GetBoxCOfficeOfDeparture()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return GetOfficeCodeAndDescription(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			}
			return GetBoxCOfficeOfDepartureForPhase4();
		}

		protected virtual ZString GetBoxCOfficeOfDepartureForPhase4() => GetOfficeDescription(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		protected override ZString GetGuaranteeCodes()
		{
			if (NctsHeader.IsPhase5Departure)
			{
				return Guarantees.ToStringCertainNumberGuaranteeCodes(4);
			}
			return Guarantees.ToStringCertainNumberGuaranteeCodes(3);
		}

		protected override ZString GetMovementReferenceNumber()
		{
			if (IsPhase5)
			{
				return IsFallBackActive ? ZString.Empty : Mrn;
			}
			return base.GetMovementReferenceNumber();
		}

		IEnumerable<ZString> CountryCodesOfRouting => countryCodesOfRouting ??= NctsHeader.CountriesOfRouting.Select(country => country.CY_Data).ToArray();
		ZString[] countryCodesOfRouting;

		IEnumerable<ZString> Itinerary => itinerary ?? (itinerary = NctsHeader.Itinerary.Cast<NonPersistentItineraryCountry>().Select(leg => leg.CountryCode).ToArray());
		ZString[] itinerary;

		#endregion
	}
}
