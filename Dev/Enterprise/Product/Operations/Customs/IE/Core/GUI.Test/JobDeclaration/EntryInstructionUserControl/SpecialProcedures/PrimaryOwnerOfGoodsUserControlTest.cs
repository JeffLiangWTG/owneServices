using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class PrimaryOwnerOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			using (var control = new PrimaryOwnerOfGoodsUserControlForTest())
			{
				control.SetJobDeclarationForTest(declaration);

				var groupBox = control.FindSingleOrDefault<ZGroupBox>("PrimaryOwnerOfGoodsGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "[Art. 163 3/8] Primary Owner of Goods", groupBox.CaptionResourceString.Caption);
					AssertEquals("Full Description", "[Article 163 3/8] Primary Owner of Goods", groupBox.CaptionResourceString.FullDescription);
				});
			}
		}
	}

	class PrimaryOwnerOfGoodsUserControlForTest : PrimaryOwnerOfGoodsUserControl
	{
		public void SetJobDeclarationForTest(JobDeclaration jobDeclaration)
		{
			this.BindingSource.SetDataBinding(jobDeclaration, ".");
			SetCaption();
		}
	}
}
