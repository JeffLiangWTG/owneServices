
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusContainer : DocBaseCusContainer
	{
		protected DocCusContainer(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
			: base(cusContainer, factoryToWrap)
		{
		}

		public static DocCusContainer New(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
		{
			DocCusContainer result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(cusContainer, factoryToWrap);
			}
			else if (cusContainer != null)
			{
				result = new DocCusContainer(cusContainer, factoryToWrap);
			}
			return result;
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

		#region Overrides

		public override ZInt TotalAllocatedJobPackages
		{
			get
			{
				ZInt result = 0;
				foreach (BasePackage package in CusContainer.Packages)
				{
					result += package.CW_PackQty;
				}
				return result;
			}
		}

		#endregion

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
