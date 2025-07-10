using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	public class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		public void TestImportFromXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = new JobDeclarationModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From XML"));
			}
		}

		public virtual void TestGridCollectionType()
		{
			using (var module = new JobDeclarationModule())
			{
				AssertEquals(typeof(JobDeclarationCollection), module.GridCollection.GetType());
			}
		}

		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			declaration.JE_LocationOfGoods = "YYY";
			declaration.ZG_CTStatusID = "A";
			var cei = declaration.CustomsEntryInstructions.AddNew();
			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTimeOffset.Now);
			cei.CEI_Style = "ABC";
			CreateExitReportData(declaration, i);
			return declaration;
		}

		void CreateExitReportData(JobDeclaration declaration, int i)
		{
			var exitReportStatuses = new List<ZString>();
			switch (i)
			{
				case 0:
					exitReportStatuses.Add("EXR");
					exitReportStatuses.Add("COX");
					break;
				case 1:
					exitReportStatuses.Add("COX");
					exitReportStatuses.Add("COX");
					break;
				default:
					exitReportStatuses.Add("EXR");
					break;
			}
			ExitControlTestHelper.CreateCusExitReportWithStatus(declaration, exitReportStatuses.ToArray());
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Latvia; }
		}

		protected override Type GetExpectedJobDeclarationType()
		{
			return typeof(JobDeclaration);
		}

		protected override Type GetExpectedInvoiceHeaderType()
		{
			return typeof(JobComInvoiceHeader);
		}

		protected override Type GetExpectedInvoiceLineType()
		{
			return typeof(JobComInvoiceLine);
		}
	}
}
