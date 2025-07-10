using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business
{
	public interface IELearningDocumentTfIdf
	{
		ZDateTime DocumentLastModified { get; set; }
		ZGuid ELearningDocumentDescriptionRefKey { get; set; }
		ZBlob Tf { get; set; }
		ZBlob TfIdf { get; set; }
	}
	public class ELearningDocumentTfIdf : AutoELearningDocumentTfIdf, IELearningDocumentTfIdf
	{
		public ELearningDocumentTfIdf(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZDateTime DocumentLastModified
		{
			get { return base.EDT_DocumentLastModified; }
			set { base.EDT_DocumentLastModified = value; }
		}

		public ZGuid ELearningDocumentDescriptionRefKey
		{
			get { return base.EDT_ELD; }
			set { base.EDT_ELD = value; }
		}

		public ZBlob Tf
		{
			get { return base.EDT_TF; }
			set { base.EDT_TF = value; }
		}

		public ZBlob TfIdf
		{
			get { return base.EDT_TFIDF; }
			set { base.EDT_TFIDF = value; }
		}
	}
}
