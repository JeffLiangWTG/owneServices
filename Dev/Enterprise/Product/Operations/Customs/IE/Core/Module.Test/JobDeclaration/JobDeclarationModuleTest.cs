using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(JobDeclarationModuleForTest))]
	class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
	{
		public void TestGetNewActionMenuItems()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var item = module.GetNewActionMenuItems().FindByText("Copy and Submit To Customs");
				AssertNotNull(item);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Ireland;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(CargoWise.EntityFramework.BusinessObjectFactory factory, string messageType, int i)
		{
			var jobDeclaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			var cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.First();
			var requestedDocument = cusEntryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			return jobDeclaration;
		}

		sealed class JobDeclarationModuleForTest : JobDeclarationModule
		{
			public new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();
		}
	}
}
