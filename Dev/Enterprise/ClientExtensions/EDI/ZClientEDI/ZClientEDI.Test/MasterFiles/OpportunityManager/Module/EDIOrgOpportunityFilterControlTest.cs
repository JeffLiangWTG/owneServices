using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	public class EDIOrgOpportunityFilterControlTest : TestCaseWithFactory
	{
		public void TestEDISpecificColumnCaptions()
		{
			var collection = new OrgOpportunityCollection(Factory);
			using (var control = new EDIOrgOpportunityFilterControl(collection, new EDIOrgOpportunityFilterBusinessObject()))
			{
				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
					if (textBoxColumnStyle != null)
					{
						if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_DiscountAmount.Name)
						{
							AssertNotNull("Precondition: column exists", textBoxColumnStyle);
							AssertEquals("CaptionResourceString", ResourceStringData.Empty, textBoxColumnStyle.CaptionResourceString);
							AssertEquals("Caption", "Contracted", textBoxColumnStyle.Caption);
						}
						else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_RentalMultiplier.Name)
						{
							AssertNotNull("Precondition: column exists", textBoxColumnStyle);
							AssertEquals("CaptionResourceString", ResourceStringData.Empty, textBoxColumnStyle.CaptionResourceString);
							AssertEquals("Caption", "Local Reach", textBoxColumnStyle.Caption);
						}
						else if (textBoxColumnStyle.ColumnName == "OrgOpportunityEx+" + EdiOrgOpportunityExSchema.EOM_GlobalPotential.Name)
						{
							AssertNotNull("Precondition: column exists", textBoxColumnStyle);
							AssertEquals("CaptionResourceString", Res.GetData("b500ec71-0a4d-4114-860b-cd8de2ed619f", "Global Reach"), textBoxColumnStyle.CaptionResourceString);
						}
						else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_EstimatedValue.Name)
						{
							AssertNotNull("Precondition: column exists", textBoxColumnStyle);
							AssertEquals("CaptionResourceString", Res.GetData("858a2fb7-37e8-458d-ac5e-492a6b03becd", "Contract (p.a)"), textBoxColumnStyle.CaptionResourceString);
							AssertEquals("Caption", "Contract (p.a)", textBoxColumnStyle.Caption);
						}
						else if (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_RX_NKEstimatedValueCurrency.Name)
						{
							AssertNotNull("Precondition: column exists", textBoxColumnStyle);
							AssertEquals("CaptionResourceString", Res.GetData("edc012c6-caea-4d78-bb56-13ba5da137ec", "Contract Currency"), textBoxColumnStyle.CaptionResourceString);
							AssertEquals("Caption", "Contract Currency", textBoxColumnStyle.Caption);
						}
						else if (textBoxColumnStyle.ColumnName == "OrgOpportunityEx+" + EdiOrgOpportunityExSchema.EOM_LifetimeValueOver3Years.Name)
						{
							AssertNotNull("Precondition: column exists", textBoxColumnStyle);
							AssertEquals("CaptionResourceString", Res.GetData("e497a4db-c6fb-41e5-ab66-ee47b8b3394c", "Lifetime (3 CLV)"), textBoxColumnStyle.CaptionResourceString);
						}
					}
				}
			}
		}
	}
}
