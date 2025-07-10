using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JASForwardingSEAShipmentDocAddressValidationTest : JXCValidationTestCase
	{
		public void TestValidateConsignorPK()
		{
			AssertHasNoJXCWarnings("Pre-condition", Shipment.ConsignorPKInfo);
			Shipment.ConsignorPK = ZGuid.Empty;
			Shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasNotEnteredJXCWarning(Shipment.ConsignorPKInfo);
			Shipment.ConsignorPK = Factory.New<JASOrgHeader>().PK;
			string expectedWarning1 = JXCConstants.JXCWarningPrefix + "Organisation Name cannot be blank";
			string expectedWarning2 = JXCConstants.JXCWarningPrefix + "Address Line 1 cannot be blank";
			string expectedWarning3 = JXCConstants.JXCWarningPrefix + "City cannot be blank";
			string expectedWarning4 = JXCConstants.JXCWarningPrefix + "Post Code and State cannot both be blank";
			string expectedWarning5 = JXCConstants.JXCWarningPrefix + "Port Code cannot be blank";
			AssertHasJXCWarning(Shipment.ConsignorPKInfo, expectedWarning1, expectedWarning2, expectedWarning3, expectedWarning4, expectedWarning5);
			Shipment.Consignor.OH_FullName = "FULLName";
			Shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsignorPKInfo, expectedWarning2, expectedWarning3, expectedWarning4, expectedWarning5);
			Shipment.Consignor.MainAddress.OA_Address1 = "ADDRESS 1";
			Shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsignorPKInfo, expectedWarning3, expectedWarning4, expectedWarning5);
			Shipment.Consignor.MainAddress.OA_City = "CITYOFGOD";
			Shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsignorPKInfo, expectedWarning4, expectedWarning5);
			Shipment.Consignor.MainAddress.OA_State = "NSW";
			Shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsignorPKInfo, expectedWarning5);
			Shipment.Consignor.OH_RL_NKClosestPort = "AUBNE";
			Shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasNoJXCWarnings(Shipment.ConsignorPKInfo);
		}

		public void TestValidateConsigneePK()
		{
			AssertHasNoJXCWarnings("Pre-condition", Shipment.ConsigneePKInfo);
			Shipment.ConsigneePK = ZGuid.Empty;
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasNotEnteredJXCWarning(Shipment.ConsigneePKInfo);
			Shipment.ConsigneePK = Factory.New<JASOrgHeader>().PK;
			string expectedWarning1 = JXCConstants.JXCWarningPrefix + "Organisation Name cannot be blank";
			string expectedWarning2 = JXCConstants.JXCWarningPrefix + "Address Line 1 cannot be blank";
			string expectedWarning3 = JXCConstants.JXCWarningPrefix + "City cannot be blank";
			string expectedWarning4 = JXCConstants.JXCWarningPrefix + "Post Code cannot be blank";
			string expectedWarning5 = JXCConstants.JXCWarningPrefix + "State cannot be blank";
			string expectedWarning6 = JXCConstants.JXCWarningPrefix + "Port Code cannot be blank";
			AssertHasJXCWarning(Shipment.ConsigneePKInfo, expectedWarning1, expectedWarning2, expectedWarning3, expectedWarning4, expectedWarning5, expectedWarning6);
			Shipment.Consignee.OH_FullName = "FULLName";
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsigneePKInfo, expectedWarning2, expectedWarning3, expectedWarning4, expectedWarning5, expectedWarning6);
			Shipment.Consignee.MainAddress.OA_Address1 = "ADDRESS 1";
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsigneePKInfo, expectedWarning3, expectedWarning4, expectedWarning5, expectedWarning6);
			Shipment.Consignee.MainAddress.OA_City = "CITYOFGOD";
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsigneePKInfo, expectedWarning4, expectedWarning5, expectedWarning6);
			Shipment.Consignee.MainAddress.OA_PostCode = "POCODE";
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsigneePKInfo, expectedWarning5, expectedWarning6);
			Shipment.Consignee.MainAddress.OA_State = "NSW";
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasJXCWarning(Shipment.ConsigneePKInfo, expectedWarning6);
			Shipment.Consignee.OH_RL_NKClosestPort = "AUBNE";
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasNoJXCWarnings(Shipment.ConsigneePKInfo);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Factory.Validation.MainGroup.RegisterValidationType(typeof(JASForwardingShipment), typeof(JXCForwardingSeaShipmentValidation));
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
				}

				return fShipment;
			}
		}

		JASForwardingShipment fShipment;
		#endregion
	}
}
