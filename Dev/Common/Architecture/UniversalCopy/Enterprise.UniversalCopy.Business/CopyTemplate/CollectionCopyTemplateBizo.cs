using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalCopy.Business
{
	public class CollectionCopyTemplateBizo : EntityCopyTemplateBizoWithCopyMethod, ICopyable
	{
		public CollectionCopyTemplateBizo(CollectionCopyTemplateNode collectionCopyTemplateNode, CopyTemplateNode rootNode, EntityCopyTemplateBizo parent)
			: base(collectionCopyTemplateNode, rootNode, parent)
		{
		}

		public new CollectionCopyTemplateNode CopyTemplateNode
		{
			get { return (CollectionCopyTemplateNode)base.CopyTemplateNode; }
		}

		#region Properties

		#region CopyMethod

		protected override ZString CopyMethodCore
		{
			get
			{
				if (!wrongCopyMethod.IsEmpty)
				{
					return wrongCopyMethod;
				}

				switch (CopyTemplateNode.CopyMethod)
				{
					case CollectionCopyMethod.All:
						return CopyMethodCodes.All;
					case CollectionCopyMethod.Filter:
						return (EntityFilter != null && EntityFilter.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter) ? CopyMethodCodes.Copy : CopyMethodCodes.Filtered;
					default:
						return ZString.Empty;
				}
			}
			set
			{
				wrongCopyMethod = ZString.Empty;

				switch (value.ToUpper())
				{
					case CopyMethodCodes.All:
						CopyTemplateNode.CopyMethod = CollectionCopyMethod.All;
						break;
					case CopyMethodCodes.Filtered:
					case CopyMethodCodes.Copy:
						CopyTemplateNode.CopyMethod = CollectionCopyMethod.Filter;
						break;
					default:
						wrongCopyMethod = value;
						CopyTemplateNode.CopyMethod = CollectionCopyMethod.None;
						break;
				}
			}
		}

		ZString wrongCopyMethod;

		protected override CodeDescriptionPairList GetCopyMethodsCore()
		{
			var copyMethods = new CodeDescriptionPairList();
			copyMethods.AddPair(CopyMethodCodes.DoNotCopy, ResString.GetMultilingualString("5c66dd52-736c-4f74-a3a6-f09a8999c4b0", "Do not copy"));
			if (EntityFilter == null || EntityFilter.FilterTypeId != EntityFilterTypeIds.MandatoryExpressionFilter)
			{
				if (!IsSplitCollection)
				{
					copyMethods.AddPair(CopyMethodCodes.All, ResString.GetMultilingualString("eda21c17-88ac-49c4-9550-34086684ebc5", "Copy all elements"));
				}
				else
				{
					copyMethods.AddPair(CopyMethodCodes.Filtered, ResString.GetMultilingualString("3ced3df0-d4c9-477b-a9f6-b934ef90bf7c", "Copy filtered elements"));
				}
			}
			else
			{
				copyMethods.AddPair(CopyMethodCodes.Copy, ResString.GetMultilingualString("94e5df2a-992b-481f-91c1-833be23b5d01", "Copy"));
			}
			return copyMethods;
		}

		public static class CopyMethodCodes
		{
			public const string DoNotCopy = "";
			public const string Copy = "CPY";
			public const string All = "ALL";
			public const string Filtered = "FLT";
		}

		#endregion

		#region SplitCollection

		public string CollectionId
		{
			get { return CopyTemplateNode.CollectionId; }
			set { CopyTemplateNode.CollectionId = value; }
		}

		public bool IsSplitCollection
		{
			get { return CopyTemplateNode.IsSplitCollection; }
			set { CopyTemplateNode.IsSplitCollection = value; }
		}

		public bool HasJustBeenSplet { get; set; }

		public bool IsUnfilteredRest { get; set; }

		public string SplitOwner
		{
			get { return CopyTemplateNode.SplitOwner; }
			set { CopyTemplateNode.SplitOwner = value; }
		}

		#endregion

		#region Order

		public ZInt Order
		{
			get => CopyTemplateNode.Order;
			set
			{
				if (CopyTemplateNode.Order != value)
				{
					HasChanges = true;
					CopyTemplateNode.Order = value;
				}
				OrderInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OrderInfo => GetZPropertyInfo(nameof(Order));

		#endregion

		#endregion

		#region Overrides

		public override EntityFilter EntityFilter
		{
			get { return CopyTemplateNode.Filter; }
			set { CopyTemplateNode.Filter = value; }
		}

		protected override string EntityTableNameCore
		{
			get { return CopyTemplateNode.ItemsTableName; }
		}

		protected override ZString KindCore
		{
			get { return Res.GetString("11210f4c-9c84-4c9f-b6a7-8b24d8dcc749", "Collection"); }
		}

		protected internal override bool NeedsChildrenForCopy()
		{
			return CopyTemplateNode.CopyMethod != CollectionCopyMethod.None;
		}

		#region Defaulting

		protected override void DefaultToDoNotCopyCore()
		{
			base.DefaultToDoNotCopyCore();

			CopyMethod = CopyMethodCodes.DoNotCopy;
		}

		protected override void DefaultToCopyCore(int level)
		{
			if (level > 0)
			{
				//collections with no properties can't be copied (like Conversations), so don't default them as such
				if (!ChildNodes.Any() && !PropertyNodes.Any())
				{ return; }

				if (EntityFilter != null && !string.IsNullOrEmpty(EntityFilter.FilterData))
				{
					CopyMethod = EntityFilter.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter ? CopyMethodCodes.Copy : CopyMethodCodes.Filtered;
				}
				else if (IsSplitCollection)
				{
					CopyMethod = CopyMethodCodes.Filtered;
				}
				else
				{
					CopyMethod = CopyMethodCodes.All;
				}

				base.DefaultToCopyCore(level);
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override CopyTemplateNodeBizoValidation GetNewValidation()
		{
			return new CollectionCopyTemplateBizoValidation(this);
		}

		#endregion
	}

	#region Validation Class

	class CollectionCopyTemplateBizoValidation : EntityCopyTemplateBizoWithCopyMethodValidation
	{
		public CollectionCopyTemplateBizoValidation(CollectionCopyTemplateBizo parent)
			: base(parent)
		{
		}

		protected new CollectionCopyTemplateBizo Parent => (CollectionCopyTemplateBizo)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOrder();
		}

		protected override bool NeedCheckCopySelection => Parent.CopyTemplateNode.CopyMethod != CollectionCopyMethod.None;

		public void ValidateOrder()
		{
			ZValidationInternals.Validate(Parent.OrderInfo, CheckOrder);
		}

		protected virtual void CheckOrder()
		{
			if (!IgnoreOrderValidation())
			{
				var thisNode = Parent.CopyTemplateNode;
				if (thisNode != null)
				{
					var siblings = Parent?.Parent?.ChildNodes.Where(i => i != null).OfType<CollectionCopyTemplateBizo>().Select(i => i.CopyTemplateNode)
						.Where(i => i.IsSplitCollection)
						.Where(i => i.ItemPropertyName == thisNode.ItemPropertyName)
						.Where(i => i.ItemParentTablePropertyName == thisNode.ItemParentTablePropertyName)
						.Where(i => i.ItemsTableName == thisNode.ItemsTableName)
						.Where(i => i.Order == thisNode.Order);
					if (siblings != null && siblings.Count() > 1)
					{
						Parent.OrderInfo.AddError(Res.GetString("4A1034DE-78FB-4148-82F8-EB3E578D414B", "There are several collection split parts with same order number."));
					}
				}
			}
		}

		bool IgnoreOrderValidation()
		{
			return Parent == null || Parent.IsUnfilteredRest || Parent.EntityFilter?.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter;
		}

		protected override void CheckCopyMethod()
		{
			base.CheckCopyMethod();

			if (Parent.CopyMethod == CollectionCopyTemplateBizo.CopyMethodCodes.Filtered &&
				Parent.FilterStripBizo != null &&
				string.IsNullOrEmpty(Parent.FilterStripBizo.Filter.LiteralTextADO))
			{
				Parent.CopyMethodInfo.AddError(
					Res.GetString("834AFD6E-1F91-45A8-8413-16210448C216", "Collection split {0} is defined as Filtered, but no filter is set up.", Parent.Name));
			}
		}
	}

	#endregion
}
