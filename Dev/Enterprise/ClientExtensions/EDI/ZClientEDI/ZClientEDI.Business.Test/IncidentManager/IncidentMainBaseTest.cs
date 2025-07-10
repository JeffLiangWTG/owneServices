using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public abstract class IncidentMainBaseTestCase : EnterpriseBusinessObjectTestCase
	{
		public void TestIM_InvoicingLocalClientHasInvoicingPreferencesNote()
		{
			Assert(!Incident.IM_InvoicingLocalClientHasInvoicingPreferencesNote);
			using (var job = new Job.Loader(Incident).TryCreateWithMutex())
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				var org = Factory.NewWithValidTestData<OrgHeader>();

				org.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoicingPreferences.Description, "");
				address.OA_OH = org.PK;
				job.JH_OA_LocalChargesAddr = address.PK;

				Assert("TryCreate should acquire lock", JobHeader.GetMutexForTest(job).HasLock);
				Assert(Incident.IM_InvoicingLocalClientHasInvoicingPreferencesNote);
				Assert("IM_InvoicingLocalClientHasInvoicingPreferencesNote should not release lock", JobHeader.GetMutexForTest(job).HasLock);
			}
		}

		public void TestIM_ClientHasInvoicingPreferencesNote()
		{
			Assert(!Incident.IM_ClientHasInvoicingPreferencesNote);

			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.InvoicingPreferences.Description, "");
			address.OA_OH = org.PK;
			Incident.IM_OA_BranchAddress = address.PK;

			Assert(Incident.IM_ClientHasInvoicingPreferencesNote);
		}

		public void TestIM_Calc_BranchFax()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_Fax = "123456789";

			AssertEquals("IM_Calc_BranchFax", "", Incident.IM_Calc_BranchFax);

			Incident.IM_OA_BranchAddress = address.PK;
			AssertEquals("IM_Calc_BranchFax", "123456789", Incident.IM_Calc_BranchFax);
		}

		public void TestIM_Calc_BranchPhone()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_Phone = "987654321";

			AssertEquals("IM_Calc_BranchPhone", "", Incident.IM_Calc_BranchPhone);

			Incident.IM_OA_BranchAddress = address.PK;
			AssertEquals("IM_Calc_BranchPhone", "987654321", Incident.IM_Calc_BranchPhone);
		}

		[ExpectNoExceptions]
		public virtual void TestPopulateWorkItem()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			Incident.PopulateWorkItem(workItem);
		}

		public void TestTypeDecider()
		{
			Type[] types =
			{
				typeof(ProfessionalServicesQuote),
				typeof(SupportIncident),
			};

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			foreach (Type type in types)
			{
				BusinessObject incident = Factory.NewWithValidTestData(type);
				Factory.Save();

				IncidentMainBase loadedIncident = newFactory.Load<IncidentMainBase>(incident.PK);
				AssertEquals("Loaded incident should be of type '" + type.Name + "'.", type, loadedIncident.GetType());
			}
		}

		public void TestIsAutoLogged()
		{
			if (Incident is SupportIncident)
			{
				Assert("Should not be auto-logged", !Incident.IsAutoAdminBusinessObjectLoggerEnabled);
			}
			else
			{
				Assert("Should be auto-logged", Incident.IsAutoAdminBusinessObjectLoggerEnabled);
			}
		}

		public void TestIM_ChargableWorkSelection()
		{
			Incident.IM_ChargableWorkYesSelection = true;
			AssertEquals("IM_ChargableWorkYesSelection", true, Incident.IM_ChargableWorkYesSelection);
			AssertEquals("IM_ChargableWorkNoSelection", false, Incident.IM_ChargableWorkNoSelection);

			Incident.IM_ChargableWorkYesSelection = false;
			AssertEquals("IM_ChargableWorkYesSelection", false, Incident.IM_ChargableWorkYesSelection);
			AssertEquals("IM_ChargableWorkNoSelection", true, Incident.IM_ChargableWorkNoSelection);

			Incident.IM_ChargableWorkNoSelection = false;
			AssertEquals("IM_ChargableWorkYesSelection", true, Incident.IM_ChargableWorkYesSelection);
			AssertEquals("IM_ChargableWorkNoSelection", false, Incident.IM_ChargableWorkNoSelection);

			Incident.IM_ChargableWorkNoSelection = true;
			AssertEquals("IM_ChargableWorkYesSelection", false, Incident.IM_ChargableWorkYesSelection);
			AssertEquals("IM_ChargableWorkNoSelection", true, Incident.IM_ChargableWorkNoSelection);
		}

		public void TestIM_ChargableWorkAsText()
		{
			Incident.IM_ChargableWork = ZBool.True;
			AssertEquals("Yes", Incident.IM_ChargableWorkAsText);

			Incident.IM_ChargableWork = ZBool.False;
			AssertEquals("No", Incident.IM_ChargableWorkAsText);
		}

		protected IncidentMainBase Incident
		{
			get { return (IncidentMainBase)CachedBusinessObject; }
		}

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent incidentMainBase = Factory.New<IncidentMainBase>();
			Assert(incidentMainBase.AllowInvoiceDeletion);
		}

		#endregion
	}

	abstract class IncidentMainBaseRelatedItemTest : WorkTaskRelatedItemTestCase
	{
		protected override string ExpectedSelectionCriterion1 => "ACC - Accountant";
		protected override string ExpectedSelectionCriterion2 => "GEN - General";
		protected override string ExpectedSelectionCriterion3 => "BUF - Buffer Management";
		protected override string ExpectedSelectionCriterion4 => string.Empty;
		protected override string ExpectedSelectionCriterion5 => string.Empty;

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			var incident = (IncidentMainBase)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			incident.IM_Product = "ACC";
			incident.IM_ProgramArea = "GEN";

			return (IWorkTaskRelatedItem)incident;
		}

		protected override Type ExpectedPivotCollectionType => typeof(GenPivotCollection);
	}

	abstract class IncidentMainBaseRelatedItemSourceTest : WorkTaskRelatedItemSourceTestCase
	{
	}
}
