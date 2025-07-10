using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ContractRevocation5ULCollection))]
	sealed class ContractRevocation5ULCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ContractRevocation5UL>
	{
		protected override CusSupportingInfoCollection<ContractRevocation5UL> GetCusSupportingInfoCollection()
		{
			var cusReconEntryline = Factory.New<CusReconDeclaration>().CusReconEntryLines.AddNew();
			return new ContractRevocation5ULCollection(cusReconEntryline);
		}
	}
}
