using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Support
{
	[System.Diagnostics.DebuggerDisplay("Count = {methods.Count}")]
	public sealed partial class OperationalActionMethodList
	{
		public OperationalActionMethodList(OperationalActionSupporter actionSupporter)
		{
			if (actionSupporter == null)
			{
				throw new ArgumentNullException(nameof(actionSupporter));
			}

			this.actionSupporter = actionSupporter;
		}

		public OperationalActionSupporter ActionSupporter
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return actionSupporter; }
		}

		public OperationalActionMethod GetOperationalActionMethod(ZGuid guid, ZGuid methodID)
		{
			return GetOperationalActionMethod(ActionMethodProviderIDs.FindByGuid(guid), methodID);
		}

		public OperationalActionMethod GetOperationalActionMethod(ActionMethodProviderID id, ZGuid methodID)
		{
			OperationalActionMethod result = null;
			IDictionary<ZGuid, OperationalActionMethod> list;

			if (id != null && methods.TryGetValue(id, out list))
			{
				if (list == null)
				{
					list = Load(id);
					methods[id] = list;
				}

				if (list != null)
				{
					list.TryGetValue(methodID, out result);
				}
			}

			return result;
		}

		public void Add(ActionMethodProviderID id)
		{
			if (id == null)
			{
				throw new ArgumentNullException(nameof(id));
			}

			if (methods.ContainsKey(id))
			{
				throw new InvalidOperationException(string.Format("The action method id '{0}' has already been added", id.Name));
			}
			else
			{
				methods.Add(id, null);
				dirty = true;
			}
		}

		#region GetEnumerables

		public IEnumerable<ActionMethodProviderID> GetAllIds()
		{
			Load();
			return methods.Keys;
		}

		public IEnumerable<OperationalActionMethod> GetMethods(ZGuid providerID)
		{
			return GetMethods(ActionMethodProviderIDs.FindByGuid(providerID));
		}

		public IEnumerable<OperationalActionMethod> GetMethods(ActionMethodProviderID id)
		{
			IDictionary<ZGuid, OperationalActionMethod> list = null;

			if (id != null && methods.TryGetValue(id, out list) && list == null)
			{
				list = Load(id);

				if (list == null)
				{
					methods.Remove(id);
				}
				else
				{
					methods[id] = list;
				}
			}

			return list == null ? Array.Empty<OperationalActionMethod>() : list.Values;
		}

		#endregion

		#region Implementation

		void Load()
		{
			if (dirty)
			{
				dirty = false;
				LoadCore();
			}
		}

		void LoadCore()
		{
			List<ActionMethodProviderID> providersToLoad = new List<ActionMethodProviderID>();

			foreach (KeyValuePair<ActionMethodProviderID, IDictionary<ZGuid, OperationalActionMethod>> pair in methods)
			{
				if (pair.Value == null)
				{
					providersToLoad.Add(pair.Key);
				}
			}

			foreach (ActionMethodProviderID providerId in providersToLoad)
			{
				IDictionary<ZGuid, OperationalActionMethod> loaded = Load(providerId);

				if (loaded == null)
				{
					methods.Remove(providerId);
				}
				else
				{
					methods[providerId] = loaded;
				}
			}
		}

		IDictionary<ZGuid, OperationalActionMethod> Load(ActionMethodProviderID id)
		{
			OperationalActionMethodProvider provider = OperationalActionMethodProvider.New(id);
			IDictionary<ZGuid, OperationalActionMethod> result = null;

			if (provider != null)
			{
				OperationalActionMethod[] providedMethods = provider.NewMethods(ActionSupporter);

				if (providedMethods != null && providedMethods.Length > 0)
				{
					Array.Sort(providedMethods, delegate(OperationalActionMethod m1, OperationalActionMethod m2)
					{ return StringComparer.OrdinalIgnoreCase.Compare(m1.Name, m2.Name); });

					result = new SortedList<ZGuid, OperationalActionMethod>();

					foreach (OperationalActionMethod method in providedMethods)
					{
						result.Add(method.MethodID, method);
					}
				}
			}

			return result;
		}

		#endregion

		bool dirty;
		readonly OperationalActionSupporter actionSupporter;
		readonly IDictionary<ActionMethodProviderID, IDictionary<ZGuid, OperationalActionMethod>> methods = new Dictionary<ActionMethodProviderID, IDictionary<ZGuid, OperationalActionMethod>>();
	}
}

#region Test
#if DEBUG

#region Display Proxy

namespace Enterprise.Services.OperationalActions.Support
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(DisplayProxy))]
	partial class OperationalActionMethodList
	{
		[System.Diagnostics.DebuggerDisplay("{Methods}", Name = "{id.Name}")]
		class ProviderDisplayProxy
		{
			public ProviderDisplayProxy(OperationalActionMethodList parent, ActionMethodProviderID id)
			{
				this.parent = parent;
				this.id = id;
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
			public OperationalActionMethod[] Methods
			{
				get
				{
					IDictionary<ZGuid, OperationalActionMethod> list;
					if (parent.methods.TryGetValue(id, out list) && list != null)
					{
						OperationalActionMethod[] result = new OperationalActionMethod[list.Count];
						list.Values.CopyTo(result, 0);
						return result;
					}
					else
					{
						return null;
					}
				}
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
			readonly ActionMethodProviderID id;
			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
			readonly OperationalActionMethodList parent;
		}

		class DisplayProxy
		{
			public DisplayProxy(OperationalActionMethodList parent)
			{
				this.parent = parent;
			}

			public OperationalActionSupporter ActionSupporter
			{
				get { return parent.ActionSupporter; }
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
			public ProviderDisplayProxy[] MethodGroups
			{
				get
				{
					ProviderDisplayProxy[] result = new ProviderDisplayProxy[parent.methods.Count];
					int i = 0;
					foreach (ActionMethodProviderID id in parent.methods.Keys)
					{
						result[i++] = new ProviderDisplayProxy(parent, id);
					}

					return result;
				}
			}

			readonly OperationalActionMethodList parent;
		}
	}
}

#endregion
#endif
#endregion
