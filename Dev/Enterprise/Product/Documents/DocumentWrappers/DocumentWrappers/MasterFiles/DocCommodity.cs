using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocCommodity : DocBaseWrapper
	{
		DocCommodity(RefCommodityCode refCommodityCode, BusinessObjectFactory factoryToWrap)
			: base(refCommodityCode, factoryToWrap)
		{
		}

		public static DocCommodity New(RefCommodityCode refCommodityCode, BusinessObjectFactory factoryToWrap)
		{
			return (refCommodityCode != null) ? new DocCommodity(refCommodityCode, factoryToWrap) : null;
		}

		public static DocCommodity New(BusinessObjectFactory factory, ZString code)
		{
			return New(factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, code), factory);
		}

		public ZString Code
		{
			get { return RefCommodityCode.RH_Code; }
		}

		public ZString Description
		{
			get { return RefCommodityCode.RH_DescriptionMultilingual; }
		}

		public ZString NMFCClass
		{
			get
			{
				ZString result = new();

				if (RefCommodityCode.NMFC != null)
				{
					result = RefCommodityCode.NMFC.FN_Class;
				}

				return result;
			}
		}

		public ZBool IsActive
		{
			get { return RefCommodityCode.RH_IsActive; }
		}

		public ZBool IsFlammable
		{
			get { return RefCommodityCode.RH_IsFlammable; }
		}

		public ZBool IsHazardous
		{
			get { return RefCommodityCode.RH_IsHazardous; }
		}

		public ZBool IsPerishable
		{
			get { return RefCommodityCode.RH_IsPerishable; }
		}

		public ZBool IsTimber
		{
			get { return RefCommodityCode.RH_IsTimber; }
		}

		public override string ToString()
		{
			return Code;
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}

		#region Implementation

		RefCommodityCode RefCommodityCode
		{
			get { return (RefCommodityCode)WrappedObject; }
		}

		#endregion
	}
}
