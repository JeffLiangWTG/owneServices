using System;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegPremisesProvider))]
sealed class CusTempStorageRegPremisesProviderTest : CollectionProviderBaseTest
{
	protected override Type ExpectedCollectionType => typeof(CusTempStorageRegPremisesCollection);

	protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.EU.TempStoragePremises;
}
