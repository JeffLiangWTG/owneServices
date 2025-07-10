using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CodeAndDescriptionWrapperCollection : DocumentWrappers.GenericWrappers.CodeAndDesriptionWrapperCollection
	{
		public CodeAndDescriptionWrapperCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new CodeAndDescriptionWrapper this[string code] => this.Cast<CodeAndDescriptionWrapper>().FirstOrDefault(x => x.Code == code);

		public static CodeAndDescriptionWrapperCollection New(IEnumerable<ZString> codes, ICodeDescriptionPairList list, BusinessObjectFactory factory)
		{
			var collection = new CodeAndDescriptionWrapperCollection(factory);

			foreach (var code in codes)
			{
				collection.Add(CodeAndDescriptionWrapper.New(code, list, factory));
			}
			return collection;
		}
	}
}
