using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Test.Actions
{
	class DummyWithDocumentSupport : DummyEnterpriseBusinessObject, IDocManagerSupport, IDocumentSupportable, IArchiveableBusinessObject
	{
		public DummyWithDocumentSupport(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
			DocManagerInfo = new DocManagerInfo(this, "DUM");
			DocumentSupporter = new DocumentSupporterForTest(this);
		}

		public readonly static ReferenceKeyType DummyCode = new("DUM", (NoResString)"Dummy Code");

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get;
			private set;
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get;
			private set;
		}

		#endregion

		#region IArchiveableBusinessObject Members

		public IEnumerable<ArchiveDocumentDescriptor> ArchiveDocuments
		{
			get
			{
				yield return new ArchiveDocumentDescriptor("Archive Pack") { DocTypeToCheck = "MSC" };
			}
		}

		IEnumerable<ArchiveReferenceKey> IArchiveableBusinessObject.AdditionalKeys
		{
			get { yield break; }
		}

		public BusinessObject ArchiveableBusinessObject
			=> this;

		ArchiveReferenceKey IArchiveableBusinessObject.NaturalKey
			=> new(DummyCode, Z0_Code);

		public Guid BranchPK
			=> Guid.Empty;

		#endregion
	}
}
