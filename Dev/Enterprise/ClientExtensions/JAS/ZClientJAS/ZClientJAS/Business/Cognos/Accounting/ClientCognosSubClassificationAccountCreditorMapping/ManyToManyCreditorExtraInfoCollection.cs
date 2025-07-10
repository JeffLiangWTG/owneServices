using System;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ManyToManyCreditorExtraInfoCollection : ManyToManyBusinessObjectCollection<CognosAccGLAccountDescriptorExtraInfo, JASOrgCreditorGroup>
	{
		public ManyToManyCreditorExtraInfoCollection(JASOrgCreditorGroup associatedObject)
			: base(associatedObject)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(CognosCreditorMapping); }
		}
	}
}
