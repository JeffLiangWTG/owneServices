using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class StmEventCollection : ActiveBusinessObjectCollection<StmEvent>
	{
		public StmEventCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}

	public class StmEventCodeDescriptionPairList : CodeDescriptionPairList
	{
		public StmEventCodeDescriptionPairList()
			: base()
		{
			Clear();
			var stmEventCollection = new StmEventCollection(new BusinessObjectFactory());
			var isPWEnabled = DataRegistry.Instance.ProductivityWiseModeEnabled;
			AddRange(stmEventCollection.
				Where(stmEvent => !isPWEnabled || stmEvent.SE_IsVisibleToPW).
				Select(stmEvent => new CodeDescriptionPair(stmEvent.SE_Code.ToString(), stmEvent.SE_DescMultilingual.ToString())).ToList());
			Sort();
		}
	}
}
