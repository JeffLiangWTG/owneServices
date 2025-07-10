using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class ReportFilterCodeListLookupsWrapperCollection : GenericWrapperCollection<CodeMultilingualDescriptionWrapper>
	{
		public ReportFilterCodeListLookupsWrapperCollection(BusinessObjectFactory factory) : base(factory)
		{
			new CodeListChoiceCodeDescriptionList().OfType<CodeDescriptionPair>().ForEach(item =>
			{
				Add(new CodeMultilingualDescriptionWrapper(item.Code, item.Description, factory));
			});
		}
	}
}
