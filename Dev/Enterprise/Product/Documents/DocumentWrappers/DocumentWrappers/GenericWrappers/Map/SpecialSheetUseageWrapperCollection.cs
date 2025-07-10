using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class SpecialSheetUseageWrapperCollection : GenericWrapperCollection<CodeMultilingualDescriptionWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public SpecialSheetUseageWrapperCollection(BusinessObjectFactory factory, CodeDescriptionPairList codePairList)
			: base(factory)
		{
			foreach (CodeDescriptionPair item in codePairList)
			{
				Add(new CodeMultilingualDescriptionWrapper(item.Code, item.Description, Factory));
			}
		}

		public CodeMultilingualDescriptionWrapper FindByUseage(ZString useage)
		{
			foreach (CodeMultilingualDescriptionWrapper item in this)
			{
				if (item.Useage == useage)
				{
					return item;
				}
			}
			return null;
		}
	}
}
