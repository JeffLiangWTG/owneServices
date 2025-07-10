using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing;

[TestedType(typeof(AUDeclarationValueObjectDataAdapter))]
public class AUDeclarationValueObjectDataAdapterTest_AHECC : AUDeclarationValueObjectDataAdapterTest
{
	protected override void SetupLine1Tariff()
	{
		var ahecc = Factory.New<AUCAHECC>();
		ahecc.UA_AHECC = "0105.12.02";
		ahecc.UA_ShortDescription = "Turkeys weighing not more than 185g";
		ahecc.UA_UQ = "NO";
		Factory.Save();
	}

	protected override bool UseCustomsReferenceDataValue => false;

	protected override bool EnableCWRefForAHECCValue => false;
}
