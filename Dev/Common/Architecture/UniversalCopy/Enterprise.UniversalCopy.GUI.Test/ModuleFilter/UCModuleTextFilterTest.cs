using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	[TestedType(typeof(UCModuleTextFilter))]
	public class UCModuleTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUCModuleTextFilter()
		{
			var moduleTextFilter = new UCModuleTextFilter(StmNoteSchema.ST_NoteType.Name, StmNoteSchema.ST_NoteType, DummyList);
			Assert("Should have a list", moduleTextFilter.List is CodeDescriptionPairList);
			Assert("Validation should be UCModuleTextFilterValidation", moduleTextFilter.Validation is UCModuleTextFilterValidation);
		}

		#region Implementation

		CodeDescriptionPairList DummyList
		{
			get
			{
				var dummyList = new CodeDescriptionPairList();
				dummyList.AddPair(StmNoteDescription.Pub, StmNoteDescription.PubDescriptive);
				dummyList.AddPair(StmNoteDescription.Int, StmNoteDescription.IntDescriptive);
				dummyList.AddPair(StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive);
				dummyList.AddPair(StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive);
				return dummyList;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UCModuleTextFilter(StmNoteSchema.ST_NoteType.Name, StmNoteSchema.ST_NoteType, DummyList);
		}

		#endregion
	}
}
