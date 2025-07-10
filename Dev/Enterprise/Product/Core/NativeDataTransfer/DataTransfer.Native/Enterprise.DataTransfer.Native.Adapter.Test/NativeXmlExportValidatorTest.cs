using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeXmlExportValidatorTest : TransactionedTestCase
	{
		public void TestCanBeExported()
		{
			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName("OrgHeader")).Returns(true);

			var result = validator.CanBeExported(typeof(OrgHeader));
			AssertEquals("Validator should return true if there is definition for business object", true, result);
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName("OrgHeader"), Times.Once);
		}

		public void TestCanBeExported_JobDeclaration()
		{
			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			
			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName("JobDeclaration")).Returns(true);
			var declarationType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			AssertEquals(true, validator.CanBeExported(declarationType));

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();
			AssertEquals(false, validator.CanBeExported(declarationType));
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName("JobDeclaration"), Times.Once);
		}

		public void TestCanBeExported_JobShipment()
		{
			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();

			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName("JobShipment")).Returns(true);
			var shipmentType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
			AssertEquals(true, validator.CanBeExported(shipmentType));

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();
			AssertEquals(false, validator.CanBeExported(shipmentType));
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName("JobShipment"), Times.Once);
		}

		public void TestCanBeExported_JobOrderHeader()
		{
			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			
			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName("JobOrderHeader")).Returns(true);
			var orderType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IOrder>();
			AssertEquals(true, validator.CanBeExported(orderType));

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();
			AssertEquals(false, validator.CanBeExported(orderType));
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName("JobOrderHeader"), Times.Once);
		}

		public void TestCanBeExported_NoDefinitionFound()
		{
			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName("OrgHeader")).Returns(false);
			var result = validator.CanBeExported(typeof(OrgHeader));
			AssertEquals(
				"Validator should return false if there is not definition for business object",
				false,
				result);
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName("OrgHeader"), Times.Once);
		}

		public void TestCanBeExported_InvalidType()
		{
			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName(null)).Returns(false);
			var result = validator.CanBeExported(typeof(NativeXmlExportValidator));
			AssertEquals("Validator should return false", false, result);
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName(null), Times.Once);
		}

		public void TestCanBeExported_CompanyTariff()
		{
			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName("RatingHeader")).Returns(true);
			var result = validator.CanBeExported(typeof(CompanyTariff));
			AssertEquals("Validator should return true", true, result);
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName("RatingHeader"), Times.Once);
		}

		public void TestTransportZoneSetCanBeExported()
		{
			definitionFinder.Setup(x => x.HasDefinitionWithTopTableName("RateTransportProvider")).Returns(true);
			var result = validator.CanBeExported(typeof(RateTransportProvider));
			AssertEquals("Validator should return true if there is definition for business object", true, result);
			definitionFinder.Verify(x => x.HasDefinitionWithTopTableName("RateTransportProvider"), Times.Once);
		}

		public void TestValidate()
		{
			AssertExceptionThrown(
				"Validator should throw error when there is no business object to export",
				typeof(NativeXMLUserVisibleException),
				() => validator.Validate(Array.Empty<IBusiness>())
				);
			businessObject.Setup(x => x.HasChanges).Returns(true);
			AssertExceptionThrown(
				"Validator should throw error when business object has been changed and not saved",
				typeof(NativeXMLUserVisibleException),
				() => validator.Validate(new[] { businessObject.Object })
				);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			mocks = new MockRepository(MockBehavior.Default);
			businessObject = mocks.Create<IBusiness>();
			definitionFinder = mocks.Create<IDefinitionFinder>();
			validator = new NativeXmlExportValidator { DefinitionFinder = definitionFinder.Object };
		}

		Mock<IBusiness> businessObject;
		Mock<IDefinitionFinder> definitionFinder;
		NativeXmlExportValidator validator;
		MockRepository mocks;

		#endregion
	}
}
