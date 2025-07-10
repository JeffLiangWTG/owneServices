using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(OwnerOfGoods))]
	sealed class OwnerOfGoodsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Owner Of Goods", ownerOfGoods.HumanReadableName);
		}

		public void TestE2_AddressSequence()
		{
			var ownerOfGoods2 = instruction.OwnerOfGoodsCollection.AddNew();
			var ownerOfGoods3 = instruction.OwnerOfGoodsCollection.AddNew();
			var ownerOfGoods4 = instruction.OwnerOfGoodsCollection.AddNew();
			AssertEquals((ZByte)0, ownerOfGoods.E2_AddressSequence);
			AssertEquals((ZByte)1, ownerOfGoods2.E2_AddressSequence);
			AssertEquals((ZByte)2, ownerOfGoods3.E2_AddressSequence);
			AssertEquals((ZByte)3, ownerOfGoods4.E2_AddressSequence);

			instruction.OwnerOfGoodsCollection.RemoveAndDelete(ownerOfGoods3);
			AssertEquals((ZByte)0, ownerOfGoods.E2_AddressSequence);
			AssertEquals((ZByte)1, ownerOfGoods2.E2_AddressSequence);
			AssertEquals((ZByte)2, ownerOfGoods4.E2_AddressSequence);

			var place5 = instruction.OwnerOfGoodsCollection.AddNew();
			AssertEquals((ZByte)3, place5.E2_AddressSequence);
		}

		public void TestISequenceNumberLine()
		{
			CombineAssertions(() =>
			{
				var sequenceLine = (IShortSequenceNumberLine)ownerOfGoods;
				AssertEquals("FKToHeader", instruction.PK, sequenceLine.FKToHeader);
				AssertEquals("SequenceNumber", new ZShort(0), sequenceLine.SequenceNumber);
			});
		}

		public void TestOwnersOfGoodsJobDocAddressValidation()
		{
			AssertType<OwnerOfGoodsValidation>(ownerOfGoods.Validation);
		}

		public void TestOrganisationPK_Caption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(OwnerOfGoods), nameof(ownerOfGoods.OrganisationPK));
			AssertEquals("Organization", resData.Caption);
			AssertEquals(string.Empty, resData.FullDescription);
			resData = DataBoundResourceStrings.GetDataForProperty(typeof(OwnerOfGoods), nameof(ownerOfGoods.OrganisationPK), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Organization", resData.Caption);
			AssertEquals("[Annex A 3/8] Parties > Owner of the Goods > Organization", resData.FullDescription);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				ownerOfGoods.OrganisationPKInfo,
				multipleResourceKey: string.Empty,
				caption: "Organization",
				fullDescription: string.Empty
			);
		}

		public void TestE2_OA_Address_Caption()
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(typeof(OwnerOfGoods), nameof(ownerOfGoods.E2_OA_Address));
			AssertEquals("Address", resData.Caption);
			AssertEquals(string.Empty, resData.FullDescription);
			resData = DataBoundResourceStrings.GetDataForProperty(typeof(OwnerOfGoods), nameof(ownerOfGoods.E2_OA_Address), new[] { JobDeclaration.CaptionKeyImportUCC6 });
			AssertEquals("Address", resData.Caption);
			AssertEquals("[Annex A 3/8] Parties > Owner of the Goods > Address", resData.FullDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			ownerOfGoods = instruction.OwnerOfGoodsCollection.AddNew();
		}

		CusEntryInstruction instruction;
		OwnerOfGoods ownerOfGoods;
	}
}
