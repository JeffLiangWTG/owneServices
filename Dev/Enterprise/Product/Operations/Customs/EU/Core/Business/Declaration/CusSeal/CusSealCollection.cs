using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusSealCollection : ActiveBusinessObjectCollection<CusSeal>
	{
		public CusSealCollection(BusinessObject master) : base(master.Factory, master, new ZQuery(CusSealSchema.BK_ParentTableCode, master.TablePrefix), CusSealSchema.BK_ParentID)
		{
		}

		protected override bool AllowNew
		{
			get
			{
				var result = Count < 100;

				if (Relationship.Master is ICusSealCollectionSupporter cusSealCollectionSupporter)
				{
					result = result && cusSealCollectionSupporter.AllowNewCusSeal;
				}

				return result;
			}
		}
	}
}
