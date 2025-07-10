using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.DocumentWrappers.TemporaryStorage;

public class TemporaryStorageHeaderWrapper : Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageHeaderWrapper
{
	public static TemporaryStorageHeaderWrapper New(TemporaryStorageHeader header, BusinessObjectFactory factoryToWrap) => new TemporaryStorageHeaderWrapper(header, factoryToWrap);

	TemporaryStorageHeaderWrapper(TemporaryStorageHeader header, BusinessObjectFactory factory) : base(header, factory)
	{
		premises = GetPremises();
	}

	new TemporaryStorageHeader ParentBusinessObject => (TemporaryStorageHeader)base.ParentBusinessObject;

	public ZString DestinationGoodsLocationAuthorisationNumber => ParentBusinessObject.DestinationGoodsLocation.Address.AuthorisationNumber;

	#region Premises

	public ZString PremisesType => premises.Type;
	public ZString PremisesCode => premises.Code;
	public ZString PremisesDescription => premises.Description;
	public ZString PremisesOwner => premises.Owner;
	readonly PremisesData premises;

	protected virtual PremisesData GetPremises()
	{
		var isLAM = ParentBusinessObject.AMA_MessageType == G5MessageTypeCodeList.Codes.LameManualEntry;
		var zQuery = new ZQuery(CusTempStorageRegPremisesSchema.SRP_CustomsLocation, DestinationGoodsLocationAuthorisationNumber)
				.AddToFilter(JoinCondition.And, CusTempStorageRegPremisesSchema.SRP_Type, SQLComparisonOperator.Equal,
				isLAM ? CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility : CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);

		var premises = Factory.LoadTop1<EU.TemporaryStorage.Business.CusTempStorageRegPremises>(zQuery);
		if (premises != null)
		{
			var premisesAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, premises.SRP_OA_PremisesAddress));
			var owner = premisesAddress?.OA_OH != ZGuid.Invalid ? Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, premisesAddress.OA_OH)).OH_FullName : ZString.Empty;
			return new PremisesData(premises.SRP_Type, premises.SRP_Code, premises.SRP_Description, owner);
		}

		return new PremisesData();
	}

	protected readonly struct PremisesData(ZString type, ZString code, ZString description, ZString owner)
	{
		public ZString Type { get; } = type;
		public ZString Code { get; } = code;
		public ZString Description { get; } = description;
		public ZString Owner { get; } = owner;
	}
	#endregion
}
