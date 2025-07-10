using CargoWise.Types;

namespace Enterprise.Customs.DE.GUI
{
	public class OrgCusAccountGUIProvider : MasterFiles.GUI.OrgCusAccountGUIProvider
	{
		public OrgCusAccountGUIProvider(ZString countryCode)
			: base(countryCode)
		{
		}

		public override ZGridColumnInfoControl CZ_Code => new ZGridColumnInfoControl(Res.GetData("893719b6-3261-442b-b3c1-33c93af22a05", "Account"), 62);
		public override ZGridColumnInfoControl CZ_Account => new ZGridColumnInfoControl(Res.GetData("259769dc-9259-461a-9bdf-b87b4d0ded0b", "Account Number"), 104);
		public override ZGridColumnInfoControl CZ_Type => new ZGridColumnInfoControl(Res.GetData("6D6D8A61-A38A-4CB3-950B-0665565EBDC3", "Account Type"), 89);
		public override ZGridColumnInfoControl CZ_Issuer => new ZGridColumnInfoControl(Res.GetData("fea877cc-149f-4147-b56e-6992310622c3", "Prefix"), 50);
		public override ZGridColumnInfoControl DecryptedPassword => new ZGridColumnInfoControl(Res.GetData("0c76dced-a089-4878-b857-77f085a80cc7", "BIN"), 159);
	}
}
