using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientPremiumServiceCollection : ActiveBusinessObjectCollection<ClientPremiumService>
	{
		public ClientPremiumServiceCollection(LicenceDatabase database)
			: base(database.Factory, database, new ZQuery(), ClientPremiumServiceSchema.CPS_LD)
		{
			Database = database;
		}

		readonly LicenceDatabase Database;

		public IEnumerable<ClientPremiumService> GetMatched(ZDateTime billingDate)
		{
			return this.Cast<ClientPremiumService>().Where(x => x.IsDateRangeMatched(billingDate));
		}

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(ClientPremiumService newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.CPS_LD = Database.PK;
		}

		#endregion
	}
}

