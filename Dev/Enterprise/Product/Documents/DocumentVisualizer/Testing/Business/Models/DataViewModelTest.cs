using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(DataViewModel))]
	sealed class DataViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var document = new DummyDocument
			{
				Data = shipment.MakeDynamic()
			};

			return new DataViewModel(document, DataViewModel.DataType.Messaging);
		}
	}
}
