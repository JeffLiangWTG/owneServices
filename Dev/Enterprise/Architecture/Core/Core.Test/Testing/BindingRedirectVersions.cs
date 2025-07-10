using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Enterprise.ZArchitecture.Core.Test
{
	class BindingRedirectVersions
	{
		static readonly Regex newVersionRegex = new Regex(@"^\d+\.\d+\.\d+\.\d+$");
		static readonly Regex oldVersionRegex = new Regex(@"^(\d+\.\d+\.\d+\.\d+)(?:\s*-\s*(\d+\.\d+\.\d+\.+\d+))?$");
		public Version OldVersionStart { get; }
		public Version OldVersionEnd { get; }
		public Version NewVersion { get; }

		BindingRedirectVersions(Version oldVersionStart, Version oldVersionEnd, Version newVersion)
		{
			NewVersion = newVersion;
			OldVersionStart = oldVersionStart;
			OldVersionEnd = oldVersionEnd;
		}

		public static BindingRedirectVersions GetVersionValues(XElement bindingRedirectElement)
		{
			var newVersionMatch = newVersionRegex.Match(bindingRedirectElement.Attribute("newVersion")?.Value ?? string.Empty);

			if (!(newVersionMatch.Success && Version.TryParse(newVersionMatch.Value, out var newVersion)))
			{
				throw new InvalidBindingRedirectElementException("Invalid newVersion attribute");
			}

			var oldVersionMatch = oldVersionRegex.Match(bindingRedirectElement.Attribute("oldVersion")?.Value ?? string.Empty);

			if (!(oldVersionMatch.Success && Version.TryParse(oldVersionMatch.Groups[1].Value, out var oldVersionStart)))
			{
				throw new InvalidBindingRedirectElementException("Invalid oldVersion attribute");
			}

			if (!(oldVersionMatch.Groups[2].Success && Version.TryParse(oldVersionMatch.Groups[2].Value, out var oldVersionEnd)))
			{
				throw new InvalidBindingRedirectElementException("Invalid oldVersion attribute");
			}

			if (oldVersionEnd < oldVersionStart)
			{
				throw new InvalidBindingRedirectElementException("Invalid oldVersion attribute. End version is less than the start version");
			}
			return new BindingRedirectVersions(oldVersionStart, oldVersionEnd, newVersion);
		}

		public bool VersionInRange(Version version) => OldVersionStart <= version && version <= OldVersionEnd;
	}

	[Serializable]
	public sealed class InvalidBindingRedirectElementException : Exception
	{
		public InvalidBindingRedirectElementException()
		{
		}

		public InvalidBindingRedirectElementException(string message)
			: base(message)
		{
		}

		public InvalidBindingRedirectElementException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		InvalidBindingRedirectElementException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
