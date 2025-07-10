using System;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyBizOWithRelatedNotes : DummyEnterpriseBusinessObject, INoteSource, IStmNoteParentWithSystemNote
	{
		public DummyBizOWithRelatedNotes(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Dummy Predefined Note Types

		public static PredefinedNoteType GreenNoteType
		{
			get
			{
				if (fGreenNoteType == null)
				{
					fGreenNoteType = new PredefinedNoteType(ResString._GetMultilingualString(ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, "green", "green is unique"), StmNoteVisibility.PUB, true, false, false, false);
				}

				return fGreenNoteType;
			}
		}

		public static PredefinedNoteType WhiteNoteType
		{
			get
			{
				if (fWhiteNoteType == null)
				{
					fWhiteNoteType = new PredefinedNoteType(ResString._GetMultilingualString(ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, "white", "white is *not* unique"), StmNoteVisibility.PUB, false, false, false, false);
				}

				return fWhiteNoteType;
			}
		}

		public static PredefinedNoteType BlackNoteTypeIsNotEditable
		{
			get
			{
				if (fBlackNoteTypeIsNotEditable == null)
				{
					fBlackNoteTypeIsNotEditable = new PredefinedNoteType(ResString._GetMultilingualString(ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, "black", "black is *not* editable"), StmNoteVisibility.PUB, false, true, false, false);
				}

				return fBlackNoteTypeIsNotEditable;
			}
		}

		public static PredefinedNoteType PurpleNoteTypeIsNotEditableAndTextOnly
		{
			get
			{
				if (fPurpleNoteTypeIsNotEditableAndTextOnly == null)
				{
					fPurpleNoteTypeIsNotEditableAndTextOnly = new PredefinedNoteType(ResString._GetMultilingualString(ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, "purple",
						"purple is not editable and text only"), StmNoteVisibility.PUB, false, true, true, false);
				}

				return fPurpleNoteTypeIsNotEditableAndTextOnly;
			}
		}

		static PredefinedNoteType fGreenNoteType;
		static PredefinedNoteType fWhiteNoteType;
		static PredefinedNoteType fBlackNoteTypeIsNotEditable;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "set once")]
		static PredefinedNoteType fPurpleNoteTypeIsNotEditableAndTextOnly;

		#endregion

		#region Related Dummy + Dummies

		public new DummyEnterpriseBusinessObject RelatedDummy
		{
			get
			{
				if (fRelatedDummy == null)
				{
					fRelatedDummy = Factory.New<DummyEnterpriseBusinessObject>();
				}
				return fRelatedDummy;
			}
		}
		DummyEnterpriseBusinessObject fRelatedDummy;

		public DummyBizOWithRelatedNotes RelatedDummyImplementsINoteSource
		{
			get
			{
				if (fRelatedDummyImplementsINoteSource == null)
				{
					fRelatedDummyImplementsINoteSource = Factory.New<DummyBizOWithRelatedNotes>();
				}
				return fRelatedDummyImplementsINoteSource;
			}
		}
		DummyBizOWithRelatedNotes fRelatedDummyImplementsINoteSource;

		public DummyBusinessObjectCollection RelatedDummies
		{
			get
			{
				if (fRelatedDummies == null)
				{
					fRelatedDummies = new DummyBusinessObjectCollection(Factory);
				}
				return fRelatedDummies;
			}
		}
		DummyBusinessObjectCollection fRelatedDummies;

		#endregion

		#region INoteSource Members

		public ZString NoteSourceName
		{
			get { return "Sample Note Source"; }
		}

		#endregion

		#region IStmNoteParent

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection result = new NoteTypeCollection();
				result.Add(GreenNoteType);
				result.Add(WhiteNoteType);
				result.Add(BlackNoteTypeIsNotEditable);
				result.Add(PurpleNoteTypeIsNotEditableAndTextOnly);
				return result;
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get { return new BusinessObject[] { RelatedDummy }; }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get { return GetBusinessObjectsWithRelatedNotes(); }
		}

		protected virtual BusinessObject[] GetBusinessObjectsWithRelatedNotes()
		{
			return new BusinessObject[] { RelatedDummy, RelatedDummyImplementsINoteSource };
		}

		#endregion

		#region IStmNoteParentWithSystemNote
		bool IStmNoteParentWithSystemNote.IsSystemNote(StmNote note)
		{
			return isSystemNoteForTesting != null && isSystemNoteForTesting(note);
		}
		public Func<StmNote, bool> isSystemNoteForTesting;

		#endregion
	}
}
