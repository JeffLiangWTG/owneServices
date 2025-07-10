using System;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ManyToManyCognosDebtorCollection : ManyToManyBusinessObjectCollection<JASOrgDebtorGroup, CognosAccGLAccountDescriptorExtraInfo>
	{
		public ManyToManyCognosDebtorCollection(CognosAccGLAccountDescriptorExtraInfo associatedObject)
			: base(associatedObject)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(CognosDebtorMapping); }
		}
	}
}
