using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.BR.Business
{
	public class DocCusContainer : DocBaseCusContainer
	{
		DocCusContainer(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
			: base(cusContainer, factoryToWrap)
		{
		}

		public static DocCusContainer New(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
		{
			return cusContainer == null ? null : new DocCusContainer(cusContainer, factoryToWrap);
		}

		public static DocCusContainer New(CusContainer cusContainer, JobDeclaration declaration, BusinessObjectFactory factoryToWrap)
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
