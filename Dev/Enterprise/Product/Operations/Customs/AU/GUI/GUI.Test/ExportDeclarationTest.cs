using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ExportDeclarationTest : TestCaseWithFactory
	{
		[TestDate(2008, 1, 1)]
		public void TestMiscellaneousSupplierImporter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "01063196", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST", description: "LIVE BIRDS OF PREY");
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation);
				orgHeader.CustomsClientID = "12345";
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "EXP";
				declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				declaration.JE_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
				declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

				declaration.JE_ExportDate = ZDateTime.Today;
				declaration.JE_RL_NKOrigin = "AUSYD";
				declaration.JE_RL_NKFinalDestination = "USLAX";
				declaration.JE_RL_NKPortOfArrival = "USLAX";
				declaration.JE_RL_NKPortOfFirstArrival = "USLAX";
				declaration.JE_TotalNoOfPacks = 1;
				declaration.JE_TransportMode = "SEA";
				declaration.JE_ContainerCount = 1;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "BOP";
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoiceHeader.JZ_InvoiceAmount = 1500m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "0106.31.96";

				invoiceLine.JI_Description = "Birds of Prey";
				invoiceLine.JI_Weight = 18m;
				invoiceLine.JI_CustomsQuantity = 2m;
				invoiceLine.JI_LinePrice = 1500m;

				using (var form = new ZAUCustomsDeclarationForm(declaration))
				{
					form.Show();
					var importerControl = FindControl<ZOrganisationControlWithMiscellaneous>(form, "ImporterOrganisationControl");
					var importerMiscellaneousTab = FindControl<TabPage>(importerControl, "MiscellaneousTab");

					var importerConsigneeNameTextBox = FindControl<ZTextBox>(importerControl, "ZA" + "_ConsigneeNameHiddenTextBox");
					Assert(importerConsigneeNameTextBox.Visible);
					importerConsigneeNameTextBox.Focus();
					importerConsigneeNameTextBox.Text = "Consignee Name";
					var importerConsigneeCityTextBox = FindControl<ZTextBox>(importerControl, "ZA" + "_ConsigneeCityHiddenTextBox");
					Assert(importerConsigneeCityTextBox.Visible);
					importerConsigneeCityTextBox.Focus();
					importerConsigneeCityTextBox.Text = "Consignee City";
					Application.DoEvents();
					var supplierControl = FindControl<ZOrganisationControlWithMiscellaneous>(form, "SupplierOrganisationControl");
					var supplierMiscellaneousTab = FindControl<TabPage>(supplierControl, "MiscellaneousTab");
					var goodsPartyIDTextBox = FindControl<ZTextBox>(supplierControl, "ZA_GoodsOwnerPartyIDHiddenTextBox");
					Assert(goodsPartyIDTextBox.Visible);
					goodsPartyIDTextBox.Focus();
					goodsPartyIDTextBox.Text = "154049728";
					importerConsigneeNameTextBox.Focus();

					declaration.LoadChildEditableObjects();
					declaration.RunPreSaveValidation();
					AssertEquals("PreCondition: No Errors expected", "", declaration.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString());
					AssertEquals("PreCondition: No Message Errors expected", "", declaration.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString());
					declaration.DoMerge();
					declaration.Factory.Save();

					var mockQuerier = new Mock<IServiceManagerQuerier>();
					mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

					using (ObjectFactory.Substitute(mockQuerier.Object))
					{
						var menuItem = form.Menu.MenuItems.FindByText("Brokerage").MenuItems.FindByText("Send Original");
						menuItem.PerformClick();
						AssertEquals("Should be one message created, error text: " + UnitTestUserNotification.Instance.LastMessage.Text + " /Message errors: " + declaration.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString(), 1, declaration.EntryHeader.Messages.Count);
						var messageText = declaration.EntryHeader.Messages[0].EM_MessageText;
						AssertContains("CONSIGNEE NAME", messageText);
						AssertContains("CONSIGNEE CITY", messageText);
						AssertContains("154049728", messageText);
					}
				}
			}
		}

		[TestDate(2008, 1, 1)]
		public void TestMiscellaneousSupplierImporter_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var orgHeader = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation);
				orgHeader.CustomsClientID = "12345";
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "EXP";
				declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				declaration.JE_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
				declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

				declaration.JE_ExportDate = ZDateTime.Today;
				declaration.JE_RL_NKOrigin = "AUSYD";
				declaration.JE_RL_NKFinalDestination = "USLAX";
				declaration.JE_RL_NKPortOfArrival = "USLAX";
				declaration.JE_RL_NKPortOfFirstArrival = "USLAX";
				declaration.JE_TotalNoOfPacks = 1;
				declaration.JE_TransportMode = "SEA";
				declaration.JE_ContainerCount = 1;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "BOP";
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoiceHeader.JZ_InvoiceAmount = 1500m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "0106.31.96";

				invoiceLine.JI_Description = "Birds of Prey";
				invoiceLine.JI_Weight = 18m;
				invoiceLine.JI_CustomsQuantity = 2m;
				invoiceLine.JI_LinePrice = 1500m;

				var mockQuerier = new Mock<IServiceManagerQuerier>();
				mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ObjectFactory.Substitute(mockQuerier.Object))
				using (var form = new ZAUCustomsDeclarationForm(declaration))
				{
					form.Show();
					var importerControl = FindControl<ZOrganisationControlWithMiscellaneous>(form, "ImporterOrganisationControl");
					var importerMiscellaneousTab = FindControl<TabPage>(importerControl, "MiscellaneousTab");

					var importerConsigneeNameTextBox = FindControl<ZTextBox>(importerControl, "ZA" + "_ConsigneeNameHiddenTextBox");
					Assert(importerConsigneeNameTextBox.Visible);
					importerConsigneeNameTextBox.Focus();
					importerConsigneeNameTextBox.Text = "Consignee Name";
					var importerConsigneeCityTextBox = FindControl<ZTextBox>(importerControl, "ZA" + "_ConsigneeCityHiddenTextBox");
					Assert(importerConsigneeCityTextBox.Visible);
					importerConsigneeCityTextBox.Focus();
					importerConsigneeCityTextBox.Text = "Consignee City";
					Application.DoEvents();
					var supplierControl = FindControl<ZOrganisationControlWithMiscellaneous>(form, "SupplierOrganisationControl");
					var supplierMiscellaneousTab = FindControl<TabPage>(supplierControl, "MiscellaneousTab");
					var goodsPartyIDTextBox = FindControl<ZTextBox>(supplierControl, "ZA_GoodsOwnerPartyIDHiddenTextBox");
					Assert(goodsPartyIDTextBox.Visible);
					goodsPartyIDTextBox.Focus();
					goodsPartyIDTextBox.Text = "154049728";
					importerConsigneeNameTextBox.Focus();

					declaration.LoadChildEditableObjects();
					declaration.RunPreSaveValidation();
					AssertEquals("PreCondition: No Errors expected", "", declaration.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString());
					AssertEquals("PreCondition: No Message Errors expected", "", declaration.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString());
					declaration.DoMerge();
					declaration.Factory.Save();

					var menuItem = form.Menu.MenuItems.FindByText("Brokerage").MenuItems.FindByText("Send Original");
					menuItem.PerformClick();
					AssertEquals("Should be one message created, error text: " + UnitTestUserNotification.Instance.LastMessage.Text + " /Message errors: " + declaration.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString(), 1, declaration.EntryHeader.Messages.Count);
					var messageText = declaration.EntryHeader.Messages[0].EM_MessageText;
					AssertContains("CONSIGNEE NAME", messageText);
					AssertContains("CONSIGNEE CITY", messageText);
					AssertContains("154049728", messageText);
				}
			}
		}

		T FindControl<T>(Control parent, string name) where T : Control
		{
			foreach (Control control in parent.Controls)
			{
				if (control.Name == name && control is T)
				{
					return (T)control;
				}
			}

			foreach (Control control in parent.Controls)
			{
				T result = FindControl<T>(control, name);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}
	}
}
