using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class DeclarationValueChangedDefaulter : TestCaseWithFactory
	{
		public void TestNationalityFromFlightNumber()
		{
			var deltaOrg = Factory.New<OrgHeader>();
			deltaOrg.OH_RL_NKClosestPort = "USATL";
			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "006";
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_RM_Airline = airline.PK;
			miscServ.OM_OH = deltaOrg.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "DL69";
			AssertEquals("Calculates country of Delta from MiscServ", "US", declaration.JE_RN_NKTransportNationality);
			declaration.JE_VoyageFlightNo = "QF69";
			AssertEquals("Without a MiscServ, falls-back to RefCountry", "AU", declaration.JE_RN_NKTransportNationality);
			declaration.JE_VoyageFlightNo = "AA69";
			AssertEquals("Without a MiscServ, falls-back to RefCountry, with a hack for 'USA'-->US", "US", declaration.JE_RN_NKTransportNationality);
		}

		public void TestDefermentApprovalNumber()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				var importer = Factory.New<OrgHeader>();
				var data = Factory.New<OrgCountryData>();
				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><VATDeferType>C</VATDeferType></ImportCustomsDefaultAddInfo>";
				data.OV_CustomsEconomicGroupAddInfo = "<CustomsEconomicGroupAddInfo><OtherDeferType>D</OtherDeferType></CustomsEconomicGroupAddInfo>";
				data.OV_OH_OrgHeader = importer.PK;
				data.OV_OA_ApprovedLocation = ZGuid.Empty;
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("C", declaration.ZG_VATDeferType);
				AssertEquals("D", declaration.JE_PaymentMethod);
			}
		}

		public void TestRepresentationType()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var organisation = Factory.New<OrgHeader>();
				var data = Factory.New<OrgCountryData>();
				data.OV_CustomsEconomicGroupAddInfo = "<CustomsEconomicGroupAddInfo><Box14UseIndirectRepresentation>Y</Box14UseIndirectRepresentation></CustomsEconomicGroupAddInfo>";
				data.OV_OH_OrgHeader = organisation.PK;
				data.OV_OA_ApprovedLocation = ZGuid.Empty;
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(RepresentationTypeList.Codes._3Indirect, declaration.JE_DeclarantType);

				data.OV_CustomsEconomicGroupAddInfo = "<CustomsEconomicGroupAddInfo><Box14UseIndirectRepresentation>N</Box14UseIndirectRepresentation></CustomsEconomicGroupAddInfo>";
				declaration.JE_OH_Importer = ZGuid.Empty;
				declaration.JE_OH_Supplier = organisation.PK;
				AssertEquals(RepresentationTypeList.Codes._3Indirect, declaration.JE_DeclarantType);

				data.OV_CustomsEconomicGroupAddInfo = "<CustomsEconomicGroupAddInfo><Box14UseIndirectRepresentation></Box14UseIndirectRepresentation></CustomsEconomicGroupAddInfo>";
				declaration.JE_DeclarantType = ZString.Empty;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_OH_Supplier = ZGuid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);
			}
		}

		public void TestClientDucrGenerationOptions()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Daniel", ""));
				FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Clarke", ""));

				var declaration = Factory.New<JobDeclaration>();
				declaration.DocsAndCartage.JP_CustomAttrib1 = "Daniel1";
				declaration.DocsAndCartage.JP_CustomAttrib2 = "Clarke1";

				var organisation = Factory.New<OrgHeader>();
				var data = Factory.New<OrgCountryData>();
				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo></ImportCustomsDefaultAddInfo>";
				data.OV_CustomsEconomicGroupAddInfo = "<CustomsEconomicGroupAddInfo></CustomsEconomicGroupAddInfo>";
				data.OV_OH_OrgHeader = organisation.PK;
				data.OV_OA_ApprovedLocation = ZGuid.Empty;
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;

				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(false, declaration.UseClientEoriForDucr);
				AssertEquals(ZString.Empty, declaration.ClientReferenceForDucr);

				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><Box44ClientsDucrSourceAttributeField>NON</Box44ClientsDucrSourceAttributeField><Box44UseClientsEoriForDucrs>Y</Box44UseClientsEoriForDucrs></ImportCustomsDefaultAddInfo>";
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(ZString.Empty, declaration.ClientReferenceForDucr);
				AssertEquals(true, declaration.UseClientEoriForDucr);

				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><Box44ClientsDucrSourceAttributeField>CA1</Box44ClientsDucrSourceAttributeField></ImportCustomsDefaultAddInfo>";
				declaration.UseClientEoriForDucr = false;
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals("Daniel1", declaration.ClientReferenceForDucr);

				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><Box44ClientsDucrSourceAttributeField>CA2</Box44ClientsDucrSourceAttributeField><Box44UseClientsEoriForDucrs>N</Box44UseClientsEoriForDucrs></ImportCustomsDefaultAddInfo>";
				declaration.UseClientEoriForDucr = false;
				declaration.ClientReferenceForDucr = ZString.Empty;
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals("Clarke1", declaration.ClientReferenceForDucr);
				AssertEquals(false, declaration.UseClientEoriForDucr);

				declaration.DocsAndCartage.JP_CustomAttrib2 = ZString.Empty;
				declaration.UseClientEoriForDucr = false;
				declaration.ClientReferenceForDucr = "";
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(ZString.Empty, declaration.ClientReferenceForDucr);
			}
		}

		public void TestDeclarantsReferenceSourceAttributeField()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Attr1", ""));
				FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Attr2", ""));

				var declaration = Factory.New<JobDeclaration>();
				declaration.DocsAndCartage.JP_CustomAttrib1 = "Attr1";
				declaration.DocsAndCartage.JP_CustomAttrib2 = "Attr2";

				var organisation = Factory.New<OrgHeader>();
				var data = Factory.New<OrgCountryData>();
				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo></ImportCustomsDefaultAddInfo>";
				data.OV_CustomsEconomicGroupAddInfo = "<CustomsEconomicGroupAddInfo></CustomsEconomicGroupAddInfo>";
				data.OV_OH_OrgHeader = organisation.PK;
				data.OV_OA_ApprovedLocation = ZGuid.Empty;
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;

				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(ZString.Empty, declaration.JE_OwnerRef);

				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><Box7DeclarantsReferenceSourceAttributeField>NON</Box7DeclarantsReferenceSourceAttributeField></ImportCustomsDefaultAddInfo>";
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(ZString.Empty, declaration.JE_OwnerRef);

				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><Box7DeclarantsReferenceSourceAttributeField>CA1</Box7DeclarantsReferenceSourceAttributeField></ImportCustomsDefaultAddInfo>";
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals("Attr1", declaration.JE_OwnerRef);

				data.OV_ImportCustomsDefaultAddInfo = "<ImportCustomsDefaultAddInfo><Box7DeclarantsReferenceSourceAttributeField>CA2</Box7DeclarantsReferenceSourceAttributeField></ImportCustomsDefaultAddInfo>";
				declaration.JE_OwnerRef = ZString.Empty;
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals("Attr2", declaration.JE_OwnerRef);

				declaration.DocsAndCartage.JP_CustomAttrib2 = ZString.Empty;
				declaration.JE_OwnerRef = ZString.Empty;
				declaration.JE_OH_Importer = Guid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(ZString.Empty, declaration.JE_OwnerRef);
			}
		}
	}
}
