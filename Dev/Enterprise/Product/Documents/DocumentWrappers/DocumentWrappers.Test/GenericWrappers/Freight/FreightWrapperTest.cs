using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapper))]
	public class FreightWrapperTest : GenericWrapperWithNotesTest
	{
		#region CFSTransport

		public virtual void TestArrivalCFSTransport()
		{
			AssertNull(Wrapper.ArrivalCFSTransport);
		}

		public virtual void TestDepartureCFSTransport()
		{
			AssertNull(Wrapper.DepartureCFSTransport);
		}

		#endregion

		#region TestClientBrandingUsesLocalClient

		public void TestClientBrandingUsesLocalClient()
		{
			TestClientBrandingUsesLocalClientCore();
		}

		#endregion

		#region TestClientBrandingUsesLocalClientCore

		protected virtual void TestClientBrandingUsesLocalClientCore()
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "123";
			jobHeader.JH_ParentID = WrappedBO.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = localClient.PK;

			if (Wrapper.Job == null)
			{
				Assert("Test not required as this wrapper does not have any related Job. (GetJob returns null)", true);
				return;
			}

			var company = GlbCompany.CurrentCompany;
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var assembly = Assembly.Load("Enterprise.Rating.Business");

			var tariff1 = Factory.New(assembly.GetType("Enterprise.Rating.Business.CompanyTariff"));
			tariff1["TH_GC"] = company.PK;
			tariff1["TH_RateType"] = "GLB";

			var tariff2 = Factory.New(assembly.GetType("Enterprise.Rating.Business.CompanyTariff"));

			var levels = new ClientTariffAndLevelCollection();
			tariff1["TH_GC"] = company.PK;
			tariff1["TH_RateType"] = "GLB";

			Factory.Save();

			var level1 = levels.AddNew();
			level1.Code = ((ICompanyTariff)tariff1).TH_GlobalRateLevel.ToString();
			level1.BrandName = "Google";
			level1.BrandEmailAddress = "google@cargowise.com";
			level1.Image = new Bitmap(1, 1);

			var level2 = levels.AddNew();
			level2.Code = ((ICompanyTariff)tariff2).TH_GlobalRateLevel.ToString();
			level2.BrandName = "Microsoft";
			level2.BrandEmailAddress = "microsoft@cargowise.com";
			level2.Image = new Bitmap(1, 1);

			DocumentsDataRegistry.Instance.ClientTariffAndLevels.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, levels);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "Google";
			consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			Factory.Save();

			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, "CNE");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, consignee.PK);
			Wrapper.SetTemplateConstants(constants);

			AssertEquals("wrapper.BrandName", "MICROSOFT", Wrapper.BrandName);
		}

		#endregion

		#region TestARInvoiceDocWrapperContext

		public void TestARInvoiceDocWrapperContext()
		{
			FreightWrapper wrapper = FreightWrapper.New(Factory.New<ARInvoice>(), Factory)[0];
			wrapper.SetReportNameForTesting("Test AR Report Name");
			AssertEquals(wrapper.ReportName, wrapper.ARInvoice.ReportName);
			AssertEquals(wrapper.ARInvoice.ReportName, "Test AR Report Name");
		}

		#endregion

		#region TestSalesRep

		public void TestSalesRep()
		{
			IJobHeaderParent headerParent = WrappedBO as IJobHeaderParent;

			if (headerParent == null)
			{
				var wrapper = GetNewDocumentWrapper() as FreightWrapper;
				AssertNotNull(wrapper);
				AssertNull(wrapper.SalesRep);
			}
			else
			{
				Job job;
				var forwardingConsol = WrappedBO as ForwardingConsol;
				if (forwardingConsol != null)
				{
					Factory.Save();
					job = new TestObjectCreator(Factory).CreateJobForLegacyGateway(forwardingConsol);
				}
				else
				{
					job = new Job.Loader(headerParent).TryLoadOrCreate();
				}
				job.JH_GS_NKRepSales = null;

				AssertNull("prerequisite", job.RepSales);
				var wrapper = FreightWrapper.New(WrappedBO, Factory)[0];
				AssertEquals(null, wrapper.SalesRep);

				GlbStaff staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AAA";
				staff.GS_FullName = "DDDD";
				job.JH_GS_NKRepSales = staff.GS_Code;

				AssertEquals("prerequisite", staff, job.RepSales);
				wrapper = FreightWrapper.New(WrappedBO, Factory)[0];
				AssertSalesRep(wrapper, staff);
			}
		}

		protected virtual void AssertSalesRep(FreightWrapper wrapper, GlbStaff salesRep)
		{
			if (wrapper.Job == null)
			{
				AssertNull(wrapper.GetType().Name + "does not have a Job", wrapper.SalesRep);
				return;
			}

			AssertEquals(salesRep, wrapper.SalesRep.WrappedObject);
		}

		#endregion

		#region TestCarrierIsNMFCEnabled

		protected virtual bool IsCarrierUsed
		{
			get { return true; }
		}

		protected virtual FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			return (FreightWrapper)GetNewDocumentWrapper();
		}

		public void TestCarrierIsNMFCEnabled()
		{
			if (IsCarrierUsed)
			{
				var wrapper = GetNewDocumentWrapperWithCarrier();
				AssertEquals("CarrierIsNMFCEnabled", false, wrapper.CarrierIsNMFCEnabled);
				var customCode = wrapper.Carrier.Organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.NMFCParticipant, OrgConstants.NMFCParticipantCodes.Code.No, "US");
				AssertEquals("CarrierIsNMFCEnabled", false, wrapper.CarrierIsNMFCEnabled);

				customCode.OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.Yes;
				AssertEquals("CarrierIsNMFCEnabled", true, wrapper.CarrierIsNMFCEnabled);
			}
			else
			{
				Assert("Carrier is not used, no need to test.", true);
			}
		}

		#endregion

		#region TestIBusinessObjectCollectionsDontReturnNull

		public void TestIBusinessObjectCollectionsDontReturnNull()
		{
			PropertyInfo[] properties = Wrapper.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			Array.Sort(properties, InfoComparison);

			string lastPropertyName = "";

			StringBuilder nullProperties = new StringBuilder();

			foreach (PropertyInfo info in properties)
			{
				if (lastPropertyName == info.Name)
				{
					continue;
				}

				if (!typeof(IBusinessObjectCollection).IsAssignableFrom(info.PropertyType))
				{
					continue;
				}

				if (!info.CanRead)
				{
					continue;
				}

				if (info.GetValue(Wrapper, null) == null)
				{
					nullProperties.AppendLine(info.Name);
				}

				lastPropertyName = info.Name;
			}

			const string message = @"
Collection properties should NEVER return null as this causes document engine to go belly up.
";

			AssertEquals(message, ZString.Empty, nullProperties.ToString());
		}

		static int InfoComparison(PropertyInfo info1, PropertyInfo info2)
		{
			int result = info1.Name.CompareTo(info2.Name);

			if (result == 0)
			{
				if (info1.DeclaringType == info2.DeclaringType)
				{
					result = 0;
				}
				else if (info1.DeclaringType.IsAssignableFrom(info2.DeclaringType))
				{
					result = 1;
				}
				else if (info2.DeclaringType.IsAssignableFrom(info1.DeclaringType))
				{
					result = -1;
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			return result;
		}

		#endregion

		#region TestCartageInfoDocWrapperContext

		public void TestCartageInfoDocWrapperContext()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertNotNull(wrapper);
			wrapper.SetDocumentDirectionForTesting("ARV");

			if (wrapper.CartageInfo == null)
			{
				Assert("Test not required as this wrapper does not have any CartageInfo. (GetCartageInfo returns null)", true);
				return;
			}

			AssertEquals("ARV", wrapper.CartageInfo.DocumentDirection);
		}

		#endregion

		#region TestJobHeaderLocalClient

		public virtual void TestJobHeaderLocalClient()
		{
			var freightWrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertNotNull(freightWrapper);
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName should be empty", "", freightWrapper.JobHeaderLocalClient.CompanyName);

			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WrappedBO.PK;
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.Addresses.AddNew(OrgAddressType.Office, true);
			orgHeader.OH_FullName = "LOCAL CLIENT COMPANY NAME";
			orgHeader.OH_Code = "OH1";
			job.LocalChargesPK = orgHeader.PK;

			freightWrapper = GetNewDocumentWrapper() as FreightWrapper;

			if (freightWrapper.Job == null)
			{
				AssertEquals("JobHeaderLocalClient", ZString.Empty, freightWrapper.JobHeaderLocalClient.ToString());
				return;
			}

			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME", freightWrapper.JobHeaderLocalClient.CompanyName);
		}

		#endregion

		#region TestJobHeaderBranchLogo

		public virtual void TestJobHeaderBranchLogo()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertNull("No branch logo", Wrapper.CompanyLogo);

			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WrappedBO.PK;
			job.JH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			AssertNull("No branch logo", Wrapper.CompanyLogo);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(1, 1));
			AssertEquals("Branch logo should be Company Logo", new Size(1, 1), Wrapper.CompanyLogo.Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, job.JH_GB.ToGuid(), Guid.Empty, new Bitmap(2, 2));

			if (Wrapper.Job == null)
			{
				AssertEquals("If GetJob returns null, logo will not be updated", new Size(1, 1), Wrapper.CompanyLogo.Size);
				return;
			}

			AssertEquals("Branch logo should be Controlling Branch Logo", new Size(2, 2), Wrapper.CompanyLogo.Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Bitmap(3, 3));
			SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Branch logo should be Login Branch Logo", new Size(3, 3), Wrapper.CompanyLogo.Size);
		}

		#endregion

		#region TestContainerLayoutStyle

		public virtual void TestContainerLayoutStyle()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertNotNull(wrapper);

			SetupBusinessObjectForContainerLayoutStyleTest(WrappedBO);

			AssertEquals("NoContainers", wrapper.ContainerLayoutStyle);

			CommonContainer container1 = AddContainer(WrappedBO);
			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			if (AddContainer(WrappedBO) == null)
			{
				AssertEquals("Containers cannot be added if AddContainer returns null", "NoContainers", Wrapper.ContainerLayoutStyle);
				return;
			}

			AssertEquals("MultipleContainersSingleDetails", wrapper.ContainerLayoutStyle);

			CommonContainer container2 = AddContainer(WrappedBO);
			container1.JC_JK = container2.JC_JK;

			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			AssertEquals("MultipleContainersSingleDetails: 2 Containers + Release Number, EmptyRequired & DepartureEstimatedPickup all empty.", "MultipleContainersSingleDetails",
				wrapper.ContainerLayoutStyle);

			container1.JC_ReleaseNum = "111111";

			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			AssertEquals("MultipleContainersSingleDetails: 2 Containers and only 1 with Release Number NOT empty.", "MultipleContainersSingleDetails", wrapper.ContainerLayoutStyle);
			container1.JC_ReleaseNum = ZString.Empty;  // Clear this value so the next assertion can test independently.

			container1.JC_EmptyRequired = ZDateTime.Today;

			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			AssertEquals("MultipleContainersSingleDetails: 2 Containers and only 1 with EmptyRequired NOT empty.", "MultipleContainersSingleDetails", wrapper.ContainerLayoutStyle);
			container1.JC_EmptyRequired = ZDateTime.Empty;  // Clear this value so the next assertion can test independently.

			container1.JC_DepartureEstimatedPickup = ZDateTime.Today;
			container2.JC_DepartureEstimatedPickup = ZDateTime.Empty;   // Clunky but but required as the line above sets the value for the second container too!

			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			AssertEquals("MultipleContainersSingleDetails: 2 Containers and only 1 with DepartureEstimatedPickup NOT empty.", "MultipleContainersSingleDetails", wrapper.ContainerLayoutStyle);

			container1.JC_ReleaseNum = "222222";
			container1.JC_EmptyRequired = ZDateTime.Today;

			container2.JC_ReleaseNum = "333333";

			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			AssertEquals("MultipleContainersMultipleDetails: 2 Containers, each with a Release Number.", "MultipleContainersMultipleDetails", wrapper.ContainerLayoutStyle);
			container2.JC_ReleaseNum = ZString.Empty;  // Clear this value so the next assertion can test independently.

			container2.JC_EmptyRequired = ZDateTime.Today;

			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			AssertEquals("MultipleContainersMultipleDetails: 2 Containers, each with EmptyRequired field set.", "MultipleContainersMultipleDetails", wrapper.ContainerLayoutStyle);
			container2.JC_EmptyRequired = ZDateTime.Empty;  // Clear this value so the next assertion can test independently.

			container2.JC_DepartureEstimatedPickup = ZDateTime.Now;

			wrapper = GetNewDocumentWrapper() as FreightWrapper;

			AssertEquals("MultipleContainersMultipleDetails: 2 Containers, each with DepartureEstimatedPickup field set.", "MultipleContainersMultipleDetails", wrapper.ContainerLayoutStyle);
		}

		protected virtual void SetupBusinessObjectForContainerLayoutStyleTest(BusinessObject parent)
		{
		}

		protected virtual CommonContainer AddContainer(BusinessObject parent) => null;

		#endregion

		#region TestExchangeRates

		public void TestExchangeRates()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			FreightWrapper[] wrappers = FreightWrapper.New(shipment, Factory);

			AssertEquals(1, wrappers.Length);
			AssertNotNull(wrappers[0].ExchangeRates);
			AssertEquals(0, wrappers[0].ExchangeRates.Count);

			Job job = Factory.NewJobForTesting<Job>();
			ExchangeRate exchangeRate = job.ExchangeRates.AddNew();
			exchangeRate.JF_JH = job.PK;
			exchangeRate.JF_RX_NKRateCurrency = "AUD";
			exchangeRate.JF_BaseRate = 2.345m;
			shipment = Factory.New<ForwardingShipment>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			wrappers = FreightWrapper.New(shipment, Factory);

			AssertEquals(1, wrappers.Length);
			AssertEquals(1, wrappers[0].ExchangeRates.Count);
			AssertEquals("AUD", wrappers[0].ExchangeRates[0].Currency.Code);
			AssertEquals(2.345m, wrappers[0].ExchangeRates[0].BuyRate);
		}

		#endregion

		#region TestCosts

		public void TestCosts()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightWrapper[] wrappers = FreightWrapper.New(consol, Factory);
			JobConsolCostCollection costs = new JobConsolCostCollection(Factory, consol);

			JobConsolCost cost1 = costs.TryAddNew();
			JobConsolCost cost2 = costs.TryAddNew();
			cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost1.E6_RX_NKCurrency = Constants.CurrencyCodes.Australia;
			cost1.E6_OSCostAmount = 12.00m;
			cost2.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost2.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			cost2.E6_OSCostAmount = 46.50m;

			AssertEquals("Costs.Count: 2", 2, wrappers[0].Costs.Count);
			AssertEquals("Costs[0]: Populated", cost1, wrappers[0].Costs[0].WrappedObject);
			AssertEquals("Costs[1]: Populated", cost2, wrappers[0].Costs[1].WrappedObject);
			AssertEquals("Costs[0].CostAmount: 12.00", cost1.E6_OSCostAmount, wrappers[0].Costs[0].OSCost.Amount);
			AssertEquals("Costs[1].Currency: USD", cost2.E6_RX_NKCurrency, wrappers[0].Costs[1].OSCost.Currency.Code);
		}

		#endregion

		#region TestCallingStaticNewInBaseReturnsConcreteWrapper

		public void TestCallingStaticNewInBaseReturnsConcreteWrapper()
		{
			FreightWrapper[] wrappers = FreightWrapper.New(WrappedBO, Factory);
			AssertNotNull("Wrapper from calling Static New in the base should be included.", wrappers);
			Assert("There should be wrappers in the array for testing.", wrappers.Length > 0);
		}

		#endregion

		#region TestOrgWrappersReturnTypesOnEmptyWrapper

		public virtual void TestOrgWrappersReturnTypesOnEmptyWrapper()
		{
			AssertOrgWrappersReturnRightTypes(Wrapper);
		}

		#endregion

		#region TestConsignorWrapperRepondsToRegistrySetting

		public void TestConsignorWrapperRepondsToRegistrySetting()
		{
			Enterprise.Registry.Business.FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Burritos!");
			AssertOrgWrapperTypeSetTo(Wrapper.Consignor, "Burritos!");
		}

		#endregion

		#region TestBarcodeTextForFont

		public void TestBarcodeTextForFont()
		{
			SetJobNumberForBarcodeTesting(WrappedBO);
			var wrapper = GetNewDocumentWrapper();
			var docTypeCode = wrapper as IDocTypeCode;
			if (docTypeCode != null)
			{
				docTypeCode.DocTypeCode = "CAD";
			}
			AssertEquals("By default the Barcode text should contain DocTypeCode, DocManagerCode and JobNumber. This allows the document to be scanned into ediEnterprise and automatically assigned to the associated Jobs eDocs.", ExpectedBarcodeText(), wrapper.BarcodeTextForFont);
		}

		protected virtual void SetJobNumberForBarcodeTesting(BusinessObject bizO)
		{
		}

		protected virtual string ExpectedBarcodeText()
		{
			return "";
		}

		#endregion

		#region JobNumberBarcode

		public void TestJobNumberBarcodeText()
		{
			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MASTERY0987235";

			var shipment = newFactory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "BILLY0987235";

			newFactory.Save();

			var shipmentWrapper = FreightWrapper.New(shipment, Factory)[0];
			var docTypeCode = shipmentWrapper as IDocTypeCode;
			if (docTypeCode != null)
			{
				docTypeCode.DocTypeCode = "CAD";
			}

			Assert("Expected to have created a JobNumberBarcode which sets JobNumberBarcodeText", !shipmentWrapper.JobNumberBarcodeText.IsEmpty);
			AssertContains("Expected to contain the unique shipment number", shipment.JS_UniqueConsignRef, shipmentWrapper.JobNumberBarcodeText);
			AssertNotContains("Expected not to match the house bill number", shipment.JS_HouseBill, shipmentWrapper.JobNumberBarcodeText);
			AssertEquals("JobNumberBarcode haves the ; at the end before the |.", "^SHP=S00001000;CAD;|", shipmentWrapper.JobNumberBarcodeText);

			var consolWrapper = FreightWrapper.New(consol, Factory)[0];
			docTypeCode = consolWrapper;
			if (docTypeCode != null)
			{
				docTypeCode.DocTypeCode = "CAD";
			}

			Assert("Expected to have created a JobNumberBarcode which sets JobNumberBarcodeText", !consolWrapper.JobNumberBarcodeText.IsEmpty);
			AssertContains("Expected to contain the unique consol number", consol.JK_UniqueConsignRef, consolWrapper.JobNumberBarcodeText);
			AssertNotContains("Expected to not contain the master bill number", consol.JK_MasterBillNum, consolWrapper.JobNumberBarcodeText);
			AssertEquals("JobNumberBarcode haves the ; at the end before the |.", "^CON=C00001000;CAD;|", consolWrapper.JobNumberBarcodeText);
		}

		#endregion

		#region IncludingInDocPack

		public void TestIExcludedFromDocPackByDefault()
		{
			var invoice = Factory.New<ARInvoice>();
			var wrapper = FreightWrapper.New(invoice, Factory).FirstOrDefault();

			Assert(!invoice.IsCancelled);
			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("FreightWrapper should be returning the ARInvoice's value", ((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded, ((IShouldExcludeFromDocPackByDefault)wrapper.ARInvoice).IsExcluded);
			Assert("A Non-reversed ARInvoice should be included by default", !((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);
			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("FreightWrapper should be returning the ARInvoice's value", ((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded, ((IShouldExcludeFromDocPackByDefault)wrapper.ARInvoice).IsExcluded);
			Assert("Should always be included if the registry item is true", !((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);

			var reversing = new ARInvoiceReversing(invoice);
			reversing.Reverse();

			AssertEquals("FreightWrapper should be returning the ARInvoice's value", ((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded, ((IShouldExcludeFromDocPackByDefault)wrapper.ARInvoice).IsExcluded);
			Assert("Should always be included if the registry item is true", !((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);
			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("FreightWrapper should be returning the ARInvoice's value", ((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded, ((IShouldExcludeFromDocPackByDefault)wrapper.ARInvoice).IsExcluded);
			Assert("A Reversed ARInvoice should not be included by default", ((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);
		}

		#endregion

		#region AssertOrgWrappersReturnRightTypes

		protected void AssertOrgWrappersReturnRightTypes(FreightWrapper wrapper)
		{
			AssertOrgWrapperTypeSetTo(wrapper.JobHeaderLocalClient, "Local Client");
			AssertOrgWrapperTypeSetTo(wrapper.AssuredParty, "Assured Party");
			AssertOrgWrapperTypeSetTo(wrapper.Buyer, "Buyer");
			AssertOrgWrapperTypeSetTo(wrapper.Carrier, ExpectedCarrierTypeDescription);
			AssertOrgWrapperTypeSetTo(wrapper.ClaimsPayableBy, "Claims Payable By");
			AssertOrgWrapperTypeSetTo(wrapper.Consignee, "Consignee");
			AssertOrgWrapperTypeSetTo(wrapper.Consignor, ExpectedConsignorTypeDescription);
			AssertOrgWrapperTypeSetTo(wrapper.ConsolCreditor, "Consol Creditor");
			AssertOrgWrapperTypeSetTo(wrapper.DeliveryAgent, "Delivery Agent");
			AssertOrgWrapperTypeSetTo(wrapper.ExportAgent, "Export Agent");
			AssertOrgWrapperTypeSetTo(wrapper.ExportBroker, "Export Broker");
			AssertOrgWrapperTypeSetTo(wrapper.ImportAgent, "Import Agent");
			AssertOrgWrapperTypeSetTo(wrapper.ImportBroker, "Import Broker");
			AssertOrgWrapperTypeSetTo(wrapper.InsuredBy, "Insured By");
			AssertOrgWrapperTypeSetTo(wrapper.NotifyParty, "Notify Party");
			AssertOrgWrapperTypeSetTo(wrapper.PickupAgent, "Pickup Agent");
			AssertOrgWrapperTypeSetTo(wrapper.SurveyReportParty, "Survey Report Party");
			AssertEquals("incorrecttype", true, wrapper.ExportAgent == null || wrapper.ExportAgent is ExportAgentOrganisationWrapper);
			AssertOrgWrapperTypeSetTo(wrapper.BookingParty, "Booking Party");
			AssertOrgWrapperTypeSetTo(wrapper.ReceivingForwarder, "Receiving Forwarder");
			AssertOrgWrapperTypeSetTo(wrapper.SendingForwarder, "Sending Forwarder");
			AssertOrgWrapperTypeSetTo(wrapper.ControllingCustomer, "Controlling Customer");
			AssertOrgWrapperTypeSetTo(wrapper.ControllingAgent, "Controlling Agent");
			AssertOrgWrapperTypeSetTo(wrapper.CarrierBookingAgent, "Carrier Booking Agent");
			AssertOrgWrapperTypeSetTo(wrapper.CarrierHandlingAgent, "Carrier Handling Agent");
		}

		void AssertOrgWrapperTypeSetTo(OrganisationWrapper organisationWrapper, ZString typeDescription)
		{
			if (organisationWrapper != null)
			{
				AssertEquals("organisationWrapper.TypeDescription", typeDescription, organisationWrapper.TypeDescription);
			}
		}

		protected virtual ZString ExpectedCarrierTypeDescription
		{
			get { return "Carrier"; }
		}

		protected virtual ZString ExpectedConsignorTypeDescription
		{
			get { return "Consignor"; }
		}

		#endregion

		#region TestTransportReferenceHeading

		public void TestTransportReferenceHeading()
		{
			AssertEquals(TransportReferenceHeadingToTest, Wrapper.TransportReferenceHeading);
		}

		#region TransportReferenceHeadingToTest

		protected virtual string TransportReferenceHeadingToTest
		{
			get
			{
				return "REFERENCE";
			}
		}

		#endregion

		#endregion

		#region TestWarehouseAreaWrapperProperties

		public void TestWarehouseAreaWrapperProperties()
		{
			AssertEquals(ExpectedWarehouseName, Wrapper.WarehouseName);
			AssertEquals(ExpectedAreaName, Wrapper.AreaName);
			AssertEquals(ExpectedAreaBarcode, Wrapper.AreaBarcode);
			AssertEquals(ExpectedWarehouseNextDischargePort, Wrapper.WarehouseNextDischargePort);
		}

		protected virtual ZString ExpectedWarehouseName { get { return ZString.Empty; } }
		protected virtual ZString ExpectedAreaName { get { return ZString.Empty; } }
		protected virtual ZString ExpectedAreaBarcode { get { return ZString.Empty; } }
		protected virtual ZString ExpectedWarehouseNextDischargePort { get { return ZString.Empty; } }

		#endregion

		#region TestEmptyOrNull 

		public override void TestWrapperNotes()
		{
			AssertEquals("Notes", 0, Wrapper.Notes.Count);
		}

		public override void TestWrapperNotesIncludingRelated()
		{
			AssertEquals("Notes", 0, Wrapper.NotesIncludingRelated.Count);
		}

		public virtual void TestInsuranceRelatedAddressFields()
		{
			if (Wrapper.InsuredBy != null)
			{
				AssertEquals("InsuredBy", ZString.Empty, Wrapper.InsuredBy.ToString());
			}
			if (Wrapper.AssuredParty != null)
			{
				AssertEquals("AssuredParty", ZString.Empty, Wrapper.AssuredParty.ToString());
			}
			if (Wrapper.ClaimsPayableBy != null)
			{
				AssertEquals("ClaimsPayableBy", ZString.Empty, Wrapper.ClaimsPayableBy.ToString());
			}
			if (Wrapper.SurveyReportParty != null)
			{
				AssertEquals("SurveyReportParty", ZString.Empty, Wrapper.SurveyReportParty.ToString());
			}
		}

		public virtual void TestBuyer()
		{
			if (Wrapper.Buyer == null)
			{
				Assert("Test not required as this wrapper does not have any related Buyer. (GetBuyer returns null)", true);
				return;
			}
			AssertEquals("Buyer", ZString.Empty, Wrapper.Buyer.ToString());
		}

		public virtual void TestRecommendedAgent()
		{
			AssertNull(Wrapper.RecommendedAgent);
		}

		public virtual void TestTrackingBusinessObjectPK()
		{
			var defaultFormattingWrapper = (FreightWrapper)GetSetupWrapperForDefaultFormatting();
			AssertEquals("Remember to override this test if GetTrackingBusinessObjectPK is overridden", ZGuid.Empty, defaultFormattingWrapper.TrackingBusinessObjectPK);
		}

		#endregion

		#region TestUOMTypeNumber

		public void TestUOMTypeNumber()
		{
			AssertEquals(0, Wrapper.UOMTypeNumber);
		}

		#endregion

		#region TestUOMTypeTotal

		public void TestUOMTypeTotal()
		{
			AssertEquals(0, Wrapper.UOMTypeTotal);
		}

		#endregion

		#region TestFormattedTotalCO2e

		public virtual void TestFormattedTotalCO2e()
		{
			AssertEquals(ZString.Empty, Wrapper.FormattedTotalCO2e);
		}

		#endregion

		#region TestCO2eCalculationDate

		public virtual void TestCO2eCalculationDate()
		{
			AssertEquals(ZDateTime.Empty, Wrapper.CO2eCalculationDate);
		}

		#endregion

		#region DocJobChargeCollections

		public void TestFreightJobsChargesForLocalClientIsCachedOnFactoryLevel()
		{
			var freightWrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertNotNull(freightWrapper);

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WrappedBO.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = orgHeader.PK;

			if (freightWrapper.Job == null)
			{
				Assert("Test not required as this wrapper does not have any related Job. (GetJob returns null)", true);
				return;
			}

			var collection = freightWrapper.FreightJobsChargesForLocalClient;
			AssertNotNull("FreightJobsChargesForLocalClient", collection);

			var newFreightWrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertNotSame("Cached on Factory level by DocWrapper PK", collection, newFreightWrapper.FreightJobsChargesForLocalClient);

			collection = newFreightWrapper.FreightJobsChargesForLocalClient;
			AssertSame("Value is cached by Factory", collection, newFreightWrapper.FreightJobsChargesForLocalClient);

			collection = newFreightWrapper.FreightJobsChargesForLocalClient;
			Factory.ClearCachedValue<DocJobChargeCollection>(newFreightWrapper.PK.ToStringKey() + "FreightJobsChargesForLocalClient");
			AssertNotSame("New value is created after resetting Factory cached value", collection, newFreightWrapper.FreightJobsChargesForLocalClient);
		}

		public void TestFreightJobsChargesForOverseasAgentIsCachedOnFactoryLevel()
		{
			var freightWrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertNotNull(freightWrapper);

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WrappedBO.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = orgHeader.PK;

			if (freightWrapper.Job == null)
			{
				Assert("Test not required as this wrapper does not have any related Job. (GetJob returns null)", true);
				return;
			}

			var collection = freightWrapper.FreightJobsChargesForOverseasAgent;
			AssertNotNull("FreightJobsChargesForLocalClient", collection);

			var newFreightWrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertNotSame("Cached on Factory level by DocWrapper PK", collection, newFreightWrapper.FreightJobsChargesForOverseasAgent);

			collection = newFreightWrapper.FreightJobsChargesForOverseasAgent;
			AssertSame("Value is cached by Factory", collection, newFreightWrapper.FreightJobsChargesForOverseasAgent);

			collection = newFreightWrapper.FreightJobsChargesForOverseasAgent;
			Factory.ClearCachedValue<DocJobChargeCollection>(newFreightWrapper.PK.ToStringKey() + "FreightJobsChargesForOverseasAgent");
			AssertNotSame("New value is created after resetting Factory cached value", collection, newFreightWrapper.FreightJobsChargesForOverseasAgent);
		}

		#endregion

		#region TestPriority

		public void TestPriority()
		{
			var freightWrapper = GetNewDocumentWrapper() as FreightWrapper;
			AssertEquals((ZByte)0, freightWrapper.Priority);
		}

		#endregion

		#region Implementation

		public sealed override void TestWrapperMappingsEmpty()
		{
			var defaultFormattingWrapper = GetSetupWrapperForDefaultFormatting();
			Type typeOfWrapper = defaultFormattingWrapper.GetType();

			var defaultFormattingWrapperIZTypePropertiesImplementation = typeOfWrapper.GetProperties()
				.Where(propertyInfo =>
					typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType)
						&& (propertyInfo.DeclaringType == typeOfWrapper || propertyInfo.DeclaringType == typeof(FreightWrapper))
						&& !PropertiesToExcludeFromWrapperMappingsEmptyTest.Contains(propertyInfo.Name))
				.ToDictionary(property => property.Name, property => property.GetValue(defaultFormattingWrapper).ToString());

			var expectedIZTypePropertiesImplementation = DefaultValuesOfIZTypeProperties;
			foreach (string propertyName in OverriddenValuesOfIZTypeProperties.Keys)
			{
				expectedIZTypePropertiesImplementation[propertyName] = OverriddenValuesOfIZTypeProperties[propertyName];
			}

			AssertContainsExactElementsInAnyOrder("Some of IZType properties of SetupWrapperForDefaultFormatting returned unexpected results",
				expectedIZTypePropertiesImplementation, defaultFormattingWrapperIZTypePropertiesImplementation);
		}

		List<string> PropertiesToExcludeFromWrapperMappingsEmptyTest
		{
			get
			{
				return new List<string>
				{
					"DocumentMenuItemPK",
					"PK",
					"TrackingBusinessObjectPK"
				};
			}
		}

		Dictionary<string, string> DefaultValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "ActualReceive", ZString.Empty },
					{ "AdditionalTerms", ZString.Empty },
					{ "ApprovalNumber", ZString.Empty },
					{ "AreaBarcode", ZString.Empty },
					{ "AreaName", ZString.Empty },
					{ "ArrivalReference", ZString.Empty },
					{ "AWBSecurityInspectionStatus", ZString.Empty },
					{ "BookingReference", ZString.Empty },
					{ "BillingDate", ZDateTime.Empty.ToString() },
					{ "CACarrierName", ZString.Empty },
					{ "CAPreviousCCN", ZString.Empty },
					{ "CargoControlNumberForCanada", ZString.Empty },
					{ "CarrierIsNMFCEnabled", ZBool.False.ToString() },
					{ "CATransactionNo", ZString.Empty },
					{ "CAUSPortOfExit", ZString.Empty },
					{ "ConNote", ZString.Empty },
					{ "ConsigneeRequiredTaxNumber", ZString.Empty },
					{ "MasterBillConsigneeOverrideRequiredTaxNumber", ZString.Empty },
					{ "ConsignorRequiredTaxNumber", ZString.Empty },
					{ "MasterBillShipperOverrideRequiredTaxNumber", ZString.Empty },
					{ "ConsolAgentsReference", ZString.Empty },
					{ "ConsolDateCreated", ZString.Empty },
					{ "ConsolNumber", ZString.Empty },
					{ "ConsolPaymentType", ZString.Empty },
					{ "ConsolReference", ZString.Empty },
					{ "ContainerCount", ZDecimal.Zero.ToString() },
					{ "ContainerLayoutStyle", "NoContainers" },
					{ "ContainerSummary", ZString.Empty },
					{ "CTOArrivalBerth", ZString.Empty },
					{ "CustomAttribute1", ZString.Empty },
					{ "CustomAttribute2", ZString.Empty },
					{ "CustomDate1", ZString.Empty },
					{ "CustomDate2", ZString.Empty },
					{ "CustomDecimal1", ZDecimal.Zero.ToString() },
					{ "CustomDecimal2", ZDecimal.Zero.ToString() },
					{ "FormattedTotalCO2e", ZString.Empty },
					{ "CO2eCalculationDate", ZDateTime.Empty.ToString() },
					{ "CustomerReference", ZString.Empty },
					{ "CustomerReferenceNumber", ZString.Empty },
					{ "CustomFlag1", ZBool.False.ToString() },
					{ "CustomFlag2", ZBool.False.ToString() },
					{ "CustomsEntryNumber", ZString.Empty },
					{ "DeliveryDueDate", ZString.Empty },
					{ "RevisedDeliveryDueDate", ZString.Empty },
					{ "DeliveryActual", ZString.Empty },
					{ "DeliveryCartageAdvised", ZString.Empty },
					{ "DeliveryEstimated", ZString.Empty },
					{ "DeliveryFrom", ZString.Empty },
					{ "DeliveryGoodsDelivered", ZString.Empty },
					{ "DeliveryRequiredBy", ZString.Empty },
					{ "DepartureOrArrivalText", ZString.Empty },
					{ "DocumentNumber", ZDecimal.Zero.ToString() },
					{ "DocumentTotal", ZDecimal.Zero.ToString() },
					{ "UOMTypeNumber", ZDecimal.Zero.ToString() },
					{ "UOMTypeTotal", ZDecimal.Zero.ToString() },
					{ "TransportZone", ZString.Empty },
					{ "EFreightStatus", ZString.Empty },
					{ "ExportAgentRequiredTaxNumber", ZString.Empty },
					{ "ExportAgentsReference", ZString.Empty },
					{ "ExWorksRequiredBy", ZString.Empty },
					{ "FactoryEx", ZString.Empty },
					{ "FreightDepotType", ZString.Empty },
					{ "FullCartageInstructions", ZString.Empty },
					{ "FullHandlingInstructions", ZString.Empty },
					{ "GoodsDescription", ZString.Empty },
					{ "GoodsHandlingInstructions", ZString.Empty },
					{ "GateInTime", ZString.Empty },
					{ "GateOutTime", ZString.Empty },
					{ "HasTACImage", ZBool.False.ToString() },
					{ "Hazardous", ZBool.False.ToString() },
					{ "HBLContainerMode", ZString.Empty },
					{ "HouseBill", ZString.Empty },
					{ "HouseBillHeading", ZString.Empty },
					{ "HouseBillIssue", ZString.Empty },
					{ "ImportAgentRequiredTaxNumber", ZString.Empty },
					{ "ImportAgentsReference", ZString.Empty },
					{ "IsAuthorisedToLeave", ZBool.False.ToString() },
					{ "IsDomestic", ZBool.False.ToString() },
					{ "IsPODRequired", ZBool.False.ToString() },
					{ "JobNumber", ZString.Empty  },
					{ "JobNumberBarcodeText", ZString.Empty },
					{ "JobNumberBarcodeTextForFont", ZString.Empty },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", ZString.Empty },
					{ "JobNumberHeading", ZString.Empty },
					{ "LoadingMeters", ZDecimal.Zero.ToString() },
					{ "LocalForwarderReference", ZString.Empty },
					{ "MarksAndNumbers", ZString.Empty },
					{ "MasterBill", ZString.Empty },
					{ "MasterBillHeading", ZString.Empty },
					{ "MasterBillIssue", ZString.Empty },
					{ "NCTSDepartureTransportID", ZString.Empty },
					{ "NCTSFrontierTransportID", ZString.Empty },
					{ "NCTSGoodsLocationCode", ZString.Empty },
					{ "NCTSGoodsLocation", ZString.Empty },
					{ "NoCopyBills", ZDecimal.Zero.ToString() },
					{ "NoOriginalBills", ZDecimal.Zero.ToString() },
					{ "NotClearedByAgentExpiryDate", ZString.Empty },
					{ "NotClearedByAgentIssueDate", ZString.Empty },
					{ "NotClearedByAgentNumber", ZString.Empty },
					{ "NotClearedByAgentStatement", ZString.Empty },
					{ "NotifyPartyRequiredTaxNumber", ZString.Empty },
					{ "OrderDate", ZString.Empty },
					{ "OrderNumbersWithOwnersReference", ZString.Empty },
					{ "OrderTrackingNumber", ZString.Empty },
					{ "OtherReferences", ZString.Empty },
					{ "OwnerReference", ZString.Empty },
					{ "Overs", ZDecimal.Zero.ToString() },
					{ "PackagesDetails", ZString.Empty },
					{ "PickupActual", ZString.Empty },
					{ "PickupCartageAdvised", ZString.Empty },
					{ "PickupDateOfReceipt", ZString.Empty },
					{ "PickupTruckWaitCharge", ZDecimal.Zero.ToString() },
					{ "PickupTruckWaitTime", ZString.Empty },
					{ "PickupFrom", ZString.Empty },
					{ "PickupGoodsPickedup", ZString.Empty },
					{ "PickupInterimReceipt", ZString.Empty },
					{ "PickupLabourCharge", ZDecimal.Zero.ToString() },
					{ "PickupLabourTime", ZString.Empty },
					{ "PickupRequiredBy", ZString.Empty },
					{ "PreviousCargoControlNumberForCanada", ZString.Empty },
					{ "Priority", ZByte.Zero.ToString() },
					{ "QuoteNumber", ZString.Empty },
					{ "Refrigerated", ZBool.False.ToString() },
					{ "SecondaryHeading", ZString.Empty },
					{ "SecondaryNumber", ZString.Empty },
					{ "ShipmentDateCreated", ZString.Empty },
					{ "ShippedOnBoardDate", ZString.Empty },
					{ "ShippersReference", ZString.Empty },
					{ "Shorts", ZDecimal.Zero.ToString() },
					{ "StorageFromDate", ZString.Empty },
					{ "StorageToDate", ZString.Empty },
					{ "TransportReference", ZString.Empty },
					{ "TransportReferenceHeading", "REFERENCE" },
					{ "UnAllocatedPackages", ZDecimal.Zero.ToString() },
					{ "UnAllocatedVolume", ZDecimal.Zero.ToString() },
					{ "UnAllocatedWeight", ZDecimal.Zero.ToString() },
					{ "VendorID", ZString.Empty },
					{ "WarehouseLocation", ZString.Empty },
					{ "WarehouseName", ZString.Empty },
					{ "UNDGsSummary", ZString.Empty },
					{ "HouseACIDNo", ZString.Empty },
					{ "CarrierContractNumber", ZString.Empty },
					{ "WarehouseNextDischargePort", ZString.Empty },
					{ "DateOfFirstArrival", ZString.Empty },
					{ "WarehouseReleaseDate", ZString.Empty }
				};
			}
		}

		protected virtual Dictionary<string, string> OverriddenValuesOfIZTypeProperties => new Dictionary<string, string>();

		protected sealed override ZString ExpectedDefaultFormatting
		{
			get
			{
				return GetExpectedDefaultFormattingMergedWithOverriddenOne();
			}
		}

		ZString GetExpectedDefaultFormattingMergedWithOverriddenOne()
		{
			if (OverriddenExpectedDefaultFormatting.IsEmpty)
			{
				return FreightWrapperExpectedDefaultFormatting;
			}

			var overriddenFormatting = OverriddenExpectedDefaultFormatting.TrimStart().Split('\n')
				.ToDictionary(line => line.Substring(0, line.IndexOf(" ")));

			var defaultFormatting = FreightWrapperExpectedDefaultFormatting.TrimStart().Split('\n')
				.ToDictionary(line => line.Substring(0, line.IndexOf(" ")));

			foreach (var overriddenLine in overriddenFormatting)
			{
				defaultFormatting[overriddenLine.Key] = overriddenLine.Value;
			}

			return string.Join("\n", defaultFormatting.Values);
		}

		ZString FreightWrapperExpectedDefaultFormatting
		{
			get
			{
				return @" 
AccreditationAttempt :  is null
ArrivalCFSTransport :  is null
AssuredParty : 
BookingParty : 
Buyer : 
CaratagePickupMode : 
Carrier : 
CarrierAccount :  is null
CarrierBookingAgent : 
CarrierHandlingAgent : 
CarrierServiceLevel : 
CartageInfo : (No Default Field Value Available on CartageInfo)
ChargeableWeight : 
ClaimsPayableBy : 
Client : 
CollectAmount : 
Consignee : 
Consignor : 
ConsolContainerMode : 
ConsolCreditor : 
Consolidator : 
ConsolTransportMode : 
ConsolType : 
ContainerYardEmptyPickupAddress : 
ContainerYardEmptyReturnAddress : 
ControllingAgent : 
ControllingCustomer : 
CTOArrival : 
DeliveryAddress : 
DeliveryAgent : 
DeliveryLocation : 
DepartureCFSTransport :  is null
Destination : 
ExportAgent : 
ExportBroker : 
ExportReceivalAddress : 
ExportReceivingCTOAddress : 
ExportReceivingDepotAddress : 
FirstForeignPort : 
FreightPayableAt : 
FreightRate : 
GateTransport : 
GoodsAvailableAt : 
GoodsValue : 
ImportAgent : 
ImportArrivalCTOAddress : 
ImportBroker : 
IncoTerm : 
InspectionType : 
InsuranceValue : 
InsuredBy : 
InterestedRoute : 
InvoicingJob :  is null
JobHeaderLocalClient : 
LastForeignPort : 
LocalForwarder : 
MainShipToParty : 
MasterBillConsigneeOverride : 
MasterBillShipperOverride : 
NCTSDeclarationType : 
NCTSDepartureOffice : 
NCTSDepartureTransportCountry : 
NCTSDepartureTransportMode : 
NCTSDestinationOffice : 
NCTSFrontierTransportCountry : 
NCTSFrontierTransportMode : 
NotifyParty : 
NotifyParty2 : 
NotifyParty3 : 
OrderTransportMode : 
OrgDocument :  is null
Origin : 
ParentJob : 
Person :  is null
PickupAddress : 
PickupAgent : 
PickupCFSAddress : 
PickupLocation : 
PortOfFirstArrival : 
Principal : 
QueryClaim :  is null
Rating :  is null
ReceivingForwarder : 
RecommendedAgent :  is null
Registry : (No Default Field Value Available on Registry)
ReleaseType : 
RunSheet :  is null
SalesRelations :  is null
SalesRep :  is null
SellingParty : 
SendingForwarder : 
ServiceLevel : 
ShipmentContainerMode : 
ShipmentInnerPacksQty : 
ShipmentOuterPacksQty : 
ShipmentStatus : 
ShipmentTransportMode : 
ShipmentType : 
ShippedOnBoardType : 
StorageTime : 
StuffingLocation : 
Supplier : 
SupplierBuyerLink :  is null
SurveyReportParty : 
TranshipmentFreightConsol :  is null
TransitJobTransportMode : 
UnpackCFSAddress : 
Volume : 
WarehouseJob :  is null
Weight : ";
			}
		}

		protected virtual ZString OverriddenExpectedDefaultFormatting => ZString.Empty;

		protected new FreightWrapper Wrapper
		{
			get { return (FreightWrapper)base.Wrapper; }
		}

		protected BusinessObject WrappedBO
		{
			get
			{
				if (fWrappedBO == null)
				{
					fWrappedBO = GetNewBusinessObjectToWrap();
				}
				return fWrappedBO;
			}
		}
		BusinessObject fWrappedBO;

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return FreightWrapper.New(WrappedBO, Factory)[0];
		}

		protected virtual BusinessObject GetNewBusinessObjectToWrap() => Factory.New<DummyEnterpriseBusinessObject>();

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			base.SetUp();
		}

		#endregion

		#region ExpectedFieldMap

		// You must not add new public properties on concrete implementations of the FreightWrapper.
		// Please do not unseal this without discussing with Zubin Appoo AND Ben Govett first.
		protected sealed override string ExpectedFieldMap
		{
			get
			{
				return @"
Freight                                     (Default Field: JobNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
AccreditationAttempt                    AccreditationAttempt
ContainerYardEmptyPickupAddress         Address
ContainerYardEmptyReturnAddress         Address
DeliveryAddress                         Address
ExportReceivalAddress                   Address
ExportReceivingCTOAddress               Address
ExportReceivingDepotAddress             Address
GoodsAvailableAt                        Address
ImportArrivalCTOAddress                 Address
PickupAddress                           Address
PickupCFSAddress                        Address
UnpackCFSAddress                        Address
CarrierServiceLevel                     CarrierServiceLevel
CartageInfo                             CartageInfo
CaratagePickupMode                      CodeAndDescription
ConsolContainerMode                     CodeAndDescription
ConsolTransportMode                     CodeAndDescription
ConsolType                              CodeAndDescription
InspectionType                          CodeAndDescription
NCTSDeclarationType                     CodeAndDescription
NCTSDepartureOffice                     CodeAndDescription
NCTSDepartureTransportCountry           CodeAndDescription
NCTSDepartureTransportMode              CodeAndDescription
NCTSDestinationOffice                   CodeAndDescription
NCTSFrontierTransportCountry            CodeAndDescription
NCTSFrontierTransportMode               CodeAndDescription
OrderTransportMode                      CodeAndDescription
ReleaseType                             CodeAndDescription
ServiceLevel                            CodeAndDescription
ShipmentContainerMode                   CodeAndDescription
ShipmentStatus                          CodeAndDescription
ShipmentTransportMode                   CodeAndDescription
ShipmentType                            CodeAndDescription
ShippedOnBoardType                      CodeAndDescription
TransitJobTransportMode                 CodeAndDescription
OrgDocument                             DocOrganisation
ExportAgent                             ExportAgentOrganisation
SendingForwarder                        ExportAgentOrganisation
ParentJob                               Freight
TranshipmentFreightConsol               Freight
GateTransport                           GateTransport
IncoTerm                                INCO Term
InvoicingJob                            Invoicing Job
LocalForwarder                          LocalForwarderOrganisation
DeliveryLocation                        Location
FreightPayableAt                        Location
PickupLocation                          Location
CollectAmount                           Money
FreightRate                             Money
GoodsValue                              Money
InsuranceValue                          Money
ArrivalCFSTransport                     Organisation
AssuredParty                            Organisation
BookingParty                            Organisation
Buyer                                   Organisation
Carrier                                 Organisation
CarrierBookingAgent                     Organisation
CarrierHandlingAgent                    Organisation
ClaimsPayableBy                         Organisation
Client                                  Organisation
Consignee                               Organisation
Consignor                               Organisation
ConsolCreditor                          Organisation
Consolidator                            Organisation
ControllingAgent                        Organisation
ControllingCustomer                     Organisation
CTOArrival                              Organisation
DeliveryAgent                           Organisation
DepartureCFSTransport                   Organisation
ExportBroker                            Organisation
ImportAgent                             Organisation
ImportBroker                            Organisation
InsuredBy                               Organisation
JobHeaderLocalClient                    Organisation
MainShipToParty                         Organisation
MasterBillConsigneeOverride             Organisation
MasterBillShipperOverride               Organisation
NotifyParty                             Organisation
NotifyParty2                            Organisation
NotifyParty3                            Organisation
PickupAgent                             Organisation
Principal                               Organisation
ReceivingForwarder                      Organisation
RecommendedAgent                        Organisation
SellingParty                            Organisation
StuffingLocation                        Organisation
Supplier                                Organisation
SurveyReportParty                       Organisation
CarrierAccount                          OrgCarrierAccount
ShipmentInnerPacksQty                   PackQTY
ShipmentOuterPacksQty                   PackQTY
Person                                  Person
Destination                             PlaceAndDate
FirstForeignPort                        PlaceAndDate
LastForeignPort                         PlaceAndDate
Origin                                  PlaceAndDate
PortOfFirstArrival                      PlaceAndDate
QueryClaim                              Query Claim
Rating                                  Rating Information
InterestedRoute                         Route
RunSheet                                RunSheet
SalesRelations                          SalesRelations
SalesRep                                StaffMember
SupplierBuyerLink                       SupplierBuyerLink
ChargeableWeight                        ValueAndUnit
StorageTime                             ValueAndUnit
Volume                                  Volume
WarehouseJob                            WarehouseJob
Weight                                  Weight
ActualReceive                           DateTime
AdditionalTerms                         String
ApprovalNumber                          String
AreaBarcode                             String
AreaName                                String
ArrivalReference                        String
AWBSecurityInspectionStatus             String
BillingDate                             DateTime
BookingReference                        String
CACarrierName                           String
CAPreviousCCN                           String
CargoControlNumberForCanada             String
CarrierContractNumber                   String
CarrierIsNMFCEnabled                    Bool
CATransactionNo                         String
CAUSPortOfExit                          String
CO2eCalculationDate                     DateTime
ConNote                                 String
ConsigneeRequiredTaxNumber              String
ConsignorRequiredTaxNumber              String
ConsolAgentsReference                   String
ConsolDateCreated                       DateTime
ConsolNumber                            String
ConsolPaymentType                       String
ConsolReference                         String
ContainerCount                          String
ContainerLayoutStyle                    String
ContainerSummary                        String
CTOArrivalBerth                         String
CustomAttribute1                        String
CustomAttribute2                        String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomerReference                       String
CustomerReferenceNumber                 String
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomsEntryNumber                      String
DateOfFirstArrival                      DateTime
DeliveryActual                          DateTime
DeliveryCartageAdvised                  DateTime
DeliveryDueDate                         DateTime
DeliveryEstimated                       DateTime
DeliveryFrom                            DateTime
DeliveryGoodsDelivered                  DateTime
DeliveryRequiredBy                      DateTime
DepartureOrArrivalText                  String
DocumentNumber                          Int
DocumentTotal                           Int
EFreightStatus                          String
ExportAgentRequiredTaxNumber            String
ExportAgentsReference                   String
ExWorksRequiredBy                       DateTime
FactoryEx                               DateTime
FormattedTotalCO2e                      String
FreightDepotType                        String
FullCartageInstructions                 String
FullHandlingInstructions                String
GateInTime                              DateTimeOffset
GateOutTime                             DateTimeOffset
GoodsDescription                        String
GoodsHandlingInstructions               String
HasTACImage                             Bool
Hazardous                               Bool
HBLContainerMode                        String
HouseACIDNo                             String
HouseBill                               String
HouseBillHeading                        String
HouseBillIssue                          DateTime
ImportAgentRequiredTaxNumber            String
ImportAgentsReference                   String
IsAuthorisedToLeave                     Bool
IsDomestic                              Bool
IsPODRequired                           Bool
JobNumber                               String
JobNumberBarcodeText                    String
JobNumberBarcodeTextForFont             String
JobNumberBarcodeTextWithoutDocManagerCodes  String
JobNumberHeading                        String
LoadingMeters                           Decimal
LocalForwarderReference                 String
MarksAndNumbers                         String
MasterBill                              String
MasterBillConsigneeOverrideRequiredTaxNumber  String
MasterBillHeading                       String
MasterBillIssue                         DateTime
MasterBillShipperOverrideRequiredTaxNumber  String
NCTSDepartureTransportID                String
NCTSFrontierTransportID                 String
NCTSGoodsLocation                       String
NCTSGoodsLocationCode                   String
NoCopyBills                             Int
NoOriginalBills                         Int
NotClearedByAgentExpiryDate             DateTime
NotClearedByAgentIssueDate              DateTime
NotClearedByAgentNumber                 String
NotClearedByAgentStatement              String
NotifyPartyRequiredTaxNumber            String
OrderDate                               DateTime
OrderNumbersWithOwnersReference         String
OrderTrackingNumber                     String
OtherReferences                         String
Overs                                   Int
OwnerReference                          String
PackagesDetails                         String
PickupActual                            DateTime
PickupCartageAdvised                    DateTime
PickupDateOfReceipt                     DateTime
PickupFrom                              DateTime
PickupGoodsPickedup                     DateTime
PickupInterimReceipt                    String
PickupLabourCharge                      Decimal
PickupLabourTime                        String
PickupRequiredBy                        DateTime
PickupTruckWaitCharge                   Decimal
PickupTruckWaitTime                     String
PreviousCargoControlNumberForCanada     String
Priority                                Byte
QuoteNumber                             String
Refrigerated                            Bool
RevisedDeliveryDueDate                  DateTimeOffset
SecondaryHeading                        String
SecondaryNumber                         String
ShipmentDateCreated                     DateTime
ShippedOnBoardDate                      DateTime
ShippersReference                       String
Shorts                                  Int
StorageFromDate                         DateTime
StorageToDate                           DateTime
TransportReference                      String
TransportReferenceHeading               String
TransportZone                           String
UnAllocatedPackages                     Int
UnAllocatedVolume                       Decimal
UnAllocatedWeight                       Decimal
UNDGsSummary                            String
UOMTypeNumber                           Int
UOMTypeTotal                            Int
VendorID                                String
WarehouseLocation                       String
WarehouseName                           String
WarehouseNextDischargePort              String
WarehouseReleaseDate                    DateTime

TransportAddresses                      Address Collection
TransportAddressesWithWarehousing       Address Collection
AutoRatedInfosForJobRevenue             AutoRateInformation Collection
CarrierCharges                          Charge Collection
Charges                                 Charge Collection
NonCarrierCharges                       Charge Collection
CO2eEmissions                           CO2eEmission Collection
ScanningBarcodes                        CodeAndDesription Collection
CommercialInvoices                      CommercialInvoice Collection
CommercialInvoiceLines                  CommercialInvoiceLine Collection
Containers                              Container Collection
TranshipmentContainers                  Container Collection
ContainerPenalties                      ContainerPenalty Collection
ExportContainerPenalties                ContainerPenalty Collection
ImportContainerPenalties                ContainerPenalty Collection
Services                                ContainerService Collection
Costs                                   Cost Collection
CustomsEntries                          CustomsEntry Collection
EDocs                                   eDoc Collection
ExchangeRates                           ExchangeRate Collection
FreightConsolidations                   Freight Collection
FreightJobs                             Freight Collection
TransportBookings                       Freight Collection
BookingInstructions                     Instruction Collection
LocalTransportLegs                      LocalTransportLeg Collection
Milestones                              Milestone Collection
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
Orders                                  Order Collection
OrderLines                              OrderLine Collection
Packages                                Package Collection
PackProducts                            PackProductWrapper Collection
PickupDeliveryConfirmations             PickupDeliveryConfirmations Collection
RequiredDocuments                       RequiredDocuments Collection
ConsolRoutes                            Route Collection
ShipmentRoutes                          Route Collection
UNDGs                                   UNDGSubstance Collection
YardUnits                               YardUnit Collection
";
			}
		}

		#endregion

		#region Helpers

		protected OrgHeader GetOrgHeader(string fullName)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_FullName = fullName;
			result.MainAddress.OA_RN_NKCountryCode = "AU";
			return result;
		}

		protected OrgHeader GetOrgHeader(string fullName, string code, bool addMainAddressDetails = false)
		{
			var orgHeader = GetOrgHeader(fullName);
			orgHeader.OH_Code = code;

			if (addMainAddressDetails)
			{
				orgHeader.MainAddress.OA_Address1 = "1 " + fullName + " STREET";
				orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			}

			return orgHeader;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return FreightWrapper.NewFreightWrapper(GetNewBusinessObjectToWrap(), Factory);
		}

		#endregion
	}
}
