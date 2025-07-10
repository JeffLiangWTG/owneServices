using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestsSubclassesOf(typeof(IPGAProgramRequirementProvider))]
	public abstract class InvoiceLinePGAProgramRequirementProviderTest<T> : TestCaseWithFactory
			where T : IPGAProgramRequirementProvider
	{
		public void TestPGAProgramCodeWorksCorrectlyForInvoiceLine()
		{
			TestPGAProgramCodeWorksCorrectly(invoiceLine);
		}

		public void TestPGAProgramCodeWorksCorrectlyForCusClassPartPivot()
		{
			TestPGAProgramCodeWorksCorrectly(pivot);
		}

		public void TestPGAHeaderDeletedWithParentForInvoiceLine()
		{
			TestPGAHeaderDeletedWithParent(invoiceLine);
		}

		public void TestPGAHeaderDeletedWithParentForCusClassPartPivot()
		{
			TestPGAHeaderDeletedWithParent(pivot);
		}

		void TestPGAProgramCodeWorksCorrectly<TParent>(TParent parent) where TParent : BusinessObject, IHasPGARequirements
		{
			AssertNotEquals("Please check if the CusAddInfoTypeAttribute is setup correctly on following class.\r\n" + typeof(T).ToString() + "AddInfo", ZString.Empty, CusAddInfoType);
			var hasMatchedPGARequirementFound = false;

			foreach (PGARequirement pgaRequirement in parent.PGARequirements)
			{
				if (pgaRequirement.AgencyCode == AgencyCode)
				{
					hasMatchedPGARequirementFound = true;
					AssertEquals("No PGA header should be generated.", 0, GetCusAddInfoFromDB(parent).Count());
					AssertNotEquals("At least 1 program exists in PGA.", 0, pgaRequirement.ProgramCodeRequirements.Count);

					pgaRequirement.ProgramCodeRequirements[0].Indicator = YesNoList.Codes.Yes;

					Factory.Save();
					AssertEquals("1 PGA header should be generated.", 1, GetCusAddInfoFromDB(parent).Count());

					var newFactory = new BusinessObjectFactory();
					var loadedInvoiceLine = newFactory.Load<TParent>(parent.PK);
					var loadedPGARequirement = loadedInvoiceLine.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == AgencyCode);
					AssertNotNull("Has matched PGA requirement found.", loadedPGARequirement);
					AssertEquals("PGA indicator should be 'Y'", YesNoList.Codes.Yes, loadedPGARequirement.Indicator);
					Assert("Program indicator should be 'Y'", loadedPGARequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().Any(x => x.Indicator == YesNoList.Codes.Yes));

					loadedPGARequirement.ProgramCodeRequirements[0].Indicator = YesNoList.Codes.No;
					AssertEquals("Then pga indicator should be no", YesNoList.Codes.No, loadedPGARequirement.Indicator);

					if (loadedPGARequirement.ProgramCodeRequirements.Count > 1)
					{
						loadedPGARequirement.ProgramCodeRequirements[1].Indicator = YesNoList.Codes.Yes;
						AssertEquals("if there is at least one Yes, then it should be Yes", YesNoList.Codes.Yes, loadedPGARequirement.Indicator);

						loadedPGARequirement.ProgramCodeRequirements[0].Indicator = YesNoList.Codes.Yes;
						AssertEquals("if there is at least one Yes, then it should be Yes", YesNoList.Codes.Yes, loadedPGARequirement.Indicator);

						loadedPGARequirement.ProgramCodeRequirements[0].Indicator = YesNoList.Codes.No;
						AssertEquals("if there is at least one Yes, then it should be Yes", YesNoList.Codes.Yes, loadedPGARequirement.Indicator);

						loadedPGARequirement.ProgramCodeRequirements[1].Indicator = YesNoList.Codes.No;
						AssertEquals("All are either No or blank", YesNoList.Codes.No, loadedPGARequirement.Indicator);
					}

					newFactory.Save();

					newFactory = new BusinessObjectFactory();
					loadedInvoiceLine = newFactory.Load<TParent>(parent.PK);
					loadedPGARequirement = loadedInvoiceLine.PGARequirements.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == AgencyCode);
					AssertNotNull("Has matched PGA requirement found.", loadedPGARequirement);
					AssertEquals("No is persisted", YesNoList.Codes.No, loadedPGARequirement.Indicator);
					AssertEquals("No is what users have indicated and should be persisted", YesNoList.Codes.No, loadedPGARequirement.ProgramCodeRequirements[0].Indicator);

					Factory.Save();
					AssertEquals("No PGA header should be generated.", 1, GetCusAddInfoFromDB(parent).Count());
					break;
				}
			}

			if (!hasMatchedPGARequirementFound)
			{
				Assert("No matched PGA requirement found", false);
			}
			else
			{
				Assert("All works correctly!", true);
			}
		}

		void TestPGAHeaderDeletedWithParent<TParent>(TParent parent) where TParent : BusinessObject, IHasPGARequirements
		{
			PGARequirementCollection pgaRequirements = parent.PGARequirements;
			PGARequirement pgaRequirement = pgaRequirements.PGARequirement(AgencyCode);
			var pgaProgramRequirement = pgaRequirement.ProgramCodeRequirements.FirstOrDefault() as PGAProgramRequirement;

			AssertNotNull(pgaProgramRequirement);
			pgaProgramRequirement.Indicator = YesNoList.Codes.Yes;

			Factory.Save();

			// PGA headers was saved
			AssertEquals(1, GetCusAddInfoFromDB(parent).Count());

			parent.Delete();
			Factory.Save();

			// PGA header was deleted
			AssertEquals(0, GetCusAddInfoFromDB(parent).Count());
		}

		protected abstract ZString AgencyCode { get; }

		protected ZString CusAddInfoType
		{
			get { return CusAddInfoTypeAttribute.GetTypeCodeFromAttribute(typeof(T)); }
		}

		IEnumerable<CusAddInfo> GetCusAddInfoFromDB(BusinessObject parent)
		{
			var query = new ZDBOnlyQuery(typeof(CusAddInfo));
			query.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoType);
			query.AddToFilter(CusAddInfoSchema.B7_ParentID, parent.PK);
			return Factory.Load<CusAddInfo>(query);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();

			pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
		}

		JobComInvoiceLine invoiceLine;
		CusClassPartPivot pivot;
	}
}
