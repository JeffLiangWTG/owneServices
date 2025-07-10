using System;
using System.Collections.Generic;
using System.Linq;

using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionMethodApplicatorCollection : NonPersistentBusinessObjectCollection<OperationalActionMethodApplicator>
	{
		public OperationalActionMethodApplicatorCollection(OperationalActionRunner runner)
			: base(runner.Factory)
		{
			this.runner = runner;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		internal IEnumerable<OperationalActionMethodApplicator> GetApplicators(OperationalActionMethodUIMode uiMode)
		{
			foreach (OperationalActionMethodApplicator applicator in this)
			{
				var method = lookup.Where(methodApplicatorPair => methodApplicatorPair.Value == applicator).Select(methodApplicatorPair => methodApplicatorPair.Key).FirstOrDefault();
				if (uiMode == OperationalActionMethodUIMode.All ||
					uiMode == OperationalActionMethodUIMode.UIOnly && (method == null || !method.RunWithoutUI) ||
					uiMode == OperationalActionMethodUIMode.NonUIOnly && (method != null && method.RunWithoutUI))
				{
					yield return applicator;
				}
			}
		}

		public override void Load()
		{
			RemoveAllButLeaveRelationshipsIntact();
			lookup.Clear();

			AddLeadingPseudoApplicators();

			runner.Action.MethodDescriptors.SortByOrder();
			foreach (OperationalActionMethodDescriptor methodDescriptor in runner.Action.MethodDescriptors)
			{
				OperationalActionMethod method = methodDescriptor.Method;

				if (method != null && !lookup.ContainsKey(method))
				{
					OperationalActionMethodApplicator applicator = method.NewApplicator(Factory, methodDescriptor.Settings);
					applicator.Build(runner.GetSelectedPrimaryKeys());
					lookup.Add(method, applicator);
					base.Add(applicator);
				}
			}

			AddTrailingPseudoApplicators();
		}

		public OperationalActionMethodApplicator this[OperationalActionMethod method]
		{
			get
			{
				OperationalActionMethodApplicator applicator;
				lookup.TryGetValue(method, out applicator);
				return applicator;
			}
		}

		public IEnumerable<OperationalActionMethod> GetMethods()
		{
			return lookup.Keys;
		}

		void AddLeadingPseudoApplicators()
		{
			if (runner.Action.DocumentEvent != null)
			{
				Add(new EventPseudoApplicator(runner));
			}

			if (runner.Fields.Count > 0)
			{
				Add(new FieldPseudoApplicator(runner));
			}
		}

		void AddTrailingPseudoApplicators()
		{
			if (runner.Action.Context.SupportsDocuments &&
				runner.Action.DocumentPivots.Count > 0)
			{
				Add(new DocumentPseudoApplicator(runner));
			}
		}

		readonly OperationalActionRunner runner;
		readonly Dictionary<OperationalActionMethod, OperationalActionMethodApplicator> lookup = new Dictionary<OperationalActionMethod, OperationalActionMethodApplicator>();
	}
}
