using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusHAWB : DocCusHAWBBase
	{
		#region Constructors and Type Overriding

		protected DocCusHAWB(CusHAWB cusHAWB, BusinessObjectFactory factoryToWrap)
			: base(cusHAWB, factoryToWrap)
		{
		}

		public static DocCusHAWB New(CusHAWB cusHAWB, BusinessObjectFactory factoryToWrap)
		{
			DocCusHAWB result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(cusHAWB, factoryToWrap);
			}
			else if (cusHAWB != null)
			{
				result = NewWithoutTypeOverride(cusHAWB, factoryToWrap);
			}

			return result;
		}

		protected static DocCusHAWB NewWithoutTypeOverride(CusHAWB cusHAWB, BusinessObjectFactory factoryToWrap)
		{
			return new DocCusHAWB(cusHAWB, factoryToWrap);
		}

		protected delegate DocCusHAWB NewDelegate(CusHAWB cusHAWB, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		protected new CusHAWB CusHAWB
		{
			get { return (CusHAWB)WrappedObject; }
		}

		public DocDeclaration Declaration
		{
			get { return DocDeclaration.New(CusHAWB.Declaration, Factory); }
		}
	}
}
