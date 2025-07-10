using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyWithUXmlSupportDataWriter : ITopLevelDataObjectWriter
	{
		public ZString EDIMessageSubType
		{
			get { return "XXX"; }
		}

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var dummy = (DummyWithUXmlSupport)sourceBO;

			var dataObject = GetDataObject(dummy);

			if (dummy.Collection.Any())
			{
				dataObject.DummyCollection = new List<DummyWithUXmlSupportDataObject>();

				foreach (var child in dummy.Collection)
				{
					var childDataObject = GetDataObject(child);
					dataObject.DummyCollection.Add(childDataObject);
				}
			}

			return dataObject;
		}

		DummyWithUXmlSupportDataObject GetDataObject(DummyBusinessObject dummy)
		{
			return new DummyWithUXmlSupportDataObject
			{
				Code = dummy.Z0_Code,
				Description = dummy.Z0_Description
			};
		}

		public ZString RootElementName
		{
			get { return "Dummy"; }
		}

		public DataContextType TopLevelDataContextType
		{
			get { return DataContextType.DummyBusinessObject; }
		}

		public void SetDataWritingManager(IDataWritingManager dataWritingManager)
		{
		}
	}
}