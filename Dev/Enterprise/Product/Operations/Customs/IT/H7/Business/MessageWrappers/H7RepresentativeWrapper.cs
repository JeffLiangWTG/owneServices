using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class H7RepresentativeWrapper : IH7Representative
{
	public H7RepresentativeWrapper(AsycudaManifestHeader header)
	{
		this.header = header;
	}

	readonly AsycudaManifestHeader header;

	public IH7Address Address => null;

	public int? RepresentativeType
	{
		get
		{
			int? result = header.AMA_AgentType.ToString() switch
			{
				EUH7AgentTypes.Codes.DIR => 2,
				EUH7AgentTypes.Codes.IND => 3,
				_ => null,
			};
			return result;
		}
	}

	public string EoriNumber => null;

	IAddress ITrader.Address => null;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => header.Representative?.CustomsCodes?
			.Cast<OrgCusCode>()
			.FirstOrDefault(o => o.OK_CodeType.EqualsIgnoringCase(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori))?
			.OK_CustomsRegNo ?? string.Empty);
	CachedValue<string> identificationNumber;
}
