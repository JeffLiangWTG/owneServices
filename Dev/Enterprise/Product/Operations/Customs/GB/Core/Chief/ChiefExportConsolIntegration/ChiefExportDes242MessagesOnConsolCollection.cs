using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ChiefExportDes242MessagesOnConsolCollection : EDIMessageCollection
	{
		public ChiefExportDes242MessagesOnConsolCollection(ForwardingConsol forwardingConsol)
			: base(forwardingConsol)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, new ZString[]
			{
				ApplicationCodeList.Codes.GbCcsuk,
				ApplicationCodeList.Codes.GbEdifactShared,
				ApplicationCodeList.Codes.GbCustomsDeclarationServices,
				ApplicationCodeList.Codes.GbCDSViaCCSUK
			});
			return query;
		}

		protected override bool AllowRemoveCore => false;

		public override bool ReadOnly => true;

		protected override bool AllowNewCore => false;

		public void Refresh()
		{
			RemoveAll();
			Load();
		}
	}
}
