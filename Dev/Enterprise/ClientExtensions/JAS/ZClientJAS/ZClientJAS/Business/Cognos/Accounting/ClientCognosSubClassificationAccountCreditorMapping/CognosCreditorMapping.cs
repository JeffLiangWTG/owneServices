using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosCreditorMapping : AutoClientCognosSubClassificationAccountCreditorMapping
	{
		public CognosCreditorMapping(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CognosAccGLAccountDescriptorExtraInfo ExtraInfo
		{
			get { return Factory.Load<CognosAccGLAccountDescriptorExtraInfo>(T7_T9); }
		}

		[RelatedBusinessObject("CreditorGroup")]
		public override ZGuid T7_OG
		{
			get { return base.T7_OG; }
			set { base.T7_OG = value; }
		}

		[RelatedBusinessObject("ExtraInfo")]
		public override ZGuid T7_T9
		{
			get { return base.T7_T9; }
			set { base.T7_T9 = value; }
		}
	}
}
