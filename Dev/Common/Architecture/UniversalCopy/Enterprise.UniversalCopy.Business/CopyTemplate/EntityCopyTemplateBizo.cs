using System;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalCopy.Business
{
	public abstract class EntityCopyTemplateBizo : CopyTemplateNodeBizo
	{
		protected EntityCopyTemplateBizo(WrappedCopyTemplateNode copyTemplateNode, CopyTemplateNode rootNode)
			: base(copyTemplateNode)
		{
			this.rootNode = rootNode;
		}

		public new WrappedCopyTemplateNode CopyTemplateNode
		{
			get { return (WrappedCopyTemplateNode)base.CopyTemplateNode; }
		}

		readonly CopyTemplateNode rootNode;

		#region Children

		#region Child Nodes

		public EntityCopyTemplateBizoCollection ChildNodes
		{
			get
			{
				if (childNodes == null)
				{
					childNodes = new EntityCopyTemplateBizoCollection(this);
					childNodes.LoadNodes(CopyTemplateNode, rootNode, ParentPropertyName);
					RegisterEditableChildObject(childNodes);
				}
				return childNodes;
			}
		}
		EntityCopyTemplateBizoCollection childNodes;

		public string ParentPropertyName { get; set; }

#if DEBUG
		internal EntityCopyTemplateBizoCollection ChildNodesNoCreateForTest
		{
			get { return childNodes; }
		}
#endif

		#endregion

		#region Property Nodes

		public PropertyCopyTemplateBizoCollection PropertyNodes
		{
			get
			{
				if (propertyNodes == null)
				{
					propertyNodes = new PropertyCopyTemplateBizoCollection(this);
					propertyNodes.LoadNodes(CopyTemplateNode, rootNode, ParentPropertyName);
					RegisterEditableChildObject(propertyNodes);
				}
				return propertyNodes;
			}
		}
		PropertyCopyTemplateBizoCollection propertyNodes;

#if DEBUG
		internal PropertyCopyTemplateBizoCollection PropertyNodesNoCreateForTest
		{
			get { return propertyNodes; }
		}
#endif

		#endregion

		#region Value Only Nodes

		public ValueOnlyPropertyCopyTemplateBizoCollection ValueOnlyPropertyNodes
		{
			get
			{
				if (valueOnlyPropertyNodes == null)
				{
					valueOnlyPropertyNodes = new ValueOnlyPropertyCopyTemplateBizoCollection(this);
					valueOnlyPropertyNodes.LoadNodes(CopyTemplateNode, rootNode, ParentPropertyName);
					RegisterEditableChildObject(valueOnlyPropertyNodes);
				}
				return valueOnlyPropertyNodes;
			}
		}
		ValueOnlyPropertyCopyTemplateBizoCollection valueOnlyPropertyNodes;

		#endregion

		protected internal abstract bool NeedsChildrenForCopy();

		#endregion

		#region Filter

		public FilterBusinessObject FilterStripBizo
		{
			get { return filterStripBizo; }
			set
			{
				if (filterStripBizo != null)
				{
					UnRegisterEditableChildObject(filterStripBizo);
				}
				filterStripBizo = value;
				if (filterStripBizo != null)
				{
					RegisterEditableChildObject(filterStripBizo);
				}
			}
		}
		FilterBusinessObject filterStripBizo;

		public abstract EntityFilter EntityFilter { get; set; }

		#endregion

		#region EntityTableName

		public string EntityTableNameDescription
		{
			get
			{
				var rawTableName = EntityTableNameCore;
				return !string.IsNullOrEmpty(rawTableName) ? DataBoundResourceStrings.GetTableDescriptiveName(rawTableName) : rawTableName;
			}
		}

		public string EntityTableName
		{
			get { return EntityTableNameCore; }
		}

		protected abstract string EntityTableNameCore { get; }

		#endregion

		#region Description

		protected override string GetFriendlyName(string name)
		{
			var tableName = EntityTableName;
			if (name == tableName ||
				(name != null && tableName != null && name.Length == tableName.Length + 1 && name.EndsWith((NoResString)"s") && name.IndexOf(tableName, StringComparison.Ordinal) == 0))
			{
				var entityTableName = EntityTableNameDescription;
				return entityTableName == tableName ? base.GetFriendlyName(entityTableName) : entityTableName;
			}

			return base.GetFriendlyName(name);
		}

		public virtual string GetCopyActionDescription()
		{
			return Res.GetString("b1af7980-8380-4c83-9cf1-2b67289ac361", "Copy");
		}

		#endregion

		#region Kind

		public ZString Kind
		{
			get { return KindCore; }
		}

		protected abstract ZString KindCore { get; }

		public ZPropertyInfo KindInfo
		{
			get { return GetZPropertyInfo(nameof(Kind)); }
		}

		#endregion

		#region Defaulting

		protected override void DefaultToDoNotCopyCore()
		{
			if (CopyTemplateNode.HasData())
			{
				foreach (PropertyCopyTemplateBizo property in PropertyNodes)
				{
					property.DefaultToDoNotCopy();
				}

				foreach (EntityCopyTemplateBizo child in ChildNodes)
				{
					child.DefaultToDoNotCopy();
				}
			}
		}

		protected override void DefaultToCopyCore(int level)
		{
			foreach (PropertyCopyTemplateBizo property in PropertyNodes)
			{
				property.DefaultToCopy();
			}

			if (level > 0)
			{
				foreach (EntityCopyTemplateBizo child in ChildNodes)
				{
					child.DefaultToCopy(level - 1);
				}
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			if (CopyTemplateNode.HasData() && NeedsChildrenForCopy())
			{
				// Load child nodes and register them as editable children business objects
				var childNodes = ChildNodes;
				var propertyNodes = PropertyNodes;
			}

			base.RunPreSaveValidationCore();
		}

		#endregion
	}

	#region CopyTemplateNodeBizoCollection

	public abstract class CopyTemplateNodeBizoCollection<T> : NonPersistentBusinessObjectCollection<T>
		where T : CopyTemplateNodeBizo
	{
		public void LoadNodes(CopyTemplateNode parentNode, CopyTemplateNode rootNode, string skipNodeName)
		{
			RemoveAll();

			if (parentNode != null)
			{
				TemplateCopyTemplateNode templateNode = parentNode as TemplateCopyTemplateNode;
				if (templateNode != null)
				{
					try
					{
						templateNode.FindTemplateAndInitializeInnerNode(rootNode);
					}
					catch (InvalidOperationException ex)
					{
						var path = new StringBuilder();
						rootNode.FindPathToNode(parentNode, path, true);
						var message = string.Format(CultureInfo.InvariantCulture,
							(NoResString)"Error populating TemplateCopyTemplateNode with name {0} while loading nodes into {1}, located at:\r\n{2}", // Error report for developers
							templateNode.Name, GetType().Name, path);

						ErrorReporter.ReportOnce("InvalidOperationException_LoadNodes", message, ex);
						return;
					}
				}

				WrappedCopyTemplateNode wrappedNode = parentNode as WrappedCopyTemplateNode;
				if (wrappedNode != null)
				{
					LoadNodes(wrappedNode.InnerNode, rootNode, skipNodeName);
				}
				else
				{
					EntityCopyTemplateNode entityNode = parentNode as EntityCopyTemplateNode;
					if (entityNode != null)
					{
						LoadNodesCore(entityNode, rootNode, skipNodeName);
					}
				}
			}

			Sort(new Comparison<T>(Compare));
		}

		protected abstract void LoadNodesCore(EntityCopyTemplateNode parentNode, CopyTemplateNode rootNode, string skipNodeName);

		int Compare(T a, T b)
		{
			return string.CompareOrdinal(a.CopyTemplateNode.Name, b.CopyTemplateNode.Name);
		}

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}

	#endregion

	#region EntityCopyTemplateBizoCollection

	public class EntityCopyTemplateBizoCollection : CopyTemplateNodeBizoCollection<EntityCopyTemplateBizo>
	{
		public EntityCopyTemplateBizoCollection(EntityCopyTemplateBizo parent)
		{
			this.parent = parent;
		}

		readonly EntityCopyTemplateBizo parent;

		protected override void LoadNodesCore(EntityCopyTemplateNode parentNode, CopyTemplateNode rootNode, string skipNodeName)
		{
			foreach (CopyTemplateNode childNode in parentNode.Nodes)
			{
				RelatedEntityCopyTemplateNode relatedNode;
				CollectionCopyTemplateNode collectionNode;

				if ((relatedNode = childNode as RelatedEntityCopyTemplateNode) != null && (relatedNode.RelatedPropertyName != skipNodeName || string.IsNullOrEmpty(skipNodeName)))
				{
					Add(new RelatedEntityCopyTemplateBizo(relatedNode, rootNode, parent));
				}
				else if ((collectionNode = childNode as CollectionCopyTemplateNode) != null)
				{
					Add(new CollectionCopyTemplateBizo(collectionNode, rootNode, parent) { ParentPropertyName = collectionNode.ItemPropertyName });
				}
			}
		}
	}

	#endregion

	#region PropertyCopyTemplateBizoCollection

	public class PropertyCopyTemplateBizoCollection : CopyTemplateNodeBizoCollection<PropertyCopyTemplateBizo>
	{
		public PropertyCopyTemplateBizoCollection(EntityCopyTemplateBizo parentEntity) : base()
		{
			this.parentEntity = parentEntity;
		}

		readonly EntityCopyTemplateBizo parentEntity;

		protected override void LoadNodesCore(EntityCopyTemplateNode parentNode, CopyTemplateNode rootNode, string skipNodeName)
		{
			foreach (CopyTemplateNode childNode in parentNode.Nodes)
			{
				PropertyCopyTemplateNode propertyNode = childNode as PropertyCopyTemplateNode;
				if (propertyNode != null && propertyNode.Name != skipNodeName)
				{
					Add(new PropertyCopyTemplateBizo(propertyNode, parentEntity));
				}
			}
		}
	}

	#endregion

	#region ValueOnlyPropertyCopyTemplateBizoCollection

	public class ValueOnlyPropertyCopyTemplateBizoCollection : CopyTemplateNodeBizoCollection<PropertyCopyTemplateBizo>
	{
		public ValueOnlyPropertyCopyTemplateBizoCollection(EntityCopyTemplateBizo parentEntity)
		{
			this.parentEntity = parentEntity;
		}

		readonly EntityCopyTemplateBizo parentEntity;

		protected override void LoadNodesCore(EntityCopyTemplateNode parentNode, CopyTemplateNode rootNode, string skipNodeName)
		{
			foreach (var propertyNode in parentNode.ValueOnlyNodes)
			{
				if (propertyNode != null && propertyNode.Name != skipNodeName)
				{
					Add(new PropertyCopyTemplateBizo(propertyNode, parentEntity));
				}
			}
		}
	}

	#endregion
}
