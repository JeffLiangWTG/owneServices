using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class EDIMessageCollectionNonDependent : BusinessObjectCollection<EDIMessage>
	{
		public EDIMessageCollectionNonDependent(BusinessObjectFactory factory, AsycudaManifestHeader manifest)
			: base(factory)
		{
			this.manifest = manifest;
		}

		readonly AsycudaManifestHeader manifest;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			var parentsQuery = new ZQuery();
			var fks = new List<ZGuid>();
			fks.Add(manifest.PK);
			fks.AddRange(manifest.Bills.OfType<AsycudaBill>().Select(b => b.PK));
			fks.AddRange(manifest.ArrivalHeaders.OfType<AsycudaArrivalHeader>().Select(c => c.PK));
			parentsQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, fks.ToArray());
			query.AddToFilter(parentsQuery);
			return query;
		}

		#region Test Helpers
#if DEBUG

		public void RemoveAndDeleteAllFromTest()
		{
			try
			{
				foreach (EDIMessage message in this)
				{
					message.IsDeletingInTest = true;
				}

				RemoveAndDeleteAll();
			}
			finally
			{
				foreach (EDIMessage message in this)
				{
					message.IsDeletingInTest = false;
				}
			}
		}

#endif
		#endregion

	}
}
