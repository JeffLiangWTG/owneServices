
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.CustomerService.Business
{
	public class ConversationNote : HiddenStmNote
	{
		public ConversationNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded note identification.")]
		public const string Description = "Conversation";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ST_Description = Description;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (ST_NoteText.IsEmpty && !Globals.IsTest)
			{
				ErrorReporter.ReportOnce("ConversationNoteIsBlank", "PK=" + PK + " ParentID=" + ST_ParentID);
			}
		}

#if DEBUG
		[BusinessObjectTestExclude] // This property calls through to ST_NoteType which is fixed as "DOC" will report a dev error during testing.
		public new ZString ST_NoteType_DescriptiveText
		{
			get { return base.ST_NoteType_DescriptiveText; }
			set { base.ST_NoteType_DescriptiveText = value; }
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ST_NoteText = "<Message />";
		}
#endif
	}
}
