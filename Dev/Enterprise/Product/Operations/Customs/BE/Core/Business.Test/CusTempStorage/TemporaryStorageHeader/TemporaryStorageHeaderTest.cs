using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageHeader))]
sealed class TemporaryStorageHeaderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageHeaderAbstractTest<TemporaryStorageHeader>
{
	protected override Type ExpectedTypeOfContainer => typeof(ManifestBase.AsycudaContainerCollection<TemporaryStorageContainer, EU.Business.CusTempStorage.TemporaryStorageHeader>);

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
	{
		return new TemporaryStorageHeaderLightValidationTester(bizObjToTest);
	}

	sealed class TemporaryStorageHeaderLightValidationTester : LightValidationTester
	{
		public TemporaryStorageHeaderLightValidationTester(BusinessObject bo) : base(bo)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			return base.ShouldTestProperty(info) && info.Name != TemporaryStorageBill.Schema.ABL_RL_NKPortOfDischarge;
		}
	}

	public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
	{
		var result = base.GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues();
		result.Add(TemporaryStorageHeader.Schema.AMA_OA_Declarant);
		return result;
	}

	public void TestMessageSendingConfiguration() => AssertType<TemporaryStorageMessageSendingConfiguration>(((TemporaryStorageHeader)GetNewBusinessObject()).MessageSendingConfiguration);
}
