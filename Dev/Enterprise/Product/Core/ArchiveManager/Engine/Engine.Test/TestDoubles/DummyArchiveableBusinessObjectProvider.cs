using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	public class DummyArchiveableBusinessObjectProvider : IArchiveableBusinessObjectProvider
	{
		#region IArchiveableBusinessObjectProvider Members

		public IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory)
		{
			return item.PKColumn.TableName == AutoDummyBizo.Schema.TableName
				? (new IArchiveableBusinessObject[] { factory.Load<DummyArchiveableBusinessObject>(item.PK) })
				: (new IArchiveableBusinessObject[] { null });
		}

		public IEnumerable<string> TableNamesSupported
		{
			get { yield return AutoDummyBizo.Schema.TableName; }
		}

		public IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get
			{
				yield return DummyArchiveableBusinessObject.DummyCode;
			}
		}

		#endregion

		class DummyArchiveableBusinessObject : DummyBusinessObject, IArchiveableBusinessObject
		{
			public DummyArchiveableBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public static readonly ReferenceKeyType DummyCode = new ReferenceKeyType("Code", (NoResString)"Dummy Code");

			#region IArchiveableBusinessObject Members

			public IEnumerable<ArchiveReferenceKey> AdditionalKeys
				=> null;

			public BusinessObject ArchiveableBusinessObject
				=> this;

			public ArchiveReferenceKey NaturalKey
				=> new ArchiveReferenceKey(DummyCode, Z0_Code);

			public IEnumerable<ArchiveDocumentDescriptor> ArchiveDocuments
				=> throw new NotImplementedException();

			public Guid BranchPK
				=> Guid.Empty;

			#endregion
		}
	}
}
