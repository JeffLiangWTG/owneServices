using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocServiceLevel : DocBaseWrapper
	{
		DocServiceLevel(RefServiceLevel refServiceLevel, BusinessObjectFactory factoryToWrap)
			: base(refServiceLevel, factoryToWrap)
		{
		}

		public static DocServiceLevel New(RefServiceLevel refServiceLevel, BusinessObjectFactory factoryToWrap)
		{
			if (refServiceLevel == null)
			{
				return null;
			}
			else
			{
				return new DocServiceLevel(refServiceLevel, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return Code;
		}

		RefServiceLevel RefServiceLevel
		{
			get { return (RefServiceLevel)WrappedObject; }
		}
		public ZString Code
		{
			get { return RefServiceLevel.RS_Code; }
		}
		public ZString Description
		{
			get { return RefServiceLevel.RS_DescriptionMultilingual; }
		}
		public ZBool IsActive
		{
			get { return RefServiceLevel.RS_IsActive; }
		}
		public ZBool IsDoorToDoor
		{
			get { return RefServiceLevel.RS_IsDoorToDoor; }
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}
	}
}
