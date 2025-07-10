
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.General
{
	public class DocCusContainer : DocBaseCusContainer
	{
		DocCusContainer(BaseCusContainer cusContainer, BusinessObjectFactory factoryToWrap)
			: base(cusContainer, factoryToWrap)
		{
		}

		public static DocCusContainer New(BaseCusContainer cusContainer, BusinessObjectFactory factoryToWrap)
		{
			if (cusContainer == null)
			{
				return null;
			}
			else
			{ return new DocCusContainer(cusContainer, factoryToWrap); }
		}

		public static DocCusContainer New(BaseCusContainer cusContainer, BaseJobDeclaration declaration, BusinessObjectFactory factoryToWrap)
		{
			DocCusContainer result = New(cusContainer, factoryToWrap);

			if (result != null)
			{
				result.SetDeclaration(declaration);
			}

			return result;
		}
	}
}
