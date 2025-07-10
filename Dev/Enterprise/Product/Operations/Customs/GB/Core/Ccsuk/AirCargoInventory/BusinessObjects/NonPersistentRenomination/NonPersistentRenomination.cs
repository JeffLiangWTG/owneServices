using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentRenomination : AutoNonPersistentRenomination
	{
		public NonPersistentRenomination(BusinessObjectFactory factory)
			: base(factory)
		{
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

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SendNewAgentGenral = true;
		}
	}
}
