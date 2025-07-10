using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSEDIMessagePrettierTests : TestCaseWithFactory
	{
		public void TestGetCodeAndDescription()
		{
			var list = new DeclarationStatusICSList();

			var code = DeclarationStatusICSList.Codes.DeclarationCorrected;
			var descrip = DeclarationStatusICSList.Descriptions.DeclarationCorrected;

			AssertEquals("Invalid code", "ABC", prettier.GetCodeAndDescription_Exposed("ABC", list));
			AssertEquals("Valid code", $"{code} - {descrip}", prettier.GetCodeAndDescription_Exposed(code, list));
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			var msg = Factory.New<CDSEDIMessage>();
			prettier = new CDSEDIMessagePrettierForTest(msg);
		}
		CDSEDIMessagePrettierForTest prettier;

		class CDSEDIMessagePrettierForTest : CDSEDIMessagePrettier
		{
			public CDSEDIMessagePrettierForTest(CDSEDIMessage message) : base(message)
			{
			}

			public override ZString MakeHumanReadable()
			{
				return "This class is for testing common formatting functions";
			}

			public ZString GetCodeAndDescription_Exposed(ZString code, CodeDescriptionPairList list) => base.GetCodeAndDescription(code, list);
		}
	}
}
