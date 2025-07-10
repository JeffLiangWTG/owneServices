using System;

namespace Enterprise.Customs.ES.Messaging
{
	public class RandomGenerator : IRandomGenerator
	{
		public int Generate(int max)
		{
			return new Random(Guid.NewGuid().GetHashCode()).Next(max);
		}
	}
}
