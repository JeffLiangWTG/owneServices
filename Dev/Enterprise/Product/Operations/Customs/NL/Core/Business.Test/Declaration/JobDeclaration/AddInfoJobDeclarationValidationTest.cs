using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_SpecificCircumstanceIndicator()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.ZG_TypeOfSecurity = ZString.Empty;
		var propertyInfo = declaration.ZG_SpecificCircumstanceIndicatorInfo;
		ValidationTestHelper.AssertInvalidCodeMessageError(propertyInfo, "0", NLSpecificCircumstanceIndicatorList.Codes.A20);
		declaration.ZG_SpecificCircumstanceIndicator = NLSpecificCircumstanceIndicatorList.Codes.A20;
		AssertNoNotifications(propertyInfo);
	}

	readonly List<string> proceduresC9008 = new string[] { "1050", "1120", "2340", "3180" }.ToList();
	readonly List<string> transportModesC9008 = new string[] { ModeOfTransportCodeList.Codes._AIR, ModeOfTransportCodeList.Codes._IWT, ModeOfTransportCodeList.Codes._OWN, ModeOfTransportCodeList.Codes._ROA, ModeOfTransportCodeList.Codes._SEA }.ToList();

	public void TestCheckZG_CheckZG_BorderTransportMeans()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var expectedError = "[C9008] Border Transport Means is required.";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			foreach (var procedure in proceduresC9008)
			{
				invoiceLine.JI_Procedure = procedure;
				foreach (var transportMode in new ModeOfTransportCodeList().GetAllCodes())
				{
					declaration.JE_TransportMode = transportMode;
					declaration.ZG_BorderTransportMeans = string.Empty;
					declaration.Validation.ValidateZG_BorderTransportMeans();
					if (transportModesC9008.Contains(transportMode))
					{
						AssertHasMessageError($"Border MOT is '{transportMode}', Procedure is '{procedure}' and Transport ID is empty", declaration.ZG_BorderTransportMeansInfo, expectedError);
						declaration.ZG_BorderTransportMeans = "20";
						declaration.Validation.ValidateZG_BorderTransportMeans();
						AssertNoMessageError($"Border MOT is '{transportMode}', Procedure is '{procedure}' and Transport ID is filled", declaration.ZG_BorderTransportMeansInfo, expectedError);
					}
					else
					{
						AssertNoMessageError($"Border MOT ({transportMode}) should not trigger message", declaration.ZG_BorderTransportMeansInfo, expectedError);
					}
				}
			}

			invoiceLine.JI_Procedure = "4280";
			foreach (var transportMode in new ModeOfTransportCodeList().GetAllCodes())
			{
				declaration.JE_TransportMode = transportMode;
				declaration.ZG_BorderTransportMeans = string.Empty;
				declaration.Validation.ValidateZG_BorderTransportMeans();
				AssertNoMessageError("Procedure on invoiceline does not start with 10, 11, 23 or 31, no message should be shown", declaration.ZG_BorderTransportMeansInfo, expectedError);
			}

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_TransportMode = ModeOfTransportCodeList.Codes._ROA;
			invoiceLine.JI_Procedure = "1000";
			AssertNoMessageError("On Import-declarations, no message should be shown.", declaration.ZG_BorderTransportMeansInfo, expectedError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
}
