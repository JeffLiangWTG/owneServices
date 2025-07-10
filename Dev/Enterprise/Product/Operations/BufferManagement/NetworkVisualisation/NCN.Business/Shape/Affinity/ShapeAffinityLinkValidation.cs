using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeAffinityLinkValidation : ZValidation
	{
		public ShapeAffinityLinkValidation(ShapeAffinityLink parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ShapeAffinityLink parent;

		public override Type AutoValidationType
		{
			get { return parent.GetType(); }
		}

		public override void ValidateAll()
		{
		}
	}
}
