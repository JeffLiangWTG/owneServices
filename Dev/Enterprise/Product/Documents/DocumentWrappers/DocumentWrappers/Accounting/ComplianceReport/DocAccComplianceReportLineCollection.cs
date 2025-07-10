using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocAccComplianceReportLineCollection : DocumentWrapperCollection
	{
		public DocAccComplianceReportLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocAccComplianceReportLine this[int index]
		{
			get { return (DocAccComplianceReportLine)base[index]; }
		}
	}
}
