using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitHeader))]
sealed class CusExitHeaderClusterKeyTest : ClusterKeyMasterMandatoryTest
{
	protected override IClusterKeyEntity NewClusterKeyEntity()
	{
		var header = Factory.New<CusExitHeader>();
		header.CXH_JobReference = "123";
		header.CXH_ApplicationCode = Common.CusExitHeaderApplicationCodeList.Codes.ExitControl;
		return header;
	}
}
