using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLDetail))]
	public class CusSeaManOBLDetailTest : BaseCusSeaManOBLDetailTest
	{
		public void TestIsForAirCargo()
		{
			ICusUnderbondUnionCollectionParent detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(false, detail.IsForAirCargo);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			ICusUnderbondDependentCollectionParent detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals(false, detail.UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, detail.DefaultTranshipmentPort);
		}

		public void TestLookups()
		{
			var detail = Factory.New<CusSeaManOBLDetail>();
			AssertType<BaseCusSeaManOBLDetailLookups>(detail.Lookups);

			AssertType<CMRImportCargoTypes>(detail.Lookups.CargoTypes);
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeaManOBLDetail), CusSeaManOBLDetail.Schema.BD_LineCargoType, true, attrib => attrib.ListDataSourceMember == "Lookups.CargoTypes");

			AssertType<CMRContainerSizes>(detail.Lookups.ContainerSizes);
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeaManOBLDetail), CusSeaManOBLDetail.Schema.BD_ContainerSizeOrISOCode, true, attrib => attrib.ListDataSourceMember == "Lookups.ContainerSizes");

			AssertType<CMRContainerTypes>(detail.Lookups.TypesOfContainers);

			AssertType<RefContainerCollection>(detail.Lookups.ContainerTypes);
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeaManOBLDetail), CusSeaManOBLDetail.Schema.BD_RC_ContainerType, true, attrib => attrib.ListDataSourceMember == "Lookups.ContainerTypes");

			AssertType<CMRQuantityUnits>(detail.Lookups.QuantityUnits);
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeaManOBLDetail), CusSeaManOBLDetail.Schema.BD_CargoVolumeUM, true, attrib => attrib.ListDataSourceMember == "Lookups.QuantityUnits");

			AssertType<CMRGrossWeightCodes>(detail.Lookups.GrossWeightCodes);
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeaManOBLDetail), CusSeaManOBLDetail.Schema.BD_GrossWeightUM, true, attrib => attrib.ListDataSourceMember == "Lookups.GrossWeightCodes");

			AssertType<CMRPackageTypes>(detail.Lookups.PackageTypes);
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeaManOBLDetail), CusSeaManOBLDetail.Schema.BD_PackType, true, attrib => attrib.ListDataSourceMember == "Lookups.PackageTypes");
		}

		public void TestValidation()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Expected overridden class", typeof(CusSeaManOBLDetailValidation), detail.Validation.GetType());
		}

		public void TestUnderbonds()
		{
			AssertNotNull(DetailUnder.Underbonds);
			Assert(detail.IsRegisteredEditableChildObject(DetailUnder.Underbonds));
		}

		public void TestUnderbondHumanReadableName()
		{
			CheckUnderbondHumanReadableName("Container", ZString.Empty, CMRImportCargoTypes.Codes.FullContainerLoad, ZString.Empty);
			CheckUnderbondHumanReadableName("OBL100 - Container", "OBL100", CMRImportCargoTypes.Codes.FullContainerLoad, ZString.Empty);
			CheckUnderbondHumanReadableName("Container: 123", ZString.Empty, CMRImportCargoTypes.Codes.FullContainerLoad, "123");
			CheckUnderbondHumanReadableName("OBL939 - Container: 99949", "OBL939", CMRImportCargoTypes.Codes.FullContainerLoad, "99949");

			CheckUnderbondHumanReadableName("Container", ZString.Empty, CMRImportCargoTypes.Codes.LessThanContainerLoad, ZString.Empty);
			CheckUnderbondHumanReadableName("OBL100 - Container", "OBL100", CMRImportCargoTypes.Codes.LessThanContainerLoad, ZString.Empty);
			CheckUnderbondHumanReadableName("Container: 123", ZString.Empty, CMRImportCargoTypes.Codes.LessThanContainerLoad, "123");
			CheckUnderbondHumanReadableName("OBL939 - Container: 99949", "OBL939", CMRImportCargoTypes.Codes.LessThanContainerLoad, "99949");

			CheckUnderbondHumanReadableName("Container", ZString.Empty, CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, ZString.Empty);
			CheckUnderbondHumanReadableName("OBL100 - Container", "OBL100", CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, ZString.Empty);
			CheckUnderbondHumanReadableName("Container: 123", ZString.Empty, CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, "123");
			CheckUnderbondHumanReadableName("OBL939 - Container: 99949", "OBL939", CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills, "99949");

			CheckUnderbondHumanReadableName("Container", ZString.Empty, ZString.Empty, ZString.Empty);
			CheckUnderbondHumanReadableName("OBL100 - Container", "OBL100", ZString.Empty, ZString.Empty);
			CheckUnderbondHumanReadableName("Container: 123", ZString.Empty, ZString.Empty, "123");
			CheckUnderbondHumanReadableName("OBL939 - Container: 99949", "OBL939", ZString.Empty, "99949");

			CheckUnderbondHumanReadableName("Break Bulk", ZString.Empty, CMRImportCargoTypes.Codes.BreakBulk, ZString.Empty);
			CheckUnderbondHumanReadableName("OBL323 - Break Bulk", "OBL323", CMRImportCargoTypes.Codes.BreakBulk, ZString.Empty);
			CheckUnderbondHumanReadableName("Break Bulk", ZString.Empty, CMRImportCargoTypes.Codes.BreakBulk, "HARHAR");
			CheckUnderbondHumanReadableName("OBL999 - Break Bulk", "OBL999", CMRImportCargoTypes.Codes.BreakBulk, "MANGMANG");

			CheckUnderbondHumanReadableName("Bulk", ZString.Empty, CMRImportCargoTypes.Codes.Bulk, ZString.Empty);
			CheckUnderbondHumanReadableName("OBL323 - Bulk", "OBL323", CMRImportCargoTypes.Codes.Bulk, ZString.Empty);
			CheckUnderbondHumanReadableName("Bulk", ZString.Empty, CMRImportCargoTypes.Codes.Bulk, "HARHAR");
			CheckUnderbondHumanReadableName("OBL999 - Bulk", "OBL999", CMRImportCargoTypes.Codes.Bulk, "MANGMANG");
		}

		void CheckUnderbondHumanReadableName(ZString expectedUnderbondHumanReadableName, ZString oceanBillNumber, ZString lineCargoType, ZString containerNumber)
		{
			header.BO_OceanBill = oceanBillNumber;
			detail.BD_LineCargoType = lineCargoType;
			detail.BD_ContainerNumber = containerNumber;
			AssertEquals(expectedUnderbondHumanReadableName, DetailUnder.UnderbondHumanReadableName);
		}

		public void TestOutturnableLines()
		{
			AssertEquals("Length", 0, ((ICusUnderbondDependentCollectionParent)detail).OutturnableLines.Length);
		}

		public void TestSetDefaultValues()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Default value for BD_CargoVolumeUM should be CMRQuantityTypes.Codes.CubicMetre", CMRQuantityUnits.Codes.CubicMetre, detail.BD_CargoVolumeUM);
		}

		public void TestCanSendWithoutDelay()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			CusSeaManOBLDetail detail = header.Details.AddNew();
			Assert("Should be delayed because No CARSTS were found", !((ICusUnderbondDependentCollectionParent)detail).CanSendWithoutDelay);
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			header.Messages.Add(message);
			Assert("Should not be delayed due to carst.", ((ICusUnderbondDependentCollectionParent)detail).CanSendWithoutDelay);
		}

		public void TestIsCargoLine()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			CusSeaManOBLDetail detail = header.Details.AddNew();

			header.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Export;
			AssertEquals("when header type export", false, detail.IsCargoListLine);

			header.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Empty;
			AssertEquals("when header type empty", false, detail.IsCargoListLine);

			header.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Cabotage;
			AssertEquals("when header type cabotage", false, detail.IsCargoListLine);

			header.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Import;
			AssertEquals("when header type import", true, detail.IsCargoListLine);
		}

		public void TestContainerNumberBlankAndReadOnlyOnBulkOrBreakBulk()
		{
			detail.BD_ContainerNumber = "123456";
			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;

			AssertEquals("container num when fcl", "123456", detail.BD_ContainerNumber);
			AssertEquals("readonly false when fcl", false, detail.BD_ContainerNumberInfo.ReadOnly);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;

			AssertEquals("container num when break bulk", CMRImportCargoTypes.Descriptions.BreakBulk.ToUpper(), detail.BD_ContainerNumber);
			AssertEquals("readonly false when break bulk", true, detail.BD_ContainerNumberInfo.ReadOnly);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;

			AssertEquals("container num when lcl", ZString.Empty, detail.BD_ContainerNumber);
			AssertEquals("readonly false when lcl", false, detail.BD_ContainerNumberInfo.ReadOnly);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;

			AssertEquals("container num when bulk", CMRImportCargoTypes.Descriptions.Bulk.ToUpper(), detail.BD_ContainerNumber);
			AssertEquals("readonly false when bulk", true, detail.BD_ContainerNumberInfo.ReadOnly);
		}

		public void TestContainerNumberUpperCase()
		{
			detail.BD_ContainerNumber = "foo";
			AssertEquals("upper case", "FOO", detail.BD_ContainerNumber);
		}

		public void TestContainerNumberBlankedOnBulkOrBreakBulk()
		{
			detail.BD_ContainerNumber = "smooooo";
			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertEquals("after bulk", ZString.Empty, detail.BD_ContainerNumber);

			detail.BD_ContainerNumber = "smeeeee";
			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertEquals("after break bulk", ZString.Empty, detail.BD_ContainerNumber);
		}

		public void TestContainerSize()
		{
			detail.BD_ContainerSizeOrISOCode = "0000";
			AssertEquals("when 0000", "0000", detail.BD_ContainerSizeOrISOCode);
			AssertEquals("when 0000", false, detail.BD_ContainerSizeOrISOCodeInfo.ReadOnly);

			detail.BD_RC_ContainerType = GetNewContainer().PK;
			AssertEquals("when containertype set", "2008", detail.BD_ContainerSizeOrISOCode);
			AssertEquals("when containertype set", true, detail.BD_ContainerSizeOrISOCodeInfo.ReadOnly);

			detail.BD_RC_ContainerType = ZGuid.Empty;
			AssertEquals("when containertype cleared", ZString.Empty, detail.BD_ContainerSizeOrISOCode);
			AssertEquals("when containertype cleared", false, detail.BD_ContainerSizeOrISOCodeInfo.ReadOnly);
		}

		public void TestTypeOfContainer()
		{
			detail.BD_TypeOfContainer = CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo;
			AssertEquals("when refr", CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo, detail.BD_TypeOfContainer);
			AssertEquals("when refr", false, detail.BD_TypeOfContainerInfo.ReadOnly);

			detail.BD_TypeOfContainer = CMRContainerTypesMapping.Codes.Otop;
			AssertEquals("when otop", CMRContainerTypes.Codes.OpenTopAContainerWithNoHardTopUsedToTransportCargoThatWouldNotNormallyFitInsideAConventionalContainer, detail.BD_TypeOfContainer);
			AssertEquals("when otop", false, detail.BD_TypeOfContainerInfo.ReadOnly);

			detail.BD_RC_ContainerType = GetNewContainer().PK;
			AssertEquals("when containertype set", CMRContainerTypes.Codes.MafiATypeOfWheeledTrailerOntoWhichCargoIsStrappedForTransportOnAVessel, detail.BD_TypeOfContainer);
			AssertEquals("when containertype set", true, detail.BD_TypeOfContainerInfo.ReadOnly);

			detail.BD_RC_ContainerType = ZGuid.Empty;
			AssertEquals("when containertype cleared", ZString.Empty, detail.BD_TypeOfContainer);
			AssertEquals("when containertype cleared", false, detail.BD_ContainerNumberInfo.ReadOnly);
		}

		public void TestSwitchTypeWhenBulkOrBreakBulkEntered()
		{
			detail.BD_ContainerNumber = "bUlK";
			AssertEquals("when bulk", CMRImportCargoTypes.Codes.Bulk, detail.BD_LineCargoType);

			detail.BD_ContainerNumber = "bReAk bUlK";
			AssertEquals(CMRImportCargoTypes.Codes.BreakBulk, detail.BD_LineCargoType);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusSeaManTranHead header = factory.New<CusSeaManTranHead>();
			return header.OceanBills.AddNew().Details.AddNew();
		}

		protected ICusUnderbondDependentCollectionParent DetailUnder
		{
			get { return detail; }
		}

		RefContainer GetNewContainer()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_Length = 20;
			container.RC_Height = 8;
			container.RC_Width = 8;
			container.RC_ContainerType = Core.Constants.ContainerTypes.MAFI;
			return container;
		}

		protected override void SetUp()
		{
			base.SetUp();

			tranHead = Factory.New<CusSeaManTranHead>();
			header = tranHead.OceanBills.AddNew();
			detail = header.Details.AddNew();
		}

		protected CusSeaManTranHead tranHead;
		protected CusSeaManOBLHeader header;
		protected CusSeaManOBLDetail detail;

		#endregion
	}
}
