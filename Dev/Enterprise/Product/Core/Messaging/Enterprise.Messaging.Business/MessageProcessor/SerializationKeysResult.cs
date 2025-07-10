using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public sealed class SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType resultType, ISet<string> keys)
	{
		public static SerializationKeysResult SerialProcessingInReceivedOrder => serialProcessingInReceivedOrder ??= new(SerializationKeysResultType.SerialProcessingInReceivedOrder, new HashSet<string> { SerializationKeysResult.ForceSerialProcessingKey });
		public static SerializationKeysResult UnconstrainedParallelProcessing => unconstrainedParallelProcessing ??= new(SerializationKeysResultType.UnconstrainedParallelProcessing, new HashSet<string>());

		public SerializationKeysResultType ResultType { get; } = resultType;
		public ISet<string> Keys { get; } = Argument.NotNull(keys, nameof(keys));

		public enum SerializationKeysResultType
		{
			KeysProvided,                       // Assert Keys is not empty
			UnconstrainedParallelProcessing,    // Assert Keys is empty
			SerialProcessingInReceivedOrder     // Assert Keys is null
		}

		public const string ForceSerialProcessingKey = "FORCESERIALPROCESSING";

		public override string ToString() => base.ToString() + $" (ResultType: {ResultType}, Keys: {string.Join("|", Keys)})";

		public override bool Equals(object obj)
		{
			return obj is SerializationKeysResult other
				&& ResultType.Equals(other.ResultType)
				&& (ReferenceEquals(Keys, other.Keys) || Keys.SetEquals(other.Keys));
		}

		public override int GetHashCode()
		{
			if (hashCode == -1)
			{
				hashCode =
					ResultType.GetHashCode() ^
					GetKeysHashCode();
			}
			return hashCode;
		}

		int GetKeysHashCode()
		{
			var result = 0;
			Keys.ForEach(x => result ^= x.GetHashCode());
			return result;
		}

		int hashCode = -1;

		[ThreadStatic]
		static SerializationKeysResult serialProcessingInReceivedOrder;
		[ThreadStatic]
		static SerializationKeysResult unconstrainedParallelProcessing;
	}
}
