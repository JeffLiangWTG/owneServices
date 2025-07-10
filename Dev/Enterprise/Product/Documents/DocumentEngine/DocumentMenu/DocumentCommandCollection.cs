using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine
{
	public class DocumentCommandCollection : StmMenuItemBaseCollection, IDocumentCommandCollection
	{
		/// <summary>
		/// Creates a document menu item collection
		/// </summary>
		/// <param name="parent">The main parent business object that the documents will run off</param>
		public DocumentCommandCollection(IDocumentSupportable parent)
			: base(parent.DocumentSupporter.Factory)
		{
			this.Parent = parent;
		}

		public DocumentCommandCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public DocumentCommandCollection(IDocumentSupportable parent, bool includeForms)
			: base(parent.DocumentSupporter.Factory, includeForms)
		{
			this.Parent = parent;
		}

		internal DocumentCommandCollection(IDocumentSupportable parent, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = parent;
		}

		public new DocumentCommand this[int index]
		{
			get { return (DocumentCommand)Elements[index]; }
		}

		protected override Core.Constants.DataContext DataContext
		{
			get { return Parent.DocumentSupporter.DefaultDataContext; }
		}

		public new DocumentCommand AddNew()
		{
			return (DocumentCommand)base.AddNew();
		}

		public new DocumentCommand AddNew(Type bizObjType)
		{
			return (DocumentCommand)base.AddNew(bizObjType);
		}

		public readonly IDocumentSupportable Parent;

		public override void Load()
		{
			var filter = GetApplicableMenusFilter(Parent.DocumentSupporter.BusinessContext.ToString(), true, includeForms);
			Parent.DocumentSupporter.CustomizeApplicableMenusFilter(filter);
			LoadMenus(filter);
		}

		#region DocumentCommandFilters Cache

		public static IDisposable EnableCacheDocumentsCommandFilters()
		{
			dictMacroValue = new Dictionary<string, string>();

			return new DisposableAction(() =>
			{
				if (dictMacroValue is null)
				{
					return;
				}

				dictMacroValue.Clear();
				dictMacroValue = null;
			});
		}

		public static bool TryGetCachedMacroValue(string macro, out string result)
		{
			if (dictMacroValue == null)
			{
				result = null;
				return false;
			}
			return dictMacroValue.TryGetValue(macro, out result);
		}

		public static void CacheMacroValue(string macro, string result)
		{
			if (dictMacroValue != null && !dictMacroValue.ContainsKey(macro))
			{
				dictMacroValue[macro] = result;
			}
		}

		[ThreadStatic]
		static Dictionary<string, string> dictMacroValue;

		#endregion

		public List<DocumentCommand> GetApplicableDocumentCommands()
		{
			var filterEvaluatedResult = new Dictionary<(string filter, string type), bool>();
			var result = new List<DocumentCommand>();
			bool isDocumentCommandApplicable;

			using (EnableCacheDocumentsCommandFilters())
			{
				foreach (DocumentCommand documentCommand in this)
				{
					if (filterEvaluatedResult.ContainsKey((documentCommand.SU_FilterList, documentCommand?.SU_MenuType.ToString())))
					{
						isDocumentCommandApplicable = filterEvaluatedResult[(documentCommand.SU_FilterList, documentCommand?.SU_MenuType.ToString())];
					}
					else
					{
						isDocumentCommandApplicable = documentCommand.IsApplicable;
						filterEvaluatedResult.Add((documentCommand.SU_FilterList, documentCommand?.SU_MenuType.ToString()), isDocumentCommandApplicable);
					}

					if (isDocumentCommandApplicable)
					{
						result.Add(documentCommand);
					}
				}
			}

#if DEBUG
			FilterEvaluatedResult = filterEvaluatedResult;
#endif

			return result;
		}

#if DEBUG
		public Dictionary<(string filter, string type), bool> FilterEvaluatedResult;
#endif

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((StmMenuItemBase)child).SU_BusinessContext = Parent.DocumentSupporter.BusinessContext.ToString();
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			DocumentCommand documentCommand = child as DocumentCommand;
			if (documentCommand != null)
			{
				documentCommand.Parent = Parent;
			}
		}

		#endregion
	}
}
