using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business
{
	public interface IELearningDocumentDescription
	{
		Guid MyAccountDocumentId { get; set; }
		string Title { get; set; }
		string Url { get; set; }
		string DocumentType { get; set; }
		ZDateTime DocumentLastModified { get; set; }
	}

	public class ELearningDocumentDescription : AutoELearningDocumentDescription, IELearningDocumentDescription
	{
		public ELearningDocumentDescription(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public Guid MyAccountDocumentId { get; set; }

		public string Title
		{
			get { return base.ELD_Title; }
			set { base.ELD_Title = value; }
		}

		public string Url
		{
			get { return base.ELD_Url; }
			set { base.ELD_Url = value; }
		}

		public string DocumentType
		{
			get { return base.ELD_DocumentType; }
			set { base.ELD_DocumentType = value; }
		}

		public ZDateTime DocumentLastModified
		{
			get { return base.ELD_DocumentLastModified; }
			set { base.ELD_DocumentLastModified = value; }
		}
	}
}
