using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	public class DummyDocumentWrapper : DocumentWrapper
	{
		public DummyDocumentWrapper(BusinessObject bizToWrap, BusinessObjectFactory factoryToWrap)
			: base(bizToWrap, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return base.WrappedObject.ToString();
		}

		protected override DocWrapperCopyInfo AdditionalCopyInfo
		{
			get { return additionalCopyInfo; }
		}
		DocWrapperCopyInfo additionalCopyInfo;

		public void SetAdditionalCopyInfoForTesting(DocWrapperCopyInfo value)
		{
			additionalCopyInfo = value;
		}

		public ZString HBL
		{
			get { return "SEA"; }
		}

		public ZString JobNumber { get; set; }

		public ZInt Int32Number { get; set; }
		public DummyDocumentWrapper RelatedDummyWrapper { get; set; }
		public DummyCollectionDocumentWrapper CollectionWrapper { get; set; }
	}
}
