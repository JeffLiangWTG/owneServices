using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules
{
	public sealed class CheckpointBuilderModel
	{
		readonly Dictionary<string, bool> config = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		public bool HasKey(string key)
		{
			if (!config.ContainsKey(key))
			{
				// All keys in the model are mandatory to specify to make sure I don't forget anything.
				throw new ArgumentException(FormattableString.Invariant($"All keys must be initialized in the model. Missing key: {key}"));
			}
			return config[key];
		}
		public void AddKey(string key, bool value) => config.Add(key, value);
	}

	public delegate ISecurityCheckpoint ConstructSecurityCheckpoint(string code, MultilingualString name, ISecurityCheckpoint parent, IZSecurity securityInstance);

	public sealed class SecurityCheckpointBuilder
	{
		readonly string code;
		readonly MultilingualString resString;
		readonly Func<CheckpointBuilderModel, MultilingualString> resStringProvider;
		Dictionary<string, SecurityCheckpointBuilder> children;
		readonly string filterKey;

		public SecurityCheckpointBuilder()
		{
			// Empty checkpoint for Root.
		}

		public SecurityCheckpointBuilder(string code, MultilingualString resString)
		{
			this.code = code ?? throw new ArgumentNullException(nameof(code));
			this.resString = resString ?? throw new ArgumentNullException(nameof(code));
		}

		public SecurityCheckpointBuilder(string filterKey, string code, MultilingualString resString)
		{
			this.filterKey = filterKey ?? throw new ArgumentNullException(nameof(filterKey));
			this.code = code ?? throw new ArgumentNullException(nameof(code));
			this.resString = resString ?? throw new ArgumentNullException(nameof(resString));
		}

		public SecurityCheckpointBuilder(string code, Func<CheckpointBuilderModel, MultilingualString> resStringProvider)
		{
			this.code = code ?? throw new ArgumentNullException(nameof(code));
			this.resStringProvider = resStringProvider ?? throw new ArgumentNullException(nameof(resStringProvider));
		}

		public string Code => code;
		public MultilingualString ResString => resString;
		public IEnumerable<SecurityCheckpointBuilder> Children => children?.Values ?? Enumerable.Empty<SecurityCheckpointBuilder>();

		public SecurityCheckpointBuilder AddChild(SecurityCheckpointBuilder child)
		{
			if (children == null)
			{
				children = new Dictionary<string, SecurityCheckpointBuilder>(StringComparer.OrdinalIgnoreCase);
			}
			children.Add(child.Code, child);
			return child;
		}

		public void BuildChildren(ISecurityCheckpoint parent, IZSecurity instance, CheckpointBuilderModel model, ConstructSecurityCheckpoint createCheckpoint)
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNull(instance, nameof(instance));
			Argument.NotNull(model, nameof(model));
			Argument.NotNull(createCheckpoint, nameof(createCheckpoint));

			foreach (var child in Children)
			{
				child.Build(parent, parent.Code, instance, model, createCheckpoint);
			}
		}

		ISecurityCheckpoint Build(ISecurityCheckpoint parent, string prefix, IZSecurity instance, CheckpointBuilderModel model, ConstructSecurityCheckpoint createCheckpoint)
		{
			if (filterKey == null || model.HasKey(filterKey))
			{
				var result = BuildCheckpoint(parent, prefix, instance, model, createCheckpoint);
				foreach (var child in Children)
				{
					child.Build(result, prefix, instance, model, createCheckpoint);
				}
				return result;
			}
			else
			{
				return null;
			}
		}

		ISecurityCheckpoint BuildCheckpoint(ISecurityCheckpoint parent, string prefix, IZSecurity instance, CheckpointBuilderModel model, ConstructSecurityCheckpoint createCheckpoint)
		{
			return createCheckpoint(GenCode(prefix), resString ?? resStringProvider(model), parent, instance);
		}

		string GenCode(string prefix)
		{
			return prefix != null ? prefix + code : code;
		}
	}
}
