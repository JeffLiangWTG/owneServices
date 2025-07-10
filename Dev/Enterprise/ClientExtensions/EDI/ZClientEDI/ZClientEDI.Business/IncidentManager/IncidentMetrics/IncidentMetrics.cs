using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeAlive("To be used in next WI")]
	public class IncidentMetrics : AutoIncidentMetrics
	{
		public IncidentMetrics(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(20)]
		public override ZString IME_IncidentNumber
		{
			get { return base.IME_IncidentNumber; }
			set
			{
				base.IME_IncidentNumber = value;
			}
		}
	}
}
