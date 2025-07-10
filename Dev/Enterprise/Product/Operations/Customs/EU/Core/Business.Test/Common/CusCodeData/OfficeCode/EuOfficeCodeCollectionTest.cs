using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(EuOfficeCodeCollection))]
	public class EuOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultMandatoryCustomsOffices_Import()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var offices = declaration.CustomsOffices.Cast<EuOfficeCode>();
			AssertContainsExactElementsInAnyOrder(new[] { "CAU" }, offices.Select(x => x.CY_Code));
		}

		public void TestDefaultMandatoryCustomsOffices_Export()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var offices = declaration.CustomsOffices.Cast<EuOfficeCode>();
			AssertContainsExactElementsInAnyOrder(new[] { "CAU", "DEP" }, offices.Select(x => x.CY_Code));
		}

		[ExpectNoExceptions]
		public void TestDefaultMandatoryCustomsOffices_MessageTypeChanged()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var offices = declaration.CustomsOffices.Cast<EuOfficeCode>();
			var cauOffice1 = offices.Single(x => x.CY_Code == "CAU");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cauOffice2 = offices.Single(x => x.CY_Code == "CAU");
			NUnit.Framework.Assert.That(cauOffice2, NUnit.Framework.Is.SameAs(cauOffice1));
		}

		[ExpectNoExceptions]
		public void TestCreateDefaultEntrySettingHasChanges()
		{
			var collection = (EuOfficeCodeCollection)GetCollectionToTest();
			collection.DefaultPurposeCode = OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch;
			collection.CreateDefaultEntry(true);
			NUnit.Framework.Assert.That(collection.HasChanges, NUnit.Framework.Is.EqualTo(false));
			collection.RemoveAll();
			collection.CreateDefaultEntry(false);
			NUnit.Framework.Assert.That(collection.HasChanges, NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestDefaultPurposeCode()
		{
			var collection = (EuOfficeCodeCollection)GetCollectionToTest();

			collection.CreateDefaultEntry(false);
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(0));

			collection.DefaultPurposeCode = OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch;

			collection.CreateDefaultEntry(false);
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(1));

			var defaultCode = collection[0];

			var newCode = collection.AddNew();
			newCode.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(2));

			collection.RemoveAndDelete(defaultCode);
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(2));

			collection.RemoveAndDelete(newCode);
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(collection[0], NUnit.Framework.Is.EqualTo(defaultCode));

			var duplicateDefaultCode = collection.AddNew();
			duplicateDefaultCode.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch;
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(2));

			collection.RemoveAndDelete(duplicateDefaultCode);
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(collection[0], NUnit.Framework.Is.EqualTo(defaultCode));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = (BaseJobDeclaration)Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();
			return new EuOfficeCodeCollection(parent);
		}

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelperForTest(this);
		}

		class JobDeclarationCustomsOfficeRequirementHelperForTest : JobDeclarationCustomsOfficeRequirementHelper
		{
			public JobDeclarationCustomsOfficeRequirementHelperForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
			{
				if (Declaration.IsImport)
				{
					return new List<CustomsOfficeRequirement>
						{
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true, false)
						};
				}
				if (Declaration.IsExport)
				{
					return new List<CustomsOfficeRequirement>
						{
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true, false),
							new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfDeparture, true, false)
						};
				}
				return base.GetOtherRequirements();
			}
		}
	}
}
