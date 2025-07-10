using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeAlive("To be used in next WI")]
	public class InvestigationItemResponseOption : AutoInvestigationItemResponseOption
	{
		public InvestigationItemResponseOption(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public InvestigationItem InvestigationItem => Factory.Load<InvestigationItem>(INR_INV_InvestigationItem);

		[RelatedBusinessObject("InvestigationItem")]
		public override ZGuid INR_INV_InvestigationItem { get => base.INR_INV_InvestigationItem; set => base.INR_INV_InvestigationItem = value; }
	}
}
