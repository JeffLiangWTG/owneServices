using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection : NonPersistentBusinessObjectCollection<ChiefExportConsolIntegrationNPBO>
	{
		public ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection(ForwardingConsol forwardingConsol)
			: base(forwardingConsol.Factory)
		{
			this.forwardingConsol = forwardingConsol;
			AddAllMessagesToCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		void AddAllMessagesToCollection()
		{
			foreach (ForwardingShipment shipment in forwardingConsol.Shipments)
			{
				if (shipment.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
				{
					foreach (BaseJobDeclaration baseDeclaration in shipment.Declarations)
					{
						var euDeclaration = baseDeclaration as JobDeclaration;
						if (euDeclaration != null)
						{
							foreach (EU.Business.Declaration.CusEntryHeader entry in euDeclaration.ActiveEntryHeaders)
							{
								foreach (var eacMessage in (from EDIMessage m in entry.Messages where m.EM_MessageType == GbDes242MessageFunction.Eac select m))
								{
									var nbpo = new ChiefExportConsolIntegrationNPBO(eacMessage);
									Add(nbpo);
								}
							}
						}
					}
				}
			}
		}

		public void Refresh()
		{
			RemoveAll();
			AddAllMessagesToCollection();
		}

		readonly ForwardingConsol forwardingConsol;
	}
}
