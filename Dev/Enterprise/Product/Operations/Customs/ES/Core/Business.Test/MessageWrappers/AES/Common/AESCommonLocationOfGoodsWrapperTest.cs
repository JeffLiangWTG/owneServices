using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class AESCommonLocationOfGoodsWrapperTest : WrapperHelperTest<AESCommonLocationOfGoodsWrapper>
{
	public void TestLocationContactPerson()
	{
		CombineAssertions(() =>
		{
			goodsLocation.Address.E2_Contact = "Contact Name";
			wrapper = GetWrapper(entryInstruction);
			var locationContactPerson = wrapper.LocationContactPerson;
			AssertNotNull("Expected filled LocationContactPerson", locationContactPerson);
			AssertSame("Cached LocationContactPerson", wrapper.LocationContactPerson, locationContactPerson);

			goodsLocation.CGL_Qualifier = "Y";
			wrapper = GetWrapper(entryInstruction);
			AssertNull("Expected empty LocationContactPerson when qualifier is Y (even if the fields are not empty)", wrapper.LocationContactPerson);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryInstruction = Factory.New<CusEntryInstruction>();
		goodsLocation = entryInstruction.GoodsLocation;

		wrapper = GetWrapper(entryInstruction);
	}
	CusEntryInstruction entryInstruction;
	EU.Business.CusGoodsLocation goodsLocation;
	AESCommonLocationOfGoodsWrapper wrapper;

	AESCommonLocationOfGoodsWrapper GetWrapper(CusEntryInstruction entryInstruction) => new AESCommonLocationOfGoodsWrapper(entryInstruction);

	protected override AESCommonLocationOfGoodsWrapper GetProvider() => wrapper;
}
