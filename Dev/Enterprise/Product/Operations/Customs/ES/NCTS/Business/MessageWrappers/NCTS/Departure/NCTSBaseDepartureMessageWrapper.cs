using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsBaseDepartureMessageWrapper : NctsHeaderMessageWrapper, INctsBaseDepartureMessageProvider
	{
		public NctsBaseDepartureMessageWrapper(NctsHeader header, ICertificateProvider certificateData)
			: base(header, certificateData)
		{
			movementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));

			Argument.GreaterThan(header.IsPhase5 ? header.Bills.Sum(x => x.GoodsItems.Count) : movementHeader.GoodsItems.Count, 0, nameof(movementHeader.GoodsItems));
		}

		protected readonly NctsDepartureMovementHeader movementHeader;

		public ZString CustomsProcedureCategory5 => nctsHeader.DepartureCustomsOfficeCode.Right(4);

		public ZString LocationOfGoodsExamCustomsOffice => GetTrimmedBM_LocationOfGoodsCode().Left(4);

		public ZString LocationOfGoodsExam => GetTrimmedBM_LocationOfGoodsCode().SubstringSafe(4);

		ZString GetTrimmedBM_LocationOfGoodsCode()
		{
			if (trimmedLocation == null)
			{
				movementHeader.Factory.GetValue(ref trimmedLocation, () =>
				{
					var location = movementHeader.BM_LocationOfGoodsCode;
					return location.Length > 10 ? location.SubstringSafe(4) : location;
				});
			}
			return trimmedLocation.Value;
		}
		CachedProperty<ZString> trimmedLocation;

		public ZString CodeOfLoadingLocation => movementHeader.BM_RL_NKForeignDestPort;

		public ZString CodeOfUnloadingLocation => nctsHeader.PlaceOfUnloadingCode;

		public ZBool GoodsInContainerIndicator => movementHeader.IsContainerised;

		public ZBool SecurityDeclaration => nctsHeader.BH_FTZMove;

		public IReadOnlyCollection<ZString> CountryCodes
		{
			get
			{
				if (countryCodes == null)
				{
					countryCodes = !nctsHeader.ItineraryCountries.IsEmpty ? nctsHeader.ItineraryCountries.Split(" ").ToList().AsReadOnly() : new List<ZString>().AsReadOnly();
				}
				return countryCodes;
			}
		}
		IReadOnlyCollection<ZString> countryCodes;

		public IReadOnlyCollection<ZString> SealCodes => sealCodes ?? (sealCodes = GetSealCodes());
		IReadOnlyCollection<ZString> sealCodes;

		IReadOnlyCollection<ZString> GetSealCodes()
		{
			var containers = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>();
			var sealCodes = containers.Where(cont => !cont.Seal1.IsEmpty).Select(cont => cont.Seal1).ToList();
			sealCodes.AddRange(containers.Where(cont => !cont.Seal2.IsEmpty).Select(cont => cont.Seal2));
			sealCodes.AddRange(nctsHeader.Seals.Cast<Seal>().Where(x => !x.CY_Data.IsEmpty).Select(x => x.CY_Data));
			return sealCodes.ToList().AsReadOnly();
		}

		public ZString TransportMethodOfPayment => movementHeader.BM_MethodOfPayment;

		public ZString ConveyanceReferenceNumber => movementHeader.BM_ConveyanceNumber;

		public ZString ReferenceNumber => !movementHeader.BM_AdditionalText.IsEmpty ? movementHeader.BM_AdditionalText : nctsHeader.LocalReferenceNumber;

		public ZString SpecificCircumstancesIndicator => movementHeader.BM_BTAIndicator;

		public ITransportMediumInfoCommon BorderTransportMode => borderTransportMode ?? (borderTransportMode = new TransportMediumInfoCommonWrapper(movementHeader.BM_ExportTransportMode, movementHeader.BM_TOLCarrierID, movementHeader.BM_TOLCarrierCode));
		TransportMediumInfoCommonWrapper borderTransportMode;

		public IPartyEmailProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => NctsDeparturePartyEmailWrapper.New(nctsHeader));
		CachedValue<IPartyEmailProvider> declarant;

		public IPartyProvider SecurityCarrier => CachedValueHelper.GetValue(ref securityCarrier, () => PartyWrapper.New(nctsHeader.Carrier));
		CachedValue<IPartyProvider> securityCarrier;

		public IPartyProvider SecurityConsignor => CachedValueHelper.GetValue(ref securityConsignor, () => PartyWrapper.New(nctsHeader.SecurityConsignor));
		CachedValue<IPartyProvider> securityConsignor;

		public IPartyProvider SecurityConsignee => CachedValueHelper.GetValue(ref securityConsignee, () => PartyWrapper.New(nctsHeader.SecurityConsignee));
		CachedValue<IPartyProvider> securityConsignee;

		public ZInt TotalNumberOfGoods => movementHeader.GoodsItems.Count;

		public ZLong TotalNumberOfPackageElements => CachedValueHelper.GetValue(ref totalNumberOfPackageElementsCached, () =>
		{
			var amount = ZLong.Zero;
			amount += movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Sum(goodsItem => GetPackageAmountFromDepartureGoodItem(goodsItem));

			return amount;
		});
		CachedValue<ZLong> totalNumberOfPackageElementsCached;
	}
}
