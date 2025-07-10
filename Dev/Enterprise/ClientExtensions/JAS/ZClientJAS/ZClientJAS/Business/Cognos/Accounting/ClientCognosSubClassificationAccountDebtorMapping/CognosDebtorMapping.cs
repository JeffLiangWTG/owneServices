using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosDebtorMapping : AutoClientCognosSubClassificationAccountDebtorMapping
	{
		public CognosDebtorMapping(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CognosAccGLAccountDescriptorExtraInfo ExtraInfo
		{
			get { return Factory.Load<CognosAccGLAccountDescriptorExtraInfo>(T8_T9); }
		}

		[RelatedBusinessObject("DebtorGroup")]
		public override ZGuid T8_OJ
		{
			get { return base.T8_OJ; }
			set { base.T8_OJ = value; }
		}

		[RelatedBusinessObject("ExtraInfo")]
		public override ZGuid T8_T9
		{
			get { return base.T8_T9; }
			set { base.T8_T9 = value; }
		}
	}
}
