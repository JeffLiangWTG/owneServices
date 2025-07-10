using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.Registry.Business.Testing
{
	sealed class OutturnRespPartyIDCodeDescriptionPairListRegistryDataTypeForTest : OutturnRespPartyIDCodeDescriptionPairListRegistryDataType
	{
		public OutturnRespPartyIDCodeDescriptionPairListRegistryDataTypeForTest(int codeMaxLength) : base(codeMaxLength)
		{
		}
		public void ValidateCoreForTesting(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK) => ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
	}
}

