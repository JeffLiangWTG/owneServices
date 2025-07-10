using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing
{
	public class JobInvoicePrintingControlSubTest : TestCaseWithFactory
	{
		public void TestInvoicesGridContainsColumnCompliaceDocDate()
		{
			var typecountrycode = typeof(Core.Constants.CountryCodes);
			var index = 0;
			var countryNames = new string[]
			{
				Core.Constants.CountryCodes.China,
				Core.Constants.CountryCodes.KoreaSouth,
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.Japan,
				Core.Constants.CountryCodes.UnitedStates
			};

			foreach (var countryname in countryNames)
			{
				index++;
				var bizo = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryname);
				if (bizo == null)
				{
					continue;
				}

				using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryname))
				{
					if (string.IsNullOrEmpty(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency))
					{
						continue;
					}

					using (ZForm form = new ZForm())
					using (JobInvoicePrintingControl control = new JobInvoicePrintingControl())
					{
						form.Controls.Add(control);
						form.Show();

						var shipment = TestObjectCreator.CreateShipment("S0001" + index.ToString());
						var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00004000" + index.ToString(), TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
						var line = TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
						var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);

						charge.JR_AL_ARLine = line.PK;
						Factory.Save();

						JobARInvoicePrintingFilter filter = new JobARInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
						control.Bind(filter);

						var type = typeof(JobInvoicePrintingControl);
						var mem = type.GetField("InvoicesGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						var grid = (ZGrid)mem.GetValue(control);
						if (countryname == Core.Constants.CountryCodes.China)
						{
							Assert(grid.Columns.FirstOrDefault(t => t.ColumnName == "AH_ComplianceDocumentDate").ColumnStyle.HeaderText == "Compliance Doc Date");
						}
						else
						{
							Assert(!grid.Columns.Contains("AH_ComplianceDocumentDate"));
						}
					}
				}
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
