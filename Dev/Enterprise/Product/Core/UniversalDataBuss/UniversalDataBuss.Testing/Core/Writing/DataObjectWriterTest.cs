using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing.Core.Writing
{
	public class DataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCustomFieldsNoExceptionThrown()
		{
			var shipment = Factory.New<DummyBusinessObject>();
			Factory.Save();
			var writer = new DataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)));
			AssertNoExceptionThrown(() => writer.GetDataObject(shipment));
		}

		public void TestExportNotified()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var infoCollector = new InformationCollectorForTesting();
			var writer = new DataObjectWriterForTesting(new DataWritingManager(new ActionInfo(null, parent), infoCollector));
			var result = writer.GetDataObject(parent);

			AssertEquals(result, infoCollector.DataObject);
			AssertEquals(parent, infoCollector.BizO);
		}

		public void TestReplaceOverFlowExceptionWithDataObjectValidationException()
		{
			var shipment = Factory.New<DummyBusinessObject>();
			Factory.Save();
			var writer = new DataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)));

			AssertNoExceptionThrown(() =>
				writer.ReplaceOverFlowExceptionWithDataObjectValidationException_Exposed("When no exception is thrown.", () => shipment.Z0_Number = 10));

			AssertExceptionThrown(
			   "Correct Exception thrown.",
				typeof(DataObjectValidationException),
				() => writer.ReplaceOverFlowExceptionWithDataObjectValidationException_Exposed("When overflow exception is thrown.", () => throw new OverflowException("Bla")));

			AssertExceptionThrown(
			   "Correct Exception thrown.",
				typeof(ArgumentException),
				() => writer.ReplaceOverFlowExceptionWithDataObjectValidationException_Exposed("When some other exception is thrown.", () => throw new ArgumentException("Bla")));
		}
	}

	class DataObjectWriterForTesting : DataObjectWriter<DummyBusinessObject, Shipment>
	{
		public DataObjectWriterForTesting(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Shipment PopulateDataObject(DummyBusinessObject sourceBO)
		{
			var ship = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var customField = new CustomizedField();
			customField.Key = "ABC";
			customField.Value = "TEST";
			ship.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			ship.CustomizedFieldCollection.Add(customField);
			return ship;
		}

		protected override IEnumerable<IPropertyValue> GetAllCustomPropertiesWithDefaultValue(DummyBusinessObject bo)
		{
			var list = new List<IPropertyValue>();
			list.Add(new PropertyValue("TEST", new ZString("TEST")));
			return list;
		}

		public void ReplaceOverFlowExceptionWithDataObjectValidationException_Exposed(string message, Action actions)
		{
			ReplaceOverFlowExceptionWithDataObjectValidationException(message, actions);
		}
	}

	class InformationCollectorForTesting : IDataWritingInformationCollector
	{
		void IDataWritingInformationCollector.NotifyExported(IDataObject dataObject, BusinessObject businessObject)
		{
			this.dataObject = dataObject;
			this.bizO = businessObject;
		}

		IDataObject dataObject;
		BusinessObject bizO;

		public IDataObject DataObject
		{
			get { return dataObject; }
		}

		public BusinessObject BizO
		{
			get { return bizO; }
		}
	}
}
