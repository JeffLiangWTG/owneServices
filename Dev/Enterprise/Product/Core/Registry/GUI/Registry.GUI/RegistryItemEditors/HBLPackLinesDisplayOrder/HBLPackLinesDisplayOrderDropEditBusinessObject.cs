using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	public class HBLPackLinesDisplayOrderDropEditBussinessObject : DropEditBusinessObject
	{
		public HBLPackLinesDisplayOrderDropEditBussinessObject(CodeDescriptionPairList list) : base(list)
		{
		}

		public ZString Description
		{
			get { return List.GetDescriptionFromCode(Value); }
			set
			{
				Value = List.GetCodeFromDescription(value);
				DescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}
	}
}
