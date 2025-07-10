using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.DevTools
{
	public sealed class BizoTreeCompare
	{
		public BizoTreeCompare(TreeNode source, TreeNode target, BizoDiffColumnProvider columnProvider, Dictionary<string, List<string>> keyFields, Dictionary<Type, List<string>> ignoreFields)
		{
			Source = source;
			Target = target;
			ColumnProvider = columnProvider;
			KeyFields = keyFields;
			IgnoreFields = ignoreFields;
		}

		readonly TreeNode Source;
		readonly TreeNode Target;
		readonly BizoDiffColumnProvider ColumnProvider;
		readonly Dictionary<string, List<string>> KeyFields;
		readonly Dictionary<Type, List<string>> IgnoreFields;

		public void Compare()
		{
			ResetColor(Source);
			ResetColor(Target);
			_ = CompareCore(Source, Target);
		}

		void ResetColor(TreeNode node)
		{
			node.Text = StripTrailingMessages(node.Text);
			if (node.ForeColor != Color.Gray)
			{
				node.ForeColor = Color.Black;
			}

			foreach (TreeNode child in node.Nodes)
			{
				ResetColor(child);
			}
		}

		bool CompareCore(TreeNode source, TreeNode target)
		{
			source.Text = StripTrailingMessages(source.Text);
			target.Text = StripTrailingMessages(target.Text);

			if (source.ForeColor == Color.Gray)
			{
				return true;
			}

			if (source.ForeColor != Color.Gray && target.ForeColor == Color.Gray)
			{
				return false;
			}

			var compareResult = true;
			var resultString = string.Empty;

			if (source.Tag is BusinessObject bizo)
			{
				resultString += CompareBizo(bizo, target.Tag as BusinessObject);
				var childrenResultString = CompareBizoChildren(source, target);
				resultString = resultString.IsNullOrEmpty() ? childrenResultString : resultString;
				compareResult &= resultString.IsNullOrEmpty();
			}
			else
			{
				resultString += CompareCollection(source, target);
				compareResult &= resultString.IsNullOrEmpty();
			}

			var sourceChildrenCount = source.Nodes.Count;
			var sourceChildrenString = sourceChildrenCount > 0 ? string.Join(" ", ChildrenCount, sourceChildrenCount.ToString()) : string.Empty;
			var targetChildrenCount = target.Nodes.Count;
			var targetChildrenString = targetChildrenCount > 0 ? string.Join(" ", ChildrenCount, targetChildrenCount.ToString()) : string.Empty;
			source.ForeColor = target.ForeColor = compareResult ? Color.Green : Color.Red;
			List<string> sourceStrings = new List<string>() { source.Text, resultString, sourceChildrenString }.Where(s => !string.IsNullOrEmpty(s)).ToList();
			List<string> targetStrings = new List<string>() { target.Text, resultString, targetChildrenString }.Where(s => !string.IsNullOrEmpty(s)).ToList();
			source.Text = string.Join(Separator, sourceStrings);
			target.Text = string.Join(Separator, targetStrings);

			return compareResult;
		}

		string CompareCollection(TreeNode source, TreeNode target)
		{
			string childrenCompareResult = string.Empty;

			var sourceNodesToCompare = source.Nodes.Cast<TreeNode>().Where(v => v.ForeColor != Color.Gray).ToList();
			var targetNodesToCompare = target.Nodes.Cast<TreeNode>().Where(v => v.ForeColor != Color.Gray).ToList();

			var childrenTypeName = sourceNodesToCompare.Count > 0 ? sourceNodesToCompare.First().Tag.GetType().Name : string.Empty;
			if (KeyFields.ContainsKey(childrenTypeName))
			{
				var comparedNodes = new List<TreeNode>();
				foreach (TreeNode node in sourceNodesToCompare)
				{
					if (node.ForeColor == Color.Gray)
					{
						continue;
					}

					var targetNodes = targetNodesToCompare.Cast<TreeNode>().Where(v => !comparedNodes.Contains(v));
					foreach (var field in KeyFields[childrenTypeName])
					{
						var valueToMatch = ((INeedRow)node.Tag).Row[field];
						if (valueToMatch != null)
						{
							targetNodes = targetNodes.Where(v => valueToMatch.Equals(((INeedRow)v.Tag).Row[field]));
						}
						else
						{
							targetNodes = new List<TreeNode>();
							break;
						}
					}

					if (targetNodes.Count() != 1)
					{
						node.ForeColor = Color.Orange;
						childrenCompareResult = MismatchedChildDifferenceErrorMessage;
					}
					else
					{
						var targetNode = targetNodes.Single();
						comparedNodes.Add(targetNode);
						childrenCompareResult = childrenCompareResult.IsNullOrEmpty() && CompareCore(node, targetNode) ? string.Empty : ChildDifferenceErrorMessage;
					}
				}

				if (comparedNodes.Count != targetNodesToCompare.Count || comparedNodes.Count != sourceNodesToCompare.Count)
				{
					childrenCompareResult = ChildCountDifferenceErrorMessage;
				}
			}
			else
			{
				if (sourceNodesToCompare.Count != targetNodesToCompare.Count)
				{
					childrenCompareResult = ChildCountDifferenceErrorMessage;
					return childrenCompareResult;
				}

				for (var i = 0; i < sourceNodesToCompare.Count; i++)
				{
					childrenCompareResult = childrenCompareResult.IsNullOrEmpty() && CompareCore(sourceNodesToCompare[i], targetNodesToCompare[i]) ? string.Empty : ChildDifferenceErrorMessage;
				}
			}

			return childrenCompareResult;
		}

		string CompareBizo(BusinessObject source, BusinessObject target)
		{
			var type = source.GetType();
			var targetType = target.GetType();

			if (type != targetType
				|| (source == null && target != null)
				|| (source != null && target == null))
			{
				return TypeDifferenceErrorMessage;
			}

			if (source == null && target == null)
			{
				return string.Empty;
			}

			IgnoreFields.TryGetValue(type, out var ignoreSet);
			ignoreSet ??= new List<string>();

			var schemaColumns = ColumnProvider.GetPropertyList(source);

			foreach (var column in schemaColumns)
			{
				if (ignoreSet.Contains(column))
				{
					continue;
				}

				var sourceValue = ((INeedRow)source).Row[column];
				var targetValue = ((INeedRow)target).Row[column];

				if ((sourceValue == null && targetValue != null)
					|| (sourceValue != null && targetValue == null))
				{
					return NoValueFoundErrorMessage;
				}

				if (sourceValue != null && !sourceValue.Equals(targetValue))
				{
					return ValueDifferenceErrorMessage;
				}
			}

			return string.Empty;
		}

		string CompareBizoChildren(TreeNode source, TreeNode target)
		{
			var childrenCompareResult = string.Empty;
			var comparedNodes = new List<TreeNode>();

			var sourceNodesToCompare = source.Nodes.Cast<TreeNode>().Where(v => v.ForeColor != Color.Gray).ToList();
			var targetNodesToCompare = target.Nodes.Cast<TreeNode>().Where(v => v.ForeColor != Color.Gray).ToList();

			foreach (var node in sourceNodesToCompare)
			{
				if (node.ForeColor == Color.Gray)
				{
					continue;
				}

				var nodeName = node.Text;
				var targetNodes = targetNodesToCompare.Cast<TreeNode>().Where(v => !comparedNodes.Contains(v) && v.Text == nodeName);
				if (targetNodes.Count() != 1)
				{
					node.ForeColor = Color.Orange;
					childrenCompareResult = MismatchedChildDifferenceErrorMessage;
				}
				else
				{
					var targetNode = targetNodes.Single();
					comparedNodes.Add(targetNode);
					childrenCompareResult = CompareCore(node, targetNode) ? childrenCompareResult : ChildDifferenceErrorMessage;
				}
			}

			if (comparedNodes.Count != sourceNodesToCompare.Count || comparedNodes.Count != targetNodesToCompare.Count)
			{
				childrenCompareResult = ChildCountDifferenceErrorMessage;
			}

			return childrenCompareResult;
		}

		string StripTrailingMessages(string nodeName)
		{
			int index = nodeName.IndexOf(Separator);
			return index >= 0 ? nodeName.Substring(0, index) : nodeName;
		}

		public const string Separator = " || ";

		string ChildrenCount => Res.GetString("3850064c-7ede-4af0-b7b9-8027a963eb08", "Children count:");
		string TypeDifferenceErrorMessage => Res.GetString("9f54c46a-f24c-42ff-9f2b-3d600afaac04", "Difference in type");
		string NoValueFoundErrorMessage => Res.GetString("ddb66e1d-21fe-435f-afd8-ee1c2a6f9497", "No value found");
		string ChildDifferenceErrorMessage => Res.GetString("f9c04912-cf35-4f7d-b8b3-a2afcca2feec", "Difference in children");
		string ChildCountDifferenceErrorMessage => Res.GetString("78b1f679-4311-4106-914f-23659d8b53fb", "Difference in comparable children");
		string MismatchedChildDifferenceErrorMessage => Res.GetString("a53acb0a-8a23-43d9-9edb-e6f0fb475d4b", "Difference in mismatched children");
		string ValueDifferenceErrorMessage => Res.GetString("6cd2bf0d-bf7c-41ff-b197-232c59aa8bd4", "Difference in value");
	}
}
