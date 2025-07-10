using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmCustomizableEventCodeDescriptionPairList : CodeDescriptionPairList
	{
		public StmCustomizableEventCodeDescriptionPairList(BusinessObjectFactory factory)
			: base()
		{
			Clear();

			foreach (var stmEvent in factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_IsCustomizable, true)))
			{
				Add(new CodeDescriptionPair(stmEvent.SE_Code.ToString(), stmEvent.SE_DescMultilingual));
			}

			Sort();
		}
	}
}
