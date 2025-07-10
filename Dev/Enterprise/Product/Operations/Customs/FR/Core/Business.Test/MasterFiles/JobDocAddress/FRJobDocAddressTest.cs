using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	[TestedType(typeof(FRJobDocAddress))]
	class FRJobDocAddressTest : JobDocAddressTest
	{
		protected override BusinessObject GetNewBusinessObject() => DocAddress;

		FRJobDocAddress DocAddress => docAddress ?? (docAddress = Factory.New<FRJobDocAddress>());
		FRJobDocAddress docAddress;
	}
}
