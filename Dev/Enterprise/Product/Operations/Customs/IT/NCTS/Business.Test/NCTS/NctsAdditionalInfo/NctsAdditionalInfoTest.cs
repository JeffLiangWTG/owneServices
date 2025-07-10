using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsAdditionalInfo))]
sealed class NctsAdditionalInfoTest : EU.NCTS.Business.Testing.NctsAdditionalInfoTest<NctsAdditionalInfo>
{
	public void TestValidation()
	{
		var additionalInfo = GetNewAdditionalInfo(Factory);
		AssertType<NctsAdditionalInfoValidation>("Validation type", additionalInfo.Validation);
	}

	public void TestCSI_CodeMaxLength()
	{
		var additionalInfo = GetNewAdditionalInfo(Factory);
		AssertEquals("CSI_Code MaxLength", NctsAdditionalInfo.Schema.CSI_CodeMaxLength, additionalInfo.CSI_CodeInfo.MaxLength);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewAdditionalInfo(Factory);

	NctsAdditionalInfo GetNewAdditionalInfo(BusinessObjectFactory factory)
	{
		var header = factory.NewDepartureNctsHeader();
		var goodsItem = header.MovementHeader.GoodsItems.AddNew();
		return goodsItem.AdditionalInfos.AddNew();
	}
}
