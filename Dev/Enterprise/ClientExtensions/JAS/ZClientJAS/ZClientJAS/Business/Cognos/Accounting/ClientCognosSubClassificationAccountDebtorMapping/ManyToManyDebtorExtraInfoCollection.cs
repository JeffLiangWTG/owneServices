using System;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ManyToManyDebtorExtraInfoCollection : ManyToManyBusinessObjectCollection<CognosAccGLAccountDescriptorExtraInfo, JASOrgDebtorGroup>
	{
		public ManyToManyDebtorExtraInfoCollection(JASOrgDebtorGroup associatedObject)
			: base(associatedObject)
		{
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(CognosDebtorMapping); }
		}
	}
}
