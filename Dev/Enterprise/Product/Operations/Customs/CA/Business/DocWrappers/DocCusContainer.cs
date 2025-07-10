using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CA.Business
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
			var result = New(cusContainer, factoryToWrap);

			if (result != null)
			{
				result.SetDeclaration(declaration);
			}

			return result;
		}

		#region Implementation

		//private CusContainer CusContainer
		//{
		//  get { return (CusContainer)WrappedObject; }
		//}

		#endregion
	}
}
