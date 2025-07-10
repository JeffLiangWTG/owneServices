using System;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class StringTreeNodeCollection : AutoStringTreeNodeCollection
	{
		public StringTreeNodeCollection()
		{ }

		public virtual StringTreeNode Last()
		{
			if (Count > 0)
			{
				return this[Count - 1];
			}
			else
			{
				throw new InvalidOperationException("StringTreeNodeCollection is empty.");
			}
		}

		public override string ToString()
		{
			string[] childStrings = new string[Count];
			for (int i = 0; i < Count; i++)
			{
				childStrings[i] = this[i].ToString();
			}
			return "(" + String.Join(", ", childStrings) + ")";
		}
	}
}
