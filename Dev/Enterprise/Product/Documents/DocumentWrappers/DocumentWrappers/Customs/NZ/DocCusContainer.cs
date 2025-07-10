using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	public class DocCusContainer : DocBaseCusContainer
	{
		protected DocCusContainer(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
			: base(cusContainer, factoryToWrap)
		{
		}

		public static DocCusContainer New(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
		{
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				return overridden(cusContainer, factoryToWrap);
			}
			else if (cusContainer == null)
			{
				return null;
			}
			else
			{ return new DocCusContainer(cusContainer, factoryToWrap); }
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

		#region Implementation

		protected delegate DocCusContainer NewDelegate(CusContainer cusContainer, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected CusContainer CusContainer
		{
			get { return (CusContainer)WrappedObject; }
		}

		#endregion
	}
}
