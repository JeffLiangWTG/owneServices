using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.MFI.DocWrappers;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using DocAUWrappers = Enterprise.DocumentWrappers.Customs.AU;
using DocNZWrappers = Enterprise.DocumentWrappers.Customs.NZ;
using NZCustoms = Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Client.MFI.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class MFIClientOverrideTest : ClientOverrideTest
	{
		public void TestOverrides()
		{
			ClientOverride @override = ClientOverride.Instance;
			AssertEquals("Client", Clients.MFI, @override.Client);
			AssertEquals("Client Display Name should be 'MFI'", "MFI", @override.ClientDisplayName);
			AssertEquals("Help Web Page should be empty", "", @override.HelpWebPage);
			AssertNotNull("ModuleOverrides should contain JobConsol", @override.ModuleOverrides[ModuleIDs.JobConsol, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertNotNull("ModuleOverrides should contain Orders", @override.ModuleOverrides[ModuleIDs.Orders, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertNotNull("ModuleOverrides should contain Transaction", @override.ModuleOverrides[ModuleIDs.APTransaction, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertNotNull("ModuleOverrides should contain Organisation", @override.ModuleOverrides[ModuleIDs.Organisation, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertEquals("AdditionalRegistryItemSet", MFIDataRegistry.Instance, @override.AdditionalRegistryItemSet);
			AssertNotNull("ClientTypeDeciders should not be null", @override.ClientTypeDeciders);
			AssertEquals("ClientTypeDeciders count", 1, new List<KeyValuePair<Type, ITypeDecider>>(@override.ClientTypeDeciders).Count);
			AssertEquals("Type of ClientTypeDeciders[0]", typeof(MFIForwardingShipment), ((TypeDeciderImpl)@override.ClientTypeDeciders[typeof(ForwardingShipment)]).ClientType);
		}

		public void TestInitialiseAndUninitialised()
		{
			if (ClientOverride.Instance.IsInitialised)
			{
				ClientOverride.Instance.Uninitialise();
			}

			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("MFIEDImenu should have been UNinitialised", typeof(EDIMenu), menu.GetType());
			}

			AssertEquals("Testing overrides", typeof(InvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(Invoice).GetType());
			AssertEquals("Testing overrides", typeof(Statement), Statement.New(GlbBranch.CurrentBranch).GetType());
			AssertEquals("Testing overrides", typeof(DocStatement), DocStatement.New(PrintStatement, Factory).GetType());
			AssertEquals("Testing overrides", typeof(PrintStatementDocumentSupporter), PrintStatementDocumentSupporter.New(PrintStatement).GetType());
			AssertEquals("Testing overrides", typeof(DocForwardingShipment), DocForwardingShipment.New(shipment, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocBillofLadingContainer), DocBillofLadingContainer.New(DocContainer).GetType());
			AssertEquals("Testing overrides", typeof(DocContainer), DocContainer.New(FreightContainer, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocContainerCollection), DocContainerCollection.New(Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocAUWrappers.DocDeclaration), DocAUWrappers.DocDeclaration.New(declaration, Factory).GetType());
			NZCustoms.JobDeclaration nZDeclaration = Factory.NewWithValidTestData<NZCustoms.JobDeclaration>();
			NZCustoms.CusContainer nzCusContainer = nZDeclaration.CusContainers.AddNew();
			AssertEquals("Testing overrides", typeof(DocNZWrappers.DocDeclaration), DocNZWrappers.DocDeclaration.New(nZDeclaration, Factory).GetType());
			AssertEquals("Testing NZCusContainer override", typeof(DocNZWrappers.DocCusContainer), DocNZWrappers.DocCusContainer.New(nzCusContainer, Factory).GetType());
			var auCusContainer = declaration.CusContainers.AddNew();
			ClientOverride.Instance.Initialise();
			AssertEquals("Testing overrides", typeof(MFIInvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(Invoice).GetType());
			AssertEquals("Testing overrides", typeof(MFIStatement), Statement.New(GlbBranch.CurrentBranch).GetType());
			AssertEquals("Testing overrides", typeof(DocMFIPrintStatement), DocStatement.New(PrintStatement, Factory).GetType());
			AssertEquals("Testing overrides", typeof(MFIPrintStatementDocumentSupporter), PrintStatementDocumentSupporter.New(PrintStatement).GetType());
			AssertEquals("Testing overrides", typeof(DocMFIBillofLadingContainer), DocBillofLadingContainer.New(DocContainer).GetType());
			ClientOverride.Instance.Uninitialise();
			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("MFIEDImenu should have been UNinitialised", typeof(EDIMenu), menu.GetType());
			}

			AssertEquals("Testing overrides", typeof(InvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(Invoice).GetType());
			AssertEquals("Testing overrides", typeof(Statement), Statement.New(GlbBranch.CurrentBranch).GetType());
			AssertEquals("Testing overrides", typeof(DocStatement), DocStatement.New(PrintStatement, Factory).GetType());
			AssertEquals("Testing overrides", typeof(PrintStatementDocumentSupporter), PrintStatementDocumentSupporter.New(PrintStatement).GetType());
			AssertEquals("Testing overrides", typeof(DocForwardingShipment), DocForwardingShipment.New(shipment, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocBillofLadingContainer), DocBillofLadingContainer.New(DocContainer).GetType());
			AssertEquals("Testing overrides", typeof(DocContainer), DocContainer.New(FreightContainer, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocContainerCollection), DocContainerCollection.New(Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocAUWrappers.DocDeclaration), DocAUWrappers.DocDeclaration.New(declaration, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocNZWrappers.DocDeclaration), DocNZWrappers.DocDeclaration.New(nZDeclaration, Factory).GetType());
			AssertEquals("Testing NZCusContainer override", typeof(DocNZWrappers.DocCusContainer), DocNZWrappers.DocCusContainer.New(nzCusContainer, Factory).GetType());
			AssertEquals("Testing AUCusContainer override", typeof(DocAUWrappers.DocCusContainer), DocAUWrappers.DocCusContainer.New(auCusContainer, Factory).GetType());
		}

		#region Implementation
		ARInvoice Invoice;
		BusinessObjectFactory Factory;
		PrintStatement PrintStatement;
		ForwardingShipment shipment;
		CommonContainer FreightContainer;
		DocContainer DocContainer;
		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			Factory = new BusinessObjectFactory();
			Invoice = Factory.New<ARInvoice>();
			PrintStatement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			FreightContainer = Factory.NewWithValidTestData<CommonContainer>();
			DocContainer = DocContainer.New(FreightContainer, Factory);
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
		#endregion
	}
}
