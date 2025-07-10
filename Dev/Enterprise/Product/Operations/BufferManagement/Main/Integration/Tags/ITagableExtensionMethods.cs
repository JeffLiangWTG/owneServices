using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public static class ITagableExtensionMethods
	{
		public static ITagOperationResult AddTag(this ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog = true)
		{
			if (magnitude == null)
			{
				throw new ArgumentNullException(nameof(magnitude));
			}

			return ObjectFactory.Get<ITagOperationStrategy>().AddTag(tagable, magnitude, showSecurityDialog);
		}

		public static ITagOperationResult RemoveTag(this ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog = true)
		{
			if (magnitude == null)
			{
				throw new ArgumentNullException(nameof(magnitude));
			}

			return ObjectFactory.Get<ITagOperationStrategy>().RemoveTag(tagable, magnitude, showSecurityDialog);
		}

		public static bool IsTagApplied(this ITagable tagable, ITagMagnitude magnitude)
		{
			return tagable.TagLinks.Cast<ITagLink>().Any(l => l.TGL_TGM_Magnitude == magnitude.PK);
		}
	}
}
