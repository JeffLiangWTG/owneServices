using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Modules.AirCargo
{
	public partial class SoundexTesterForm : ZChildForm
	{
		public SoundexTesterForm()
		{
		}

		void zButton1_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgPatternMatch match = factory.New<OrgPatternMatch>();
			OrgHeader tempOrg = factory.New<OrgHeader>();
			tempOrg.OH_FullName = cp1Name.Text;
			tempOrg.MainAddress.OA_Address1 = cp1Address1.Text;
			tempOrg.MainAddress.OA_Address2 = cp1Address2.Text;
			tempOrg.MainAddress.OA_City = cp1City.Text;
			tempOrg.MainAddress.OA_State = cp1State.Text;
			tempOrg.MainAddress.OA_PostCode = cp1Postcode.Text;
			tempOrg.MainAddress.OA_Phone = cp1Phone.Text;

			match.EncodeOrganisation(tempOrg, new StringWithLanguage(tempOrg.OH_FullNameTruncated, tempOrg.OH_Language), tempOrg.MainAddress, string.Empty);

			tempOrg = factory.New<OrgHeader>();
			tempOrg.OH_FullName = cp2Name.Text;
			tempOrg.MainAddress.OA_Address1 = cp2Address1.Text;
			tempOrg.MainAddress.OA_Address2 = cp2Address2.Text;
			tempOrg.MainAddress.OA_City = cp2City.Text;
			tempOrg.MainAddress.OA_State = cp2State.Text;
			tempOrg.MainAddress.OA_PostCode = cp2Postcode.Text;
			tempOrg.MainAddress.OA_Phone = cp2Phone.Text;

			OrgPatternMatch expectedMatch = factory.New<OrgPatternMatch>();
			expectedMatch.EncodeOrganisation(tempOrg, new StringWithLanguage(tempOrg.OH_FullNameTruncated, tempOrg.OH_Language), tempOrg.MainAddress, string.Empty);

			OrgPatternMatchCollection collection = new OrgPatternMatchCollection(factory);
			collection.Add(match);
			expectedMatch.SetMatchScoreForOrg(collection);
			scoreLabel.Text = expectedMatch.OS_Score.ToString();
		}
	}
}
