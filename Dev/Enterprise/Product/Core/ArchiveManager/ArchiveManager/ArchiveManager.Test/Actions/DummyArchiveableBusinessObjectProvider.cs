using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.Actions
{
	class DummyArchiveableBusinessObjectProvider : IArchiveableBusinessObjectProvider
	{
		#region IArchiveableBusinessObjectProvider Members

		public IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory)
			=> item.PKColumn.TableName == AutoDummyBizo.Schema.TableName
				? (new IArchiveableBusinessObject[] { factory.Load<DummyWithDocumentSupport>(item.PK) })
				: (new IArchiveableBusinessObject[] { null });

		public IEnumerable<string> TableNamesSupported
		{
			get { yield return AutoDummyBizo.Schema.TableName; }
		}

		public IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get { yield return DummyWithDocumentSupport.DummyCode; }
		}

		#endregion
	}
}
