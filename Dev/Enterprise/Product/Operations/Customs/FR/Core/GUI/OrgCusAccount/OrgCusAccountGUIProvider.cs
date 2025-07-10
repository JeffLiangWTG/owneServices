using CargoWise.Types;

namespace Enterprise.Customs.FR.GUI
{
	public class OrgCusAccountGUIProvider : MasterFiles.GUI.OrgCusAccountGUIProvider
	{
		public OrgCusAccountGUIProvider(ZString countryCode)
			: base(countryCode)
		{
		}

		public override ZGridColumnInfoControl CZ_Code => new ZGridColumnInfoControl(Res.GetData("A3C54040-96E8-46D9-982F-39DF83FEC756", "Agreement Type"), 114);
		public override ZGridColumnInfoControl CZ_Account => new ZGridColumnInfoControl(Res.GetData("DC816850-6BC6-42A8-B2A8-6DBEE259FCAF", "Delta Agreement/Profile Number"), 225);
		public override ZGridColumnInfoControl CZ_Type => new ZGridColumnInfoControl(Res.GetData("9F00443D-DE88-499D-8F0B-AB7F58A41E32", "Delta Mode"), 73);
		public override ZGridColumnInfoControl CZ_Issuer => new ZGridColumnInfoControl(Res.GetData("24F67BC2-02FF-418F-ABF3-CA269C98CFE0", "Customs Office"), 90);
		public override ZGridColumnInfoControl DecryptedPassword => new ZGridColumnInfoControl(Res.GetData("AE7988BE-D48F-4540-9753-9E9A0154D15B", "BIN2"), 159, false);
		public override ZGridColumnInfoControl CZ_ReportingPeriod => new ZGridColumnInfoControl(Res.GetData("B61DF467-1E21-4B2F-9BEB-F94463A736A3", "Reporting Frequency"), 154);
		public override ZGridColumnInfoControl CZ_RepresentativeID => new ZGridColumnInfoControl(Res.GetData("45A8B244-F521-40C6-91DF-63B1558D604B", "Declarant/Representative ID"), 225);
	}
}
