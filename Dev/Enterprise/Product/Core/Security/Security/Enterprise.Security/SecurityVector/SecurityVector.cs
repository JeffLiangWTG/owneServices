using System;
using System.Collections.Generic;
using System.Diagnostics;
using Enterprise.Security.Provider;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	public interface ISecurityInfo : IEquatable<ISecurityInfo>
	{
		string Name { get; }
		ISecurityCheckpoint Checkpoint { get; }
		bool IsGrouped { get; }
		IEnumerable<ISecurityInfo> Nodes { get; }
		bool IsChildOf(ISecurityInfo info);
	}

	public class SecurityVector : IEnumerable<ISecurityInfo>
	{
		public void Initialise(SecurityCore security)
		{
			list = new List<SecurityInfo>();
			_ = security.AllLoadedCheckPoints;
			Add(new RootSecurityInfoProvider(security));
			security.HasBeenSecurityVectorInitialized = true;
		}

		public IEnumerable<ISecurityInfo> Nodes
		{
			get
			{
				int childIndex = 0;
				while (childIndex < list.Count)
				{
					SecurityInfo child = list[childIndex];
					yield return child;
					childIndex = childIndex + child.NumberOfHeirs + 1;
				}
			}
		}

		void Add(SecurityInfoProvider provider)
		{
			int currentIndex = -1;
			if (!provider.IsRoot)
			{
				currentIndex = list.Count;
				list.Add(new SecurityInfo(this, currentIndex, provider.Name, provider.Checkpoint));
			}
			foreach (SecurityInfoProvider child in provider.GetChildren())
			{
				if (child.Checkpoint != null && child.Checkpoint.Visible)
				{
					Add(child);
				}
			}
			if (currentIndex >= 0)
			{
				list[currentIndex].NumberOfHeirs = list.Count - currentIndex - 1;
			}
		}

		[DebuggerDisplay("{Name}")]
		class SecurityInfo : ISecurityInfo
		{
			public SecurityInfo(SecurityVector vector, int index, string name, ISecurityCheckpoint checkpoint)
			{
				this.vector = vector;
				this.index = index;
				Name = name;
				Checkpoint = checkpoint;
			}

			public IEnumerable<ISecurityInfo> Nodes
			{
				get
				{
					int childIndex = index + 1;
					while (childIndex <= index + NumberOfHeirs)
					{
						SecurityInfo child = vector.list[childIndex];
						yield return child;
						childIndex = childIndex + child.NumberOfHeirs + 1;
					}
				}
			}

			public bool Equals(ISecurityInfo other)
			{
				SecurityInfo info2 = other as SecurityInfo;
				return info2 != null && Object.ReferenceEquals(vector, info2.vector) && index == info2.index;
			}

			public bool IsChildOf(ISecurityInfo info)
			{
				SecurityInfo parent = info as SecurityInfo;
				return parent != null && Object.ReferenceEquals(vector, parent.vector) && index > parent.index && index <= parent.index + parent.NumberOfHeirs;
			}

			public string Name { get; private set; }
			public ISecurityCheckpoint Checkpoint { get; private set; }
			public bool IsGrouped { get; private set; }

			public int NumberOfHeirs { get; set; }
			readonly SecurityVector vector;
			readonly int index;
		}

		List<SecurityInfo> list;

		#region IEnumerable<ISecurityInfo> Members

		IEnumerator<ISecurityInfo> IEnumerable<ISecurityInfo>.GetEnumerator()
		{
			foreach (ISecurityInfo info in list)
			{
				yield return info;
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ISecurityInfo>)this).GetEnumerator();
		}

		#endregion
	}
}
