using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentLookups : AutoBMComponentLookups
	{
		public BMComponentLookups(AutoBMComponent parent)
			: base(parent)
		{
		}

		#region ParentComponents

		public virtual BMComponentCollection ParentComponents
		{
			get { return new BMComponentCollection(Factory); }
		}

		#endregion

		#region Systems

		public virtual BMSystemCollection Systems
		{
			get { return new BMSystemCollection(Factory); }
		}

		#endregion

		public CodeDescriptionPairList Types
		{
			get
			{
				var component = (BMComponent)Parent;

				if (component.FC_FS_System.IsValid)
				{
					return Factory.GetCachedValue("BMComponentTypeListWithSystem", () =>
					{
						var list = new BMComponentTypeList();
						list.RemoveCode(BMComponentTypeList.Codes.ComponentRelationship);
						return list;
					});
				}
				else
				{
					return Factory.GetCachedValue("BMComponentTypeListWithoutSystem", () =>
					{
						var list = new BMComponentTypeList();
						list.RemoveCode(BMComponentTypeList.Codes.Buffer);
						list.RemoveCode(BMComponentTypeList.Codes.Bucket);
						list.RemoveCode(BMComponentTypeList.Codes.Decouple);
						list.RemoveCode(BMComponentTypeList.Codes.Constraint);
						return list;
					});
				}
			}
		}
	}
}
