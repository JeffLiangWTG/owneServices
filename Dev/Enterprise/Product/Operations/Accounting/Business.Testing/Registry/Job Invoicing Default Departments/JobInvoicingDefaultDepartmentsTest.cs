using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoicingDefaultDepartments))]
	public class JobInvoicingDefaultDepartmentsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestConsolTypesList()
		{
			var testSubject = (JobInvoicingDefaultDepartments)GetNewBusinessObject();
			AssertEquals("ALL, NCN, DRT, CLD, AGT, CHT, COU, OTH, CLA", testSubject.ConsolTypes.CodesAsString);
		}

		public void TestCountOfConsolTypesAreEquals()
		{
			var testSubject = (JobInvoicingDefaultDepartments)GetNewBusinessObject();
			// Except OnBoardCourier, looks like we do not add it in JK_AgentType_List yet.
			// Except AWBMaster, looks like it can't create a job header.
			var countOfAgentTypes = typeof(Constants.AgentType).GetFields().Length - 2;
			AssertEquals(countOfAgentTypes, testSubject.ConsolTypes.Count - 2);
		}

		public void TestDepartmentsList()
		{
			JobInvoicingDefaultDepartments testSubject = (JobInvoicingDefaultDepartments)GetNewBusinessObject();
			ZQuery referenceQuery = new ZDBOnlyQuery(typeof(GlbDepartment));
			referenceQuery.AddFilterAndZSQLParameterCollection(string.Format(@"
								{0} = 0
								AND
								(
									{1} IS NULL OR
									{1} NOT IN
									(
										SELECT {2}
										FROM {3}
										WHERE {0} = 1
									)
								)",
												   GlbDepartmentSchema.Constants.GE_Misc, // 0
												   GlbDepartmentSchema.Constants.GE_GE, // 1
												   GlbDepartmentSchema.Constants.PK, // 2
												   GlbDepartmentSchema.Constants.TableName), new ZSqlParameterCollection());
			GlbDepartmentCollection referenceCollection = new GlbDepartmentCollection(Factory, referenceQuery);

			AssertEquals("Count", referenceCollection.Count, testSubject.Departments.Count);

			foreach (GlbDepartment department in referenceCollection)
			{
				Assert(string.Format("Should contain {0}", department.GE_Code), testSubject.Departments.FindByPK(department.PK) != null);
			}
		}

		public void TestConsolTypeValidation()
		{
			JobInvoicingDefaultDepartmentsCollection collection = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments entry1 = collection.AddNew();
			entry1.ConsolType = "ALL";
			AssertNoErrors("Prerequisite: should have no errors", entry1.ConsolTypeInfo);

			entry1 = collection.AddNew();
			entry1.RunPreSaveValidation();
			AssertHasError(entry1.ConsolTypeInfo, "Please enter a value.");
			entry1.ConsolType = "XYZ";
			AssertHasError(entry1.ConsolTypeInfo, "Enter a valid selection.");
			entry1.ConsolType = "AGT";
			AssertNoErrors(entry1.ConsolTypeInfo);

			JobInvoicingDefaultDepartments entry2 = collection.AddNew();
			entry2.ConsolType = "AGT";
			AssertHasError(entry2.ConsolTypeInfo, "There must be only one 'AGT' line.");
		}

		public void TestConsolTypeValidationForCompleteness()
		{
			JobInvoicingDefaultDepartmentsCollection collection = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments entry1 = collection.AddNew();

			entry1.ConsolType = "ALL";
			AssertNoErrors(entry1.ConsolTypeInfo);

			JobInvoicingDefaultDepartments entry2 = collection.AddNew();
			entry2.ConsolType = "CLD";
			AssertNoErrors(entry1.ConsolTypeInfo);
			AssertNoErrors(entry2.ConsolTypeInfo);

			collection.Remove(entry1);
			AssertHasError(entry2.ConsolTypeInfo, "Either all consol types must be entered, or an 'ALL' line should exist.");

			foreach (ICodeDescription pair in entry2.ConsolTypes)
			{
				if (pair.Code != Constants.JobInvoicingDefaultDepartmentConsolType.All && pair.Code != "CLD")
				{
					entry1 = collection.AddNew();
					entry1.ConsolType = pair.Code;
				}
			}

			foreach (JobInvoicingDefaultDepartments entry in collection)
			{
				entry.RunPreSaveValidation();
				AssertNoErrors(entry.ConsolTypeInfo);
			}
		}

		public void TestDepartmentValidation()
		{
			JobInvoicingDefaultDepartmentsCollection collection = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments entry = collection.AddNew();

			entry.RunPreSaveValidation();
			AssertHasError(entry.DepartmentInfo, "Please enter a value.");
			entry.Department = ZGuid.NewZGuid();
			AssertHasError(entry.DepartmentInfo, "Enter a valid selection.");
			entry.Department = entry.Departments[0].PK;
			AssertNoErrors(entry.DepartmentInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new JobInvoicingDefaultDepartments();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobInvoicingDefaultDepartments();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
