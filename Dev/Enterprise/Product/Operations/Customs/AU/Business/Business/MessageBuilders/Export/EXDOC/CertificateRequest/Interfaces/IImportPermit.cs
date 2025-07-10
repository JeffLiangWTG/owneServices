using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	interface IImportPermit
	{
		ZString PermitNumber { get; }
		ZDateTime PermitDate { get; }
	}
}
