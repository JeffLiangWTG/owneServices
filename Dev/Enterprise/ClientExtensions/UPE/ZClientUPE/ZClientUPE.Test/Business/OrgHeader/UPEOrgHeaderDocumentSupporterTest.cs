using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEOrgHeaderDocumentSupporter))]
	class UPEOrgHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] docWrappers = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Organisation, null);
			AssertEquals("Should return the correct type of doc wrapper", typeof(UPEDocOrganisation), docWrappers[0].GetType());
			DocumentWrapper intermediateWrapper = (DocumentWrapper)docWrappers[0].WrappedObject;
			AssertEquals("Should return the correct type of doc wrapper", Organisation.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<UPEOrgHeader>();
			orgHeader.OH_Code = "CODE1";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_IsForwarder = true;
			var newAppAgPorts = orgHeader.AppointedAgentPorts.AddNew();
			newAppAgPorts.O5_PortOrCountry = "AUSYD";
			newAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Published;
			newAppAgPorts.O5_OA_AgentOfficeAddress = orgHeader.MainAddress.PK;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CODE2";
			consignor.OH_IsConsignor = true;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CODE3";
			consignee.OH_IsConsignee = true;
			consignee.OH_RL_NKClosestPort = "AUSYD";
			var supplierLink = orgHeader.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;
			supplierLink.SelectedForPrinting = true;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";
			var buyerLink = orgHeader.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = consignee.PK;
			buyerLink.SelectedForPrinting = true;
			buyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";
			Factory.Save();
			return orgHeader;
		}

		UPEOrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<UPEOrgHeader>();
				}

				return fOrganisation;
			}
		}

		UPEOrgHeader fOrganisation;
		UPEOrgHeaderDocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new UPEOrgHeaderDocumentSupporter(Organisation);
				}

				return fDocumentSupporter;
			}
		}

		UPEOrgHeaderDocumentSupporter fDocumentSupporter;
		#endregion
	}
}
