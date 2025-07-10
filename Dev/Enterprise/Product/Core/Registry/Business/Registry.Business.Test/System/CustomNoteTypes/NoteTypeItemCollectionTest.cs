using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomNoteTypeItemCollection))]
	sealed class NoteTypeItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CustomNoteTypeItemCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CustomNoteTypeItemCollection GetCollectionToTest()
		{
			return new CustomNoteTypeItemCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomNoteTypeItem();
		}

		public void TestToNoteTypeCollectionSetsIsCustom()
		{
			CustomNoteTypeItem type1 = new CustomNoteTypeItem();
			type1.NoteName = "Type 1";
			type1.IsTextOnly = ZBool.True;
			type1.IsAppendingNote = ZBool.True;
			type1.IsReadOnlyAfterAdd = ZBool.False;
			type1.ForceRead = ZBool.False;
			type1.DefaultVisibility = nameof(StmNoteVisibility.PRV);

			CustomNoteTypeItem type2 = new CustomNoteTypeItem();
			type2.NoteName = "Type 2";
			type2.IsTextOnly = ZBool.False;
			type2.IsAppendingNote = ZBool.False;
			type2.IsReadOnlyAfterAdd = ZBool.True;
			type2.ForceRead = ZBool.True;
			type2.DefaultVisibility = nameof(StmNoteVisibility.PUB);

			CustomNoteTypeItemCollection collection = new CustomNoteTypeItemCollection();
			collection.Add(type1);
			collection.Add(type2);
			foreach (PredefinedNoteType type in collection.ToNoteTypeCollection())
			{
				Assert(type.IsCustomNoteType);
			}
		}
	}
}
