using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BE.NCTS.DocumentWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class NctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper
	{
		public const string CancellationLogReferenceCode = "CAN";

		protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public new static Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));
			return new NctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
		}

		public ZString CancellationDate => GetCancellationDate();

		public ZString LRN => MovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty;

		public ZString HolderOfTheTransitProcedureIdentificationNumber => GetHolderOfTheTransitProcedureIdentificationNumber();

		public ZString HolderOfTheTransitProcedureTIRNumber => MovementHeader?.TirCarnetNumber ?? ZString.Empty;

		public ZString HolderOfTheTransitProcedureName => GetHolderOfTheTransitProcedureName();

		public ZString BOX7UCR => MovementHeader?.BM_UniqueConsignmentReference ?? ZString.Empty;

		public ZString BOX21BORDERTRANSPORTFLAG => Tools.ValueOrThreeDashes(MovementHeader?.BM_TOLCarrierCode ?? ZString.Empty);

		public ZString BOX21BORDERTRANSPORTID => Tools.ValueOrThreeDashes(MovementHeader?.BM_TOLCarrierID ?? ZString.Empty);

		public ZString BOX25BORDERTRANSPORTMODE => Tools.ValueOrThreeDashes(MovementHeader?.BM_ExportTransportMode ?? ZString.Empty);

		public ZString BOX30LOCATIONOFGOODS => ((MovementHeader?.BM_LocationOfGoods ?? ZString.Empty) + " " + (MovementHeader?.BM_CustomsSubPlace ?? ZString.Empty)).Trim();

		public ZString BOX50REPRESENTATIVE => Representative?.TIN ?? ZString.Empty;

		public ZString BOX7REFERENCENUMBERS => NctsHeader.LocalReferenceNumber;

		public ZString BOXS10CONVEYANCE => Tools.ValueOrThreeDashes(MovementHeader?.BM_ConveyanceNumber ?? ZString.Empty);

		public ZString BOXS12FIRSTARRIVALTIME => "---";

		public ZString BOXS13ROUTING => GetItineraries();

		public ZString BOXS17PLACEOFLOADING => Tools.ValueOrThreeDashes(MovementHeader?.BM_RL_NKForeignDestPort ?? ZString.Empty);

		public ZString BOXS18PLACEOFUNLOADING => Tools.ValueOrThreeDashes(MovementHeader?.BM_PlaceOfUnloading ?? ZString.Empty);

		public ZString BOXS28SEALSNUMBER => ConvertNumberToString(MovementHeader?.BM_SealQty ?? ZShort.Zero);

		public ZString BOXS29TRANSPORTCHARGESMOP => Tools.ValueOrThreeDashes(MovementHeader?.BM_MethodOfPayment ?? ZString.Empty);

		public ZString BOXS32OTHERSCI => Tools.ValueOrThreeDashes(MovementHeader?.BM_BTAIndicator ?? ZString.Empty);

		public ZString BOXS4SECURITYCONSIGNOR => SecurityConsignor.GetWrappedAddress();

		public ZString BOXS4SECURITYCONSIGNOREORI => SecurityConsignor?.TIN ?? ZString.Empty;

		public ZString BOXS6SECURITYCONSIGNEE => SecurityConsignee.GetWrappedAddress();

		public ZString BOXS6SECURITYCONSIGNEEEORI => SecurityConsignee?.TIN ?? ZString.Empty;

		public ZString BOXS7CARRIER => GetCarrier()?.GetWrappedAddress() ?? ZString.Empty;

		public ZString BOXS7CARRIEREORI => GetCarrier()?.TIN ?? ZString.Empty;

		public ZString PRESENTATIONOFGOODSDATETIME => ZString.Empty;

		public ZString BOX44AUTHORISATIONS => GetAuthorisations();

		public ZBool BOXS00SECURITY => NctsHeader.IsSecurityDeclaration;

		ZString GetAuthorisations()
		{
			var sb = new ZStringBuilder();
			var cusAuthorizationUsages = NctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)NctsHeader.MovementHeader.CusAuthorizationUsages : NctsHeader.CusAuthorizationUsages;
			cusAuthorizationUsages.ForEach(a => sb.Append($"{a.AGC_Code} - {a.AGC_Number}"));
			return sb.ToStringWithNewLineBetweenAppends();
		}

		protected override DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> GetLinesCore() => new NctsDepartureCargoDescWrapperCollection(NctsHeader.Bills.FirstOrDefault(), Factory);

		ITrader Representative => CachedValueHelper.GetValue(ref representative, () => TraderWrapper.New(MovementHeader.Representative, populateTIR: false, IsAddressExtended));
		CachedValue<ITrader> representative;

		ITrader SecurityConsignee => CachedValueHelper.GetValue(ref securityConsignee, () => TraderWrapper.New(NctsHeader.SecurityConsignee, false, IsAddressExtended));
		CachedValue<ITrader> securityConsignee;

		public IEnumerable<ZString> Itineraries => itineraries ?? (itineraries = NctsHeader.Itinerary.Cast<NonPersistentItineraryCountry>().Select(leg => leg.CountryCode).ToArray());
		ZString[] itineraries;

		ITrader SecurityConsignor => CachedValueHelper.GetValue(ref securityConsignor, () => TraderWrapper.New(NctsHeader.SecurityConsignor, false, IsAddressExtended));
		CachedValue<ITrader> securityConsignor;

		protected virtual ITrader GetCarrier() => CachedValueHelper.GetValue(ref carrier, () => TraderWrapper.New(NctsHeader.CarrierOrgAddress, false, IsAddressExtended));
		CachedValue<ITrader> carrier;

		ZString GetItineraries()
		{
			var sb = new ZStringBuilder();
			foreach (var itineraryItem in Itineraries)
			{
				sb.AppendIfNotEmpty(itineraryItem);
			}
			return sb.ToStringWithDelimiterBetweenAppends(";").TrimEnd();
		}

		ZString GetCancellationDate()
		{
			var log = NctsHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, CancellationLogReferenceCode);

			return ConvertDateToString(log?.SL_EventTime.Date);
		}

		ZString GetHolderOfTheTransitProcedureIdentificationNumber()
		{
			ZString result;
			var orgHeader = NctsHeader.Principal?.Organisation;
			var firstEoriCusCode = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).FirstOrDefault();
			if (firstEoriCusCode != null)
			{
				result = firstEoriCusCode.OK_RN_NKCodeCountry + firstEoriCusCode.OK_CustomsRegNo;
			}
			else
			{
				var firstTcuCusCode = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).FirstOrDefault();
				if (firstTcuCusCode != null)
				{
					result = firstTcuCusCode.OK_RN_NKCodeCountry + firstTcuCusCode.OK_CustomsRegNo;
				}
				else
				{
					result = ZString.Empty;
				}
			}

			return result;
		}

		ZString GetHolderOfTheTransitProcedureName()
		{
			var result = NctsHeader.Principal.Address?.OA_CompanyNameOverride ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = NctsHeader.Principal.Organisation?.OH_FullName ?? ZString.Empty;
			}

			return result;
		}
	}
}
