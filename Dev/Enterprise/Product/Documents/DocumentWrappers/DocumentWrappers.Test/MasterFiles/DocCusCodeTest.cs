using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusCode))]
	public class DocCusCodeTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCusCode.New(Code, Factory)
			};
		}

		OrgCusCode Code;
		protected override void SetUp()
		{
			Code = Factory.New<OrgCusCode>();
			var premisesHeader = Factory.New<OrgHeader>();
			var premisesAddress = Factory.New<OrgAddress>();
			premisesAddress.OA_OH = premisesHeader.PK;
			Code.OK_OA_PremisesAddress = premisesAddress.PK;
			base.SetUp();
		}
	}
}
