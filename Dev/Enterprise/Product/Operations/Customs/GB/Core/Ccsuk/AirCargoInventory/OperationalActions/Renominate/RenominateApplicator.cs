using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class RenominateApplicator : AutoRenominateApplicator
	{
		public RenominateApplicator(BusinessObjectFactory factory)
			: base("GB CCSUK function: nominate new agent and send FRC", factory)
		{ }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var methodInvoker = new CcsukOperationalActionApplicatorRunner(log, targets);
			methodInvoker.Renominate(NewAgent);
		}

		[List(nameof(AgentsList))]
		public override ZString NewAgent
		{
			get { return base.NewAgent; }
			set { base.NewAgent = value; }
		}

		public CodeDescriptionPairList AgentsList
		{
			get { return CusMAWBLookups.GetAllAgents(Factory); }
		}
	}

	public class RenominateApplicatorValidation : AutoRenominateApplicatorValidation
	{
		public RenominateApplicatorValidation(AutoRenominateApplicator parent)
			: base(parent)
		{ }
	}
}
