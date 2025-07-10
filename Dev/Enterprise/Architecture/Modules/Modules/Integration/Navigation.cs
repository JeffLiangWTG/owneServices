using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Core.Modules
{
	public interface IIdentifiable
	{
		string ID { get; }
	}

	public interface INamedModule : ISecuredModule, IIdentifiable
	{
		MultilingualString Description { get; }
		ModuleIdentifier ModuleID { get; }
	}

	public interface IZModule : INamedModule, IDisposable
	{
		bool AllowNew { get; }
		bool BypassParentSecurityCheckpointVerification { get; }
		bool CouldAllowUniversalCopy { get; }
		bool SupportsConversations { get; }
		BusinessContext[] BusinessContexts { get; }
		bool SupportsWorkflow { get; }
		ISecurityCheckpoint GetWorkflowTasksCheckpoint(string workflowTasksCode);
		ISecurityCheckpoint GetWorkflowTasksCheckpoint(IBusiness parentJob, string workflowTasksCode);
	}

	public interface IZPopupModule : IZModule
	{
	}

	public interface IMainFormModule : INamedModule
	{
		string ModuleTreeID { get; }
		ModuleSection ParentSection { get; set; }
		IZModule CreateZModule();
		bool IsPopup { get; }
		MultilingualString ExtendedDescription { get; }
	}

	public class ModuleTree
	{
		public static ModuleTree Tree
		{
			get
			{
				if (fModuleTree == null)
				{
					fModuleTree = new ModuleTree();
				}
				return fModuleTree;
			}
		}
		[ThreadSafe] //going to be initialized at a known time
		static ModuleTree fModuleTree;

		readonly ModuleCategoryCollection fCategories = new ModuleCategoryCollection();

		public ModuleCategoryCollection Categories
		{
			get { return fCategories; }
		}

		public IMainFormModule FindByID(string iD)
		{
			foreach (ModuleCategory category in Categories.Values)
			{
				foreach (ModuleSection section in category.Sections.Values)
				{
					foreach (IMainFormModule module in section.Modules.Values)
					{
						if (iD.Equals(module.ID, StringComparison.InvariantCultureIgnoreCase))
						{
							return module;
						}
					}
				}
			}
			return null;
		}

		public IEnumerable<IMainFormModule> FindByModuleTreeID(string moduleTreeId, bool includeHidden = false)
		{
			foreach (ModuleCategory category in includeHidden ? Categories.ValuesIncludingHidden : Categories.Values.Cast<ModuleCategory>())
			{
				foreach (ModuleSection section in includeHidden ? category.Sections.ValuesIncludingHidden : category.Sections.Values.Cast<ModuleSection>())
				{
					foreach (IMainFormModule module in includeHidden ? section.Modules.ValuesIncludingHidden : section.Modules.Values.Cast<IMainFormModule>())
					{
						if (moduleTreeId.Equals(module.ModuleTreeID, StringComparison.InvariantCultureIgnoreCase))
						{
							yield return module;
						}
					}
				}
			}
		}

		#region Test
#if DEBUG

		public static IDisposable OverrideTreeForTest(ModuleTree moduleTree)
		{
			var previousTree = Tree;
			fModuleTree = moduleTree;
			return new DisposableAction(() => fModuleTree = previousTree);
		}

#endif
		#endregion
	}

	#region Base

	public class ModuleNodeBase : IIdentifiable
	{
		public ModuleNodeBase(string name, MultilingualString displayText, ISecurityCheckpoint checkPoint)
		{
			Name = name;
			DisplayText = displayText;
			SecurityCheckpoint = checkPoint;
			if (SecurityCheckpoint != null && DisplayText != null)
			{
				SecurityCheckpoint.SetDisplayText((NoResString)DisplayTextWithoutAmpersand);
			}
		}

		public string DisplayTextWithoutAmpersand
		{
			get { return RemoveAmpersand(DisplayText); }
		}

		public string UnresolvedDisplayTextWithoutAmpersand
		{
			get { return RemoveAmpersand(DisplayText.GetUnresolvedString()); }
		}

		static string RemoveAmpersand(string mnemonicString)
		{
			string temp = "ZZZZZ";
			string result = mnemonicString.Replace("&&", temp);
			result = result.Replace("&", "");
			result = result.Replace(temp, "&");
			return result;
		}

		public readonly string Name;
		public readonly MultilingualString DisplayText;
		public readonly ISecurityCheckpoint SecurityCheckpoint;

		string IIdentifiable.ID => Name;
	}

	public abstract class ModuleNodeCollectionBase<TNodeValue> : DictionaryBase
		where TNodeValue : class, IIdentifiable
	{
		protected ModuleNodeCollectionBase()
		{
		}

		public IReadOnlyCollection<TNodeValue> Values
		{
			get { return nodes.Where(n => !n.IsHidden).Select(n => n.Value).ToArray(); }
		}

		protected void Add(string key, TNodeValue value)
		{
			AddToDictionary(key, value);
			nodes.Add(new Node(value));
		}

		public bool ContainsKey(string key)
		{
			return Dictionary[key] != null;
		}

		protected override void OnClear()
		{
			nodes.Clear();
		}

		public void Add(TNodeValue item)
		{
			Add(item.ID, item);
			OnAdded(item);
		}

		protected virtual void OnAdded(TNodeValue item)
		{
		}

		public void AddIf(bool condition, Func<TNodeValue> itemGetter)
		{
			if (condition)
			{
				var item = itemGetter();
				if (item is not null)
				{
					Add(item);
				}
			}
			else
			{
				nodes.Add(new Node(itemGetter));
			}
		}

		public TNodeValue this[string categoryName]
		{
			get { return Dictionary[categoryName] as TNodeValue; }
		}

		public void InsertBefore(string keyOfNodeToInsertBefore, TNodeValue item)
		{
			var index = FindIndexOfKey(keyOfNodeToInsertBefore);
			nodes.Insert(index, new Node(item));
			AddToDictionary(item.ID, item);
			OnAdded(item);
		}

		public void InsertBeforeIf(string keyOfNodeToInsertBefore, bool condition, Func<TNodeValue> itemGetter)
		{
			if (condition)
			{
				var item = itemGetter();
				if (item is not null)
				{
					InsertBefore(keyOfNodeToInsertBefore, item);
				}
			}
			else
			{
				var index = FindIndexOfKey(keyOfNodeToInsertBefore);
				nodes.Insert(index, new Node(itemGetter));
			}
		}

		public void InsertAfter(string keyOfNodeToInsertAfter, TNodeValue item)
		{
			var index = FindIndexOfKey(keyOfNodeToInsertAfter);
			nodes.Insert(index + 1, new Node(item));
			AddToDictionary(item.ID, item);
			OnAdded(item);
		}

		public void InsertAfterIf(string keyOfNodeToInsertAfter, bool condition, Func<TNodeValue> itemGetter)
		{
			if (condition)
			{
				var item = itemGetter();
				if (item is not null)
				{
					InsertAfter(keyOfNodeToInsertAfter, item);
				}
			}
			else
			{
				var index = FindIndexOfKey(keyOfNodeToInsertAfter);
				nodes.Insert(index + 1, new Node(itemGetter));
			}
		}

		int FindIndexOfKey(string key)
		{
			var index = nodes.IndexOf(n => n.Value?.ID == key);
			if (index == -1)
			{
				throw new KeyNotFoundException("Cannot find existing item with key " + key);
			}
			return index;
		}

		void AddToDictionary(string key, TNodeValue value)
		{
			if (ContainsKey(key))
			{
				throw new ModuleAlreadyExistsException($"Node '{key}' already exists.");
			}

			Dictionary.Add(key, value);
		}

		#region HiddenValues

		public IReadOnlyCollection<TNodeValue> HiddenValues
		{
			get
			{
				if (hiddenValues == null)
				{
					hiddenValues = new List<TNodeValue>();
					foreach (var node in nodes.Where(n => n.IsHidden))
					{
						var item = node.Value;
						if (node.Value is not null)
						{
							OnAdded(item);
							hiddenValues.Add(item);
						}
					}
				}

				return hiddenValues;
			}
		}
		List<TNodeValue> hiddenValues;

		public IEnumerable<TNodeValue> ValuesIncludingHidden
		{
			get { return Values.Cast<TNodeValue>().Concat(HiddenValues); }
		}

		public IEnumerable<TNodeValue> ValuesIncludingHiddenInRegistrationOrder => nodes.Select(n => n.Value);

		#endregion

		class Node
		{
			public Node(TNodeValue value)
			{
				this.value = value;
			}

			public Node(Func<TNodeValue> valueGetter)
			{
				this.valueGetter = valueGetter;
				IsHidden = true;
			}

			public bool IsHidden { get; }

			public TNodeValue Value
			{
				get
				{
					if (value is null && valueGetter is not null)
					{
						value = valueGetter();
					}
					return value;
				}
			}

			TNodeValue value;
			readonly Func<TNodeValue> valueGetter;
		}

		readonly List<Node> nodes = [];
	}

	#endregion

	#region Category

	public class ModuleCategory : ModuleNodeBase
	{
		public ModuleCategory(ModuleTreeLoaderConstant.Entry entry, ISecurityCheckpoint checkPoint)
			: this(entry.Name, entry.DisplayText, checkPoint)
		{ }

		public ModuleCategory(string name, MultilingualString displayText, ISecurityCheckpoint checkPoint)
			: base(name, displayText, checkPoint)
		{
			fSections = new ModuleSectionCollection(this);
		}

		public ModuleSectionCollection Sections
		{
			get { return fSections; }
		}

		readonly ModuleSectionCollection fSections;
	}

	public class ModuleCategoryCollection : ModuleNodeCollectionBase<ModuleCategory>
	{
		public ModuleCategoryCollection()
		{
		}
	}

	#endregion

	#region Section

	public class ModuleSection : ModuleNodeBase
	{
		public ModuleSection(ModuleTreeLoaderConstant.Entry entry, string customerServiceMenuSectionCode, ISecurityCheckpoint checkPoint, IconTypes icon, IconTypes groupImage, ModuleTreeLoaderConstant.Entry subcategory)
			: this(entry.Name, entry.DisplayText, customerServiceMenuSectionCode, checkPoint, icon, groupImage, subcategory)
		{
		}

		public ModuleSection(ModuleTreeLoaderConstant.Entry entry, string customerServiceMenuSectionCode, ISecurityCheckpoint checkPoint, IconTypes icon, IconTypes groupImage)
			: this(entry.Name, entry.DisplayText, customerServiceMenuSectionCode, checkPoint, icon, groupImage, null)
		{
		}

		public ModuleSection(string sectionName, MultilingualString displayText, string customerServiceMenuSectionCode, ISecurityCheckpoint checkPoint, IconTypes icon, IconTypes groupImage)
			: this(sectionName, displayText, customerServiceMenuSectionCode, checkPoint, icon, groupImage, null)
		{
		}

		public ModuleSection(string sectionName, MultilingualString displayText, string customerServiceMenuSectionCode, ISecurityCheckpoint checkPoint, IconTypes icon, IconTypes groupImage, ModuleTreeLoaderConstant.Entry subcategory)
			: base(sectionName, displayText, checkPoint)
		{
			Icon = icon;
			GroupImage = groupImage;
			CustomerServiceMenuSectionCode = customerServiceMenuSectionCode;
			Subcategory = subcategory;

			fModules = new OModuleCollection(this);
		}

		public ModuleCategory ParentCategory
		{
			get { return fParentCategory; }
			set { fParentCategory = value; }
		}

		public OModuleCollection Modules
		{
			get { return fModules; }
		}

		public ModuleTreeLoaderConstant.Entry Subcategory { get; private set; }

		public readonly string CustomerServiceMenuSectionCode;
		public readonly IconTypes Icon;
		public readonly IconTypes GroupImage;

		readonly OModuleCollection fModules;
		ModuleCategory fParentCategory;
	}

	public class ModuleSectionAddOn : ModuleSection, IModuleSectionAddOn
	{
		public ModuleSectionAddOn(string categoryName, string sectionName, MultilingualString displayText, string customerServiceMenuSection, ISecurityCheckpoint checkPoint, IconTypes icon, IconTypes groupImage, ModuleTreeLoaderConstant.Entry subcategory)
			: base(sectionName, displayText, customerServiceMenuSection, checkPoint, icon, groupImage, subcategory)
		{
			fCategoryName = categoryName;
		}

		#region IModuleSectionAddOn Members

		public string CategoryName
		{
			get { return fCategoryName; }
		}

		readonly string fCategoryName;

		#endregion
	}

	public class ModuleSectionCollection : ModuleNodeCollectionBase<ModuleSection>
	{
		public ModuleSectionCollection(ModuleCategory parentCategory)
		{
			ParentCategory = parentCategory;
		}

		protected override void OnAdded(ModuleSection item)
		{
			base.OnAdded(item);
			item.ParentCategory = ParentCategory;
		}

		protected readonly ModuleCategory ParentCategory;
	}

	#endregion

	#region Implementation

	public class OModuleCollection : ModuleNodeCollectionBase<IMainFormModule>
	{
		public OModuleCollection(ModuleSection parentSection)
		{
			ParentSection = parentSection;
		}

		protected override void OnAdded(IMainFormModule item)
		{
			base.OnAdded(item);
			item.ParentSection = ParentSection;
		}

		protected readonly ModuleSection ParentSection;
	}

	[Serializable]
	public class ModuleAlreadyExistsException : Exception
	{
		public ModuleAlreadyExistsException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ModuleAlreadyExistsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion
}
