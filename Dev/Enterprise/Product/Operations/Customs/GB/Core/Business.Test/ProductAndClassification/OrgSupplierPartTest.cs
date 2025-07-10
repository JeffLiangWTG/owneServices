using System;
using System.Linq;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	public class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, pivot.CI_RN_NKCountry);
			}
		}

		public void TestMultipleKeysToUse()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(1, part.MultipleKeysToUse.Count);
			AssertEquals(JobDeclaration.MultipleKeyCdsExport, part.MultipleKeysToUse[0]);

			pivot.Delete();
			AssertEquals(1, part.MultipleKeysToUse.Count);
			AssertEquals(JobDeclaration.MultipleKeyCdsImport, part.MultipleKeysToUse[0]);
		}

		public void TestIOrgSupplierPartImplements()
		{
			var part = Factory.New<OrgSupplierPart>();
			AssertSame(part.PivotsForBinding, ((Integration.Customs.GB.IOrgSupplierPart)part).PivotsForBinding);

			part.AddNote("TEST", "PUB");
			AssertEquals(true, part.Notes.GetAllNotes().Cast<StmNote>().Any(note => note.ST_NoteText == "TEST" && note.ST_NoteType == "PUB"));
		}

		protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);
	}
}
