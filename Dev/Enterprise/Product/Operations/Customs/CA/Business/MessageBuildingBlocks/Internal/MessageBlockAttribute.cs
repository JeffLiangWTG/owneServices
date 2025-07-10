using System;

using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public abstract class MessageBlockAttribute : Attribute
	{
		protected MessageBlockAttribute(int position, int length)
		{
			if (position < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(position), "Position must be greater than 0");
			}

			Position = position;
			if (length < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0");
			}
			Length = length;
		}

		public string Serialise(IZType value, bool humanFriendly)
		{
			string result = SerialiseCore(value, humanFriendly);
			if (!humanFriendly)
			{
				if (result.Length > Length)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Data provided exceeded maximum field length");
				}
				else if (result.Length < Length)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Data returned was not padded to correct length");
				}
			}
			return result;
		}

		public string Serialise(IZType value)
		{
			return Serialise(value, false);
		}

		public IZType DeSerialise(string characterBlock)
		{
			string rawData = GetRawData(characterBlock);
			return DeSerialiseCore(rawData);
		}

		public int Offset
		{
			get { return Position - 1; }
		}

		public string GetRawData(ZString characterBlock)
		{
			return characterBlock.SubstringSafe(Offset, Length).TrimEnd();
		}

		public readonly int Length;
		public readonly int Position;

		#region Implementation

		protected abstract string SerialiseCore(IZType value, bool humanFriendly);
		protected abstract IZType DeSerialiseCore(string value);

		protected const char InvalidPaddingCharacter = '*';

		#endregion
	}
}
