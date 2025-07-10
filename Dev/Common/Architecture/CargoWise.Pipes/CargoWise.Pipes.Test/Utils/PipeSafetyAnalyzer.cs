using System;
using System.Globalization;
using System.Linq;
using System.Text;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Pipes.Test
{
	// All strings in this file are for developer only validation.
	#region SuppressResourceStringsCheckRegion

	[CodeAlive("Used to test thread safety")]
	public static class PipeSafetyAnalyzer
	{
		public static PipeSafetyAnalysis FindSafetyLevel(this PipeEngine engine)
		{
			var builder = new StringBuilder();
			var worstLevel = engine.AllPipes.Any() ? PipeSafetyLevel.Safe : PipeSafetyLevel.None;

			foreach (var pipe in engine.AllPipes)
			{
				if (pipe.Type == PipeType.Asynchronous)
				{
					foreach (var input in pipe.Inputs)
					{
						if (!input.Output.IsSafe())
						{
							if (input is IPipeHandoverWrapper)
							{
								worstLevel = Max(PipeSafetyLevel.Unknown, worstLevel);
								builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "The safety of the handover from [{0}] to [{1}] is unverifiable.", input, pipe));
							}
							else
							{
								worstLevel = Max(PipeSafetyLevel.Unsafe, worstLevel);
								builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "[{0}] has mutable input from [{1}].", pipe, input));
							}
						}
					}
				}
				else if (pipe.Type == PipeType.Synchronous)
				{
					// Assume all handovers between Synchronous Pipes are safe.
				}
				else
				{
					worstLevel = Max(PipeSafetyLevel.Critical, worstLevel);
					builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "[{0}] has unrunnable type [{1}].", pipe, pipe.Type));
				}
			}

			return new PipeSafetyAnalysis(worstLevel, builder.Length > 0 ? builder.ToString() : null);
		}

		static PipeSafetyLevel Max(PipeSafetyLevel level, PipeSafetyLevel otherLevel)
		{
			if (level > otherLevel)
			{
				return level;
			}
			else
			{
				return otherLevel;
			}
		}

		static bool IsSafe(this Type type)
		{
			return type.IsPrimitive
				|| type.IsEnum
				|| HasThreadSafeAttribute(type)
				|| HasImmutableAttribute(type);
		}

		static bool HasThreadSafeAttribute(Type type)
		{
			return Attribute.GetCustomAttribute(type, typeof(ThreadSafeAttribute), false) != null;
		}

		static bool HasImmutableAttribute(Type type)
		{
			return Attribute.GetCustomAttribute(type, typeof(ImmutableAttribute), false) != null;
		}
	}

	public class PipeSafetyAnalysis
	{
		public PipeSafetyAnalysis(PipeSafetyLevel level, string description)
		{
			Description = description;
			Level = level;
		}
		public string Description { get; }
		public PipeSafetyLevel Level { get; }
	}

	public enum PipeSafetyLevel
	{
		None = 0,
		Safe = 1,
		Unknown = 2,
		Unsafe = 3,
		Critical = 4,
	}

	#endregion
}
