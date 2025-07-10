using System;
using System.Collections.Generic;

namespace Enterprise.DataTransfer.Native.Utils.Models
{
	public interface IGraphNode<T> : IGraphNode where T : IGraphNode
	{
		new T Self { get; }
		new T Parent { get; }
		new IEnumerable<T> Parents { get; }
		new IEnumerable<T> Children { get; }
	}

	public interface IGraphNode
	{
		IGraphNode Self { get; }
		IGraphNode Parent { get; }
		IEnumerable<IGraphNode> Parents { get; }
		IEnumerable<IGraphNode> Children { get; }
		Guid ID { get; }
	}

	public interface ITreeNode
	{
		ITreeNode Self { get; }
		ITreeNode Parent { get; }
		IEnumerable<ITreeNode> Children { get; }
	}

	public class TreeNode<T> : ITreeNode
	{
		public TreeNode(T data)
		{
			this.data = data;
		}
		readonly T data;

		public T Data
		{
			get { return data; }
		}

		public ITreeNode Self
		{
			get { return this; }
		}

		public ITreeNode Parent
		{
			get;
			set;
		}

		public IEnumerable<ITreeNode> Children
		{
			get;
			set;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer information")]
		public override string ToString()
		{
			return string.Format("Data: {0}", data);
		}
	}
}
