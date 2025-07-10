using System;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalCopy.Business
{
	public class RelatedEntityCopyTemplateBizo : EntityCopyTemplateBizoWithCopyMethod, ICopyable
	{
		public RelatedEntityCopyTemplateBizo(RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode, CopyTemplateNode rootNode, EntityCopyTemplateBizo parent)
			: base(relatedEntityCopyTemplateNode, rootNode, parent)
		{
		}

		public new RelatedEntityCopyTemplateNode CopyTemplateNode
		{
			get { return (RelatedEntityCopyTemplateNode)base.CopyTemplateNode; }
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
					case RelatedEntityCopyMethod.Link:
						return CopyMethodCodes.Link;
					case RelatedEntityCopyMethod.Copy:
						return CopyMethodCodes.Copy;
					case RelatedEntityCopyMethod.LinkCopied:
						return CopyMethodCodes.LinkCopied;
					default:
						return CopyMethodCodes.DoNotCopy;
				}
			}
			set
			{
				wrongCopyMethod = ZString.Empty;

				switch (value.ToUpper())
				{
					case CopyMethodCodes.Link:
						CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.Link;
						break;
					case CopyMethodCodes.Copy:
						CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.Copy;
						break;
					case CopyMethodCodes.LinkCopied:
						CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.LinkCopied;
						break;
					default:
						wrongCopyMethod = value;
						CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.None;
						break;
				}
			}
		}

		ZString wrongCopyMethod;

		protected override CodeDescriptionPairList GetCopyMethodsCore()
		{
			var copyMethods = new CodeDescriptionPairList();
			copyMethods.AddPair(CopyMethodCodes.DoNotCopy, ResString.GetMultilingualString("0e8262c1-0935-465d-9141-a88517d53c2f", "Do not copy"));
			if (CopyTemplateNode.CanCopy)
			{
				copyMethods.AddPair(CopyMethodCodes.Copy, ResString.GetMultilingualString("ed520b20-25b8-49e6-9a0b-aeec68af921a", "Copy related record"));
			}
			if (CopyTemplateNode.CanLink)
			{
				copyMethods.AddPair(CopyMethodCodes.Link, ResString.GetMultilingualString("d6251819-71d8-4d6c-abb0-0806d2c2c31b", "Link to source record"));
			}
			if (CopyTemplateNode.CanLink || CopyTemplateNode.AllowCopyMethodLinkCopiedWhenLinkIsDisabled)
			{
				copyMethods.AddPair(CopyMethodCodes.LinkCopied, ResString.GetMultilingualString("1b8a1a4e-db68-4ca5-a910-2bb1d05f9c98", "Link to copied record (to source if not copied)"));
			}
			return copyMethods;
		}

		public static class CopyMethodCodes
		{
			public const string DoNotCopy = "";
			public const string Copy = "CPY";
			public const string Link = "LNK";
			public const string LinkCopied = "LCP";
		}

		#endregion

		#endregion

		#region Overrides

		public override EntityFilter EntityFilter
		{
			get { return null; }
			set { throw new NotSupportedException(); }
		}

		protected override string EntityTableNameCore
		{
			get { return CopyTemplateNode.RelatedEntityTableName; }
		}

		protected override ZString KindCore
		{
			get { return Res.GetString("2ef68d4b-e52e-4891-847a-a8993ab7b08b", "Related Record"); }
		}

		protected internal override bool NeedsChildrenForCopy()
		{
			return CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Copy;
		}

		#region Defaulting

		protected override void DefaultToDoNotCopyCore()
		{
			base.DefaultToDoNotCopyCore();
			CopyMethod = CopyMethodCodes.DoNotCopy;
		}

		protected override void DefaultToCopyCore(int level)
		{
			if (level > 0 && CopyTemplateNode.CanCopy)
			{
				CopyMethod = CopyMethodCodes.Copy;
			}
			else if (CopyTemplateNode.CanLink)
			{
				CopyMethod = CopyMethodCodes.Link;
			}

			if (CopyMethod == CopyMethodCodes.Copy)
			{
				base.DefaultToCopyCore(level);
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override CopyTemplateNodeBizoValidation GetNewValidation()
		{
			return new RelatedEntityCopyTemplateBizoValidation(this);
		}

		#endregion
	}

	#region Validation Class

	internal class RelatedEntityCopyTemplateBizoValidation : EntityCopyTemplateBizoWithCopyMethodValidation
	{
		public RelatedEntityCopyTemplateBizoValidation(RelatedEntityCopyTemplateBizo parent)
			: base(parent) { }

		protected new RelatedEntityCopyTemplateBizo Parent
		{
			get { return (RelatedEntityCopyTemplateBizo)base.Parent; }
		}

		protected override void CheckCopyMethod()
		{
			if (!Parent.CopyTemplateNode.CanCopy && Parent.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Copy)
			{
				Parent.CopyMethodInfo.AddError(Res.GetString("1fe9533b-77cc-4bdc-bd96-03a70c2fd140", "{0} cannot be copied.", Parent.Name));
			}

			if (!Parent.CopyTemplateNode.CanLink &&
				(Parent.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Link ||
					(Parent.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.LinkCopied && !Parent.CopyTemplateNode.AllowCopyMethodLinkCopiedWhenLinkIsDisabled)))
			{
				Parent.CopyMethodInfo.AddError(Res.GetString("084e1631-732d-46fd-b313-033f0d032acb", "{0} cannot be linked.", Parent.Name));
			}

			base.CheckCopyMethod();
		}

		protected override bool NeedCheckCopySelection
		{
			get
			{
				if (Parent.CopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Copy)
				{
					var entityNode = Parent.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
					return entityNode == null || entityNode.Nodes.Count != 0 || !Parent.CopyTemplateNode.CanCopyWithZeroNodes;
				}

				return false;
			}
		}
	}

	#endregion
}
