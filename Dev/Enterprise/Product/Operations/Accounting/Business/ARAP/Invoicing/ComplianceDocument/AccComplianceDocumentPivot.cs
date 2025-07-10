using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentPivot : AutoAccComplianceDocumentPivot
	{
		public AccComplianceDocumentPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("AccComplianceDocumentLine")]
		public override ZGuid ADP_ADL { get => base.ADP_ADL; set => base.ADP_ADL = value; }

		public virtual AccComplianceDocumentLine AccComplianceDocumentLine
		{
			get { return Factory.Load<AccComplianceDocumentLine>(ADP_ADL); }
		}
	}
}