using System.Linq;
using CargoWise.Customs.IN.MessageContracts.SeaCgm;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm.Testing;

[TestedType(typeof(SeaCgmCMCHI21DataProvider))]
sealed class ConsolContDataProviderTest : SeaCgmIConsolContDataProviderBase
{
	public override void TestCarnNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestCarnNumber(() => dataProvider.CarnNumber);
	}

	public override void TestContainerAgentCode()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		container.ContainerAgentCodeOrgPK = orgHeader.PK;
		container.ContainerAgentCode = orgHeader.MainAddress.PK;

		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNullOrEmpty($"{nameof(IConsolContDataProvider.ContainerAgentCode)} is empty", dataProvider.ContainerAgentCode);

			CreateCusCodeForType(IndiaOrgCusCodeInfo.OrgCusCodes.PAN);
			CreateCusCodeForType(IndiaOrgCusCodeInfo.OrgCusCodes.TAN);
			dataProvider = CreateDataProvider();
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerAgentCode)} is set", "PAN_1234", dataProvider.ContainerAgentCode);
		});

		void CreateCusCodeForType(string codeType)
		{
			var code = orgHeader.CustomsCodes.AddNew();
			code.OK_CodeType = codeType;
			code.OK_RN_NKCodeCountry = CountryCodes.India;
			code.OK_CustomsRegNo = codeType + "_1234";
		}
	}

	public override void TestContainerNumber()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerNumber)} is empty", string.Empty, dataProvider.ContainerNumber);

			dataProvider = CreateDataProvider();
			container.ACN_ContainerNumber = "ABC123";
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerNumber)} is set", "ABC123", dataProvider.ContainerNumber);
		});
	}

	public override void TestContainerSealNo()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerSealNo)} is empty", string.Empty, dataProvider.ContainerSealNo);

			dataProvider = CreateDataProvider();
			container.ACN_Seal1 = "001";
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerSealNo)} is set", "001", dataProvider.ContainerSealNo);
		});
	}

	public override void TestContainerStatus()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerStatus)} is empty", string.Empty, dataProvider.ContainerStatus);

			dataProvider = CreateDataProvider();
			container.ACN_EmptyFullIndicator = "ABC";
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerStatus)} is set", "ABC", dataProvider.ContainerStatus);
		});
	}

	public override void TestContainerWeight()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerWeight)} is empty as no weight is set", ZDecimal.Zero, dataProvider.ContainerWeight);

			dataProvider = CreateDataProvider();
			container.ACN_GoodsWeight = 1234m;
			container.ACN_GoodsWeightUQ = "XYZ";
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerWeight)} is empty as invalid weight unit is set", ZDecimal.Zero, dataProvider.ContainerWeight);

			dataProvider = CreateDataProvider();
			container.ACN_GoodsWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals($"{nameof(IConsolContDataProvider.ContainerWeight)} is set in Tonnes", 1.234m, dataProvider.ContainerWeight);
		});
	}

	public override void TestCustomHouseCode()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestCustomHouseCode(header, () => dataProvider.CustomHouseCode);
	}

	public override void TestIgmDate()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestIgmDate(header, () => dataProvider.IgmDate);
	}

	public override void TestIgmNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestIgmNumber(header, () => dataProvider.IgmNumber);
	}

	public override void TestImoCodeOfVessel()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestImoCodeOfVessel(header, () => dataProvider.ImoCodeOfVessel);
	}

	public override void TestIsoCode()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNullOrEmpty($"{nameof(IConsolContDataProvider.IsoCode)} is empty", dataProvider.IsoCode);

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "XYZ";
			Factory.Save();
			dataProvider = CreateDataProvider();
			container.ACN_RC_ContainerType = refContainer.PK;
			AssertEquals($"{nameof(IConsolContDataProvider.IsoCode)} is set", "XYZ", dataProvider.IsoCode);
		});
	}

	public override void TestLineNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestLineNumber(header, () => dataProvider.LineNumber);
	}

	public override void TestMessageType()
	{
		AdditionalDataProviderMock.Setup(x => x.GetMessageType(It.IsAny<CGMAsycudaPack>())).Returns("S");
		AssertEquals("When message type is S", "S", CreateDataProvider().MessageType);
	}
	public override void TestSocFlagYn()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals($"{nameof(IConsolContDataProvider.SocFlagYn)} is false", ZBool.False, dataProvider.SocFlagYn);

			dataProvider = CreateDataProvider();
			container.ACN_IsShipperOwned = true;
			AssertEquals($"{nameof(IConsolContDataProvider.SocFlagYn)} is true", ZBool.True, dataProvider.SocFlagYn);
		});
	}

	public override void TestSubLineNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestSubLineNumber(bill, () => dataProvider.SubLineNumber);
	}

	public override void TestTotalPackages()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals($"{nameof(IConsolContDataProvider.TotalPackages)} is empty", 0, dataProvider.TotalPackages);

			dataProvider = CreateDataProvider();
			container.ACN_NumberOfPackages = 5;
			AssertEquals($"{nameof(IConsolContDataProvider.TotalPackages)} is set", 5, dataProvider.TotalPackages);
		});
	}

	public override void TestVesselCode()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestVesselCode(header, () => dataProvider.VesselCode);
	}

	public override void TestVoyageNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestVoyageNumber(header, () => dataProvider.VoyageNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		bill = header.Bills.AddNew();
		var pack = bill.Packs.AddNew();
		container = header.Containers.AddNew();
		pack.ContainerPK = container.PK;
	}

	protected override IConsolContDataProvider CreateDataProvider()
		=> SeaCgmCMCHI21DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Consoligm.Containers.First();

	Mock<ISeaCgmCMCHI21AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new();
	Mock<ISeaCgmCMCHI21AdditionalDataProvider> additionalDataProviderMock;

	ConsolDataProviderTestHelper DataProviderTestHelper => dataProviderTestHelper ??= new();
	ConsolDataProviderTestHelper dataProviderTestHelper;

	CGMAsycudaBill bill;
	CGMAsycudaContainer container;
}
