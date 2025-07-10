using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CustomsManifestLineSequence))]
	public class CustomsManifestLineSequenceTest : CusCodeDataTest<CustomsManifestLineSequence>
	{
		public void TestICustomsManifestLineSequenceIsCorrectlySetup()
		{
			AssertEquals(typeof(CustomsManifestLineSequence), ObjectFactory.GetType<Integration.Customs.AU.ICustomsManifestLineSequence>());
		}

		public void TestSetDefaultValues()
		{
			CustomsManifestLineSequence customsManifestLineSequence = Factory.New<CustomsManifestLineSequence>();
			AssertEquals(CusCodeDataTypeList.Codes.CustomsManifestLineSequence, customsManifestLineSequence.CY_Type);
		}

		public void TestIsPreliminaryNumberType()
		{
			var customsManifestLineSequence = Factory.New<CustomsManifestLineSequence>();
			customsManifestLineSequence.CY_Code = Enterprise.Customs.AU.Declaration.Business.CustomsManifestLineSequence.DataCodes.PreliminaryLineNumber;
			AssertEquals("IsPreliminaryNumberType", true, customsManifestLineSequence.IsPreliminaryNumberType);

			customsManifestLineSequence.UpdatePreliminaryLineNumberToManifestedLineNumber();
			AssertEquals("IsPreliminaryNumberType", false, customsManifestLineSequence.IsPreliminaryNumberType);
		}

		public void TestUpdatePreliminaryLineNumberToManifestedLineNumber()
		{
			var customsManifestLineSequence = Factory.New<CustomsManifestLineSequence>();
			customsManifestLineSequence.CY_Code = Enterprise.Customs.AU.Declaration.Business.CustomsManifestLineSequence.DataCodes.PreliminaryLineNumber;
			AssertEquals("IsPreliminaryNumberType", true, customsManifestLineSequence.IsPreliminaryNumberType);

			customsManifestLineSequence.UpdatePreliminaryLineNumberToManifestedLineNumber();
			AssertEquals("Should be Manifested Line Number", "MLN", customsManifestLineSequence.CY_Code);
		}

		public void TestIsPreliminaryDeletedType()
		{
			var customsManifestLineSequence = Factory.New<CustomsManifestLineSequence>();
			customsManifestLineSequence.CY_Code = Enterprise.Customs.AU.Declaration.Business.CustomsManifestLineSequence.DataCodes.PreliminaryDeletedLine;
			AssertEquals("IsPreliminaryDeletedLine", true, customsManifestLineSequence.IsPreliminaryDeletedLine);

			customsManifestLineSequence.UpdatePreliminaryDeletedLineNumberToDeletedLine();
			AssertEquals("IsPreliminaryDeletedLine", false, customsManifestLineSequence.IsPreliminaryNumberType);
		}

		public void TestUpdatePreliminaryDeletedLineNumberToDeletedLineNumber()
		{
			var customsManifestLineSequence = Factory.New<CustomsManifestLineSequence>();
			customsManifestLineSequence.CY_Code = Enterprise.Customs.AU.Declaration.Business.CustomsManifestLineSequence.DataCodes.PreliminaryDeletedLine;
			AssertEquals("PreliminaryDeletedLine", true, customsManifestLineSequence.IsPreliminaryDeletedLine);

			customsManifestLineSequence.UpdatePreliminaryDeletedLineNumberToDeletedLine();
			AssertEquals("Should be Manifested Deleted Line Now", "DLN", customsManifestLineSequence.CY_Code);
		}

		public void TestNoNotificationOnCorrectCode()
		{
			var customsManifestLineSequence = Factory.New<CustomsManifestLineSequence>();
			customsManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.PreliminaryDeletedLine;
			customsManifestLineSequence.Validation.ValidateAll();
			AssertNoNotifications(customsManifestLineSequence);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper.LineSequenceCollection.AddNew();
		}

		protected override IEnumerable<CustomsManifestLineSequence> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<CommonShipment>();
			var wrapper = new FreightShipmentWrapper(shipment, null);
			yield return wrapper.LineSequenceCollection.AddNew();

			var hvlvBookingHeader = (IHVLVBookingHeader)factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVBookingHeader>());
			var hvlvConsignment = hvlvBookingHeader.Consignments.AddNew();
			wrapper = new FreightShipmentWrapper(shipment, null, hvlvConsignment as IEManifestLine);
			yield return wrapper.LineSequenceCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<CommonShipment>();
			var wrapper = new FreightShipmentWrapper(shipment, null);
			return wrapper.LineSequenceCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var shipment = Factory.New<CommonShipment>();
			var wrapper = new FreightShipmentWrapper(shipment, null);
			return wrapper.LineSequenceCollection.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var shipment = Factory.New<CommonShipment>();
			wrapper = new FreightShipmentWrapper(shipment, null);
		}
		protected FreightShipmentWrapper wrapper;
	}
}
