using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IT.GUI;

public class BrokeragePlugIn : EU.GUI.BrokeragePlugIn
{
	public BrokeragePlugIn(ForwardingShipment shipment) : base(shipment)
	{
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();

	protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
	{
		var preSaveStrategies = new List<PreSaveDialogStrategy>(base.GetPreSaveDialogStrategies());

		var declaration = JobDeclaration;
		if (declaration != null)
		{
			preSaveStrategies.Add(new DeclarationOfIntentPreSaveDialogStrategy(declaration.DeclarationOfIntentRefresher));
		}
		return preSaveStrategies;
	}

	protected override MenuItem GetNewTopLevelMenuCore() => new EDIMenu();
}
