using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	public class EDIOpportunityManagementControlTest : TestCaseWithFactory
	{
		public void TestEDISpecificColumnCaptions()
		{
			using (var control = new EDIOpportunityManagementControlForTest())
			{
				foreach (var columnStyle in control.OpportunitiesGridExposed().ColumnStyles)
				{
					var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
					if (textBoxColumnStyle != null && textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_DiscountAmount.Name)
					{
						AssertNotNull("Precondition: column exists", textBoxColumnStyle);
						AssertEquals("CaptionResourceString", ResourceStringData.Empty, textBoxColumnStyle.CaptionResourceString);
						AssertEquals("Caption", "Contracted", textBoxColumnStyle.Caption);
					}
					else if (textBoxColumnStyle != null && textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_RentalMultiplier.Name)
					{
						AssertNotNull("Precondition: column exists", textBoxColumnStyle);
						AssertEquals("CaptionResourceString", ResourceStringData.Empty, textBoxColumnStyle.CaptionResourceString);
						AssertEquals("Caption", "Local Reach", textBoxColumnStyle.Caption);
					}
					else if (textBoxColumnStyle != null && textBoxColumnStyle.ColumnName == "OrgOpportunityEx+" + EdiOrgOpportunityExSchema.EOM_GlobalPotential.Name)
					{
						AssertNotNull("Precondition: column exists", textBoxColumnStyle);
						AssertEquals("CaptionResourceString", Res.GetData("b500ec71-0a4d-4114-860b-cd8de2ed619f", "Global Reach"), textBoxColumnStyle.CaptionResourceString);
					}
				}
			}
		}
	}
}
