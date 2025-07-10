using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public sealed class TasksByChannel
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public TasksByChannel(Dictionary<IVisualBoardChannel, List<ProcessTask>> source)
		{
			dictionary = source.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.ToImmutableHashSet());
		}

		readonly ImmutableDictionary<IVisualBoardChannel, ImmutableHashSet<ProcessTask>> dictionary;

		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public ImmutableHashSet<ProcessTask> this[IVisualBoardChannel channel]
		{
			get
			{
				ImmutableHashSet<ProcessTask> result;
				if (dictionary.TryGetValue(channel, out result))
				{
					return result;
				}
				else
				{
					return ImmutableHashSet<ProcessTask>.Empty;
				}
			}
		}
	}
}
