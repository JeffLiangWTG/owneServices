using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	public class NctsHeaderDocumentWrapper : NctsHeaderDocBaseWrapper, INctsHeaderDocumentWrapper
	{
		protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		static Comparison<NctsDepartureCargoDescWrapper> declarationItemNumberComparison
			=> (x, y) => x.DeclarationItemNumber - y.DeclarationItemNumber;

		static Comparison<NctsDepartureCargoDescWrapper> billAndLineNumberComparison
			=> (x, y) =>
			{
				if (x.BillSequenceNumber == y.BillSequenceNumber)
				{
					return x.LineNumber - y.LineNumber;
				}

				return x.BillSequenceNumber - y.BillSequenceNumber;
			};

		internal static Type GetDocNctsType(Type nctsType) =>
			nctsType.FullName switch
			{
				"Enterprise.Customs.ES.NCTS.Business.NctsHeader" => ObjectFactory.GetType("ES.NCTS.NctsHeaderDocumentWrapper"),
				"Enterprise.Customs.IT.NCTS.Business.NctsHeader" => ObjectFactory.GetType("IT.NCTS.NctsHeaderDocumentWrapper"),
				_ => typeof(NctsHeaderDocumentWrapper)
			};

		public static NctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factory)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));
			return ShouldUseSecurityNctsHeaderDocumentWrapper(nctsHeader)
				? SecurityNctsHeaderDocumentWrapper.New(nctsHeader, factory)
				: new NctsHeaderDocumentWrapper(nctsHeader, factory);
		}

		public static ZBool ShouldUseSecurityNctsHeaderDocumentWrapper(NctsHeader nctsHeader)
		{
			return nctsHeader.IsSecurityDeclaration || nctsHeader.IsPhase5;
		}

		#region Cached Properties

		EnterpriseInformationRetriever EnterpriseInfo => fEnterpriseInfo ?? (fEnterpriseInfo = new EnterpriseInformationRetriever());
		EnterpriseInformationRetriever fEnterpriseInfo;

		#endregion

		#region Document Properties

		public DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = GetLinesCore();

					var sortByIndex = lines.Cast<NctsDepartureCargoDescWrapper>().Any(l => l.DeclarationItemNumber.IsEmpty);
					if (sortByIndex)
					{
						lines.Sort(billAndLineNumberComparison);
						FillBox32ItemIndex(lines);
					}
					else
					{
						lines.Sort(declarationItemNumberComparison);
					}
				}

				return lines;
			}
		}

		DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> lines;

		protected virtual DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> GetLinesCore()
			=> NctsHeader.IsPhase5 ? new NctsDepartureCargoDescWrapperCollection(NctsHeader.Bills.FirstOrDefault(), Factory) : new NctsDepartureCargoDescWrapperCollection(NctsHeader, Factory);

		public ZString BOX1REGIME => MovementHeader?.BM_InBondEntryType ?? ZString.Empty;

		public ZString BOX2CONSIGNOR => Consignor.GetWrappedAddress();

		public ZString BOX2CONSIGNOREORI => Consignor?.TIN ?? ZString.Empty;

		public ZString BOX15COUNTRYOFORIGIN => GetBox15CountryOfOrigin();

		public ZString BOX8CONSIGNEE => Consignee.GetWrappedAddress();

		public ZString BOX8CONSIGNEEEORI => Consignee?.TIN ?? ZString.Empty;

		public ZString BOX17COUNTRYOFDESTINATION => MovementHeader?.BM_RL_NKDestinationPort ?? ZString.Empty;

		public ZString BOXCOFFICEOFDEPARTURE => GetBoxCOfficeOfDeparture();

		public ZString RETURNOFFICEADDRESS => GetOfficeAddress(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		public ZString BOXCOFFICEOFDEPARTURECODE => GetBoxCOfficeOfDepartureCode();

		public ZBool SHOWSTAMPONBOXC => GetShowStampOnBoxC();

		public ZString BOXCDEPARTUREOFFICECOUNTRYCODE => GetBoxCDeparureOfficeCountryCode();

		public ZString BOXCUNIQUEREFERENCENUMBER => GetBoxCUniqueReferenceNumber();

		public ZString BOXCAUTHORIZEDCONSIGNORNAME => GetBoxCAuthorizedConsignorName();

		public ZString BOXCAUTHORISATIONNUMBER => GetBoxCAuthorisationNumber();

		public ZString BOX53OFFICEOFDESTINATION => GetOfficeOfDestination();

		public ZString BOX5ITEMS => GetBox5Items();

		public ZString BOX6PACKAGES => GetBox6Packages();

		public ZString BOX18DEPARTURETRANSPORTID => MovementHeader?.BM_TransportAtDeparture ?? ZString.Empty;

		public ZString BOX18DEPARTURETRANSPORTFLAG => MovementHeader?.BM_RN_NKTransportAtDepartureCountry ?? ZString.Empty;

		public ZString BOX35GROSSMASS => GetBox35GrossMass();

		public ZString BOX38NETTMASS => ConvertNumberToString(NctsHeader.TotalNettMassInKilograms);

		public ZString BOX50PRINCIPAL => Principal.GetWrappedAddress();

		public ZString BOX50PRINCIPALEORI => Principal?.TIN ?? ZString.Empty;

		public ZString BOX50SIGNATURE => GetBOX50Signature();

		public ZString BOX51TRANSITOFFICE1 => GetTransitCustomsOffices(1);

		public ZString BOX51TRANSITOFFICE2 => GetTransitCustomsOffices(2);

		public ZString BOX51TRANSITOFFICE3 => GetTransitCustomsOffices(3);

		public ZString BOX51TRANSITOFFICE4 => GetTransitCustomsOffices(4);

		public ZString BOX51TRANSITOFFICE5 => GetTransitCustomsOffices(5);

		public ZString BOX51TRANSITOFFICE6 => GetTransitCustomsOffices(6);

		public ZString BOX52GUARANTEE => GetGuarantee();

		public ZString BOX52GUARANTEEVALIDITY => Guarantees.ToStringThreeFirstGuaranteeValidities();

		public ZString BOX52GUARANTEECODE => GetGuaranteeCodes();

		public ZString BOXCDATE => GetBoxCDate();

		public ZString BOXDRESULT => GetBoxDResult();

		public ZString BOXDTIMELIMITDATE => GetBoxDTimeLimitDate();

		public ZString BOXDSIGNATURE => GetBoxDSignature();

		public IEnumerable<ISealID> Seals => seals ??= GetSeals().ToArray();
		ISealID[] seals;

		public ZString BOXDSEALSAFFIXEDNUMBER => GetBoxDSealsAffixedNumber();

		public ZString BOXDSEALSIDENTITY => GetBoxSealsIdentity();

		public ZString EDIENTERPRISEVERSION => (NoResString)"WiseTechGlobal.com - " + Core.Constants.ProductName + (NoResString)" v" + EnterpriseInfo.VersionNumber; // Version Information

		public ZString FALLBACKINFORMATION => CachedValueHelper.GetValue(ref fallbackInformationCache, () => NctsHeader.FallbackInformation);
		CachedValue<ZString> fallbackInformationCache;

		public ZString NOTRELEASEDWATERMARK => GetNotReleasedWatermark();

		public ZString BOXDCLEARANCE => GetBoxDClearance();

		#endregion

		#region Methods

		protected IEnumerable<ISealID> GetSeals()
		{
			var result = Enumerable.Empty<ISealID>();
			var sealType = MovementHeader?.BM_SealType ?? ZString.Empty;
			switch (sealType)
			{
				case SealTypeList.Codes.ContainerSeal:
					result = SealsSelected.Select(x => new SealWrapper(x))
						.Cast<ISealID>();
					break;

				case SealTypeList.Codes.PackageSeal:
					result = NctsHeader.Seals
						.Cast<Seal>()
						.Select(x => new SealWrapper(x.CY_Data))
						.Cast<ISealID>();
					break;
			}

#if NETFRAMEWORK
			return result.Where(x => !x.SealIdentity.IsEmpty).DistinctBy(x => x.SealIdentity);
#else
			return IEnumerableExtensions.DistinctBy(result.Where(x => !x.SealIdentity.IsEmpty), x => x.SealIdentity);
#endif
		}

		ZString[] SealsSelected => sealsSelected ??= NctsHeader.GetSealsSelected();
		ZString[] sealsSelected;

		protected virtual ZString GetTransitCustomsOffices(ZInt rank)
		{
			var customsOffices = NctsHeader.IsPhase5 ? NctsHeader.MovementHeader.CustomsOffices : NctsHeader.CustomsOffices;
			var transitCustomsOfficeList = customsOffices.Cast<NctsEuOfficeCode>().Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).ToList();
			var selectedTransitCustomsOffice = transitCustomsOfficeList.Count >= rank ? transitCustomsOfficeList[rank - 1] : null;
			string officeCode = selectedTransitCustomsOffice?.CY_Data ?? ZString.Empty;
			string officeDescription = selectedTransitCustomsOffice?.CY_OfficeDescription ?? ZString.Empty;
			return officeDescription == ZString.Empty ? officeCode : officeCode + " (" + officeDescription + ")";
		}

		protected ZString GetOfficeCodeAndDescription(ZString purpose)
		{
			string officeCode = GetOfficeCode(purpose);
			string officeDescription = GetOfficeDescription(purpose);
			return officeDescription == ZString.Empty ? officeCode : officeCode + " (" + officeDescription + ")";
		}

		protected ZString GetOfficeCode(ZString purpose)
		{
			return NctsEuOfficeCode.Load<NctsEuOfficeCode>(NctsHeader.IsPhase5 ? NctsHeader.MovementHeader : NctsHeader, purpose)?.CY_Data ?? ZString.Empty;
		}

		ZString GetOfficeAddress(ZString purpose)
		{
			var office = NctsEuOfficeCode.Load<NctsEuOfficeCode>(NctsHeader, purpose);
			string officeDescription = office?.CY_OfficeDescription ?? ZString.Empty;
			string officeAddress = office?.CY_OfficeAddress ?? ZString.Empty;
			return ZString.Format("{0}\n{1}", officeDescription, officeAddress);
		}

		ZString GetOfficeCountryCode(ZString purpose)
		{
			var office = NctsEuOfficeCode.Load<NctsEuOfficeCode>(NctsHeader, purpose);
			return office?.CY_Data.Left(2) ?? ZString.Empty;
		}

		protected ZString GetOfficeDescription(string purpose)
		{
			return NctsEuOfficeCode.Load<NctsEuOfficeCode>(NctsHeader.IsPhase5 ? NctsHeader.CommonMovementHeader : NctsHeader, purpose)?.CY_OfficeDescription ?? ZString.Empty;
		}

		protected virtual ZString GetNotReleasedWatermark()
		{
			switch (MovementHeader?.BM_CustomsStatus ?? ZString.Empty)
			{
				case NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture:
				case NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival:
				case NctsTransitStatusList.Codes.GoodsWrittenOff:
				case NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit:
				case NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed:
					return ZString.Empty;

				default:
					return Tools.GetNotReleasedCaption();
			}
		}

		protected virtual ZString ConvertNumberToString(IZType number) => number?.ToString() ?? ZString.Empty;

		protected virtual ZString ConvertDateToString(ZDateTime? date) => date?.ToString("dd/MM/yyyy") ?? ZString.Empty;

		protected virtual ZString GetBoxCDate()
		{
			var log = NctsHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code).AddToFilter(StmALogSchema.SL_Reference, NctsTransitStatusList.Codes.DeclarationAccepted)).FirstOrDefault();
			return ConvertDateToString(log?.SL_EventTime.Date);
		}

		protected virtual ZString GetBoxCOfficeOfDepartureCode() => GetOfficeCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		protected virtual ZBool GetShowStampOnBoxC() => ZBool.False;

		protected virtual ZString GetBoxCDeparureOfficeCountryCode() => GetOfficeCountryCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		protected virtual ZString GetBoxCUniqueReferenceNumber() => LOCALREFERENCENUMBER;

		protected virtual ZString GetBoxCAuthorizedConsignorName() => ZString.Empty;

		protected virtual ZString GetBoxCAuthorisationNumber() => MovementHeader?.BM_LocationOfGoodsCode ?? ZString.Empty;

		protected virtual ZString GetBoxDResult() => MovementHeader?.BM_GONumber ?? ZString.Empty;

		protected virtual ZString GetBoxDTimeLimitDate() => ConvertDateToString(MovementHeader?.BM_ExportDate);

		protected virtual ZString GetBoxDClearance() => ZString.Empty;

		protected virtual ZString GetBoxDSealsAffixedNumber() => ConvertNumberToString((ZInt)Seals.Count());

		protected virtual ZString GetBoxDSignature() => ZString.Empty;

		protected virtual ZString GetBOX50Signature() => ZString.Empty;

		protected virtual ZString GetBoxSealsIdentity() => ZString.Join("; ", GetSeals().Select(x => x.SealIdentity).ToArray());

		protected virtual ZString GetBox15CountryOfOrigin() => NctsHeader.BH_RL_NKImportLoadPort;

		protected virtual ZString GetBoxCOfficeOfDeparture() => GetOfficeDescription(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		protected virtual ZString GetOfficeOfDestination() => GetOfficeCodeAndDescription(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

		protected virtual ZString GetBox5Items() => ConvertNumberToString(NctsHeader?.TotalNumberOfItems);

		protected virtual ZString GetBox6Packages() => ConvertNumberToString(NctsHeader?.TotalNumberOfPackages);

		protected virtual ZString GetBox35GrossMass() => ConvertNumberToString(NctsHeader?.TotalGrossMassInKilograms);

		protected virtual ZString GetGuarantee() => Guarantees.ToStringCertainNumberGuarantees(3);

		protected virtual ZString GetGuaranteeCodes() => Guarantees.ToStringThreeFirstGuaranteeCodes();

		void FillBox32ItemIndex(DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> lines)
		{
			var index = new ZInt(1);
			lines.Cast<NctsDepartureCargoDescWrapper>().ForEach(l => l.Box32ItemIndex = ConvertNumberToString(index++));
		}
		#endregion
	}
}
