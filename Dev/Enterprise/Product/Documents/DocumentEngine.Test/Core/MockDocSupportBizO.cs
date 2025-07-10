using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	[BusinessObjectTestExclude()]
	public class MockDocSupportBizO : NonPersistentBusinessObject, IDocumentSupportable
	{
		public MockDocSupportBizO()
		{
		}

		public MockDocSupportBizO(DocumentSupporter docSupporter)
		{
			fDocumentSupporter = docSupporter;
		}

		public MockDocSupportBizO(OrgHeader org)
		{
			fOrganisation = org;
		}

		public MockDocSupportBizO(OrgHeader org1, OrgHeader org2)
		{
			fOrganisation = org1;
			fRelatedParty = org2;
		}

		public short CopyCount = 42;

		public string Description;

		public DocumentWrapper[] Wrappers
		{
			get
			{
				if (fWrappers == null)
				{
					fWrappers = new DocumentWrapper[] { new DummyDocumentWrapper(this, Factory) };
				}
				return fWrappers;
			}
			set
			{
				fWrappers = value;
				fDocumentSupporter = null;
			}
		}

		DocumentWrapper[] fWrappers;

		public OrgHeader Organisation
		{
			get { return fOrganisation; }
		}

		public OrgHeader RelatedParty
		{
			get { return fRelatedParty; }
		}

		public DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new MockDocSupportBizODocumentSupporter(this, Wrappers, CopyCount);
				}
				return fDocumentSupporter;
			}
		}

		public ZString HBL
		{
			get { return "SEA"; }
		}

		readonly OrgHeader fOrganisation;
		readonly OrgHeader fRelatedParty;
		DocumentSupporter fDocumentSupporter;
	}
}
