using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocProfitLossDetailedLineCollection))]
	public class DocProfitLossDetailedLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocProfitLossDetailedLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = ConsolWrapper.ProfitLoss.ProfitLossFilteredDetails[0];
			return DocProfitLossDetailedLine.New(line, Factory);
		}

		protected override DocProfitLossDetailedLineCollection GetCollectionToTest()
		{
			return new DocProfitLossDetailedLineCollection(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var consol = Factory.New<ForwardingConsol>();
			ConsolWrapper = DocForwardingConsol.New(consol, Factory);
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment shipment = consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();
			job.LoadCharges_ForTestOnly();

			ConsolWrapper = DocForwardingConsol.New(consol, Factory);
		}

		DocForwardingConsol ConsolWrapper;

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}

				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;
	}
}
