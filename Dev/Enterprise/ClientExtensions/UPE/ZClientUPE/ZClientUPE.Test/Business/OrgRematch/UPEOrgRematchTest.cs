using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(UPEOrgRematch))]
	public class UPEOrgRematchTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2000, 1, 1)]
		public void TestLogRematch()
		{
			Declaration.JE_OH_Importer = Organisation1.PK;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2000, 2, 2);
			Declaration.JE_OH_Importer = Organisation1.PK;
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			Declaration.JE_OH_Importer = Organisation2.PK;
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2000, 3, 3);
			Declaration.JE_OH_Importer = Organisation1.PK;
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			Factory.Save();
			ZQuery query = new ZQuery(ClientOrgRematchSchema.T5_JE, Declaration.PK);
			query.OrderBy = ClientOrgRematchSchema.T5_RematchedToDate.Name;
			UPEOrgRematch[] rematches = Factory.Load<UPEOrgRematch>(query);
			AssertEquals("Re-match 1 should be logged", Organisation1.PK, rematches[0].T5_OH_RematchedFromOrg);
			AssertEquals("Re-match 1 should be logged", Organisation2.PK, rematches[0].T5_OH_RematchedToOrg);
			AssertEquals("Re-match 1 should be logged", new ZDateTime(2000, 1, 1), rematches[0].T5_RematchedFromDate);
			AssertEquals("Re-match 1 should be logged", new ZDateTime(2000, 2, 2), rematches[0].T5_RematchedToDate);
			AssertEquals("Re-match 2 should be logged", Organisation2.PK, rematches[1].T5_OH_RematchedFromOrg);
			AssertEquals("Re-match 2 should be logged", Organisation1.PK, rematches[1].T5_OH_RematchedToOrg);
			AssertEquals("Re-match 2 should be logged", new ZDateTime(2000, 2, 2), rematches[1].T5_RematchedFromDate);
			AssertEquals("Re-match 2 should be logged", new ZDateTime(2000, 3, 3), rematches[1].T5_RematchedToDate);
		}

		public void TestLogRematch_WhenRematchedFromNothing()
		{
			Declaration.Factory.Save();
			Declaration.JE_OH_Importer = Organisation1.PK;
			Factory.Save();
			AssertRematch(UPEOrgRematch.OrgTypes.Importer, ZGuid.Empty, Organisation1.PK);
		}

		public void TestLogRematch_WhenRematchedToNothing()
		{
			Declaration.JE_OH_Importer = Organisation1.PK;
			Factory.Save();
			Declaration.JE_OH_Importer = ZGuid.Empty;
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			Factory.Save();
			AssertRematch(UPEOrgRematch.OrgTypes.Importer, Organisation1.PK, ZGuid.Empty);
		}

		public void TestLogRematch_IfOrganisationDidntChange()
		{
			Declaration.JE_OH_Importer = Organisation2.PK;
			Factory.Save();
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			Declaration.JE_OH_Importer = Organisation1.PK;
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			Declaration.JE_OH_Importer = Organisation2.PK;
			UPEOrgRematch rematch = Factory.LoadTop1<UPEOrgRematch>(new ZQuery(ClientOrgRematchSchema.T5_JE, Declaration.PK));
			Factory.Save();
			AssertNull("No rematch record should be created if there was no change", rematch);
		}

		public void TestLogRematch_OrgTypeCorrect()
		{
			Declaration.JE_OH_Importer = Organisation1.PK;
			Declaration.JE_OH_Supplier = Organisation2.PK;
			Factory.Save();
			Declaration.JE_OH_Importer = Organisation2.PK;
			Declaration.JE_OH_Supplier = Organisation1.PK;
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
			UPEOrgRematch.LogRematch(Declaration, Declaration.JE_OH_SupplierInfo, UPEOrgRematch.OrgTypes.Supplier);
			Factory.Save();
			AssertRematch(UPEOrgRematch.OrgTypes.Importer, Organisation1.PK, Organisation2.PK);
			AssertRematch(UPEOrgRematch.OrgTypes.Supplier, Organisation2.PK, Organisation1.PK);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		void AssertRematch(ZString organisationType, ZGuid rematchedFromOrg, ZGuid rematchedToOrg)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ClientOrgRematchSchema.T5_JE, SQLComparisonOperator.Equal, Declaration.PK);
			query.AddToFilter(ClientOrgRematchSchema.T5_OrganisationType, SQLComparisonOperator.Equal, organisationType);
			UPEOrgRematch rematch = Factory.LoadTop1<UPEOrgRematch>(query);
			AssertEquals("T5_OrganisationType", organisationType, rematch.T5_OrganisationType);
			AssertEquals("T5_OH_RematchedFromOrg", rematchedFromOrg, rematch.T5_OH_RematchedFromOrg);
			AssertEquals("T5_OH_RematchedToOrg", rematchedToOrg, rematch.T5_OH_RematchedToOrg);
		}

		UPEJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
				}

				return fDeclaration;
			}
		}

		UPEJobDeclaration fDeclaration;
		UPEOrgHeader Organisation1
		{
			get
			{
				if (fOrganisation1 == null)
				{
					fOrganisation1 = Factory.NewWithValidTestData<UPEOrgHeader>();
				}

				return fOrganisation1;
			}
		}

		UPEOrgHeader fOrganisation1;
		UPEOrgHeader Organisation2
		{
			get
			{
				if (fOrganisation2 == null)
				{
					fOrganisation2 = Factory.NewWithValidTestData<UPEOrgHeader>();
				}

				return fOrganisation2;
			}
		}

		UPEOrgHeader fOrganisation2;

		#endregion
	}
}
