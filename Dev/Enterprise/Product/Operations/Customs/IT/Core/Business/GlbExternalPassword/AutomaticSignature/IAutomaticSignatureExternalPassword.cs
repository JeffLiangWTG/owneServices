using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business;

public interface IAutomaticSignatureExternalPassword : IGlbExternalPassword
{
	ZBool IsConfigurationActive { get; }
	ZString GP_Name { get; }
	ZString DelegateName { get; }
}
