using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader, IInlandTransportParent, Integration.Customs.BE.IDepartureMovementHeader
{
	public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsDepartureMovementHeader.Schema
	{
		public new const int BM_AdditionalTextMaxLength = 500;
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	[ChildEditable]
	public InlandTransportCollection InlandTransports
	{
		get
		{
			if (inlandTransports == null)
			{
				inlandTransports = new InlandTransportCollection(this);
				inlandTransports.Load();
				RegisterEditableChildObject(inlandTransports);
			}

			return inlandTransports;
		}
	}
	InlandTransportCollection inlandTransports;

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore() => new Dictionary<ZString, Type>
	{
		{ Constants.CusCodeDataTypes.TransportInland, typeof(InlandTransport) },
		{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(NctsEuOfficeCode) }
	};

	#region InlandTransportLineNumberGenerator

	public IEnumerable<IShortSequenceNumberLine> InlandTransportLines => new TypedEnumerable<IShortSequenceNumberLine>(InlandTransports);

	public ShortSequenceNumberGenerator InlandTransportLineNumberGenerator => inlandTransportLineNumberGenerator ?? (inlandTransportLineNumberGenerator = new ShortSequenceNumberGenerator(() => InlandTransportLines));
	ShortSequenceNumberGenerator inlandTransportLineNumberGenerator;

	#endregion

	protected override INumberFountainProxy LrnNumberFountain => Env.NumberFountains.BELocalReferenceNumber(GlbCompany.CurrentCompany.PK.ToGuid());

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public bool IsAmendingDeclarationAllowed => BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged || BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged || BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated || BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

	[MaxLength(Schema.BM_AdditionalTextMaxLength)]
	public override ZString BM_AdditionalText { get => base.BM_AdditionalText; set => base.BM_AdditionalText = value; }

	public new NctsDepartureMovementHeaderLookups Lookups => (NctsDepartureMovementHeaderLookups)base.Lookups;

	protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderLookups(this);

	protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderLookups(this);

	public new NctsDepartureMovementHeaderValidation Validation => (NctsDepartureMovementHeaderValidation)base.Validation;

	protected override NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderValidation(this);

	public new NctsEuOfficeCode EnquiryCustomsOffice => EU.NCTS.Business.NctsEuOfficeCode.LoadOrCreate<EU.NCTS.Business.NctsEuOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

	public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

	protected override void DefaultDepartureLocationCodeFromCusAuthorisationIfBlank(CusAuthorisationHeader authorizationToUse)
	{
		var configuration = Header.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
		if (configuration.IsDefaultingEnabled)
		{
			if ((GoodsLocation?.DisplayText ?? ZString.Empty).IsEmpty)
			{
				var locationCode = GetLocationCodeFromCusAuthorisation(authorizationToUse);
				if (locationCode != null)
				{
					if (locationCode.CPR_ValueFrom.Length == 8)
					{
						GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
						GoodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
						GoodsLocation.CGL_CustomsOffice = locationCode.CPR_ValueFrom.SubstringSafe(0, CusGoodsLocationSchema.CGL_CustomsOffice.MaxLength);
					}
					else
					{
						GoodsLocation.CGL_Qualifier = configuration.QualifierCode;
						GoodsLocation.CGL_Type = configuration.TypeCode;
						GoodsLocation.CGL_AdditionalIdentifier = locationCode.CPR_ValueFrom.SubstringSafe(0, CusGoodsLocationSchema.CGL_AdditionalIdentifier.MaxLength);
					}
				}
			}
		}
	}

	protected override bool IsRepresentativeReadOnly => base.IsRepresentativeReadOnly || Header.AdditionalDocuments.Any(x => x.CSI_Code == Constants.AdditionalDocumentTypes._4009 && x.IsAnAdditionalReference);
}
