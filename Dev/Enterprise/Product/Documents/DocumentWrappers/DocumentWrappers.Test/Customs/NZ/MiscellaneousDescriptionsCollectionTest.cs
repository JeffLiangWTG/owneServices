using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(MiscellaneousDescriptionsCollection))]
	sealed class MiscellaneousDescriptionsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MiscellaneousDescriptionsCollection>
	{
		public void TestCollection()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_GoodsDescription = "GOODS";
			declaration.Notes.AddNew();

			StmNote note = declaration.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;

			ZStringBuilder marks = new ZStringBuilder();
			for (int i = 0; i < 30; i++)
			{
				marks.Append("Mark" + i);
			}
			note.ST_NoteText = marks.ToStringWithNewLineBetweenAppends();
			Factory.Save();

			DocDeclaration declarationWrapper = DocDeclaration.New(declaration, Factory);
			MiscellaneousDescriptionsCollection descriptions = new MiscellaneousDescriptionsCollection(declarationWrapper);
			descriptions.Load();
			AssertEquals(2, descriptions.Count);

			ZStringBuilder expectedString1 = new ZStringBuilder();
			expectedString1.Append("GOODS");
			for (int i = 0; i < 24; i++)
			{
				expectedString1.Append("Mark" + i);
			}
			AssertEquals(expectedString1.ToStringWithNewLineBetweenAppends(), descriptions[0].Description);

			ZStringBuilder expectedString2 = new ZStringBuilder();
			for (int i = 24; i < 30; i++)
			{
				expectedString2.Append("Mark" + i);
			}
			AssertEquals(expectedString2.ToStringWithNewLineBetweenAppends(), descriptions[1].Description);
		}

		protected override MiscellaneousDescriptionsCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.New(Factory);
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			DocDeclaration decWrapper = DocDeclaration.New(declaration, Factory);
			return new MiscellaneousDescriptionsCollection(decWrapper);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MiscellaneousDescription(Factory, "Description of goods");
		}
	}
}
