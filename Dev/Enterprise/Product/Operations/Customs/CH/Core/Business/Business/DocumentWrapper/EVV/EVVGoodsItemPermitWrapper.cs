using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemPermitWrapper : DocumentWrapper
{
	const string EmptyProperty = "---";

	public static EVVGoodsItemPermitWrapper New(IEvvGoodsItemPermit permit, BusinessObjectFactory factory)
		=> new EVVGoodsItemPermitWrapper(Argument.NotNull(permit, nameof(permit)), Argument.NotNull(factory, nameof(factory)));

	EVVGoodsItemPermitWrapper(IEvvGoodsItemPermit permit, BusinessObjectFactory factory)
		: base(permit, factory)
	{
		this.permit = permit;
	}

	readonly IEvvGoodsItemPermit permit;

	public ZString PermitType => string.IsNullOrEmpty(permit.PermitType)
		? EmptyProperty
		: RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitType, ZDateTime.Now).GetDescriptionFromCode(permit.PermitType) ?? permit.PermitType;

	public ZString PermitAuthority => string.IsNullOrEmpty(permit.PermitAuthority)
		? EmptyProperty
		: RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitAuthority, ZDateTime.Now).GetDescriptionFromCode(permit.PermitAuthority) ?? permit.PermitAuthority;

	public ZString PermitNumber => string.IsNullOrEmpty(permit.PermitNumber) ? EmptyProperty : permit.PermitNumber;

	public ZString IssueDate => permit.IssueDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? EmptyProperty;

	public ZString AdditionalInformation => string.IsNullOrEmpty(permit.AdditionalInformation) ? EmptyProperty : permit.AdditionalInformation;
}
