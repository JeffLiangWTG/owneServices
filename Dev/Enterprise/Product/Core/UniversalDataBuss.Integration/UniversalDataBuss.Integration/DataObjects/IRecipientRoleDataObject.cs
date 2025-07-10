using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IRecipientRoleDataObject : IDataObject
	{
		RecipientRoleType? Code { get; }
		ZString? Description { get; }
		ServiceCodeType? ServiceCode { get; }
		ZString? ServiceDescription { get; }
	}
}
