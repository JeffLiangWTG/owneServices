using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public static class UnlocoExtensions
	{
		public static Unloco WithCustomNameProvider(this Unloco unloco, Func<IRefUNLOCO, string> nameProvider)
		{
			if (unloco != null)
			{
				unloco.SetNameProvider(nameProvider);
			}

			return unloco;
		}
	}
}
