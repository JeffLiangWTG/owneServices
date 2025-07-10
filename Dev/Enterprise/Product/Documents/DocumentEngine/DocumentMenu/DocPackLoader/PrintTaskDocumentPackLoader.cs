using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine
{
	public class PrintTaskDocumentPackLoader
	{
		public PrintTaskDocumentPackLoader(PrintTask task, DocumentCommand command, UserControlProviderList userFieldList, Guid[] documentSuppressionList = null, bool shouldCreateeDocs = true)
		{
			this.task = task;
			this.command = command;
			this.userFieldList = userFieldList;
			this.addedCommands = new List<DocumentPackId>();
			this.addedPacks = new List<DocumentPackId>();
			this.reasonsForEmptyPacks = new List<string>();
			if (command != null && !command.SU_MenuDataContext.IsEmpty && command.Parent != null && command.Parent.DocumentSupporter != null)
			{
				if (!DataContextValue.IsValidFullDataContext(command.SU_MenuDataContext))
				{
					throw new DocumentMenuException(Res.GetString("512d59a2-98a1-4f05-90fc-513ce05aa465", "The {0} of \"{1}\" for document \"{2}\" is not valid. Please check the Delivery Options of this document", command.SU_MenuDataContextInfo.HumanReadableName, command.SU_MenuDataContext, command.SU_MenuName));
				}
			}
			this.documentSuppressionList = documentSuppressionList;
			this.shouldCreateeDocs = shouldCreateeDocs;
		}

		internal PrintTask Task
		{
			get { return task; }
		}
		readonly PrintTask task;

		internal DocumentCommand Command
		{
			get { return command; }
		}
		readonly DocumentCommand command;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly UserControlProviderList userFieldList;

		readonly Guid[] documentSuppressionList;

		internal List<DocumentPackId> AddedPacks
		{
			get { return addedPacks; }
		}
		readonly List<DocumentPackId> addedPacks;

		internal List<DocumentPackId> AddedCommands
		{
			get { return addedCommands; }
		}
		readonly List<DocumentPackId> addedCommands;

		public List<string> ReasonsForEmptyPacks
		{
			get { return reasonsForEmptyPacks; }
		}
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly List<string> reasonsForEmptyPacks;

		public UserControlProviderList UserFieldList
		{
			get { return userFieldList; }
		}

		public void LoadAll()
		{
			Load(command, UserFieldList, null);
		}

		public void LoadChildCommands()
		{
			ChildCommandsLoader.LoadChildCommands(command, this);
		}

		public void LoadChildCommandsFromProviderPlaceholder()
		{
			ChildCommandsLoader.LoadChildCommandsFromProviderPlaceholder(command, this);
		}

		internal void Load(DocumentCommand currentCommand, UserControlProviderList currentUserFieldList, DocumentCommand parentCommand, ZGuid sourcePivotPK = default(ZGuid), string language = "")
		{
			var currentCommandShouldAddEDocs = shouldCreateeDocs && currentCommand.SU_MenuType != Core.Constants.StmMenuItemTypes.Forms;
			var pack = new DocumentPack(currentCommand, currentCommand.Parent, currentUserFieldList, parentCommand, currentCommandShouldAddEDocs, documentSuppressionList, language);
			pack.Loader = this;
			Load(pack, currentCommand, currentUserFieldList, parentCommand, sourcePivotPK);
		}

		internal void Load(DocumentPack pack, DocumentCommand currentCommand, UserControlProviderList currentUserFieldList, DocumentCommand parentCommand, ZGuid sourcePivotPK = default(ZGuid))
		{
			DocumentPackLoader.Load(this, task, pack, currentCommand, command, sourcePivotPK);
		}

		internal void ResetForReload()
		{
			AddedCommands.Clear();
			AddedPacks.Clear();
			Task.Clear();
		}

		readonly bool shouldCreateeDocs;

#if DEBUG
		internal int ChildCommandMetFilterCount { get; set; }
#endif
	}
}
