using System;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ManyToManyCognosCreditorCollection : ManyToManyBusinessObjectCollection<JASOrgCreditorGroup, CognosAccGLAccountDescriptorExtraInfo>
	{
		public ManyToManyCognosCreditorCollection(CognosAccGLAccountDescriptorExtraInfo associatedObject)
			: base(associatedObject)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(CognosCreditorMapping); }
		}
	}
}
