using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocARComplianceDocumentLineCollection : DocumentWrapperCollection
	{
		public DocARComplianceDocumentLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new DocARComplianceDocumentLine this[int index]
		{
			get { return (DocARComplianceDocumentLine)base[index]; }
		}
	}
}
