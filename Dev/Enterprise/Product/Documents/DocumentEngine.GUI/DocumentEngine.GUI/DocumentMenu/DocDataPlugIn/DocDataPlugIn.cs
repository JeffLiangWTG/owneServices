using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn
{
	public class DocDataPlugIn : ZPlugIn
	{
		public DocDataPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			if (hostBusinessEntity is IBusinessObjectCollection)
			{
				IBusinessObjectCollection collection = ((IBusinessObjectCollection)hostBusinessEntity);
				if (collection.Count > 0)
				{
					InputBusinessObject = (BusinessObject)((IList)collection)[0];
				}
			}
			else
			{
				InputBusinessObject = (BusinessObject)hostBusinessEntity;
			}

			if (!(InputBusinessObject is NonPersistentBusinessObject))
			{
				if (!(InputBusinessObject is IStmNoteParent))
				{
					throw new ArgumentException(string.Format(@"You cannot use the DocDataPlugin on a Host Persistent BusinessObject (of type ""{0}"") that does not implement IStmNoteParent. This is because the Plugin uses an StmNote to save the data the user enters.", InputBusinessObject.GetType()));
				}
				DocDataControl = new DocDataUserControl();
			}

			DocsMenu = new ZDocumentMenuItem();
			DocsMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MenuItem.Documents.NoDocumentsFound", "No Documents Found.")));
		}

		readonly DocDataUserControl DocDataControl;
		readonly BusinessObject InputBusinessObject;
		internal readonly ZDocumentMenuItem DocsMenu;

#if !WINZOR

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			if (Form == null)
			{
				throw new InvalidOperationException("Form should not be null on HookFormEventsCore.");
			}
			if (Form.Menu == null)
			{
				throw new InvalidOperationException("Form.Menu should not be null on HookFormEventsCore.");
			}

			((ZMainMenu)Form.Menu).CmdKeyPressed += DocDataPlugIn_CmdKeyPressed;
		}

		void DocDataPlugIn_CmdKeyPressed(object sender, CmdKeyEventArgs e)
		{
			if ((e.KeyData & Keys.Modifiers) == (Keys.Shift | Keys.Control))
			{
				SetupMenu();
			}
		}

#endif

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DocDataControl?.Dispose();
				DocsMenu?.Dispose();
				fNote?.Dispose();
				fNoteWithoutMutex?.Dispose();
			}

			base.Dispose(disposing);
		}

		public override bool ShouldHideTopLevelMenuWithTab => false;

		protected override MenuItem GetNewTopLevelMenu()
		{
			return DocsMenu;
		}
		bool isSetup;

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			if (Note != null)
			{
				Note.UpdateSDFsAndUDFsOnFactorySaving = true;
			}
		}

		public override void OnMenuShown()
		{
			SetupMenu();
			var parentForm = TabPage?.FindForm() ?? Form;
			DocsMenu.LoadMenus(parentForm);

			base.OnMenuShown();
		}

		void SetupMenu()
		{
			if (!isSetup && InputBusinessObject is IDocumentSupportable)
			{
				UserControlProviderList userDefinedFieldList = null;
				UserControlProviderList systemDefinedFieldList = null;
				if (NoteWithoutMutex != null)
				{
					userDefinedFieldList = NoteWithoutMutex.UserDefinedFieldList;
					systemDefinedFieldList = NoteWithoutMutex.GetSystemDefinedFieldList();
				}

				DocsMenu.Setup((IDocumentSupportable)InputBusinessObject, DocumentEventsForMenu, userDefinedFieldList, systemDefinedFieldList);

				var parentForm = TabPage?.FindForm() ?? Form;
				DocsMenu.LoadMenus(parentForm);
			}
			isSetup = true;
		}

		public override bool ShouldBeReadOnly => Note == null;

		protected DocumentNote NoteWithoutMutex
		{
			get
			{
				if (fNoteWithoutMutex == null && InputBusinessObject is IStmNoteParent)
				{
					fNoteWithoutMutex = DocumentNote.LoadNote((IStmNoteParent)InputBusinessObject);
				}
				return fNoteWithoutMutex;
			}
		}
		DocumentNote fNoteWithoutMutex;

		protected DocumentNote Note
		{
			get
			{
				if (fNote == null && InputBusinessObject is IStmNoteParent)
				{
					fNote = DocumentNote.LoadNoteWithExclusiveMutex((IStmNoteParent)InputBusinessObject);
				}
				return fNote;
			}
		}
		DocumentNote fNote;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return NoteWithoutMutex != null;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				return ResString.GetMultilingualString("67c776d3-0aa9-4b50-9ae4-eeb1a69ef40f", "Another user is currently accessing this document note.");
			}
		}

		public override BusinessObjectFactory Factory
		{
			get { return InputBusinessObject.Factory; }
		}

		public override void Delete()
		{
			if (fNote == null && InputBusinessObject is IStmNoteParent)
			{
				fNote = DocumentNote.RetrieveNote((IStmNoteParent)InputBusinessObject);
			}
			base.Delete();
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return DocDataControl;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return NoteWithoutMutex;
		}

		protected override CargoWise.Types.ZBool HasUserControl
		{
			get { return DocDataControl != null; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override string Name
		{
			get { return (NoResString)"Doc Data"; }
		}

		#region IDocumentEvents Members

		public DocumentEventsForMenu DocumentEventsForMenu
		{
			get
			{
				return documentEventsForMenu ??= new DocumentEventsForMenu(InputBusinessObject);
			}
		}

		DocumentEventsForMenu documentEventsForMenu;
		#endregion
	}
}
